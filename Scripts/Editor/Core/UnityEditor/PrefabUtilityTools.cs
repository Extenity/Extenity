using System;
using Extenity.DataToolbox;
using UnityEngine;
using UnityEditor;

namespace Extenity.UnityEditorToolbox.Editor
{

	public static class PrefabUtilityTools
	{
		#region Prefab Asset

		public static bool IsPrefab(this GameObject me, bool includePrefabInstances, bool includeDisconnectedPrefabInstances, bool includeMissingPrefabInstances)
		{
			if (!me)
				return false;
			var type = PrefabUtility.GetPrefabType(me);

			if (type.IsPrefabAsset())
				return true;
			if (includePrefabInstances && type.IsHealthyInstance())
				return true;
			if (includeDisconnectedPrefabInstances && type.IsDisconnectedInstance())
				return true;
			if (includeMissingPrefabInstances && type.IsInstanceMissing())
				return true;
			return false;
		}

		public static GameObject GetRootGameObjectIfChildOfAPrefabAsset(this GameObject me)
		{
			if (!me)
				return null;
			var type = PrefabUtility.GetPrefabType(me);

			if (type.IsPrefabAsset())
			{
				return PrefabUtility.FindPrefabRoot(me);
			}

			return me;
		}

		#endregion

		#region Prefab Type

		public static bool IsHealthyInstance(this PrefabType type)
		{
			return type == PrefabType.PrefabInstance || type == PrefabType.ModelPrefabInstance;
		}

		public static bool IsDisconnectedInstance(this PrefabType type)
		{
			return type == PrefabType.DisconnectedPrefabInstance || type == PrefabType.DisconnectedModelPrefabInstance;
		}

		public static bool IsInstanceMissing(this PrefabType type)
		{
			return type == PrefabType.MissingPrefabInstance;
		}

		public static bool IsPrefabAsset(this PrefabType type)
		{
			return type == PrefabType.Prefab || type == PrefabType.ModelPrefab;
		}

		#endregion

		#region Instantiate Prefab

		public static GameObject InstantiatePrefabOrSceneObject(GameObject original, bool keepPrefabLinkIfSceneObject)
		{
			ObjectTools.CheckNullArgument(original, "The Object you want to instantiate is null.");

			var prefabType = PrefabUtility.GetPrefabType(original);

			switch (prefabType)
			{
				case PrefabType.None:
					{
						// Not a prefab. Do a standard object instantiation.
						return GameObject.Instantiate(original);
					}
				case PrefabType.Prefab:
				case PrefabType.ModelPrefab:
					{
						var instantiated = PrefabUtility.InstantiatePrefab(original);
						if (!instantiated)
						{
							throw new Exception($"Could not instantiate prefab from reference '{original}'.");
						}
						return (GameObject)instantiated;
					}
				case PrefabType.PrefabInstance:
				case PrefabType.ModelPrefabInstance:
					{
						if (keepPrefabLinkIfSceneObject)
						{
							return original.Duplicate(true);
						}
						else
						{
							return GameObject.Instantiate(original);
						}
					}
				case PrefabType.MissingPrefabInstance:
				case PrefabType.DisconnectedPrefabInstance:
				case PrefabType.DisconnectedModelPrefabInstance:
					throw new NotImplementedException("Prefab instantiation with a missing or disconnected prefab instance is not implemented yet.");
				default:
					throw new ArgumentOutOfRangeException("prefabType", prefabType, "prefabType");
			}
		}

		#endregion
	}

}
