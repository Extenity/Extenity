using System;
using UnityEngine;
using Extenity.Logging;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Extenity.Parallel;
using Debug = UnityEngine.Debug;

namespace Extenity.ImportTunneling
{

	/// <summary>
	/// This is an exact copy of BuildTarget which provides independency from UnityEditor namespace.
	/// </summary>
	public enum AssetBundlePlatform
	{
		//StandaloneOSXUniversal = 2,
		//StandaloneOSXIntel = 4,
		//StandaloneWindows = 5,
		//WebPlayer = 6,
		//WebPlayerStreamed = 7,
		//iOS = 9,
		//PS3 = 10,
		//XBOX360 = 11,
		//Android = 13,
		//StandaloneGLESEmu = 14,
		//StandaloneLinux = 17,
		StandaloneWindows64 = 19,
		//WebGL = 20,
		//WSAPlayer = 21,
		//StandaloneLinux64 = 24,
		//StandaloneLinuxUniversal = 25,
		//WP8Player = 26,
		//StandaloneOSXIntel64 = 27,
		//BlackBerry = 28,
		//Tizen = 29,
		//PSP2 = 30,
		//PS4 = 31,
		//PSM = 32,
		//XboxOne = 33,
		//SamsungTV = 34,
		//Nintendo3DS = 35,
		//WiiU = 36,
		//tvOS = 37,
	}

	public class ImportTunneler : MonoBehaviour
	{
		#region Initialization

		protected void Start()
		{
			if (LoadConfigurationAtStart)
			{
				LoadConfigurationFile();
			}
		}

		#endregion

		#region Deinitialization

		//protected void OnDestroy()
		//{
		//}

		#endregion

		#region Update

		//protected void Update()
		//{
		//}

		#endregion

		#region Configuration

		[Serializable]
		public class ImportTunnelerConfiguration
		{
			public string UnityEditorPath = DefaultUnityEditorPath;
			public string DummyProjectPath = "";
		}

		public ImportTunnelerConfiguration Configuration;

		#endregion

		#region Configuration Loading

		public bool LoadConfigurationAtStart = false;
		public string ConfigurationFilePath = "ImportTunneler.conf";

		private void LoadConfigurationFile()
		{
			try
			{
				var fileContent = File.ReadAllText(ConfigurationFilePath);
				JsonUtility.FromJsonOverwrite(fileContent, Configuration);
			}
			catch
			{
				// ignored
			}
		}

		#endregion

		#region Dummy Project Path

		public string DummyProjectFullPath { get; private set; }

		public string DummyProjectDirectoryPrefix = "ImportTunnelerProject-";

		private void GenerateDummyProjectFullPath()
		{
			var projectDirectoryName = DummyProjectDirectoryPrefix + Guid.NewGuid();

			if (string.IsNullOrEmpty(Configuration.DummyProjectPath))
			{
				DummyProjectFullPath = Path.Combine(Path.GetTempPath(), projectDirectoryName);
			}
			else
			{
				DummyProjectFullPath = Path.Combine(Configuration.DummyProjectPath, projectDirectoryName);
			}
		}

		#endregion

		#region Find Unity Installation

		private static readonly string DefaultUnityEditorPath = "C:\\Program Files\\Unity\\Editor\\Unity.exe";

		public string EnsuredUnityEditorPath { get; private set; }

		private void FindUnityInstallationPath()
		{
			// Check if path was already found and a file still exists at path
			if (!string.IsNullOrEmpty(EnsuredUnityEditorPath))
				if (File.Exists(EnsuredUnityEditorPath))
					return;

			if (string.IsNullOrEmpty(Configuration.UnityEditorPath))
			{
				// Configuration does not specify a path. So check if an editor is installed in default path
				if (File.Exists(DefaultUnityEditorPath))
				{
					EnsuredUnityEditorPath = DefaultUnityEditorPath;
				}
				else
				{
					throw new Exception("Failed to find a Unity installation. You can specify an installation path in configuration file.");
				}
			}
			else
			{
				// Make sure a file exists at path
				if (File.Exists(Configuration.UnityEditorPath))
				{
					EnsuredUnityEditorPath = Configuration.UnityEditorPath;
				}
				else
				{
					throw new FileNotFoundException("Unity editor was not found at path '" + Configuration.UnityEditorPath + "'.");
				}
			}
		}

