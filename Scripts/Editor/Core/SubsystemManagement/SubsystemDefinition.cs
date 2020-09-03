using System;
using System.Collections.Generic;
using Extenity.ConsistencyToolbox;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.SubsystemManagementToolbox
{

	public enum SubsystemType
	{
		Prefab,
		SingletonClass,
	}

	[Serializable]
	public struct SubsystemLevel
	{
		[ListDrawerSettings(AlwaysAddDefaultValue = true, Expanded = true)]
		public SubsystemDefinition[] Subsystems;

		internal void ClearUnusedReferences()
		{
			foreach (var subsystem in Subsystems)
			{
				subsystem.ClearUnusedReferences();
			}
		}
	}

	[Serializable]
	public struct SubsystemDefinition : IConsistencyChecker
	{
		[HorizontalGroup(100f), HideLabel]
		public SubsystemType Type;

		[ShowIf(nameof(Type), SubsystemType.Prefab)]
		[HorizontalGroup, HideLabel]
		[AssetsOnly]
		public GameObject Prefab;

		[ShowIf(nameof(Type), SubsystemType.SingletonClass)]
		[HorizontalGroup, HideLabel]
		[InfoBox("Not implemented yet!", InfoMessageType.Error), ReadOnly]
		public string SingletonType;

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
		}

		#region Consistency

		public void CheckConsistency(ref List<ConsistencyError> errors)
		{
		}

		#endregion
	}

}
