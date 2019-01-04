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

		[MenuItem("GameObject/Operations/Delete Empty Unreferenced GameObjects/In Loaded Scenes", priority = 0)]
		private static void Menu_DestroyEmptyUnreferencedGameObjects_InLoadedScenes()
		{
			EditorGameObjectTools.DestroyEmptyUnreferencedGameObjectsInLoadedScenes(new[] { typeof(Instantiator) }, true, true);
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
			EditorGameObjectTools.DestroyAllStaticMeshRenderersAndMeshFiltersInLoadedScenes(ActiveCheck.InactiveOnly, true, true);
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
		private static void Menu_ListAllReferencedTextures_InLoadedScenes()
		{
			SceneManagerTools.GetRootGameObjectsOfLoadedScenes().LogListReferencedAssetsOfTypeInGameObjects(true, typeof(Sprite));
		}

		[MenuItem("GameObject/Operations/List All Referenced Sprites/In Active Scene", priority = 4002)]
		private static void Menu_ListAllReferencedTextures_InActiveScene()
		{
			SceneManager.GetActiveScene().GetRootGameObjects().LogListReferencedAssetsOfTypeInGameObjects(true, typeof(Sprite));
		}

		[MenuItem("GameObject/Operations/List All Referenced Sprites/In Selection", priority = 4003)]
		private static void Menu_ListAllReferencedTextures_InSelection()
		{
			Selection.gameObjects.LogListReferencedAssetsOfTypeInGameObjects(true, typeof(Sprite));
		}

		[MenuItem("GameObject/Operations/List All Referenced Sprites/In Selection And Children", priority = 4004)]
		private static void Menu_ListAllReferencedTextures_InSelectionAndChildren()
		{
			Selection.gameObjects.LogListReferencedAssetsOfTypeInGameObjects(true, typeof(Sprite));
		}

		#endregion
	}

}