		#endregion

		#region Unity Project

		private IEnumerator CreateAndLaunchUnityProject(string projectPath, string executedMethod, string sourceAssetPath, string outputAssetPath, AssetBundlePlatform assetBundlePlatform)
		{
			if (string.IsNullOrEmpty(projectPath))
				throw new ArgumentNullException("projectPath");
			if (string.IsNullOrEmpty(executedMethod))
				throw new ArgumentNullException("executedMethod");

			var args = string.Format(
				//"-createProject {0} -executeMethod {1} +sourceAssetPath {2} +outputAssetPath {3} +assetBundlePlatform {4}",
				"-quit -batchmode -nographics -silent-crashes -createProject {0} -executeMethod {1} +sourceAssetPath {2} +outputAssetPath {3} +assetBundlePlatform {4}",
				projectPath,
				executedMethod,
				sourceAssetPath,
				outputAssetPath,
				assetBundlePlatform.ToString());

			var process = new Process();
			process.StartInfo.FileName = EnsuredUnityEditorPath;
			process.StartInfo.Arguments = args;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.UseShellExecute = false;
			//process.ErrorDataReceived += OnUnityEditorProcessErrorDataReceived;
			//process.OutputDataReceived += OnUnityEditorProcessOutputDataReceived;
			process.EnableRaisingEvents = true;
			process.Start();

			while (!process.HasExited)
			{
				yield return new WaitForEndOfFrame();
			}
		}

		//private void OnUnityEditorProcessOutputDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
		//{
		//	var line = dataReceivedEventArgs.Data;
		//	Debug.Log("  |  " + line);
		//}

		//private void OnUnityEditorProcessErrorDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
		//{
		//	var line = dataReceivedEventArgs.Data;
		//	Debug.Log("  Err |  " + line);
		//}

		#endregion

		#region ImportTunnelerSatellite as Text

		private static readonly string ImportTunnelerConverterFileContent =
@"using System;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace ImportTunneling
{

	public static class ImportTunnelerSatellite
	{
		#region Conversion

		public static void ConvertUsingArgs()
		{
			var sourceAssetPath = CommandLineTools.GetValue(""+sourceAssetPath"");
			var outputAssetPath = CommandLineTools.GetValue(""+outputAssetPath"");
			var assetBundlePlatformString = CommandLineTools.GetValue(""+assetBundlePlatform"");
			if (string.IsNullOrEmpty(sourceAssetPath))
				throw new ArgumentNullException(""sourceAssetPath"");
			if (string.IsNullOrEmpty(outputAssetPath))
				throw new ArgumentNullException(""outputAssetPath"");
			if (string.IsNullOrEmpty(assetBundlePlatformString))
				throw new ArgumentNullException(""assetBundlePlatform"");


			var assetBundlePlatform = EnumTools.ParseSafe<BuildTarget>(assetBundlePlatformString, true);

			Convert(sourceAssetPath, outputAssetPath, assetBundlePlatform);
		}

		public static void Convert(string sourceAssetPath, string outputAssetPath, BuildTarget assetBundlePlatform)
		{
			Debug.LogFormat(""Starting asset conversion for '{0}' output as '{1}'."", sourceAssetPath, outputAssetPath);

			var outputFileName = Path.GetFileName(outputAssetPath);
			var outputDirectoryPath = Path.GetDirectoryName(outputAssetPath);
			var importer = AssetImporter.GetAtPath(sourceAssetPath);
			importer.assetBundleName = outputFileName;

			if (!Directory.Exists(outputDirectoryPath))
				Directory.CreateDirectory(outputDirectoryPath);

			var builds = new AssetBundleBuild[]
			{
				new AssetBundleBuild()
				{
					assetBundleName = outputFileName,
					assetNames = new string[] {sourceAssetPath}
				}
			};

			BuildPipeline.BuildAssetBundles(outputDirectoryPath, builds, BuildAssetBundleOptions.UncompressedAssetBundle, assetBundlePlatform);

			Debug.Log(""Conversion completed"");
		}

		#endregion

		#region Tools - CommandLineTools

		private static class CommandLineTools
		{
			#region Initialization

			static CommandLineTools()
			{
				CommandLine = Environment.CommandLine;
				SplitCommandLine = CommandLine.Split(' ');
			}

			#endregion

			#region Data

			public static string CommandLine { get; private set; }
			public static string[] SplitCommandLine { get; private set; }

			#endregion

			#region Get

			public static string GetValue(string key)
			{
				for (int i = 0; i < SplitCommandLine.Length; i++)
				{
					if (SplitCommandLine[i] == key)
					{
						i++;

						if (i < SplitCommandLine.Length)
						{
							return SplitCommandLine[i];
						}
						else
						{
							return null;
						}
					}
				}
				return null;
			}

			#endregion
		}

		#endregion

		#region Tools - EnumTools

		private static class EnumTools
		{
			public static T ParseSafe<T>(string value, bool ignoreCase = false)
			{
				var enumType = typeof(T);

				if (!enumType.IsEnum)
					throw new ArgumentException(""Generic type must be an enumeration."", ""enumType"");

				try
				{
					var result = (T)Enum.Parse(enumType, value, ignoreCase);
					return result;
				}
				catch
				{
				}
				return default(T);
			}
		}

		#endregion
	}

}
";

