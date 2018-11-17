using System;
using System.Collections.Generic;
using Extenity.DataToolbox;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

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

		public static void DeselectAll()
		{
			Selection.objects = new Object[0];
		}

		#endregion

		#region Get Selected Objects In Scene

		//private static readonly Transform[] EmptyTransforms = new Transform[0];

		public static Transform GetSingleTransformInScene()
		{
			var objects = Selection.objects;
			if (objects.Length == 0 || objects.Length > 1)
				return null;
			var selected = Selection.activeTransform;
			if (selected == null || !selected.gameObject.scene.isLoaded)
				return null;
			return selected;
		}

		#endregion

		#region Push/Pop Selection

		private static List<int[]> SelectionStack;

		public static void PushSelection(bool keepSelection = false)
		{
			//Log.Info("Pushing current selection");
			if (SelectionStack == null)
				SelectionStack = new List<int[]>(4);
			else if (SelectionStack.Count == 100)
				Debug.LogWarning("Enormous amount of selection pushes detected.");

			var selection = Selection.instanceIDs;
			SelectionStack.Add(selection);

			if (!keepSelection)
				DeselectAll();
		}

		public static void PopSelection()
		{
			//Log.Info("Popping selection");
			if (SelectionStack.IsNullOrEmpty())
				throw new Exception("Tried to pop selection while there were none in stack.");

			var selection = SelectionStack[SelectionStack.Count - 1];
			SelectionStack.RemoveAt(SelectionStack.Count - 1);
			Selection.instanceIDs = selection;
		}

		#endregion
	}

}
