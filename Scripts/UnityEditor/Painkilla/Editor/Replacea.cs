using System;
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

			ReplaceAsPrefabProperty = serializedObject.FindProperty("ReplaceAsPrefab");
			ReplacePrefabParentProperty = serializedObject.FindProperty("ReplacePrefabParent");
			OverrideRotationsProperty = serializedObject.FindProperty("OverrideRotations");
			OverrideScalesProperty = serializedObject.FindProperty("OverrideScales");

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

		private SerializedProperty ReplaceAsPrefabProperty;
		private SerializedProperty ReplacePrefabParentProperty;
		private SerializedProperty OverrideRotationsProperty;
		private SerializedProperty OverrideScalesProperty;

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

				EditorGUILayout.PropertyField(OverrideRotationsProperty);
				EditorGUILayout.PropertyField(OverrideScalesProperty);

				EditorGUI.BeginDisabledGroup(FilteredSelection.IsNullOrEmpty() || ReplaceWithObject == null);
				if (GUILayout.Button(ReplaceButtonContent, "Button", ReplaceButtonOptions))
				{
					Replace();
				}
				EditorGUI.EndDisabledGroup();
			}

			GUILayout.Space(20f);

			GUILayout.BeginHorizontal();
			var filteredCount = FilteredSelection.SafeCount();
			var selectedCount = Selection.objects.SafeLength();
			EditorGUILayoutTools.DrawHeader("Selected Objects (" + filteredCount + " of " + selectedCount + ")");
			if (!FilteredSelection.IsNullOrEmpty())
			{
				EditorGUI.BeginDisabledGroup(filteredCount == selectedCount);
				if (GUILayout.Button("Select Filtered"))
				{
					EditorApplication.delayCall += SelectFilteredObjects;
				}
				EditorGUI.EndDisabledGroup();
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
		public bool OverrideRotations = false;
		public bool OverrideScales = false;

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
			// - The object itself?
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
				// The object itself.
				instantiatedObject = ReplaceWithObject.gameObject;
			}

			var createdObjects = new List<GameObject>(FilteredSelection.Count);

			foreach (var selection in FilteredSelection)
			{
				var siblingIndex = selection.GetSiblingIndex();

				Transform duplicate;
				if (isPrefab)
				{

					duplicate = ((GameObject)PrefabUtility.InstantiatePrefab(instantiatedObject)).transform;
					duplicate.SetParent(selection.parent);
				}
				else
				{
					//Selection.activeObject = instantiatedObject;
					//SceneView.lastActiveSceneView.Focus();
					//EditorWindow.focusedWindow.SendEvent(EditorGUIUtility.CommandEvent("Duplicate"));
					//duplicate = Selection.activeTransform;
					//duplicate.SetParent(selection.parent);

					duplicate = Instantiate(instantiatedObject, selection.parent).transform;
				}
				duplicate.SetSiblingIndex(siblingIndex);
				duplicate.localPosition = selection.localPosition;
				if (OverrideRotationsProperty.boolValue)
					duplicate.localRotation = ReplaceWithObject.localRotation;
				else
					duplicate.localRotation = selection.localRotation;
				if (OverrideScalesProperty.boolValue)
					duplicate.localScale = ReplaceWithObject.localScale;
				else
					duplicate.localScale = selection.localScale;

				duplicate.gameObject.name = selection.gameObject.name;

				createdObjects.Add(duplicate.gameObject);

				Undo.DestroyObjectImmediate(selection.gameObject);
				Undo.RegisterCreatedObjectUndo(duplicate.gameObject, "Replace Selected Objects");
			}

			Selection.objects = createdObjects.ToArray();
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
