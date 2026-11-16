using System;
using Extenity.InspectorToolbox.Editor;
using UnityEditor;

namespace Extenity.DataToolbox.Editor
{

	/// <summary>
	/// The inspector of <see cref="StringFilter"/>: its entries as a table, with a warning under the header for
	/// every entry whose filter value has no effect.
	/// </summary>
	[CustomPropertyDrawer(typeof(StringFilter))]
	public sealed class StringFilterInspector : NestedInspector<StringFilter>
	{
		protected override void Configure(InspectorBuilder<StringFilter> b)
		{
			b.List(x => x.Filters)
			 .AsTable(table =>
			 {
				 table.Column(entry => entry.FilterType).Title("Type").MinWidth(90);
				 table.Column(entry => entry.Filter).Title("Filter").Stretch(2f).MinWidth(80);
				 table.Column(entry => entry.ComparisonType).Title("Comparison").Stretch(1.5f).MinWidth(90);
				 table.Column(entry => entry.Inverted).Title("Invert").MinWidth(44).Stretch(0.3f);
				 table.Column(entry => entry.MustMatch).Title("Must").MinWidth(44).Stretch(0.3f);
			 })
			 .ExpandedByDefault()
			 .CustomAdd(filter => new StringFilterEntry())
			 .Validate((filter, context) => GetWarnings(filter));
		}

		internal static ValidationResult GetWarnings(StringFilter filter)
		{
			if (filter.Filters == null)
				return ValidationResult.Ok;
			string message = null;
			for (int i = 0; i < filter.Filters.Length; i++)
			{
				var warning = filter.Filters[i]?.FilterWarning;
				if (warning == null)
					continue;
				var line = $"Entry {i}: {warning}";
				message = message == null ? line : message + Environment.NewLine + line;
			}
			return message == null ? ValidationResult.Ok : ValidationResult.Warning(message);
		}
	}

}
