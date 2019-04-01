using System;
using System.Collections.Generic;
using System.Linq;
using Extenity.IMGUIToolbox.Editor;
using Extenity.ReflectionToolbox.Editor;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Extenity.PainkillaTool.Editor
{

	[Serializable]
	public class MaterialList
	{
		#region GUI

		public void OnGUI()
		{
			InitializeListIfNeeded();

			var rect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label, GUILayoutTools.ExpandWidth, GUILayout.Height(30f));
			DrawSearchBar(rect);
			rect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label, GUILayoutTools.ExpandWidthAndHeight);
			DrawTreeView(rect);
			//DrawBottomToolBar();
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

		#endregion

		#region Data

		public void GatherData()
		{
			TreeModel.SetData(GetData());
			TreeView.Reload();
		}

		private IList<MaterialElement> GetData()
		{
			var materialsInScenes = GatherMaterialsInScenes();
			return materialsInScenes;
		}

		private static List<MaterialElement> GatherMaterialsInScenes()
		{
			var objectsInScenes = EditorReflectionTools.CollectDependenciesReferencedInLoadedScenes<Material>();

			var elementsByObjects = new Dictionary<Material, MaterialElement>(objectsInScenes.Sum(item => item.Value.Length));

			foreach (var objectsInScene in objectsInScenes)
			{
				var scene = objectsInScene.Key;
				var objects = objectsInScene.Value;

				foreach (var obj in objects)
				{
					if (!elementsByObjects.TryGetValue(obj, out var element))
					{
						element = new MaterialElement(obj, scene.name);
						elementsByObjects.Add(obj, element);
					}
					else
					{
						element.AddScene(scene.name);
					}
				}
			}

			var elements = elementsByObjects.Values.ToList();
			elements.Insert(0, MaterialElement.CreateRoot());
			return elements;
		}

		#endregion

		#region ListView

		[NonSerialized]
		private bool IsListViewInitialized;

		[SerializeField]
		private TreeViewState TreeViewState;
		[SerializeField]
		private MultiColumnHeaderState MultiColumnHeaderState;

		[NonSerialized]
		private SearchField SearchField;
		[NonSerialized]
		private MaterialListView TreeView;
		[NonSerialized]
		private TreeModel<MaterialElement> TreeModel;

		private void InitializeListIfNeeded()
		{
			if (IsListViewInitialized)
				return;
			IsListViewInitialized = true;

			// Check if it already exists (deserialized from window layout file or scriptable object)
			if (TreeViewState == null)
				TreeViewState = new TreeViewState();

			var isFirstInitialization = MultiColumnHeaderState == null;
			var headerState = MaterialListView.CreateDefaultMultiColumnHeaderState();
			if (MultiColumnHeaderState.CanOverwriteSerializedFields(MultiColumnHeaderState, headerState))
				MultiColumnHeaderState.OverwriteSerializedFields(MultiColumnHeaderState, headerState);
			MultiColumnHeaderState = headerState;

			var multiColumnHeader = new MultiColumnHeader(headerState);
			if (isFirstInitialization)
				multiColumnHeader.ResizeToFit();

			TreeModel = new TreeModel<MaterialElement>(GetData());

			TreeView = new MaterialListView(TreeViewState, multiColumnHeader, TreeModel);

			SearchField = new SearchField();
			SearchField.downOrUpArrowKeyPressed += TreeView.SetFocusAndEnsureSelectedItem;
		}

		#endregion
	}

}
