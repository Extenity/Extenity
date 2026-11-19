using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>
	/// A list of structs or classes shown as a table: one row per element, one column per field. The header is
	/// a foldout that follows the property's expanded state. Rows can be added, removed and reordered.
	/// </summary>
	public class ExtTableView : VisualElement, ILazyContent, IValidationMessageHost
	{
		public const string UssClassName = "ext-table";

		public SerializedObject SerializedObject { get; }
		public string ArrayPropertyPath { get; }
		public VisualElement ValidationMessageContainer { get; }

		private readonly ListOptions Options;
		private readonly InspectorInstance Owner;
		private readonly LazyFoldout Header;
		private MultiColumnListView Table;

		/// <param name="owner">The inspector the table belongs to. Its cells are bound with the property handler cache of that inspector.</param>
		public ExtTableView(SerializedProperty arrayProperty, string label, ListOptions options, InspectorInstance owner = null)
		{
			if (arrayProperty == null)
				throw new ArgumentNullException(nameof(arrayProperty));
			if (!arrayProperty.isArray || arrayProperty.propertyType == SerializedPropertyType.String)
				throw new ArgumentException($"'{arrayProperty.propertyPath}' is not an array or a list.", nameof(arrayProperty));
			Options = options ?? throw new ArgumentNullException(nameof(options));
			Owner = owner;
			SerializedObject = arrayProperty.serializedObject;
			ArrayPropertyPath = arrayProperty.propertyPath;

			AddToClassList(UssClassName);
			AddToClassList(ExtListView.UssClassName);
			InspectorStyles.Apply(this);

			ExtListView.ApplyDefaultExpandedState(arrayProperty, Options);

			Header = new LazyFoldout(label ?? arrayProperty.displayName, BuildContent, arrayProperty.isExpanded)
			{
				bindingPath = ArrayPropertyPath,
			};
			Header.FollowExpandedStateOf(arrayProperty);
			Header.AddToClassList(ExtListView.UssClassName + "__header");
			Add(Header);

			ValidationMessageContainer = new VisualElement();
			ValidationMessageContainer.AddToClassList(ExtListView.UssClassName + "__messages");
			Header.hierarchy.Insert(1, ValidationMessageContainer);
		}

		private void BuildContent(VisualElement content)
		{
			Table = new MultiColumnListView
			{
				bindingPath = ArrayPropertyPath,
				showBoundCollectionSize = false,
				showAddRemoveFooter = true,
				showBorder = true,
				reorderable = true,
				reorderMode = ListViewReorderMode.Animated,
				virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
				showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
				selectionType = SelectionType.Single,
			};
			Table.AddToClassList(UssClassName + "__view");
			if (Options.MaxVisibleHeight > 0f)
				Table.style.maxHeight = Options.MaxVisibleHeight;
			else
				Table.AddToClassList(ExtListView.CappedUssClassName);

			foreach (var columnOptions in GetColumns())
			{
				var column = columnOptions;
				var fieldName = column.FieldName;
				Table.columns.Add(new Column
				{
					name = fieldName,
					title = column.Title ?? ObjectNames.NicifyVariableName(fieldName),
					stretchable = true,
					// A stretchable column takes its share of the table width in proportion to its width.
					width = BaseColumnWidth * column.Stretch,
					minWidth = column.MinWidth,
					makeCell = () =>
					{
						if (column.NumbersOnOneLine)
						{
							var host = new VisualElement();
							host.AddToClassList(UssClassName + "__cell");
							return host;
						}
						var cell = new PropertyField { label = "" };
						cell.AddToClassList(UssClassName + "__cell");
						return cell;
					},
					bindCell = (cell, rowIndex) =>
					{
						var array = SerializedObject.FindProperty(ArrayPropertyPath);
						if (array == null || rowIndex >= array.arraySize)
							return;
						var property = array.GetArrayElementAtIndex(rowIndex).FindPropertyRelative(fieldName)
						               ?? throw new InvalidOperationException($"An element of '{ArrayPropertyPath}' has no serialized field '{fieldName}' for the table column.");
						// A table without an owner (default) selects nothing and binds as before.
						using (Owner?.SelectHandlerCache() ?? default)
						{
							if (column.NumbersOnOneLine)
							{
								cell.Clear();
								var numbers = new NumberLineField(property, "", showMemberNames: false);
								cell.Add(numbers);
								numbers.Bind(SerializedObject);
							}
							else
							{
								((PropertyField)cell).BindProperty(property);
							}
						}
					},
					unbindCell = (cell, rowIndex) =>
					{
						if (column.NumbersOnOneLine)
							cell.Clear();
						else
							cell.Unbind();
					},
				});
			}
			content.Add(Table);
			if (content.panel != null)
			{
				using (Owner?.SelectHandlerCache() ?? default)
					Table.Bind(SerializedObject);
			}
		}

		private const float BaseColumnWidth = 100f;

		private List<TableColumnOptions> GetColumns()
		{
			if (Options.TableColumnSettings != null && Options.TableColumnSettings.Count > 0)
				return Options.TableColumnSettings;

			var columns = new List<TableColumnOptions>();
			if (Options.TableColumns != null && Options.TableColumns.Length > 0)
			{
				foreach (var name in Options.TableColumns)
					columns.Add(new TableColumnOptions { FieldName = name });
				return columns;
			}

			// Every serialized field of the element type, base type fields first, in declaration order.
			var hierarchy = new List<Type>();
			for (var type = Options.ElementType; type != null && type != typeof(object); type = type.BaseType)
				hierarchy.Insert(0, type);
			foreach (var type in hierarchy)
			{
				foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
				{
					if (FieldNames.IsDeclaredSerialized(field) && !field.IsDefined(typeof(UnityEngine.HideInInspector), true))
						columns.Add(new TableColumnOptions { FieldName = field.Name });
				}
			}
			if (columns.Count == 0)
				throw new InvalidOperationException($"'{Options.ElementType.FullName}' has no serialized fields to make table columns from.");
			return columns;
		}

		public bool ForceBuildContent(int maxListElements)
		{
			return Header.ForceBuildContent(maxListElements);
		}
	}

}
