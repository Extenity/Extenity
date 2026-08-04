using System;
using System.Collections.Generic;
using System.Linq;
using Extenity.DataToolbox;
using Extenity.FileSystemToolbox;
using Extenity.IMGUIToolbox.Editor;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace Extenity.PainkillerToolbox.Editor
{

	public class ResourceTreeView : CatalogueTreeView<ResourceElement>
	{
		#region Configuration

		private enum Columns
		{
			Preview,
			Asset,
			AssetType,
			FileSize,
			ResourcePath,
			FullPath,
		}

		// Sorting options per column
		private static readonly SortMethod[] SortOptions =
		{
			SortMethod.NotApplicable,
			SortMethod.Name,
			SortMethod.AssetType,
			SortMethod.FileSize,
			SortMethod.ResourcePath,
			SortMethod.FullPath,
		};

		private enum SortMethod
		{
			NotApplicable,
			Name,
			AssetType,
			FileSize,
			ResourcePath,
			FullPath,
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
					headerContent = new GUIContent("Asset"),
					headerTextAlignment = TextAlignment.Center,
					sortedAscending = true,
					sortingArrowAlignment = TextAlignment.Center,
					width = 100,
					minWidth = 60,
					autoResize = true,
					allowToggleVisibility = false,
				},
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent("Asset Type"),
					headerTextAlignment = TextAlignment.Center,
					sortedAscending = true,
					sortingArrowAlignment = TextAlignment.Center,
					width = 140,
					minWidth = 80,
					autoResize = false,
					allowToggleVisibility = true,
				},
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent("File Size", "The size of asset file. Note that the asset file size does not tell anything about the built size."),
					headerTextAlignment = TextAlignment.Center,
					sortedAscending = true,
					sortingArrowAlignment = TextAlignment.Center,
					width = 60,
					minWidth = 40,
					autoResize = false,
					allowToggleVisibility = true,
				},
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent("Resource Path"),
					headerTextAlignment = TextAlignment.Center,
					sortedAscending = true,
					sortingArrowAlignment = TextAlignment.Center,
					width = 140,
					minWidth = 60,
					autoResize = true,
					allowToggleVisibility = true,
				},
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent("Full Path"),
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
			var item = (IMGUIToolbox.Editor.ExtenityTreeViewItem<ResourceElement>)args.item;

			for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
			{
				CellGUI(args.GetCellRect(i), item, (Columns)args.GetColumn(i), ref args);
			}
		}

		private void CellGUI(Rect cellRect, IMGUIToolbox.Editor.ExtenityTreeViewItem<ResourceElement> item, Columns column, ref RowGUIArgs args)
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

				case Columns.Asset:
					{
						//var indent = GetContentIndent(item); Indentation is not needed since this tree is actually a list.
						//cellRect.x += indent;
						//cellRect.width -= indent;
						EditorGUI.ObjectField(cellRect, GUIContent.none, item.Data.Asset, typeof(Object), false);
					}
					break;

				case Columns.FileSize:
					{
						GUI.Label(cellRect, item.Data.FileSize.ToFileSizeString());
					}
					break;

				case Columns.AssetType:
					{
						GUI.Label(cellRect, item.Data.AssetType);
					}
					break;

				case Columns.ResourcePath:
					{
						GUI.Label(cellRect, item.Data.ResourcePath);
					}
					break;

				case Columns.FullPath:
					{
						GUI.Label(cellRect, item.Data.FullPath);
					}
					break;
			}
		}

		#endregion

		#region Initialization

#if UNITY_6000_4_OR_NEWER
		public ResourceTreeView(TreeViewState<EntityId> state, MultiColumnHeader multiColumnHeader, TreeModel<ResourceElement> model)
#else
		public ResourceTreeView(TreeViewState<int> state, MultiColumnHeader multiColumnHeader, TreeModel<ResourceElement> model)
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
			SortByMultipleColumns();
			TreeToList(root, rows);
			Repaint();
		}

		private void SortByMultipleColumns()
		{
			var sortedColumns = multiColumnHeader.state.sortedColumns;

			if (sortedColumns.Length == 0)
				return;

			var myTypes      = rootItem.children.Cast<IMGUIToolbox.Editor.ExtenityTreeViewItem<ResourceElement>>();
			var orderedQuery = InitialOrder(myTypes, sortedColumns);
			for (int i = 1; i < sortedColumns.Length; i++)
			{
				var sortMethod = SortOptions[sortedColumns[i]];
				var ascending = multiColumnHeader.IsSortedAscending(sortedColumns[i]);

				switch (sortMethod)
				{
					case SortMethod.Name:
						orderedQuery = orderedQuery.ThenBy(l => l.Data.name, ascending);
						break;
					case SortMethod.AssetType:
						orderedQuery = orderedQuery.ThenBy(l => l.Data.AssetType, ascending);
						break;
					case SortMethod.FileSize:
						orderedQuery = orderedQuery.ThenBy(l => l.Data.FileSize, ascending);
						break;
					case SortMethod.ResourcePath:
						orderedQuery = orderedQuery.ThenBy(l => l.Data.ResourcePath, ascending);
						break;
					case SortMethod.FullPath:
						orderedQuery = orderedQuery.ThenBy(l => l.Data.FullPath, ascending);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

#if UNITY_6000_4_OR_NEWER
			rootItem.children = orderedQuery.Cast<TreeViewItem<EntityId>>().ToList();
#else
			rootItem.children = orderedQuery.Cast<TreeViewItem<int>>().ToList();
#endif
		}

		private IOrderedEnumerable<IMGUIToolbox.Editor.ExtenityTreeViewItem<ResourceElement>> InitialOrder(IEnumerable<IMGUIToolbox.Editor.ExtenityTreeViewItem<ResourceElement>> myTypes, int[] history)
		{
			var sortMethod = SortOptions[history[0]];
			var ascending = multiColumnHeader.IsSortedAscending(history[0]);
			switch (sortMethod)
			{
				case SortMethod.Name:
					return myTypes.Order(l => l.Data.name, ascending);
				case SortMethod.AssetType:
					return myTypes.Order(l => l.Data.AssetType, ascending);
				case SortMethod.FileSize:
					return myTypes.Order(l => l.Data.FileSize, ascending);
				case SortMethod.ResourcePath:
					return myTypes.Order(l => l.Data.ResourcePath, ascending);
				case SortMethod.FullPath:
					return myTypes.Order(l => l.Data.FullPath, ascending);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		#endregion
	}

}
