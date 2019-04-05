using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Extenity.ApplicationToolbox;
using Extenity.CryptoToolbox;
using Extenity.DataToolbox;
using Extenity.FileSystemToolbox;
using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using UnityEngine;

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

		#region Pro License

		public class SplashDisposeHandler : IDisposable
		{
			private bool Result;

			internal SplashDisposeHandler(bool result)
			{
				Result = result;
			}

			public void Dispose()
			{
				PlayerSettings.SplashScreen.show = Result;
				AssetDatabase.SaveAssets();
			}
		}

		public static bool RemoveSplashIfPro()
		{
			if (PlayerSettings.advancedLicense)
			{
				if (PlayerSettings.SplashScreen.show)
				{
					PlayerSettings.SplashScreen.show = false;
					return true;
				}
			}
			return false;
		}

		public static SplashDisposeHandler TemporarilyRemoveSplashIfPro()
		{
			if (RemoveSplashIfPro())
			{
				return new SplashDisposeHandler(true);
			}
			return null;
		}

		#endregion

		#region Android Keys

		public class AndroidKeyDisposeHandler : IDisposable
		{
			public readonly AndroidKeys ResultingKeys;

			internal AndroidKeyDisposeHandler(AndroidKeys resultingKeys)
			{
				ResultingKeys = resultingKeys;
			}

			public void Dispose()
			{
				ResultingKeys.SetToProjectSettings(true);
			}
		}

		public static AndroidKeyDisposeHandler TemporarilySetAndroidKeys(AndroidKeys setKeys, AndroidKeys resultingKeys)
		{
			setKeys.SetToProjectSettings(true);
			return new AndroidKeyDisposeHandler(resultingKeys);
		}

		public static AndroidKeyDisposeHandler TemporarilySetAndroidKeys(AndroidKeys setKeys)
		{
			var resultingKeys = AndroidKeys.GetFromProjectSettings();
			return TemporarilySetAndroidKeys(setKeys, resultingKeys);
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

		public static void AddDefineSymbols(string[] symbols)
		{
			AddDefineSymbols(symbols, EditorUserBuildSettings.selectedBuildTargetGroup);
		}

		/// <summary>
		/// Source: https://answers.unity.com/questions/1225189/how-can-i-change-scripting-define-symbols-before-a.html
		/// </summary>
		public static void AddDefineSymbols(string[] symbols, BuildTargetGroup targetGroup)
		{
			var definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
			var allDefines = definesString.Split(';').ToList();

			foreach (var symbol in symbols)
				if (!allDefines.Contains(symbol))
					allDefines.Add(symbol);

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

		public class TemporarilyAddDefineSymbolsHandler : IDisposable
		{
			public readonly string[] Symbols;

			internal TemporarilyAddDefineSymbolsHandler(string[] symbols, bool ensureNotAddedBefore)
			{
				Symbols = (string[])symbols.Clone();

				Log.Info($"Adding define symbols '{string.Join(", ", Symbols)}'.");

				if (ensureNotAddedBefore)
				{
					foreach (var symbol in Symbols)
					{
						if (HasDefineSymbol(symbol))
						{
							throw new Exception($"The symbol '{symbol}' was already added before.");
						}
					}
				}
				AddDefineSymbols(Symbols);
			}

			public void Dispose()
			{
				Log.Info($"Removing define symbols '{string.Join(", ", Symbols)}'.");
				RemoveDefineSymbols(Symbols);
			}
		}

		public static TemporarilyAddDefineSymbolsHandler TemporarilyAddDefineSymbols(string[] symbols, bool ensureNotAddedBefore)
		{
			return new TemporarilyAddDefineSymbolsHandler(symbols, ensureNotAddedBefore);
		}

		#endregion

		#region Temporarily Move Directories

		public class TemporarilyMoveHandler : IDisposable
		{
			public readonly List<string> OriginalPaths;
			public readonly List<string> TempPaths;
			public readonly string TempLocationBasePath;

			internal TemporarilyMoveHandler(IEnumerable<string> originalPaths, string tempLocationBasePath)
			{
				OriginalPaths = originalPaths.Select(path => path.FixDirectorySeparatorChars('/')).ToList();
				TempLocationBasePath = tempLocationBasePath;

				TempPaths = OriginalPaths.Select(path =>
				{
					if (!path.StartsWith("Assets/"))
					{
						throw new Exception($"Original paths are expected to start with 'Assets' directory, which is not the case for path '{path}'");
					}
					return tempLocationBasePath.AddDirectorySeparatorToEnd('/') + path.Remove(0, "Assets/".Length);
				}).ToList();

				MoveToTemp();
			}

			public void Dispose()
			{
				MoveToOriginal();
			}

			#region Move

			private void MoveToTemp()
			{
				using (Log.Indent($"Moving assets to temporary location '{TempLocationBasePath}'..."))
				{
					// Initial idea was to move scripts in a safe environment with no ongoing compilations. But that's a fairy tale.
					//EditorApplicationTools.EnsureNotCompiling();
					MakeSureNothingExists(TempPaths);
					for (var i = 0; i < OriginalPaths.Count; i++)
					{
						MoveTo(OriginalPaths[i], TempPaths[i]);
					}
					MakeSureNothingExists(OriginalPaths); // Before refreshing AssetDatabase.
					AssetDatabase.Refresh();
					MakeSureNothingExists(OriginalPaths); // After refreshing AssetDatabase.
					// Initial idea was to move scripts in a safe environment with no ongoing compilations. But that's a fairy tale.
					//EditorApplicationTools.EnsureNotCompiling();
				}
			}

			private void MoveToOriginal()
			{
				using (Log.Indent($"Moving assets back to original location from '{TempLocationBasePath}'..."))
				{
					// Initial idea was to move scripts in a safe environment with no ongoing compilations. But that's a fairy tale.
					//EditorApplicationTools.EnsureNotCompiling();
					MakeSureNothingExists(OriginalPaths);
					for (var i = 0; i < OriginalPaths.Count; i++)
					{
						MoveTo(TempPaths[i], OriginalPaths[i]);
					}
					MakeSureNothingExists(TempPaths); // Before refreshing AssetDatabase.
					AssetDatabase.Refresh();
					MakeSureNothingExists(TempPaths); // After refreshing AssetDatabase.
					// Initial idea was to move scripts in a safe environment with no ongoing compilations. But that's a fairy tale.
					//EditorApplicationTools.EnsureNotCompiling();
				}
			}

			private void MoveTo(string source, string destination)
			{
				if (string.IsNullOrEmpty(source))
					throw new ArgumentNullException(nameof(source));
				if (string.IsNullOrEmpty(destination))
					throw new ArgumentNullException(nameof(destination));

				var assetName = Path.GetFileName(source.RemoveEndingDirectorySeparatorChar());
				var sourceMeta = source.RemoveEndingDirectorySeparatorChar() + ".meta";
				var destinationMeta = destination.RemoveEndingDirectorySeparatorChar() + ".meta";

				if (Directory.Exists(source))
				{
					Log.Info($"{assetName} (Directory)\n\tFROM: {source}\n\tTO: {destination}");
					DirectoryTools.CreateFromFilePath(destination.RemoveEndingDirectorySeparatorChar());
					Directory.Move(source, destination);
					File.Move(sourceMeta, destinationMeta);
				}
				else if (File.Exists(source))
				{
					Log.Info($"{assetName} (File)\n\tFROM: {source}\n\tTO: {destination}");
					DirectoryTools.CreateFromFilePath(destination.RemoveEndingDirectorySeparatorChar());
					File.Move(source, destination);
					File.Move(sourceMeta, destinationMeta);
				}
				else
				{
					Log.Info($"{assetName} (Not Found)\n\tFROM: {source}\n\tTO: {destination}");
				}

				//AssetDatabase.MoveAsset(source, destination); Won't work since we are moving outside of Assets folder.
			}

			#endregion

			#region Consistency

			private void MakeSureNothingExists(IEnumerable<string> paths)
			{
				foreach (var path in paths)
				{
					if (Directory.Exists(path) || File.Exists(path))
					{
						throw new Exception($"The asset is not expected to exist at path '{path}'.");
					}
					var metaPath = path.RemoveEndingDirectorySeparatorChar() + ".meta";
					if (Directory.Exists(metaPath) || File.Exists(metaPath))
					{
						throw new Exception($"The asset is not expected to exist at path '{metaPath}'.");
					}
				}
			}

			#endregion
		}

		public static TemporarilyMoveHandler TemporarilyMoveFilesAndDirectories(IEnumerable<string> originalPaths, string tempLocationBasePath)
		{
			return new TemporarilyMoveHandler(originalPaths, tempLocationBasePath);
		}

		#endregion

		#region Temporarily Increment Version

		public class TemporarilyIncrementVersionHandler : IDisposable
		{
			public bool KeepTheChange = false;
			public readonly int AddMajor;
			public readonly int AddMinor;
			public readonly int AddBuild;
			public readonly bool SaveAssets;

			internal TemporarilyIncrementVersionHandler(int addMajor, int addMinor, int addBuild, bool saveAssets)
			{
				AddMajor = addMajor;
				AddMinor = addMinor;
				AddBuild = addBuild;
				SaveAssets = saveAssets;
				ApplicationVersion.AddToUnityVersionConfiguration(AddMajor, AddMinor, AddBuild, SaveAssets);
			}

			public void Dispose()
			{
				// Revert back to previous version
				if (!KeepTheChange)
				{
					ApplicationVersion.AddToUnityVersionConfiguration(-AddMajor, -AddMinor, -AddBuild, SaveAssets);
				}
			}
		}

		public static TemporarilyIncrementVersionHandler TemporarilyIncrementVersion(int addMajor, int addMinor, int addBuild, bool saveAssets)
		{
			return new TemporarilyIncrementVersionHandler(addMajor, addMinor, addBuild, saveAssets);
		}

		#endregion
	}

}
