using System;
using System.Linq.Expressions;
using UnityEngine.UIElements;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>Fields, buttons and labels next to each other.</summary>
	public sealed class RowNode<T> : ContainerNode<T, RowNode<T>>
	{
		public const string UssClassName = "ext-row";

		public override VisualElement Build(InspectorInstance instance)
		{
			var row = new VisualElement();
			row.AddToClassList(UssClassName);

			// With something that grows in the row, everything else keeps its natural width.
			// Otherwise the entries share the width equally.
			var hasGrowingEntry = false;
			foreach (var entry in Entries)
			{
				if (entry.IsField && instance.Blueprint.RulesByName[entry.FieldName].Grow)
					hasGrowingEntry = true;
				if (entry.Node is RowSpacerNode)
					hasGrowingEntry = true;
			}

			var hasField = false;
			foreach (var entry in Entries)
			{
				if (entry.IsField)
					hasField = true;
			}
			if (!hasField)
				row.AddToClassList(UssClassName + "--no-fields");

			var isFirstField = true;
			BuildEntries(instance, row, (entry, element) =>
			{
				element.AddToClassList(UssClassName + "__item");
				if (entry.IsField)
				{
					var rule = instance.Blueprint.RulesByName[entry.FieldName];
					if (rule.Width > 0f)
					{
						element.style.width = rule.Width;
						element.style.flexGrow = 0f;
						element.style.flexShrink = 0f;
					}
					else if (rule.Grow || !hasGrowingEntry)
					{
						element.style.flexGrow = 1f;
						element.style.flexBasis = 0f;
					}
					if (!isFirstField || rule.Width > 0f)
					{
						// Only the first field of a row uses the inspector's label column.
						element.AddToClassList(UssClassName + "__item--compact");
						FieldLabelTools.KeepLabelsUnaligned(element);
					}
					isFirstField = false;
				}
				else if (entry.Node is RowSpacerNode)
				{
					element.style.flexGrow = 1f;
				}
				else if (!hasGrowingEntry && !(entry.Node is RowLabelNode))
				{
					element.style.flexGrow = 1f;
					element.style.flexBasis = 0f;
				}
			});
			return row;
		}
	}

	internal sealed class RowLabelNode : InspectorNode
	{
		public string Text;

		public override VisualElement Build(InspectorInstance instance)
		{
			var label = new Label(Text);
			label.AddToClassList(RowNode<object>.UssClassName + "__label");
			return label;
		}
	}

	internal sealed class RowSpacerNode : InspectorNode
	{
		public override VisualElement Build(InspectorInstance instance)
		{
			return new VisualElement { pickingMode = PickingMode.Ignore };
		}
	}

	/// <summary>Fills a row, from left to right.</summary>
	public sealed class RowBuilder<T> : IInspectorNodeContainer<T>
	{
		public RowNode<T> Node { get; }
		public InspectorBuilder<T> Builder { get; }

		internal RowBuilder(InspectorBuilder<T> builder, RowNode<T> node)
		{
			Builder = builder;
			Node = node;
		}

		/// <summary>Puts the field into the row and returns its rule. Use Width or Grow on the rule to size it.</summary>
		public FieldRule<T, TField> Field<TField>(Expression<Func<T, TField>> field)
		{
			Node.Entries.Add(new LayoutEntry(FieldNames.Get(field)));
			return Builder.Field(field);
		}

		public FieldRule<T, object> Field(string serializedFieldName)
		{
			var rule = Builder.Field(serializedFieldName);
			Node.Entries.Add(new LayoutEntry(serializedFieldName));
			return rule;
		}

		public RowBuilder<T> Label(string text)
		{
			Node.Entries.Add(new LayoutEntry(new RowLabelNode { Text = text }));
			return this;
		}

		/// <summary>An empty stretch that pushes what follows to the right.</summary>
		public RowBuilder<T> Spacer()
		{
			Node.Entries.Add(new LayoutEntry(new RowSpacerNode()));
			return this;
		}

		public TNode Add<TNode>(TNode node) where TNode : InspectorNode
		{
			if (node == null)
				throw new ArgumentNullException(nameof(node));
			Node.Entries.Add(new LayoutEntry(node));
			return node;
		}
	}

	public static class RowFeature
	{
		/// <summary>
		/// A row of fields, buttons and labels. It is drawn where the first of its fields is declared, or at the
		/// bottom when it has no fields, unless the returned node is placed explicitly.
		/// </summary>
		public static RowNode<T> Row<T>(this IInspectorNodeContainer<T> container, Action<RowBuilder<T>> content)
		{
			if (content == null)
				throw new ArgumentNullException(nameof(content));
			var node = container.Add(new RowNode<T>());
			content(new RowBuilder<T>(container.Builder, node));
			return node;
		}
	}

}
