using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Extenity.ApplicationToolbox;
using Extenity.AssetToolbox.Editor;
using Extenity.CryptoToolbox;
using Extenity.DataToolbox;
using Extenity.FileSystemToolbox;
using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace Extenity.BuildToolbox.Editor
{

	#region Android Keys

	[Serializable]
	public struct AndroidKeys
	{
		public string KeystoreName;
		public string KeystorePass;
		public string KeyaliasName;
		public string KeyaliasPass;
		public bool UseCustomKeystore;

		public static AndroidKeys Empty => new AndroidKeys(null, null, null, null, false);

		public AndroidKeys(string keystoreName, string keystorePass, string keyaliasName, string keyaliasPass, bool useCustomKeystore)
		{
			KeystoreName = keystoreName;
			KeystorePass = keystorePass;
			KeyaliasName = keyaliasName;
			KeyaliasPass = keyaliasPass;
			UseCustomKeystore = useCustomKeystore;
		}

		public void SetToProjectSettings(bool saveAssets)
		{
			PlayerSettings.Android.useCustomKeystore = UseCustomKeystore;
			PlayerSettings.Android.keystoreName = KeystoreName;
			PlayerSettings.Android.keystorePass = KeystorePass;
			PlayerSettings.Android.keyaliasName = KeyaliasName;
			PlayerSettings.Android.keyaliasPass = KeyaliasPass;
			if (saveAssets)
			{
				AssetDatabase.SaveAssets();
			}
		}

		public static AndroidKeys GetFromProjectSettings()
		{
			return new AndroidKeys(
				PlayerSettings.Android.keystoreName,
				PlayerSettings.Android.keystorePass,
				PlayerSettings.Android.keyaliasName,
				PlayerSettings.Android.keyaliasPass,
				PlayerSettings.Android.useCustomKeystore);
		}
	}

	#endregion

	#region Move Assets Outside

	[Serializable]
	public class MoveAssetsOutsideOperation
	{
		#region Initialize

		public MoveAssetsOutsideOperation(IEnumerable<string> originalPaths, string outsideLocationBasePath)
		{
			OriginalPaths = originalPaths.Select(path => path.FixDirectorySeparatorChars('/')).ToList();
			OutsideLocationBasePath = outsideLocationBasePath;

			OutsidePaths = OriginalPaths.Select(path =>
			{
				if (!path.StartsWith("Assets/"))
				{
					throw new Exception($"Original paths are expected to start with 'Assets' directory, which is not the case for path '{path}'");
				}
				return outsideLocationBasePath.AddDirectorySeparatorToEnd('/') + path.Remove(0, "Assets/".Length);
			}).ToList();

			Debug.Assert(OriginalPaths.Count == OutsidePaths.Count);
		}

		#endregion

		#region Data

		public List<string> OriginalPaths;
		public List<string> OutsidePaths;
		public string OutsideLocationBasePath;

		#endregion

		#region Move

		public void MoveToTemp()
		{
			using (Log.Indent($"Moving assets to temporary outside location '{OutsideLocationBasePath}'..."))
			{
				AssetTools.ManuallyMoveFilesAndDirectoriesWithMetaAndEnsureCompleted(OriginalPaths, OutsidePaths, false);
			}
		}

		public void MoveToOriginal()
		{
			using (Log.Indent($"Moving assets back to original location from '{OutsideLocationBasePath}'..."))
			{
				AssetTools.ManuallyMoveFilesAndDirectoriesWithMetaAndEnsureCompleted(OutsidePaths, OriginalPaths, false);
			}
		}

		#endregion
	}

	#endregion

	#region Temporary Build Operation

	public abstract class TemporaryBuildOperation
	{
		public bool IsInitialized = false;
		public bool IsFinalized = false;
		public bool KeepTheChange = false;

		public abstract void DoApply();
		public abstract void DoRevert();

		protected static T Apply<T>(T instance) where T : TemporaryBuildOperation
		{
			if (instance.IsFinalized)
				throw new Exception($"Tried to apply a finalized build operation of type '{instance.GetType()}'.");

			instance.IsInitialized = true;
			instance.DoApply();
			return instance;
		}
	}

	public static class TemporaryBuildOperationTools
	{
		public static void Revert(this TemporaryBuildOperation instance)
		{
			if (instance == null)
				return; // Ignore.
			if (instance.IsFinalized) // Check this before checking for IsInitialized.
				throw new Exception($"Tried to revert an already finalized build operation of type '{instance.GetType()}'.");
			if (!instance.IsInitialized)
				return; // Ignore.

			if (!instance.KeepTheChange)
			{
				instance.DoRevert();
			}
		}
	}

	#endregion

	#region Define Symbols

	[Serializable]
	public struct DefineSymbolEntry
	{
		/// <summary>
		/// The index that tells where this define symbol should be located. 0 means at the beginning. -1 means at the end.
		/// </summary>
		public int Index;
		public string Symbol;

		public bool IsAtTheEnd => Index < 0;

		public DefineSymbolEntry(int index, string symbol)
		{
			Index = index;
			Symbol = symbol;
		}

		public DefineSymbolEntry(string symbol)
		{
			Index = -1;
			Symbol = symbol;
		}

		public override string ToString()
		{
			return Symbol;
		}
	}

	#endregion
	
	public static class BuildTools
	{
		#region Windows Build Cleanup

		public static void ClearWindowsBuild(string outputExecutablePath,
			bool simplifyDataFolderName, bool deleteDLLArtifacts, bool deleteCrashHandlerExecutable,
			string[] deleteFilesWithExtensions, string[] deleteFilesWithFileNamePatterns)
		{
			var outputDirectory = Path.GetDirectoryName(outputExecutablePath);
			var executableNameWithoutExtension = Path.GetFileNameWithoutExtension(outputExecutablePath);
			var executablePathWithoutExtension = Path.Combine(outputDirectory, executableNameWithoutExtension);

			// Rename data folder to just "Data"
			if (simplifyDataFolderName)
			{
				var originalDataFolderPath = executablePathWithoutExtension + "_Data";
				var newDataFolderPath = Path.Combine(outputDirectory, "Data");
				if (!Directory.Exists(originalDataFolderPath))
				{
					if (!Directory.Exists(newDataFolderPath))
					{
						throw new Exception($"Output data folder at path '{originalDataFolderPath}' does not exist.");
					}
					else
					{
						Log.Info("Skipping data folder name simplification because looks like it has already been done.");
					}
				}
				else
				{
					if (Directory.Exists(newDataFolderPath))
					{
						throw new Exception($"Output data folder at path '{newDataFolderPath}' already exists.");
					}
					else
					{
						Directory.Move(originalDataFolderPath, newDataFolderPath);
					}
				}
			}

			// Clear unwanted files
			{
				var deletedFiles = new List<FileInfo>();
				var failedFiles = new List<FileInfo>();

				// Clear DLL artifacts
				if (deleteDLLArtifacts)
				{
					DirectoryTools.ClearDLLArtifacts(outputDirectory, SearchOption.AllDirectories, ref deletedFiles, ref failedFiles);
				}
				// Clear crash handler executable
				if (deleteCrashHandlerExecutable)
				{
					DirectoryTools.DeleteFilesWithPatternInDirectory(outputDirectory, "UnityCrashHandler.exe", SearchOption.AllDirectories, ref deletedFiles, ref failedFiles);
					DirectoryTools.DeleteFilesWithPatternInDirectory(outputDirectory, "UnityCrashHandler64.exe", SearchOption.AllDirectories, ref deletedFiles, ref failedFiles);
				}
				// Clear by extensions
				if (deleteFilesWithExtensions.IsNotNullAndEmpty())
				{
					foreach (var extension in deleteFilesWithExtensions)
					{
						DirectoryTools.DeleteFilesWithExtensionInDirectory(outputDirectory, extension, SearchOption.AllDirectories, ref deletedFiles, ref failedFiles);
					}
				}
				// Clear by file name patterns
				if (deleteFilesWithFileNamePatterns.IsNotNullAndEmpty())
				{
					foreach (var filePattern in deleteFilesWithFileNamePatterns)
					{
						DirectoryTools.DeleteFilesWithPatternInDirectory(outputDirectory, filePattern, SearchOption.AllDirectories, ref deletedFiles, ref failedFiles);
					}
				}

				deletedFiles.LogList($"Cleared '{deletedFiles.Count}' files:");
				if (failedFiles.Count > 0)
				{
					throw new Exception($"Failed to delete '{failedFiles.Count}' files:\n" + failedFiles.Serialize('\n'));
				}
			}
		}

		#endregion

		#region Process

		public static int RunConsoleCommandAndCaptureOutput(string filePath, string arguments, out string output)
		{
			if (string.IsNullOrEmpty(filePath))
				throw new ArgumentNullException(nameof(filePath));

			var process = new Process();

			// Redirect the output stream of the child process.
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.FileName = filePath;
			if (!string.IsNullOrEmpty(arguments))
				process.StartInfo.Arguments = arguments;
			process.Start();

			// Do not wait for the child process to exit before
			// reading to the end of its redirected stream.
			// process.WaitForExit();
			// Read the output stream first and then wait.
			output = process.StandardOutput.ReadToEnd();
			process.WaitForExit();

			return process.ExitCode;
		}

		public static int RunCommandLine(string commandLine, out string output, bool terminateWhenDone)
		{
			if (string.IsNullOrEmpty(commandLine))
				throw new ArgumentNullException(nameof(commandLine));

			string arguments = (terminateWhenDone ? "/C " : "/K ") + commandLine;
			return RunConsoleCommandAndCaptureOutput("cmd.exe", arguments, out output);
		}

		#endregion

		#region Mercurial

		public static void EnsureMercurialRepositoryDoesNotContainAnyChanges()
		{
			string output;
			int exitCode;
			try
			{
				exitCode = RunConsoleCommandAndCaptureOutput("hg", "status", out output);
			}
			catch (Exception exception)
			{
				throw new Exception("Could not get status of Mercurial repository.", exception);
			}
			if (exitCode != 0)
			{
				throw new Exception("Could not get status of Mercurial repository. Exit code is " + exitCode + ". Output: '" + output + "'");
			}
			output = output.Trim().Trim(new[] { '\r', '\n' });

			if (!string.IsNullOrWhiteSpace(output))
			{
				throw new Exception("There are some modifications in Mercurial repository. Details:\n" + output);
			}

			// Check subrepositories
			try
			{
				exitCode = RunConsoleCommandAndCaptureOutput("hg", "onsub \"hg status\"", out output);
			}
			catch (Exception exception)
			{
				throw new Exception("Could not get status of Mercurial subrepositories.", exception);
			}
			if (exitCode != 0)
			{
				throw new Exception("Could not get status of Mercurial subrepositories. Exit code is " + exitCode + ". Output: '" + output + "'");
			}
			output = output.Trim().Trim(new[] { '\r', '\n' });

			if (!string.IsNullOrWhiteSpace(output))
			{
				throw new Exception("There are some modifications in Mercurial subrepositories. Details:\n" + output);
			}
		}

		public static string GetVersionInfoFromMercurialRepository()
		{
			string output;
			int exitCode;
			try
			{
				exitCode = RunConsoleCommandAndCaptureOutput("hg", "id -i -b", out output);
			}
			catch (Exception exception)
			{
				throw new Exception("Could not get version from Mercurial repository. Exception: " + exception);
			}

			if (exitCode != 0)
			{
				throw new Exception("Could not get version from Mercurial repository. Exit code is " + exitCode + ". Output: '" + output + "'");
			}
			return output.Trim().Trim(new[] { '\r', '\n' });
		}

		#endregion

		#region Unity FileID Calculator

		/// <summary>
		/// http://forum.unity3d.com/threads/yaml-fileid-hash-function-for-dll-scripts.252075/
		/// </summary>
		public static int CalculateFileID(Type t)
		{
			string toBeHashed = "s\0\0\0" + t.Namespace + t.Name;

			using (HashAlgorithm hash = new MD4())
			{
				byte[] hashed = hash.ComputeHash(Encoding.UTF8.GetBytes(toBeHashed));

				int result = 0;

				for (int i = 3; i >= 0; --i)
				{
					result <<= 8;
					result |= hashed[i];
				}

				return result;
			}
		}

		#endregion

		#region BuildTarget

		public static BuildTargetGroup GetBuildTargetGroup(this BuildTarget buildTarget)
		{
			return BuildPipeline.GetBuildTargetGroup(buildTarget);
		}

		#endregion

		#region BuildOptions

		public static BuildOptions SetAutoRunPlayer(this BuildOptions options, bool runAfterBuild)
		{
			if (runAfterBuild)
			{
				return options | BuildOptions.AutoRunPlayer;
			}
			else
			{
				return options & ~BuildOptions.AutoRunPlayer;
			}
		}

		#endregion

		#region Tell Unity To Build

		public static void TellUnityToBuild(string outputPath, BuildTargetGroup buildTargetGroup, BuildTarget buildTarget, BuildOptions buildOptions, bool runAfterBuild)
		{
			Log.Info("Telling Unity to start the build.");

			var buildPlayerOptions = new BuildPlayerOptions
			{
				scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(),
				locationPathName = outputPath,
				targetGroup = buildTargetGroup,
				target = buildTarget,
				options = buildOptions.SetAutoRunPlayer(runAfterBuild),
				//assetBundleManifestPath = ,
			};

			var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

			Log.Info($"Unity build took '{report?.summary.totalTime.ToStringHoursMinutesSecondsMilliseconds() ?? string.Empty}'.");

			if (report.summary.result != BuildResult.Succeeded)
			{
				throw new Exception("Build failed. See logs for more detail.");
			}
			else
			{
				Log.Info("Unity reported successful build.");
			}

			BuildReportTools.CreateBuildReport(buildPlayerOptions);
		}

		#endregion

		#region Pro License

		[Serializable]
		public class TemporarilyRemoveSplashIfPro : TemporaryBuildOperation
		{
			public bool PreviousState;

			public static TemporarilyRemoveSplashIfPro Create()
			{
				return Apply(new TemporarilyRemoveSplashIfPro());
			}

			public override void DoApply()
			{
				PreviousState = PlayerSettings.SplashScreen.show;
				if (PlayerSettings.advancedLicense)
				{
					PlayerSettings.SplashScreen.show = false;
				}
			}

			public override void DoRevert()
			{
				PlayerSettings.SplashScreen.show = PreviousState;
			}
		}

		#endregion

		#region Android Keys

		[Serializable]
		public class TemporarilySetAndroidKeys : TemporaryBuildOperation
		{
			public AndroidKeys SetKeys;
			public AndroidKeys PreviousKeys;

			public static TemporarilySetAndroidKeys Create(AndroidKeys setKeys)
			{
				return Apply(new TemporarilySetAndroidKeys
				{
					SetKeys = setKeys
				});
			}

			public override void DoApply()
			{
				PreviousKeys = AndroidKeys.GetFromProjectSettings();
				SetKeys.SetToProjectSettings(false);
			}

			public override void DoRevert()
			{
				PreviousKeys.SetToProjectSettings(false);
			}
		}

		#endregion

		#region Register Keys in EditorPrefs

		public static string GetKeyFromEditorPrefsOrPrompt(string key)
		{
			// Add project path postfix. Because we need to keep the key only for this project, where EditorPrefs data is shared between all projects on this PC.
			key = PlayerPrefsTools.GenerateKey(key, PathHashPostfix.Yes);

			var value = EditorPrefs.GetString(key, "");

			if (string.IsNullOrWhiteSpace(value))
			{
				var inputField = new UserInputField(key, false);
				EditorMessageBox.Show(new Vector2Int(300, 300), "Enter " + key, "", new[] { inputField }, "Ok", "Cancel",
					() =>
					{
						EditorPrefs.SetString(key, inputField.Value.Trim());
					}
				);
				throw new Exception($"Set the key and restart the process.");
			}

			return value;
		}

		#endregion

		#region Add/Remove Define Symbols

		public static void AddDefineSymbols(string[] symbols, bool ensureNotAddedBefore)
		{
			AddDefineSymbols(symbols.Select(entry => new DefineSymbolEntry(entry)).ToArray(), ensureNotAddedBefore);
		}

		public static void AddDefineSymbols(string[] symbols, BuildTargetGroup targetGroup, bool ensureNotAddedBefore)
		{
			AddDefineSymbols(symbols.Select(entry => new DefineSymbolEntry(entry)).ToArray(), targetGroup, ensureNotAddedBefore);
		}

		public static void AddDefineSymbols(DefineSymbolEntry[] symbols, bool ensureNotAddedBefore)
		{
			AddDefineSymbols(symbols, EditorUserBuildSettings.selectedBuildTargetGroup, ensureNotAddedBefore);
		}

		/// <summary>
		/// Source: https://answers.unity.com/questions/1225189/how-can-i-change-scripting-define-symbols-before-a.html
		/// </summary>
		public static void AddDefineSymbols(DefineSymbolEntry[] symbols, BuildTargetGroup targetGroup, bool ensureNotAddedBefore)
		{
			if (symbols.IsNullOrEmpty())
				throw new ArgumentNullException();
			Log.Info($"Adding {symbols.Length.ToStringWithEnglishPluralPostfix("define symbol")} '{string.Join(", ", symbols)}'.");

			symbols = symbols.OrderBy(entry => entry.Index).ToArray();
			var definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
			var allDefines = definesString.Split(';').ToList();

			foreach (var symbol in symbols)
			{
				if (!allDefines.Contains(symbol.Symbol))
				{
					if (symbol.Index < 0 || symbol.Index >= allDefines.Count)
					{
						allDefines.Add(symbol.Symbol);
					}
					else
					{
						allDefines.Insert(symbol.Index, symbol.Symbol);
					}
				}
				else if (ensureNotAddedBefore)
				{
					throw new Exception($"The symbol '{symbol}' was already added before.");
				}
			}

			var newDefinesString = string.Join(";", allDefines);
			if (!definesString.Equals(newDefinesString, StringComparison.Ordinal))
			{
				PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, newDefinesString);
			}

			// Ensure the symbols added.
			foreach (var symbol in symbols)
			{
				if (!HasDefineSymbol(symbol.Symbol, targetGroup))
				{
					throw new Exception($"Failed to complete Define Symbol Add operation for symbol(s) '{string.Join(", ", symbol)}'.");
				}
			}
		}

		public static DefineSymbolEntry[] RemoveDefineSymbols(string[] symbols)
		{
			return RemoveDefineSymbols(symbols, EditorUserBuildSettings.selectedBuildTargetGroup);
		}

		public static DefineSymbolEntry[] RemoveDefineSymbols(string[] symbols, BuildTargetGroup targetGroup)
		{
			if (symbols.IsNullOrEmpty())
				throw new ArgumentNullException();
			Log.Info($"Removing {symbols.Length.ToStringWithEnglishPluralPostfix("define symbol")} '{string.Join(", ", symbols)}'.");

			var definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
			var allDefines = definesString.Split(';').ToList();
			var originalDefines = allDefines.Clone();
			var removedDefines = new List<DefineSymbolEntry>();

			foreach (var symbol in symbols)
			{
				if (allDefines.Remove(symbol))
				{
					removedDefines.Add(new DefineSymbolEntry(originalDefines.IndexOf(symbol), symbol));
				}
			}

			var newDefinesString = string.Join(";", allDefines);
			if (!definesString.Equals(newDefinesString, StringComparison.Ordinal))
			{
				PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, newDefinesString);
			}

			// Ensure the symbols removed.
			foreach (var symbol in symbols)
			{
				if (HasDefineSymbol(symbol, targetGroup))
				{
					throw new Exception($"Failed to complete Define Symbol Remove operation for symbol(s) '{string.Join(", ", symbol)}'.");
				}
			}

			return removedDefines.ToArray();
		}

		public static string GetDefineSymbols()
		{
			return GetDefineSymbols(EditorUserBuildSettings.selectedBuildTargetGroup);
		}

		public static string GetDefineSymbols(BuildTargetGroup targetGroup)
		{
			return PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
		}

		public static bool HasDefineSymbol(string symbol)
		{
			return HasDefineSymbol(symbol, EditorUserBuildSettings.selectedBuildTargetGroup);
		}

		public static bool HasDefineSymbol(string symbol, BuildTargetGroup targetGroup)
		{
			if (string.IsNullOrWhiteSpace(symbol))
				throw new ArgumentNullException(nameof(symbol));

			var definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
			var allDefines = definesString.Split(';').ToList();
			var result = allDefines.Contains(symbol);

			// Warn the user if there is that symbol but with different letter cases.
			if (!result)
			{
				if (allDefines.Contains(symbol, StringComparer.OrdinalIgnoreCase))
				{
					Log.Warning($"Checking for define symbol '{symbol}' for build target group '{targetGroup}' which has the symbol but with different letter cases.");
				}
			}

			return result;
		}

		#endregion

		#region Temporarily Add Define Symbols

		[Serializable]
		public class TemporarilySetDefineSymbols : TemporaryBuildOperation
		{
			public BuildTargetGroup BuildTargetGroup;
			public string[] AddedSymbols;
			public bool EnsureNotAddedBefore;
			public string[] RemovedSymbols;
			public DefineSymbolEntry[] ActuallyRemovedSymbols;

			public static TemporarilySetDefineSymbols Create(BuildTargetGroup buildTargetGroup, string[] addedSymbols, bool ensureNotAddedBefore, string[] removedSymbols = null)
			{
				return Apply(new TemporarilySetDefineSymbols
				{
					BuildTargetGroup = buildTargetGroup,
					AddedSymbols = addedSymbols != null ? (string[])addedSymbols.Clone() : new string[0],
					EnsureNotAddedBefore = ensureNotAddedBefore,
					RemovedSymbols = removedSymbols != null ? (string[])removedSymbols.Clone() : new string[0],
					ActuallyRemovedSymbols = new DefineSymbolEntry[0],
				});
			}

			public override void DoApply()
			{
				AddDefineSymbols(AddedSymbols, BuildTargetGroup, EnsureNotAddedBefore);
				if (RemovedSymbols != null && RemovedSymbols.Length > 0)
				{
					ActuallyRemovedSymbols = RemoveDefineSymbols(RemovedSymbols, BuildTargetGroup);
				}
			}

			public override void DoRevert()
			{
				if (ActuallyRemovedSymbols != null && ActuallyRemovedSymbols.Length > 0)
				{
					AddDefineSymbols(ActuallyRemovedSymbols, BuildTargetGroup, false);
				}
				RemoveDefineSymbols(AddedSymbols, BuildTargetGroup);
			}
		}

		#endregion

		#region Temporarily Move Assets Outside

		[Serializable]
		public class TemporarilyMoveAssetsOutside : TemporaryBuildOperation
		{
			public MoveAssetsOutsideOperation Operation;

			public static TemporarilyMoveAssetsOutside Create(IEnumerable<string> originalPaths, string outsideLocationBasePath)
			{
				return Apply(new TemporarilyMoveAssetsOutside
				{
					Operation = new MoveAssetsOutsideOperation(originalPaths, outsideLocationBasePath)
				});
			}

			public override void DoApply()
			{
				Operation.MoveToTemp();
			}

			public override void DoRevert()
			{
				Operation.MoveToOriginal();
			}
		}

		public static void MoveAssetsOutside(IEnumerable<string> originalPaths, string outsideLocationBasePath)
		{
			var operation = new MoveAssetsOutsideOperation(originalPaths, outsideLocationBasePath);
			operation.MoveToTemp();
		}

		#endregion

		#region Temporarily Increment Version

		[Serializable]
		public class TemporarilyIncrementVersion : TemporaryBuildOperation
		{
			public int AddMajor;
			public int AddMinor;
			public int AddBuild;

			public static TemporarilyIncrementVersion Create(int addMajor, int addMinor, int addBuild)
			{
				return Apply(new TemporarilyIncrementVersion
				{
					AddMajor = addMajor,
					AddMinor = addMinor,
					AddBuild = addBuild,
					KeepTheChange = false,
				});
			}

			public override void DoApply()
			{
				ApplicationVersion.AddToUnityVersionConfiguration(AddMajor, AddMinor, AddBuild, false);
			}

			public override void DoRevert()
			{
				ApplicationVersion.AddToUnityVersionConfiguration(-AddMajor, -AddMinor, -AddBuild, false);
			}
		}

		#endregion

		#region Tools

		public static bool IsBatchMode => Application.isBatchMode;
		public static bool IsCompiling => EditorApplication.isCompiling;

		#endregion
	}

}
