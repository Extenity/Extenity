using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using Extenity.ApplicationToolbox;
using Extenity.BuildToolbox.Editor;
using Extenity.FileSystemToolbox;

namespace Extenity.UnityEditorToolbox.Editor
{

	public static class PlatformSettingsConfigurer
	{
		#region Configuration

		private static readonly string ConfigurationFileName = "PlatformSettings.json";
		private static readonly string ConfigurationFilePath = ApplicationTools.UnityProjectPaths.ProjectSettingsRelativePath.AppendFileToPath(ConfigurationFileName);

		#endregion

		#region Desired Platform Settings

		[Serializable]
		public class DesiredPlatformSettings
		{
			public bool Active = false;
			public string DesiredBuildTargetGroup = "Unknown";

			public BuildTargetGroup ParsedDesiredBuildTargetGroup
			{
				get
				{
					try
					{
						if (!string.IsNullOrEmpty(DesiredBuildTargetGroup))
							return (BuildTargetGroup)Enum.Parse(typeof(BuildTargetGroup), DesiredBuildTargetGroup);
					}
					catch
					{
						// ignored
					}
					return BuildTargetGroup.Unknown;
				}
			}

			public void Reset()
			{
				Active = false;
				DesiredBuildTargetGroup = "Unknown";
			}
		}

		private static DesiredPlatformSettings _Settings = new DesiredPlatformSettings();
		public static DesiredPlatformSettings Settings { get { return _Settings; } private set { _Settings = value; } }

		#endregion

		#region Load Settings From File

		public static void LoadSettingsFromFile()
		{
			try
			{
				if (File.Exists(ConfigurationFilePath))
				{
					var json = File.ReadAllText(ConfigurationFilePath);
					Settings = JsonUtility.FromJson<DesiredPlatformSettings>(json);
				}
				else
				{
					SaveSettingsToFile();
				}
			}
			catch (Exception)
			{
				Settings.Reset();
			}
		}

		public static void SaveSettingsToFile()
		{
			var json = JsonUtility.ToJson(Settings, true);
			File.WriteAllText(ConfigurationFilePath, json);
		}

		#endregion

		#region Process

		[InitializeOnLoadMethod]
		private static void CheckPlatformSettings()
		{
			LoadSettingsFromFile();

			if (!Settings.Active)
				return;

			var desiredBuildTargetGroup = Settings.ParsedDesiredBuildTargetGroup;
			if (desiredBuildTargetGroup == BuildTargetGroup.Unknown)
				return;

			if (EditorUserBuildSettings.activeBuildTarget.GetBuildTargetGroup() != desiredBuildTargetGroup)
			{
				Log.Error($"This project requires you to switch to platform '{desiredBuildTargetGroup}', rather than '{EditorUserBuildSettings.activeBuildTarget}'. See the configuration at '{ConfigurationFilePath}'.");
			}
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(PlatformSettingsConfigurer));

		#endregion
	}

}
