using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine.UIElements;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>
	/// Creates the element that edits a field, in place of the default property field. The element must bind to
	/// the property with <c>bindingPath</c>, so that undo, prefab override marks and the revert menu keep working.
	/// <paramref name="label"/> is null for the default label and empty for no label.
	/// </summary>
	public delegate VisualElement FieldFactory(InspectorInstance instance, SerializedProperty property, string label);

	/// <summary>
	/// Everything declared for one serialized field. The typed rule classes write into it; features defined as
	/// extension methods reach it through the rule's <c>Data</c>.
	/// </summary>
	public sealed class FieldRuleData
	{
		public string FieldName { get; }
		public FieldInfo Field { get; }

		public string Label;
		public Func<object, string> DynamicLabel;
		public bool HideLabel;
		public string Suffix;
		public bool Hidden;
		public bool ReadOnly;
		public bool Inline;
		public float Width;
		public bool Grow;
		public Placement Placement;

		/// <summary>The field is visible for a target when every rule holds for it.</summary>
		public readonly List<Func<object, InspectorContext, bool>> VisibleRules = new List<Func<object, InspectorContext, bool>>();
		/// <summary>The field is enabled when every rule holds for every target.</summary>
		public readonly List<Func<object, InspectorContext, bool>> EnabledRules = new List<Func<object, InspectorContext, bool>>();
		public readonly List<Func<object, InspectorContext, ValidationResult>> Validators = new List<Func<object, InspectorContext, ValidationResult>>();
		public readonly List<Action<object>> ChangeHandlers = new List<Action<object>>();

		/// <summary>Set for a list rule. Null for a plain field rule.</summary>
		public ListOptions List;

		/// <summary>The node that draws this field, when a group or a row claimed it.</summary>
		public InspectorNode Container { get; internal set; }

		public FieldFactory Factory { get; private set; }
		public string FactoryFeature { get; private set; }

		internal FieldRuleData(string fieldName, FieldInfo field)
		{
			FieldName = fieldName;
			Field = field;
		}

		/// <summary>
		/// Replaces the default property field. Only one feature can draw a field: a second call throws.
		/// </summary>
		public void SetFactory(string featureName, FieldFactory factory)
		{
			if (factory == null)
				throw new ArgumentNullException(nameof(factory));
			if (Factory != null)
				throw new InvalidOperationException($"Field '{FieldName}' is already drawn by '{FactoryFeature}' and cannot also be drawn by '{featureName}'.");
			if (List != null && List.ReplacesDefaultList)
				throw new InvalidOperationException($"Field '{FieldName}' has list options and cannot also be drawn by '{featureName}'.");
			Factory = factory;
			FactoryFeature = featureName;
		}

		/// <summary>The value of the field on a target, read through reflection.</summary>
		public object GetValue(object target)
		{
			return Field.GetValue(target);
		}

		public bool HasRulesThatNeedEvaluation =>
			Hidden || ReadOnly || DynamicLabel != null || VisibleRules.Count > 0 || EnabledRules.Count > 0 || Validators.Count > 0;
	}

	/// <summary>
	/// The options of a list rule. Setting any option that <see cref="ReplacesDefaultList"/> reports makes the
	/// list be drawn by the toolbox's own list element in place of Unity's.
	/// </summary>
	public sealed class ListOptions
	{
		public Type ElementType { get; }

		public Func<object, string> ElementLabel;
		public bool ShowIndex;
		public bool CollapsedByDefault;
		public bool ExpandedByDefault;
		public float MaxVisibleHeight;
		public bool Searchable;
		/// <summary>Element and search text. Null: the element label is searched.</summary>
		public Func<object, string, bool> SearchMatch;

		public bool Table;
		public string[] TableColumns;

		public bool ManagedReferences;
		/// <summary>The menu path of a type in the type picker. Null: the nicified type name.</summary>
		public Func<Type, string> ManagedReferenceMenuPath;

		/// <summary>Owner target and context to the choices of each element. Set by ElementDropdown.</summary>
		public Func<object, InspectorContext, IEnumerable<DropdownItem<object>>> ElementDropdownItems;
		public DropdownOptions ElementDropdownOptions;

		/// <summary>Owner target to the new element. Set by CustomAdd.</summary>
		public Func<object, object> CustomAdd;

		/// <summary>
		/// Creates the element that draws a list element, in place of a property field: the element's property
		/// and the label of its row (null: the element decides, empty: none). The result must bind through
		/// <c>bindingPath</c>. Set by ElementNumbersOnOneLine.
		/// </summary>
		public Func<SerializedProperty, string, VisualElement> ElementFactory;

		/// <summary>The columns of a table that were declared one by one. Null: see <see cref="TableColumns"/>.</summary>
		public List<TableColumnOptions> TableColumnSettings;

		public ListOptions(Type elementType)
		{
			ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
		}

		public bool ReplacesDefaultList =>
			ElementLabel != null || ShowIndex || CollapsedByDefault || ExpandedByDefault || MaxVisibleHeight > 0f || Searchable || Table
			|| ManagedReferences || ElementDropdownItems != null || CustomAdd != null || ElementFactory != null;
	}

	/// <summary>One column of a list that is shown as a table.</summary>
	public sealed class TableColumnOptions
	{
		/// <summary>The serialized field of the element type the column shows.</summary>
		public string FieldName;
		/// <summary>The text of the column header. Null: the nicified field name.</summary>
		public string Title;
		/// <summary>The share of the table width the column gets, relative to the other columns. A column of 2 is twice as wide as a column of 1.</summary>
		public float Stretch = 1f;
		/// <summary>The column is never narrower than this many pixels.</summary>
		public float MinWidth = 50f;
		/// <summary>Draws a struct of numbers as number fields next to each other, without their member names.</summary>
		public bool NumbersOnOneLine;
	}

}
