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
		[ToggleLeft]
		[Tooltip("Activate to include whole category in runtime checks. When loading a scene, only active Subsystem Categories will be checked if the scene should trigger loading of some Subsystem Levels in that Category. The active state can be changed in runtime or in a build preprocessor that selectively activates Categories by build preferences and platforms.")]
		public bool Active = true;

		[LabelWidth(60)]
		public string Name;

		[ListDrawerSettings(Expanded = true, OnBeginListElementGUI = "OnBeginListElementGUI", OnEndListElementGUI = "OnEndListElementGUI")]
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
		public string Name;

		[ListDrawerSettings(Expanded = true, CustomAddFunction = "_AddSubsystemsBeforeScene")]
		public SubsystemDefinition[] SubsystemsBeforeScene;

		[ListDrawerSettings(Expanded = true, CustomAddFunction = "_AddSubsystemsAfterScene")]
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
					var instance = GameObject.Instantiate(Prefab);

					// Remove "(Clone)" from the name and add '_' prefix.
					instance.name = "_" + Prefab.name;

					// Set parent
					// if (parent != null)
					// {
					// 	instance.transform.SetParent(parent);
					// }

					if (dontDestroyOnLoad)
					{
						GameObject.DontDestroyOnLoad(instance);
					}

					return;
				}

				case SubsystemType.SingletonClass:
				{
					throw new NotImplementedException();
				}

				case SubsystemType.Resource:
				{
					throw new NotImplementedException();
				}

				default:
					throw new ArgumentOutOfRangeException();
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
