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
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Extenity.GameObjectToolbox.Editor
{

	public static class EditorGameObjectTools
	{
		#region FindStaticObjectsOfTypeAll in Scene

		public static List<T> FindStaticObjectsOfTypeAllInActiveScene<T>(StaticEditorFlags leastExpectedFlags, bool includeInactive) where T : Component
		{
			return SceneManager.GetActiveScene().FindStaticObjectsOfTypeAll<T>(leastExpectedFlags, includeInactive);
		}

		public static List<T> FindStaticObjectsOfTypeAllInLoadedScenes<T>(StaticEditorFlags leastExpectedFlags, bool includeInactive) where T : Component
		{
			return SceneManagerTools.GetLoadedScenes().FindStaticObjectsOfTypeAll<T>(leastExpectedFlags, includeInactive);
		}

		public static List<T> FindStaticObjectsOfTypeAll<T>(this IList<Scene> scenes, StaticEditorFlags leastExpectedFlags, bool includeInactive) where T : Component
		{
			var results = new List<T>();
			for (var i = 0; i < scenes.Count; i++)
			{
				var list = scenes[i].FindStaticObjectsOfTypeAll<T>(leastExpectedFlags, includeInactive);
				results.AddRange(list);
			}

			return results;
		}

		public static List<T> FindStaticObjectsOfTypeAll<T>(this Scene scene, StaticEditorFlags leastExpectedFlags, bool includeInactive) where T : Component
		{
			var temp = new List<T>();
			var results = new List<T>();
			var rootGameObjects = scene.GetRootGameObjects();
			for (int i = 0; i < rootGameObjects.Length; i++)
			{
				rootGameObjects[i].GetComponentsInChildren(includeInactive, temp);
				for (int iChild = 0; iChild < temp.Count; iChild++)
				{
					var child = temp[iChild];
					if (child.gameObject.IsStaticEditorFlagsSetToAtLeast(leastExpectedFlags))
					{
						results.Add(child);
					}
				}
				temp.Clear();
			}
			return results;
		}

		#endregion

		#region SetParentOfAllStaticObjectsContainingComponent

		public static void SetParentOfAllStaticObjectsContainingComponentInActiveScene<T>(Transform parent, bool worldPositionStays, StaticEditorFlags leastExpectedFlags, bool includeInactive) where T : Component
		{
			SceneManager.GetActiveScene().SetParentOfAllStaticObjectsContainingComponent<T>(parent, worldPositionStays, leastExpectedFlags, includeInactive);
		}

		public static void SetParentOfAllStaticObjectsContainingComponentInLoadedScenes<T>(Transform parent, bool worldPositionStays, StaticEditorFlags leastExpectedFlags, bool includeInactive) where T : Component
		{
			SceneManagerTools.GetLoadedScenes(true).ForEach(scene => scene.SetParentOfAllStaticObjectsContainingComponent<T>(parent, worldPositionStays, leastExpectedFlags, includeInactive));
		}

		public static void SetParentOfAllStaticObjectsContainingComponent<T>(this Scene scene, Transform parent, bool worldPositionStays, StaticEditorFlags leastExpectedFlags, bool includeInactive) where T : Component
		{
			var componentTransforms = scene.FindObjectsOfTypeAll<T>(includeInactive)
					.Where(item => item.gameObject.IsStaticEditorFlagsSetToAtLeast(leastExpectedFlags))
					.Select(item => item.transform);

			foreach (var transform in componentTransforms)
			{
				transform.SetParent(parent, worldPositionStays);
				transform.SetAsLastSibling();
			}
		}

		#endregion

		#region MakeSureNoStaticObjectsContainingComponentExist

		public static void MakeSureNoStaticObjectsContainingComponentExistInActiveScene<T>(StaticEditorFlags leastExpectedFlags, bool includeInactive) where T : Component
		{
			SceneManager.GetActiveScene().MakeSureNoStaticObjectsContainingComponentExist<T>(leastExpectedFlags, includeInactive);
		}

		public static void MakeSureNoStaticObjectsContainingComponentExistInLoadedScenes<T>(StaticEditorFlags leastExpectedFlags, bool includeInactive) where T : Component
		{
			SceneManagerTools.GetLoadedScenes(true).ForEach(scene => scene.MakeSureNoStaticObjectsContainingComponentExist<T>(leastExpectedFlags, includeInactive));
		}

		public static void MakeSureNoStaticObjectsContainingComponentExist<T>(this Scene scene, StaticEditorFlags leastExpectedFlags, bool includeInactive) where T : Component
		{
			var components = scene.FindObjectsOfTypeAll<T>(includeInactive)
				.Where(item => item.gameObject.IsStaticEditorFlagsSetToAtLeast(leastExpectedFlags));

			var found = false;
			foreach (var component in components)
			{
				if (component)
				{
					// Do not merge all logs in a single log entry. Log each error in a separate entry that points to the gameobject in log's context for ease of use.
					Debug.LogError($"Static object of component '{typeof(T).Name}' exists in scene '{scene.name}'.", component.gameObject);
					found = true;
				}
			}

			// Fail by throwing an exception.
			if (found)
				throw new Exception($"Scene has static object(s) that contain '{typeof(T).Name}' component. See previous error logs.");
		}

		#endregion

		// TODO: Remove 'Disabled' and change all usages to InactiveOnly
		#region MakeSureNoDisabledStaticObjectsContainingComponentExist

		public static void MakeSureNoDisabledStaticObjectsContainingComponentExistInActiveScene<T>(StaticEditorFlags leastExpectedFlags) where T : Component
		{
			SceneManager.GetActiveScene().MakeSureNoDisabledStaticObjectsContainingComponentExist<T>(leastExpectedFlags);
		}

		public static void MakeSureNoDisabledStaticObjectsContainingComponentExistInLoadedScenes<T>(StaticEditorFlags leastExpectedFlags) where T : Component
		{
			SceneManagerTools.GetLoadedScenes(true).ForEach(scene => scene.MakeSureNoDisabledStaticObjectsContainingComponentExist<T>(leastExpectedFlags));
		}

		public static void MakeSureNoDisabledStaticObjectsContainingComponentExist<T>(this Scene scene, StaticEditorFlags leastExpectedFlags) where T : Component
		{
			var components = scene.FindObjectsOfTypeAll<T>(true) // TODO: OPTIMIZATION: Find a way to get only inactive objects
				.Where(item => item.gameObject.IsStaticEditorFlagsSetToAtLeast(leastExpectedFlags) &&
							   //(!item.gameObject.activeInHierarchy || !item.enabled)); // TODO: Find a way to check if the component is disabled too. Better yet, implement this as a part of FindObjectsOfTypeAll like stated above which will list only disabled objects. Then we can delete this check here altogether.
							   !item.gameObject.activeInHierarchy);

			var found = false;
			foreach (var component in components)
			{
				if (component)
				{
					// Do not merge all logs in a single log entry. Log each error in a separate entry that points to the gameobject in log's context for ease of use.
					Debug.LogError($"Disabled static object of component '{typeof(T).Name}' exists in scene '{scene.name}'.", component.gameObject);
					found = true;
				}
			}

			// Fail by throwing an exception.
			if (found)
				throw new Exception($"Scene has disabled static object(s) that contain '{typeof(T).Name}' component. See previous error logs.");
		}

		#endregion

		#region Destroy Empty Unreferenced GameObjects

		public static void DestroyEmptyUnreferencedGameObjectsInActiveScene(bool includeInactive, bool undoable, bool log)
		{
			SceneManager.GetActiveScene().DestroyEmptyUnreferencedGameObjects(includeInactive, undoable, log);
		}

		public static void DestroyEmptyUnreferencedGameObjectsInLoadedScenes(bool includeInactive, bool undoable, bool log)
		{
			SceneManagerTools.GetLoadedScenes(true).ForEach(scene => scene.DestroyEmptyUnreferencedGameObjects(includeInactive, undoable, log));
		}

		public static void DestroyEmptyUnreferencedGameObjects(this Scene scene, bool includeInactive, bool undoable, bool log)
		{
			var gameObjects = scene.ListAllGameObjectsInScene();
			if (gameObjects.IsNullOrEmpty())
				return;

			var allComponents = scene.FindObjectsOfTypeAll<Component>(includeInactive);
			var allReferencedObjects = new HashSet<GameObject>();
			foreach (var component in allComponents)
			{
				if (component)
				{
					component.FindAllReferencedGameObjectsInComponent(allReferencedObjects);
				}
			}

			// SPECIAL CARE
			{
				const bool includeInactiveForSpecialCare = true;

				// Exclude all child gameobjects of Animators.
				{
					var animators = GameObjectTools.FindObjectsOfTypeAllInActiveScene<Animator>(includeInactiveForSpecialCare);
					if (animators.Count > 0)
					{
						var children = new List<GameObject>();
						foreach (var animator in animators)
						{
							children.Clear();
							animator.gameObject.ListAllChildrenGameObjects(children, true);
							foreach (var child in children)
							{
								allReferencedObjects.Add(child);
							}
						}
					}
				}

				// Exclude referenced gameobjects in OffMeshLinks
				{
					var offMeshLinks = GameObjectTools.FindObjectsOfTypeAllInActiveScene<OffMeshLink>(includeInactiveForSpecialCare);
					foreach (var offMeshLink in offMeshLinks)
					{
						var linked = offMeshLink.startTransform;
						if (linked)
							allReferencedObjects.Add(linked.gameObject);
						linked = offMeshLink.endTransform;
						if (linked)
							allReferencedObjects.Add(linked.gameObject);
					}
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
					if (!gameObject || !gameObject.IsEmpty())
						continue;

					// Check if the object referenced in any of the components in active scene
					if (!allReferencedObjects.Contains(gameObject))
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
							skippedObjectsText.AppendLine(gameObject.FullName());
					}

					// We won't need to process this gameObject anymore. Remove it from list.
					// Or better, remove only the reference so no heavy list operations will be needed.
					gameObjects[i] = null;
				}
			}
			while (needsReRun);

			if (log)
				Debug.Log($"<b>Destroyed {destroyedCount.ToStringWithEnglishPluralPostfix("empty object", '\'')} in scene '{scene.name}':</b>\n{deletedObjectsText}\n\n<b>Skipped {skippedCount.ToStringWithEnglishPluralPostfix("empty object", '\'')}:</b>\n");
		}

		#endregion

		#region Destroy All GameObjects Containing Component

		public static void DestroyAllGameObjectsContainingComponentInLoadedScenes<T>(bool includeInactive, bool undoable, bool log) where T : Component
		{
			SceneManagerTools.GetLoadedScenes().ForEach(scene => scene.DestroyAllGameObjectsContainingComponent<T>(includeInactive, undoable, log));
		}

		public static void DestroyAllGameObjectsContainingComponentInActiveScene<T>(bool includeInactive, bool undoable, bool log) where T : Component
		{
			SceneManager.GetActiveScene().DestroyAllGameObjectsContainingComponent<T>(includeInactive, undoable, log);
		}

		public static void DestroyAllGameObjectsContainingComponent<T>(this Scene scene, bool includeInactive, bool undoable, bool log) where T : Component
		{
			var components = scene.FindObjectsOfTypeAll<T>(includeInactive);

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
					deletedObjectsText.AppendLine(component.gameObject.FullName());
				if (undoable)
					Undo.DestroyObjectImmediate(component.gameObject);
				else
					Object.DestroyImmediate(component.gameObject);
			}

			if (log)
				Debug.Log($"<b>Destroyed {count.ToStringWithEnglishPluralPostfix("object", '\'')} containing component '{typeof(T).Name}' in scene '{scene.name}':</b>\n{deletedObjectsText}");
		}

		#endregion

		#region Destroy All Static GameObjects Containing Component

		public static void DestroyAllStaticGameObjectsContainingComponentInLoadedScenes<T>(StaticEditorFlags leastExpectedFlags, bool includeInactive, bool undoable, bool log) where T : Component
		{
			SceneManagerTools.GetLoadedScenes().ForEach(scene => scene.DestroyAllStaticGameObjectsContainingComponent<T>(leastExpectedFlags, includeInactive, undoable, log));
		}

		public static void DestroyAllStaticGameObjectsContainingComponentInActiveScene<T>(StaticEditorFlags leastExpectedFlags, bool includeInactive, bool undoable, bool log) where T : Component
		{
			SceneManager.GetActiveScene().DestroyAllStaticGameObjectsContainingComponent<T>(leastExpectedFlags, includeInactive, undoable, log);
		}

		public static void DestroyAllStaticGameObjectsContainingComponent<T>(this Scene scene, StaticEditorFlags leastExpectedFlags, bool includeInactive, bool undoable, bool log) where T : Component
		{
			var components = scene.FindStaticObjectsOfTypeAll<T>(leastExpectedFlags, includeInactive);

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
					deletedObjectsText.AppendLine(component.gameObject.FullName());
				if (undoable)
					Undo.DestroyObjectImmediate(component.gameObject);
				else
					Object.DestroyImmediate(component.gameObject);
			}

			if (log)
				Debug.Log($"<b>Destroyed {count.ToStringWithEnglishPluralPostfix("static object", '\'')} containing component '{typeof(T).Name}' in scene '{scene.name}':</b>\n{deletedObjectsText}");
		}

		#endregion

		#region Destroy All Components

		public static void DestroyAllComponentsInLoadedScenes<T>(bool includeInactive, bool undoable, bool log) where T : Component
		{
			SceneManagerTools.GetLoadedScenes().ForEach(scene => scene.DestroyAllComponents<T>(includeInactive, undoable, log));
		}

		public static void DestroyAllComponentsInActiveScene<T>(bool includeInactive, bool undoable, bool log) where T : Component
		{
			SceneManager.GetActiveScene().DestroyAllComponents<T>(includeInactive, undoable, log);
		}

		public static void DestroyAllComponents<T>(this Scene scene, bool includeInactive, bool undoable, bool log) where T : Component
		{
			var components = scene.FindObjectsOfTypeAll<T>(includeInactive);

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
				Debug.Log($"<b>Destroyed {count.ToStringWithEnglishPluralPostfix("component", '\'')} of type '{typeof(T).Name}' in scene '{scene.name}':</b>\n{deletedObjectsText}");
		}

		#endregion

		#region Destroy All Static Components

		public static void DestroyAllStaticComponentsInLoadedScenes<T>(StaticEditorFlags leastExpectedFlags, bool includeInactive, bool undoable, bool log) where T : Component
		{
			SceneManagerTools.GetLoadedScenes().ForEach(scene => scene.DestroyAllStaticComponents<T>(leastExpectedFlags, includeInactive, undoable, log));
		}

		public static void DestroyAllStaticComponentsInActiveScene<T>(StaticEditorFlags leastExpectedFlags, bool includeInactive, bool undoable, bool log) where T : Component
		{
			SceneManager.GetActiveScene().DestroyAllStaticComponents<T>(leastExpectedFlags, includeInactive, undoable, log);
		}

		public static void DestroyAllStaticComponents<T>(this Scene scene, StaticEditorFlags leastExpectedFlags, bool includeInactive, bool undoable, bool log) where T : Component
		{
			var components = scene.FindStaticObjectsOfTypeAll<T>(leastExpectedFlags, includeInactive);

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
				Debug.Log($"<b>Destroyed {count.ToStringWithEnglishPluralPostfix("static component", '\'')} of type '{typeof(T).Name}' in scene '{scene.name}':</b>\n{deletedObjectsText}");
		}

		#endregion

		// TODO: Remove 'Disabled' and change all usages to InactiveOnly
		#region Destroy All Disabled Static Components

		public static void DestroyAllDisabledStaticComponentsInLoadedScenes<T>(StaticEditorFlags leastExpectedFlags, bool undoable, bool log) where T : Component
		{
			SceneManagerTools.GetLoadedScenes().ForEach(scene => scene.DestroyAllDisabledStaticComponents<T>(leastExpectedFlags, undoable, log));
		}

		public static void DestroyAllDisabledStaticComponentsInActiveScene<T>(StaticEditorFlags leastExpectedFlags, bool undoable, bool log) where T : Component
		{
			SceneManager.GetActiveScene().DestroyAllDisabledStaticComponents<T>(leastExpectedFlags, undoable, log);
		}

		public static void DestroyAllDisabledStaticComponents<T>(this Scene scene, StaticEditorFlags leastExpectedFlags, bool undoable, bool log) where T : Component
		{
			var components = scene.FindStaticObjectsOfTypeAll<T>(leastExpectedFlags, true); // TODO: Find a way to only get disabled objects here

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
				// TODO: Find a way to check if the component is disabled too. Better yet, implement this as a part of FindStaticObjectsOfTypeAll like stated above which will list only disabled objects. Then we can delete this check here altogether.
				//if (component.gameObject.activeInHierarchy && component.enabled)
				if (component.gameObject.activeInHierarchy)
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
				Debug.Log($"<b>Destroyed {count.ToStringWithEnglishPluralPostfix("disabled static components", '\'')} of type '{typeof(T).Name}' in scene '{scene.name}':</b>\n{deletedObjectsText}");
		}

		#endregion

		// TODO: Remove 'Disabled' and change all usages to InactiveOnly
		#region Destroy All Disabled Static MeshRenderers And MeshFilters

		public static void DestroyAllDisabledStaticMeshRenderersAndMeshFiltersInLoadedScenes(bool undoable, bool log)
		{
			SceneManager.GetActiveScene().DestroyAllDisabledStaticMeshRenderersAndMeshFilters(undoable, log);
		}

		public static void DestroyAllDisabledStaticMeshRenderersAndMeshFiltersInActiveScene(bool undoable, bool log)
		{
			SceneManagerTools.GetLoadedScenes(true).ForEach(scene => scene.DestroyAllDisabledStaticMeshRenderersAndMeshFilters(undoable, log));
		}

		public static void DestroyAllDisabledStaticMeshRenderersAndMeshFilters(this Scene scene, bool undoable, bool log)
		{
			if (!scene.IsValid())
				throw new Exception("Scene is not valid.");
			if (!scene.isLoaded)
				throw new Exception("Scene is not loaded.");

			var meshRenderers = scene.FindStaticObjectsOfTypeAll<MeshRenderer>(StaticEditorFlags.BatchingStatic, true); // TODO: Find a way to only get disabled objects here

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
				// TODO: Implement this 'disabled' check as a part of FindStaticObjectsOfTypeAll like stated above which will list only disabled objects. Then we can delete this check here altogether.
				var gameObject = meshRenderer.gameObject;
				if (gameObject.activeInHierarchy && meshRenderer.enabled)
					continue;

				var meshFilter = meshRenderer.GetComponent<MeshFilter>();

				// Make sure mesh filter is not used elsewhere.
				//var deleteMeshFilterToo = meshFilter && meshFilter.IsNotReferencedFromAnySerializedField(); // TODO: Implement this
				var deleteMeshFilterToo = meshFilter;

				meshRendererCount++;
				if (deleteMeshFilterToo)
					meshFilterCount++;

				if (log)
					deletedObjectsText.AppendLine(gameObject.FullName());
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
				Debug.Log($"<b>Destroyed {meshRendererCount.ToStringWithEnglishPluralPostfix("disabled static MeshRenderer", '\'')} and {meshFilterCount.ToStringWithEnglishPluralPostfix("disabled static MeshFilter", '\'')} in scene '{scene.name}':</b>\n{deletedObjectsText}");
		}

		#endregion
	}

}
