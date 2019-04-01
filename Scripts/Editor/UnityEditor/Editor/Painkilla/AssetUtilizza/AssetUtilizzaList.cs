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

namespace Extenity.PainkillaTool.Editor
{

	public class AssetUtilizzaList : TreeViewWithTreeModel<MaterialElement>
	{
		#region Configuration

		const float kRowHeights = 20f;
		const float kToggleWidth = 18f;

		//static Texture2D[] s_TestIcons =
		//{
		//	EditorGUIUtility.FindTexture("Material Icon"),
		//	EditorGUIUtility.FindTexture("Folder Icon"),
		//	EditorGUIUtility.FindTexture("AudioSource Icon"),
		//	EditorGUIUtility.FindTexture("Camera Icon"),
		//	EditorGUIUtility.FindTexture("Windzone Icon"),
		//	EditorGUIUtility.FindTexture("GameObject Icon"),
		//	EditorGUIUtility.FindTexture("Texture Icon")
		//};

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

		private enum SortOption
		{
			Name,
			TextureCount,
			MaxTextureSize,
			Instanced,
			ShaderName,
			AssetPath,
			SceneCount,
		}

		// Sort options per column
		private readonly SortOption[] SortOptions =
		{
			SortOption.Name, // Not applicable
			SortOption.Name,
			SortOption.TextureCount,
			SortOption.MaxTextureSize,
			SortOption.Instanced,
			SortOption.ShaderName,
			SortOption.SceneCount,
			SortOption.AssetPath,
		};

