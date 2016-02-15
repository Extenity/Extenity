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
			JsonUtility.FromJsonOverwrite(ConfigurationFilePath, Configuration);
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

		private IEnumerator CreateAndLaunchUnityProject(string projectPath, string executedMethod, params string[] executedMethodParameters)
		{
			var args = string.Format(
				"-quit -batchmode -nographics -silent-crashes -createProject {0} -executeMethod {1}",
				projectPath,
				executedMethod);
			args += " " + string.Join(" ", executedMethodParameters);

			var process = new Process();
			process.StartInfo.FileName = EnsuredUnityEditorPath;
			process.StartInfo.Arguments = args;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.RedirectStandardOutput = true;
			process.EnableRaisingEvents = true;

			while (!process.HasExited)
			{
				yield return new WaitForEndOfFrame();
			}
		}

		#endregion

		#region Process - Convert To Asset Bundle

		public IEnumerator ConvertToAssetBundle(CoroutineTask task, string sourceAssetPath)
		{
			if (string.IsNullOrEmpty(sourceAssetPath))
				throw new ArgumentNullException("sourceAssetPath");
			if (!File.Exists(sourceAssetPath))
				throw new FileNotFoundException("Source asset does not exist at path '" + sourceAssetPath + "'.");

			// Load configuration
			LoadConfigurationFile();

			// Find Unity installation path if necessary
			FindUnityInstallationPath();

			// Generate dummy project path
			GenerateDummyProjectFullPath();
			var dummyProjectPath = DummyProjectFullPath;
			Directory.CreateDirectory(dummyProjectPath);
			Debug.Log("Dummy project: " + dummyProjectPath);

			// Create dummy project files
			// TODO:

			// Generate asset bundle path
			// TODO:
			var assetBundlePath = "C:\\TEMP\\ImportTunnelerTest\\Output.ab";

			// Launch dummy project with conversion command
			// TODO: remove hardcode
			yield return task.StartNested(CreateAndLaunchUnityProject(dummyProjectPath, "AssetConverter.Convert", sourceAssetPath, assetBundlePath));

			//// Wait for dummy project output
			//// TODO:
			//throw new NotImplementedException();

			//// Delete dummy project (excluding the output)
			//// TODO:
			//throw new NotImplementedException();

			//// Import asset bundle
			//// TODO:
			//throw new NotImplementedException();
		}

		#endregion
	}

}
