using System;
using System.Collections.Generic;
using System.Linq;
using Extenity.DataToolbox;
using Extenity.IMGUIToolbox.Editor;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Assertions;

namespace Extenity.PainkillerToolbox.Editor
{

	public class CanvasTreeView : CatalogueTreeView<CanvasElement>
	{
		#region Configuration

		private enum Columns
		{
			Preview,
			Canvas,
			SortingOrder,
			Scenes,
			AssetPath,
		}

		// Sorting options per column
		private static readonly SortMethod[] SortOptions =
		{
			SortMethod.NotApplicable,
			SortMethod.Name,
			SortMethod.SortingOrder,
			SortMethod.SceneCount,
			SortMethod.AssetPath,
		};

		private enum SortMethod
		{
			NotApplicable,
			Name,
			AssetPath,
			SortingOrder,
			SceneCount,
		}

		internal static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
		{
			var columns = new[]
			{
				new MultiColumnHeaderState.Column
				{
					headerContent = GUIContent.none,
					contextMenuText = "Preview",
					headerTextAlignment = TextAlignment.Center,
					width = 30,
					minWidth = 30,
					maxWidth = 60,
					autoResize = false,
					allowToggleVisibility = true,
				},
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent("Canvas"),
					headerTextAlignment = TextAlignment.Center,
					sortedAscending = true,
					sortingArrowAlignment = TextAlignment.Center,
					width = 250,
					minWidth = 60,
					autoResize = true,
					allowToggleVisibility = false,
				},
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent("Order"),
					headerTextAlignment = TextAlignment.Center,
					sortedAscending = true,
					sortingArrowAlignment = TextAlignment.Center,
					width = 60,
					minWidth = 60,
					autoResize = false,
					allowToggleVisibility = true,
				},
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent("Scenes"),
					headerTextAlignment = TextAlignment.Center,
					sortedAscending = true,
					sortingArrowAlignment = TextAlignment.Center,
					width = 110,
					minWidth = 60,
					autoResize = true,
					allowToggleVisibility = true,
				},
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent("Asset Path"),
					headerTextAlignment = TextAlignment.Center,
					sortedAscending = true,
					sortingArrowAlignment = TextAlignment.Center,
					width = 180,
					minWidth = 60,
					autoResize = true,
					allowToggleVisibility = true,
				},
			};

			Assert.AreEqual(columns.Length, Enum.GetValues(typeof(Columns)).Length, "Number of columns should match number of enum values: You probably forgot to update one of them.");

			// Automatically set if the column is sortable.
			for (int iColumn = 0; iColumn < columns.Length; iColumn++)
			{
				columns[iColumn].canSort = SortOptions[iColumn++] != SortMethod.NotApplicable;
			}

			var state = new MultiColumnHeaderState(columns);
			return state;
		}

#if UNITY_6000_4_OR_NEWER
		protected override bool CanRename(TreeViewItem<EntityId> item)
#else
		protected override bool CanRename(TreeViewItem<int> item)
#endif
		{
			return false;
		}

		protected override void RenameEnded(RenameEndedArgs args)
		{
		}

#if UNITY_6000_4_OR_NEWER
		protected override bool CanMultiSelect(TreeViewItem<EntityId> item)
#else
		protected override bool CanMultiSelect(TreeViewItem<int> item)
