using System.IO;
using Extenity.UnityEditorToolbox.Editor;
using UnityEngine;

namespace Extenity.SubsystemManagementToolbox
{

	public class SubsystemSettings : ScriptableObject
	{
		#region Configuration

		private const string Path = SubsystemConstants.ConfigurationPath;
		private const string CurrentVersion = SubsystemConstants.Version;

		#endregion

		#region Singleton

		private static SubsystemSettings _Instance;

		public static SubsystemSettings Instance
		{
			get
			{
				if (_Instance == null)
				{
					_Instance = LoadOrCreate();
				}
				return _Instance;
			}
		}

		#endregion

		#region Version

		public string Version = CurrentVersion;

		#endregion

		#region Save / Load

		private static SubsystemSettings CreateNewSettingsFile()
		{
			var settings = CreateInstance<SubsystemSettings>();
			Save(settings);
			return settings;
		}

		private static void Save(SubsystemSettings settings)
		{
			EditorUtilityTools.SaveUnityAssetFile(Path, settings);
		}

		private static SubsystemSettings LoadOrCreate()
		{
			SubsystemSettings settings;

			if (!File.Exists(Path))
			{
				settings = CreateNewSettingsFile();
			}
			else
			{
				settings = EditorUtilityTools.LoadUnityAssetFile<SubsystemSettings>(Path);
			}

			settings.hideFlags = HideFlags.HideAndDontSave;

			if (settings.Version != CurrentVersion)
			{
				ApplyMigration(settings, CurrentVersion);
				Save(settings);
			}

			return settings;
		}

		#endregion

		#region Migration

		private static void ApplyMigration(SubsystemSettings settings, string targetVersion)
		{
			switch (settings.Version)
			{
				// Example
				case "0":
				{
					// Do the migration here.
					// MigrationsToUpdateFromVersion0ToVersion1();

					// Mark the settings with resulting migration.
					settings.Version = "1";
					break;
				}

				default:
					settings.Version = targetVersion;
					return;
			}

			// Apply migration over and over until we reach the target version.
			ApplyMigration(settings, targetVersion);
		}

		#endregion
	}

}
