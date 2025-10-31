using System;
using System.Collections.Generic;
using Extenity.IMGUIToolbox.Editor;
using Extenity.SceneManagementToolbox;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Extenity.PainkillerToolbox.Editor
{

	[Serializable]
	public class MaterialsTool : CatalogueTool
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
			TreeModel.SetData(BuildElementsListByCollectingDependenciesReferencedInLoadedScenes());
			TreeView.Reload();
			SendRepaintRequest();
		}

		private static List<MaterialElement> BuildElementsListByCollectingDependenciesReferencedInLoadedScenes()
		{
			return BuildElementsListByCollectingDependenciesReferencedInScenes<Material, MaterialElement>(
				(canvas, sceneName, parentElement) => new MaterialElement(canvas, sceneName, parentElement),
				MaterialElement.CreateRoot,
				SceneListFilter.LoadedScenesAndDontDestroyOnLoadScene);
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
		private MaterialTreeView TreeView;
		[NonSerialized]
		private TreeModel<MaterialElement> TreeModel;

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
			var headerState = MaterialTreeView.CreateDefaultMultiColumnHeaderState();
			if (MultiColumnHeaderState.CanOverwriteSerializedFields(MultiColumnHeaderState, headerState))
				MultiColumnHeaderState.OverwriteSerializedFields(MultiColumnHeaderState, headerState);
			MultiColumnHeaderState = headerState;

			var multiColumnHeader = new MultiColumnHeader(headerState);
			if (isFirstInitialization)
				multiColumnHeader.ResizeToFit();

			TreeModel = new TreeModel<MaterialElement>(BuildElementsListByCollectingDependenciesReferencedInLoadedScenes());

			TreeView = new MaterialTreeView(TreeViewState, multiColumnHeader, TreeModel);

			InitializeSearchField(TreeView.SetFocusAndEnsureSelectedItem);
		}

		#endregion
	}

}
