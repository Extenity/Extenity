using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>
	/// Remembers whether a foldout that has no property of its own is open: group foldouts, live lists, inline
	/// object editors. Kept for the editor session, per target type and title.
	/// </summary>
	public static class FoldoutStates
	{
		private const string KeyPrefix = "Extenity.InspectorToolbox.Foldout|";

		public static bool Get(Type targetType, string title, bool defaultValue)
		{
			return SessionState.GetBool(MakeKey(targetType, title), defaultValue);
		}

		public static void Set(Type targetType, string title, bool expanded)
		{
			SessionState.SetBool(MakeKey(targetType, title), expanded);
		}

		private static string MakeKey(Type targetType, string title)
		{
			return KeyPrefix + targetType.FullName + "|" + title;
		}
	}

	/// <summary>Fields and nodes drawn together under a title, in a box, in a foldout or in an accent-coloured group.</summary>
	public sealed class GroupNode<T> : ContainerNode<T, GroupNode<T>>
	{
		public string Title;
		public GroupStyle Style;
		public Color? AccentColor;
		public bool IsCollapsedByDefault;
		/// <summary>A group without any decoration: only a container. Used for the pages of tabs.</summary>
		public bool IsPlain;
		public readonly List<Func<object, InspectorContext, bool>> VisibleRules = new List<Func<object, InspectorContext, bool>>();
		public readonly List<Action<object>> AnyChangedHandlers = new List<Action<object>>();

		public override VisualElement Build(InspectorInstance instance)
		{
			var targetType = instance.Blueprint.TargetType;
			var isFoldout = Style == GroupStyle.Foldout && !IsPlain;
			var expanded = !isFoldout || FoldoutStates.Get(targetType, Title, !IsCollapsedByDefault);

			VisualElement group;
			Foldout foldout = null;
			if (IsPlain)
			{
				group = new VisualElement();
			}
			else if (AccentColor.HasValue)
			{
				var accentGroup = new AccentGroup(Title, AccentColor.Value, isFoldout, expanded);
				foldout = accentGroup.FoldoutElement;
				group = accentGroup;
			}
			else
			{
				switch (Style)
				{
					case GroupStyle.Title:
						group = new TitleGroup(Title);
						break;
					case GroupStyle.Box:
						group = new BoxGroup(Title);
						break;
					case GroupStyle.Foldout:
						foldout = new Foldout { text = Title, value = expanded };
						foldout.AddToClassList("ext-group-foldout");
						group = foldout;
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(Style), Style, null);
				}
			}
			group.AddToClassList("ext-group");
			group.name = "ext-group-" + Title;

			if (foldout != null)
			{
				var title = Title;
				foldout.RegisterValueChangedCallback(evt =>
				{
					if (evt.target == foldout)
						FoldoutStates.Set(targetType, title, evt.newValue);
				});
			}

			BuildEntries(instance, group);

			if (VisibleRules.Count > 0)
			{
				instance.OnEvaluate(pass =>
				{
					var isVisible = pass.Any((target, context) =>
					{
						foreach (var rule in VisibleRules)
						{
							if (!rule(target, context))
								return false;
						}
						return true;
					});
					group.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
				});
			}

			if (AnyChangedHandlers.Count > 0)
			{
				foreach (var fieldName in ClaimedFieldNames)
				{
					instance.TrackField(group, fieldName, () => instance.Mutate(null, false, target =>
					{
						foreach (var handler in AnyChangedHandlers)
							handler(target);
						return target;
					}));
				}
			}
			return group;
		}
	}

	/// <summary>Fills a group. Everything that can be added to an inspector can be added to a group.</summary>
	public sealed class GroupBuilder<T> : IInspectorNodeContainer<T>
	{
		public GroupNode<T> Node { get; }
		public InspectorBuilder<T> Builder { get; }

		internal GroupBuilder(InspectorBuilder<T> builder, GroupNode<T> node)
		{
			Builder = builder;
			Node = node;
		}

		/// <summary>Puts the fields into the group, in this order.</summary>
		public GroupBuilder<T> Fields(params Expression<Func<T, object>>[] fields)
		{
			foreach (var field in fields)
				Claim(FieldNames.Get(field));
			return this;
		}

		/// <summary>Puts the field into the group and returns its rule.</summary>
		public FieldRule<T, TField> Field<TField>(Expression<Func<T, TField>> field)
		{
			Claim(FieldNames.Get(field));
			return Builder.Field(field);
		}

		public FieldRule<T, object> Field(string serializedFieldName)
		{
			Claim(serializedFieldName);
			return Builder.Field(serializedFieldName);
		}

		public ListRule<T, TElement> List<TElement>(Expression<Func<T, IList<TElement>>> list)
		{
			Claim(FieldNames.Get(list));
			return Builder.List(list);
		}

		private void Claim(string fieldName)
		{
			// Creates the rule, which also checks that the field exists.
			Builder.Field(fieldName);
			Node.Entries.Add(new LayoutEntry(fieldName));
		}

		public TNode Add<TNode>(TNode node) where TNode : InspectorNode
		{
			if (node == null)
				throw new ArgumentNullException(nameof(node));
			Node.Entries.Add(new LayoutEntry(node));
			return node;
		}

		/// <summary>Draws the group with a coloured bar, a tinted header and a faintly tinted background.</summary>
		public GroupBuilder<T> Accent(Color color)
		{
			Node.AccentColor = color;
			return this;
		}

		/// <summary>A foldout group starts closed, until the user opens it.</summary>
		public GroupBuilder<T> CollapsedByDefault()
		{
			Node.IsCollapsedByDefault = true;
			return this;
		}

		public GroupBuilder<T> VisibleWhen(Func<T, bool> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));
			Node.VisibleRules.Add((target, context) => predicate((T)target));
			return this;
		}

		public GroupBuilder<T> VisibleWhen(Func<T, InspectorContext, bool> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));
			Node.VisibleRules.Add((target, context) => predicate((T)target, context));
			return this;
		}

		/// <summary>Runs after any field of the group changed. See <c>OnChanged</c> of a field rule for what the action gets.</summary>
		public GroupBuilder<T> OnAnyChanged(Action<T> afterApply)
		{
			if (afterApply == null)
				throw new ArgumentNullException(nameof(afterApply));
			Node.AnyChangedHandlers.Add(target => afterApply((T)target));
			return this;
		}

		public GroupBuilder<T> AtTop()
		{
			Node.AtTop();
			return this;
		}

		public GroupBuilder<T> AtBottom()
		{
			Node.AtBottom();
			return this;
		}

		public GroupBuilder<T> Before<TField>(Expression<Func<T, TField>> field)
		{
			Node.Before(field);
			return this;
		}

		public GroupBuilder<T> After<TField>(Expression<Func<T, TField>> field)
		{
			Node.After(field);
			return this;
		}
	}

	public static class GroupFeature
	{
		/// <summary>
		/// A group of fields and nodes. It is drawn where the first of its fields is declared, or at the bottom
		/// when it has no fields, unless it is placed explicitly.
		/// </summary>
		public static GroupBuilder<T> Group<T>(this IInspectorNodeContainer<T> container, string title, GroupStyle style, Action<GroupBuilder<T>> content)
		{
			if (string.IsNullOrEmpty(title))
				throw new ArgumentException("A group needs a title.", nameof(title));
			if (content == null)
				throw new ArgumentNullException(nameof(content));
			var node = container.Add(new GroupNode<T> { Title = title, Style = style });
			var group = new GroupBuilder<T>(container.Builder, node);
			content(group);
			return group;
		}
	}

}
