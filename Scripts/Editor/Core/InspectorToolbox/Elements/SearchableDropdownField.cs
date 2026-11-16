using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>
	/// A field that picks its value from a list of labelled choices in a searchable popup. It looks like
	/// Unity's popup field. Bound through <c>bindingPath</c> it supports the value types Unity can bind: string,
	/// int, long, float, double and object references.
	/// <para>
	/// A current value that is not among the choices is shown as it is, marked as unknown, and is never
	/// rewritten by the field.
	/// </para>
	/// </summary>
	public class SearchableDropdownField<TValue> : BaseField<TValue>
	{
		public new const string ussClassName = "ext-dropdown";
		public const string UnknownUssClassName = ussClassName + "--unknown";

		private readonly Func<IReadOnlyList<DropdownItem<TValue>>> Items;
		private readonly DropdownOptions Options;
		private readonly TextElement TextElement;
		private readonly VisualElement Input;

		/// <summary>True when the current value is not among the choices (and is not the empty choice).</summary>
		public bool IsValueUnknown { get; private set; }

		/// <summary>The text shown for the current value.</summary>
		public string DisplayText => TextElement.text;

		public SearchableDropdownField(string label, Func<IReadOnlyList<DropdownItem<TValue>>> items, DropdownOptions options = default)
			: this(label, items, options, new VisualElement())
		{
		}

		private SearchableDropdownField(string label, Func<IReadOnlyList<DropdownItem<TValue>>> items, DropdownOptions options, VisualElement input)
			: base(label, input)
		{
			Items = items ?? throw new ArgumentNullException(nameof(items));
			Options = options;
			Input = input;

			AddToClassList(ussClassName);
			AddToClassList("unity-base-popup-field");
			AddToClassList(alignedFieldUssClassName);
			labelElement.AddToClassList("unity-base-popup-field__label");

			input.AddToClassList("unity-base-popup-field__input");
			input.AddToClassList(ussClassName + "__input");
			input.focusable = true;
			TextElement = new TextElement { pickingMode = PickingMode.Ignore };
			TextElement.AddToClassList("unity-base-popup-field__text");
			input.Add(TextElement);
			var arrow = new VisualElement { pickingMode = PickingMode.Ignore };
			arrow.AddToClassList("unity-base-popup-field__arrow");
			input.Add(arrow);

			input.RegisterCallback<ClickEvent>(evt =>
			{
				ShowPicker();
				evt.StopPropagation();
			});
			input.RegisterCallback<KeyDownEvent>(evt =>
			{
				if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Space)
				{
					ShowPicker();
					evt.StopPropagation();
				}
			});
			RegisterCallback<AttachToPanelEvent>(_ => RefreshDisplay());
			RefreshDisplay();
		}

		public override void SetValueWithoutNotify(TValue newValue)
		{
			base.SetValueWithoutNotify(newValue);
			// TextElement is null while the base constructor runs.
			if (TextElement != null)
				RefreshDisplay();
		}

		protected override void UpdateMixedValueContent()
		{
			RefreshDisplay();
		}

		/// <summary>Reads the choices again and updates the shown text. Call it when the choices changed.</summary>
		public void RefreshDisplay()
		{
			if (showMixedValue)
			{
				TextElement.text = "—";
				SetUnknown(false);
				return;
			}

			var current = value;
			foreach (var item in Items())
			{
				if (AreEqual(item.Value, current))
				{
					TextElement.text = item.Label;
					SetUnknown(false);
					return;
				}
			}
			if (IsEmpty(current))
			{
				TextElement.text = Options.EmptyLabel ?? "";
				// Without an empty choice an empty value is as unknown as any other value that is not listed.
				SetUnknown(Options.EmptyLabel == null);
				return;
			}
			TextElement.text = $"{Describe(current)} (not in list)";
			SetUnknown(true);
		}

		private void SetUnknown(bool isUnknown)
		{
			IsValueUnknown = isUnknown;
			EnableInClassList(UnknownUssClassName, isUnknown);
			Input.tooltip = isUnknown ? "The current value is not one of the choices. It is kept until another one is picked." : "";
		}

		/// <summary>The choices as the popup lists them: the empty choice first when there is one, sorted when asked for.</summary>
		public List<DropdownItem<TValue>> GetChoices()
		{
			IEnumerable<DropdownItem<TValue>> items = Items();
			if (Options.Sorted)
				items = items.OrderBy(item => item.Group ?? "", StringComparer.OrdinalIgnoreCase).ThenBy(item => item.Label, StringComparer.OrdinalIgnoreCase);
			var choices = new List<DropdownItem<TValue>>();
			if (Options.EmptyLabel != null)
				choices.Add(new DropdownItem<TValue>(Options.EmptyLabel, default));
			choices.AddRange(items);
			return choices;
		}

		private void ShowPicker()
		{
			var choices = GetChoices();
			var boxed = new List<DropdownItem<object>>(choices.Count);
			foreach (var choice in choices)
				boxed.Add(new DropdownItem<object>(choice.Label, choice.Value, choice.Group));
			SearchablePicker.Show(Input.worldBound, boxed, picked => value = picked == null ? default : (TValue)picked, label);
		}

		private static bool AreEqual(TValue a, TValue b)
		{
			if (IsEmpty(a) && IsEmpty(b))
				return true;
			return EqualityComparer<TValue>.Default.Equals(a, b);
		}

		private static bool IsEmpty(TValue candidate)
		{
			object boxed = candidate;
			if (boxed == null)
				return true;
			if (boxed is string text)
				return text.Length == 0;
			if (boxed is UnityEngine.Object unityObject)
				return unityObject == null;
			return EqualityComparer<TValue>.Default.Equals(candidate, default);
		}

		private static string Describe(TValue candidate)
		{
			object boxed = candidate;
			if (boxed is UnityEngine.Object unityObject)
				return unityObject.name;
			return Convert.ToString(boxed, System.Globalization.CultureInfo.InvariantCulture);
		}
	}

	/// <summary>
	/// Creates the dropdown field for a property: picks the value type Unity can bind for it and looks through
	/// wrapper structs that hold a single value.
	/// </summary>
	public static class DropdownFieldFactory
	{
		/// <param name="label">Null for the property's own name, empty for no label.</param>
		/// <param name="items">The choices, with values of the field's own type (the wrapper struct for a wrapped value).</param>
		public static VisualElement Create(SerializedProperty property, string label, Func<IEnumerable<DropdownItem<object>>> items, DropdownOptions options)
		{
			if (property == null)
				throw new ArgumentNullException(nameof(property));
			if (items == null)
				throw new ArgumentNullException(nameof(items));

			// A wrapper struct with a single value is edited through that value.
			var raw = property.Copy();
			var wrapperFieldNames = new List<string>();
			while (raw.propertyType == SerializedPropertyType.Generic && !raw.isArray)
			{
				var end = raw.GetEndProperty();
				var child = raw.Copy();
				var childCount = 0;
				SerializedProperty onlyChild = null;
				if (child.NextVisible(true))
				{
					while (!SerializedProperty.EqualContents(child, end))
					{
						childCount++;
						onlyChild ??= child.Copy();
						if (!child.NextVisible(false))
							break;
					}
				}
				if (childCount != 1)
					throw new NotSupportedException($"A dropdown cannot edit '{property.propertyPath}': '{raw.type}' has {childCount} serialized fields, and only a struct that wraps a single value can be edited as one value.");
				wrapperFieldNames.Add(onlyChild.name);
				raw = onlyChild;
			}

			var displayLabel = label ?? property.displayName;
			switch (raw.propertyType)
			{
				case SerializedPropertyType.String:
					return Make<string>(raw, displayLabel, items, options, wrapperFieldNames);
				case SerializedPropertyType.Integer:
					return raw.numericType == SerializedPropertyNumericType.Int64 || raw.numericType == SerializedPropertyNumericType.UInt64 || raw.numericType == SerializedPropertyNumericType.UInt32
						? Make<long>(raw, displayLabel, items, options, wrapperFieldNames)
						: Make<int>(raw, displayLabel, items, options, wrapperFieldNames);
				case SerializedPropertyType.Float:
					return raw.numericType == SerializedPropertyNumericType.Double
						? Make<double>(raw, displayLabel, items, options, wrapperFieldNames)
						: Make<float>(raw, displayLabel, items, options, wrapperFieldNames);
				case SerializedPropertyType.ObjectReference:
					return Make<UnityEngine.Object>(raw, displayLabel, items, options, wrapperFieldNames);
				default:
					throw new NotSupportedException($"A dropdown cannot edit '{property.propertyPath}' of type '{raw.propertyType}'. Supported: string, integer, float and object reference fields, and structs that wrap one of those.");
			}
		}

		private static VisualElement Make<TRaw>(SerializedProperty raw, string label, Func<IEnumerable<DropdownItem<object>>> items, DropdownOptions options, List<string> wrapperFieldNames)
		{
			var field = new SearchableDropdownField<TRaw>(label, () =>
			{
				var converted = new List<DropdownItem<TRaw>>();
				foreach (var item in items())
					converted.Add(new DropdownItem<TRaw>(item.Label, ToRaw<TRaw>(item.Value, wrapperFieldNames), item.Group));
				return converted;
			}, options)
			{
				bindingPath = raw.propertyPath,
			};
			return field;
		}

		private static TRaw ToRaw<TRaw>(object value, List<string> wrapperFieldNames)
		{
			foreach (var fieldName in wrapperFieldNames)
			{
				if (value == null)
					break;
				if (!FieldNames.TryFind(value.GetType(), fieldName, out var field))
					throw new InvalidOperationException($"A dropdown item of type '{value.GetType().FullName}' has no field '{fieldName}' to read the wrapped value from.");
				value = field.GetValue(value);
			}
			if (value == null)
				return default;
			if (value is TRaw raw)
				return raw;
			try
			{
				return (TRaw)Convert.ChangeType(value, typeof(TRaw), System.Globalization.CultureInfo.InvariantCulture);
			}
			catch (Exception exception) when (exception is InvalidCastException || exception is FormatException || exception is OverflowException)
			{
				throw new InvalidOperationException($"A dropdown item value of type '{value.GetType().FullName}' cannot be stored in a field of type '{typeof(TRaw).Name}'.", exception);
			}
		}
	}

}
