using System.Collections.Generic;
using System.Linq;
using Extenity.DataToolbox;
using Extenity.IMGUIToolbox.Editor;
using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Extenity.PainkillaTool.Editor
{

	public class Replacea : ExtenityEditorWindowBase
	{
		#region Configuration

		private static readonly Vector2 MinimumWindowSize = new Vector2(200f, 50f);

		#endregion

		#region Initialization

		[MenuItem("Edit/Replacea", false, 1010)] // Just below Unity's "Snap Settings"
		private static void ShowWindow()
		{
			var window = GetWindow<Replacea>();
			window.Show();
		}

		private void OnEnable()
		{
			SetTitleAndIcon("Replacea", null);
			minSize = MinimumWindowSize;

			Selection.selectionChanged -= OnSelectionChanged;
			Selection.selectionChanged += OnSelectionChanged;
			//SceneView.onSceneGUIDelegate -= OnSceneGUI;
			//SceneView.onSceneGUIDelegate += OnSceneGUI;
		}

		#endregion

		#region Deinitialization

		protected void OnDestroy()
		{
			Selection.selectionChanged -= OnSelectionChanged;
			//SceneView.onSceneGUIDelegate -= OnSceneGUI;
		}

		#endregion

		#region GUI - Window

		private readonly GUILayoutOption[] ReplaceButtonOptions = { GUILayout.Width(100f), GUILayout.Height(30f) };
		private readonly GUIContent ReplaceButtonContent = new GUIContent("Replace", "Replaces all selected objects with the specified object.");
		private readonly GUIContent ReplaceWithContent = new GUIContent("Replace With", "This object will be duplicated and replaced with selected objects.");

		protected override void OnGUIDerived()
		{
			GUILayout.Space(8f);

			{
				var newReplaceWithObject = (Transform)EditorGUILayout.ObjectField(ReplaceWithContent, ReplaceWithObject, typeof(Transform), true);
				if (ReplaceWithObject != newReplaceWithObject)
				{
					ReplaceWithObject = newReplaceWithObject;
					OnSelectionChanged();
				}

				ReplaceRotations = EditorGUILayout.Toggle("Replace Rotations", ReplaceRotations);
				ReplaceScales = EditorGUILayout.Toggle("Replace Scales", ReplaceScales);

				EditorGUI.BeginDisabledGroup(FilteredSelection.IsNullOrEmpty() || ReplaceWithObject == null);
				if (GUILayout.Button(ReplaceButtonContent, "Button", ReplaceButtonOptions))
				{
					Replace();
				}
				EditorGUI.EndDisabledGroup();
			}

			GUILayout.Space(20f);

			GUILayout.BeginHorizontal();
			EditorGUILayoutTools.DrawHeader("Selected Objects (" + FilteredSelection.SafeCount() + " of " + Selection.objects.SafeLength() + ")");
			if (!FilteredSelection.IsNullOrEmpty())
			{
				if (GUILayout.Button("Select Filtered"))
				{
					EditorApplication.delayCall += SelectFilteredObjects;
				}
			}
			GUILayout.EndHorizontal();
			{
				if (!FilteredSelection.IsNullOrEmpty())
				{
					//EditorGUI.BeginDisabledGroup(true);
					for (var i = 0; i < FilteredSelection.Count; i++)
					{
						var selection = FilteredSelection[i];
						EditorGUILayout.ObjectField("Object " + i.ToString(), selection, typeof(Transform), true);
					}
					//EditorGUI.EndDisabledGroup();
				}
			}

			if (GUI.changed)
			{
				SceneView.RepaintAll();
			}
		}

		#endregion

		#region Replace

		public Transform ReplaceWithObject;
		public bool ReplaceRotations = false;
		public bool ReplaceScales = false;

		private void Replace()
		{
			if (FilteredSelection.IsNullOrEmpty())
				return;
			if (ReplaceWithObject == null)
				return;

			foreach (var selection in FilteredSelection)
			{
				var duplicate = Instantiate(ReplaceWithObject.gameObject, selection.parent).transform;
				duplicate.localPosition = selection.localPosition;
				if (!ReplaceRotations)
					duplicate.localRotation = selection.localRotation;
				if (!ReplaceScales)
					duplicate.localScale = selection.localScale;
				duplicate.gameObject.name = selection.gameObject.name;

				Undo.DestroyObjectImmediate(selection.gameObject);
				Undo.RegisterCreatedObjectUndo(duplicate.gameObject, "Replace Selected Objects");
			}
		}

		#endregion

		#region Selection

		public List<Transform> FilteredSelection;

		private void OnSelectionChanged()
		{
			FilteredSelection = GetSelection();
			Repaint();
		}

		private List<Transform> GetSelection()
		{
			return Selection.GetFiltered<Transform>(SelectionMode.TopLevel)
				.Where(transform => transform != null && transform != ReplaceWithObject)
				.ToList();
		}

		private void SelectFilteredObjects()
		{
			if (FilteredSelection.IsNullOrEmpty())
			{
				Selection.activeObject = null;
			}
			Selection.objects = FilteredSelection.Select(transform => transform.gameObject).ToArray();
		}

		#endregion
	}

}
