using UnityEngine;
using UnityEditor;

namespace Extenity.UnityEditorToolbox
{

	public class BatchRemove : ScriptableObject
	{
		[MenuItem("Tools/Batch Remove/Remove Colliders")]
		private static void RemoveColliders()
		{
			ProcessRemoveColliders();
		}

		private static void ProcessRemoveColliders()
		{
			Object[] selectedObjects = GetSelectedObjects();
			//Selection.objects = new Object[0];
			foreach (GameObject child in selectedObjects)
			{
				DestroyImmediate(child.GetComponent<Collider>());
			}
		}

		private static Object[] GetSelectedObjects()
		{
			return Selection.GetFiltered(typeof(GameObject), SelectionMode.Deep);
		}
	}

}
