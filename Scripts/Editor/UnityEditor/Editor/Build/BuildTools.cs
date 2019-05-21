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
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace Extenity.BuildToolbox.Editor
{

	public struct AndroidKeys
	{
		public readonly string KeystoreName;
		public readonly string KeystorePass;
		public readonly string KeyaliasName;
		public readonly string KeyaliasPass;

		public static AndroidKeys Empty => new AndroidKeys(null, null, null, null);

		public AndroidKeys(string keystoreName, string keystorePass, string keyaliasName, string keyaliasPass)
		{
			KeystoreName = keystoreName;
			KeystorePass = keystorePass;
			KeyaliasName = keyaliasName;
			KeyaliasPass = keyaliasPass;
		}

		public void SetToProjectSettings(bool saveAssets)
		{
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
				PlayerSettings.Android.keyaliasPass);
		}
	}

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

		#region Pro License

		[Serializable]
		public class TemporarilyRemoveSplashIfPro
		{
			public bool PreviousState;

			public static TemporarilyRemoveSplashIfPro Create()
			{
				var instance = new TemporarilyRemoveSplashIfPro();

				instance.PreviousState = PlayerSettings.SplashScreen.show;
				if (PlayerSettings.advancedLicense)
				{
					PlayerSettings.SplashScreen.show = false;
				}

				return instance;
			}

			public void Revert()
			{
				PlayerSettings.SplashScreen.show = PreviousState;
			}
		}

		#endregion

		#region Android Keys

		[Serializable]
		public class TemporarilySetAndroidKeys
		{
			public AndroidKeys PreviousKeys;

			public static TemporarilySetAndroidKeys Create(AndroidKeys setKeys)
			{
				var instance = new TemporarilySetAndroidKeys();

				instance.PreviousKeys = AndroidKeys.GetFromProjectSettings();
				setKeys.SetToProjectSettings(false);

				return instance;
			}

			public void Revert()
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
			AddDefineSymbols(symbols, EditorUserBuildSettings.selectedBuildTargetGroup, ensureNotAddedBefore);
		}

		/// <summary>
		/// Source: https://answers.unity.com/questions/1225189/how-can-i-change-scripting-define-symbols-before-a.html
		/// </summary>
		public static void AddDefineSymbols(string[] symbols, BuildTargetGroup targetGroup, bool ensureNotAddedBefore)
		{
			if (symbols.IsNullOrEmpty())
				throw new ArgumentNullException();
			Log.Info($"Adding {symbols.Length.ToStringWithEnglishPluralPostfix("define symbol")} '{string.Join(", ", symbols)}'.");

			var definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
			var allDefines = definesString.Split(';').ToList();

			foreach (var symbol in symbols)
			{
				if (!allDefines.Contains(symbol))
				{
					allDefines.Add(symbol);
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
				if (!HasDefineSymbol(symbol, targetGroup))
				{
					throw new Exception($"Failed to complete Define Symbol Add operation for symbol(s) '{string.Join(", ", symbol)}'.");
				}
			}
		}

		public static void RemoveDefineSymbols(string[] symbols)
		{
			RemoveDefineSymbols(symbols, EditorUserBuildSettings.selectedBuildTargetGroup);
		}

		public static void RemoveDefineSymbols(string[] symbols, BuildTargetGroup targetGroup)
		{
			if (symbols.IsNullOrEmpty())
				throw new ArgumentNullException();
			Log.Info($"Removing {symbols.Length.ToStringWithEnglishPluralPostfix("define symbol")} '{string.Join(", ", symbols)}'.");

			var definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
			var allDefines = definesString.Split(';').ToList();

			foreach (var symbol in symbols)
				allDefines.Remove(symbol);

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
		public class TemporarilyAddDefineSymbols
		{
			public string[] Symbols;

			public static TemporarilyAddDefineSymbols Create(string[] symbols, bool ensureNotAddedBefore)
			{
				var instance = new TemporarilyAddDefineSymbols();

				instance.Symbols = (string[])symbols.Clone();
				AddDefineSymbols(instance.Symbols, ensureNotAddedBefore);

				return instance;
			}

			public void Revert()
			{
				RemoveDefineSymbols(Symbols);
			}
		}

		#endregion

		#region Temporarily Move Assets Outside

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
					AssetTools.ManuallyMoveFilesAndDirectoriesWithMetaAndEnsureCompleted(OriginalPaths, OutsidePaths, true);
				}
			}

			public void MoveToOriginal()
			{
				using (Log.Indent($"Moving assets back to original location from '{OutsideLocationBasePath}'..."))
				{
					AssetTools.ManuallyMoveFilesAndDirectoriesWithMetaAndEnsureCompleted(OutsidePaths, OriginalPaths, true);
				}
			}

			#endregion
		}

		[Serializable]
		public class TemporarilyMoveAssetsOutside
		{
			public MoveAssetsOutsideOperation Operation;

			public static TemporarilyMoveAssetsOutside Create(IEnumerable<string> originalPaths, string outsideLocationBasePath)
			{
				var instance = new TemporarilyMoveAssetsOutside();

				instance.Operation = new MoveAssetsOutsideOperation(originalPaths, outsideLocationBasePath);
				instance.Operation.MoveToTemp();

				return instance;
			}

			public void Revert()
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
		public class TemporarilyIncrementVersion
		{
			public bool KeepTheChange = false;
			public int AddMajor;
			public int AddMinor;
			public int AddBuild;

			public static TemporarilyIncrementVersion Create(int addMajor, int addMinor, int addBuild)
			{
				var instance = new TemporarilyIncrementVersion();

				instance.AddMajor = addMajor;
				instance.AddMinor = addMinor;
				instance.AddBuild = addBuild;
				instance.KeepTheChange = false;
				ApplicationVersion.AddToUnityVersionConfiguration(instance.AddMajor, instance.AddMinor, instance.AddBuild, false);

				return instance;
			}

			public void Revert()
			{
				if (!KeepTheChange)
				{
					ApplicationVersion.AddToUnityVersionConfiguration(-AddMajor, -AddMinor, -AddBuild, false);
				}
			}
		}

		#endregion
	}

}
