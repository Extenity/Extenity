using System;
using System.Text.RegularExpressions;

namespace Extenity.DataToolbox
{

	public static class RegexTools
	{
		public static readonly string Regex_Decimal = @"^\d*\.?\d*$";

		#region Wildcard

		public static string WildcardToRegex(this string textWithWildcards)
		{
			return "^" + Regex.Escape(textWithWildcards).Replace("\\?", ".").Replace("\\*", ".*") + "$";
		}

		public static bool CheckWildcardMatching(this string text, string textWithWildcards, bool ignoreCase = false, bool invariantCulture = false)
		{
			var options =
				(ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None) |
				(invariantCulture ? RegexOptions.CultureInvariant : RegexOptions.None);
			return Regex.IsMatch(text, textWithWildcards.WildcardToRegex(), options);
		}

		#endregion
	}

}