#endif
		{
			return true;
		}

		#endregion

		#region GUI

		protected override void RowGUI(RowGUIArgs args)
		{
			var item = (IMGUIToolbox.Editor.ExtenityTreeViewItem<CanvasElement>)args.item;

			for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
			{
				CellGUI(args.GetCellRect(i), item, (Columns)args.GetColumn(i), ref args);
			}
		}

		private void CellGUI(Rect cellRect, IMGUIToolbox.Editor.ExtenityTreeViewItem<CanvasElement> item, Columns column, ref RowGUIArgs args)
		{
			// Center cell rect vertically (makes it easier to place controls, icons etc in the cells)
			CenterRectUsingSingleLineHeight(ref cellRect);

			switch (column)
			{
				case Columns.Preview:
					{
						if (item.Data.Preview)
						{
							GUI.DrawTexture(cellRect, item.Data.Preview, ScaleMode.ScaleToFit);
						}
					}
					break;

				//case MyColumns.Name:
				//	{
				//		//// Do toggle
				//		//Rect toggleRect = cellRect;
				//		//toggleRect.x += GetContentIndent(item);
				//		//toggleRect.width = kToggleWidth;
				//		//if (toggleRect.xMax < cellRect.xMax)
				//		//	item.data.enabled = EditorGUI.Toggle(toggleRect, item.data.enabled); // hide when outside cell rect

				//		// Default icon and label
				//		args.rowRect = cellRect;
				//		base.RowGUI(args);
				//	}
				//	break;

				case Columns.Canvas:
					{
						var indent = GetContentIndent(item);
						cellRect.x += indent;
						cellRect.width -= indent;
						EditorGUI.ObjectField(cellRect, GUIContent.none, item.Data.Canvas, typeof(Canvas), false);
					}
					break;

				case Columns.SortingOrder:
					{
						GUI.Label(cellRect, item.Data.SortingOrder.ToString(), EditorStylesTools.CenteredLabel);
					}
					break;

				case Columns.Scenes:
					{
						GUI.Label(cellRect, item.Data.FoundInScenesCombined);
					}
					break;

				case Columns.AssetPath:
					{
						GUI.Label(cellRect, item.Data.AssetPath);
					}
					break;
			}
		}

		#endregion

		#region Initialization

#if UNITY_6000_4_OR_NEWER
		public CanvasTreeView(TreeViewState<EntityId> state, MultiColumnHeader multiColumnHeader, TreeModel<CanvasElement> model)
#else
		public CanvasTreeView(TreeViewState<int> state, MultiColumnHeader multiColumnHeader, TreeModel<CanvasElement> model)
#endif
			: base(state, multiColumnHeader, model)
		{
			Assert.AreEqual(SortOptions.Length, Enum.GetValues(typeof(Columns)).Length, "Ensure number of sort options are in sync with number of MyColumns enum values");

			columnIndexForTreeFoldouts = 1;
			// Center foldout in the row since we also center the content. See RowGUI.
			customFoldoutYOffset = (RowHeights - EditorGUIUtility.singleLineHeight) * 0.5f;
			//extraSpaceBeforeIconAndLabel = kToggleWidth;
			multiColumnHeader.sortingChanged += OnSortingChanged;

			Reload();
		}

		#endregion

		#region Data

		// Note that we only build the visible rows. Only the backend has the full tree information. 
		// The treeview only creates info for the row list.
#if UNITY_6000_4_OR_NEWER
		protected override IList<TreeViewItem<EntityId>> BuildRows(TreeViewItem<EntityId> root)
#else
		protected override IList<TreeViewItem<int>> BuildRows(TreeViewItem<int> root)
#endif
		{
			var rows = base.BuildRows(root);
			SortIfNeeded(root, rows);
			return rows;
		}

#if UNITY_6000_4_OR_NEWER
		public static void TreeToList(TreeViewItem<EntityId> root, IList<TreeViewItem<EntityId>> result)
#else
		public static void TreeToList(TreeViewItem<int> root, IList<TreeViewItem<int>> result)
