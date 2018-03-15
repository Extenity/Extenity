using System;
using System.Collections.Generic;
using Extenity.IMGUIToolbox.Editor;
using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Extenity.PainkillaTool.Editor
{

	public static class MyTreeElementGenerator
	{
		static int IDCounter;
		static int minNumChildren = 5;
		static int maxNumChildren = 10;
		static float probabilityOfBeingLeaf = 0.5f;

		public static List<AssetUtilizzaElement> GenerateRandomTree(int numTotalElements)
		{
			int numRootChildren = numTotalElements / 4;
			IDCounter = 0;
			var treeElements = new List<AssetUtilizzaElement>(numTotalElements);

			var root = new AssetUtilizzaElement("Root", -1, IDCounter);
			treeElements.Add(root);
			for (int i = 0; i < numRootChildren; ++i)
			{
				int allowedDepth = 6;
				AddChildrenRecursive(root, Random.Range(minNumChildren, maxNumChildren), true, numTotalElements, ref allowedDepth, treeElements);
			}

			return treeElements;
		}

		private static void AddChildrenRecursive(TreeElement element, int numChildren, bool force, int numTotalElements, ref int allowedDepth, List<AssetUtilizzaElement> treeElements)
		{
			if (element.depth >= allowedDepth)
			{
				allowedDepth = 0;
				return;
			}

			for (int i = 0; i < numChildren; ++i)
			{
				if (IDCounter > numTotalElements)
					return;

				var child = new AssetUtilizzaElement("Element " + IDCounter, element.depth + 1, ++IDCounter);
				treeElements.Add(child);

				if (!force && Random.value < probabilityOfBeingLeaf)
					continue;

				AddChildrenRecursive(child, Random.Range(minNumChildren, maxNumChildren), false, numTotalElements, ref allowedDepth, treeElements);
			}
		}
	}

	public class MyMultiColumnHeader : MultiColumnHeader
	{
		Mode m_Mode;

		public enum Mode
		{
			LargeHeader,
			DefaultHeader,
			MinimumHeaderWithoutSorting
		}

		public MyMultiColumnHeader(MultiColumnHeaderState state) : base(state)
		{
			mode = Mode.DefaultHeader;
		}

		public Mode mode
		{
			get
			{
				return m_Mode;
			}
			set
			{
				m_Mode = value;
				switch (m_Mode)
				{
					case Mode.LargeHeader:
						canSort = true;
						height = 37f;
						break;
					case Mode.DefaultHeader:
						canSort = true;
						height = DefaultGUI.defaultHeight;
						break;
					case Mode.MinimumHeaderWithoutSorting:
						canSort = false;
						height = DefaultGUI.minimumHeight;
						break;
				}
			}
		}
	}



	public class AssetUtilizza : ExtenityEditorWindowBase
	{
		#region Configuration

		private static readonly Vector2 MinimumWindowSize = new Vector2(200f, 50f);

		#endregion

		#region Initialization

		[MenuItem("Tools/Painkilla/Asset Utilizza %&A", false, 100)]
		private static void ShowWindow()
		{
			var window = GetWindow<AssetUtilizza>();
			window.Show();
		}

		private void OnEnable()
		{
			SetTitleAndIcon("Asset Utilizza", null);
			minSize = MinimumWindowSize;

			OnEnableTreeView();

			//SceneView.onSceneGUIDelegate -= OnSceneGUI;
			//SceneView.onSceneGUIDelegate += OnSceneGUI;
			//Selection.selectionChanged -= SelectionChanged;
			//Selection.selectionChanged += SelectionChanged;
		}

		#endregion

		#region Deinitialization

		protected void OnDisable()
		{
			//SceneView.onSceneGUIDelegate -= OnSceneGUI;
			//Selection.selectionChanged -= SelectionChanged;
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
			//	//Debug.LogFormat("Applying new selection ({0}): {1}",
			//	//	NewSelection.Length,
			//	//	NewSelection.Serialize('|'));
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
				var rect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label, GUILayout.ExpandWidth(true), GUILayout.Height(30f));
				DrawSearchBar(rect);
				rect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
				DrawTreeView(rect);
				DrawBottomToolBar();
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
		//				Debug.LogFormat("Selection was ({0}): {1}",
		//					RecordedSelectionOnMouseDown.Length,
		//					RecordedSelectionOnMouseDown.Serialize('|'));

		//				//if (currentEvent.button == 0)
		//				//{
		//				//	var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
		//				//	RaycastHit hitInfo;
		//				//	if (Physics.Raycast(ray, out hitInfo, float.MaxValue))
		//				//	{
		//				//		Event.current.Use();
		//				//		Debug.Log("Selected: " + hitInfo.transform);
		//				//		//Selection.activeTransform = hitInfo.transform;
		//				//		HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
		//				//	}
		//				//	else
		//				//	{
		//				//		Event.current.Use();
		//				//		Debug.Log("Miss! Selection: " + Selection.activeTransform);
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

		private void OnEnableTreeView()
		{
			IsTreeViewInitialized = false;
		}

		void DrawSearchBar(Rect rect)
		{
			TreeView.searchString = SearchField.OnGUI(rect, TreeView.searchString);
		}

		private void DrawTreeView(Rect rect)
		{
			TreeView.OnGUI(rect);
		}

		void DrawBottomToolBar()
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
					var myColumnHeader = (MyMultiColumnHeader)TreeView.multiColumnHeader;
					myColumnHeader.SetSortingColumns(new int[] { 4, 3, 2 }, new[] { true, false, true });
					myColumnHeader.mode = MyMultiColumnHeader.Mode.LargeHeader;
				}


				GUILayout.Label("Header: ", "minilabel");
				if (GUILayout.Button("Large", style))
				{
					var myColumnHeader = (MyMultiColumnHeader)TreeView.multiColumnHeader;
					myColumnHeader.mode = MyMultiColumnHeader.Mode.LargeHeader;
				}
				if (GUILayout.Button("Default", style))
				{
					var myColumnHeader = (MyMultiColumnHeader)TreeView.multiColumnHeader;
					myColumnHeader.mode = MyMultiColumnHeader.Mode.DefaultHeader;
				}
				if (GUILayout.Button("No sort", style))
				{
					var myColumnHeader = (MyMultiColumnHeader)TreeView.multiColumnHeader;
					myColumnHeader.mode = MyMultiColumnHeader.Mode.MinimumHeaderWithoutSorting;
				}

				GUILayout.Space(10);

				if (GUILayout.Button("values <-> controls", style))
				{
					TreeView.showControls = !TreeView.showControls;
				}
			}
			GUILayout.EndHorizontal();
		}

		private void InitializeListIfNeeded()
		{
			if (IsTreeViewInitialized)
				return;
			IsTreeViewInitialized = true;

			// Check if it already exists (deserialized from window layout file or scriptable object)
			if (TreeViewState == null)
				TreeViewState = new TreeViewState();

			bool firstInit = MultiColumnHeaderState == null;
			var headerState = AssetUtilizzaList.CreateDefaultMultiColumnHeaderState();
			if (MultiColumnHeaderState.CanOverwriteSerializedFields(MultiColumnHeaderState, headerState))
				MultiColumnHeaderState.OverwriteSerializedFields(MultiColumnHeaderState, headerState);
			MultiColumnHeaderState = headerState;

			var multiColumnHeader = new MyMultiColumnHeader(headerState);
			if (firstInit)
				multiColumnHeader.ResizeToFit();

			var treeModel = new TreeModel<AssetUtilizzaElement>(GetData());

			TreeView = new AssetUtilizzaList(TreeViewState, multiColumnHeader, treeModel);

			SearchField = new SearchField();
			SearchField.downOrUpArrowKeyPressed += TreeView.SetFocusAndEnsureSelectedItem;
		}

		#endregion

		#region Data

		private void GatherData()
		{
			TreeView.Reload();
		}

		private IList<AssetUtilizzaElement> GetData()
		{
			// generate some test data
			return MyTreeElementGenerator.GenerateRandomTree(130);
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

			//Debug.LogFormat("Current selection ({0}): {1}",
			//	currentSelection.Length,
			//	currentSelection.Serialize('|'));

			if (changed)
			{
				//Debug.Log("Should deselect");
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
