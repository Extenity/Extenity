using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>
	/// The element of one field that has a rule: the field itself, an optional suffix, and the validation
	/// messages under it. Evaluating it applies visibility, enabled state, the dynamic label and validation.
	/// </summary>
	internal sealed class FieldSlot
	{
		public const string UssClassName = "ext-field";
		public const string InvalidUssClassName = UssClassName + "--invalid";
		public const string ReadOnlyUssClassName = "ext-readonly";

		public readonly VisualElement Element;

		private readonly InspectorInstance Instance;
		private readonly FieldRuleData Rule;
		private readonly VisualElement Field;
		private readonly VisualElement MessageContainer;
		private readonly string PropertyPath;
		private string InvalidSeverityClass;

		public FieldSlot(InspectorInstance instance, FieldRuleData rule, SerializedProperty property, VisualElement field)
		{
			Instance = instance;
			Rule = rule;
			Field = field;
			PropertyPath = property.propertyPath;

			Element = new VisualElement { name = "ext-field-" + rule.FieldName };
			Element.AddToClassList(UssClassName);

			if (!string.IsNullOrEmpty(rule.Suffix))
			{
				var row = new VisualElement();
				row.AddToClassList(UssClassName + "__row");
				field.AddToClassList(UssClassName + "__main");
				row.Add(field);
				var suffix = new Label(rule.Suffix);
				suffix.AddToClassList("ext-suffix");
				row.Add(suffix);
				Element.Add(row);
			}
			else
			{
				Element.Add(field);
			}

			if (rule.ReadOnly)
			{
				field.SetEnabled(false);
				Element.AddToClassList(ReadOnlyUssClassName);
			}
			if (rule.Hidden)
				Element.style.display = DisplayStyle.None;

			if (field is IValidationMessageHost host)
			{
				MessageContainer = host.ValidationMessageContainer;
			}
			else
			{
				MessageContainer = new VisualElement();
				Element.Add(MessageContainer);
			}
			MessageContainer.AddToClassList(UssClassName + "__messages");
		}

		public void Evaluate(EvaluationPass pass)
		{
			var isVisible = !Rule.Hidden && (Rule.VisibleRules.Count == 0 || pass.Any(IsVisibleFor));
			Element.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
			if (!isVisible)
			{
				ShowMessages(null);
				return;
			}

			if (!Rule.ReadOnly && Rule.EnabledRules.Count > 0)
				Field.SetEnabled(pass.All(IsEnabledFor));

			if (Rule.DynamicLabel != null && pass.Targets.Length > 0)
				FieldLabelTools.SetLabel(Field, Rule.DynamicLabel(pass.Targets[0]));

			List<ValidationResult> failures = null;
			foreach (var validator in Rule.Validators)
			{
				var result = pass.Worst(validator);
				if (result.IsOk)
					continue;
				failures ??= new List<ValidationResult>();
				failures.Add(result);
			}
			ShowMessages(failures);
		}

		private bool IsVisibleFor(object target, InspectorContext context)
		{
			foreach (var rule in Rule.VisibleRules)
			{
				if (!rule(target, context))
					return false;
			}
			return true;
		}

		private bool IsEnabledFor(object target, InspectorContext context)
		{
			foreach (var rule in Rule.EnabledRules)
			{
				if (!rule(target, context))
					return false;
			}
			return true;
		}

		private void ShowMessages(List<ValidationResult> failures)
		{
			var count = failures?.Count ?? 0;
			var worst = MessageSeverity.None;
			for (var i = 0; i < count; i++)
			{
				MessageBox box;
				if (i < MessageContainer.childCount)
				{
					box = (MessageBox)MessageContainer[i];
				}
				else
				{
					box = new MessageBox();
					MessageContainer.Add(box);
				}
				box.Severity = failures[i].Severity;
				box.Text = failures[i].Message;
				if (failures[i].Severity > worst)
					worst = failures[i].Severity;
				Instance.AddMessage(new ValidationMessage(failures[i].Severity, failures[i].Message, PropertyPath));
			}
			while (MessageContainer.childCount > count)
				MessageContainer.RemoveAt(MessageContainer.childCount - 1);

			var severityClass = worst == MessageSeverity.None ? null : InvalidUssClassName + "-" + worst.ToString().ToLowerInvariant();
			if (severityClass != InvalidSeverityClass)
			{
				if (InvalidSeverityClass != null)
					Element.RemoveFromClassList(InvalidSeverityClass);
				if (severityClass != null)
					Element.AddToClassList(severityClass);
				InvalidSeverityClass = severityClass;
			}
			Element.EnableInClassList(InvalidUssClassName, worst != MessageSeverity.None);
		}
	}

	/// <summary>Changes the label of a field element after it was created.</summary>
	public static class FieldLabelTools
	{
		private const string BaseFieldLabelClass = "unity-base-field__label";

		private static readonly System.Reflection.FieldInfo PropertyFieldLabelField =
			typeof(PropertyField).GetField("m_Label", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
			?? throw new System.MissingFieldException(nameof(PropertyField), "m_Label");

		public static void SetLabel(VisualElement field, string text)
		{
			text ??= "";
			if (field is PropertyField propertyField)
			{
				if (propertyField.label == text)
					return;
				if (propertyField.childCount == 0)
				{
					propertyField.label = text;
					return;
				}
				// Unity's setter binds the field again, which builds its content anew: a text field that is being
				// typed in (and that the label is made of) would lose the keyboard focus with every key. So the
				// label is stored for a later bind, and what is built already is updated in place.
				PropertyFieldLabelField.SetValue(propertyField, text);
				foreach (var child in propertyField.Children())
				{
					if (child is Foldout foldout)
					{
						foldout.text = text;
						return;
					}
					if (TrySetOwnLabel(child, text))
						return;
				}
				return;
			}
			if (field is Foldout ownFoldout)
			{
				ownFoldout.text = text;
				return;
			}
			TrySetOwnLabel(field, text);
		}

		private const string AlignedFieldClass = "unity-base-field__aligned";
		private const string ReleasedFieldClass = "ext-field--alignment-released";

		/// <summary>
		/// Stops Unity from sizing the label of a field to the inspector's label column, so that style sheets
		/// decide its width. Unity sets that width inline, where no style sheet can override it.
		/// Returns false when the element is not a field that is aligned to the label column.
		/// </summary>
		public static bool ReleaseLabelAlignment(VisualElement field)
		{
			if (!field.ClassListContains(AlignedFieldClass))
				return false;
			field.RemoveFromClassList(AlignedFieldClass);
			field.AddToClassList(ReleasedFieldClass);
			foreach (var child in field.Children())
			{
				if (child is Label label && label.ClassListContains(BaseFieldLabelClass))
				{
					label.style.width = StyleKeyword.Null;
					label.style.minWidth = StyleKeyword.Null;
					break;
				}
			}
			return true;
		}

		/// <summary>Undoes <see cref="ReleaseLabelAlignment"/>. Unity sizes the label again at the next layout change of the field.</summary>
		public static void RestoreLabelAlignment(VisualElement field)
		{
			if (!field.ClassListContains(ReleasedFieldClass))
				return;
			field.RemoveFromClassList(ReleasedFieldClass);
			field.AddToClassList(AlignedFieldClass);
		}

		/// <summary>Releases the label alignment of every aligned field inside the element, now and as fields appear in it.</summary>
		public static void KeepLabelsUnaligned(VisualElement container)
		{
			void Release()
			{
				container.Query<VisualElement>(className: AlignedFieldClass).ForEach(field => ReleaseLabelAlignment(field));
			}
			container.RegisterCallback<GeometryChangedEvent>(_ => Release());
			Release();
		}

		private static bool TrySetOwnLabel(VisualElement field, string text)
		{
			foreach (var child in field.Children())
			{
				if (child is Label label && label.ClassListContains(BaseFieldLabelClass))
				{
					label.text = text;
					return true;
				}
			}
			return false;
		}
	}

	/// <summary>Draws a field without its own foldout and label.</summary>
	public static class InlineFieldFactory
	{
		public const string UssClassName = "ext-inline";

		/// <summary>
		/// A value with a property drawer is drawn by that drawer, asked for no label. Any other class or struct
		/// has its children drawn directly. <paramref name="valueType"/> is the declared type of the field.
		/// </summary>
		public static VisualElement Create(SerializedProperty property, System.Type valueType)
		{
			var isPlainComposite = property.propertyType == SerializedPropertyType.Generic && !property.isArray;
			if (!isPlainComposite || (valueType != null && PropertyDrawerLookup.HasDrawer(valueType)))
				return new PropertyField(property, "");

			var container = new VisualElement { name = "ext-inline-" + property.name };
			container.AddToClassList(UssClassName);
			var end = property.GetEndProperty();
			var iterator = property.Copy();
			if (iterator.NextVisible(true))
			{
				while (!SerializedProperty.EqualContents(iterator, end))
				{
					container.Add(new PropertyField(iterator.Copy()));
					if (!iterator.NextVisible(false))
						break;
				}
			}
			return container;
		}
	}

}
