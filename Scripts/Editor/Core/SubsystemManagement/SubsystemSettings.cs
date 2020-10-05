using System.IO;
using Extenity.ApplicationToolbox.Editor;
using Extenity.FileSystemToolbox;
using Extenity.UnityEditorToolbox.Editor;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.SubsystemManagementToolbox
{

	[HideMonoScript]
	public class SubsystemSettings : ScriptableObject, ISerializationCallbackReceiver
	{
		#region Configuration

		private static readonly string ConfigurationFilePath = EditorApplicationTools.ProjectSettingsRelativePath.AppendFileToPath(SubsystemConstants.ConfigurationFileName);
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

		[HideInInspector]
		public string Version = CurrentVersion;

		#endregion

		#region Subsystems

		[TitleGroup("Subsystems", Alignment = TitleAlignments.Centered)]
		[ListDrawerSettings(AlwaysAddDefaultValue = true, Expanded = true)]
		public SubsystemLevel[] SubsystemLevels = new SubsystemLevel[]
		{
			new SubsystemLevel() { Name = "Splash" },
			new SubsystemLevel() { Name = "Splash Delayed" },
			new SubsystemLevel() { Name = "Main Menu" },
			new SubsystemLevel() { Name = "Ingame" },
		};

		private void ClearUnusedReferences()
		{
			if (SubsystemLevels != null)
			{
				for (var i = 0; i < SubsystemLevels.Length; i++)
				{
					SubsystemLevels[i].ClearUnusedReferences();
				}
			}
		}

		#endregion

		#region Save / Load

		private static SubsystemSettings CreateNewSettingsFile()
		{
			var settings = CreateInstance<SubsystemSettings>();
			Save(settings);
			return settings;
		}

		internal static void Save(SubsystemSettings settings)
		{
			EditorUtilityTools.SaveUnityAssetFile(ConfigurationFilePath, settings);
		}

		internal void Save()
		{
			EditorUtilityTools.SaveUnityAssetFile(ConfigurationFilePath, this);
		}

		private static SubsystemSettings LoadOrCreate()
		{
			SubsystemSettings settings;

			if (!File.Exists(ConfigurationFilePath))
			{
				settings = CreateNewSettingsFile();
			}
			else
			{
				settings = EditorUtilityTools.LoadUnityAssetFile<SubsystemSettings>(ConfigurationFilePath);
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

		#region Serialization

		public void OnBeforeSerialize()
		{
			ClearUnusedReferences();

			if (Version != CurrentVersion)
			{
				ApplyMigration(this, CurrentVersion);
			}
		}

		public void OnAfterDeserialize()
		{
		}

		#endregion
	}

}
