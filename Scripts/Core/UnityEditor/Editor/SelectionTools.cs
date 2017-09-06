using System.Collections;
using System.Collections.Generic;
using Extenity.DataToolbox;
using UnityEditor;
using UnityEngine;

namespace Extenity.UnityEditorToolbox.Editor
{

	public static class SelectionTools
	{
		#region Add/Remove

		public static void AddToSelection(UnityEngine.Object obj)
		{
			Selection.objects = Selection.objects.Add(obj);
		}

		public static void RemoveFromSelection(UnityEngine.Object obj)
		{
			var objects = Selection.objects;
			for (int i = 0; i < objects.Length; i++)
			{
				if (objects[i] == obj)
				{
					Selection.objects = objects.RemoveAt(i);
				}
			}
		}

		#endregion

		#region Get Selected Objects In Scene

		//private static readonly Transform[] EmptyTransforms = new Transform[0];

		public static Transform GetSingleTransformInScene()
		{
			var objects = Selection.objects;
			if (objects.Length == 0 || objects.Length > 1)
				return null;
			var selected = objects[0] as Transform;
			if (selected == null || !selected.gameObject.scene.isLoaded)
				return null;
			return selected;
		}

		#endregion
	}

}
