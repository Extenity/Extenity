using System;
using System.Collections.Generic;
using System.Linq;
using Extenity.IMGUIToolbox.Editor;
using Extenity.ReflectionToolbox.Editor;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Extenity.PainkillerToolbox.Editor
{

	[Serializable]
	public class MaterialsTool : CatalogueTool
	{
		#region GUI

		private readonly GUILayoutOption[] RefreshButtonOptions = { GUILayout.Width(100f), GUILayout.Height(24f) };
		private readonly GUIContent RefreshButtonContent = new GUIContent("Refresh", "Scans all objects.");

		public void OnGUI(Catalogue window)
		{
			InitializeListViewIfNeeded();

			// Top bar
			{
				GUILayout.BeginHorizontal();

				// Refresh button
				if (GUILayout.Button(RefreshButtonContent, RefreshButtonOptions))
				{
					GatherData();
					window.Repaint();
				}

				// Search field
				GUILayout.BeginVertical();
				GUILayout.Space(6f);
				var rect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label, GUILayoutTools.ExpandWidth);
				TreeView.searchString = SearchField.OnGUI(rect, TreeView.searchString);
				GUILayout.EndVertical();

				GUILayout.EndHorizontal();
			}

			GUILayout.Space(3f);

			// Tree view
			{
				var rect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label, GUILayoutTools.ExpandWidthAndHeight);
				TreeView.OnGUI(rect);
			}
		}

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

		private void InitializeListViewIfNeeded()
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
