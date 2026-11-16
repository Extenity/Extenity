#if UNITY_5_3_OR_NEWER
#if ExtenitySubsystems

using System;
using Extenity.DataToolbox;
using UnityEngine;
using UnityEngine.Serialization;

namespace Extenity.SubsystemManagementToolbox
{

	[Serializable]
	[ExcludeFromPresetAttribute]
	public class SubsystemSettings : ExtenityScriptableObject<SubsystemSettings>
	{
		#region Configuration

		protected override string LatestVersion => SubsystemConstants.Version;

		#endregion

		#region Subsystems

		[FormerlySerializedAs("ApplicationSubsystems")]
		public ApplicationSubsystemGroup ApplicationSubsystemGroup = new ApplicationSubsystemGroup();

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
			Log.Verbose("Initializing application subsystems.");
			using (Log.IndentedScope)
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
					Log.Verbose($"Initializing subsystems for scene '{sceneName}'.");
					using (Log.IndentedScope)
					{
						foreach (var subsystemGroupName in definition.SubsystemGroupsToBeLoaded)
						{
							var subsystemGroup = GetSceneSubsystemGroup(subsystemGroupName);
							Log.Verbose($"Initializing subsystem group '{subsystemGroupName}'.");
							using (Log.IndentedScope)
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

		#region Log

		private static readonly Logger Log = new(nameof(SubsystemSettings));

		#endregion
	}

}

#endif
#endif
