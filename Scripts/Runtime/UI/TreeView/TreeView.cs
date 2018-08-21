using System;
using UnityEngine;
using System.Collections.Generic;
using Extenity.GameObjectToolbox;
using UnityEngine.EventSystems;

namespace Extenity.UIToolbox
{

	public class TreeView<TData> : UIBehaviour
	{
		#region Initialization

		protected override void Awake()
		{
			InitializeConfiguration();
			InitializeNodes();
		}

		#endregion

		#region Node

		public class Node
		{
			public readonly int ID;
			public Node Parent;
			public List<Node> Children;
			public int Indentation;
			public bool Collapsed = false;
			public TreeViewItem<TData> Component;
			public CanvasGroup CanvasGroup;

			public TData Data;

			#region Initialization

			public Node()
			{
				ID = ++LastGivenNodeID;
			}

			#endregion

			#region ID

			private static int LastGivenNodeID = 0;

			#endregion

			#region Parent / Child

			public bool IsRoot { get { return Parent == null; } }
			public bool IsLeaf { get { return !HasChild; } }
			public bool HasChild { get { return Children != null && Children.Count > 0; } }

			public Node LastChild
			{
				get
				{
					return Children != null && Children.Count > 0
						? Children[Children.Count - 1]
						: null;
				}
			}

			public int GetLastGrandchildOrSelfSiblingIndex()
			{
				var node = LastChild;
				if (node == null)
				{
					node = this;
				}
				else
				{
					// Go deeper until no grandchild is left.
					var child = node.LastChild;
					while (child != null)
					{
						node = child;
						child = node.LastChild;
					}
				}
				return node.Component.transform.GetSiblingIndex();
			}

			#endregion
		}

		#endregion

		#region Configuration

		[Header("General")]
		public Transform Container;
		public GameObject ItemTemplate;
		public bool IncludeRootNode = false;

		private void InitializeConfiguration()
		{
			ItemTemplate.SetActive(false);
		}

		#endregion

		#region Nodes

		/// <summary>
		/// All existing nodes indexed by IDs. Do not modify this dictionary.
		/// </summary>
		public Dictionary<int, Node> Nodes = new Dictionary<int, Node>();

		private Node RootNode;

		private void InitializeNodes()
		{
			if (IncludeRootNode)
			{
				RootNode = AddNode(default(TData), null);
			}
			else
			{
				RootNode = new Node
				{
					Parent = null,
					Children = null,
					Indentation = -1, // Root should be invisible. Give it a negative indentation so the children will have 0 indentation.
					Collapsed = false,
					Component = null,
					CanvasGroup = null,
					Data = default(TData),
				};

				Nodes.Add(RootNode.ID, RootNode);
			}
		}

		public void GatherAllChildren(Node target, List<Node> list)
		{
			if (target.Children != null && target.Children.Count > 0)
			{
				for (int i = 0; i < target.Children.Count; ++i)
				{
					var child = target.Children[i];
					GatherAllChildren(child, list);
					list.Add(child);
				}
			}
		}

		#endregion

		#region Nodes - Add / Remove

		public virtual Node AddNode(TData data, Node parent = null)
		{
			if (parent == null && RootNode != null)
			{
				parent = RootNode;
			}

			var isRoot = parent == null;

			var itemGO = Instantiate(ItemTemplate);
			var itemComponent = itemGO.GetComponent<TreeViewItem<TData>>();
			if (!itemComponent)
			{
#if UNITY_EDITOR // Optimization. No need to include any details in runtime.
				throw new Exception("TreeView item should have a TreeViewItem component.");
#else
				throw new Exception();
#endif
			}
			var itemCanvasGroup = itemGO.GetSingleOrAddComponent<CanvasGroup>();

			var newNode = new Node
			{
				Parent = parent,
				Children = null,
				Collapsed = false,
				Indentation = isRoot ? 0 : parent.Indentation + 1,
				Component = itemComponent,
				CanvasGroup = itemCanvasGroup,
				Data = data,
			};
			Nodes.Add(newNode.ID, newNode);

			var eventHandler = itemGO.GetSingleOrAddComponent<TreeViewItemEventHandler>();
			eventHandler.onBeginDrag = (evt) => OnBeginDrag(evt, newNode);
			eventHandler.onDrag = (evt) => OnDrag(evt, newNode);
			eventHandler.onEndDrag = (evt) => OnEndDrag(evt, newNode);
			eventHandler.onPointerClick = (evt) => OnPointerClick(evt, newNode);
			eventHandler.onPointerEnter = (evt) => OnPointerEnter(evt, newNode);
			eventHandler.onPointerExit = (evt) => OnPointerExit(evt, newNode);

			// Set transform parent and order.
			itemGO.transform.SetParent(Container, false);
			if (!isRoot)
			{
				var siblingIndexOfLastNodeInParent = parent.GetLastGrandchildOrSelfSiblingIndex();
				itemGO.transform.SetSiblingIndex(siblingIndexOfLastNodeInParent + 1);
			}

			// Add this node into parent's children list.
			if (!isRoot)
			{
				var parentWasLeaf = parent.IsLeaf;
				if (parent.Children == null)
				{
					parent.Children = new List<Node>();
				}
				parent.Children.Add(newNode);
				if ((parentWasLeaf != parent.IsLeaf) && parent.Component)
				{
					parent.Component.OnLeafStateChanged(false);
				}
			}

			itemGO.SetActive(true);
			itemComponent.OnItemCreated(newNode);
			itemComponent.OnLeafStateChanged(true);
			return newNode;
		}

