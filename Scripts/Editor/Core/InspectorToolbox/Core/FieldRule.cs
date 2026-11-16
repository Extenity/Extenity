using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEditor;
using UnityEngine.UIElements;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>
	/// The deviations declared for one serialized field. Every method returns the rule, so calls chain.
	/// </summary>
	public abstract class FieldRuleBase<T, TField, TSelf> where TSelf : FieldRuleBase<T, TField, TSelf>
	{
		/// <summary>The untyped data of the rule: the extension point for features defined elsewhere.</summary>
		public FieldRuleData Data { get; }

		protected FieldRuleBase(FieldRuleData data)
		{
			Data = data ?? throw new ArgumentNullException(nameof(data));
		}

		private TSelf Self => (TSelf)this;

		#region Label

		public TSelf Label(string label)
		{
			if (string.IsNullOrEmpty(label))
				throw new ArgumentException("Use HideLabel() to draw a field without a label.", nameof(label));
			Data.Label = label;
			return Self;
		}

		/// <summary>A label computed from the target. With several objects selected the first one decides.</summary>
		public TSelf Label(Func<T, string> label)
		{
			if (label == null)
				throw new ArgumentNullException(nameof(label));
			Data.DynamicLabel = target => label((T)target);
			return Self;
		}

		public TSelf HideLabel()
		{
			Data.HideLabel = true;
			return Self;
		}

		/// <summary>A muted text after the field, like a unit.</summary>
		public TSelf Suffix(string suffix)
		{
			Data.Suffix = suffix;
			return Self;
		}

		#endregion

		#region Visibility and enabled state

		/// <summary>Never draws the field.</summary>
		public TSelf Hide()
		{
			Data.Hidden = true;
			return Self;
		}

		public TSelf VisibleWhen(Func<T, bool> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));
			Data.VisibleRules.Add((target, context) => predicate((T)target));
			return Self;
		}

		public TSelf VisibleWhen(Func<T, InspectorContext, bool> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));
			Data.VisibleRules.Add((target, context) => predicate((T)target, context));
			return Self;
		}

		public TSelf HiddenWhen(Func<T, bool> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));
			Data.VisibleRules.Add((target, context) => !predicate((T)target));
			return Self;
		}

		public TSelf HiddenWhen(Func<T, InspectorContext, bool> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));
			Data.VisibleRules.Add((target, context) => !predicate((T)target, context));
			return Self;
		}

		public TSelf EnabledWhen(Func<T, bool> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));
			Data.EnabledRules.Add((target, context) => predicate((T)target));
			return Self;
		}

		public TSelf EnabledWhen(Func<T, InspectorContext, bool> predicate)
		{
			if (predicate == null)
				throw new ArgumentNullException(nameof(predicate));
			Data.EnabledRules.Add((target, context) => predicate((T)target, context));
			return Self;
		}

		/// <summary>Shows the value without letting it be edited. The text stays fully legible.</summary>
		public TSelf ReadOnly()
		{
			Data.ReadOnly = true;
			return Self;
		}

		#endregion

		#region Validation

		/// <summary>Shows <paramref name="message"/> under the field while <paramref name="isValid"/> is false for its value.</summary>
		public TSelf Validate(Func<TField, bool> isValid, string message, MessageSeverity severity = MessageSeverity.Error)
		{
			if (isValid == null)
				throw new ArgumentNullException(nameof(isValid));
			if (string.IsNullOrEmpty(message))
				throw new ArgumentException("A validation needs a message.", nameof(message));
			if (severity == MessageSeverity.None)
				throw new ArgumentException("A validation message needs a severity.", nameof(severity));
			var data = Data;
			data.Validators.Add((target, context) =>
				isValid((TField)data.GetValue(target)) ? ValidationResult.Ok : severity switch
				{
					MessageSeverity.Error => ValidationResult.Error(message),
					MessageSeverity.Warning => ValidationResult.Warning(message),
					_ => ValidationResult.Info(message),
				});
			return Self;
		}

		public TSelf Validate(Func<T, InspectorContext, ValidationResult> rule)
		{
			if (rule == null)
				throw new ArgumentNullException(nameof(rule));
			Data.Validators.Add((target, context) => rule((T)target, context));
			return Self;
		}

		/// <summary>Shows an error while the field is null, a missing object or an empty string.</summary>
		public TSelf Required(string message = null)
		{
			var data = Data;
			message ??= $"{ObjectNames.NicifyVariableName(data.FieldName)} is required.";
			data.Validators.Add((target, context) =>
			{
				var value = data.GetValue(target);
				var isMissing = value == null
				                || (value is UnityEngine.Object unityObject && unityObject == null)
				                || (value is string text && text.Length == 0);
				return isMissing ? ValidationResult.Error(message) : ValidationResult.Ok;
			});
			return Self;
		}

		#endregion

		#region Change

		/// <summary>
		/// Runs after the field's value changed and was applied: an edit in the inspector, an undo or a change
		/// made by code. In the inspector of an object the action gets the object itself. In a nested inspector
		/// it gets a copy made from the serialized data, so it can read but not change the value.
		/// </summary>
		public TSelf OnChanged(Action<T> afterApply)
		{
			if (afterApply == null)
				throw new ArgumentNullException(nameof(afterApply));
			Data.ChangeHandlers.Add(target => afterApply((T)target));
			return Self;
		}

		#endregion

		#region Drawing

		/// <summary>Draws the children of the field directly, without a foldout or a label.</summary>
		public TSelf Inline()
		{
			Data.Inline = true;
			return Self;
		}

		/// <summary>
		/// The escape hatch: draws the field with the given element. It must bind through <c>bindingPath</c>.
		/// </summary>
		public TSelf Replace(Func<InspectorContext, SerializedProperty, VisualElement> factory)
		{
			if (factory == null)
				throw new ArgumentNullException(nameof(factory));
			Data.SetFactory(nameof(Replace), (instance, property, label) =>
				factory(instance.Context, property)
				?? throw new InvalidOperationException($"The Replace factory of field '{property.propertyPath}' returned no element."));
			return Self;
		}

		#endregion

		#region Layout

		/// <summary>A fixed width in pixels. Only has an effect inside a row.</summary>
		public TSelf Width(float pixels)
		{
			if (pixels <= 0f)
				throw new ArgumentOutOfRangeException(nameof(pixels));
			Data.Width = pixels;
			return Self;
		}

		/// <summary>Takes the remaining width. Only has an effect inside a row.</summary>
		public TSelf Grow()
		{
			Data.Grow = true;
			return Self;
		}

		public TSelf AtTop()
		{
			Data.Placement = new Placement(PlacementKind.AtTop);
			return Self;
		}

		public TSelf AtBottom()
		{
			Data.Placement = new Placement(PlacementKind.AtBottom);
			return Self;
		}

		public TSelf Before<TOther>(Expression<Func<T, TOther>> field)
		{
			Data.Placement = new Placement(PlacementKind.Before, FieldNames.Get(field));
			return Self;
		}

		public TSelf After<TOther>(Expression<Func<T, TOther>> field)
		{
			Data.Placement = new Placement(PlacementKind.After, FieldNames.Get(field));
			return Self;
		}

		public TSelf Before(string serializedFieldName)
		{
			Data.Placement = new Placement(PlacementKind.Before, serializedFieldName);
			return Self;
		}

		public TSelf After(string serializedFieldName)
		{
			Data.Placement = new Placement(PlacementKind.After, serializedFieldName);
			return Self;
		}

		#endregion
	}

	public sealed class FieldRule<T, TField> : FieldRuleBase<T, TField, FieldRule<T, TField>>
	{
		internal FieldRule(FieldRuleData data) : base(data)
		{
		}
	}

	/// <summary>
	/// The rule of a list or array field. It has every method of a field rule, plus the list options. Using an
	/// option replaces Unity's default list with the toolbox's list element.
	/// </summary>
	public sealed class ListRule<T, TElement> : FieldRuleBase<T, IList<TElement>, ListRule<T, TElement>>
	{
		public ListOptions Options => Data.List;

		internal ListRule(FieldRuleData data) : base(data)
		{
			if (data.List == null)
			{
				if (data.Factory != null)
					throw new InvalidOperationException($"Field '{data.FieldName}' is already drawn by '{data.FactoryFeature}' and cannot have list options.");
				data.List = new ListOptions(typeof(TElement));
			}
			else if (data.List.ElementType != typeof(TElement))
			{
				throw new InvalidOperationException($"Field '{data.FieldName}' was declared as a list of '{data.List.ElementType.Name}' before, now as a list of '{typeof(TElement).Name}'.");
			}
		}

		/// <summary>The label of each element's row, computed from the element. Wins over the title of the element's own inspector.</summary>
		public ListRule<T, TElement> ElementLabel(Func<TElement, string> label)
		{
			if (label == null)
				throw new ArgumentNullException(nameof(label));
			Options.ElementLabel = element => label((TElement)element);
			return this;
		}

		/// <summary>Shows the index in front of each element.</summary>
		public ListRule<T, TElement> ShowIndex()
		{
			Options.ShowIndex = true;
			return this;
		}

		/// <summary>Closes the list the first time it is shown for an object in an editor session.</summary>
		public ListRule<T, TElement> CollapsedByDefault()
		{
			if (Options.ExpandedByDefault)
				throw new InvalidOperationException($"The list '{Data.FieldName}' is already expanded by default. It cannot be both.");
			Options.CollapsedByDefault = true;
			return this;
		}

		/// <summary>
		/// Opens the list the first time it is shown for an object in an editor session. For short lists whose
		/// elements are what the reader came for. Without it a list starts in the state Unity remembers, closed
		/// when it was never opened.
		/// </summary>
		public ListRule<T, TElement> ExpandedByDefault()
		{
			if (Options.CollapsedByDefault)
				throw new InvalidOperationException($"The list '{Data.FieldName}' is already collapsed by default. It cannot be both.");
			Options.ExpandedByDefault = true;
			return this;
		}

		/// <summary>Limits the height of the element area. Longer lists scroll inside it.</summary>
		public ListRule<T, TElement> MaxVisibleHeight(float pixels)
		{
			if (pixels <= 0f)
				throw new ArgumentOutOfRangeException(nameof(pixels));
			Options.MaxVisibleHeight = pixels;
			return this;
		}

		/// <summary>
		/// Adds a search field above the elements. Without <paramref name="match"/> the element label is searched.
		/// </summary>
		public ListRule<T, TElement> Searchable(Func<TElement, string, bool> match = null)
		{
			Options.Searchable = true;
			if (match != null)
				Options.SearchMatch = (element, text) => match((TElement)element, text);
			return this;
		}
	}

}