		internal static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
		{
			var columns = new[]
			{
				new MultiColumnHeaderState.Column
				{
					headerContent = GUIContent.none,
					contextMenuText = "Preview",
					headerTextAlignment = TextAlignment.Center,
					canSort = false,
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

			var state = new MultiColumnHeaderState(columns);
			return state;
		}

		protected override bool CanRename(TreeViewItem item)
		{
			return false;
		}

		protected override void RenameEnded(RenameEndedArgs args)
		{
		}

		protected override bool CanMultiSelect(TreeViewItem item)
		{
			return true;
		}

		#endregion

		#region GUI

		private GUIStyle _CenteredLabel;
		private GUIStyle CenteredLabel
		{
			get
			{
				if (_CenteredLabel == null)
				{
					_CenteredLabel = new GUIStyle(EditorStyles.label);
					_CenteredLabel.alignment = TextAnchor.UpperCenter;
				}
				return _CenteredLabel;
			}
		}

		protected override void RowGUI(RowGUIArgs args)
		{
			var item = (TreeViewItem<MaterialElement>)args.item;

			for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
			{
				CellGUI(args.GetCellRect(i), item, (Columns)args.GetColumn(i), ref args);
			}
		}

		private void CellGUI(Rect cellRect, TreeViewItem<MaterialElement> item, Columns column, ref RowGUIArgs args)
		{
			// Center cell rect vertically (makes it easier to place controls, icons etc in the cells)
			CenterRectUsingSingleLineHeight(ref cellRect);

			switch (column)
			{
				case Columns.Preview:
					{
						GUI.DrawTexture(cellRect, item.Data.Preview, ScaleMode.ScaleToFit);
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
						EditorGUI.ObjectField(cellRect, GUIContent.none, item.Data.Material, typeof(Material), false);
					}
					break;

				case Columns.TextureCount:
					{
						GUI.Label(cellRect, item.Data.TextureCount.ToString(), CenteredLabel);
					}
					break;

				case Columns.MaxTextureSize:
					{
						var size = item.Data.MaxTextureSize;
						GUI.Label(cellRect, size.IsAllZero() ? "" : size.x + ", " + size.y, CenteredLabel);
					}
					break;

				case Columns.Instanced:
					{
						var toggleRect = cellRect;
						toggleRect.x += (toggleRect.width - kToggleWidth) / 2;
						toggleRect.width = kToggleWidth;
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

		public AssetUtilizzaList(TreeViewState state, MultiColumnHeader multiColumnHeader, TreeModel<MaterialElement> model) : base(state, multiColumnHeader, model)
		{
			Assert.AreEqual(SortOptions.Length, Enum.GetValues(typeof(Columns)).Length, "Ensure number of sort options are in sync with number of MyColumns enum values");

			// Custom setup
			rowHeight = kRowHeights;
			columnIndexForTreeFoldouts = 1;
			showAlternatingRowBackgrounds = true;
			showBorder = true;
			// Center foldout in the row since we also center the content. See RowGUI.
			customFoldoutYOffset = (kRowHeights - EditorGUIUtility.singleLineHeight) * 0.5f;
			//extraSpaceBeforeIconAndLabel = kToggleWidth;
			multiColumnHeader.sortingChanged += OnSortingChanged;

			Reload();
		}

		#endregion

		#region Data

		// Note that we only build the visible rows. Only the backend has the full tree information. 
		// The treeview only creates info for the row list.
		protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
		{
			var rows = base.BuildRows(root);
			SortIfNeeded(root, rows);
			return rows;
		}

		public static void TreeToList(TreeViewItem root, IList<TreeViewItem> result)
		{
			if (root == null)
				throw new NullReferenceException(nameof(root));
			if (result == null)
				throw new NullReferenceException(nameof(result));

			result.Clear();

			if (root.children == null)
				return;

			var stack = new Stack<TreeViewItem>();
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

		private void SortIfNeeded(TreeViewItem root, IList<TreeViewItem> rows)
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

			var myTypes = rootItem.children.Cast<TreeViewItem<MaterialElement>>();
			var orderedQuery = InitialOrder(myTypes, sortedColumns);
			for (int i = 1; i < sortedColumns.Length; i++)
			{
				var sortOption = SortOptions[sortedColumns[i]];
				var ascending = multiColumnHeader.IsSortedAscending(sortedColumns[i]);

				switch (sortOption)
				{
					case SortOption.Name:
						orderedQuery = orderedQuery.ThenBy(l => l.Data.name, ascending);
						break;
					case SortOption.TextureCount:
						orderedQuery = orderedQuery.ThenBy(l => l.Data.TextureCount, ascending);
						break;
					case SortOption.MaxTextureSize:
						orderedQuery = orderedQuery.ThenBy(l => l.Data.MaxTextureSize.MultiplyComponents(), ascending);
						break;
					case SortOption.Instanced:
						orderedQuery = orderedQuery.ThenBy(l => l.Data.IsInstanced, ascending);
						break;
					case SortOption.ShaderName:
						orderedQuery = orderedQuery.ThenBy(l => l.Data.ShaderName, ascending);
						break;
					case SortOption.SceneCount:
						orderedQuery = orderedQuery.ThenBy(l => l.Data.FoundInScenes.Length, ascending);
						break;
					case SortOption.AssetPath:
						orderedQuery = orderedQuery.ThenBy(l => l.Data.AssetPath, ascending);
						break;
				}
			}

			rootItem.children = orderedQuery.Cast<TreeViewItem>().ToList();
		}

		private IOrderedEnumerable<TreeViewItem<MaterialElement>> InitialOrder(IEnumerable<TreeViewItem<MaterialElement>> myTypes, int[] history)
		{
			SortOption sortOption = SortOptions[history[0]];
			bool ascending = multiColumnHeader.IsSortedAscending(history[0]);
			switch (sortOption)
			{
				case SortOption.Name:
					return myTypes.Order(l => l.Data.name, ascending);
				case SortOption.TextureCount:
					return myTypes.Order(l => l.Data.TextureCount, ascending);
				case SortOption.MaxTextureSize:
					return myTypes.Order(l => l.Data.MaxTextureSize.MultiplyComponents(), ascending);
				case SortOption.Instanced:
					return myTypes.Order(l => l.Data.IsInstanced, ascending);
				case SortOption.ShaderName:
					return myTypes.Order(l => l.Data.ShaderName, ascending);
				case SortOption.SceneCount:
					return myTypes.Order(l => l.Data.FoundInScenes.Length, ascending);
				case SortOption.AssetPath:
					return myTypes.Order(l => l.Data.AssetPath, ascending);
				default:
					Assert.IsTrue(false, $"Unhandled enum '{sortOption}'.");
					break;
			}

			// default
			return myTypes.Order(l => l.Data.name, ascending);
		}

		#endregion
	}

}
