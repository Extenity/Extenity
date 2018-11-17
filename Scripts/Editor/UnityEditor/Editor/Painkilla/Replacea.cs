using System;
using System.Collections.Generic;
using System.Linq;
using Extenity.DataToolbox;
using Extenity.GameObjectToolbox;
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

			ReplaceWithObjectProperty = serializedObject.FindProperty("_ReplaceWithObject");
			ReplaceAsPrefabProperty = serializedObject.FindProperty("_ReplaceAsPrefab");
			ReplacePrefabParentProperty = serializedObject.FindProperty("_ReplacePrefabParent");
			OverrideRotationsProperty = serializedObject.FindProperty("_OverrideRotations");
			OverrideScalesProperty = serializedObject.FindProperty("_OverrideScales");
			OverrideNamesProperty = serializedObject.FindProperty("_OverrideNames");

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

		private SerializedProperty ReplaceWithObjectProperty;
		private SerializedProperty ReplaceAsPrefabProperty;
		private SerializedProperty ReplacePrefabParentProperty;
		private SerializedProperty OverrideRotationsProperty;
		private SerializedProperty OverrideScalesProperty;
		private SerializedProperty OverrideNamesProperty;

		#endregion

		#region GUI - Window

		private readonly GUILayoutOption[] ReplaceButtonOptions = { GUILayout.Width(100f), GUILayout.Height(30f) };
		private readonly GUIContent ReplaceButtonContent = new GUIContent("Replace", "Replaces all selected objects with the specified object.");
		private readonly GUIContent ReplaceWithContent = new GUIContent("Replace With", "This object will be duplicated and replaced with selected objects.");

		protected override void OnGUIDerived()
		{
			GUILayout.Space(8f);

			{
				var newReplaceWithObject = (GameObject)EditorGUILayout.ObjectField(ReplaceWithContent, ReplaceWithObject, typeof(GameObject), true);

				// Ensure ReplaceWithObject targets the root object of prefab, if it is a child object of a prefab.
				{
					var go = newReplaceWithObject.GetRootGameObjectIfChildOfAPrefabAsset();
					if (go != newReplaceWithObject)
					{
						Log.Info("Correcting the reference. Switched to parent object of the prefab, rather than the child '{newReplaceWithObject}'. See 'Replace Prefab Parent' option's tooltip for more information.", go);
						newReplaceWithObject = go;
					}
				}

				if (ReplaceWithObject != newReplaceWithObject)
				{
					//ReplaceWithObject = newReplaceWithObject;
					ReplaceWithObjectProperty.objectReferenceValue = newReplaceWithObject;

					OnSelectionChanged();
				}

				var isPrefab = IsReplaceWithObjectReferencesToAPrefabOrPrefabInstance;
				EditorGUI.BeginDisabledGroup(!isPrefab);
				{
					EditorGUILayout.PropertyField(ReplaceAsPrefabProperty);

					var isReplacePrefabParentOptionDisabled =
						!ReplaceAsPrefab || !isPrefab || // Don't need to display 'ReplacePrefabParent' if we are not even replacing the prefab as it is
						!ReplaceWithObject.IsAnInstanceInScene() || // This option is only available for prefab instances
						IsRootOfThePrefabInstance; // This option is only available if a child of the prefab instance is selected

					//if (isReplacePrefabParentOptionDisabled) No need to force user to change this value every time. We just display it as disabled in UI.
					//	ReplacePrefabParentProperty.boolValue = true;

					EditorGUI.BeginDisabledGroup(isReplacePrefabParentOptionDisabled);
					{
						EditorGUILayout.PropertyField(ReplacePrefabParentProperty);
					}
					EditorGUI.EndDisabledGroup();
				}
				EditorGUI.EndDisabledGroup();

				EditorGUILayout.PropertyField(OverrideRotationsProperty);
				EditorGUILayout.PropertyField(OverrideScalesProperty);
				EditorGUILayout.PropertyField(OverrideNamesProperty);

				EditorGUI.BeginDisabledGroup(FilteredSelection.IsNullOrEmpty() || !ReplaceWithObject);
				if (GUILayout.Button(ReplaceButtonContent, "Button", ReplaceButtonOptions))
				{
					EditorApplication.delayCall += () =>
					{
						Repaint(); // Call it before replace to ensure a repaint is being queued even if Replace throws exceptions.
						Replace();
					};
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

#pragma warning disable 414

		[SerializeField]
		private GameObject _ReplaceWithObject;
		[SerializeField]
		[Tooltip("This option becomes available if ReplaceWithObject is a prefab or a scene instance of a prefab. The cloned object simply won't keep a link to the prefab if this option is disabled.")]
		private bool _ReplaceAsPrefab = true;
		[SerializeField]
		[Tooltip("This option becomes available when a child of a prefab is selected in scene, rather than selecting the parent object of the prefab. This allows user to decide whether the selected child object or the prefab parent should be cloned. Note that it won't work if a child object of a prefab is selected in Project window because Unity won't tell us enough info about the selection that way. Instead, just drag a temporary instance into the scene and select the child object inside the scene.")]
		private bool _ReplacePrefabParent = true;
		[SerializeField]
		private bool _OverrideRotations = false;
		[SerializeField]
		private bool _OverrideScales = false;
		[SerializeField]
		private bool _OverrideNames = true;

#pragma warning restore 414

		public GameObject ReplaceWithObject { get { return (GameObject)ReplaceWithObjectProperty.objectReferenceValue; } }
		public bool ReplaceAsPrefab { get { return ReplaceAsPrefabProperty.boolValue; } }
		public bool ReplacePrefabParent { get { return ReplacePrefabParentProperty.boolValue; } }

		public bool IsReplaceWithObjectReferencesToAPrefabOrPrefabInstance
		{
			get { return ReplaceWithObject.IsPrefab(true, false, false); }
		}

		public bool IsRootOfThePrefabInstance
		{
			get { return PrefabUtility.FindPrefabRoot(ReplaceWithObject) == ReplaceWithObject; }
		}

		private void Replace()
		{
			//{
			//	//foreach (var selection in Selection.objects.Where(item => item as GameObject).Cast<GameObject>())
			//	var selection = ReplaceWithObject;
			//	{
			//		Log.Info("----------------------------- selection: " + selection.FullName());
			//		var go = selection.gameObject;
			//		Log.Info("go.IsPrefab(includePrefabInstances: true): " + go.IsPrefab(true));
			//		Log.Info("go.IsPrefab(includePrefabInstances: false): " + go.IsPrefab(false));
			//		Log.Info("go.IsAnInstanceInScene(): " + go.IsAnInstanceInScene());
			//		Log.Info("FindPrefabRoot(go): " + PrefabUtility.FindPrefabRoot(go) + "           \t Type: " + PrefabUtility.FindPrefabRoot(go).GetTypeSafe(), PrefabUtility.FindPrefabRoot(go));
			//		Log.Info("GetPrefabObject(go): " + PrefabUtility.GetPrefabObject(go) + "          \t Type: " + PrefabUtility.GetPrefabObject(go).GetTypeSafe(), PrefabUtility.GetPrefabObject(go));
			//		Log.Info("GetPrefabParent(go): " + PrefabUtility.GetPrefabParent(go), PrefabUtility.GetPrefabParent(go));
			//		Log.Info("FindValidUploadPrefabInstanceRoot(go): " + PrefabUtility.FindValidUploadPrefabInstanceRoot(go).FullName(), PrefabUtility.FindValidUploadPrefabInstanceRoot(go));
			//	}
			//	//return;
			//}

			if (FilteredSelection.IsNullOrEmpty())
				return;
			if (!ReplaceWithObject)
				return;

			// Select which object we should instantiate. 
			// - The object itself?
			// - The corresponding object in the prefab?
			// - The root of the prefab?
			var isDoingPrefabCloning = ReplaceAsPrefab && IsReplaceWithObjectReferencesToAPrefabOrPrefabInstance;
			GameObject instantiatedObject;
			if (isDoingPrefabCloning)
			{
				var isPrefabInstance = ReplaceWithObject.IsAnInstanceInScene();
				if (isPrefabInstance)
				{
					if (IsRootOfThePrefabInstance)
					{
						instantiatedObject = ReplaceWithObject;
					}
					else
					{
						if (ReplacePrefabParent)
						{
							var root = PrefabUtility.FindRootGameObjectWithSameParentPrefab(ReplaceWithObject);
							if (!root)
							{
								throw new Exception("Internal error! Failed to find prefab root.");
							}
							instantiatedObject = PrefabUtility.FindPrefabRoot(root);
						}
						else
						{
							instantiatedObject = ReplaceWithObject;
						}
					}
				}
				else
				{
					instantiatedObject = PrefabUtility.FindPrefabRoot(ReplaceWithObject);
				}
			}
			else
			{
				// The object itself.
				instantiatedObject = ReplaceWithObject;
			}

			// We should have selected an object to instantiate. Make sure it worked.
			if (!instantiatedObject)
				throw new Exception();

			var createdObjects = new List<GameObject>(FilteredSelection.Count);

			// This helps losing sibling order on undo
			var previousObjectSiblingIndices = new List<int>(FilteredSelection.Count);
			for (var i = 0; i < FilteredSelection.Count; i++)
			{
				previousObjectSiblingIndices.Add(FilteredSelection[i].GetSiblingIndex());
			}

			for (var i = 0; i < FilteredSelection.Count; i++)
			{
				var selection = FilteredSelection[i];
				Transform duplicate = PrefabUtilityTools.InstantiatePrefabOrSceneObject(instantiatedObject, isDoingPrefabCloning).transform;
				duplicate.SetParent(selection.parent);
				duplicate.SetSiblingIndex(previousObjectSiblingIndices[i]);
				duplicate.localPosition = selection.localPosition;
				if (OverrideRotationsProperty.boolValue)
					duplicate.localRotation = ReplaceWithObject.transform.localRotation;
				else
					duplicate.localRotation = selection.localRotation;
				if (OverrideScalesProperty.boolValue)
					duplicate.localScale = ReplaceWithObject.transform.localScale;
				else
					duplicate.localScale = selection.localScale;

				if (OverrideNamesProperty.boolValue)
					duplicate.gameObject.name = instantiatedObject.name;
				else
					duplicate.gameObject.name = selection.gameObject.name;

				createdObjects.Add(duplicate.gameObject);

				Undo.DestroyObjectImmediate(selection.gameObject);
				Undo.RegisterCreatedObjectUndo(duplicate.gameObject, "Replace Selected Objects");
			}

			Selection.objects = createdObjects.ToArray();

			if (GUI.changed)
			{
				serializedObject.ApplyModifiedProperties();
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
				.OrderBy(item => item.GetSiblingIndex()) // This helps losing sibling order on undo
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
