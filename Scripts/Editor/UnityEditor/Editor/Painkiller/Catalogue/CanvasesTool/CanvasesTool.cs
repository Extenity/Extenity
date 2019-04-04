using System;
using System.Collections.Generic;
using Extenity.GameObjectToolbox;
using Extenity.IMGUIToolbox.Editor;
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
			TreeModel.SetData(BuildCanvasElementsTreeByWalkingInLoadedScenes());
			TreeView.Reload();
			SendRepaintRequest();
		}

		private static List<CanvasElement> BuildCanvasElementsTreeByWalkingInLoadedScenes()
		{
			var canvases = GameObjectTools.FindObjectsOfTypeInLoadedScenes<Canvas>(ActiveCheck.IncludingInactive, true, true);
			return BuildCanvasElementsTree(canvases);
		}

		private static List<CanvasElement> BuildCanvasElementsTreeByWalkingScenesAndPrefabs()
		{
			//var scenePaths = AssetTools.GetAllSceneAssetPaths();
			//var prefabPaths = AssetTools.GetAllPrefabAssetPaths();
			throw new NotImplementedException();
		}

		private static List<CanvasElement> BuildCanvasElementsTree(List<Canvas> canvases)
		{
			var rootElement = CanvasElement.CreateRoot();
			var elements = new List<CanvasElement>(canvases.Count + 1); // +1 is for the root.
			elements.Add(rootElement);

			foreach (var canvas in canvases)
			{
				var parentCanvas = canvas.rootCanvas;
				var isParentlessCanvas = parentCanvas == canvas;

				CanvasElement parentElement = null;
				if (isParentlessCanvas)
				{
					parentElement = rootElement;
				}
				else
				{
					// Find the parent's element. Thanks to how 'canvases' list is generated, it's guaranteed that
					// the parent was already added to 'elements' list. See 1178135234.
					foreach (var entry in elements)
					{
						if (entry.Canvas == parentCanvas)
						{
							parentElement = entry;
							break;
						}
					}
					if (parentElement == null)
					{
						// Something went wrong. The parent should have already been added to the 'elements' list. See 1178135234.
						Log.InternalError(1178135234);
					}
				}

				if (parentElement != null) // Being null is not expected. But it's just a precaution.
				{
					string sceneOrPrefabName;
					if (canvas.gameObject.scene != null)
					{
						sceneOrPrefabName = canvas.gameObject.scene.name;
					}
					else
					{
						// TODO: Get the prefab name.
						sceneOrPrefabName = "";
					}
					var element = new CanvasElement(canvas, sceneOrPrefabName, parentElement);
					elements.Add(element);
				}
			}

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

			TreeModel = new TreeModel<CanvasElement>(BuildCanvasElementsTreeByWalkingInLoadedScenes());

			TreeView = new CanvasTreeView(TreeViewState, multiColumnHeader, TreeModel);

			InitializeSearchField(TreeView.SetFocusAndEnsureSelectedItem);
		}

		#endregion
	}

}
