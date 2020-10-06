using System;
using System.Collections.Generic;
using Extenity.ConsistencyToolbox;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
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
	public class SubsystemCategory
	{
		[HideInInspector]
		public bool Active = true;

#if UNITY_EDITOR
		[HideIf(nameof(Active))]
		[HorizontalGroup("NameLine", Order = 1, Width = 40)]
		[VerticalGroup("NameLine/Toggle", Order = 1), PropertyOrder(2)]
		[GUIColor(0.7f, 0.2f, 0.2f, 1f)]
		[Button(ButtonHeight = 20, Name = "Off")]
		[PropertySpace(SpaceBefore = 10)]
		[PropertyTooltip("Activate to include whole category in runtime checks. When loading a scene, only active Subsystem Categories will be checked if the scene should trigger loading of some Subsystem Levels in that Category. The active state can be changed in runtime or in a build preprocessor that selectively activates Categories by build preferences and platforms.")]
		private void _Activate()
		{
			Active = true;
		}

		[ShowIf(nameof(Active))]
		[VerticalGroup("NameLine/Toggle"), PropertyOrder(1)]
		[GUIColor(0.2f, 0.7f, 0.2f, 1f)]
		[Button(ButtonHeight = 20, Name = "On")]
		[PropertySpace(SpaceBefore = 10)]
		[PropertyTooltip("Activate to include whole category in runtime checks. When loading a scene, only active Subsystem Categories will be checked if the scene should trigger loading of some Subsystem Levels in that Category. The active state can be changed in runtime or in a build preprocessor that selectively activates Categories by build preferences and platforms.")]
		private void _Deactivate()
		{
			Active = false;
		}
#endif

		[HorizontalGroup("NameLine/Name", Order = 2, MaxWidth = 250)]
		[HideLabel, SuffixLabel("Category Name")]
		[Tooltip("Name of the Subsystem Category. It's used for accessing category by its name and logging.")]
		[PropertySpace(SpaceBefore = 10)]
		public string Name;

		[PropertyOrder(2)]
		[ListDrawerSettings(OnBeginListElementGUI = "OnBeginListElementGUI", OnEndListElementGUI = "OnEndListElementGUI")]
		[PropertySpace(SpaceAfter = 10)]
		[PropertyTooltip("A Subsystem Level is configured to create its subsystems whenever loading a scene that matches Subsystem Level's scene filters.")] // It's called Level because the previous Levels will also be applied
		public SubsystemLevel[] SubsystemLevels = new SubsystemLevel[]
		{
			new SubsystemLevel() { Name = "Splash" },
			new SubsystemLevel() { Name = "Splash Delayed" },
			new SubsystemLevel() { Name = "Main Menu" },
			new SubsystemLevel() { Name = "Ingame" },
		};

		internal void ClearUnusedReferences()
		{
			if (SubsystemLevels != null)
			{
				for (var i = 0; i < SubsystemLevels.Length; i++)
				{
					SubsystemLevels[i].ClearUnusedReferences();
				}
			}
		}

#if UNITY_EDITOR
		private void OnBeginListElementGUI(int index)
		{
			if (!Active)
			{
				GUIHelper.PushColor(new Color(1f, 1f, 1f, 0.55f));
			}
		}

		private void OnEndListElementGUI(int index)
		{
			if (!Active)
			{
				GUIHelper.PopColor();
			}
		}
#endif
	}

	[Serializable]
	public struct SubsystemLevel
	{
		[HorizontalGroup("NameLine", Order = 1, MaxWidth = 250)]
		[HideLabel, SuffixLabel("Level Name")]
		[Tooltip("Name of the Subsystem Level.")]
		[PropertySpace(SpaceBefore = 10, SpaceAfter = 6)]
		public string Name;

		[PropertyOrder(2)]
		[ListDrawerSettings(CustomAddFunction = "_AddSubsystemsBeforeScene")]
		public SubsystemDefinition[] SubsystemsBeforeScene;

		[PropertyOrder(3)]
		[ListDrawerSettings(CustomAddFunction = "_AddSubsystemsAfterScene")]
		[PropertySpace(SpaceAfter = 10)]
		public SubsystemDefinition[] SubsystemsAfterScene;

		internal void ClearUnusedReferences()
		{
			if (SubsystemsBeforeScene != null)
			{
				for (var i = 0; i < SubsystemsBeforeScene.Length; i++)
				{
					SubsystemsBeforeScene[i].ClearUnusedReferences();
				}
			}
			if (SubsystemsAfterScene != null)
			{
				for (var i = 0; i < SubsystemsAfterScene.Length; i++)
				{
					SubsystemsAfterScene[i].ClearUnusedReferences();
				}
			}
		}

#if UNITY_EDITOR
		private SubsystemDefinition _AddSubsystemsBeforeScene()
		{
			return new SubsystemDefinition() { Type = SubsystemType.Prefab, InstantiateEveryTime = false, DontDestroyOnLoad = true };
		}

		private SubsystemDefinition _AddSubsystemsAfterScene()
		{
			return new SubsystemDefinition() { Type = SubsystemType.Prefab, InstantiateEveryTime = true, DontDestroyOnLoad = false };
		}
#endif
	}

	[Serializable]
	public struct SubsystemDefinition : IConsistencyChecker
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

		[InfoBox("Not implemented yet!", InfoMessageType.Error), ReadOnly]
		[HorizontalGroup, HideLabel]
		[ShowIf(nameof(Type), SubsystemType.Resource)]
		public string ResourcePath;

		internal void Initialize(bool dontDestroyOnLoad)
		{
			switch (Type)
			{
				case SubsystemType.Prefab:
				{
					if (Prefab)
					{
						InstantiateGameObject(Prefab);
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
						InstantiateGameObject(prefab);
					}
					return;
				}

				default:
					throw new ArgumentOutOfRangeException();
			}

			void InstantiateGameObject(GameObject prefab)
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

		public void CheckConsistency(ref List<ConsistencyError> errors)
		{
		}

		#endregion
	}

}
