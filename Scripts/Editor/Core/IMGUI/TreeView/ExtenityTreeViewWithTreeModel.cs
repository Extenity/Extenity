using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Extenity.IMGUIToolbox.Editor
{

#if UNITY_6000_4_OR_NEWER
	public class ExtenityTreeViewWithTreeModel<T> : TreeView<EntityId> where T : TreeElement
#else
	public class ExtenityTreeViewWithTreeModel<T> : TreeView<int> where T : TreeElement
#endif
	{
#pragma warning disable 67
		private TreeModel<T> m_TreeModel;
#if UNITY_6000_4_OR_NEWER
		private readonly List<TreeViewItem<EntityId>> m_Rows = new List<TreeViewItem<EntityId>>(100);
#else
		private readonly List<TreeViewItem<int>> m_Rows = new List<TreeViewItem<int>>(100);
#endif
#pragma warning restore 67


#if UNITY_6000_4_OR_NEWER
		public ExtenityTreeViewWithTreeModel(TreeViewState<EntityId> state, MultiColumnHeader multiColumnHeader, TreeModel<T> model)
#else
		public ExtenityTreeViewWithTreeModel(TreeViewState<int> state, MultiColumnHeader multiColumnHeader, TreeModel<T> model)
#endif
			: base(state, multiColumnHeader)
		{
			Init(model);
		}

		private void Init(TreeModel<T> model)
		{
			m_TreeModel = model;
		}

#if UNITY_6000_4_OR_NEWER
		protected override TreeViewItem<EntityId> BuildRoot()
#else
		protected override TreeViewItem<int> BuildRoot()
#endif
		{
			int depthForHiddenRoot = -1;
			return new ExtenityTreeViewItem<T>(m_TreeModel.root.id, depthForHiddenRoot, m_TreeModel.root.name, m_TreeModel.root);
		}

#if UNITY_6000_4_OR_NEWER
		protected override IList<TreeViewItem<EntityId>> BuildRows(TreeViewItem<EntityId> root)
#else
		protected override IList<TreeViewItem<int>> BuildRows(TreeViewItem<int> root)
#endif
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

#if UNITY_6000_4_OR_NEWER
		private void AddChildrenRecursive(T parent, int depth, IList<TreeViewItem<EntityId>> newRows)
#else
		private void AddChildrenRecursive(T parent, int depth, IList<TreeViewItem<int>> newRows)
#endif
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

#if UNITY_6000_4_OR_NEWER
		private void Search(T searchFromThis, string search, List<TreeViewItem<EntityId>> result)
#else
		private void Search(T searchFromThis, string search, List<TreeViewItem<int>> result)
#endif
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

#if UNITY_6000_4_OR_NEWER
		protected virtual void SortSearchResult(List<TreeViewItem<EntityId>> rows)
#else
		protected virtual void SortSearchResult(List<TreeViewItem<int>> rows)
#endif
		{
			rows.Sort((x, y) => EditorUtility.NaturalCompare(x.displayName, y.displayName)); // sort by displayName by default, can be overriden for multicolumn solutions
		}

#if UNITY_6000_4_OR_NEWER
		protected override IList<EntityId> GetAncestors(EntityId id)
#else
		protected override IList<int> GetAncestors(int id)
#endif
		{
			return m_TreeModel.GetAncestors(id);
		}

#if UNITY_6000_4_OR_NEWER
		protected override IList<EntityId> GetDescendantsThatHaveChildren(EntityId id)
#else
		protected override IList<int> GetDescendantsThatHaveChildren(int id)
#endif
		{
			return m_TreeModel.GetDescendantsThatHaveChildren(id);
		}

		#region Log

		private static readonly Logger Log = new("TreeView");

		#endregion
	}

}
