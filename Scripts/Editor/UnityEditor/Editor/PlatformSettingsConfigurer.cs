using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using Extenity.ApplicationToolbox.Editor;

namespace Extenity.UnityEditorToolbox.Editor
{

	public static class PlatformSettingsConfigurer
	{
		#region Desired Build Target

		public enum DesiredBuildTarget
		{
			None,
			Standalone,
			Standalone64,
			WebGL,

			Android,
			iOS,

			PS4,
			XboxOne,

			tvOS,

			//Facebook,
		}

		public static void ConvertToBuildTarget(DesiredBuildTarget desiredBuildTarget, out BuildTargetGroup buildTargetGroup, out BuildTarget buildTarget)
		{
			switch (desiredBuildTarget)
			{
				case DesiredBuildTarget.None:
					buildTarget = BuildTarget.NoTarget;
					buildTargetGroup = BuildTargetGroup.Unknown;
					break;
				case DesiredBuildTarget.Standalone:
					{
						buildTargetGroup = BuildTargetGroup.Standalone;
						switch (Application.platform)
						{
							case RuntimePlatform.OSXEditor:
							case RuntimePlatform.OSXPlayer:
								buildTarget = BuildTarget.StandaloneOSX;
								break;
							case RuntimePlatform.WindowsPlayer:
							case RuntimePlatform.WindowsEditor:
								buildTarget = BuildTarget.StandaloneWindows;
								break;
#if !UNITY_2019_2_OR_NEWER
							case RuntimePlatform.LinuxPlayer:
								buildTarget = BuildTarget.StandaloneLinux;
								break;
#endif
							default:
								throw new ArgumentOutOfRangeException("Application.platform");
						}
					}
					break;
				case DesiredBuildTarget.Standalone64:
					{
						buildTargetGroup = BuildTargetGroup.Standalone;
						switch (Application.platform)
						{
							case RuntimePlatform.OSXEditor:
							case RuntimePlatform.OSXPlayer:
								buildTarget = BuildTarget.StandaloneOSX;
								break;
							case RuntimePlatform.WindowsPlayer:
							case RuntimePlatform.WindowsEditor:
								buildTarget = BuildTarget.StandaloneWindows64;
								break;
							case RuntimePlatform.LinuxPlayer:
								buildTarget = BuildTarget.StandaloneLinux64;
								break;
							default:
								throw new ArgumentOutOfRangeException("Application.platform");
						}
					}
					break;
				case DesiredBuildTarget.WebGL:
					buildTargetGroup = BuildTargetGroup.WebGL;
					buildTarget = BuildTarget.WebGL;
					break;
				case DesiredBuildTarget.Android:
					buildTargetGroup = BuildTargetGroup.Android;
					buildTarget = BuildTarget.Android;
					break;
				case DesiredBuildTarget.iOS:
					buildTargetGroup = BuildTargetGroup.iOS;
					buildTarget = BuildTarget.iOS;
					break;
				case DesiredBuildTarget.PS4:
					buildTargetGroup = BuildTargetGroup.PS4;
					buildTarget = BuildTarget.PS4;
					break;
				case DesiredBuildTarget.XboxOne:
					buildTargetGroup = BuildTargetGroup.XboxOne;
					buildTarget = BuildTarget.XboxOne;
					break;
				case DesiredBuildTarget.tvOS:
					buildTargetGroup = BuildTargetGroup.tvOS;
					buildTarget = BuildTarget.tvOS;
					break;
				//case DesiredBuildTarget.Facebook:
				//	buildTargetGroup = BuildTargetGroup.Facebook;
				//	buildTarget = BuildTarget.;
				//	break;
				default:
					throw new ArgumentOutOfRangeException(nameof(desiredBuildTarget), desiredBuildTarget, null);
			}
		}

		#endregion

		#region Desired Platform Settings

		[Serializable]
		public class DesiredPlatformSettings
		{
			public bool Active = false;
			public string DesiredBuildTarget = "None";

			public DesiredBuildTarget ParsedDesiredBuildTarget
			{
				get
				{
					try
					{
						if (!string.IsNullOrEmpty(DesiredBuildTarget))
							return (PlatformSettingsConfigurer.DesiredBuildTarget)Enum.Parse(typeof(PlatformSettingsConfigurer.DesiredBuildTarget), DesiredBuildTarget);
					}
					catch
					{
						// ignored
					}
					return PlatformSettingsConfigurer.DesiredBuildTarget.None;
				}
			}

			public void Reset()
			{
				Active = false;
				DesiredBuildTarget = "None";
			}
		}

		private static DesiredPlatformSettings _Settings = new DesiredPlatformSettings();
		public static DesiredPlatformSettings Settings { get { return _Settings; } private set { _Settings = value; } }

		#endregion

		#region Load Settings From File

		private static readonly string ConfigurationFileName = "PlatformSettings.json";
		private static readonly string ConfigurationFilePath = Path.Combine(EditorApplicationTools.ProjectSettingsDirectory, ConfigurationFileName);

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

			var desiredBuildTarget = Settings.ParsedDesiredBuildTarget;
			if (desiredBuildTarget == DesiredBuildTarget.None)
				return;

			ConvertToBuildTarget(desiredBuildTarget, out var buildTargetGroup, out var buildTarget);

			if (EditorUserBuildSettings.activeBuildTarget != buildTarget)
			{
				Log.Warning($"Changing active build platform from '{EditorUserBuildSettings.activeBuildTarget}' to '{buildTarget}' as stated in '{ConfigurationFilePath}'.");

				EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, buildTarget);
			}
		}

		#endregion
	}

}
