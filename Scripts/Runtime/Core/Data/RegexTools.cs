using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Extenity.DataToolbox
{

	public static class RegexTools
	{
		public const string Regex_Decimal = @"^\d*\.?\d*$";

		#region Wildcard

		private static readonly Dictionary<string, string> WildcardToRegexHistory = new Dictionary<string, string>();

		public static string WildcardToRegex(this string textWithWildcards)
		{
			lock (WildcardToRegexHistory)
			{
				if (WildcardToRegexHistory.TryGetValue(textWithWildcards, out var resultInHistory))
				{
					return resultInHistory;
				}
				else
				{
					var result = "^" + Regex.Escape(textWithWildcards).Replace("\\?", ".").Replace("\\*", ".*") + "$";
					// TODO OPTIMIZATION: Here we can keep the compiled regex for better performance. new Regex(..., RegexOptions.Compiled)
					WildcardToRegexHistory.Add(textWithWildcards, result);
					return result;
				}
			}
		}

		public static bool CheckWildcardMatchingRegex(this string text, string textWithWildcards, bool ignoreCase = false, bool invariantCulture = false)
		{
			var options =
				(ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None) |
				(invariantCulture ? RegexOptions.CultureInvariant : RegexOptions.None);
			return Regex.IsMatch(text, textWithWildcards.WildcardToRegex(), options);
		}

		#endregion
	}

}
