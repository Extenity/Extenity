using System;
using System.Collections.Generic;
using System.Linq;
using Extenity.DataToolbox;
using Extenity.GameObjectToolbox;
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

		#region DeselectNonChildrenIfAnyChildSelected

#if UNITY_EDITOR

		public static bool DeselectNonChildrenIfAnyChildSelected(this Transform transform, bool delayedSelection = true)
		{
			if (!transform)
				throw new ArgumentOutOfRangeException();

			var selection = Selection.transforms;
			if (selection != null && selection.Length != 0)
			{
				if (selection.IsAnyTransformChildOf(transform))
				{
					if (!selection.IsAllTransformsChildOf(transform))
					{
						var filteredSelection = selection.Where(item => item.IsChildOf(transform)).Select(item => item.gameObject).ToArray();
						// Debug.Log($"Original selection ({selection.Length}):\n" + string.Join("\n", selection.Select(entry => entry.gameObject.name).ToArray()));
						// Debug.Log($"Filtered selection ({filteredSelection.Length}):\n" + string.Join("\n", filteredSelection.Select(entry => entry.gameObject.name).ToArray()));

						if (delayedSelection)
						{
							EditorApplication.delayCall += () =>
							{
								Selection.objects = filteredSelection;
							};
						}
						else
						{
							Selection.objects = filteredSelection;
						}
						return true;
					}
				}
			}
			return false;
		}

#endif

		#endregion

		#region Push/Pop Selection

		private static List<int[]> SelectionStack;

		public static void PushSelection(bool keepSelection = false)
		{
			//Log.Info("Pushing current selection");
			if (SelectionStack == null)
				SelectionStack = new List<int[]>(4);
			else if (SelectionStack.Count == 100)
				Log.Warning("Enormous amount of selection pushes detected.");

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

		#region Duplicate Selection

		/// <summary>
		/// Triggers Unity's duplicate (CTRL+D) functionality.
		/// Source: https://answers.unity.com/questions/168580/how-do-i-properly-duplicate-an-object-in-a-editor.html
		/// </summary>
		public static void Duplicate()
		{
			// Need to focus a scene view first. Create one if there is none.
			var sceneView = SceneView.lastActiveSceneView;
			if (!sceneView)
			{
				sceneView = SceneView.currentDrawingSceneView;
				if (!sceneView)
				{
					var allSceneViews = SceneView.sceneViews;
					if (allSceneViews == null || allSceneViews.Count == 0)
					{
						// That crashes Unity so we need to figure out another way if this functionality really needed. 
						//EditorWindow.GetWindow<SceneView>();
						throw new Exception("There must be a visible Scene window for duplication to work.");
					}
					else
					{
						sceneView = (SceneView)allSceneViews[0];
					}
				}
			}
			sceneView.Focus();

			EditorWindow.focusedWindow.SendEvent(EditorGUIUtility.CommandEvent("Duplicate"));
		}

		/// <summary>
		/// Triggers Unity's duplicate (CTRL+D) functionality.
		/// Source: https://answers.unity.com/questions/168580/how-do-i-properly-duplicate-an-object-in-a-editor.html
		/// </summary>
		public static GameObject Duplicate(this GameObject original, bool keepSelectionIntact)
		{
			ObjectTools.CheckNullArgument(original, "The Object you want to instantiate is null.");

			// Save selection
			if (keepSelectionIntact)
				PushSelection(true);

			try
			{
				// Select the original object and tell Unity to duplicate the object.
				Selection.activeGameObject = original;
				Duplicate();
				var duplicate = Selection.activeGameObject;
				return duplicate;
			}
			finally
			{
				// Restore selection
				if (keepSelectionIntact)
					PopSelection();
			}
		}

		#endregion
	}

}
