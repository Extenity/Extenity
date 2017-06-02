using System.Linq;
using System.Text.RegularExpressions;

namespace Extenity.DataToolbox
{

	public static class CamelCaseWording
	{
		#region Split Adjoint Words

		private static Regex _CamelCasedAdjointWordSeparatorRegex;

		public static Regex CamelCasedAdjointWordSeparatorRegex
		{
			get
			{
				if (_CamelCasedAdjointWordSeparatorRegex == null)
					_CamelCasedAdjointWordSeparatorRegex = new Regex("((?<=[a-z])(?=[A-Z]))|((?<=[A-Z])(?=[A-Z][a-z]))",
						RegexOptions.Compiled);
				return _CamelCasedAdjointWordSeparatorRegex;
			}
		}

		/// <summary>
		/// Results: 
		///    AsdZxcQwe -> Asd Zxc Qwe
		///    AsdZXCQwe -> Asd ZXC Qwe
		///    AsdZxc123 -> Asd Zxc123
		///    AsdZxc123Qwe -> Asd Zxc123Qwe
		///    AsdZxc123QweFgh -> Asd Zxc123Qwe Fgh
		///    123QweFgh -> 123Qwe Fgh
		///    Asd Qwe 123 ASD ZXC -> Asd Qwe 123 ASD ZXC
		/// 
		/// Source: http://nlpdotnet.com/SampleCode/CamelCaseStringToWords.aspx
		/// </summary>
		public static string SeparateCamelCasedAdjointWords(this string input, string separator = " ")
		{
			if (string.IsNullOrEmpty(input))
				return "";
			return CamelCasedAdjointWordSeparatorRegex.Replace(input, separator);
		}

		/// <summary>
		/// See 'SeparateCamelCasedAdjointWords' for more info.
		/// </summary>
		public static string[] SplitCamelCasedAdjointWords(this string input)
		{
			if (string.IsNullOrEmpty(input))
				return new string[0];
			return CamelCasedAdjointWordSeparatorRegex.Split(input).Where(item => !string.IsNullOrEmpty(item)).ToArray();
		}

		#endregion
	}

}
