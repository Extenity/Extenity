#if UNITY
#if ExtenitySubsystems

using System;
using Extenity.DataToolbox;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Extenity.SubsystemManagementToolbox
{

	[HideMonoScript]
	[Serializable]
	[ExcludeFromPresetAttribute]
	public class SubsystemSettings : ExtenityScriptableObject<SubsystemSettings>
	{
		#region Configuration

		protected override string LatestVersion => SubsystemConstants.Version;

		#endregion

		#region Subsystems

#if UNITY_EDITOR

		[DetailedInfoBox("\n" +
		                 "Subsystem Manager initializes subsystems of the application whenever a scene is loaded." +
		                 "\n\n" +
		                 "<b>Click here for details.</b>" +
		                 "\n",
		                 "\n" +
		                 "Subsystem Manager initializes subsystems of the application whenever a scene is loaded." +
		                 "\n\n" +
		                 "<b>What is a Subsystem?</b>" +
		                 "\n" +
		                 "Like an Audio Manager, Camera Manager, Network Manager, Ingame Console, etc. Subsystems " +
		                 "are the pillars of an application, which are generally required to live throughout the entire " +
		                 "lifetime of the application. Some of them are required to initialize with loading a scene " +
		                 "and then deinitialize when switching to another scene." +
		                 "\n\n" +
		                 "<b>The Idea Behind</b>" +
		                 "\n" +
		                 "Subsystem Manager decouples Subsystems from Level Design practices. Designers won't have to " +
		                 "add subsystem prefabs or scripts in any of their scenes. That solves the great pain of " +
		                 "managing the synchronization of every game level scene in the project. The scenes may even " +
		                 "come from asset bundles or external projects." +
		                 "\n\n" +
		                 "There is also that great need to press Play from any scene, and then all the required "+
		                 "systems magically initialize themselves." +
		                 "\n\n" +
		                 "The system is designed to be foolproof. Being project-wide configuration rather than " +
		                 "per-scene configuration, allows less human error and requires less thinking when designers " +
		                 "create a new Level scene or a UI scene." +
		                 "\n\n" +
		                 "The system is flexible enough to provide different sets of subsystems to be initialized for " +
		                 "different scenes. Let's say the Menu scene may require Audio Manager subsystem but a game " +
		                 "level scene may both require Ingame HUD subsystem along with that Audio Manager subsystem. " +
		                 "So the system is flexible enough to allow these consecutive loading operations too." +
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
		[VerticalGroup("Main", Order = 1)]
		[OnInspectorGUI, PropertySpace(SpaceBefore = 12)]
		private void _InfoBox() { }

		// Failed to find a proper way of inserting spaces.
		// [VerticalGroup("Main/Tabs/Subsystem Groups/Vertical")]
		// [OnInspectorGUI, PropertySpace(SpaceBefore = 20)]
		// private static void _Separator1() { }
		// [VerticalGroup("Main/Tabs/Subsystem Groups/Vertical")]
		// [OnInspectorGUI, PropertySpace(SpaceBefore = 20)]
		// private static void _Separator2() { }
#endif

		[TabGroup("Main/Tabs", "Application Subsystems", Order = 2)]
		[InfoBox("Application Subsystems are created before ALL game objects at the time of Unity's " +
		         "<i>RuntimeInitializeLoadType.BeforeSceneLoad</i> call. They will automatically be marked with " +
		         "DontDestroyOnLoad.")]
		[ListDrawerSettings(Expanded = true), HideLabel]
		[FormerlySerializedAs("ApplicationSubsystems")]
		public ApplicationSubsystemGroup ApplicationSubsystemGroup = new ApplicationSubsystemGroup();

		[TabGroup("Main/Tabs", "Scene Subsystems", Order = 3)]
		[InfoBox("The groups defined here will be initialized for scenes as defined in <b>Scene Setup</b> page.")]
		[VerticalGroup("Main/Tabs/Scene Subsystems/Vertical")]
		[ListDrawerSettings(Expanded = true)]
		[FormerlySerializedAs("SceneSubsystems")]
		public SceneSubsystemGroup[] SceneSubsystemGroups = new SceneSubsystemGroup[]
		{
			new SceneSubsystemGroup() { Name = "Splash" },
			new SceneSubsystemGroup() { Name = "Splash Delayed" },
			new SceneSubsystemGroup() { Name = "Main Menu" },
			new SceneSubsystemGroup() { Name = "Ingame" },
		};

		public SceneSubsystemGroup GetSceneSubsystemGroup(string groupName)
		{
			foreach (var group in SceneSubsystemGroups)
			{
				if (group.Name.Equals(groupName, StringComparison.Ordinal))
				{
					return group;
				}
			}
			throw new Exception($"Scene subsystem group '{groupName}' does not exist.");
		}

		internal void ResetStatus()
		{
			ApplicationSubsystemGroup.ResetStatus();

			if (SceneSubsystemGroups != null)
			{
				for (var i = 0; i < SceneSubsystemGroups.Length; i++)
				{
					SceneSubsystemGroups[i].ResetStatus();
				}
			}
		}

		internal void ClearUnusedReferences()
		{
			ApplicationSubsystemGroup.ClearUnusedReferences();

			if (SceneSubsystemGroups != null)
			{
				for (var i = 0; i < SceneSubsystemGroups.Length; i++)
				{
					SceneSubsystemGroups[i].ClearUnusedReferences();
				}
			}
		}

		#endregion

		#region Scenes

		[TabGroup("Main/Tabs", "Scene Setup", Order = 4)]
		[ListDrawerSettings(Expanded = true)]
		[InfoBox("Whenever the application loads a scene, the system will determine which subsystems needs to be " +
		         "loaded for that scene. The system does that by looking up the name of the scene for each filter in " +
		         "this list one by one. The first filter that matches the scene name will be selected and its " +
		         "<i>`Subsystem Groups To Be Loaded`</i> list will be executed. All other filters will be discarded.")]
		[LabelText("Scene Filters")]
		public SubsystemDefinitionOfScene[] Scenes = new SubsystemDefinitionOfScene[]
		{
			new SubsystemDefinitionOfScene() { SubsystemGroupsToBeLoaded = new string[] { "Splash" }, SceneNameMatch = new StringFilter(new StringFilterEntry(StringFilterType.Exactly, "Splash")) },
			new SubsystemDefinitionOfScene() { SubsystemGroupsToBeLoaded = new string[] { "Splash", "Splash Delayed", "Main Menu" }, SceneNameMatch = new StringFilter(new StringFilterEntry(StringFilterType.Exactly, "MainMenu")) },
			new SubsystemDefinitionOfScene() { SubsystemGroupsToBeLoaded = new string[] { "Splash", "Splash Delayed", "Main Menu", "Ingame" }, SceneNameMatch = new StringFilter(new StringFilterEntry(StringFilterType.Wildcard, "*")) },
		};

		public bool FindMatchingSceneDefinition(string sceneName, out SubsystemDefinitionOfScene definition)
		{
			foreach (var sceneDefinition in Scenes)
			{
				if (sceneDefinition.SceneNameMatch.IsMatching(sceneName))
				{
					definition = sceneDefinition;
					return true;
				}
			}
			definition = default;
			return false;
		}

		#endregion

		#region Instantiate Subsystems

		public void InitializeForApplication()
		{
			using (Log.Indent("Initializing application subsystems."))
			{
				foreach (var subsystem in ApplicationSubsystemGroup.Subsystems)
				{
					subsystem.Initialize();
				}
			}
		}

		public bool InitializeForScene(string sceneName)
		{
			if (FindMatchingSceneDefinition(sceneName, out var definition))
			{
				if (definition.SubsystemGroupsToBeLoaded.IsNotNullAndEmpty())
				{
					using (Log.Indent($"Initializing subsystems for scene '{sceneName}'."))
					{
						foreach (var subsystemGroupName in definition.SubsystemGroupsToBeLoaded)
						{
							var subsystemGroup = GetSceneSubsystemGroup(subsystemGroupName);
							using (Log.Indent($"Initializing subsystem group '{subsystemGroupName}'."))
							{
								foreach (var subsystem in subsystemGroup.Subsystems)
								{
									subsystem.Initialize();
								}
							}
						}
					}
				}
				else
				{
					Log.Info($"Skipped subsystem initialization for scene '{sceneName}' that has no groups defined.");
				}
				return true;
			}
			return false;
		}

		#endregion

		#region Validation

		public override bool IsFileNameValid()
		{
			return name.Equals(SubsystemConstants.ConfigurationFileNameWithoutExtension, StringComparison.Ordinal);
		}

		#endregion

		#region Version and Migration

		protected override void ApplyMigration(string targetVersion)
		{
			switch (Version)
			{
				// Example
				case "0":
				{
					// Do the migration here.
					// MigrationsToUpdateFromVersion0ToVersion1();

					// Mark the settings with resulting migration.
					Version = "1";
					break;
				}

				default:
					Version = targetVersion;
					return;
			}

			// Apply migration over and over until we reach the target version.
			ApplyMigration(targetVersion);
		}

		#endregion

		#region Serialization

		protected override void OnBeforeSerializeDerived()
		{
			ClearUnusedReferences();
		}

		#endregion

		#region Search

#if UNITY_EDITOR

		internal static bool CheckSearchFilter(SubsystemType type, GameObject prefab, string singletonType, string resourcePath, string searchString)
		{
			const float MatchThreshold = 0.6f;

			if (string.IsNullOrWhiteSpace(searchString))
				return false;

			searchString = searchString.Replace(" ", "");

			switch (type)
			{
				case SubsystemType.Prefab:
				{
					if (prefab)
					{
						var name = prefab.name;
						if (name.Contains(searchString, StringComparison.InvariantCultureIgnoreCase))
							return true;
						if (LiquidMetalStringMatcher.Score(name, searchString) > MatchThreshold)
							return true;
						var assetPath = UnityEditor.AssetDatabase.GetAssetPath(prefab);
						if (assetPath.Contains(searchString, StringComparison.InvariantCultureIgnoreCase))
							return true;
						return LiquidMetalStringMatcher.Score(assetPath, searchString) > MatchThreshold;
					}
					break;
				}

				case SubsystemType.SingletonClass:
					if (!string.IsNullOrWhiteSpace(singletonType))
					{
						if (singletonType.Contains(searchString, StringComparison.InvariantCultureIgnoreCase))
							return true;
						return LiquidMetalStringMatcher.Score(singletonType, searchString) > MatchThreshold;
					}
					break;

				case SubsystemType.Resource:
				{
					if (!string.IsNullOrWhiteSpace(resourcePath))
					{
						if (resourcePath.Contains(searchString, StringComparison.InvariantCultureIgnoreCase))
							return true;
						return LiquidMetalStringMatcher.Score(resourcePath, searchString) > MatchThreshold;
					}
					break;
				}

				default:
					throw new ArgumentOutOfRangeException();
			}

			return false;
		}

#endif

		#endregion
	}

}

#endif
#endif
