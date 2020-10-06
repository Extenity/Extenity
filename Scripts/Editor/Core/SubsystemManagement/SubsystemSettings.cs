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

		[DetailedInfoBox("Click here for some help.",
		                 "\n" +
		                 "Subsystem Manager is responsible for creating subsystems of the application whenever a scene is loaded.\n\n" +
		                 "Every scene may require different subsystems. Menu scene may only need some basic subsystems but a game level scene may both require some ingame related ones on top of that menu subsystems.\n\n" +
		                 // TODO: Write more documentation here.
		                 "\n")]
		[PropertySpace(SpaceBefore = 12)]
		[ListDrawerSettings(Expanded = true)]
		[PropertyTooltip("An application generally needs one category. But mini games or headless server builds might need their own categories.")]
		public SubsystemCategory[] SubsystemCategories = new SubsystemCategory[]
		{
			new SubsystemCategory() { Name = "Main Game" },
		};

		internal void ClearUnusedReferences()
		{
			if (SubsystemCategories != null)
			{
				for (var i = 0; i < SubsystemCategories.Length; i++)
				{
					SubsystemCategories[i].ClearUnusedReferences();
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
