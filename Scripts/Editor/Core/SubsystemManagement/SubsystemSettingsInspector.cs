#if UNITY_5_3_OR_NEWER
#if ExtenitySubsystems

using System;
using System.Text;
using Extenity.DataToolbox;
using Extenity.InspectorToolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Extenity.SubsystemManagementToolbox.Editor
{

	/// <summary>
	/// The inspector of <see cref="SubsystemSettings"/>, also shown on the Subsystems page of the Project Settings:
	/// a short description with the details in a closed foldout, then three tabs for the application subsystems,
	/// the scene subsystem groups and the scene filters.
	/// </summary>
	[CustomEditor(typeof(SubsystemSettings))]
	public sealed class SubsystemSettingsInspector : ObjectInspector<SubsystemSettings>
	{
		internal const string Summary = "Subsystem Manager initializes subsystems of the application whenever a scene is loaded.";

		internal const string Details =
			"<b>What is a Subsystem?</b>\n" +
			"Like an Audio Manager, Camera Manager, Network Manager, Ingame Console, etc. Subsystems " +
			"are the pillars of an application, which are generally required to live throughout the entire " +
			"lifetime of the application. Some of them are required to initialize with loading a scene " +
			"and then deinitialize when switching to another scene." +
			"\n\n" +
			"<b>The Idea Behind</b>\n" +
			"Subsystem Manager decouples Subsystems from Level Design practices. Designers won't have to " +
			"add subsystem prefabs or scripts in any of their scenes. That solves the great pain of " +
			"managing the synchronization of every game level scene in the project. The scenes may even " +
			"come from asset bundles or external projects." +
			"\n\n" +
			"There is also that great need to press Play from any scene, and then all the required " +
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
			"<b>Usage</b>\n" +
			// TODO:
			// Initialize("Splash Delayed");
			// What if a subsystem requires reference to an object in a scene?
			// Scene name filters are checked from top to bottom and the first matching scene definition is picked.
			// Adding * wildcard as the last scene will allow applying the last entry to any other scene.
			"***TODO***";

		internal const string ApplicationSubsystemsInfo =
			"Application Subsystems are created before ALL game objects at the time of Unity's " +
			"<i>RuntimeInitializeLoadType.BeforeSceneLoad</i> call. They will automatically be marked with " +
			"DontDestroyOnLoad.";

		internal const string SceneSubsystemsInfo = "The groups defined here will be initialized for scenes as defined in <b>Scene Setup</b> page.";

		internal const string SceneSetupInfo =
			"Whenever the application loads a scene, the system will determine which subsystems needs to be " +
			"loaded for that scene. The system does that by looking up the name of the scene for each filter in " +
			"this list one by one. The first filter that matches the scene name will be selected and its " +
			"<i>`Subsystem Groups To Be Loaded`</i> list will be executed. All other filters will be discarded.";

		protected override void Configure(InspectorBuilder<SubsystemSettings> b)
		{
			b.HideScriptField();
			b.Info(Summary).AtTop();
			b.Group("Details", GroupStyle.Foldout, g => g.Info(Details)).CollapsedByDefault().AtTop();

			b.Tabs(tabs =>
			{
				tabs.Tab("Application Subsystems", page =>
				{
					page.Info(ApplicationSubsystemsInfo);
					page.Fields(x => x.ApplicationSubsystemGroup);
				});
				tabs.Tab("Scene Subsystems", page =>
				{
					page.Info(SceneSubsystemsInfo);
					page.Fields(x => x.SceneSubsystemGroups);
				});
				tabs.Tab("Scene Setup", page =>
				{
					page.Info(SceneSetupInfo);
					page.Fields(x => x.Scenes);
				});
			});
			b.Field(x => x.ApplicationSubsystemGroup).Inline();
			b.List(x => x.SceneSubsystemGroups).ExpandedByDefault().ElementLabel(group => group.Name);
			b.List(x => x.Scenes).Label("Scene Filters").ExpandedByDefault();
		}
	}

	/// <summary>
	/// A scene filter: titled by the scenes it matches (in a list row), its name filter and the groups it loads.
	/// </summary>
	[CustomPropertyDrawer(typeof(SubsystemDefinitionOfScene))]
	public sealed class SubsystemDefinitionOfSceneInspector : NestedInspector<SubsystemDefinitionOfScene>
	{
		protected override void Configure(InspectorBuilder<SubsystemDefinitionOfScene> b)
		{
			b.Title(DescribeScenes);
			b.Field(x => x.SceneNameMatch).Inline();
			b.List(x => x.SubsystemGroupsToBeLoaded).ExpandedByDefault();
		}

		/// <summary>The scenes that a scene filter matches, in words.</summary>
		internal static string DescribeScenes(SubsystemDefinitionOfScene definition)
		{
			var filters = definition.SceneNameMatch?.Filters;
			if (filters == null || filters.Length == 0)
				return "Not configured yet";

			if (filters.Length == 1 && (
				    (filters[0].FilterType == StringFilterType.Wildcard && filters[0].Filter == "*") ||
				    filters[0].FilterType == StringFilterType.Any))
			{
				return "All other scenes";
			}

			var builder = new StringBuilder();
			for (var i = 0; i < filters.Length; i++)
			{
				builder.Append('\'');
				builder.Append(filters[i].ToHumanReadableString());
				builder.Append('\'');
				if (i < filters.Length - 1)
					builder.Append(", ");
			}
			builder.Append(filters.Length > 1 ? " scenes" : " scene");
			return builder.ToString();
		}
	}

	[CustomPropertyDrawer(typeof(ApplicationSubsystemGroup))]
	public sealed class ApplicationSubsystemGroupInspector : NestedInspector<ApplicationSubsystemGroup>
	{
		protected override void Configure(InspectorBuilder<ApplicationSubsystemGroup> b)
		{
			b.List(x => x.Subsystems)
			 .Label("Application Subsystems")
			 .ExpandedByDefault()
			 .Searchable((definition, text) => SubsystemSearch.Matches(definition.Type, definition.Prefab, definition.SingletonType, definition.ResourcePath, text));
		}
	}

	/// <summary>
	/// A scene subsystem group: its name and its subsystems. A new subsystem takes the settings of the last one.
	/// </summary>
	[CustomPropertyDrawer(typeof(SceneSubsystemGroup))]
	public sealed class SceneSubsystemGroupInspector : NestedInspector<SceneSubsystemGroup>
	{
		protected override void Configure(InspectorBuilder<SceneSubsystemGroup> b)
		{
			b.Title(group => group.Name);
			b.Field(x => x.Name).Label("Group Name");
			b.List(x => x.Subsystems)
			 .Label(group => $"Subsystems to be initialized in '{group.Name}' group")
			 .Searchable((definition, text) => SubsystemSearch.Matches(definition.Type, definition.Prefab, definition.SingletonType, definition.ResourcePath, text))
			 .CustomAdd(CreateNewSubsystem);
		}

		internal static SubsystemDefinition CreateNewSubsystem(SceneSubsystemGroup group)
		{
			if (group.Subsystems == null || group.Subsystems.Length == 0)
				return new SubsystemDefinition() { Type = SubsystemType.Prefab, InstantiateEveryTime = false, DontDestroyOnLoad = true };

			// Copy the last item's configuration.
			var last = group.Subsystems[group.Subsystems.Length - 1];
			return new SubsystemDefinition() { Type = last.Type, InstantiateEveryTime = last.InstantiateEveryTime, DontDestroyOnLoad = last.DontDestroyOnLoad };
		}
	}

	[CustomPropertyDrawer(typeof(ApplicationSubsystemDefinition))]
	public sealed class ApplicationSubsystemDefinitionInspector : NestedInspector<ApplicationSubsystemDefinition>
	{
		protected override void Configure(InspectorBuilder<ApplicationSubsystemDefinition> b)
		{
			b.Title(definition => SubsystemSearch.Describe(definition.Type, definition.Prefab, definition.SingletonType, definition.ResourcePath));
			b.Row(r =>
			{
				r.Field(x => x.Type).HideLabel().Width(110);
				r.Field(x => x.Prefab).HideLabel().Grow()
				 .VisibleWhen(definition => definition.Type == SubsystemType.Prefab)
				 .Validate(SubsystemSearch.IsPrefabAsset, SubsystemSearch.PrefabAssetMessage);
				r.Field(x => x.SingletonType).HideLabel().Grow().ReadOnly()
				 .VisibleWhen(definition => definition.Type == SubsystemType.SingletonClass);
				r.Field(x => x.ResourcePath).HideLabel().Grow()
				 .VisibleWhen(definition => definition.Type == SubsystemType.Resource);
			});
			b.Info(SubsystemSearch.NotImplementedMessage, MessageSeverity.Error)
			 .VisibleWhen(definition => definition.Type == SubsystemType.SingletonClass);
		}
	}

	[CustomPropertyDrawer(typeof(SubsystemDefinition))]
	public sealed class SubsystemDefinitionInspector : NestedInspector<SubsystemDefinition>
	{
		protected override void Configure(InspectorBuilder<SubsystemDefinition> b)
		{
			b.Title(definition => SubsystemSearch.Describe(definition.Type, definition.Prefab, definition.SingletonType, definition.ResourcePath));
			b.Row(r =>
			{
				r.Field(x => x.Type).HideLabel().Width(110);
				r.Field(x => x.Prefab).HideLabel().Grow()
				 .VisibleWhen(definition => definition.Type == SubsystemType.Prefab)
				 .Validate(SubsystemSearch.IsPrefabAsset, SubsystemSearch.PrefabAssetMessage);
				r.Field(x => x.SingletonType).HideLabel().Grow().ReadOnly()
				 .VisibleWhen(definition => definition.Type == SubsystemType.SingletonClass);
				r.Field(x => x.ResourcePath).HideLabel().Grow()
				 .VisibleWhen(definition => definition.Type == SubsystemType.Resource);
			});
			b.Row(r =>
			{
				r.Field(x => x.InstantiateEveryTime).ToggleLeft().Grow()
				 .HiddenWhen(definition => definition.Type == SubsystemType.SingletonClass);
				r.Field(x => x.DontDestroyOnLoad).ToggleLeft().Grow()
				 .HiddenWhen(definition => definition.Type == SubsystemType.SingletonClass);
			});
			b.Info(SubsystemSearch.NotImplementedMessage, MessageSeverity.Error)
			 .VisibleWhen(definition => definition.Type == SubsystemType.SingletonClass);
		}
	}

	internal static class SubsystemSearch
	{
		internal const string NotImplementedMessage = "Not implemented yet!";
		internal const string PrefabAssetMessage = "The prefab must be an asset, not an object of a scene.";

		internal static bool IsPrefabAsset(GameObject prefab)
		{
			return !prefab || EditorUtility.IsPersistent(prefab);
		}

		internal static string Describe(SubsystemType type, GameObject prefab, string singletonType, string resourcePath)
		{
			switch (type)
			{
				case SubsystemType.Prefab: return prefab ? prefab.name : "(No prefab)";
				case SubsystemType.SingletonClass: return string.IsNullOrWhiteSpace(singletonType) ? "(No singleton type)" : singletonType;
				case SubsystemType.Resource: return string.IsNullOrWhiteSpace(resourcePath) ? "(No resource path)" : resourcePath;
				default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}

		/// <summary>
		/// Whether a subsystem matches the search text: its prefab's name or path, its singleton type or its
		/// resource path contains the text (spaces ignored) or resembles it closely enough.
		/// </summary>
		internal static bool Matches(SubsystemType type, GameObject prefab, string singletonType, string resourcePath, string searchString)
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
						var assetPath = AssetDatabase.GetAssetPath(prefab);
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
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}

			return false;
		}
	}

}

#endif
#endif
