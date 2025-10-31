using System;
using System.Collections.Generic;
using Extenity.IMGUIToolbox.Editor;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Extenity.PainkillerToolbox.Editor
{

	[Serializable]
	public class ResourcesTool : CatalogueTool
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

		public bool IncludeInEditorFolders;

		protected override void OnRefreshButtonClicked()
		{
			TreeModel.SetData(BuildElementsListByCollectingAllResourceAssets());
			TreeView.Reload();
			SendRepaintRequest();
		}

		private List<ResourceElement> BuildElementsListByCollectingAllResourceAssets()
		{
			return BuildElementsListByCollectingResources<Object, ResourceElement>(
				(asset, assetPath, resourcePath, parentElement) => new ResourceElement(asset, assetPath, resourcePath, parentElement),
				ResourceElement.CreateRoot,
				"", IncludeInEditorFolders);
		}

		#endregion

		#region TreeView

		[NonSerialized]
		private bool IsTreeViewInitialized;

		[SerializeField]
		private ExtenityTreeViewState TreeViewState;
		[SerializeField]
		private MultiColumnHeaderState MultiColumnHeaderState;

		[NonSerialized]
		private ResourceTreeView TreeView;
		[NonSerialized]
		private TreeModel<ResourceElement> TreeModel;

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
				TreeViewState = new ExtenityTreeViewState();

			var isFirstInitialization = MultiColumnHeaderState == null;
			var headerState = ResourceTreeView.CreateDefaultMultiColumnHeaderState();
			if (MultiColumnHeaderState.CanOverwriteSerializedFields(MultiColumnHeaderState, headerState))
				MultiColumnHeaderState.OverwriteSerializedFields(MultiColumnHeaderState, headerState);
			MultiColumnHeaderState = headerState;

			var multiColumnHeader = new MultiColumnHeader(headerState);
			if (isFirstInitialization)
				multiColumnHeader.ResizeToFit();

			TreeModel = new TreeModel<ResourceElement>(BuildElementsListByCollectingAllResourceAssets());

			TreeView = new ResourceTreeView(TreeViewState, multiColumnHeader, TreeModel);

			InitializeSearchField(TreeView.SetFocusAndEnsureSelectedItem);
		}

		#endregion
	}

}
