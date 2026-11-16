using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>A message box in the inspector, with fixed or computed text.</summary>
	public sealed class MessageNode<T> : InspectorNode<T, MessageNode<T>>
	{
		public string Text;
		public Func<object, string> DynamicText;
		public MessageSeverity Severity;
		public readonly List<Func<object, InspectorContext, bool>> VisibleRules = new List<Func<object, InspectorContext, bool>>();

		public MessageNode<T> VisibleWhen(Func<T, bool> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));
			VisibleRules.Add((target, context) => predicate((T)target));
			return this;
		}

		public MessageNode<T> VisibleWhen(Func<T, InspectorContext, bool> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));
			VisibleRules.Add((target, context) => predicate((T)target, context));
			return this;
		}

		public override VisualElement Build(InspectorInstance instance)
		{
			var box = new MessageBox(Text, Severity);
			if (DynamicText == null && VisibleRules.Count == 0)
				return box;

			instance.OnEvaluate(pass =>
			{
				var isVisible = VisibleRules.Count == 0 || pass.Any((target, context) =>
				{
					foreach (var rule in VisibleRules)
					{
						if (!rule(target, context))
							return false;
					}
					return true;
				});
				if (isVisible && DynamicText != null)
				{
					// With several objects selected the first one that has a text decides.
					string text = null;
					foreach (var target in pass.Targets)
					{
						text = DynamicText(target);
						if (!string.IsNullOrEmpty(text))
							break;
					}
					box.Text = text;
					isVisible = !string.IsNullOrEmpty(text);
				}
				box.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
			});
			return box;
		}
	}

	public static class InfoFeature
	{
		public static MessageNode<T> Info<T>(this IInspectorNodeContainer<T> container, string text, MessageSeverity severity = MessageSeverity.Info)
		{
			if (string.IsNullOrEmpty(text))
				throw new ArgumentException("A message needs a text.", nameof(text));
			return container.Add(new MessageNode<T> { Text = text, Severity = severity });
		}

		/// <summary>A message whose text is computed from the target. A null or empty text hides it.</summary>
		public static MessageNode<T> Info<T>(this IInspectorNodeContainer<T> container, Func<T, string> text, MessageSeverity severity = MessageSeverity.Info)
		{
			if (text == null)
				throw new ArgumentNullException(nameof(text));
			return container.Add(new MessageNode<T> { DynamicText = target => text((T)target), Severity = severity });
		}
	}

}
