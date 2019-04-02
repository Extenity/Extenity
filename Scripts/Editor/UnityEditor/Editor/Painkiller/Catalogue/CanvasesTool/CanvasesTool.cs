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
	public class CanvasesTool : CatalogueTool
	{
		#region GUI

		public override void OnGUI()
		{
			InitializeTreeViewIfNeeded();

			// Top bar
			{
				DrawTopBar();
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

		protected override void OnRefreshButtonClicked()
		{
			TreeModel.SetData(GatherCanvasesInScenes());
			TreeView.Reload();
			SendRepaintRequest();
		}

		private static List<CanvasElement> GatherCanvasesInScenes()
		{
			var objectsInScenes = EditorReflectionTools.CollectDependenciesReferencedInLoadedScenes<Canvas>();

			var elementsByObjects = new Dictionary<Canvas, CanvasElement>(objectsInScenes.Sum(item => item.Value.Length));

			foreach (var objectsInScene in objectsInScenes)
			{
				var scene = objectsInScene.Key;
				var objects = objectsInScene.Value;

				foreach (var obj in objects)
				{
					if (!elementsByObjects.TryGetValue(obj, out var element))
					{
						element = new CanvasElement(obj, scene.name);
						elementsByObjects.Add(obj, element);
					}
					else
					{
						element.AddScene(scene.name);
					}
				}
			}

			var elements = elementsByObjects.Values.ToList();
			elements.Insert(0, CanvasElement.CreateRoot());
			return elements;
		}

		#endregion

		#region TreeView

		[NonSerialized]
		private bool IsTreeViewInitialized;

		[SerializeField]
		private TreeViewState TreeViewState;
		[SerializeField]
		private MultiColumnHeaderState MultiColumnHeaderState;

		[NonSerialized]
		private CanvasTreeView TreeView;
		[NonSerialized]
		private TreeModel<CanvasElement> TreeModel;

		protected override string SearchString
		{
			get => TreeView.searchString;
			set => TreeView.searchString = value;
		}

		private void InitializeTreeViewIfNeeded()
		{
			if (IsTreeViewInitialized)
				return;
			IsTreeViewInitialized = true;

			// Check if it already exists (deserialized from window layout file or scriptable object)
			if (TreeViewState == null)
				TreeViewState = new TreeViewState();

			var isFirstInitialization = MultiColumnHeaderState == null;
			var headerState = CanvasTreeView.CreateDefaultMultiColumnHeaderState();
			if (MultiColumnHeaderState.CanOverwriteSerializedFields(MultiColumnHeaderState, headerState))
				MultiColumnHeaderState.OverwriteSerializedFields(MultiColumnHeaderState, headerState);
			MultiColumnHeaderState = headerState;

			var multiColumnHeader = new MultiColumnHeader(headerState);
			if (isFirstInitialization)
				multiColumnHeader.ResizeToFit();

			TreeModel = new TreeModel<CanvasElement>(GatherCanvasesInScenes());

			TreeView = new CanvasTreeView(TreeViewState, multiColumnHeader, TreeModel);

			InitializeSearchField(TreeView.SetFocusAndEnsureSelectedItem);
		}

		#endregion
	}

}
