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

		#region Match

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

		#endregion
	}

	[Serializable]
	public class StringFilterEntry
	{
		public StringFilterType FilterType = StringFilterType.Contains;
		public string Filter = "";
		public StringComparison ComparisonType = StringComparison.InvariantCulture;
		public bool Inverted = false;
		[Tooltip("When checking if a text matches the filter list, 'MustMatch' option states that the check must match this filter entry to be accepted. If the text does not match any one of the 'MustMatch' filter entries, check will fail. In other words, this option basically allows to build filters like 'AND' operator.")]
		public bool MustMatch = false;

		#region Initialization

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

		#endregion

		#region Creators

		public static StringFilterEntry CreateSmartWildcard(string filter)
		{
			var seenAtTheBeginning = false;
			for (int i = 0; i < filter.Length; i++)
			{
				if (filter[i] == '?')
				{
					// Nothing more to do about '?' matching. It's only supported by wildcards.
					return new StringFilterEntry(StringFilterType.Wildcard, filter);
				}

				if (filter[i] == '*')
				{
					if (i == 0)
					{
						// Wildcard at the beginning
						seenAtTheBeginning = true;
					}
					else if (i == filter.Length - 1)
					{
						// Wildcard at the end
						return seenAtTheBeginning
							? new StringFilterEntry(StringFilterType.Contains, filter.Substring(1, filter.Length - 2))
							: new StringFilterEntry(StringFilterType.StartsWith, filter.Substring(0, filter.Length - 1));
					}
					else
					{
						// Wildcard in the middle
						// Fall back to wildcard matching, which is non performant.
						// TODO OPTIMIZATION: There are still things to do for performance.
						return new StringFilterEntry(StringFilterType.Wildcard, filter);
					}
				}
			}

			return seenAtTheBeginning
				? new StringFilterEntry(StringFilterType.EndsWith, filter.Substring(1, filter.Length - 1))
				: new StringFilterEntry(StringFilterType.Exactly, filter);
		}

		#endregion

		#region Match

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
							return text.CheckWildcardMatchingRegex(Filter, false, false).InvertIf(Inverted);

						case StringComparison.CurrentCultureIgnoreCase:
							return text.CheckWildcardMatchingRegex(Filter, true, false).InvertIf(Inverted);

						case StringComparison.InvariantCulture:
							return text.CheckWildcardMatchingRegex(Filter, false, true).InvertIf(Inverted);

						case StringComparison.InvariantCultureIgnoreCase:
							return text.CheckWildcardMatchingRegex(Filter, true, true).InvertIf(Inverted);

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

		#endregion

		#region ToString

		public override string ToString()
		{
			return $"{(MustMatch ? "MustMatch " : "")}{(Inverted ? "Inverted " : "")}{FilterType} for '{Filter}' in {ComparisonType}";
		}

		#endregion
	}

	public static class StringFilterTools
	{
		public static bool IsAnyMatching(this StringFilterEntry[] entries, string text)
		{
			for (int i = 0; i < entries.Length; i++)
			{
				if (entries[i].IsMatching(text))
					return true;
			}
			return false;
		}
	}

}
