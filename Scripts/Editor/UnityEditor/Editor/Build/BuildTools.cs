using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using Extenity.CryptoToolbox;
using Extenity.DataToolbox;
using UnityEditor;

namespace Extenity.BuildToolbox.Editor
{

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

		public static string GetVersionInfoFromMercurialRepository()
		{
			string output;
			int exitCode;
			try
			{
				exitCode = RunConsoleCommandAndCaptureOutput("hg", "id -i -b", out output);
			}
			catch (Exception e)
			{
				throw new Exception("Could not get version from Mercurial repository. Exception: " + e);
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
				byte[] hashed = hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(toBeHashed));

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

		public static IDisposable TemporarilyRemoveSplashIfPro()
		{
			if (RemoveSplashIfPro())
			{
				return new SplashDisposeHandler(true);
			}
			return null;
		}

		#endregion

		#region Keys

		public class KeyDisposeHandler : IDisposable
		{
			public string ResultingKeystoreName;
			public string ResultingKeystorePass;
			public string ResultingKeyaliasName;
			public string ResultingKeyaliasPass;

			internal KeyDisposeHandler(string resultingKeystoreName, string resultingKeystorePass, string resultingKeyaliasName, string resultingKeyaliasPass)
			{
				ResultingKeystoreName = resultingKeystoreName;
				ResultingKeystorePass = resultingKeystorePass;
				ResultingKeyaliasName = resultingKeyaliasName;
				ResultingKeyaliasPass = resultingKeyaliasPass;
			}

			public void Dispose()
			{
				PlayerSettings.Android.keystoreName = ResultingKeystoreName;
				PlayerSettings.Android.keystorePass = ResultingKeystorePass;
				PlayerSettings.Android.keyaliasName = ResultingKeyaliasName;
				PlayerSettings.Android.keyaliasPass = ResultingKeyaliasPass;
				AssetDatabase.SaveAssets();
			}
		}

		public static void SetKeys(string setKeystoreName, string setKeystorePass, string setKeyaliasName, string setKeyaliasPass)
		{
			PlayerSettings.Android.keystoreName = setKeystoreName;
			PlayerSettings.Android.keystorePass = setKeystorePass;
			PlayerSettings.Android.keyaliasName = setKeyaliasName;
			PlayerSettings.Android.keyaliasPass = setKeyaliasPass;
		}

		public static IDisposable TemporarilySetKeys(
			string setKeystoreName, string setKeystorePass, string setKeyaliasName, string setKeyaliasPass,
			string resultingKeystoreName, string resultingKeystorePass, string resultingKeyaliasName, string resultingKeyaliasPass)
		{
			PlayerSettings.Android.keystoreName = setKeystoreName;
			PlayerSettings.Android.keystorePass = setKeystorePass;
			PlayerSettings.Android.keyaliasName = setKeyaliasName;
			PlayerSettings.Android.keyaliasPass = setKeyaliasPass;
			return new KeyDisposeHandler(resultingKeystoreName, resultingKeystorePass, resultingKeyaliasName, resultingKeyaliasPass);
		}

		public static IDisposable TemporarilySetKeys(string setKeystoreName, string setKeystorePass, string setKeyaliasName, string setKeyaliasPass)
		{
			var resultingKeystoreName = PlayerSettings.Android.keystoreName;
			var resultingKeystorePass = PlayerSettings.Android.keystorePass;
			var resultingKeyaliasName = PlayerSettings.Android.keyaliasName;
			var resultingKeyaliasPass = PlayerSettings.Android.keyaliasPass;
			return TemporarilySetKeys(
				setKeystoreName, setKeystorePass, setKeyaliasName, setKeyaliasPass,
				resultingKeystoreName, resultingKeystorePass, resultingKeyaliasName, resultingKeyaliasPass);
		}

		#endregion

		#region Increment Mobile Version (Android/iOS/Bundle)

		public static void DecrementMobileVersion(int subtractMajor, int subtractMinor, int subtractBuild, bool saveAssets)
		{
			IncrementMobileVersion(-subtractMajor, -subtractMinor, -subtractBuild, saveAssets);
		}

		public static void IncrementMobileVersion(int addMajor, int addMinor, int addBuild, bool saveAssets)
		{
			var androidCode = PlayerSettings.Android.bundleVersionCode;

			// TODO: Check if Android and iOS codes match. If not, throw exception and ask user to correct it first.

			var code = androidCode;

			const int MajorDigits = 10000;
			const int MinorDigits = 100;

			// Get version details
			var major = code / 10000;
			var minor = (code - major * 10000) / 100;
			var build = code - major * 10000 - minor * 100;

			// Increment
			major += addMajor;
			minor += addMinor;
			build += addBuild;
			if (
				(major < 0) ||
				(minor < 0 || minor >= (MajorDigits / MinorDigits)) ||
				(build < 0 || build >= MinorDigits)
			)
			{
				throw new Exception($"Version change makes the version go out of range. New version is: {major}.{minor}.{build}");
			}

			// Calculate new code and version
			code = major * 10000 + minor * 100 + build;
			var version = major + "." + minor + "." + build;

			Log.Info($"New version: {version}  (increment by {addMajor}.{addMinor}.{addBuild})");

			// Android
			{
				PlayerSettings.Android.bundleVersionCode = code;
				PlayerSettings.bundleVersion = version;
			}

			// iOS
			{
				// TODO: Set code and version for iOS.
				//PlayerSettings.iOS.bundleVersionCode or something
				//PlayerSettings.iOS.buildNumber
			}

			if (saveAssets)
			{
				AssetDatabase.SaveAssets();
			}
		}

		#endregion
	}

}
