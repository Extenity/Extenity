using System.IO;
using Extenity.ApplicationToolbox.Editor;
using Extenity.DataToolbox;
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

		private static SubsystemSettings _EditorInstance;

		public static SubsystemSettings EditorInstance
		{
			get
			{
				if (_EditorInstance == null)
				{
					_EditorInstance = LoadOrCreate();
				}

				return _EditorInstance;
			}
		}

		#endregion

		#region Version

		[HideInInspector]
		public string Version = CurrentVersion;

		#endregion

		#region Subsystems

#if UNITY_EDITOR
		[DetailedInfoBox("\n" +
		                 "Subsystem Manager initializes subsystems of the application whenever a scene is loaded." +
		                 "\n\n" +
		                 "Click here for details." +
		                 "\n",
		                 "\n" +
		                 "Subsystem Manager initializes subsystems of the application whenever a scene is loaded." +
		                 "\n\n" +
		                 "<b>What is a Subsystem?</b>" +
		                 "\n" +
		                 "Like an Audio Manager, Camera Manager, Network Manager, Ingame Console, etc. Subsystems are the pillars of an application. They are generally required to live throughout the entire lifetime of the application. Some of them are required to initialize with loading a scene and then deinitialize when switching to another scene." +
		                 "\n\n" +
		                 "<b>The Idea Behind</b>" +
		                 "\n" +
		                 "Subsystem Manager decouples Subsystems from Level Design practices. Designers won't have to add subsystem prefabs or scripts in any of their scenes. That solves the great pain of managing the synchronization of every game level scene in the project. The scenes may even come from asset bundles or external projects." +
		                 "\n\n" +
		                 "The system is designed to be foolproof. Being project-wide configuration rather than per-scene configuration, allows less human error and requires less thinking when designers create a new Level scene or a UI scene." +
		                 "\n\n" +
		                 "Every scene may require different subsystems. So the system allows configuring different subsystems per scene. The application may also need to initialize subsystems step by step throughout loading consecutive scenes. Let's say the Menu scene may require Audio Manager subsystem but a game level scene may both require Ingame HUD subsystem along with that Audio Manager subsystem. So the system is flexible enough to allow these consecutive loading operations too." +
		                 "\n\n" +
		                 "There is also that great need to press Play from any scene, and then all the required systems magically initialize themselves." +
		                 "\n\n" +
		                 "<b>Usage</b>" +
		                 "\n" +
		                 // TODO:
		                 // Initialize("Splash Delayed");
		                 // What if a subsystem requires reference to an object in a scene?
		                 // Scene name filters are checked from top to bottom and the first matching scene definition is picked.
		                 // Adding * wildcard as the last scene will allow applying the last entry to any other scene.
		                 "***TODO***" +
		                 "\n")]
		[VerticalGroup("Main")]
		[VerticalGroup("Main/Help", Order = 1)]
		[PropertySpace(SpaceBefore = 12), PropertyOrder(1)]
		[OnInspectorGUI]
		private void _InfoBox() { }
#endif

		[TabGroup("Main/Tabs", "Subsystem Groups", Order = 2)]
		[ListDrawerSettings(Expanded = true)]
		public SubsystemGroup[] SubsystemGroups = new SubsystemGroup[]
		{
			new SubsystemGroup() { Name = "Splash" },
			new SubsystemGroup() { Name = "Splash Delayed" },
			new SubsystemGroup() { Name = "Main Menu" },
			new SubsystemGroup() { Name = "Ingame" },
		};

		[TabGroup("Main/Tabs", "Scene Definitions")]
		[ListDrawerSettings(Expanded = true)]
		public SubsystemDefinitionOfScene[] Scenes = new SubsystemDefinitionOfScene[]
		{
			new SubsystemDefinitionOfScene() { SubsystemGroupsToBeLoaded = new string[] { "Splash" }, SceneNameMatch = new StringFilter(new StringFilterEntry(StringFilterType.Exactly, "Splash")) },
			new SubsystemDefinitionOfScene() { SubsystemGroupsToBeLoaded = new string[] { "Splash", "Splash Delayed", "Main Menu" }, SceneNameMatch = new StringFilter(new StringFilterEntry(StringFilterType.Exactly, "MainMenu")) },
			new SubsystemDefinitionOfScene() { SubsystemGroupsToBeLoaded = new string[] { "Splash", "Splash Delayed", "Main Menu", "Ingame" }, SceneNameMatch = new StringFilter(new StringFilterEntry(StringFilterType.Wildcard, "*")) },
		};

		internal void ClearUnusedReferences()
		{
			if (SubsystemGroups != null)
			{
				for (var i = 0; i < SubsystemGroups.Length; i++)
				{
					SubsystemGroups[i].ClearUnusedReferences();
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