		public void RemoveSelectedNode()
		{
			var selected = SelectedNode;
			if (selected != null)
			{
				Select(null);
				RemoveNode(selected);
			}
		}

		public virtual void RemoveNode(Node target)
		{
			var nodes = new List<Node>();
			nodes.Add(target); // Add self
			GatherAllChildren(target, nodes); // Add all children

			for (int i = 0; i < nodes.Count; i++)
			{
				var node = nodes[i];

				Nodes.Remove(node.ID);

				if (node.Component)
				{
					Destroy(node.Component.gameObject);
				}
			}

			nodes.Clear();
		}

		#endregion

		#region Drag and Drop

		[Header("Drag and Drop")]
		public bool EnableDragAndDrop = false;
		public Transform DraggingCanvas;

		private Node DraggingNode;

		protected virtual void OnBeginDrag(PointerEventData eventData, Node node)
		{
			if (!EnableDragAndDrop)
				return;

			throw new NotImplementedException();

			/*
			if (DraggingNode != null)
			{
				OnEndDrag(eventData, DraggingNode);
			}
			DraggingNode = node;
			Collapse(DraggingNode);
			DraggingNode.Component.transform.SetParent(DraggingCanvas);
			DraggingNode.CanvasGroup.blocksRaycasts = false;
			*/
		}

		protected virtual void OnDrag(PointerEventData eventData, Node node)
		{
			if (DraggingNode == null)
				return;

			if (DraggingNode == node)
			{
				DraggingNode.Component.transform.position = eventData.position;
			}
		}

		protected virtual void OnEndDrag(PointerEventData eventData, Node node)
		{
			if (DraggingNode == null)
				return;

			if (DraggingNode != null)
			{
				if (HoveringNode != null)
				{
					// TODO:
				}
				DraggingNode.CanvasGroup.blocksRaycasts = true;
				DraggingNode = null;
			}
		}

		#endregion

		#region Node Hovering

		/// <summary>
		/// Currently hovering node that is beneath the mouse pointer.
		///
		/// Only set it manually if you really know what you are doing.
		/// </summary>
		[NonSerialized]
		public Node HoveringNode;

		protected virtual void OnPointerEnter(PointerEventData eventData, Node node)
		{
			HoveringNode = node;
		}

		protected virtual void OnPointerExit(PointerEventData eventData, Node node)
		{
			if (HoveringNode == node)
			{
				HoveringNode = null;
			}
		}

		#endregion

		#region Node Selection

		public Node SelectedNode { get; private set; }

		protected virtual void OnPointerClick(PointerEventData eventData, Node node)
		{
			if (node == SelectedNode)
				CollapseOrExpand(node);
			else
				Select(node);
		}

		public void Select(Node target)
		{
			if (SelectedNode != null && SelectedNode.Component)
			{
				SelectedNode.Component.OnItemDeselected();
			}

			SelectedNode = target;

			if (SelectedNode != null && SelectedNode.Component)
			{
				SelectedNode.Component.OnItemSelected();
			}
		}

		#endregion

		#region Expand / Collapse

		private bool IsAnimating;
		private readonly List<Node> CollapseChangeList = new List<Node>();

		public bool CollapseOrExpand(Node target)
		{
			if (IsAnimating)
				return true;

			if (target.HasChild)
			{
				if (target.Collapsed)
				{
					if (Expand(target))
						target.Component.OnItemExpanded();
				}
				else
				{
					if (Collapse(target))
						target.Component.OnItemCollapsed();
				}
			}
			return false;
		}

		public bool Expand(Node target)
		{
			if (IsAnimating)
				return false;
			IsAnimating = true;

			CollapseChangeList.Clear();
			target.Collapsed = false;
			GatherExpandingNodes(target, CollapseChangeList);
			ExpandOrCollapseAll(true, CollapseChangeList);
			CollapseChangeList.Clear();

			IsAnimating = false;
			return true;
		}

		public bool Collapse(Node target)
		{
			if (IsAnimating)
				return false;
			IsAnimating = true;

			CollapseChangeList.Clear();
			target.Collapsed = true;
			GatherAllChildren(target, CollapseChangeList);
			ExpandOrCollapseAll(false, CollapseChangeList);
			CollapseChangeList.Clear();

			IsAnimating = false;
			return true;
		}

		private void GatherExpandingNodes(Node target, List<Node> list)
		{
			if (target.Children != null && target.Children.Count > 0)
			{
				for (int i = 0; i < target.Children.Count; ++i)
				{
					var child = target.Children[i];
					list.Add(child);
					if (!child.Collapsed)
					{
						GatherExpandingNodes(child, list);
					}
				}
			}
		}

		private void ExpandOrCollapseAll(bool expandOrCollapse, List<Node> list)
		{
			for (int i = 0; i < list.Count; ++i)
			{
				var target = list[i];
				target.Component.gameObject.SetActive(expandOrCollapse);
			}
		}

		#endregion
	}

}
