using System;
using UnityEngine;
using Extenity.Logging;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Extenity.ImportTunneling
{

	public class ImportTunneler : MonoBehaviour
	{
		#region Initialization

		//protected void Awake()
		//{
		//}

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

		#region Process - Convert To Asset Bundle

		public void ConvertToAssetBundle(string sourceAssetPath)
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

			// Create dummy project files
			// TODO:
			throw new NotImplementedException();

			// Launch dummy project with conversion command
			// TODO:
			throw new NotImplementedException();

			// Wait for dummy project output
			// TODO:
			throw new NotImplementedException();

			// Import asset bundle
			// TODO:
			throw new NotImplementedException();
		}

		#endregion
	}

}
