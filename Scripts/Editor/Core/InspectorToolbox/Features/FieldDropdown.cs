using System;
using System.Collections.Generic;

namespace Extenity.InspectorToolbox.Editor
{

	public static class FieldDropdownFeature
	{
		/// <summary>
		/// Edits the field by picking from labelled choices in a searchable popup. The choices are read from the
		/// target whenever the popup opens. A current value that is not among them is shown marked and is kept.
		/// Works for string, integer, float and object reference fields, and for structs that wrap one of those.
		/// </summary>
		public static FieldRule<T, TField> Dropdown<T, TField>(this FieldRule<T, TField> rule, Func<T, InspectorContext, IEnumerable<DropdownItem<TField>>> items, DropdownOptions options = default)
		{
			if (items == null)
				throw new ArgumentNullException(nameof(items));
			rule.Data.SetFactory(nameof(Dropdown), (instance, property, label) =>
				DropdownFieldFactory.Create(property, label, () => GetItems(instance, items), options));
			return rule;
		}

		/// <summary>Draws every element of the list as a dropdown with the given choices.</summary>
		public static ListRule<T, TElement> ElementDropdown<T, TElement>(this ListRule<T, TElement> rule, Func<T, InspectorContext, IEnumerable<DropdownItem<TElement>>> items, DropdownOptions options = default)
		{
			if (items == null)
				throw new ArgumentNullException(nameof(items));
			rule.Options.ElementDropdownItems = (target, context) => Box(items((T)target, context));
			rule.Options.ElementDropdownOptions = options;
			return rule;
		}

		private static IEnumerable<DropdownItem<object>> GetItems<T, TField>(InspectorInstance instance, Func<T, InspectorContext, IEnumerable<DropdownItem<TField>>> items)
		{
			// With several objects selected the first one provides the choices.
			var pass = instance.CreatePass();
			if (pass == null || pass.Targets.Length == 0)
				return Array.Empty<DropdownItem<object>>();
			return Box(items((T)pass.Targets[0], pass.Context));
		}

		private static IEnumerable<DropdownItem<object>> Box<TValue>(IEnumerable<DropdownItem<TValue>> items)
		{
			var boxed = new List<DropdownItem<object>>();
			if (items == null)
				return boxed;
			foreach (var item in items)
				boxed.Add(new DropdownItem<object>(item.Label, item.Value, item.Group));
			return boxed;
		}
	}

}
