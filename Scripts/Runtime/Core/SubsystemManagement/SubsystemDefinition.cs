#if UNITY_5_3_OR_NEWER
#if ExtenitySubsystems

using System;
using Extenity.ConsistencyToolbox;
using Extenity.DataToolbox;
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
		public StringFilter SceneNameMatch;

		public string[] SubsystemGroupsToBeLoaded;
	}

	[Serializable]
	public struct ApplicationSubsystemGroup
	{
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
		[Tooltip("Name of the Subsystem Group.")]
		public string Name;

		public SubsystemDefinition[] Subsystems;

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
	public class ApplicationSubsystemDefinition : IConsistencyChecker
	{
		public SubsystemType Type;

		public GameObject Prefab;

		public string SingletonType;

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

		#region Log

		private static readonly Logger Log = new(nameof(ApplicationSubsystemDefinition));

		#endregion
	}

	[Serializable]
	public class SubsystemDefinition : IConsistencyChecker
	{
		public SubsystemType Type;

		[Tooltip("Set to instantiate the subsystem only once or every time the scene loads.")]
		public bool InstantiateEveryTime;

		[Tooltip("Tell Unity to mark the instantiated object to not destroy on scene changes.")]
		public bool DontDestroyOnLoad;

		public GameObject Prefab;

		public string SingletonType;

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

		#region Log

		private static readonly Logger Log = new(nameof(SubsystemDefinition));

		#endregion
	}

}

#endif
#endif