		#endregion

		#region Process - Convert To Asset Bundle

		public IEnumerator ConvertToAssetBundle(CoroutineTask task, string sourceAssetPath, string outputAssetPath)
		{
			if (string.IsNullOrEmpty(sourceAssetPath))
				throw new ArgumentNullException("sourceAssetPath");
			if (!File.Exists(sourceAssetPath))
				throw new FileNotFoundException("Source asset does not exist at path '" + sourceAssetPath + "'.");

			var outputAssetFullPath = Path.GetFullPath(outputAssetPath);

			// Load configuration
			LoadConfigurationFile();

			// Find Unity installation path if necessary
			FindUnityInstallationPath();

			// Generate dummy project path
			GenerateDummyProjectFullPath();
			var dummyProjectPath = DummyProjectFullPath;
			Directory.CreateDirectory(dummyProjectPath);

			// Create dummy project Assets directory
			var dummyProjectAssetsPath = Path.Combine(dummyProjectPath, "Assets");
			if (!Directory.Exists(dummyProjectAssetsPath))
				Directory.CreateDirectory(dummyProjectAssetsPath);

			// Copy source asset into dummy project
			var sourceAssetFileName = Path.GetFileName(sourceAssetPath);
			var sourceAssetDummyProjectPath = Path.Combine(dummyProjectAssetsPath, sourceAssetFileName);
			var sourceAssetDummyProjectRelativePath = dummyProjectPath.AddDirectorySeparatorToEnd().MakeRelativePath(sourceAssetDummyProjectPath);
			File.Copy(sourceAssetPath, sourceAssetDummyProjectPath);

			// Create converter code in dummy project
			// TODO: remove hardcode
			var converterCodePath = Path.Combine(dummyProjectAssetsPath, "ImportTunnelerSatellite.cs");
			File.WriteAllText(converterCodePath, ImportTunnelerConverterFileContent);

			// Launch dummy project with conversion command
			yield return task.StartNested(CreateAndLaunchUnityProject(
				dummyProjectPath,
				"ImportTunneling.ImportTunnelerSatellite.ConvertUsingArgs", // TODO: remove hardcode
				sourceAssetDummyProjectRelativePath,
				outputAssetFullPath,
				AssetBundlePlatform.StandaloneWindows64)); // TODO: remove hardcode

			//// Delete dummy project (excluding the output)
			DirectoryTools.Delete(dummyProjectPath);
		}

		#endregion
	}

}
