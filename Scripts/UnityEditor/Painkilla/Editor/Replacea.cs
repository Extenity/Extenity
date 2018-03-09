using System;
using System.Collections.Generic;
using System.Linq;
using Extenity.AssetToolbox.Editor;
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

			ReplaceAsPrefabProperty = serializedObject.FindProperty("ReplaceAsPrefab");
			ReplacePrefabParentProperty = serializedObject.FindProperty("ReplacePrefabParent");
			OverrideRotationsProperty = serializedObject.FindProperty("OverrideRotations");
			OverrideScalesProperty = serializedObject.FindProperty("OverrideScales");
			OverrideNamesProperty = serializedObject.FindProperty("OverrideNames");

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
				if (ReplaceWithObject != newReplaceWithObject)
				{
					ReplaceWithObject = newReplaceWithObject;
					OnSelectionChanged();
				}

				var isPrefab = IsReplaceWithObjectReferencesToAPrefabOrPrefabInstance;
				EditorGUI.BeginDisabledGroup(!isPrefab);
				{
					EditorGUILayout.PropertyField(ReplaceAsPrefabProperty);
					EditorGUI.BeginDisabledGroup(
						!ReplaceAsPrefab || !isPrefab || // Don't need to display 'ReplacePrefabParent' if we are not even replacing the prefab as it is
						!ReplaceWithObject.IsAnInstanceInScene() || // This option is only available for prefab instances
						IsRootOfThePrefabInstance // This option is only available if a child of the prefab instance is selected
					);
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

		public GameObject ReplaceWithObject;
		[Tooltip("This option becomes available if ReplaceWithObject is a prefab or a scene instance of a prefab. The cloned object simply won't keep a link to the prefab if this option is disabled.")]
		public bool ReplaceAsPrefab = true;
		[Tooltip("This option becomes available when a child of the prefab is selected, rather than selecting the parent object of the prefab. This allows user to decide whether the selected child object or the prefab parent should be cloned.")]
		public bool ReplacePrefabParent = true;
		public bool OverrideRotations = false;
		public bool OverrideScales = false;
		public bool OverrideNames = false;

		public bool IsReplaceWithObjectReferencesToAPrefabOrPrefabInstance
		{
			get { return ReplaceWithObject.IsPrefab(true); }
		}

		public bool IsRootOfThePrefabInstance
		{
			get { return PrefabUtility.FindPrefabRoot(ReplaceWithObject) == ReplaceWithObject; }
		}

		private void Replace()
		{
			{
				//foreach (var selection in Selection.objects.Where(item => item as GameObject).Cast<GameObject>())
				var selection = ReplaceWithObject;
				{
					Debug.Log("----------------------------- selection: " + selection.FullName());
					var go = selection.gameObject;
					Debug.Log("go.IsPrefab(includePrefabInstances: true): " + go.IsPrefab(true));
					Debug.Log("go.IsPrefab(includePrefabInstances: false): " + go.IsPrefab(false));
					Debug.Log("go.IsAnInstanceInScene(): " + go.IsAnInstanceInScene());
					Debug.Log("FindPrefabRoot(go): " + PrefabUtility.FindPrefabRoot(go) + "           \t Type: " + PrefabUtility.FindPrefabRoot(go).GetType(), PrefabUtility.FindPrefabRoot(go));
					Debug.Log("GetPrefabObject(go): " + PrefabUtility.GetPrefabObject(go) + "          \t Type: " + PrefabUtility.GetPrefabObject(go).GetType(), PrefabUtility.GetPrefabObject(go));
					Debug.Log("GetPrefabParent(go): " + PrefabUtility.GetPrefabParent(go), PrefabUtility.GetPrefabParent(go));
					Debug.Log("FindValidUploadPrefabInstanceRoot(go): " + PrefabUtility.FindValidUploadPrefabInstanceRoot(go).FullName(), PrefabUtility.FindValidUploadPrefabInstanceRoot(go));
					//Debug.Log("------ ");
					//Debug.Log("selection.IsPrefab(includePrefabInstances: true): " + selection.IsPrefab(true));
					//Debug.Log("selection.IsPrefab(includePrefabInstances: false): " + selection.IsPrefab(false));
					//Debug.Log("selection.IsAnInstanceInScene(): " + selection.IsAnInstanceInScene());
					//Debug.Log("GetPrefabObject(selection): " + PrefabUtility.GetPrefabObject(selection) + "      \t Type: " + PrefabUtility.GetPrefabObject(selection).GetType(), PrefabUtility.GetPrefabObject(selection));
					//Debug.Log("GetPrefabParent(selection): " + PrefabUtility.GetPrefabParent(selection), PrefabUtility.GetPrefabParent(selection));
					//Debug.Log("FindValidUploadPrefabInstanceRoot(selection): " + PrefabUtility.FindValidUploadPrefabInstanceRoot(selection).FullName(), PrefabUtility.FindValidUploadPrefabInstanceRoot(selection));
				}
				//return;
			}

			if (FilteredSelection.IsNullOrEmpty())
				return;
			if (ReplaceWithObject == null)
				return;

			// Select which object we should instantiate. 
			// - The object itself?
			// - The corresponding object in the prefab?
			// - The root of the prefab?
			var isPrefab = ReplaceAsPrefab && IsReplaceWithObjectReferencesToAPrefabOrPrefabInstance;
			GameObject instantiatedObject;
			Debug.Log("isPrefab: " + isPrefab + "           ReplaceAsPrefab: " + ReplaceAsPrefab + "                IsReplaceWithObjectReferencesToAPrefab: " + IsReplaceWithObjectReferencesToAPrefabOrPrefabInstance);
			if (isPrefab)
			{
				var isPrefabInstance = ReplaceWithObject.IsAnInstanceInScene();
				if (isPrefabInstance)
				{
					if (IsRootOfThePrefabInstance)
					{
						Debug.Log("#### prefab replacement isPrefabInstance-AAAA");
						instantiatedObject = PrefabUtility.FindPrefabRoot(ReplaceWithObject);
					}
					else
					{
						if (ReplacePrefabParent)
						{
							Debug.Log("#### prefab replacement isPrefabInstance-BBBB");
							var root = PrefabUtility.FindRootGameObjectWithSameParentPrefab(ReplaceWithObject);
							if (!root)
							{
								throw new Exception("Internal error! Failed to find prefab root.");
							}
							instantiatedObject = PrefabUtility.FindPrefabRoot(root);
						}
						else
						{
							Debug.Log("#### prefab replacement isPrefabInstance-CCCCC");
							instantiatedObject = PrefabUtility.FindPrefabRoot(ReplaceWithObject);
						}
					}
				}
				else
				{
					Debug.Log("#### prefab replacement 2222222");
					instantiatedObject = PrefabUtility.FindPrefabRoot(ReplaceWithObject);
				}
				//if (ReplacePrefabParent && IsNotThePrefabParent)
				//{
				//	Debug.Log("#### prefab replacement 11111");

				//	// The root of the prefab.
				//	var root = PrefabUtility.FindValidUploadPrefabInstanceRoot(ReplaceWithObject);
				//	if (!root)
				//	{
				//		throw new Exception("Internal error! Failed to find prefab root.");
				//	}
				//	instantiatedObject = (GameObject)PrefabUtility.GetPrefabObject(root);
				//}
				//else
				//{
				//	Debug.Log("#### prefab replacement 2222222");

				//	// The corresponding object in the prefab.
				//	//PrefabUtility.ConnectGameObjectToPrefab()
				//	//PrefabUtility.FindPrefabRoot()
				//	//PrefabUtility.FindRootGameObjectWithSameParentPrefab()
				//	instantiatedObject = PrefabUtility.FindPrefabRoot(ReplaceWithObject);
				//}
			}
			else
			{
				Debug.Log("object replacement");
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

					duplicate = Instantiate(instantiatedObject, selection.parent).Cast<GameObject>().transform;
				}
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