#endif
		{
			if (root == null)
				throw new NullReferenceException(nameof(root));
			if (result == null)
				throw new NullReferenceException(nameof(result));

			result.Clear();

			if (root.children == null)
				return;

#if UNITY_6000_4_OR_NEWER
			var stack = new Stack<TreeViewItem<EntityId>>();
#else
			var stack = new Stack<TreeViewItem<int>>();
#endif
			for (int i = root.children.Count - 1; i >= 0; i--)
				stack.Push(root.children[i]);

			while (stack.Count > 0)
			{
				var current = stack.Pop();
				result.Add(current);

				if (current.hasChildren && current.children[0] != null)
				{
					for (int i = current.children.Count - 1; i >= 0; i--)
					{
						stack.Push(current.children[i]);
					}
				}
			}
		}

		#endregion

		#region Sorting

		private void OnSortingChanged(MultiColumnHeader multiColumnHeader)
		{
			SortIfNeeded(rootItem, GetRows());
		}

#if UNITY_6000_4_OR_NEWER
		private void SortIfNeeded(TreeViewItem<EntityId> root, IList<TreeViewItem<EntityId>> rows)
#else
		private void SortIfNeeded(TreeViewItem<int> root, IList<TreeViewItem<int>> rows)
#endif
		{
			if (rows.Count <= 1)
				return;

			if (multiColumnHeader.sortedColumnIndex == -1)
			{
				return; // No column to sort for (just use the order the data are in)
			}

			// Sort the roots of the existing tree items
			SortChildrenByMultipleColumns(rootItem, false);
			TreeToList(root, rows);
			Repaint();
		}

#if UNITY_6000_4_OR_NEWER
		private void SortChildrenByMultipleColumns(TreeViewItem<EntityId> parentItem, bool recursive)
#else
		private void SortChildrenByMultipleColumns(TreeViewItem<int> parentItem, bool recursive)
#endif
		{
			// TODO: Recursion into children items. Recursion will be implemented when the CanvasTreeView is first used as a tree, rather than a list.
			if (recursive)
				throw new NotImplementedException();

			var sortedColumns = multiColumnHeader.state.sortedColumns;
			if (sortedColumns.Length == 0)
				return;

			var children = parentItem.children.Cast<IMGUIToolbox.Editor.ExtenityTreeViewItem<CanvasElement>>();

			// First sorting column
			IOrderedEnumerable<IMGUIToolbox.Editor.ExtenityTreeViewItem<CanvasElement>> orderedQuery;
			int i = 0;
			{
				var columnIndex = sortedColumns[i++];
				var sortMethod = SortOptions[columnIndex];
				var ascending = multiColumnHeader.IsSortedAscending(columnIndex);

				switch (sortMethod)
				{
					case SortMethod.Name:
						orderedQuery = children.Order(l => l.Data.name, ascending);
						break;
					case SortMethod.SortingOrder:
						orderedQuery = children.Order(l => l.Data.SortingOrder, ascending);
						break;
					case SortMethod.SceneCount:
						orderedQuery = children.Order(l => l.Data.FoundInScenes.Length, ascending);
						break;
					case SortMethod.AssetPath:
						orderedQuery = children.Order(l => l.Data.AssetPath, ascending);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			// Following sorting columns
			for (; i < sortedColumns.Length; i++)
			{
				var columnIndex = sortedColumns[i];
				var sortMethod = SortOptions[columnIndex];
				var ascending = multiColumnHeader.IsSortedAscending(columnIndex);

				switch (sortMethod)
				{
					case SortMethod.Name:
						orderedQuery = orderedQuery.ThenBy(l => l.Data.name, ascending);
						break;
					case SortMethod.SortingOrder:
						orderedQuery = orderedQuery.ThenBy(l => l.Data.SortingOrder, ascending);
						break;
					case SortMethod.SceneCount:
						orderedQuery = orderedQuery.ThenBy(l => l.Data.FoundInScenes.Length, ascending);
						break;
					case SortMethod.AssetPath:
						orderedQuery = orderedQuery.ThenBy(l => l.Data.AssetPath, ascending);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

#if UNITY_6000_4_OR_NEWER
			parentItem.children = orderedQuery.Cast<TreeViewItem<EntityId>>().ToList();
#else
			parentItem.children = orderedQuery.Cast<TreeViewItem<int>>().ToList();
#endif
		}

		#endregion
	}

}
