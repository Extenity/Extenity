using System;
using Extenity.MathToolbox;
using UnityEngine;

namespace Extenity.DataToolbox
{

	public enum StringFilterType
	{
		Contains = 0,
		StartsWith = 1,
		EndsWith = 2,
		Exactly = 3,
		Wildcard = 4,
		//RegExp = 8, Not implemented yet
		//LiquidMetal = 9, Not implemented yet
	}

	[Serializable]
	public class StringFilter
	{
		public StringFilterEntry[] Filters;

		public StringFilter()
		{
		}

		public StringFilter(params StringFilterEntry[] filters)
		{
			Filters = filters;
		}

		public bool IsMatching(string text)
		{
			// Does not match if there are no filters.
			if (Filters == null || Filters.Length == 0)
				return false;

			var matched = false;
			for (var i = 0; i < Filters.Length; i++)
			{
				var filter = Filters[i];
				var thisOneMatched = filter.IsMatching(text);
				if (thisOneMatched)
				{
					matched = true;
				}
				else if (filter.MustMatch) // If any one of the 'MustMatch' filters fails, result is immediately negative.
				{
					return false;
				}
			}

			return matched;
		}
	}

	[Serializable]
	public class StringFilterEntry
	{
		public StringFilterType FilterType = StringFilterType.Contains;
		public string Filter = "";
		public StringComparison ComparisonType = StringComparison.CurrentCulture;
		public bool Inverted = false;
		[Tooltip("When checking if a text matches the filter list, 'MustMatch' option states that the check must match this filter entry to be accepted. If the text does not match any one of the 'MustMatch' filter entries, check will fail. In other words, this option basically allows to build filters like 'AND' operator.")]
		public bool MustMatch = false;

		public StringFilterEntry()
		{
		}

		public StringFilterEntry(StringFilterType filterType, string filter)
		{
			FilterType = filterType;
			Filter = filter;
		}

		public StringFilterEntry(StringFilterType filterType, string filter, StringComparison comparisonType, bool inverted = false, bool mustMatch = false)
		{
			FilterType = filterType;
			Filter = filter;
			ComparisonType = comparisonType;
			Inverted = inverted;
			MustMatch = mustMatch;
		}

		public bool IsMatching(string text)
		{
			// Does not match if filter is not specified.
			if (string.IsNullOrEmpty(Filter))
				return false;

			switch (FilterType)
			{
				case StringFilterType.Contains:
					return text.Contains(Filter, ComparisonType).InvertIf(Inverted);

				case StringFilterType.StartsWith:
					return text.StartsWith(Filter, ComparisonType).InvertIf(Inverted);

				case StringFilterType.EndsWith:
					return text.EndsWith(Filter, ComparisonType).InvertIf(Inverted);

				case StringFilterType.Exactly:
					return text.Equals(Filter, ComparisonType).InvertIf(Inverted);

				case StringFilterType.Wildcard:
					switch (ComparisonType)
					{
						case StringComparison.CurrentCulture:
							return text.CheckWildcardMatchingRegex(Filter, false, false);
						case StringComparison.CurrentCultureIgnoreCase:
							return text.CheckWildcardMatchingRegex(Filter, true, false);
						case StringComparison.InvariantCulture:
							return text.CheckWildcardMatchingRegex(Filter, false, true);
						case StringComparison.InvariantCultureIgnoreCase:
							return text.CheckWildcardMatchingRegex(Filter, true, true);
						case StringComparison.Ordinal:
						case StringComparison.OrdinalIgnoreCase:
							throw new ArgumentException("Ordinal comparison type is not supported in wildcard filters.");
						default:
							throw new ArgumentOutOfRangeException(nameof(ComparisonType), (int)ComparisonType, "");
					}

				default:
					throw new ArgumentOutOfRangeException(nameof(FilterType), (int)FilterType, "");
			}
		}
	}

}
