using System.Linq;
using UnityEditor;
using UnityEngine;

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

		[MenuItem("GameObject/Operations/Delete Empty Unreferenced GameObjects In Loaded Scenes", priority = 0)]
		private static void Menu_DestroyEmptyUnreferencedGameObjectsInLoadedScenes()
		{
			EditorGameObjectTools.DestroyEmptyUnreferencedGameObjectsInLoadedScenes(ActiveCheck.IncludingInactive, true, true);
		}

		[MenuItem("GameObject/Operations/Delete Empty Unreferenced GameObjects In Active Scene", priority = 1)]
		private static void Menu_DestroyEmptyUnreferencedGameObjectsInActiveScene()
		{
			EditorGameObjectTools.DestroyEmptyUnreferencedGameObjectsInActiveScene(ActiveCheck.IncludingInactive, true, true);
		}

		#endregion

		#region Hierarchy Menu - Delete All Disabled Static MeshRenderers

		[MenuItem("GameObject/Operations/Delete All Disabled Static MeshRenderers In Loaded Scenes", priority = 4)]
		private static void Menu_DeleteAllDisabledStaticMeshRenderersInLoadedScenes()
		{
			EditorGameObjectTools.DestroyAllStaticMeshRenderersAndMeshFiltersInLoadedScenes(ActiveCheck.InactiveOnly, true, true);
		}

		[MenuItem("GameObject/Operations/Delete All Disabled Static MeshRenderers In Active Scene", priority = 5)]
		private static void Menu_DeleteAllDisabledStaticMeshRenderersInActiveScene()
		{
			EditorGameObjectTools.DestroyAllStaticMeshRenderersAndMeshFiltersInActiveScene(ActiveCheck.InactiveOnly, true, true);
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
	}

}
