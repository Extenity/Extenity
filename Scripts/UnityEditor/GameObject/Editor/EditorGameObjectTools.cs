using System;
using System.Linq;
using System.Text;
using Extenity.DataToolbox;
using Extenity.ReflectionToolbox;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Extenity.GameObjectToolbox.Editor
{

	public static class EditorGameObjectTools
	{
		// Understanding the GameObject menu
		//[MenuItem("GameObject/XXXXXX_-100/AAA", priority = -100)] public static void XXXXX_minus_100() { }
		//[MenuItem("GameObject/XXXXXX_-50/AAA", priority = -50)] public static void XXXXX_minus_50() { }
		//[MenuItem("GameObject/XXXXXX_-10/AAA", priority = -10)] public static void XXXXX_minus_10() { }
		//[MenuItem("GameObject/XXXXXX_-2/AAA", priority = -2)] public static void XXXXX_minus_2() { }
		//[MenuItem("GameObject/XXXXXX_-1/AAA", priority = -1)] public static void XXXXX_minus_1() { }
		//[MenuItem("GameObject/XXXXXX_0/AAA", priority = 0)] public static void XXXXX_0() { }
		//[MenuItem("GameObject/XXXXXX_1/AAA", priority = 1)] public static void XXXXX_1() { }
		//[MenuItem("GameObject/XXXXXX_2/AAA", priority = 2)] public static void XXXXX_2() { }
		//[MenuItem("GameObject/XXXXXX_3/AAA", priority = 3)] public static void XXXXX_3() { }
		//[MenuItem("GameObject/XXXXXX_4/AAA", priority = 4)] public static void XXXXX_4() { }
		//[MenuItem("GameObject/XXXXXX_5/AAA", priority = 5)] public static void XXXXX_5() { }
		//[MenuItem("GameObject/XXXXXX_6/AAA", priority = 6)] public static void XXXXX_6() { }
		//[MenuItem("GameObject/XXXXXX_7/AAA", priority = 7)] public static void XXXXX_7() { }
		//[MenuItem("GameObject/XXXXXX_8/AAA", priority = 8)] public static void XXXXX_8() { }
		//[MenuItem("GameObject/XXXXXX_9/AAA", priority = 9)] public static void XXXXX_9() { }
		//[MenuItem("GameObject/XXXXXX_10/AAA", priority = 10)] public static void XXXXX_10() { }
		//[MenuItem("GameObject/XXXXXX_11/AAA", priority = 11)] public static void XXXXX_11() { }
		//[MenuItem("GameObject/XXXXXX_12/AAA", priority = 12)] public static void XXXXX_12() { }
		//[MenuItem("GameObject/XXXXXX_13/AAA", priority = 13)] public static void XXXXX_13() { }
		//[MenuItem("GameObject/XXXXXX_50/AAA", priority = 50)] public static void XXXXX_50() { }
		//[MenuItem("GameObject/XXXXXX_100/AAA", priority = 100)] public static void XXXXX_100() { }
		//[MenuItem("GameObject/XXXXXX_1000/AAA", priority = 1000)] public static void XXXXX_1000() { }

		// CAUTION! This method should be at the top of this class, above other MenuItem methods!
		// This is just to give the menu a priority so that sub items may have their own priorities.
		// It's not a good way, though there is no other way.
		[MenuItem("GameObject/Operations/", priority = -53)]
		private static void Menu_Dummy() { }

		#region Hierarchy Menu - Delete Empty Unreferenced GameObjects

		[MenuItem("GameObject/Operations/Delete Empty Unreferenced GameObjects In Active Scene", priority = 0)]
		private static void Menu_DeleteEmptyUnreferencedGameObjectsInActiveScene()
		{
			DeleteEmptyUnreferencedGameObjectsInActiveScene(true, true);
		}

		#endregion

		#region Hierarchy Menu - Delete All Disabled Static MeshRenderers

		[MenuItem("GameObject/Operations/Delete All Disabled Static MeshRenderers In Active Scene", priority = 4)]
		private static void Menu_DeleteAllDisabledStaticMeshRenderersInActiveScene()
		{
			DeleteAllDisabledStaticMeshRenderersInActiveScene(true, true);
		}

		#endregion

		#region Hierarchy Menu - Remove All Colliders

		[MenuItem("GameObject/Operations/Remove Colliders In Selection", priority = 101)]
		public static void RemoveCollidersInSelection()
		{
			foreach (var go in Selection.GetFiltered(typeof(GameObject), SelectionMode.Deep).Cast<GameObject>())
			{
				foreach (var component in go.GetComponents<Collider>())
				{
					Debug.Log("Removing: " + component);
					Undo.DestroyObjectImmediate(component);
				}
			}
		}

		#endregion

		#region Delete Empty Unreferenced GameObjects

		public static void DeleteEmptyUnreferencedGameObjectsInActiveScene(bool undoable, bool log)
		{
			var gameObjects = GameObjectTools.ListAllGameObjectsInActiveScene();
			if (gameObjects.IsNullOrEmpty())
				return;

			var allComponents = GameObjectTools.FindObjectsOfTypeAllInActiveScene<Component>();
			var allObjectFields = allComponents.FindAllReferencedObjectsInComponents();

			StringBuilder deletedObjectsText = null;
			StringBuilder skippedObjectsText = null;
			if (log)
			{
				deletedObjectsText = new StringBuilder();
				skippedObjectsText = new StringBuilder();
			}

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
					if (!allObjectFields.Contains(gameObject))
					{
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
				Debug.Log("Deleting empty objects:\n" + deletedObjectsText.ToString() + (skippedObjectsText.Length == 0 ? "" : "\n\nSkipping empty objects:\n" + skippedObjectsText.ToString()));
		}

		#endregion

		#region Delete All GameObjects Containing Component

		public static void DeleteAllGameObjectsContainingComponentInActiveScene<T>(bool undoable, bool log) where T : Component
		{
			SceneManager.GetActiveScene().DeleteAllGameObjectsContainingComponent<T>(undoable, log);
		}

		public static void DeleteAllGameObjectsContainingComponent<T>(this Scene scene, bool undoable, bool log) where T : Component
		{
			var components = scene.FindObjectsOfTypeAll<T>();

			StringBuilder deletedObjectsText = null;
			if (log)
			{
				deletedObjectsText = new StringBuilder();
			}

			foreach (var component in components)
			{
				if (!component)
					continue;

				if (log)
					deletedObjectsText.AppendLine(component.gameObject.FullName());
				if (undoable)
					Undo.DestroyObjectImmediate(component.gameObject);
				else
					Object.DestroyImmediate(component.gameObject);
			}

			if (log)
				Debug.LogFormat("Deleting objects containing component '{0}':\n{1}", typeof(T).Name, deletedObjectsText.ToString());
		}

		#endregion

		#region Delete All Static MeshRenderers

		public static void DeleteAllDisabledStaticMeshRenderersInActiveScene(bool undoable, bool log)
		{
			SceneManager.GetActiveScene().DeleteAllDisabledStaticMeshRenderers(undoable, log);
		}

		public static void DeleteAllDisabledStaticMeshRenderers(this Scene scene, bool undoable, bool log)
		{
			if (!scene.IsValid())
				throw new Exception("Scene is not valid.");
			if (!scene.isLoaded)
				throw new Exception("Scene is not loaded.");

			var meshRenderers = scene.FindObjectsOfTypeAll<MeshRenderer>();

			StringBuilder deletedObjectsText = null;
			if (log)
			{
				deletedObjectsText = new StringBuilder();
			}

			foreach (var meshRenderer in meshRenderers)
			{
				if (!meshRenderer)
					continue;
				var gameObject = meshRenderer.gameObject;
				if (gameObject.activeInHierarchy)
					continue;
				if (!gameObject.isStatic)
					continue;

				var meshFilter = meshRenderer.GetComponent<MeshFilter>();

				if (log)
					deletedObjectsText.AppendLine(gameObject.FullName());
				if (undoable)
				{
					Undo.DestroyObjectImmediate(meshRenderer);
					if (meshFilter)
						Undo.DestroyObjectImmediate(meshFilter);
				}
				else
				{
					Object.DestroyImmediate(meshRenderer);
					if (meshFilter)
						Object.DestroyImmediate(meshFilter);
				}
			}

			if (log)
				Debug.LogFormat("Deleting disabled static MeshRenderers (and their filters):\n{0}", deletedObjectsText.ToString());
		}

		#endregion
	}

}
