using System;
using System.Collections.Generic;
using System.Linq;
using Extenity.DataToolbox;
using Extenity.IMGUIToolbox.Editor;
using Extenity.MathToolbox;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Assertions;

namespace Extenity.PainkillerToolbox.Editor
{

	public class MaterialTreeView : CatalogueTreeView<MaterialElement>
	{
		#region Configuration

		private enum Columns
		{
			Preview,
			Material,
			TextureCount,
			MaxTextureSize,
			Instanced,
			ShaderName,
			Scenes,
			AssetPath,
		}

		// Sorting options per column
		private static readonly SortMethod[] SortOptions =
		{
			SortMethod.NotApplicable,
			SortMethod.Name,
			SortMethod.TextureCount,
			SortMethod.MaxTextureSize,
			SortMethod.Instanced,
			SortMethod.ShaderName,
			SortMethod.SceneCount,
			SortMethod.AssetPath,
		};

		private enum SortMethod
		{
			NotApplicable,
			Name,
			TextureCount,
			MaxTextureSize,
			Instanced,
			ShaderName,
			AssetPath,
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
					headerContent = new GUIContent("Material"),
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
					headerContent = new GUIContent("Textures"),
					headerTextAlignment = TextAlignment.Center,
					sortedAscending = true,
					sortingArrowAlignment = TextAlignment.Center,
					width = 65,
					minWidth = 65,
					maxWidth = 65,
					autoResize = false,
					allowToggleVisibility = true,
				},
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent("Largest Texture"),
					headerTextAlignment = TextAlignment.Center,
					sortedAscending = true,
					sortingArrowAlignment = TextAlignment.Center,
					width = 105,
					minWidth = 105,
					maxWidth = 105,
					autoResize = false,
					allowToggleVisibility = true,
				},
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent("Instanced"),
					headerTextAlignment = TextAlignment.Center,
					sortedAscending = true,
					sortingArrowAlignment = TextAlignment.Center,
					width = 65,
					minWidth = 65,
					maxWidth = 65,
					autoResize = false,
					allowToggleVisibility = true,
				},
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent("Shader"),
					headerTextAlignment = TextAlignment.Center,
					sortedAscending = true,
					sortingArrowAlignment = TextAlignment.Center,
					width = 250,
					minWidth = 60,
					autoResize = true,
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
			var item = (IMGUIToolbox.Editor.ExtenityTreeViewItem<MaterialElement>)args.item;

			for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
			{
				CellGUI(args.GetCellRect(i), item, (Columns)args.GetColumn(i), ref args);
			}
		}

		private void CellGUI(Rect cellRect, IMGUIToolbox.Editor.ExtenityTreeViewItem<MaterialElement> item, Columns column, ref RowGUIArgs args)
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

				case Columns.Material:
					{
						//var indent = GetContentIndent(item); Indentation is not needed since this tree is actually a list.
						//cellRect.x += indent;
						//cellRect.width -= indent;
						EditorGUI.ObjectField(cellRect, GUIContent.none, item.Data.Material, typeof(Material), false);
					}
					break;

				case Columns.TextureCount:
					{
						GUI.Label(cellRect, item.Data.TextureCount.ToString(), EditorStylesTools.CenteredLabel);
					}
					break;

				case Columns.MaxTextureSize:
					{
						var size = item.Data.MaxTextureSize;
						GUI.Label(cellRect, size.IsAllZero() ? "" : size.x + ", " + size.y, EditorStylesTools.CenteredLabel);
					}
					break;

				case Columns.Instanced:
					{
						var toggleRect = cellRect;
						toggleRect.x += (toggleRect.width - ToggleWidth) / 2;
						toggleRect.width = ToggleWidth;
						if (toggleRect.xMax < cellRect.xMax)
							EditorGUI.Toggle(toggleRect, item.Data.IsInstanced); // hide when outside cell rect
					}
					break;

				case Columns.ShaderName:
					{
						GUI.Label(cellRect, item.Data.ShaderName);
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
		public MaterialTreeView(TreeViewState<EntityId> state, MultiColumnHeader multiColumnHeader, TreeModel<MaterialElement> model)
#else
		public MaterialTreeView(TreeViewState<int> state, MultiColumnHeader multiColumnHeader, TreeModel<MaterialElement> model)
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

			var myTypes      = rootItem.children.Cast<IMGUIToolbox.Editor.ExtenityTreeViewItem<MaterialElement>>();
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
					case SortMethod.TextureCount:
						orderedQuery = orderedQuery.ThenBy(l => l.Data.TextureCount, ascending);
						break;
					case SortMethod.MaxTextureSize:
						orderedQuery = orderedQuery.ThenBy(l => l.Data.MaxTextureSize.MultiplyComponents(), ascending);
						break;
					case SortMethod.Instanced:
						orderedQuery = orderedQuery.ThenBy(l => l.Data.IsInstanced, ascending);
						break;
					case SortMethod.ShaderName:
						orderedQuery = orderedQuery.ThenBy(l => l.Data.ShaderName, ascending);
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
			rootItem.children = orderedQuery.Cast<TreeViewItem<EntityId>>().ToList();
#else
			rootItem.children = orderedQuery.Cast<TreeViewItem<int>>().ToList();
#endif
		}

		private IOrderedEnumerable<IMGUIToolbox.Editor.ExtenityTreeViewItem<MaterialElement>> InitialOrder(IEnumerable<IMGUIToolbox.Editor.ExtenityTreeViewItem<MaterialElement>> myTypes, int[] history)
		{
			var sortMethod = SortOptions[history[0]];
			var ascending = multiColumnHeader.IsSortedAscending(history[0]);
			switch (sortMethod)
			{
				case SortMethod.Name:
					return myTypes.Order(l => l.Data.name, ascending);
				case SortMethod.TextureCount:
					return myTypes.Order(l => l.Data.TextureCount, ascending);
				case SortMethod.MaxTextureSize:
					return myTypes.Order(l => l.Data.MaxTextureSize.MultiplyComponents(), ascending);
				case SortMethod.Instanced:
					return myTypes.Order(l => l.Data.IsInstanced, ascending);
				case SortMethod.ShaderName:
					return myTypes.Order(l => l.Data.ShaderName, ascending);
				case SortMethod.SceneCount:
					return myTypes.Order(l => l.Data.FoundInScenes.Length, ascending);
				case SortMethod.AssetPath:
					return myTypes.Order(l => l.Data.AssetPath, ascending);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		#endregion
	}

}
