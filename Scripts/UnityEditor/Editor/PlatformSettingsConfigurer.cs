using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace Extenity.EditorUtilities
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
			PSM,
			PSP2,
			XboxOne,
			WiiU,
			tvOS,
		}

		public static BuildTarget ConvertToBuildTarget(DesiredBuildTarget desiredBuildTarget)
		{
			switch (desiredBuildTarget)
			{
				case DesiredBuildTarget.None:
					return (BuildTarget)0;
				case DesiredBuildTarget.Standalone:
					{
						switch (Application.platform)
						{
							case RuntimePlatform.OSXEditor:
							case RuntimePlatform.OSXPlayer:
							case RuntimePlatform.OSXDashboardPlayer:
								return BuildTarget.StandaloneOSXIntel;
							case RuntimePlatform.WindowsPlayer:
							case RuntimePlatform.WindowsEditor:
								return BuildTarget.StandaloneWindows;
							case RuntimePlatform.LinuxPlayer:
								return BuildTarget.StandaloneLinux;
							default:
								throw new ArgumentOutOfRangeException();
						}
					}
				case DesiredBuildTarget.Standalone64:
					{
						switch (Application.platform)
						{
							case RuntimePlatform.OSXEditor:
							case RuntimePlatform.OSXPlayer:
							case RuntimePlatform.OSXDashboardPlayer:
								return BuildTarget.StandaloneOSXIntel64;
							case RuntimePlatform.WindowsPlayer:
							case RuntimePlatform.WindowsEditor:
								return BuildTarget.StandaloneWindows64;
							case RuntimePlatform.LinuxPlayer:
								return BuildTarget.StandaloneLinux64;
							default:
								throw new ArgumentOutOfRangeException();
						}
					}
				case DesiredBuildTarget.WebGL:
					return BuildTarget.WebGL;
				case DesiredBuildTarget.Android:
					return BuildTarget.Android;
				case DesiredBuildTarget.iOS:
					return BuildTarget.iOS;
				case DesiredBuildTarget.PS4:
					return BuildTarget.PS4;
				case DesiredBuildTarget.PSM:
					return BuildTarget.PSM;
				case DesiredBuildTarget.PSP2:
					return BuildTarget.PSP2;
				case DesiredBuildTarget.XboxOne:
					return BuildTarget.XboxOne;
				case DesiredBuildTarget.WiiU:
					return BuildTarget.WiiU;
				case DesiredBuildTarget.tvOS:
					return BuildTarget.tvOS;
				default:
					throw new ArgumentOutOfRangeException("desiredBuildTarget", desiredBuildTarget, null);
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

		private static string SettingsFilePath = "ProjectSettings/PlatformSettings.json";

		public static void LoadSettingsFromFile()
		{
			try
			{
				if (File.Exists(SettingsFilePath))
				{
					var json = File.ReadAllText(SettingsFilePath);
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
			File.WriteAllText(SettingsFilePath, json);
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

			var buildTarget = ConvertToBuildTarget(desiredBuildTarget);

			if (EditorUserBuildSettings.activeBuildTarget != buildTarget)
			{
				Debug.LogWarningFormat("Changing active build platform from '{0}' to '{1}' as stated in '{2}'.", EditorUserBuildSettings.activeBuildTarget, buildTarget, SettingsFilePath);

				EditorUserBuildSettings.SwitchActiveBuildTarget(buildTarget);
			}
		}

		#endregion
	}

}
