using System;
using System.Linq.Expressions;

namespace Extenity.InspectorToolbox.Editor
{

	public static class ListFeatures
	{
		/// <summary>
		/// Shows the list as a table: one row per element, one column per field. Without columns every serialized
		/// field of the element type gets one.
		/// </summary>
		public static ListRule<T, TElement> AsTable<T, TElement>(this ListRule<T, TElement> rule, params Expression<Func<TElement, object>>[] columns)
		{
			rule.Options.Table = true;
			if (columns != null && columns.Length > 0)
			{
				var names = new string[columns.Length];
				for (var i = 0; i < columns.Length; i++)
				{
					names[i] = FieldNames.Get(columns[i]);
					if (!FieldNames.TryFind(typeof(TElement), names[i], out var field) || !FieldNames.IsDeclaredSerialized(field))
						throw new ArgumentException($"'{typeof(TElement).Name}.{names[i]}' is not a serialized field, so it cannot be a table column.");
				}
				rule.Options.TableColumns = names;
			}
			return rule;
		}

		/// <summary>
		/// Shows the list as a table whose columns are declared one by one, each with its own header text, width
		/// share and way of drawing: <c>.AsTable(table => { table.Column(e => e.Id).Title("Id"); table.Column(e => e.Position).NumbersOnOneLine().Stretch(2); })</c>.
		/// </summary>
		public static ListRule<T, TElement> AsTable<T, TElement>(this ListRule<T, TElement> rule, Action<TableBuilder<TElement>> columns)
		{
			if (columns == null)
				throw new ArgumentNullException(nameof(columns));
			var builder = new TableBuilder<TElement>();
			columns(builder);
			if (builder.Columns.Count == 0)
				throw new ArgumentException("The table has no columns. Declare at least one, or use AsTable() for a column per field.");
			rule.Options.Table = true;
			rule.Options.TableColumnSettings = builder.Columns;
			return rule;
		}

		/// <summary>
		/// Draws every element of the list, a struct that consists of a few numbers (a vector), on one line: one
		/// small field per member. See <see cref="FieldVariantFeatures.NumbersOnOneLine{T,TField}"/>.
		/// </summary>
		public static ListRule<T, TElement> ElementNumbersOnOneLine<T, TElement>(this ListRule<T, TElement> rule) where TElement : struct
		{
			rule.Options.ElementFactory = (elementProperty, label) => new NumberLineField(elementProperty, label ?? "");
			return rule;
		}

		/// <summary>
		/// For a SerializeReference list whose element type is the base type: the add button opens a picker of
		/// the concrete types, each row can change its type (fields with the same name keep their values), and
		/// empty elements are shown as such. <paramref name="menuPath"/> gives the path of a type in the picker,
		/// levels separated with '/'. Without it the type name is used.
		/// </summary>
		public static ListRule<T, TElement> ManagedReferences<T, TElement>(this ListRule<T, TElement> rule, Func<Type, string> menuPath = null) where TElement : class
		{
			rule.Options.ManagedReferences = true;
			rule.Options.ManagedReferenceMenuPath = menuPath;
			return rule;
		}

		/// <summary>The add button appends what the function returns for the owning value.</summary>
		public static ListRule<T, TElement> CustomAdd<T, TElement>(this ListRule<T, TElement> rule, Func<T, TElement> create)
		{
			if (create == null)
				throw new ArgumentNullException(nameof(create));
			rule.Options.CustomAdd = target => create((T)target);
			return rule;
		}
	}

	/// <summary>Declares the columns of a table, from left to right.</summary>
	public sealed class TableBuilder<TElement>
	{
		internal readonly System.Collections.Generic.List<TableColumnOptions> Columns = new System.Collections.Generic.List<TableColumnOptions>();

		/// <summary>Adds a column for a serialized field of the element type.</summary>
		public TableColumnBuilder Column(Expression<Func<TElement, object>> field)
		{
			var name = FieldNames.Get(field);
			if (!FieldNames.TryFind(typeof(TElement), name, out var fieldInfo) || !FieldNames.IsDeclaredSerialized(fieldInfo))
				throw new ArgumentException($"'{typeof(TElement).Name}.{name}' is not a serialized field, so it cannot be a table column.");
			foreach (var existing in Columns)
			{
				if (existing.FieldName == name)
					throw new ArgumentException($"The table already has a column for '{typeof(TElement).Name}.{name}'.");
			}
			var column = new TableColumnOptions { FieldName = name };
			Columns.Add(column);
			return new TableColumnBuilder(column);
		}
	}

	/// <summary>The options of one table column. Every method returns the builder, so calls chain.</summary>
	public sealed class TableColumnBuilder
	{
		private readonly TableColumnOptions Column;

		internal TableColumnBuilder(TableColumnOptions column)
		{
			Column = column;
		}

		/// <summary>The text of the column header, in place of the nicified field name.</summary>
		public TableColumnBuilder Title(string title)
		{
			if (string.IsNullOrEmpty(title))
				throw new ArgumentException("A column needs a title.", nameof(title));
			Column.Title = title;
			return this;
		}

		/// <summary>The share of the table width, relative to the other columns (default 1).</summary>
		public TableColumnBuilder Stretch(float share)
		{
			if (share <= 0f)
				throw new ArgumentOutOfRangeException(nameof(share));
			Column.Stretch = share;
			return this;
		}

		/// <summary>The smallest width of the column in pixels (default 50).</summary>
		public TableColumnBuilder MinWidth(float pixels)
		{
			if (pixels <= 0f)
				throw new ArgumentOutOfRangeException(nameof(pixels));
			Column.MinWidth = pixels;
			return this;
		}

		/// <summary>Draws the cells, structs of a few numbers (a vector), as number fields next to each other without member names.</summary>
		public TableColumnBuilder NumbersOnOneLine()
		{
			Column.NumbersOnOneLine = true;
			return this;
		}
	}

}
