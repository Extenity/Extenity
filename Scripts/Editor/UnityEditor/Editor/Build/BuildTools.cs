using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Extenity.ApplicationToolbox;
using Extenity.AssetToolbox.Editor;
using Extenity.CryptoToolbox;
using Extenity.DataToolbox;
using Extenity.FileSystemToolbox;
using Extenity.ProjectToolbox;
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
				// Old implementation
				// if (!path.StartsWith("Assets/"))
				// {
				// 	throw new Exception($"Original paths are expected to start with 'Assets' directory, which is not the case for path '{path}'");
				// }
				// return outsideLocationBasePath.AddDirectorySeparatorToEnd('/') + path.Remove(0, "Assets/".Length);

				return outsideLocationBasePath.AddDirectorySeparatorToEnd('/') + path;
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
			Log.Info($"Moving assets to temporary outside location '{OutsideLocationBasePath}'...");
			using (Log.IndentedScope)
			{
				// Delete Outside Location if it exists and contains only empty directories.
				if (Directory.Exists(OutsideLocationBasePath) &&
				    DirectoryTools.IsDirectoryEmptyOrOnlyContainsEmptySubdirectories(OutsideLocationBasePath))
				{
					Log.Info($"Removing empty directories at temporary location '{OutsideLocationBasePath}'.");
					DirectoryTools.DeleteWithContent(OutsideLocationBasePath);
				}

				AssetDatabaseTools.ManuallyMoveFilesAndDirectoriesWithMetaAndEnsureCompleted(OriginalPaths, OutsidePaths, false);
			}
		}

		public void MoveToOriginal()
		{
			Log.Info($"Moving assets back to original location from '{OutsideLocationBasePath}'...");
			using (Log.IndentedScope)
			{
				AssetDatabaseTools.ManuallyMoveFilesAndDirectoriesWithMetaAndEnsureCompleted(OutsidePaths, OriginalPaths, false);
			}
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(MoveAssetsOutsideOperation));

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

	#region Git Command Runner

	public class GitCommandRunner
	{
		public string ExecutablePath { get; }
		public string WorkingDirectory { get; }

		public GitCommandRunner(string workingDirectory = null)
		{
			ExecutablePath = "git";
			WorkingDirectory = workingDirectory ?? "";
		}

		public void Run(string arguments, out int exitCode, bool logOutput = true, bool logError = true)
		{
			var info = new ProcessStartInfo(ExecutablePath, arguments)
			{
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				WorkingDirectory = WorkingDirectory,
			};

			var process = new Process
			{
				StartInfo = info,
			};

			if (logOutput)
			{
				Log.Info($"Running: {ExecutablePath} {arguments}");
			}

			process.Start();
			process.WaitForExit();
			exitCode = process.ExitCode;

			var output = process.StandardOutput.ReadToEnd();
			if (!string.IsNullOrEmpty(output))
			{
				if (logOutput)
				{
					Log.Info(output);
				}
			}

			if (exitCode != 0)
			{
				var error = process.StandardError.ReadToEnd();
				if (!string.IsNullOrEmpty(error))
				{
					if (logError)
					{
						Log.Error(error);
					}
				}
			}
		}

		#region Log

		private static readonly Logger Log = new("Git");

		#endregion
	}

	#endregion

	public static class BuildTools
	{
		#region Windows Build Cleanup

		public static void ClearWindowsBuild(
			string outputExecutablePath,
			bool simplifyDataFolderName,
			bool deleteDebugSymbolsFolder, bool deleteDLLArtifacts,
			bool deleteCrashHandlerExecutable,
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
				using var _1 = New.List<FileInfo>(out var deletedFiles);
				using var _2 = New.List<FileInfo>(out var failedFiles);

				// Clear debug symbols folder
				if (deleteDebugSymbolsFolder)
				{
					Thread.Sleep(2000); // Just wait for couple of seconds to hopefully prevent "IOException: Sharing violation on path ..." error.

					DirectoryTools.DeleteWithContent(Path.Combine(outputDirectory, "_BackUpThisFolder_ButDontShipItWithYourGame"));
					DirectoryTools.DeleteWithContent(Path.Combine(outputDirectory, "_BurstDebugInformation_DoNotShip"));
				}
				// Clear DLL artifacts
				if (deleteDLLArtifacts)
				{
					DirectoryTools.ClearDLLArtifacts(outputDirectory, SearchOption.AllDirectories, deletedFiles, failedFiles);
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

		#region Git

		public static void EnsureGitRepositoryDoesNotContainAnyChanges()
		{
			string output;
			int exitCode;
			try
			{
				exitCode = RunConsoleCommandAndCaptureOutput("git", "status --porcelain", out output);
			}
			catch (Exception exception)
			{
				throw new Exception("Could not get status of Git repository.", exception);
			}
			if (exitCode != 0)
			{
				throw new Exception("Could not get status of Git repository. Exit code is " + exitCode + ". Output: '" + output + "'");
			}
			output = output.Trim().Trim(new[] { '\r', '\n' });

			if (!string.IsNullOrWhiteSpace(output))
			{
				throw new Exception("There are some modifications in Git repository. Details:\n" + output);
			}
		}

		private delegate void GitOperation(out int exitCode);

        public static void StashAllLocalGitChanges(string path = null, bool includeSubmodules = true)
        {
            var commandRunner = new GitCommandRunner(path);
            using var _ = New.List<GitOperation>(out var commands);
            
            if (includeSubmodules)
            {
                commands.Add((out int code) => commandRunner.Run("submodule foreach git add .", out code));
                commands.Add((out int code) => commandRunner.Run("submodule foreach git stash", out code));
            }

            commands.Add((out int code) => commandRunner.Run("add .", out code));
            commands.Add((out int code) => commandRunner.Run("stash", out code));

            try
            {
                foreach (var gitOperation in commands)
                {
                    gitOperation.Invoke(out var exitCode);

                    if (exitCode != 0)
                    {
                        throw new Exception("Could not stash changes of Git repository. Exit code is " + exitCode);
                    }
                }
            }
            catch (Exception exception)
            {
                throw new Exception("Could not stash changes of Git repository.", exception);
            }
        }

        public static void ApplyLastGitStash(string repoPath = null, bool includeSubmodules = true)
        {
            var commandRunner = new GitCommandRunner(repoPath);
            using var _ = New.List<GitOperation>(out var commands);

            if (includeSubmodules)
            {
                commands.Add((out int code) => commandRunner.Run($"submodule foreach git stash pop 0", out code));
            }
            
            commands.Add((out int code) => commandRunner.Run("stash pop 0", out code));

            try
            {
                foreach (var gitOperation in commands)
                {
                    gitOperation.Invoke(out var exitCode);

                    if (exitCode != 0)
                    {
                        throw new Exception("Could not apply stash of Git repository. Exit code is " + exitCode);
                    }
                }
            }
            catch (Exception exception)
            {
                throw new Exception("Could apply stash of Git repository.", exception);
            }
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

		#region Beyond Compare / File Comparison

		public static void LaunchBeyondCompareFileComparison(string leftFilePath, string rightFilePath)
		{
			try
			{
				Process.Start(@"C:\Program Files\Beyond Compare 4\BComp.exe",
				              $@"""{leftFilePath}"" ""{rightFilePath}""");
			}
			catch (Exception exception)
			{
				Log.Error(new Exception("Failed to launch file comparer.", exception));
			}
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

		#region Check If Platform Available

		public static bool IsPlatformAvailable(BuildTarget buildTarget)
		{
			var moduleManager = Type.GetType("UnityEditor.Modules.ModuleManager,UnityEditor.dll");
			Debug.Assert(moduleManager != null);
			var isPlatformSupportLoaded = moduleManager.GetMethod("IsPlatformSupportLoaded", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
			Debug.Assert(isPlatformSupportLoaded != null);
			var getTargetStringFromBuildTarget = moduleManager.GetMethod("GetTargetStringFromBuildTarget", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
			Debug.Assert(getTargetStringFromBuildTarget != null);

			var targetString = (string)getTargetStringFromBuildTarget.Invoke(null, new object[] { buildTarget });
			return (bool)isPlatformSupportLoaded.Invoke(null, new object[] { targetString });
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

		public static string[] GetUnityBuildSettingsScenes()
		{
			return EditorBuildSettings.scenes
			                          .Where(scene => scene.enabled)
			                          .Select(scene => scene.path)
			                          .ToArray();
		}

		public static void TellUnityToBuild(string outputPath, BuildTargetGroup buildTargetGroup, BuildTarget buildTarget, BuildOptions buildOptions, string assetBundleManifestPath, bool runAfterBuild)
		{
			var scenes = GetUnityBuildSettingsScenes();
			TellUnityToBuild(scenes, outputPath, buildTargetGroup, buildTarget, buildOptions, assetBundleManifestPath, runAfterBuild);
		}

		public static void TellUnityToBuild(string[] scenes, string outputPath, BuildTargetGroup buildTargetGroup, BuildTarget buildTarget, BuildOptions buildOptions, string assetBundleManifestPath, bool runAfterBuild)
		{
			Log.Info($"Telling Unity to start the build. More info:\n" +
			         $"\tScenes: {string.Join("\n\t\t", scenes)}\n" +
			         $"\tOutput path: {outputPath}\n" +
			         $"\tTarget group: {buildTargetGroup}, Target: {buildTarget}\n" +
			         $"\tBuild options: {buildOptions}\n" +
			         $"\tRun after build: {runAfterBuild}");

			var buildPlayerOptions = new BuildPlayerOptions
			{
				scenes = scenes,
				locationPathName = outputPath,
				targetGroup = buildTargetGroup,
				target = buildTarget,
				options = buildOptions.SetAutoRunPlayer(runAfterBuild),
				assetBundleManifestPath = assetBundleManifestPath,
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
					AddedSymbols = addedSymbols != null ? (string[])addedSymbols.Clone() : Array.Empty<string>(),
					EnsureNotAddedBefore = ensureNotAddedBefore,
					RemovedSymbols = removedSymbols != null ? (string[])removedSymbols.Clone() : Array.Empty<string>(),
					ActuallyRemovedSymbols = Array.Empty<DefineSymbolEntry>(),
				});
			}

			public override void DoApply()
			{
				const bool saveAssets = false;
				PlayerSettingsTools.AddDefineSymbols(AddedSymbols, BuildTargetGroup, EnsureNotAddedBefore, saveAssets);
				if (RemovedSymbols != null && RemovedSymbols.Length > 0)
				{
					ActuallyRemovedSymbols = PlayerSettingsTools.RemoveDefineSymbols(RemovedSymbols, BuildTargetGroup, saveAssets);
				}
			}

			public override void DoRevert()
			{
				const bool saveAssets = false;
				if (ActuallyRemovedSymbols != null && ActuallyRemovedSymbols.Length > 0)
				{
					PlayerSettingsTools.AddDefineSymbols(ActuallyRemovedSymbols, BuildTargetGroup, false, saveAssets);
				}
				PlayerSettingsTools.RemoveDefineSymbols(AddedSymbols, BuildTargetGroup, saveAssets);
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
#if !BuildlessVersioning
			public int AddBuild;
#endif

#if !BuildlessVersioning
			public static TemporarilyIncrementVersion Create(int addMajor, int addMinor, int addBuild)
#else
			public static TemporarilyIncrementVersion Create(int addMajor, int addMinor)
#endif
			{
				return Apply(new TemporarilyIncrementVersion
				{
					AddMajor = addMajor,
					AddMinor = addMinor,
#if !BuildlessVersioning
					AddBuild = addBuild,
#endif
					KeepTheChange = false,
				});
			}

			public override void DoApply()
			{
#if !BuildlessVersioning
				ApplicationVersion.AddToUnityVersionConfiguration(AddMajor, AddMinor, AddBuild, false);
#else
				ApplicationVersion.AddToUnityVersionConfiguration(AddMajor, AddMinor, false);
#endif
			}

			public override void DoRevert()
			{
#if !BuildlessVersioning
				ApplicationVersion.AddToUnityVersionConfiguration(-AddMajor, -AddMinor, -AddBuild, false);
#else
				ApplicationVersion.AddToUnityVersionConfiguration(-AddMajor, -AddMinor, false);
#endif
			}
		}

		#endregion

		#region Tools

		public static bool IsBatchMode => Application.isBatchMode;
		public static bool IsCompiling => EditorApplication.isCompiling;

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(BuildTools));

		#endregion
	}

}
