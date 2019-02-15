using System;
using System.Collections.Generic;
using System.Linq;
using Extenity.IMGUIToolbox;
using Extenity.IMGUIToolbox.Editor;
using Extenity.ReflectionToolbox.Editor;
using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Extenity.PainkillaTool.Editor
{

	public class AssetUtilizza : ExtenityEditorWindowBase
	{
		#region Configuration

		protected override WindowSpecifications Specifications => new WindowSpecifications
		{
			Title = "Asset Utilizza",
			MinimumWindowSize = new Vector2(200f, 50f),
		};

		#endregion

		#region Initialization

		protected override void OnEnableDerived()
		{
			OnEnableTreeView();
		}

		[MenuItem("Tools/Painkilla/Asset Utilizza %&A", false, 100)]
		private static void ToggleWindow()
		{
			EditorWindowTools.ToggleWindow<AssetUtilizza>();
		}

		#endregion

		#region GUI - Window

		private readonly GUILayoutOption[] RefreshButtonOptions = { GUILayout.Width(100f), GUILayout.Height(30f) };
		private readonly GUIContent RefreshButtonContent = new GUIContent("Refresh", "Scans all objects.");

		protected override void OnGUIDerived()
		{
			InitializeListIfNeeded();

			//if (SelectionShouldUpdate)
			//{
			//	SelectionShouldUpdate = false;
			//	Selection.objects = NewSelection;
			//	//Log.Info($"Applying new selection ({NewSelection.Length}): " + NewSelection.Serialize('|'));
			//}

			GUILayout.Space(8f);

			GUILayout.BeginHorizontal();
			if (GUILayout.Button(RefreshButtonContent, RefreshButtonOptions))
			{
				GatherData();
			}
			GUILayout.EndHorizontal();

			// Tree view
			{
				var rect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label, GUILayoutTools.ExpandWidth, GUILayout.Height(30f));
				DrawSearchBar(rect);
				rect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label, GUILayoutTools.ExpandWidthAndHeight);
				DrawTreeView(rect);
				//DrawBottomToolBar();
			}

			if (GUI.changed)
			{
				//Calculate();
				SceneView.RepaintAll();
			}
		}

		#endregion

		#region GUI - Scene

		//private void OnSceneGUI(SceneView sceneview)
		//{
		//	var currentEvent = Event.current;
		//	var currentEventType = currentEvent.type;

		//	if (!IsActive)
		//		return;

		//	// Keep track of mouse events
		//	switch (currentEventType)
		//	{
		//		case EventType.MouseDown:
		//			{
		//				RecordedSelectionOnMouseDown = Selection.objects;
		//				Log.Info($"Selection was ({RecordedSelectionOnMouseDown.Length}): " + RecordedSelectionOnMouseDown.Serialize('|'));

		//				//if (currentEvent.button == 0)
		//				//{
		//				//	var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
		//				//	RaycastHit hitInfo;
		//				//	if (Physics.Raycast(ray, out hitInfo, float.MaxValue))
		//				//	{
		//				//		Event.current.Use();
		//				//		Log.Info("Selected: " + hitInfo.transform);
		//				//		//Selection.activeTransform = hitInfo.transform;
		//				//		HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
		//				//	}
		//				//	else
		//				//	{
		//				//		Event.current.Use();
		//				//		Log.Info("Miss! Selection: " + Selection.activeTransform);
		//				//		//Selection.activeObject = null;
		//				//		HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
		//				//	}
		//				//}
		//			}
		//			break;
		//	}
		//}

		#endregion

		#region GUI - TreeView

		[NonSerialized]
		private bool IsTreeViewInitialized;

		[SerializeField]
		private TreeViewState TreeViewState;
		[SerializeField]
		private MultiColumnHeaderState MultiColumnHeaderState;

		[NonSerialized]
		private SearchField SearchField;
		[NonSerialized]
		private AssetUtilizzaList TreeView;
		[NonSerialized]
		private TreeModel<AssetUtilizzaElement> TreeModel;

		private void OnEnableTreeView()
		{
			IsTreeViewInitialized = false;
		}

		private void DrawSearchBar(Rect rect)
		{
			TreeView.searchString = SearchField.OnGUI(rect, TreeView.searchString);
		}

		private void DrawTreeView(Rect rect)
		{
			TreeView.OnGUI(rect);
		}

		/*
		private void DrawBottomToolBar()
		{
			GUILayout.BeginHorizontal();
			{
				var style = EditorStyles.miniButton;
				if (GUILayout.Button("Expand All", style))
				{
					TreeView.ExpandAll();
				}

				if (GUILayout.Button("Collapse All", style))
				{
					TreeView.CollapseAll();
				}

				GUILayout.FlexibleSpace();

				if (GUILayout.Button("Set sorting", style))
				{
					var columnHeader = (AssetUtilizzaColumnHeader)TreeView.multiColumnHeader;
					columnHeader.SetSortingColumns(new int[] { 4, 3, 2 }, new[] { true, false, true });
					columnHeader.mode = AssetUtilizzaColumnHeader.Mode.LargeHeader;
				}

				GUILayout.Label("Header: ", "minilabel");
				if (GUILayout.Button("Large", style))
				{
					var columnHeader = (AssetUtilizzaColumnHeader)TreeView.multiColumnHeader;
					columnHeader.mode = AssetUtilizzaColumnHeader.Mode.LargeHeader;
				}
				if (GUILayout.Button("Default", style))
				{
					var columnHeader = (AssetUtilizzaColumnHeader)TreeView.multiColumnHeader;
					columnHeader.mode = AssetUtilizzaColumnHeader.Mode.DefaultHeader;
				}
				if (GUILayout.Button("No sort", style))
				{
					var columnHeader = (AssetUtilizzaColumnHeader)TreeView.multiColumnHeader;
					columnHeader.mode = AssetUtilizzaColumnHeader.Mode.MinimumHeaderWithoutSorting;
				}
			}
			GUILayout.EndHorizontal();
		}
		*/

		private void InitializeListIfNeeded()
		{
			if (IsTreeViewInitialized)
				return;
			IsTreeViewInitialized = true;

			// Check if it already exists (deserialized from window layout file or scriptable object)
			if (TreeViewState == null)
				TreeViewState = new TreeViewState();

			var isFirstInitialization = MultiColumnHeaderState == null;
			var headerState = AssetUtilizzaList.CreateDefaultMultiColumnHeaderState();
			if (MultiColumnHeaderState.CanOverwriteSerializedFields(MultiColumnHeaderState, headerState))
				MultiColumnHeaderState.OverwriteSerializedFields(MultiColumnHeaderState, headerState);
			MultiColumnHeaderState = headerState;

			var multiColumnHeader = new AssetUtilizzaColumnHeader(headerState);
			if (isFirstInitialization)
				multiColumnHeader.ResizeToFit();

			TreeModel = new TreeModel<AssetUtilizzaElement>(GetData());

			TreeView = new AssetUtilizzaList(TreeViewState, multiColumnHeader, TreeModel);

			SearchField = new SearchField();
			SearchField.downOrUpArrowKeyPressed += TreeView.SetFocusAndEnsureSelectedItem;
		}

		#endregion

		#region Data

		private void GatherData()
		{
			TreeModel.SetData(GetData());
			TreeView.Reload();
			Repaint();
		}

		private IList<AssetUtilizzaElement> GetData()
		{
			var materialsInScenes = GatherMaterialsInScenes();
			return materialsInScenes;
		}

		private static List<AssetUtilizzaElement> GatherMaterialsInScenes()
		{
			var objectsInScenes = EditorReflectionTools.FindAllReferencedObjectsInLoadedScenes<Material>();

			var elementsByObjects = new Dictionary<Material, AssetUtilizzaElement>(objectsInScenes.Sum(item => item.Value.Length));

			foreach (var objectsInScene in objectsInScenes)
			{
				var scene = objectsInScene.Key;
				var objects = objectsInScene.Value;

				foreach (var obj in objects)
				{
					if (!elementsByObjects.TryGetValue(obj, out var element))
					{
						element = new AssetUtilizzaElement(obj, scene.name);
						elementsByObjects.Add(obj, element);
					}
					else
					{
						element.AddScene(scene.name);
					}
				}
			}

			var elements = elementsByObjects.Values.ToList();
			elements.Insert(0, AssetUtilizzaElement.CreateRoot());
			return elements;
		}

		private static void GatherTexturesInScenes()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Selection

		/*
		//private Object[] RecordedSelectionOnMouseDown;
		private bool SelectionShouldUpdate;
		private Object[] NewSelection;

		private void SelectionChanged()
		{
			Calculate();
		}

		private void Calculate()
		{
			if (!IsActive)
				return;

			var changed = false;
			var currentSelection = Selection.objects;
			//var expectedSelection = currentSelection.Where(
			//	(item) =>
			//	{
			//		var gameObject = item as GameObject;
			//		var shouldDeselect = gameObject == null || gameObject.GetComponent<Collider>() == null;
			//		if (shouldDeselect)
			//			change = true;
			//		return !shouldDeselect;
			//	}
			//).ToList();
			var expectedSelection = currentSelection.Select(
				(item) =>
				{
					var gameObject = item as GameObject;
					if (gameObject == null)
					{
						changed = true;
						return null;
					}
					else
					{
						if (gameObject.GetComponent<Collider>() != null)
						{
							return gameObject;
						}
						else
						{
							changed = true;

							// See if we can find a collider on parent objects
							var parent = gameObject.GetComponentInParent<Collider>();
							if (parent != null)
							{
								return parent.gameObject;
							}
							return null;
						}
					}
				}
			).Where(item => item != null).ToList();

			//Log.Info($"Current selection ({currentSelection.Length}): " + currentSelection.Serialize('|'));

			if (changed)
			{
				//Log.Info("Should deselect");
				NewSelection = expectedSelection.ToArray();
				SelectionShouldUpdate = true;
				Repaint();

				// Somehow changing the selection won't work inside SelectionChanged event.
				//Selection.activeObject = null;
			}
		}
		*/

		#endregion
	}

}
