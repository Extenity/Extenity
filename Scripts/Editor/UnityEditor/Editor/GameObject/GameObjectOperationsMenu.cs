using System;
using System.Linq;
using Extenity.AssetToolbox.Editor;
using Extenity.DataToolbox;
using Extenity.SceneManagementToolbox;
using Extenity.UnityEditorToolbox;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Extenity.GameObjectToolbox.Editor
{

	public static class GameObjectOperationsMenu
	{
		private const string Menu = ExtenityMenu.GameObjectOperations;

		#region Hierarchy Menu - Delete Empty Unreferenced GameObjects

		[MenuItem(Menu + "Delete Empty Unreferenced GameObjects/In Loaded Scenes", priority = ExtenityMenu.GameObjectOperationsPriority + 0)]
		private static void Menu_DestroyEmptyUnreferencedGameObjects_InLoadedScenes()
		{
			var excludedTypes = new Type[]
			{
#if ExtenityInstantiator
				typeof(Instantiator),
#endif
			};
			EditorGameObjectTools.DestroyEmptyUnreferencedGameObjectsInScenes(excludedTypes, SceneListFilter.LoadedScenesAndDontDestroyOnLoadScene, true, true);
		}

		[MenuItem(Menu + "Delete Empty Unreferenced GameObjects/In Active Scene", priority = ExtenityMenu.GameObjectOperationsPriority + 1)]
		private static void Menu_DestroyEmptyUnreferencedGameObjects_InActiveScene()
		{
			var excludedTypes = new Type[]
			{
#if ExtenityInstantiator
				typeof(Instantiator),
#endif
			};
			EditorGameObjectTools.DestroyEmptyUnreferencedGameObjectsInScenes(excludedTypes, SceneListFilter.LoadedActiveScene, true, true);
		}

		#endregion

		#region Hierarchy Menu - Delete All Disabled Static MeshRenderers

		[MenuItem(Menu + "Delete All Disabled Static MeshRenderers/In Loaded Scenes", priority = ExtenityMenu.GameObjectOperationsPriority + 21)]
		private static void Menu_DeleteAllDisabledStaticMeshRenderers_InLoadedScenes()
		{
			EditorGameObjectTools.DestroyAllStaticMeshRenderersAndMeshFiltersInScenes(ActiveCheck.InactiveOnly, SceneListFilter.LoadedScenesAndDontDestroyOnLoadScene, true, true);
		}

		[MenuItem(Menu + "Delete All Disabled Static MeshRenderers/In Active Scene", priority = ExtenityMenu.GameObjectOperationsPriority + 22)]
		private static void Menu_DeleteAllDisabledStaticMeshRenderers_InActiveScene()
		{
			EditorGameObjectTools.DestroyAllStaticMeshRenderersAndMeshFiltersInScenes(ActiveCheck.InactiveOnly, SceneListFilter.LoadedActiveScene, true, true);
		}

		#endregion

		#region Hierarchy Menu - Remove All Colliders

		[MenuItem(Menu + "Remove Colliders/In Selection", priority = ExtenityMenu.GameObjectOperationsPriority + 23)]
		public static void RemoveColliders_InSelection()
		{
			foreach (var go in Selection.GetFiltered(typeof(GameObject), SelectionMode.Deep).Cast<GameObject>().OrderBy(item => item.FullName()))
			{
				foreach (var component in go.GetComponents<Collider>())
				{
					Log.InfoWithContext(component.gameObject, $"Removing '{component.FullName()}'.");
					Undo.DestroyObjectImmediate(component);
				}
			}
		}

		#endregion

		#region Hierarchy Menu - Remove Numbered Parentheses Postfix

		[MenuItem(Menu + "Remove Numbered Parentheses Postfix/In Selected Object Names", priority = ExtenityMenu.GameObjectOperationsPriority + 41)]
		public static void RemoveNumberedParenthesesPostfix_InSelectedObjectNames()
		{
			foreach (var go in Selection.GetFiltered(typeof(GameObject), SelectionMode.Editable).Cast<GameObject>().OrderBy(item => item.FullName()))
			{
				var newName = go.name.RemoveEndingNumberedParentheses();
				if (newName != go.name)
				{
					Log.InfoWithContext(go, $"Renaming '{go.FullName()}' to '{newName}'.");
					Undo.RecordObject(go, "Remove Numbered Parentheses Postfix");
					go.name = newName;
				}
			}
		}

		[MenuItem(Menu + "Remove Numbered Parentheses Postfix/In Selected Object Names (With Children)", priority = ExtenityMenu.GameObjectOperationsPriority + 42)]
		public static void RemoveNumberedParenthesesPostfix_InSelectedObjectNamesWithChildren()
		{
			foreach (var go in Selection.GetFiltered(typeof(GameObject), SelectionMode.Editable | SelectionMode.Deep).Cast<GameObject>().OrderBy(item => item.FullName()))
			{
				var newName = go.name.RemoveEndingNumberedParentheses();
				if (newName != go.name)
				{
					Log.InfoWithContext(go, $"Renaming '{go.FullName()}' to '{newName}'.");
					Undo.RecordObject(go, "Remove Numbered Parentheses Postfix");
					go.name = newName;
				}
			}
		}

		#endregion

		#region Hierarchy Menu - List All Referenced Sprites

		[MenuItem(Menu + "List All Referenced Sprites/In Loaded Scenes", priority = ExtenityMenu.GameObjectOperationsPriority + 61)]
		private static void Menu_ListAllReferencedSprites_InLoadedScenes()
		{
			SceneManagerTools.GetRootGameObjectsOfScenes(SceneListFilter.LoadedScenesAndDontDestroyOnLoadScene)
			                 .LogListReferencedAssetsOfTypeInGameObjects(true, typeof(Sprite));
		}

		[MenuItem(Menu + "List All Referenced Sprites/In Active Scene", priority = ExtenityMenu.GameObjectOperationsPriority + 62)]
		private static void Menu_ListAllReferencedSprites_InActiveScene()
		{
			SceneManager.GetActiveScene().GetRootGameObjects().LogListReferencedAssetsOfTypeInGameObjects(true, typeof(Sprite));
		}

		[MenuItem(Menu + "List All Referenced Sprites/In Selection", priority = ExtenityMenu.GameObjectOperationsPriority + 63)]
		private static void Menu_ListAllReferencedSprites_InSelection()
		{
			Selection.gameObjects.LogListReferencedAssetsOfTypeInGameObjects(true, typeof(Sprite));
		}

		[MenuItem(Menu + "List All Referenced Sprites/In Selection And Children", priority = ExtenityMenu.GameObjectOperationsPriority + 64)]
		private static void Menu_ListAllReferencedSprites_InSelectionAndChildren()
		{
			Selection.gameObjects.LogListReferencedAssetsOfTypeInGameObjects(true, typeof(Sprite));
		}

		#endregion

		#region Hierarchy Menu - Sort Children By Name

		[MenuItem(Menu + "Sort Children By Name/Ascending", priority = ExtenityMenu.GameObjectOperationsPriority + 81)]
		public static void SortChildrenByName_Ascending()
		{
			var gameObjects = Selection.GetFiltered(typeof(GameObject), SelectionMode.Editable).Cast<GameObject>().ToList();
			for (var i = 0; i < gameObjects.Count; i++)
			{
				gameObjects[i].transform.SortChildren(false);
			}
		}

		[MenuItem(Menu + "Sort Children By Name/Descending", priority = ExtenityMenu.GameObjectOperationsPriorityEnd)]
		public static void SortChildrenByName_Descending()
		{
			var gameObjects = Selection.GetFiltered(typeof(GameObject), SelectionMode.Editable).Cast<GameObject>().ToList();
			for (var i = 0; i < gameObjects.Count; i++)
			{
				gameObjects[i].transform.SortChildren(true);
			}
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(GameObjectOperationsMenu));

		#endregion
	}

}
