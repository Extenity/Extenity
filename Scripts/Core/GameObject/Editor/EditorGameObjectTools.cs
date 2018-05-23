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

		public static List<T> FindStaticObjectsOfTypeInActiveScene<T>(StaticEditorFlags leastExpectedFlags, ActiveCheck activeCheck) where T : Component
		{
			return SceneManager.GetActiveScene().FindStaticObjectsOfType<T>(leastExpectedFlags, activeCheck);
		}

		public static List<T> FindStaticObjectsOfTypeInLoadedScenes<T>(StaticEditorFlags leastExpectedFlags, ActiveCheck activeCheck) where T : Component
		{
			return SceneManagerTools.GetLoadedScenes().FindStaticObjectsOfType<T>(leastExpectedFlags, activeCheck);
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

		#region SetParentOfAllStaticObjectsContainingComponent

		public static void SetParentOfAllStaticObjectsContainingComponentInActiveScene<T>(Transform parent, bool worldPositionStays, StaticEditorFlags leastExpectedFlags, ActiveCheck activeCheck) where T : Component
		{
			SceneManager.GetActiveScene().SetParentOfAllStaticObjectsContainingComponent<T>(parent, worldPositionStays, leastExpectedFlags, activeCheck);
		}

		public static void SetParentOfAllStaticObjectsContainingComponentInLoadedScenes<T>(Transform parent, bool worldPositionStays, StaticEditorFlags leastExpectedFlags, ActiveCheck activeCheck) where T : Component
		{
			SceneManagerTools.GetLoadedScenes(true).ForEach(scene => scene.SetParentOfAllStaticObjectsContainingComponent<T>(parent, worldPositionStays, leastExpectedFlags, activeCheck));
		}

		public static void SetParentOfAllStaticObjectsContainingComponent<T>(this Scene scene, Transform parent, bool worldPositionStays, StaticEditorFlags leastExpectedFlags, ActiveCheck activeCheck) where T : Component
		{
			var componentTransforms = scene.FindObjectsOfType<T>(activeCheck)
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

		public static void MakeSureNoStaticObjectsContainingComponentExistInActiveScene<T>(StaticEditorFlags leastExpectedFlags, ActiveCheck activeCheck) where T : Component
		{
			SceneManager.GetActiveScene().MakeSureNoStaticObjectsContainingComponentExist<T>(leastExpectedFlags, activeCheck);
		}

		public static void MakeSureNoStaticObjectsContainingComponentExistInLoadedScenes<T>(StaticEditorFlags leastExpectedFlags, ActiveCheck activeCheck) where T : Component
		{
			SceneManagerTools.GetLoadedScenes(true).ForEach(scene => scene.MakeSureNoStaticObjectsContainingComponentExist<T>(leastExpectedFlags, activeCheck));
		}

		public static void MakeSureNoStaticObjectsContainingComponentExist<T>(this Scene scene, StaticEditorFlags leastExpectedFlags, ActiveCheck activeCheck) where T : Component
		{
			var components = scene.FindObjectsOfType<T>(activeCheck)
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

		#region Destroy Empty Unreferenced GameObjects

		public static void DestroyEmptyUnreferencedGameObjectsInActiveScene(bool undoable, bool log)
		{
			SceneManager.GetActiveScene().DestroyEmptyUnreferencedGameObjects(undoable, log);
		}

		public static void DestroyEmptyUnreferencedGameObjectsInLoadedScenes(bool undoable, bool log)
		{
			SceneManagerTools.GetLoadedScenes(true).ForEach(scene => scene.DestroyEmptyUnreferencedGameObjects(undoable, log));
		}

		public static void DestroyEmptyUnreferencedGameObjects(this Scene scene, bool undoable, bool log)
		{
			var gameObjects = scene.ListAllGameObjectsInScene();
			if (gameObjects.IsNullOrEmpty())
				return;

			var allComponents = scene.FindObjectsOfType<Component>(ActiveCheck.IncludingInactive);
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
				const ActiveCheck includeInactiveForSpecialCare = ActiveCheck.IncludingInactive;

				// Exclude all child gameobjects of Animators.
				{
					var animators = GameObjectTools.FindObjectsOfTypeInActiveScene<Animator>(includeInactiveForSpecialCare);
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
					var offMeshLinks = GameObjectTools.FindObjectsOfTypeInActiveScene<OffMeshLink>(includeInactiveForSpecialCare);
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

		public static void DestroyAllGameObjectsContainingComponentInLoadedScenes<T>(ActiveCheck activeCheck, bool undoable, bool log) where T : Component
		{
			SceneManagerTools.GetLoadedScenes().ForEach(scene => scene.DestroyAllGameObjectsContainingComponent<T>(activeCheck, undoable, log));
		}

		public static void DestroyAllGameObjectsContainingComponentInActiveScene<T>(ActiveCheck activeCheck, bool undoable, bool log) where T : Component
		{
			SceneManager.GetActiveScene().DestroyAllGameObjectsContainingComponent<T>(activeCheck, undoable, log);
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

		public static void DestroyAllStaticGameObjectsContainingComponentInLoadedScenes<T>(StaticEditorFlags leastExpectedFlags, ActiveCheck activeCheck, bool undoable, bool log) where T : Component
		{
			SceneManagerTools.GetLoadedScenes().ForEach(scene => scene.DestroyAllStaticGameObjectsContainingComponent<T>(leastExpectedFlags, activeCheck, undoable, log));
		}

		public static void DestroyAllStaticGameObjectsContainingComponentInActiveScene<T>(StaticEditorFlags leastExpectedFlags, ActiveCheck activeCheck, bool undoable, bool log) where T : Component
		{
			SceneManager.GetActiveScene().DestroyAllStaticGameObjectsContainingComponent<T>(leastExpectedFlags, activeCheck, undoable, log);
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

		public static void DestroyAllComponentsInLoadedScenes<T>(ActiveCheck activeCheck, bool undoable, bool log) where T : Component
		{
			SceneManagerTools.GetLoadedScenes().ForEach(scene => scene.DestroyAllComponents<T>(activeCheck, undoable, log));
		}

		public static void DestroyAllComponentsInActiveScene<T>(ActiveCheck activeCheck, bool undoable, bool log) where T : Component
		{
			SceneManager.GetActiveScene().DestroyAllComponents<T>(activeCheck, undoable, log);
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
				Debug.Log($"<b>Destroyed {count.ToStringWithEnglishPluralPostfix("component", '\'')} of type '{typeof(T).Name}' in scene '{scene.name}':</b>\n{deletedObjectsText}");
		}

		#endregion

		#region Destroy All Static Components

		public static void DestroyAllStaticComponentsInLoadedScenes<T>(StaticEditorFlags leastExpectedFlags, ActiveCheck activeCheck, bool undoable, bool log) where T : Component
		{
			SceneManagerTools.GetLoadedScenes().ForEach(scene => scene.DestroyAllStaticComponents<T>(leastExpectedFlags, activeCheck, undoable, log));
		}

		public static void DestroyAllStaticComponentsInActiveScene<T>(StaticEditorFlags leastExpectedFlags, ActiveCheck activeCheck, bool undoable, bool log) where T : Component
		{
			SceneManager.GetActiveScene().DestroyAllStaticComponents<T>(leastExpectedFlags, activeCheck, undoable, log);
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
				Debug.Log($"<b>Destroyed {count.ToStringWithEnglishPluralPostfix("static component", '\'')} of type '{typeof(T).Name}' in scene '{scene.name}':</b>\n{deletedObjectsText}");
		}

		#endregion

		#region Destroy All Disabled Static MeshRenderers And MeshFilters

		public static void DestroyAllStaticMeshRenderersAndMeshFiltersInLoadedScenes(ActiveCheck activeCheck, bool undoable, bool log)
		{
			SceneManager.GetActiveScene().DestroyAllStaticMeshRenderersAndMeshFilters(activeCheck, undoable, log);
		}

		public static void DestroyAllStaticMeshRenderersAndMeshFiltersInActiveScene(ActiveCheck activeCheck, bool undoable, bool log)
		{
			SceneManagerTools.GetLoadedScenes(true).ForEach(scene => scene.DestroyAllStaticMeshRenderersAndMeshFilters(activeCheck, undoable, log));
		}

		public static void DestroyAllStaticMeshRenderersAndMeshFilters(this Scene scene, ActiveCheck activeCheck, bool undoable, bool log)
		{
			if (!scene.IsValid())
				throw new Exception("Scene is not valid.");
			if (!scene.isLoaded)
				throw new Exception("Scene is not loaded.");

			var meshRenderers = scene.FindStaticObjectsOfType<MeshRenderer>(StaticEditorFlags.BatchingStatic, ActiveCheck.InactiveOnly);

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
					deletedObjectsText.AppendLine(meshRenderer.gameObject.FullName());
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
