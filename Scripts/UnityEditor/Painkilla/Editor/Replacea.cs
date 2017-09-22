using System;
using System.Collections.Generic;
using System.Linq;
using Extenity.DataToolbox;
using Extenity.IMGUIToolbox.Editor;
using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using UnityEngine;
using SerializedObject = UnityEditor.SerializedObject;

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

			serializedObject = new SerializedObject(this);
			ReplaceAsPrefabProperty = serializedObject.FindProperty("ReplaceAsPrefab");
			ReplacePrefabParentProperty = serializedObject.FindProperty("ReplacePrefabParent");
			ReplaceRotationsProperty = serializedObject.FindProperty("ReplaceRotations");
			ReplaceScalesProperty = serializedObject.FindProperty("ReplaceScales");

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

		#region Serialized Properties

		private SerializedObject serializedObject;
		private SerializedProperty ReplaceAsPrefabProperty;
		private SerializedProperty ReplacePrefabParentProperty;
		private SerializedProperty ReplaceRotationsProperty;
		private SerializedProperty ReplaceScalesProperty;

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

				var isPrefab = ReplaceWithObject != null && IsReplaceWithObjectReferencesToAPrefab;
				EditorGUI.BeginDisabledGroup(!isPrefab);
				{
					EditorGUILayout.PropertyField(ReplaceAsPrefabProperty);
					EditorGUI.BeginDisabledGroup(!ReplaceAsPrefab || !isPrefab || !IsNotThePrefabParent);
					{
						EditorGUILayout.PropertyField(ReplacePrefabParentProperty);
					}
					EditorGUI.EndDisabledGroup();
				}
				EditorGUI.EndDisabledGroup();

				EditorGUILayout.PropertyField(ReplaceRotationsProperty);
				EditorGUILayout.PropertyField(ReplaceScalesProperty);

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
		[Tooltip("This option becomes available if ReplaceWithObject is a prefab or a scene instance of a prefab. The cloned object simply won't keep a link to the prefab if this option is disabled.")]
		public bool ReplaceAsPrefab = true;
		[Tooltip("This option becomes available when a child of the prefab is selected, rather than selecting the parent object of the prefab. This allows user to decide whether the selected child object or the prefab parent should be cloned.")]
		public bool ReplacePrefabParent = true;
		public bool ReplaceRotations = false;
		public bool ReplaceScales = false;

		public bool IsReplaceWithObjectReferencesToAPrefab
		{
			get { return ReplaceWithObject == null ? false : PrefabUtility.GetPrefabParent(ReplaceWithObject.gameObject); }
		}

		public bool IsNotThePrefabParent
		{
			get { return ReplaceWithObject.gameObject != PrefabUtility.FindValidUploadPrefabInstanceRoot(ReplaceWithObject.gameObject); }
		}

		private void Replace()
		{
			if (FilteredSelection.IsNullOrEmpty())
				return;
			if (ReplaceWithObject == null)
				return;

			// Select which object we should instantiate. 
			// - The object itselft?
			// - The corresponding object in the prefab?
			// - The root of the prefab?
			var isPrefab = ReplaceAsPrefab && IsReplaceWithObjectReferencesToAPrefab;
			GameObject instantiatedObject;
			if (isPrefab)
			{
				if (ReplacePrefabParent && IsNotThePrefabParent)
				{
					// The root of the prefab.
					var root = PrefabUtility.FindValidUploadPrefabInstanceRoot(ReplaceWithObject.gameObject);
					if (root == null)
					{
						throw new Exception("Internal error! Failed to find prefab root.");
					}
					instantiatedObject = (GameObject)PrefabUtility.GetPrefabParent(root);
				}
				else
				{
					// The corresponding object in the prefab.
					instantiatedObject = (GameObject)PrefabUtility.GetPrefabParent(ReplaceWithObject.gameObject);
				}
			}
			else
			{
				// The object itselft.
				instantiatedObject = ReplaceWithObject.gameObject;
			}

			foreach (var selection in FilteredSelection)
			{
				Transform duplicate;
				if (isPrefab)
				{
					duplicate = ((GameObject)PrefabUtility.InstantiatePrefab(instantiatedObject)).transform;
				}
				else
				{
					duplicate = Instantiate(instantiatedObject, selection.parent).transform;
				}
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
