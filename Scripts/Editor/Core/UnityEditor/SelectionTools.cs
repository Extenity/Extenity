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
			Selection.objects.Add(obj, out var result);
			Selection.objects = result;
		}

		public static void RemoveFromSelection(UnityEngine.Object obj)
		{
			var objects = Selection.objects;
			for (int i = 0; i < objects.Length; i++)
			{
				if (objects[i] == obj)
				{
					objects.RemoveAt(i, out var result);
					Selection.objects = result;
				}
			}
		}

		public static void DeselectAll()
		{
			Selection.objects = Array.Empty<Object>();
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
				// Need to focus a scene view first. Create one if there is none.
				FocusOnSceneViewForDuplicationToWork(); // TODO: Doing this won't be a good idea for Assets. It might require focusing on Project window instead.

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

		[MenuItem(ExtenityMenu.Edit + "Duplicate Adjacently #%d", priority = ExtenityMenu.UnityPlayMenuPriority_BelowDuplicate)]
		public static void DuplicateAdjacently()
		{
			var selectedObjects = Selection.objects;
			var newObjects = New.List<Object>();

			// Log.Info("Total selected objects : " + selectedObjects.Length);
			// foreach (var selectedObject in selectedObjects)
			// {
			// 	Log.Info("Selected object : " + selectedObject, selectedObject);
			// }

			for (int i = 0; i < selectedObjects.Length; i++)
			{
				// Log.Info("Duplicating : " + selectedObjects[i].FullObjectName(), selectedObjects[i]);
				var newObject = DuplicateAdjacently(selectedObjects[i]);
				if (newObject)
				{
					// Log.Info("Duplicate : " + newObject.FullObjectName(), newObject);
					newObjects.Add(newObject);
				}
				// else
				// {
				// 	Log.Info("Duplication failed");
				// }
			}

			// Mark the freshly instantiated objects as selected. But first, ensure at least some duplication operations
			// are completed. Otherwise we would lose current selection, which is a bad user experience. Operations may
			// fail if Hierarchy or Project windows doesn't have the focus.
			if (newObjects.Count > 0)
			{
				Selection.objects = newObjects.ToArray();
			}
			Release.List(ref newObjects);
		}

		public static Object DuplicateAdjacently(Object originalObject)
		{
			// Tell Unity to duplicate the object. It needs to be selected first.
			Selection.objects = new[] { originalObject };
			Duplicate();

			// See if the Duplication operation was successful. It may fail if Hierarchy or Project windows does not
			// have the focus. At that point, we should respect how Unity works and we should not be trying to do the
			// duplication forcibly.
			var newObjects = Selection.objects;
			if (newObjects == null || newObjects.Length != 1 ||
			    newObjects[0] == originalObject)
			{
				return null;
			}

			var newObject = Selection.objects[0];

			if (newObject is GameObject newGameObject &&
			    !EditorUtility.IsPersistent(newGameObject))
			{
				var originalGameObject = originalObject as GameObject;
				var originalTransform = originalGameObject.transform;
				var newTransform = newGameObject.transform;
				var newSiblingIndex = originalTransform.GetSiblingIndex() + 1;
				newTransform.SetSiblingIndex(newSiblingIndex);
				// Log.Info($"Sibling index set to {newSiblingIndex}");
			}

			return newObject;
		}

		private static void FocusOnSceneViewForDuplicationToWork()
		{
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
		}

		#endregion
	}

}
