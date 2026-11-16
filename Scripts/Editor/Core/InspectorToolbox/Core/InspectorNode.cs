using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine.UIElements;

namespace Extenity.InspectorToolbox.Editor
{

	public enum PlacementKind
	{
		/// <summary>A field stays where it is declared. A node goes to the bottom, in call order.</summary>
		Default,
		AtTop,
		AtBottom,
		Before,
		After,
	}

	/// <summary>Where a field or a node is drawn when it should not be at its default place.</summary>
	public readonly struct Placement
	{
		public readonly PlacementKind Kind;
		public readonly string AnchorFieldName;

		public Placement(PlacementKind kind, string anchorFieldName = null)
		{
			Kind = kind;
			AnchorFieldName = anchorFieldName;
		}
	}

	/// <summary>A field by name, or a node. The unit a layout is made of.</summary>
	public readonly struct LayoutEntry
	{
		public readonly string FieldName;
		public readonly InspectorNode Node;

		public LayoutEntry(string fieldName)
		{
			FieldName = fieldName;
			Node = null;
		}

		public LayoutEntry(InspectorNode node)
		{
			FieldName = null;
			Node = node;
		}

		public bool IsField => FieldName != null;
	}

	/// <summary>
	/// Anything an inspector shows that is not a plain serialized field: a message, a button, a live value,
	/// a group of fields. Deriving from this class and adding the node with <c>Add</c> is how new building blocks
	/// are made without touching the core.
	/// </summary>
	public abstract class InspectorNode
	{
		public Placement Placement { get; set; }

		/// <summary>The serialized fields this node draws itself. A field can be claimed by one node only.</summary>
		public virtual IEnumerable<string> ClaimedFieldNames => Array.Empty<string>();

		/// <summary>
		/// Creates the element of this node for one inspector instance. Called once per instance. Register what
		/// must be kept up to date with <see cref="InspectorInstance.OnEvaluate"/> or
		/// <see cref="InspectorInstance.OnRefreshLive"/>.
		/// </summary>
		public abstract VisualElement Build(InspectorInstance instance);
	}

	/// <summary>Adds the fluent placement methods to a node type.</summary>
	public abstract class InspectorNode<T, TSelf> : InspectorNode where TSelf : InspectorNode<T, TSelf>
	{
		public TSelf AtTop()
		{
			Placement = new Placement(PlacementKind.AtTop);
			return (TSelf)this;
		}

		public TSelf AtBottom()
		{
			Placement = new Placement(PlacementKind.AtBottom);
			return (TSelf)this;
		}

		public TSelf Before<TField>(Expression<Func<T, TField>> field)
		{
			Placement = new Placement(PlacementKind.Before, FieldNames.Get(field));
			return (TSelf)this;
		}

		public TSelf After<TField>(Expression<Func<T, TField>> field)
		{
			Placement = new Placement(PlacementKind.After, FieldNames.Get(field));
			return (TSelf)this;
		}

		public TSelf Before(string serializedFieldName)
		{
			Placement = new Placement(PlacementKind.Before, serializedFieldName);
			return (TSelf)this;
		}

		public TSelf After(string serializedFieldName)
		{
			Placement = new Placement(PlacementKind.After, serializedFieldName);
			return (TSelf)this;
		}
	}

	/// <summary>
	/// Something nodes can be added to: the builder itself, a group, a row, a tab. Every building block is an
	/// extension method on this interface, so it is available at each of these levels.
	/// </summary>
	public interface IInspectorNodeContainer<T>
	{
		/// <summary>The builder of the inspector this container belongs to.</summary>
		InspectorBuilder<T> Builder { get; }

		/// <summary>Adds a node at the end of this container and returns it.</summary>
		TNode Add<TNode>(TNode node) where TNode : InspectorNode;
	}

	/// <summary>
	/// A node that holds fields and other nodes, drawn in the order they were added.
	/// </summary>
	public abstract class ContainerNode<T, TSelf> : InspectorNode<T, TSelf> where TSelf : ContainerNode<T, TSelf>
	{
		public List<LayoutEntry> Entries { get; } = new List<LayoutEntry>();

		public override IEnumerable<string> ClaimedFieldNames
		{
			get
			{
				foreach (var entry in Entries)
				{
					if (entry.IsField)
					{
						yield return entry.FieldName;
					}
					else
					{
						foreach (var name in entry.Node.ClaimedFieldNames)
							yield return name;
					}
				}
			}
		}

		/// <summary>Builds every entry into <paramref name="parent"/>, in order.</summary>
		protected void BuildEntries(InspectorInstance instance, VisualElement parent, Action<LayoutEntry, VisualElement> onBuilt = null)
		{
			foreach (var entry in Entries)
			{
				var element = entry.IsField ? instance.BuildField(entry.FieldName) : entry.Node.Build(instance);
				if (element == null)
					throw new InvalidOperationException($"The node '{entry.Node?.GetType().Name}' built no element.");
				parent.Add(element);
				onBuilt?.Invoke(entry, element);
			}
		}
	}

}
