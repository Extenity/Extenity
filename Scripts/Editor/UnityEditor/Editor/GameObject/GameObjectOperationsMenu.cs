using System.Linq;
using Extenity.AssetToolbox.Editor;
using Extenity.DataToolbox;
using Extenity.SceneManagementToolbox;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Extenity.GameObjectToolbox.Editor
{

	public static class GameObjectOperationsMenu
	{
		// CAUTION! This method should be at the top of this class, above other MenuItem methods!
		// This is just to give the menu a priority so that sub items may have their own priorities.
		// It's not a good way, though there is no other way.
		[MenuItem("GameObject/Operations/", priority = -53)]
		private static void Menu_Dummy() { }

		#region Hierarchy Menu - Delete Empty Unreferenced GameObjects

		[MenuItem("GameObject/Operations/Delete Empty Unreferenced GameObjects/In Loaded Scenes", priority = 0)]
		private static void Menu_DestroyEmptyUnreferencedGameObjects_InLoadedScenes()
		{
			EditorGameObjectTools.DestroyEmptyUnreferencedGameObjectsInLoadedScenes(new[] { typeof(Instantiator) }, true, true, true, true);
		}

		[MenuItem("GameObject/Operations/Delete Empty Unreferenced GameObjects/In Active Scene", priority = 1)]
		private static void Menu_DestroyEmptyUnreferencedGameObjects_InActiveScene()
		{
			EditorGameObjectTools.DestroyEmptyUnreferencedGameObjectsInActiveScene(new[] { typeof(Instantiator) }, true, true);
		}

		#endregion

		#region Hierarchy Menu - Delete All Disabled Static MeshRenderers

		[MenuItem("GameObject/Operations/Delete All Disabled Static MeshRenderers/In Loaded Scenes", priority = 4)]
		private static void Menu_DeleteAllDisabledStaticMeshRenderers_InLoadedScenes()
		{
			EditorGameObjectTools.DestroyAllStaticMeshRenderersAndMeshFiltersInLoadedScenes(ActiveCheck.InactiveOnly, true, true, true, true);
		}

		[MenuItem("GameObject/Operations/Delete All Disabled Static MeshRenderers/In Active Scene", priority = 5)]
		private static void Menu_DeleteAllDisabledStaticMeshRenderers_InActiveScene()
		{
			EditorGameObjectTools.DestroyAllStaticMeshRenderersAndMeshFiltersInActiveScene(ActiveCheck.InactiveOnly, true, true);
		}

		#endregion

		#region Hierarchy Menu - Remove All Colliders

		[MenuItem("GameObject/Operations/Remove Colliders/In Selection", priority = 101)]
		public static void RemoveColliders_InSelection()
		{
			foreach (var go in Selection.GetFiltered(typeof(GameObject), SelectionMode.Deep).Cast<GameObject>().OrderBy(item => item.FullName()))
			{
				foreach (var component in go.GetComponents<Collider>())
				{
					Log.Info($"Removing '{component.FullName()}'.", component.gameObject);
					Undo.DestroyObjectImmediate(component);
				}
			}
		}

		#endregion

		#region Hierarchy Menu - Remove Numbered Parentheses Postfix

		[MenuItem("GameObject/Operations/Remove Numbered Parentheses Postfix/In Selected Object Names", priority = 2001)]
		public static void RemoveNumberedParenthesesPostfix_InSelectedObjectNames()
		{
			foreach (var go in Selection.GetFiltered(typeof(GameObject), SelectionMode.Editable).Cast<GameObject>().OrderBy(item => item.FullName()))
			{
				var newName = go.name.RemoveEndingNumberedParentheses();
				if (newName != go.name)
				{
					Log.Info($"Renaming '{go.FullName()}' to '{newName}'.", go);
					Undo.RecordObject(go, "Remove Numbered Parentheses Postfix");
					go.name = newName;
				}
			}
		}

		[MenuItem("GameObject/Operations/Remove Numbered Parentheses Postfix/In Selected Object Names (With Children)", priority = 2002)]
		public static void RemoveNumberedParenthesesPostfix_InSelectedObjectNamesWithChildren()
		{
			foreach (var go in Selection.GetFiltered(typeof(GameObject), SelectionMode.Editable | SelectionMode.Deep).Cast<GameObject>().OrderBy(item => item.FullName()))
			{
				var newName = go.name.RemoveEndingNumberedParentheses();
				if (newName != go.name)
				{
					Log.Info($"Renaming '{go.FullName()}' to '{newName}'.", go);
					Undo.RecordObject(go, "Remove Numbered Parentheses Postfix");
					go.name = newName;
				}
			}
		}

		#endregion

		#region Hierarchy Menu - List All Referenced Sprites

		[MenuItem("GameObject/Operations/List All Referenced Sprites/In Loaded Scenes", priority = 4001)]
		private static void Menu_ListAllReferencedSprites_InLoadedScenes()
		{
			SceneManagerTools.GetRootGameObjectsOfLoadedScenes(true, true).LogListReferencedAssetsOfTypeInGameObjects(true, typeof(Sprite));
		}

		[MenuItem("GameObject/Operations/List All Referenced Sprites/In Active Scene", priority = 4002)]
		private static void Menu_ListAllReferencedSprites_InActiveScene()
		{
			SceneManager.GetActiveScene().GetRootGameObjects().LogListReferencedAssetsOfTypeInGameObjects(true, typeof(Sprite));
		}

		[MenuItem("GameObject/Operations/List All Referenced Sprites/In Selection", priority = 4003)]
		private static void Menu_ListAllReferencedSprites_InSelection()
		{
			Selection.gameObjects.LogListReferencedAssetsOfTypeInGameObjects(true, typeof(Sprite));
		}

		[MenuItem("GameObject/Operations/List All Referenced Sprites/In Selection And Children", priority = 4004)]
		private static void Menu_ListAllReferencedSprites_InSelectionAndChildren()
		{
			Selection.gameObjects.LogListReferencedAssetsOfTypeInGameObjects(true, typeof(Sprite));
		}

		#endregion

		#region Hierarchy Menu - Sort Children By Name

		[MenuItem("GameObject/Operations/Sort Children By Name/Ascending", priority = 7001)]
		public static void SortChildrenByName_Ascending()
		{
			var gameObjects = Selection.GetFiltered(typeof(GameObject), SelectionMode.Editable).Cast<GameObject>().ToList();
			for (var i = 0; i < gameObjects.Count; i++)
			{
				gameObjects[i].SortChildren(false);
			}
		}

		[MenuItem("GameObject/Operations/Sort Children By Name/Descending", priority = 7002)]
		public static void SortChildrenByName_Descending()
		{
			var gameObjects = Selection.GetFiltered(typeof(GameObject), SelectionMode.Editable).Cast<GameObject>().ToList();
			for (var i = 0; i < gameObjects.Count; i++)
			{
				gameObjects[i].SortChildren(true);
			}
		}

		#endregion
	}

}
