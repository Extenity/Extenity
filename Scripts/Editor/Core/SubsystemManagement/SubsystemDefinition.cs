using System;
using System.Collections.Generic;
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
		[PropertySpace(SpaceBefore = 10)]
		[OnInspectorGUI, PropertyOrder(1)]
		private void _Separator() { }
#endif

		[BoxGroup("Scene Name Filter")]
		[InlineProperty, HideLabel, PropertyOrder(2)]
		public StringFilter SceneNameMatch;

		[PropertyOrder(3)]
		[PropertySpace(SpaceAfter = 10)]
		[ListDrawerSettings(Expanded = true)]
		public string[] SubsystemGroups;
	}

	[Serializable]
	public struct SubsystemGroup
	{
		[HorizontalGroup("NameLine", Order = 1, MaxWidth = 250)]
		[HideLabel, SuffixLabel("Group Name")]
		[Tooltip("Name of the Subsystem Group.")]
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
