#if UNITY
#if ExtenitySubsystems

using System;
using Extenity.ConsistencyToolbox;
using Extenity.DataToolbox;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.SubsystemManagementToolbox
{

	public enum SubsystemType
	{
		Prefab,
		SingletonClass,
		Resource,
	}

	[Serializable]
	public struct SubsystemDefinitionOfScene
	{
#if UNITY_EDITOR
		[Title("$_Title")]
		[OnInspectorGUI, PropertyOrder(2)]
		[PropertySpace(SpaceAfter = 6)]
		private void _TitleDrawer() { }

		private string _Title
		{
			get
			{
				var stringBuilder = StringTools.SharedStringBuilder.Value;
				StringTools.ClearSharedStringBuilder(stringBuilder);
				if (SceneNameMatch?.Filters?.Length > 0)
				{
					if (SceneNameMatch.Filters.Length == 1 && (
						(SceneNameMatch.Filters[0].FilterType == StringFilterType.Wildcard && SceneNameMatch.Filters[0].Filter == "*") ||
						(SceneNameMatch.Filters[0].FilterType == StringFilterType.Any)
					))
					{
						stringBuilder.Append("All other scenes");
					}
					else
					{
						for (var i = 0; i < SceneNameMatch.Filters.Length; i++)
						{
							var filter = SceneNameMatch.Filters[i];
							stringBuilder.Append('\'');
							stringBuilder.Append(filter.ToHumanReadableString());
							stringBuilder.Append('\'');

							if (i < SceneNameMatch.Filters.Length - 1)
							{
								stringBuilder.Append(", ");
							}
						}
						stringBuilder.Append(SceneNameMatch.Filters.Length > 1 ? " scenes" : " scene");
					}
				}
				else
				{
					stringBuilder.Append("Not configured yet");
				}
				return stringBuilder.ToString();
			}
		}
#endif

		[InlineProperty, HideLabel, PropertyOrder(3)]
		public StringFilter SceneNameMatch;

		[PropertyOrder(4)]
		[PropertySpace(SpaceAfter = 10)]
		[ListDrawerSettings(Expanded = true)]
		public string[] SubsystemGroupsToBeLoaded;
	}

	[Serializable]
	public struct ApplicationSubsystemGroup
	{
		[PropertySpace(SpaceBefore = 10, SpaceAfter = 6)]
		[ListDrawerSettings(Expanded = true)]
		[Searchable(FilterOptions = SearchFilterOptions.ISearchFilterableInterface)]
		[LabelText("Application Subsystems")]
		public ApplicationSubsystemDefinition[] Subsystems;

		internal void ResetStatus()
		{
			if (Subsystems != null)
			{
				for (var i = 0; i < Subsystems.Length; i++)
				{
					Subsystems[i].ResetStatus();
				}
			}
		}

		internal void ClearUnusedReferences()
		{
			if (Subsystems != null)
			{
				for (var i = 0; i < Subsystems.Length; i++)
				{
					Subsystems[i].ClearUnusedReferences();
				}
			}
		}
	}

	[Serializable]
	public struct SceneSubsystemGroup
	{
		[HorizontalGroup("NameLine", Order = 1, MaxWidth = 250)]
		[HideLabel, SuffixLabel("Group Name")]
		[Tooltip("Name of the Subsystem Group.")]
		[PropertySpace(SpaceBefore = 10, SpaceAfter = 6)]
		public string Name;

		[PropertyOrder(2)]
		[ListDrawerSettings(CustomAddFunction = "_AddNewSubsystem")]
		[Searchable(FilterOptions = SearchFilterOptions.ISearchFilterableInterface)]
		[LabelText("$_SubsystemsLabelText")]
		public SubsystemDefinition[] Subsystems;

#if UNITY_EDITOR
		private string _SubsystemsLabelText => $"Subsystems to be initialized in '{Name}' group";
#endif

		internal void ResetStatus()
		{
			if (Subsystems != null)
			{
				for (var i = 0; i < Subsystems.Length; i++)
				{
					Subsystems[i].ResetStatus();
				}
			}
		}

		internal void ClearUnusedReferences()
		{
			if (Subsystems != null)
			{
				for (var i = 0; i < Subsystems.Length; i++)
				{
					Subsystems[i].ClearUnusedReferences();
				}
			}
		}

		#region Editor

#if UNITY_EDITOR
		private SubsystemDefinition _AddNewSubsystem()
		{
			if (Subsystems == null || Subsystems.Length == 0)
			{
				return new SubsystemDefinition() { Type = SubsystemType.Prefab, InstantiateEveryTime = false, DontDestroyOnLoad = true };
			}
			else
			{
				// Copy the last item's configuration.
				var last = Subsystems[Subsystems.Length - 1];
				return new SubsystemDefinition() { Type = last.Type, InstantiateEveryTime = last.InstantiateEveryTime, DontDestroyOnLoad = last.DontDestroyOnLoad };
			}
		}
#endif

		#endregion
	}

	[Serializable]
	public class ApplicationSubsystemDefinition : IConsistencyChecker
#if UNITY_EDITOR
	                                              , ISearchFilterable
#endif
	{
		[HorizontalGroup(100f), HideLabel]
		public SubsystemType Type;

		[HorizontalGroup, HideLabel]
		[AssetsOnly]
		[ShowIf(nameof(Type), SubsystemType.Prefab)]
		public GameObject Prefab;

		[InfoBox("Not implemented yet!", InfoMessageType.Error), ReadOnly]
		[HorizontalGroup, HideLabel]
		[ShowIf(nameof(Type), SubsystemType.SingletonClass)]
		public string SingletonType;

		[HorizontalGroup, HideLabel]
		[ShowIf(nameof(Type), SubsystemType.Resource)]
		public string ResourcePath;

		public bool IsInstantiated { get; private set; }

		internal void ResetStatus()
		{
			IsInstantiated = false;
		}

		internal void Initialize()
		{
			if (IsInstantiated)
			{
				// Already instantiated before. Skip.
				return;
			}
			IsInstantiated = true;

			switch (Type)
			{
				case SubsystemType.Prefab:
				{
					if (Prefab)
					{
						InstantiateGameObject(Prefab, true);
					}
					return;
				}

				case SubsystemType.SingletonClass:
				{
					throw new NotImplementedException();
				}

				case SubsystemType.Resource:
				{
					if (!string.IsNullOrWhiteSpace(ResourcePath))
					{
						var prefab = Resources.Load<GameObject>(ResourcePath);
						if (!prefab)
						{
							Log.Error($"Subsystem prefab does not exist at resource path '{ResourcePath}'.");
							return;
						}
						InstantiateGameObject(prefab, true);
					}
					return;
				}

				default:
					throw new ArgumentOutOfRangeException();
			}

			void InstantiateGameObject(GameObject prefab, bool dontDestroyOnLoad)
			{
				var instance = GameObject.Instantiate(prefab);

				// Remove "(Clone)" from the name and add '_' prefix.
				instance.name = "_" + prefab.name;

				// Set parent
				// if (parent != null)
				// {
				// 	instance.transform.SetParent(parent);
				// }

				if (dontDestroyOnLoad)
				{
					GameObject.DontDestroyOnLoad(instance);
				}
			}
		}

		internal void ClearUnusedReferences()
		{
			if (Type != SubsystemType.Prefab)
			{
				Prefab = null;
			}

			if (Type != SubsystemType.SingletonClass)
			{
				SingletonType = null;
			}

			if (Type != SubsystemType.Resource)
			{
				ResourcePath = null;
			}
		}

		#region Consistency

		public void CheckConsistency(ConsistencyChecker checker) { }

		#endregion

		#region Search

#if UNITY_EDITOR

		public bool IsMatch(string searchString)
		{
			return SubsystemSettings.CheckSearchFilter(Type, Prefab, SingletonType, ResourcePath, searchString);
		}

#endif

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(ApplicationSubsystemDefinition));

		#endregion
	}

	[Serializable]
	public class SubsystemDefinition : IConsistencyChecker
