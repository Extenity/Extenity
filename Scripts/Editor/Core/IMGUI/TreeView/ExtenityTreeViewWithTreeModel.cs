﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Extenity.IMGUIToolbox.Editor
{

	public class ExtenityTreeViewWithTreeModel<T> : TreeView where T : TreeElement
	{
#pragma warning disable 67
		private TreeModel<T> m_TreeModel;
		private readonly List<TreeViewItem> m_Rows = new List<TreeViewItem>(100);
#pragma warning restore 67


		public ExtenityTreeViewWithTreeModel(TreeViewState state, MultiColumnHeader multiColumnHeader, TreeModel<T> model)
			: base(state, multiColumnHeader)
		{
			Init(model);
		}

		private void Init(TreeModel<T> model)
		{
			m_TreeModel = model;
		}

		protected override TreeViewItem BuildRoot()
		{
			int depthForHiddenRoot = -1;
			return new ExtenityTreeViewItem<T>(m_TreeModel.root.id, depthForHiddenRoot, m_TreeModel.root.name, m_TreeModel.root);
		}

		protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
		{
			if (m_TreeModel.root == null)
			{
				Log.Error("tree model root is null. did you call SetData()?");
			}

			m_Rows.Clear();
			if (!string.IsNullOrEmpty(searchString))
			{
				Search(m_TreeModel.root, searchString, m_Rows);
			}
			else
			{
				if (m_TreeModel.root.hasChildren)
					AddChildrenRecursive(m_TreeModel.root, 0, m_Rows);
			}

			// We still need to setup the child parent information for the rows since this 
			// information is used by the TreeView internal logic (navigation, dragging etc)
			SetupParentsAndChildrenFromDepths(root, m_Rows);

			return m_Rows;
		}

		private void AddChildrenRecursive(T parent, int depth, IList<TreeViewItem> newRows)
		{
			foreach (T child in parent.children)
			{
				var item = new ExtenityTreeViewItem<T>(child.id, depth, child.name, child);
				newRows.Add(item);

				if (child.hasChildren)
				{
					if (IsExpanded(child.id))
					{
						AddChildrenRecursive(child, depth + 1, newRows);
					}
					else
					{
						item.children = CreateChildListForCollapsedParent();
					}
				}
			}
		}

		private void Search(T searchFromThis, string search, List<TreeViewItem> result)
		{
			if (string.IsNullOrEmpty(search))
				throw new ArgumentException("Invalid search: cannot be null or empty", nameof(search));

			const int kItemDepth = 0; // tree is flattened when searching

			Stack<T> stack = new Stack<T>();
			foreach (var element in searchFromThis.children)
				stack.Push((T)element);
			while (stack.Count > 0)
			{
				T current = stack.Pop();
				// Matches search?
				if (current.name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
				{
					result.Add(new ExtenityTreeViewItem<T>(current.id, kItemDepth, current.name, current));
				}

				if (current.children != null && current.children.Count > 0)
				{
					foreach (var element in current.children)
					{
						stack.Push((T)element);
					}
				}
			}
			SortSearchResult(result);
		}

		protected virtual void SortSearchResult(List<TreeViewItem> rows)
		{
			rows.Sort((x, y) => EditorUtility.NaturalCompare(x.displayName, y.displayName)); // sort by displayName by default, can be overriden for multicolumn solutions
		}

		protected override IList<int> GetAncestors(int id)
		{
			return m_TreeModel.GetAncestors(id);
		}

		protected override IList<int> GetDescendantsThatHaveChildren(int id)
		{
			return m_TreeModel.GetDescendantsThatHaveChildren(id);
		}

		#region Log

		private static readonly Logger Log = new("TreeView");

		#endregion
	}

}
