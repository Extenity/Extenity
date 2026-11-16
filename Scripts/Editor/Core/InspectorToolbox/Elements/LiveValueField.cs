using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>
	/// A labelled read-only value that is meant to change while it is looked at. The label lines up with the
	/// labels of Unity's fields. The text can be selected. With a range the value is also drawn as a thin bar.
	/// </summary>
	public class LiveValueField : BaseField<string>
	{
		public new const string ussClassName = "ext-live";
		public const string PlaceholderUssClassName = ussClassName + "--placeholder";
		public const string ErrorUssClassName = ussClassName + "--error";
		public const string MultilineUssClassName = ussClassName + "--multiline";

		private readonly VisualElement Bar;
		private readonly VisualElement BarFill;
		private readonly Label TextLabel;

		public LiveValueField(string label) : this(label, new VisualElement())
		{
		}

		private LiveValueField(string label, VisualElement input) : base(label, input)
		{
			AddToClassList(ussClassName);
			AddToClassList(alignedFieldUssClassName);
			input.AddToClassList(ussClassName + "__input");

			Bar = new VisualElement { pickingMode = PickingMode.Ignore };
			Bar.AddToClassList(ussClassName + "__bar");
			Bar.style.display = DisplayStyle.None;
			BarFill = new VisualElement { pickingMode = PickingMode.Ignore };
			BarFill.AddToClassList(ussClassName + "__bar-fill");
			Bar.Add(BarFill);
			input.Add(Bar);

			TextLabel = new Label();
			TextLabel.selection.isSelectable = true;
			TextLabel.AddToClassList(ussClassName + "__text");
			input.Add(TextLabel);
		}

		public override void SetValueWithoutNotify(string newValue)
		{
			base.SetValueWithoutNotify(newValue);
			if (TextLabel != null)
				TextLabel.text = newValue ?? "";
		}

		public bool IsMultiline
		{
			get => ClassListContains(MultilineUssClassName);
			set => EnableInClassList(MultilineUssClassName, value);
		}

		/// <summary>Shows a value.</summary>
		public void ShowValue(string text)
		{
			RemoveFromClassList(PlaceholderUssClassName);
			RemoveFromClassList(ErrorUssClassName);
			SetValueWithoutNotify(text);
		}

		/// <summary>Shows a muted note in place of a value, like "Play Mode only".</summary>
		public void ShowPlaceholder(string text)
		{
			AddToClassList(PlaceholderUssClassName);
			RemoveFromClassList(ErrorUssClassName);
			Bar.style.display = DisplayStyle.None;
			SetValueWithoutNotify(text);
		}

		/// <summary>Shows that the value could not be read.</summary>
		public void ShowError(string text)
		{
			RemoveFromClassList(PlaceholderUssClassName);
			AddToClassList(ErrorUssClassName);
			Bar.style.display = DisplayStyle.None;
			SetValueWithoutNotify(text);
		}

		/// <summary>Draws the value as a bar between the two ends of the range. Call <see cref="HideBar"/> to remove it.</summary>
		public void ShowBar(float current, float min, float max)
		{
			BarMin = min;
			BarMax = max;
			BarFraction = Mathf.Approximately(max, min) ? 0f : Mathf.Clamp01((current - min) / (max - min));
			Bar.style.display = DisplayStyle.Flex;
			var fraction = Mathf.Approximately(max, min) ? 0f : Mathf.Clamp01((current - min) / (max - min));
			BarFill.style.width = Length.Percent(fraction * 100f);
		}

		public void HideBar()
		{
			Bar.style.display = DisplayStyle.None;
		}

		/// <summary>True while the value is drawn as a bar.</summary>
		public bool IsBarShown => Bar.style.display == DisplayStyle.Flex;

		/// <summary>The range of the bar that was shown last, and how much of it was filled (0 to 1).</summary>
		public float BarMin { get; private set; }
		public float BarMax { get; private set; }
		public float BarFraction { get; private set; }

		/// <summary>The colour set with <see cref="SetTint"/>, null while the default colours are used.</summary>
		public Color? Tint { get; private set; }

		/// <summary>The text that is shown: a value, a placeholder or an error.</summary>
		public string Text => value;

		/// <summary>Colours the text and the bar. Null goes back to the default colours.</summary>
		public void SetTint(Color? tint)
		{
			Tint = tint;
			if (tint.HasValue)
			{
				TextLabel.style.color = tint.Value;
				BarFill.style.backgroundColor = tint.Value;
			}
			else
			{
				TextLabel.style.color = StyleKeyword.Null;
				BarFill.style.backgroundColor = StyleKeyword.Null;
			}
		}

		/// <summary>The text a live value is shown as.</summary>
		public static string Format(object value)
		{
			switch (value)
			{
				case null:
					return "null";
				case string text:
					return text;
				case UnityEngine.Object unityObject:
					return unityObject != null ? $"{unityObject.name} ({unityObject.GetType().Name})" : "None";
				case float single:
					return single.ToString("0.###", CultureInfo.InvariantCulture);
				case double number:
					return number.ToString("0.###", CultureInfo.InvariantCulture);
				case bool flag:
					return flag ? "Yes" : "No";
				case Vector2 vector2:
					return vector2.ToString("0.###");
				case Vector3 vector3:
					return vector3.ToString("0.###");
				case IFormattable formattable:
					return formattable.ToString(null, CultureInfo.InvariantCulture);
				default:
					return value.ToString();
			}
		}
	}

}
