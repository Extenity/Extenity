using System;
using System.Diagnostics;
using System.Security.Cryptography;
using Extenity.CryptoToolbox;
using UnityEditor;

namespace Extenity.BuildToolbox.Editor
{

	public static class BuildTools
	{
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

		public static IDisposable RemoveSplashIfPro()
		{
			if (PlayerSettings.advancedLicense)
			{
				if (PlayerSettings.SplashScreen.show)
				{
					PlayerSettings.SplashScreen.show = false;
					return new SplashDisposeHandler(true);
				}
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

		public static IDisposable SetTemporaryKeys(
			string setKeystoreName, string setKeystorePass, string setKeyaliasName, string setKeyaliasPass,
			string resultingKeystoreName, string resultingKeystorePass, string resultingKeyaliasName, string resultingKeyaliasPass)
		{
			PlayerSettings.Android.keystoreName = setKeystoreName;
			PlayerSettings.Android.keystorePass = setKeystorePass;
			PlayerSettings.Android.keyaliasName = setKeyaliasName;
			PlayerSettings.Android.keyaliasPass = setKeyaliasPass;
			return new KeyDisposeHandler(resultingKeystoreName, resultingKeystorePass, resultingKeyaliasName, resultingKeyaliasPass);
		}

		public static IDisposable SetTemporaryKeys(string setKeystoreName, string setKeystorePass, string setKeyaliasName, string setKeyaliasPass)
		{
			var resultingKeystoreName = PlayerSettings.Android.keystoreName;
			var resultingKeystorePass = PlayerSettings.Android.keystorePass;
			var resultingKeyaliasName = PlayerSettings.Android.keyaliasName;
			var resultingKeyaliasPass = PlayerSettings.Android.keyaliasPass;
			return SetTemporaryKeys(
				setKeystoreName, setKeystorePass, setKeyaliasName, setKeyaliasPass,
				resultingKeystoreName, resultingKeystorePass, resultingKeyaliasName, resultingKeyaliasPass);
		}

		#endregion
		#region Increment Android Version

		public static void IncrementAndroidVersion(bool alsoIncrementBundleVersion, bool saveAssets)
		{
			PlayerSettings.Android.bundleVersionCode++;
			if (alsoIncrementBundleVersion)
			{
				IncrementBundleVersion(false);
			}

			if (saveAssets)
			{
				AssetDatabase.SaveAssets();
			}
		}

		public static void IncrementIOSVersion(bool alsoIncrementBundleVersion, bool saveAssets)
		{
			throw new NotImplementedException();
			//PlayerSettings.iOS.buildNumber
			//if (alsoIncrementBundleVersion)
			//{
			//	IncrementBundleVersion(false);
			//}

			//if (saveAssets)
			//{
			//	AssetDatabase.SaveAssets();
			//}
		}

		public static void IncrementBundleVersion(bool saveAssets)
		{
			var split = PlayerSettings.bundleVersion.Split('.');

			// Check if bundle version format is correct
			int dummy;
			if (split.Length != 2 ||
				!int.TryParse(split[0], out dummy) ||
				!int.TryParse(split[1], out dummy)
			)
			{
				throw new FormatException($"Unknown bundle version format '{PlayerSettings.bundleVersion}' while expecting '#.#' that is being 'MAJOR.MINOR' versions.");
			}

			// Increment the version
			split[1] = (int.Parse(split[1]) + 1).ToString();
			PlayerSettings.bundleVersion = string.Join(".", split);

			if (saveAssets)
			{
				AssetDatabase.SaveAssets();
			}
		}

		#endregion
	}

}
