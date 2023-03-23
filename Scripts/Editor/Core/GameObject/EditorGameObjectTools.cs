using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Extenity.DataToolbox;
using Extenity.ReflectionToolbox;
using Extenity.SceneManagementToolbox;
using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Extenity.GameObjectToolbox.Editor
{

	public static class EditorGameObjectTools
	{
		#region FindStaticObjectsOfTypeAll in Scene

		public static List<T> FindStaticObjectsOfTypeInScenes<T>(StaticEditorFlags leastExpectedFlags, ActiveCheck activeCheck, SceneListFilter sceneListFilter) where T : Component
		{
			return SceneManagerTools.GetScenes(sceneListFilter)
			                        .FindStaticObjectsOfType<T>(leastExpectedFlags, activeCheck);
		}

		public static List<T> FindStaticObjectsOfType<T>(this IList<Scene> scenes, StaticEditorFlags leastExpectedFlags, ActiveCheck activeCheck) where T : Component
		{
			var results = new List<T>();
			for (var i = 0; i < scenes.Count; i++)
			{
				var list = scenes[i].FindStaticObjectsOfType<T>(leastExpectedFlags, activeCheck);
				results.AddRange(list);
			}
			return results;
		}

		public static List<T> FindStaticObjectsOfType<T>(this Scene scene, StaticEditorFlags leastExpectedFlags, ActiveCheck activeCheck) where T : Component
		{
			return scene.FindObjectsOfType<T>(activeCheck)
						.Where(component => component.gameObject.IsStaticEditorFlagsSetToAtLeast(leastExpectedFlags)).ToList();
		}

		#endregion

		#region SetParentOfAllObjectsContainingComponent

		public static void SetParentOfAllObjectsContainingComponentInScenes<T>(Transform parent, bool worldPositionStays, bool skipPrefabs, ActiveCheck activeCheck, SceneListFilter sceneListFilter) where T : Component
		{
			SceneManagerTools.GetScenes(sceneListFilter)
			                 .ForEach(scene => scene.SetParentOfAllObjectsContainingComponent<T>(parent, worldPositionStays, skipPrefabs, activeCheck));
		}

		public static void SetParentOfAllObjectsContainingComponent<T>(this Scene scene, Transform parent, bool worldPositionStays, bool skipPrefabs, ActiveCheck activeCheck) where T : Component
		{
			var componentTransforms = scene.FindObjectsOfType<T>(activeCheck)
			                               .Select(item => item.transform);

			if (skipPrefabs)
			{
				componentTransforms = componentTransforms.Where(xform => !PrefabUtility.IsPartOfNonAssetPrefabInstance(xform.gameObject));
			}

			foreach (var transform in componentTransforms)
			{
				transform.SetParent(parent, worldPositionStays);
				transform.SetAsLastSibling();
			}
		}

		#endregion

		#region SetParentOfAllStaticObjectsContainingComponent

		public static void SetParentOfAllStaticObjectsContainingComponentInScenes<T>(Transform parent, bool worldPositionStays, bool skipPrefabs, StaticEditorFlags leastExpectedFlags, ActiveCheck activeCheck, SceneListFilter sceneListFilter) where T : Component
		{
			SceneManagerTools.GetScenes(sceneListFilter)
			                 .ForEach(scene => scene.SetParentOfAllStaticObjectsContainingComponent<T>(parent, worldPositionStays, skipPrefabs, leastExpectedFlags, activeCheck));
		}

		public static void SetParentOfAllStaticObjectsContainingComponent<T>(this Scene scene, Transform parent, bool worldPositionStays, bool skipPrefabs, StaticEditorFlags leastExpectedFlags, ActiveCheck activeCheck) where T : Component
		{
			var componentTransforms = scene.FindObjectsOfType<T>(activeCheck)
					.Where(item => item.gameObject.IsStaticEditorFlagsSetToAtLeast(leastExpectedFlags))
					.Select(item => item.transform);

			if (skipPrefabs)
			{
				componentTransforms = componentTransforms.Where(xform => !PrefabUtility.IsPartOfNonAssetPrefabInstance(xform.gameObject));
			}

			foreach (var transform in componentTransforms)
			{
				transform.SetParent(parent, worldPositionStays);
				transform.SetAsLastSibling();
			}
		}

		#endregion

		#region EnsureNoObjectsContainingComponentExist

		public static void EnsureNoObjectsContainingComponentExistInScenes<T>(ActiveCheck activeCheck, SceneListFilter sceneListFilter) where T : Component
		{
			SceneManagerTools.GetScenes(sceneListFilter)
			                 .ForEach(scene => scene.EnsureNoObjectsContainingComponentExist<T>(activeCheck));
		}

		public static void EnsureNoObjectsContainingComponentExist<T>(this Scene scene, ActiveCheck activeCheck) where T : Component
		{
			var components = scene.FindObjectsOfType<T>(activeCheck);

			var found = false;
			foreach (var component in components)
			{
				if (component)
				{
					// Do not merge all logs in a single log entry. Log each error in a separate entry that points to the gameobject in log's context for ease of use.
					Log.ErrorWithContext(component, $"Object of component '{typeof(T).Name}' exists in scene '{scene.name}'.");
					found = true;
				}
			}

			// Fail by throwing an exception.
			if (found)
				throw new Exception($"Scene has object(s) that contain '{typeof(T).Name}' component. See previous error logs.");
		}

		#endregion

		#region EnsureNoStaticObjectsContainingComponentExist

		public static void EnsureNoStaticObjectsContainingComponentExistInScenes<T>(StaticEditorFlags leastExpectedFlags, ActiveCheck activeCheck, SceneListFilter sceneListFilter) where T : Component
		{
			SceneManagerTools.GetScenes(sceneListFilter)
			                 .ForEach(scene => scene.EnsureNoStaticObjectsContainingComponentExist<T>(leastExpectedFlags, activeCheck));
		}

		public static void EnsureNoStaticObjectsContainingComponentExist<T>(this Scene scene, StaticEditorFlags leastExpectedFlags, ActiveCheck activeCheck) where T : Component
		{
			var components = scene.FindObjectsOfType<T>(activeCheck)
				.Where(item => item.gameObject.IsStaticEditorFlagsSetToAtLeast(leastExpectedFlags));

			var found = false;
			foreach (var component in components)
			{
				if (component)
				{
					// Do not merge all logs in a single log entry. Log each error in a separate entry that points to the gameobject in log's context for ease of use.
					Log.ErrorWithContext(component, $"Static object of component '{typeof(T).Name}' exists in scene '{scene.name}'.");
					found = true;
				}
			}

			// Fail by throwing an exception.
			if (found)
				throw new Exception($"Scene has static object(s) that contain '{typeof(T).Name}' component. See previous error logs.");
		}

		#endregion

		#region Destroy Empty Unreferenced GameObjects

		public static void DestroyEmptyUnreferencedGameObjectsInScenes(Type[] excludedTypes, SceneListFilter sceneListFilter, bool undoable, bool log)
		{
			SceneManagerTools.GetScenes(sceneListFilter)
			                 .ForEach(scene => scene.DestroyEmptyUnreferencedGameObjects(excludedTypes, undoable, log));
		}

		public static void DestroyEmptyUnreferencedGameObjects(this Scene scene, Type[] excludedTypes, bool undoable, bool log)
		{
			var gameObjects = scene.ListAllGameObjectsInScene();
			if (gameObjects.IsNullOrEmpty())
				return;

			// Get all components in scene, then get a list of what other scene components and gameobjects they keep references to.
			// We will destroy all gameobjects in scene those are not put into the list.
			var allComponents = scene.FindObjectsOfType<Component>(ActiveCheck.IncludingInactive);
			var allReferencedObjects = new HashSet<GameObject>();
			foreach (var component in allComponents)
			{
				if (component)
				{
					component.FindAllReferencedGameObjectsInComponent(allReferencedObjects, excludedTypes);
				}
			}

			StringBuilder deletedObjectsText = null;
			StringBuilder skippedObjectsText = null;
			if (log)
			{
				deletedObjectsText = new StringBuilder();
				skippedObjectsText = new StringBuilder();
			}

			int destroyedCount = 0;
			int skippedCount = 0;
			bool needsReRun;
			do
			{
				needsReRun = false;
				for (var i = 0; i < gameObjects.Count; i++)
				{
					var gameObject = gameObjects[i];
					if (!gameObject || !gameObject.HasNoComponentAndNoChild())
						continue;

					var skip = false;
					var skipReason = "";

					// Check if the object referenced in any of the components in active scene.
					if (allReferencedObjects.Contains(gameObject))
					{
						skip = true;
						skipReason = "Referenced";
					}
					// TODO: See if this is the best way to handle Nested Prefabs. See how much empty objects are passing through for this reason.
					// Check if the object is part of a prefab. It's okay to delete if the object
					// is the prefab's root. But not okay if the object is deep inside a prefab.
					// This operation requires breaking the prefab.
					else if (PrefabUtility.IsPartOfAnyPrefab(gameObject) && !PrefabUtility.IsAnyPrefabInstanceRoot(gameObject))
					{
						skip = true;
						skipReason = "Prefab";
					}

					if (!skip)
					{
						destroyedCount++;
						if (log)
							deletedObjectsText.AppendLine(gameObject.FullName());
						if (undoable)
							Undo.DestroyObjectImmediate(gameObject);
						else
							Object.DestroyImmediate(gameObject);

						// Maybe the parent too turns into an empty object after deleting it's child. Easiest way to find out is restart the whole search.
						needsReRun = true;
					}
					else
					{
						skippedCount++;
						if (log)
							skippedObjectsText.AppendLine($"({skipReason}) " + gameObject.FullName());
					}

					// We won't need to process this gameObject anymore. Remove it from list.
					// Or better, remove only the reference so no heavy list operations will be needed.
					gameObjects[i] = null;
				}
			}
			while (needsReRun);

			if (log)
				Log.Info(
					$"<b>Destroyed {destroyedCount.ToStringWithEnglishPluralPostfix("empty object", '\'')} in scene '{scene.name}':</b>\n" +
					$"{deletedObjectsText}\n\n" +
					$"<b>Skipped {skippedCount.ToStringWithEnglishPluralPostfix("empty object", '\'')}:</b>\n" +
					$"{skippedObjectsText}");
		}

		#endregion

		#region Destroy All GameObjects Containing Component

		public static void DestroyAllGameObjectsContainingComponentInScenes<T>(ActiveCheck activeCheck, SceneListFilter sceneListFilter, bool undoable, bool log) where T : Component
		{
			SceneManagerTools.GetScenes(sceneListFilter)
			                 .ForEach(scene => scene.DestroyAllGameObjectsContainingComponent<T>(activeCheck, undoable, log));
		}

		public static void DestroyAllGameObjectsContainingComponent<T>(this Scene scene, ActiveCheck activeCheck, bool undoable, bool log) where T : Component
		{
			var components = scene.FindObjectsOfType<T>(activeCheck);

			StringBuilder deletedObjectsText = null;
			if (log)
			{
				deletedObjectsText = new StringBuilder();
			}

			int count = 0;
			foreach (var component in components)
			{
				if (!component)
					continue;

				count++;
				if (log)
					deletedObjectsText.AppendLine(component.FullGameObjectName());
				if (undoable)
					Undo.DestroyObjectImmediate(component.gameObject);
				else
					Object.DestroyImmediate(component.gameObject);
			}

			if (log)
				Log.Info($"<b>Destroyed {count.ToStringWithEnglishPluralPostfix("object", '\'')} containing component '{typeof(T).Name}' in scene '{scene.name}':</b>\n{deletedObjectsText}");
		}

		#endregion

		#region Destroy All Static GameObjects Containing Component

		public static void DestroyAllStaticGameObjectsContainingComponentInScenes<T>(StaticEditorFlags leastExpectedFlags, ActiveCheck activeCheck, SceneListFilter sceneListFilter, bool undoable, bool log) where T : Component
		{
			SceneManagerTools.GetScenes(sceneListFilter)
			                 .ForEach(scene => scene.DestroyAllStaticGameObjectsContainingComponent<T>(leastExpectedFlags, activeCheck, undoable, log));
		}

		public static void DestroyAllStaticGameObjectsContainingComponent<T>(this Scene scene, StaticEditorFlags leastExpectedFlags, ActiveCheck activeCheck, bool undoable, bool log) where T : Component
		{
			var components = scene.FindStaticObjectsOfType<T>(leastExpectedFlags, activeCheck);

			StringBuilder deletedObjectsText = null;
			if (log)
			{
				deletedObjectsText = new StringBuilder();
			}

			int count = 0;
			foreach (var component in components)
			{
				if (!component)
					continue;

				count++;
				if (log)
					deletedObjectsText.AppendLine(component.FullGameObjectName());
				if (undoable)
					Undo.DestroyObjectImmediate(component.gameObject);
				else
					Object.DestroyImmediate(component.gameObject);
			}

			if (log)
				Log.Info($"<b>Destroyed {count.ToStringWithEnglishPluralPostfix("static object", '\'')} containing component '{typeof(T).Name}' in scene '{scene.name}':</b>\n{deletedObjectsText}");
		}

		#endregion

		#region Destroy All Components

		public static void DestroyAllComponentsInScenes<T>(ActiveCheck activeCheck, SceneListFilter sceneListFilter, bool undoable, bool log) where T : Component
		{
			SceneManagerTools.GetScenes(sceneListFilter)
			                 .ForEach(scene => scene.DestroyAllComponents<T>(activeCheck, undoable, log));
		}

		public static void DestroyAllComponents<T>(this Scene scene, ActiveCheck activeCheck, bool undoable, bool log) where T : Component
		{
			var components = scene.FindObjectsOfType<T>(activeCheck);

			StringBuilder deletedObjectsText = null;
			if (log)
			{
				deletedObjectsText = new StringBuilder();
			}

			int count = 0;
			foreach (var component in components)
			{
				if (!component)
					continue;

				count++;
				if (log)
					deletedObjectsText.AppendLine(component.FullName());
				if (undoable)
					Undo.DestroyObjectImmediate(component);
				else
					Object.DestroyImmediate(component);
			}

			if (log)
				Log.Info($"<b>Destroyed {count.ToStringWithEnglishPluralPostfix("component", '\'')} of type '{typeof(T).Name}' in scene '{scene.name}':</b>\n{deletedObjectsText}");
		}

		#endregion

		#region Destroy All Static Components

		public static void DestroyAllStaticComponentsInScenes<T>(StaticEditorFlags leastExpectedFlags, ActiveCheck activeCheck, SceneListFilter sceneListFilter, bool undoable, bool log) where T : Component
		{
			SceneManagerTools.GetScenes(sceneListFilter)
			                 .ForEach(scene => scene.DestroyAllStaticComponents<T>(leastExpectedFlags, activeCheck, undoable, log));
		}

		public static void DestroyAllStaticComponents<T>(this Scene scene, StaticEditorFlags leastExpectedFlags, ActiveCheck activeCheck, bool undoable, bool log) where T : Component
		{
			var components = scene.FindStaticObjectsOfType<T>(leastExpectedFlags, activeCheck);

			StringBuilder deletedObjectsText = null;
			if (log)
			{
				deletedObjectsText = new StringBuilder();
			}

			int count = 0;
			foreach (var component in components)
			{
				if (!component)
					continue;

				count++;
				if (log)
					deletedObjectsText.AppendLine(component.FullName());
				if (undoable)
					Undo.DestroyObjectImmediate(component);
				else
					Object.DestroyImmediate(component);
			}

			if (log)
				Log.Info($"<b>Destroyed {count.ToStringWithEnglishPluralPostfix("static component", '\'')} of type '{typeof(T).Name}' in scene '{scene.name}':</b>\n{deletedObjectsText}");
		}

		#endregion

		#region Destroy All Disabled Static MeshRenderers And MeshFilters

		public static void DestroyAllStaticMeshRenderersAndMeshFiltersInScenes(ActiveCheck activeCheck, SceneListFilter sceneListFilter, bool undoable, bool log)
		{
			SceneManagerTools.GetScenes(sceneListFilter)
			                 .ForEach(scene => scene.DestroyAllStaticMeshRenderersAndMeshFilters(activeCheck, undoable, log));
		}

		public static void DestroyAllStaticMeshRenderersAndMeshFilters(this Scene scene, ActiveCheck activeCheck, bool undoable, bool log)
		{
			if (!scene.IsValid())
				throw new Exception("Scene is not valid.");
			if (!scene.isLoaded)
				throw new Exception("Scene is not loaded.");

			var meshRenderers = scene.FindStaticObjectsOfType<MeshRenderer>(StaticEditorFlags.BatchingStatic, activeCheck);

			StringBuilder deletedObjectsText = null;
			if (log)
			{
				deletedObjectsText = new StringBuilder();
			}

			int meshRendererCount = 0;
			int meshFilterCount = 0;
			foreach (var meshRenderer in meshRenderers)
			{
				if (!meshRenderer)
					continue;

				var meshFilter = meshRenderer.GetComponent<MeshFilter>();

				// Make sure mesh filter is not used elsewhere.
				//var deleteMeshFilterToo = meshFilter && meshFilter.IsNotReferencedFromAnySerializedField(); // TODO: Implement this
				var deleteMeshFilterToo = meshFilter;

				meshRendererCount++;
				if (deleteMeshFilterToo)
					meshFilterCount++;

				if (log)
					deletedObjectsText.AppendLine(meshRenderer.FullGameObjectName());
				if (undoable)
				{
					Undo.DestroyObjectImmediate(meshRenderer);
					if (deleteMeshFilterToo)
						Undo.DestroyObjectImmediate(meshFilter);
				}
				else
				{
					Object.DestroyImmediate(meshRenderer);
					if (deleteMeshFilterToo)
						Object.DestroyImmediate(meshFilter);
				}
			}

			if (log)
				Log.Info($"<b>Destroyed {meshRendererCount.ToStringWithEnglishPluralPostfix("disabled static MeshRenderer", '\'')} and {meshFilterCount.ToStringWithEnglishPluralPostfix("disabled static MeshFilter", '\'')} in scene '{scene.name}':</b>\n{deletedObjectsText}");
		}

		#endregion

		#region Sort Children

		public static void SortChildren(this Transform transform, bool inverse)
		{
			var children = transform.GetChildren().Select(child => (child, child.name)).ToList();

			QuicksortAscending(children, 0, children.Count - 1);

			if (inverse)
			{
				for (int i = children.Count - 1; i >= 0; --i)
				{
					Undo.SetTransformParent(children[i].Item1, transform, "Sort children"); // Not the best way to record undo, but could not find any other way that records the order of objects.
					children[i].Item1.SetAsLastSibling();
				}
			}
			else
			{
				for (int i = 0; i < children.Count; ++i)
				{
					Undo.SetTransformParent(children[i].Item1, transform, "Sort children"); // Not the best way to record undo, but could not find any other way that records the order of objects.
					children[i].Item1.SetAsLastSibling();
				}
			}
		}

		public static void QuicksortAscending(List<(Transform transform, string name)> transformsAndNames, int left, int right)
		{
			var i = left;
			var j = right;
			var pivotName = transformsAndNames[(left + right) / 2].name;

			while (i <= j)
			{
				while (string.CompareOrdinal(transformsAndNames[i].name, pivotName) < 0)
				{
					i++;
				}

				while (string.CompareOrdinal(transformsAndNames[j].name, pivotName) > 0)
				{
					j--;
				}

				if (i <= j)
				{
					// Swap
					var temp = transformsAndNames[i];
					transformsAndNames[i] = transformsAndNames[j];
					transformsAndNames[j] = temp;

					i++;
					j--;
				}
			}

			// Recursive calls
			if (left < j)
			{
				QuicksortAscending(transformsAndNames, left, j);
			}

			if (i < right)
			{
				QuicksortAscending(transformsAndNames, i, right);
			}
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(EditorGameObjectTools));

		#endregion
	}

}