#if UNITY_EDITOR
	                                   , ISearchFilterable
#endif
	{
		[HorizontalGroup(100f), HideLabel]
		public SubsystemType Type;

		[Tooltip("Set to instantiate the subsystem only once or every time the scene loads.")]
		[HorizontalGroup, ToggleLeft]
		[HideIf(nameof(Type), SubsystemType.SingletonClass)]
		public bool InstantiateEveryTime;

		[Tooltip("Tell Unity to mark the instantiated object to not destroy on scene changes.")]
		[HorizontalGroup, ToggleLeft]
		[HideIf(nameof(Type), SubsystemType.SingletonClass)]
		public bool DontDestroyOnLoad;

		[HorizontalGroup, HideLabel]
		[AssetsOnly]
		[ShowIf(nameof(Type), SubsystemType.Prefab)]
		public GameObject Prefab;

		[InfoBox("Not implemented yet!", InfoMessageType.Error), ReadOnly]
		[HorizontalGroup, HideLabel]
		[ShowIf(nameof(Type), SubsystemType.SingletonClass)]
		public string SingletonType;

		[HorizontalGroup, HideLabel]
		[ShowIf(nameof(Type), SubsystemType.Resource)]
		public string ResourcePath;

		public bool IsInstantiated { get; private set; }

		internal void ResetStatus()
		{
			IsInstantiated = false;
		}

		internal void Initialize()
		{
			if (IsInstantiated && !InstantiateEveryTime)
			{
				// Already instantiated before. Skip.
				return;
			}
			IsInstantiated = true;

			switch (Type)
			{
				case SubsystemType.Prefab:
				{
					if (Prefab)
					{
						InstantiateGameObject(Prefab, DontDestroyOnLoad);
					}
					return;
				}

				case SubsystemType.SingletonClass:
				{
					throw new NotImplementedException();
				}

				case SubsystemType.Resource:
				{
					if (!string.IsNullOrWhiteSpace(ResourcePath))
					{
						var prefab = Resources.Load<GameObject>(ResourcePath);
						if (!prefab)
						{
							Log.Error($"Subsystem prefab does not exist at resource path '{ResourcePath}'.");
							return;
						}
						InstantiateGameObject(prefab, DontDestroyOnLoad);
					}
					return;
				}

				default:
					throw new ArgumentOutOfRangeException();
			}

			void InstantiateGameObject(GameObject prefab, bool dontDestroyOnLoad)
			{
				var instance = GameObject.Instantiate(prefab);

				// Remove "(Clone)" from the name and add '_' prefix.
				instance.name = "_" + prefab.name;

				// Set parent
				// if (parent != null)
				// {
				// 	instance.transform.SetParent(parent);
				// }

				if (dontDestroyOnLoad)
				{
					GameObject.DontDestroyOnLoad(instance);
				}
			}
		}

		internal void ClearUnusedReferences()
		{
			if (Type != SubsystemType.Prefab)
			{
				Prefab = null;
			}

			if (Type != SubsystemType.SingletonClass)
			{
				SingletonType = null;
			}

			if (Type != SubsystemType.Resource)
			{
				ResourcePath = null;
			}
		}

		#region Consistency

		public void CheckConsistency(ConsistencyChecker checker) { }

		#endregion

		#region Search

#if UNITY_EDITOR

		public bool IsMatch(string searchString)
		{
			return SubsystemSettings.CheckSearchFilter(Type, Prefab, SingletonType, ResourcePath, searchString);
		}

#endif

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(SubsystemDefinition));

		#endregion
	}

}

#endif
#endif
