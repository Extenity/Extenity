using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using Extenity.MathToolbox;
using JetBrains.Annotations;
#if UNITY
using UnityEngine;
#endif

namespace Extenity.DataToolbox
{

	public static class StringTools
	{
		#region Shared Objects

		// TODO: Convert this to StringBuilderPool. Then use the pool where "new StringBuilder" is used.

		/// <summary>
		/// CAUTION! <c>SharedStringBuilder</c> has its strict usage rules. See examples around before using it.
		/// </summary>
		/// <example>
		/// <code>
		/// var stringBuilder = StringTools.SharedStringBuilder.Value;
		/// lock (stringBuilder)
		/// {
		/// 	stringBuilder.Clear(); // Make sure it is clean before starting to use.
		///
		/// 	...
		///
		/// 	var result = stringBuilder.ToString();
		/// 	StringTools.ClearSharedStringBuilder(stringBuilder); // Make sure we will leave it clean after use.
		/// 	return result;
		/// }
		/// </code>
		/// </example>
		public static readonly ThreadLocal<StringBuilder> SharedStringBuilder = new ThreadLocal<StringBuilder>(() => new StringBuilder(SharedStringBuilderInitialCapacity));
		private const int SharedStringBuilderInitialCapacity = 1000;
		private const int SharedStringBuilderCapacityTolerance = 10 * 1000 * 1000; // If the StringBuilder exceeds this capacity, it will be freed.

		public static void ClearSharedStringBuilder(StringBuilder stringBuilder)
		{
			stringBuilder.Clear();
			if (stringBuilder.Capacity > SharedStringBuilderCapacityTolerance)
			{
				// If this happens regularly, consider increasing the tolerance.
				Log.DebugWarning($"Shared StringBuilder size '{stringBuilder.Capacity}' exceeded the tolerance '{SharedStringBuilderCapacityTolerance}'.");
				stringBuilder.Capacity = SharedStringBuilderInitialCapacity;
			}
		}

		#endregion

		#region Constants

		public static readonly char[] LineEndingCharacters = { '\r', '\n' };
		public static readonly char[] NumericCharacters = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
		public static readonly char[] HexadecimalCharacters = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

		#endregion

		#region Equals

		public static bool EqualsOrBothEmpty(this string str1, string str2, StringComparison comparisonType)
		{
			if (string.IsNullOrEmpty(str1))
			{
				return string.IsNullOrEmpty(str2);
			}
			return str1.Equals(str2, comparisonType);
		}

		public static bool EqualsOrBothWhiteSpace(this string str1, string str2, StringComparison comparisonType)
		{
			if (string.IsNullOrWhiteSpace(str1))
			{
				return string.IsNullOrWhiteSpace(str2);
			}
			return str1.Equals(str2, comparisonType);
		}

		public static bool IsAllZeros(this string str)
		{
			if (string.IsNullOrEmpty(str))
				return false;
			for (int i = 0; i < str.Length; i++)
			{
				if (str[i] != '0')
					return false;
			}
			return true;
		}

		#endregion

		#region String Operations

		public static int CountCharacters(this string text, char character)
		{
			int count = 0;
			int i = -1;
			do
			{
				i = text.IndexOf(character, i + 1);
				if (i >= 0)
					count++;
			} while (i >= 0);
			return count;
		}

		// Got from http://stackoverflow.com/questions/541954/how-would-you-count-occurences-of-a-string-within-a-string-c
		public static int CountSubstrings(this string text, string substring, StringComparison comparisonType = StringComparison.CurrentCulture)
		{
			if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(substring))
				return 0;

			var count = 0;
			var index = 0;
			while ((index = text.IndexOf(substring, index, comparisonType)) != -1)
			{
				index += substring.Length;
				++count;
			}
			return count;
		}

		public static bool IsAsciiLetter(this char value)
		{
			return
				(value >= 'A' && value <= 'Z') ||
				(value >= 'a' && value <= 'z');
		}

		public static bool IsAlphaNumericUnicode(this string str, bool allowSpace = false, bool ensureStartsWithAlpha = false)
		{
			if (string.IsNullOrEmpty(str))
				return false;

			if (ensureStartsWithAlpha && !char.IsLetter(str[0]))
				return false;

			if (allowSpace)
			{
				for (int i = 0; i < str.Length; i++)
				{
					if (char.IsLetter(str[i]) || char.IsNumber(str[i]) || str[i] == ' ')
						continue;
					return false;
				}
			}
			else
			{
				for (int i = 0; i < str.Length; i++)
				{
					if (char.IsLetter(str[i]) || char.IsNumber(str[i]))
						continue;
					return false;
				}
			}

			return true;
		}

		public static bool IsAlphaNumericAscii(this string str, bool allowSpace = false, bool ensureStartsWithAlpha = false)
		{
			if (string.IsNullOrEmpty(str))
				return false;

			if (ensureStartsWithAlpha && !str[0].IsAsciiLetter())
				return false;

			if (allowSpace)
			{
				for (int i = 0; i < str.Length; i++)
				{
					if (str[i].IsAsciiLetter() || char.IsNumber(str[i]) || str[i] == ' ')
						continue;
					return false;
				}
			}
			else
			{
				for (int i = 0; i < str.Length; i++)
				{
					if (str[i].IsAsciiLetter() || char.IsNumber(str[i]))
						continue;
					return false;
				}
			}

			return true;
		}

		public static bool IsAlphaNumericAscii(this string str, char additionalAllowedCharacter, bool allowSpace = false, bool ensureStartsWithAlpha = false)
		{
			if (string.IsNullOrEmpty(str))
				return false;

			if (ensureStartsWithAlpha && !str[0].IsAsciiLetter())
				return false;

			if (allowSpace)
			{
				for (int i = 0; i < str.Length; i++)
				{
					if (str[i].IsAsciiLetter() || char.IsNumber(str[i]) || str[i] == ' ' || str[i] == additionalAllowedCharacter)
						continue;
					return false;
				}
			}
			else
			{
				for (int i = 0; i < str.Length; i++)
				{
					if (str[i].IsAsciiLetter() || char.IsNumber(str[i]) || str[i] == additionalAllowedCharacter)
						continue;
					return false;
				}
			}

			return true;
		}

		public static bool IsNumeric(this string str, bool allowSpace = false)
		{
			if (string.IsNullOrEmpty(str))
				return false;

			for (int i = 0; i < str.Length; i++)
			{
				if (char.IsNumber(str[i]) || (allowSpace && str[i] == ' '))
					continue;
				return false;
			}

			return true;
		}

		public static bool IsNumericFloat(this string str, bool allowSpace = false)
		{
			if (string.IsNullOrEmpty(str))
				return false;

			for (int i = 0; i < str.Length; i++)
			{
				if (char.IsNumber(str[i]) || str[i] == NumberDecimalSeparator || (allowSpace && str[i] == ' '))
					continue;
				return false;
			}

			return true;
		}

		public static string ClearSpecialCharacters(this string text)
		{
			return Regex.Replace(text, @"\W+", "");
		}

		public static string ClipIfNecessary(this string text, int maxCharacters)
		{
			return text.Length <= maxCharacters ? text : text.Substring(0, maxCharacters);
		}

		public static string SubstringBetween(this string text, string startTag, string endTag, int startIndex = 0)
		{
			if (text == null)
				throw new ArgumentNullException(nameof(text));
			if (string.IsNullOrEmpty(startTag))
				throw new ArgumentNullException(nameof(startTag));
			if (string.IsNullOrEmpty(endTag))
				throw new ArgumentNullException(nameof(endTag));
			if (text.Length == 0)
				return null;

			if (startIndex < 0)
				startIndex = 0;

			var startTagIndex = text.IndexOf(startTag, startIndex);
			if (startTagIndex < 0)
				return null;

			startTagIndex += startTag.Length;

			var endTagIndex = text.IndexOf(endTag, startTagIndex);
			if (endTagIndex <= startTagIndex)
				return null;

			return text.Substring(startTagIndex, endTagIndex - startTagIndex);
		}

		public static string ReplaceFirstOccurrence(this string text, string oldValue, string newValue, int startIndex = 0, StringComparison comparisonType = StringComparison.CurrentCulture)
		{
			//if (string.IsNullOrEmpty(text)) Commented out for performance reasons.
			//	throw new ArgumentNullException(nameof(text));
			//if (string.IsNullOrEmpty(oldValue))
			//	throw new ArgumentNullException(nameof(oldValue));
			////if (string.IsNullOrEmpty(newValue)) No! It's okay to have the new value empty. This means user wants to remove the 'oldValue' instances, rather than replacing it with something else.
			////	throw new ArgumentNullException(nameof(newValue));

			var index = text.IndexOf(oldValue, startIndex, comparisonType);
			if (index < 0)
				return text;
			return text.Remove(index, oldValue.Length).Insert(index, newValue);
		}

		public static string ReplaceLastOccurrence(this string text, string oldValue, string newValue, int startIndex = 0, StringComparison comparisonType = StringComparison.CurrentCulture)
		{
			//if (string.IsNullOrEmpty(text)) Commented out for performance reasons.
			//	throw new ArgumentNullException(nameof(text));
			//if (string.IsNullOrEmpty(oldValue))
			//	throw new ArgumentNullException(nameof(oldValue));
			////if (string.IsNullOrEmpty(newValue)) No! It's okay to have the new value empty. This means user wants to remove the 'oldValue' instances, rather than replacing it with something else.
			////	throw new ArgumentNullException(nameof(newValue));

			var index = text.LastIndexOf(oldValue, startIndex, comparisonType);
			if (index < 0)
				return text;
			return text.Remove(index, oldValue.Length).Insert(index, newValue);
		}

		/// <summary>
		/// Replaces the text found between startTag and endTag. Only the first occurence will be replaced.
		/// </summary>
		/// <returns>Modified text.</returns>
		public static string ReplaceBetween(this string text, string startTag, string endTag, Func<string, string> decider, bool keepTags, int startIndex = 0)
		{
			if (text == null)
				throw new ArgumentNullException(nameof(text));
			if (string.IsNullOrEmpty(startTag))
				throw new ArgumentNullException(nameof(startTag));
			if (string.IsNullOrEmpty(endTag))
				throw new ArgumentNullException(nameof(endTag));
			if (text.Length == 0)
				return text;
			if (startIndex < 0 || startIndex >= text.Length)
				throw new ArgumentOutOfRangeException(nameof(startIndex));

			var startTagIndex = text.IndexOf(startTag, startIndex);
			if (startTagIndex < 0)
				return text;
			var keyStartIndex = startTagIndex + startTag.Length;
			var endTagIndex = text.IndexOf(endTag, keyStartIndex);
			if (endTagIndex < 0)
				return text;

			var key = text.Substring(keyStartIndex, endTagIndex - keyStartIndex);
			var replacedWith = decider(key);
			// Null string means we won't be processing this key and leave it as it is. 
			// Empty string means we replace the key with the empty text, i.e we remove the key.
			if (replacedWith == null) // Do not use string.IsNullOrEmpty here.
				return text;
			int replaceStart;
			int replaceEnd;
			if (keepTags)
			{
				replaceStart = keyStartIndex;
				replaceEnd = endTagIndex;
			}
			else
			{
				replaceStart = startTagIndex;
				replaceEnd = endTagIndex + endTag.Length;
			}
			return text.Substring(0, replaceStart) +
			       replacedWith +
			       text.Substring(replaceEnd, text.Length - replaceEnd);
		}

		/// <summary>
		/// Replaces the text found between startTag and endTag. All occurences will be replaced. 
		/// </summary>
		public static bool ReplaceBetweenAll(this string text, string startTag, string endTag, Func<string, string> decider, bool keepTags, bool skipTagsInReplacedText, out string result, int startIndex = 0)
		{
			if (text == null)
				throw new ArgumentNullException(nameof(text));
			if (string.IsNullOrEmpty(startTag))
				throw new ArgumentNullException(nameof(startTag));
			if (string.IsNullOrEmpty(endTag))
				throw new ArgumentNullException(nameof(endTag));
			if (text.Length == 0)
			{
				result = text;
				return false;
			}
			if (startIndex < 0 || startIndex >= text.Length)
				throw new ArgumentOutOfRangeException(nameof(startIndex));

			var changed = false;
			while (true)
			{
				var startTagIndex = text.IndexOf(startTag, startIndex);
				if (startTagIndex < 0)
				{
					result = text;
					return changed;
				}
				var keyStartIndex = startTagIndex + startTag.Length;
				var endTagIndex = text.IndexOf(endTag, keyStartIndex);
				if (endTagIndex < 0)
				{
					result = text;
					return changed;
				}

				var key = text.Substring(keyStartIndex, endTagIndex - keyStartIndex);
				var replacedWith = decider(key);
				// Null string means we won't be processing this key and leave it as it is. 
				// Empty string means we replace the key with the empty text, i.e we remove the key.
				if (replacedWith == null) // Do not use string.IsNullOrEmpty here.
				{
					startIndex = endTagIndex + endTag.Length;
					continue;
				}
				int replaceStart;
				int replaceEnd;
				if (keepTags)
				{
					replaceStart = keyStartIndex;
					replaceEnd = endTagIndex;
				}
				else
				{
					replaceStart = startTagIndex;
					replaceEnd = endTagIndex + endTag.Length;
				}
				text = text.Substring(0, replaceStart) +
				       replacedWith +
				       text.Substring(replaceEnd, text.Length - replaceEnd);

				if (skipTagsInReplacedText)
				{
					startIndex = keyStartIndex + replacedWith.Length;
				}
				else
				{
					startIndex = keyStartIndex;
				}

				changed = true;
			}
		}

		public static int IndexOfNextLineEnding(this string text, int startIndex)
		{
			return text.IndexOfAny(LineEndingCharacters, startIndex);
		}

		public static string Repeat(this string text, int count)
		{
			if (string.IsNullOrEmpty(text))
				return string.Empty;

			var stringBuilder = new StringBuilder(text.Length * count);

			for (int i = 0; i < count; i++)
				stringBuilder.Append(text);

			return stringBuilder.ToString();
		}

		public static string TrimSafe(this string text)
		{
			if (text == null)
				return null;
			return text.Trim();
		}

		public static string TrimStart(this string text, string part, StringComparison comparison)
		{
			if (text == null)
				return null;
			if (text.StartsWith(part, comparison))
				return text.Substring(part.Length);
			return text;
		}

		public static string TrimEnd(this string text, string part, StringComparison comparison)
		{
			if (text == null)
				return null;
			if (text.EndsWith(part, comparison))
				return text.Substring(0, text.Length - part.Length);
			return text;
		}

		#endregion

		#region String Operations - Replace Selective

		public static string ReplaceSelective(this string text, string oldValue, string newValue, ImmutableStack<bool> selection, StringComparison comparisonType = StringComparison.CurrentCulture)
		{
			//if (string.IsNullOrEmpty(text)) Commented out for performance reasons.
			//	throw new ArgumentNullException(nameof(text));
			//if (string.IsNullOrEmpty(oldValue))
			//	throw new ArgumentNullException(nameof(oldValue));
			////if (string.IsNullOrEmpty(newValue)) No! It's okay to have the new value empty. This means user wants to remove the 'oldValue' instances, rather than replacing it with something else.
			////	throw new ArgumentNullException(nameof(newValue));
			if (newValue == null)
				newValue = string.Empty;

			var oldValueLength = oldValue.Length;
			var newValueLength = newValue.Length;
			int index = 0;
			foreach (var isReplaced in selection)
			{
				index = text.IndexOf(oldValue, index, comparisonType);
				if (index < 0)
					return text; // Seems like 'selection' contains more entries than the 'oldValue' count in 'text'. This is considered as okay. Break the operation.
				if (isReplaced)
				{
					text = text.Remove(index, oldValue.Length).Insert(index, newValue);
					index += newValueLength;
				}
				else
				{
					index += oldValueLength;
				}
			}
			return text;
		}

		#endregion

		#region String Operations - Count and Replace Tags

		/// <returns>True if succeeds, even there are no tags detected.
		/// False if there is a missing tag character or there is a nested tag inside another tag.
		/// TagCount will be int.MinValue when failed.</returns>
		public static bool CountTags(this string text, char tagStartCharacter, char tagEndCharacter, out int tagCount)
		{
			// TODO-IMMEDIATE: Check if for every start tag, there is an end tag. And ensure there are no nested tags.

			// Temp implementation.
			var tagStartCharacterCount = text.CountCharacters(tagStartCharacter);
			var tagEndCharacterCount = text.CountCharacters(tagEndCharacter);
			if (tagStartCharacterCount != tagEndCharacterCount)
			{
				tagCount = int.MinValue;
				return false;
			}
			tagCount = tagStartCharacterCount;
			return true;
		}

		public interface IReplaceTagProcessor
		{
			void AppendText(ReadOnlySpan<char> partOfText);
			void AppendTag(ReadOnlySpan<char> tag);
		}

		public enum ReplaceTagResult
		{
			Succeeded = 1,
			NoTagsFound = 2,
			EmptyInputText = 3,
			MismatchingTagBraces = 4,
		}

		public static ReplaceTagResult ReplaceTags(this string text, char tagStartCharacter, char tagEndCharacter, IReplaceTagProcessor processor)
		{
			if (string.IsNullOrEmpty(text))
			{
				return ReplaceTagResult.EmptyInputText;
			}

			// Check for mismatching start and end tags.
			if (!text.CountTags(tagStartCharacter, tagEndCharacter, out int tagCount))
			{
				return ReplaceTagResult.MismatchingTagBraces;
			}

			if (tagCount == 0)
			{
				processor.AppendText(text.AsSpan());
				return ReplaceTagResult.NoTagsFound;
			}

			int indexAfterTheEndTag = 0;
			for (int iTag = 0; iTag < tagCount; iTag++)
			{
				int startTagIndex = text.IndexOf(tagStartCharacter, indexAfterTheEndTag);
				int indexAfterTheStartTag = startTagIndex + 1;

				// Append the text to the left of the tag
				processor.AppendText(text.AsSpan(indexAfterTheEndTag, startTagIndex - indexAfterTheEndTag));

				var endTagIndex = text.IndexOf(tagEndCharacter, indexAfterTheStartTag);
				indexAfterTheEndTag = endTagIndex + 1;

				// Append the tag
				processor.AppendTag(text.AsSpan(indexAfterTheStartTag, endTagIndex - indexAfterTheStartTag));
			}

			// Append the text to the right of the tag
			processor.AppendText(text.AsSpan(indexAfterTheEndTag, text.Length - indexAfterTheEndTag));
			return ReplaceTagResult.Succeeded;
		}

		#endregion

		#region Number At The End

		public static int GetNumberAtTheEnd(this string text)
		{
			var stack = new Stack<char>(10);

			for (var i = text.Length - 1; i >= 0; i--)
			{
				if (!char.IsNumber(text[i]))
					break;

				stack.Push(text[i]);
			}

			if (stack.Count == 0)
				return 0;

			return Convert.ToInt32(new string(stack.ToArray()));
		}

		public static bool GetNumberAtTheEnd(this string text, out int value)
		{
			var stack = new Stack<char>(10);

			for (var i = text.Length - 1; i >= 0; i--)
			{
				if (!char.IsNumber(text[i]))
					break;

				stack.Push(text[i]);
			}

			if (stack.Count == 0)
			{
				value = 0;
				return false;
			}

			value = Convert.ToInt32(new string(stack.ToArray()));
			return true;
		}

		public static int GetNumberInParenthesesAtTheEnd(this string text, char begin = '(', char end = ')', bool hasAtLeastOneSpaceBeforeBeginning = true)
		{
			//var index = text.IndexOfNumberedParentheses(begin, end, hasAtLeastOneSpaceBeforeBeginning);
			throw new NotImplementedException();
		}

		public static bool GetNumberInParenthesesAtTheEnd(this string text, out int value, char begin = '(', char end = ')', bool hasAtLeastOneSpaceBeforeBeginning = true)
		{
			//var index = text.IndexOfNumberedParentheses(begin, end, hasAtLeastOneSpaceBeforeBeginning);
			throw new NotImplementedException();
		}

		public static string RemoveEndingNumberedParentheses(this string text, char begin = '(', char end = ')', bool hasAtLeastOneSpaceBeforeBeginning = true)
		{
			var index = text.IndexOfNumberedParentheses(begin, end, hasAtLeastOneSpaceBeforeBeginning);
			if (index > 0)
				return text.Substring(0, index);
			return text;
		}

		public static bool IsEndingWithNumberedParentheses(this string text, char begin = '(', char end = ')', bool hasAtLeastOneSpaceBeforeBeginning = true)
		{
			return -1 < text.IndexOfNumberedParentheses(begin, end, hasAtLeastOneSpaceBeforeBeginning);
		}

		public static int IndexOfNumberedParentheses(this string text, char begin = '(', char end = ')', bool hasAtLeastOneSpaceBeforeBeginning = true)
		{
			if (string.IsNullOrWhiteSpace(text))
				return -1;
			var length = text.Length;
			if (text[length - 1] != end)
				return -1;

			int indexOfBeginning;
			if (hasAtLeastOneSpaceBeforeBeginning)
			{
				indexOfBeginning = text.LastIndexOf(" " + begin, length - 1, StringComparison.Ordinal);
			}
			else
			{
				indexOfBeginning = text.LastIndexOf(begin, length - 1);
			}
			if (indexOfBeginning < 0)
				return -1;

			return indexOfBeginning;
		}

		#endregion

		#region Normalize Line Endings

		public static bool IsLineEndingNormalizationNeededCRLF(this string text)
		{
			if (text == null)
				return false;
			var lastWasCR = false;

			foreach (char c in text)
			{
				if (lastWasCR)
				{
					lastWasCR = false;
					if (c != '\n')
					{
						return true;
					}
				}
				else
				{
					switch (c)
					{
						case '\r':
							lastWasCR = true;
							break;

						case '\n':
							return true;
					}
				}
			}
			return lastWasCR;
		}

		public static string NormalizeLineEndingsCRLF(this string text)
		{
			if (string.IsNullOrEmpty(text))
				return text;

			// Check if normalization needed. Don't do any operation if not required.
			if (!text.IsLineEndingNormalizationNeededCRLF())
				return text;

			var stringBuilder = SharedStringBuilder.Value;
			lock (stringBuilder)
			{
				stringBuilder.Clear(); // Make sure it is clean before starting to use.

				var lastWasCR = false;
				foreach (char c in text)
				{
					if (lastWasCR)
					{
						lastWasCR = false;
						if (c == '\n')
						{
							continue; // Already written \r\n
						}
					}
					switch (c)
					{
						case '\r':
							stringBuilder.Append("\r\n");
							lastWasCR = true;
							break;

						case '\n':
							stringBuilder.Append("\r\n");
							break;

						default:
							stringBuilder.Append(c);
							break;
					}
				}

				var result = stringBuilder.ToString();
				ClearSharedStringBuilder(stringBuilder); // Make sure we will leave it clean after use.
				return result;
			}
		}

		#endregion

		#region StringBuilder

		public static void Clear(this StringBuilder stringBuilder)
		{
			stringBuilder.Length = 0;
			//stringBuilder.Capacity = 0; This is not a good idea since we want to reuse the already allocated memory
		}

		public static void AppendLine(this StringBuilder stringBuilder, string linePart1, string linePart2)
		{
			stringBuilder.Append(linePart1);
			stringBuilder.AppendLine(linePart2);
		}

		public static void AppendLine(this StringBuilder stringBuilder, string linePart1, string linePart2, string linePart3)
		{
			stringBuilder.Append(linePart1);
			stringBuilder.Append(linePart2);
			stringBuilder.AppendLine(linePart3);
		}

		#endregion

		#region Conversions - Predefined Numbers

		public static readonly string[] NumbersFrom0To9 =
		{
			"0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
		};

		public static readonly string[] NumbersFrom00To09 =
		{
			"00", "01", "02", "03", "04", "05", "06", "07", "08", "09",
		};

		public static readonly string[] NumbersFrom10To99 =
		{
			"10", "11", "12", "13", "14", "15", "16", "17", "18", "19",
			"20", "21", "22", "23", "24", "25", "26", "27", "28", "29",
			"30", "31", "32", "33", "34", "35", "36", "37", "38", "39",
			"40", "41", "42", "43", "44", "45", "46", "47", "48", "49",
			"50", "51", "52", "53", "54", "55", "56", "57", "58", "59",
			"60", "61", "62", "63", "64", "65", "66", "67", "68", "69",
			"70", "71", "72", "73", "74", "75", "76", "77", "78", "79",
			"80", "81", "82", "83", "84", "85", "86", "87", "88", "89",
			"90", "91", "92", "93", "94", "95", "96", "97", "98", "99"
		};

		public static string ToStringClampedBetween0To99(this int value)
		{
            if (value < 0) value = 0;
			else if (value > 99) value = 99;
			if (value < 10)
				return NumbersFrom0To9[value];
			else
				return NumbersFrom10To99[value - 10];
		}

		public static string ToStringClampedBetween00To99(this int value)
		{
            if (value < 0) value = 0;
            else if (value > 99) value = 99;
			if (value < 10)
				return NumbersFrom00To09[value];
			else
				return NumbersFrom10To99[value - 10];
		}

		#endregion

		#region Conversions - Int32 ToStringAsCharArray

		private static void _ToStringAsCharArray(Int32 value, char[] array, int endOffset, out int startIndex, out int length)
		{
			const int radix = 10;
			var i = array.Length - endOffset;
			var isNegative = (value < 0);
			if (value <= 0) // handles 0 and int.MinValue special cases
			{
				array[--i] = HexadecimalCharacters[-(value % radix)];
				value = -(value / radix);
			}

			while (value != 0)
			{
				array[--i] = HexadecimalCharacters[value % radix];
				value /= radix;
			}

			if (isNegative)
			{
				array[--i] = '-';
			}

			startIndex = i;
			length = array.Length - endOffset - i;
		}

		private static void _ToStringAsCharArray(Int32 value, char[] array, int endOffset, char thousandsSeparator, out int startIndex, out int length)
		{
			const int radix = 10;
			var i = array.Length - endOffset;
			var isNegative = (value < 0);
			var thousandsCounter = 3;
			if (value <= 0) // handles 0 and int.MinValue special cases
			{
				array[--i] = HexadecimalCharacters[-(value % radix)];
				value = -(value / radix);
				thousandsCounter--;
			}

			while (value != 0)
			{
				array[--i] = HexadecimalCharacters[value % radix];
				value /= radix;
				if (--thousandsCounter == 0 && value > 0)
				{
					array[--i] = thousandsSeparator;
					thousandsCounter = 3;
				}
			}

			if (isNegative)
			{
				array[--i] = '-';
			}

			startIndex = i;
			length = array.Length - endOffset - i;
		}

		/// <summary>
		/// Converts the Int to String in char[] form. This is especially useful where non-alloc conversions needed. 
		/// 
		/// Note that maximum length needed for char array is 10+1 (Digits+Minus).
		/// 
		/// Array length assumed to be enough and won't be checked because of performance concerns.
		/// </summary>
		public static void ToStringAsCharArray(this Int32 value, char[] array, out int startIndex, out int length)
		{
			_ToStringAsCharArray(value, array, 0, out startIndex, out length);
		}

		/// <summary>
		/// Converts the Int to String in char[] form. This is especially useful where non-alloc conversions needed. 
		/// 
		/// Note that maximum length needed for char array is 10+1+1 (Digits+Minus+Prefix).
		/// 
		/// Array length assumed to be enough and won't be checked because of performance concerns.
		/// </summary>
		public static void ToStringAsCharArrayWithPrefix(this Int32 value, char[] array, char prefix, out int startIndex, out int length)
		{
			_ToStringAsCharArray(value, array, 0, out startIndex, out length);
			startIndex--;
			length++;
			array[startIndex] = prefix;
		}

		/// <summary>
		/// Converts the Int to String in char[] form. This is especially useful where non-alloc conversions needed. 
		/// 
		/// Note that maximum length needed for char array is 10+1+1 (Digits+Minus+Postfix).
		/// 
		/// Array length assumed to be enough and won't be checked because of performance concerns.
		/// </summary>
		public static void ToStringAsCharArrayWithPostfix(this Int32 value, char[] array, char postfix, out int startIndex, out int length)
		{
			_ToStringAsCharArray(value, array, 1, out startIndex, out length);
			length++;
			array[array.Length - 1] = postfix;
		}

		/// <summary>
		/// Converts the Int to String in char[] form. This is especially useful where non-alloc conversions needed. 
		/// 
		/// Note that maximum length needed for char array is 10+1+3 (Digits+Minus+Commas).
		/// 
		/// Array length assumed to be enough and won't be checked because of performance concerns.
		/// </summary>
		public static void ToStringAsCharArray(this Int32 value, char[] array, char thousandsSeparator, out int startIndex, out int length)
		{
			_ToStringAsCharArray(value, array, 0, thousandsSeparator, out startIndex, out length);
		}

		/// <summary>
		/// Converts the Int to String in char[] form. This is especially useful where non-alloc conversions needed. 
		/// 
		/// Note that maximum length needed for char array is 10+1+3+1 (Digits+Minus+Commas+Prefix).
		/// 
		/// Array length assumed to be enough and won't be checked because of performance concerns.
		/// </summary>
		public static void ToStringAsCharArrayWithPrefix(this Int32 value, char[] array, char prefix, char thousandsSeparator, out int startIndex, out int length)
		{
			_ToStringAsCharArray(value, array, 0, thousandsSeparator, out startIndex, out length);
			startIndex--;
			length++;
			array[startIndex] = prefix;
		}

		/// <summary>
		/// Converts the Int to String in char[] form. This is especially useful where non-alloc conversions needed. 
		/// 
		/// Note that maximum length needed for char array is 10+1+3+1 (Digits+Minus+Commas+Postfix).
		/// 
		/// Array length assumed to be enough and won't be checked because of performance concerns.
		/// </summary>
		public static void ToStringAsCharArrayWithPostfix(this Int32 value, char[] array, char postfix, char thousandsSeparator, out int startIndex, out int length)
		{
			_ToStringAsCharArray(value, array, 1, thousandsSeparator, out startIndex, out length);
			length++;
			array[array.Length - 1] = postfix;
		}

		#endregion

		#region Conversions - Int64 ToStringAsCharArray

		private static void _ToStringAsCharArray(Int64 value, char[] array, int endOffset, out int startIndex, out int length)
		{
			const int radix = 10;
			var i = array.Length - endOffset;
			var isNegative = (value < 0);
			if (value <= 0) // handles 0 and int.MinValue special cases
			{
				array[--i] = HexadecimalCharacters[-(value % radix)];
				value = -(value / radix);
			}

			while (value != 0)
			{
				array[--i] = HexadecimalCharacters[value % radix];
				value /= radix;
			}

			if (isNegative)
			{
				array[--i] = '-';
			}

			startIndex = i;
			length = array.Length - endOffset - i;
		}

		private static void _ToStringAsCharArray(Int64 value, char[] array, int endOffset, char thousandsSeparator, out int startIndex, out int length)
		{
			const int radix = 10;
			var i = array.Length - endOffset;
			var isNegative = (value < 0);
			var thousandsCounter = 3;
			if (value <= 0) // handles 0 and int.MinValue special cases
			{
				array[--i] = HexadecimalCharacters[-(value % radix)];
				value = -(value / radix);
				thousandsCounter--;
			}

			while (value != 0)
			{
				array[--i] = HexadecimalCharacters[value % radix];
				value /= radix;
				if (--thousandsCounter == 0 && value > 0)
				{
					array[--i] = thousandsSeparator;
					thousandsCounter = 3;
				}
			}

			if (isNegative)
			{
				array[--i] = '-';
			}

			startIndex = i;
			length = array.Length - endOffset - i;
		}

		/// <summary>
		/// Converts the Int to String in char[] form. This is especially useful where non-alloc conversions needed. 
		/// 
		/// Note that maximum length needed for char array is 19+1 (Digits+Minus).
		/// 
		/// Array length assumed to be enough and won't be checked because of performance concerns.
		/// </summary>
		public static void ToStringAsCharArray(this Int64 value, char[] array, out int startIndex, out int length)
		{
			_ToStringAsCharArray(value, array, 0, out startIndex, out length);
		}

		/// <summary>
		/// Converts the Int to String in char[] form. This is especially useful where non-alloc conversions needed. 
		/// 
		/// Note that maximum length needed for char array is 19+1+1 (Digits+Minus+Prefix).
		/// 
		/// Array length assumed to be enough and won't be checked because of performance concerns.
		/// </summary>
		public static void ToStringAsCharArrayWithPrefix(this Int64 value, char[] array, char prefix, out int startIndex, out int length)
		{
			_ToStringAsCharArray(value, array, 0, out startIndex, out length);
			startIndex--;
			length++;
			array[startIndex] = prefix;
		}

		/// <summary>
		/// Converts the Int to String in char[] form. This is especially useful where non-alloc conversions needed. 
		/// 
		/// Note that maximum length needed for char array is 19+1+1 (Digits+Minus+Postfix).
		/// 
		/// Array length assumed to be enough and won't be checked because of performance concerns.
		/// </summary>
		public static void ToStringAsCharArrayWithPostfix(this Int64 value, char[] array, char postfix, out int startIndex, out int length)
		{
			_ToStringAsCharArray(value, array, 1, out startIndex, out length);
			length++;
			array[array.Length - 1] = postfix;
		}

		/// <summary>
		/// Converts the Int to String in char[] form. This is especially useful where non-alloc conversions needed. 
		/// 
		/// Note that maximum length needed for char array is 19+1+6 (Digits+Minus+Commas).
		/// 
		/// Array length assumed to be enough and won't be checked because of performance concerns.
		/// </summary>
		public static void ToStringAsCharArray(this Int64 value, char[] array, char thousandsSeparator, out int startIndex, out int length)
		{
			_ToStringAsCharArray(value, array, 0, thousandsSeparator, out startIndex, out length);
		}

		/// <summary>
		/// Converts the Int to String in char[] form. This is especially useful where non-alloc conversions needed. 
		/// 
		/// Note that maximum length needed for char array is 19+1+6+1 (Digits+Minus+Commas+Prefix).
		/// 
		/// Array length assumed to be enough and won't be checked because of performance concerns.
		/// </summary>
		public static void ToStringAsCharArrayWithPrefix(this Int64 value, char[] array, char prefix, char thousandsSeparator, out int startIndex, out int length)
		{
			_ToStringAsCharArray(value, array, 0, thousandsSeparator, out startIndex, out length);
			startIndex--;
			length++;
			array[startIndex] = prefix;
		}

		/// <summary>
		/// Converts the Int to String in char[] form. This is especially useful where non-alloc conversions needed. 
		/// 
		/// Note that maximum length needed for char array is 19+1+3+1 (Digits+Minus+Commas+Postfix).
		/// 
		/// Array length assumed to be enough and won't be checked because of performance concerns.
		/// </summary>
		public static void ToStringAsCharArrayWithPostfix(this Int64 value, char[] array, char postfix, char thousandsSeparator, out int startIndex, out int length)
		{
			_ToStringAsCharArray(value, array, 1, thousandsSeparator, out startIndex, out length);
			length++;
			array[array.Length - 1] = postfix;
		}

		#endregion

		#region Conversions - Vector2/Vector3/Quaternion

#if UNITY

		public static string ToSerializableString(this Vector2 val)
		{
			return val.x + " " + val.y;
		}

		public static string ToSerializableString(this Vector3 val)
		{
			return val.x + " " + val.y + " " + val.z;
		}

		public static string ToSerializableString(this Vector4 val)
		{
			return val.x + " " + val.y + " " + val.z + " " + val.w;
		}

		public static string ToSerializableString(this Quaternion val)
		{
			return val.x + " " + val.y + " " + val.z + " " + val.w;
		}

		public static string ToStringAngleAxis(this Quaternion val)
		{
			val.ToAngleAxis(out var angle, out var axis);
			return angle + " " + axis.x + " " + axis.y + " " + axis.z;
		}

		public static string ToStringAngleAxisDecorated(this Quaternion val)
		{
			val.ToAngleAxis(out var angle, out var axis);
			return "Angle " + angle.ToString("N1") + " Axis " + axis.ToString();
		}

		public static string ToStringAngleAxisEulerDecorated(this Quaternion val, string separatorBeforeEuler = "\t")
		{
			val.ToAngleAxis(out var angle, out var axis);
			return "Angle " + angle.ToString("N1") + " Axis " + axis.ToString() + separatorBeforeEuler + " Euler " + val.eulerAngles.ToString();
		}

		public static Vector2 ParseVector2(this string text)
		{
			var parts = text.Split(' ');
			return new Vector2(float.Parse(parts[0]), float.Parse(parts[1]));
		}

		public static Vector3 ParseVector3(this string text)
		{
			var parts = text.Split(' ');
			return new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
		}

		public static Vector4 ParseVector4(this string text)
		{
			var parts = text.Split(' ');
			return new Vector4(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
		}

		public static Quaternion ParseQuaternion(this string text)
		{
			var parts = text.Split(' ');
			return new Quaternion(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
		}

#endif

		#endregion

		#region Conversions - Hex

		public static bool IsHexString(this string text, bool treatEmptyAsHex)
		{
			if (string.IsNullOrEmpty(text))
				return treatEmptyAsHex;
			for (int i = 0; i < text.Length; i++)
			{
				var character = text[i];
				bool isHexChar = (character >= '0' && character <= '9') ||
				                 (character >= 'a' && character <= 'f') ||
				                 (character >= 'A' && character <= 'F');

				if (!isHexChar)
					return false;
			}
			return true;
		}

		public static string ToHexString(this byte value, bool decorated = false) { return string.Format(decorated ? "{0x0:X}" : "{0:X}", value); }
		public static string ToHexString(this Int16 value, bool decorated = false) { return string.Format(decorated ? "{0x0:X}" : "{0:X}", value); }
		public static string ToHexString(this Int32 value, bool decorated = false) { return string.Format(decorated ? "{0x0:X}" : "{0:X}", value); }
		public static string ToHexString(this Int64 value, bool decorated = false) { return string.Format(decorated ? "{0x0:X}" : "{0:X}", value); }
		public static string ToHexString(this UInt16 value, bool decorated = false) { return string.Format(decorated ? "{0x0:X}" : "{0:X}", value); }
		public static string ToHexString(this UInt32 value, bool decorated = false) { return string.Format(decorated ? "{0x0:X}" : "{0:X}", value); }
		public static string ToHexString(this UInt64 value, bool decorated = false) { return string.Format(decorated ? "{0x0:X}" : "{0:X}", value); }

		public static string ToHexString(this string text, bool uppercase = false)
		{
			if (text == null)
				return null;
			var stringBuilder = new StringBuilder(text.Length * 3);
			var format = uppercase ? "{0:X2} " : "{0:x2} ";
			for (int i = 0; i < text.Length; i++)
				stringBuilder.AppendFormat(format, (byte)text[i]);
			return stringBuilder.ToString();
		}

		public static string ToHexString(this byte[] bytes, bool uppercase = false)
		{
			var stringBuilder = new StringBuilder(bytes.Length * 3);
			var format = uppercase ? "{0:X2} " : "{0:x2} ";
			for (int i = 0; i < bytes.Length; i++)
				stringBuilder.AppendFormat(format, bytes[i]);
			return stringBuilder.ToString();
		}

		public static string ToHexString(this byte[] bytes, int bytesToRead, bool uppercase = false)
		{
			var stringBuilder = new StringBuilder(bytesToRead * 3);
			var format = uppercase ? "{0:X2} " : "{0:x2} ";
			for (int i = 0; i < bytesToRead; i++)
				stringBuilder.AppendFormat(format, bytes[i]);
			return stringBuilder.ToString();
		}

		public static string ToHexStringCombined(this string text, bool uppercase = false)
		{
			if (text == null)
				return null;
			var stringBuilder = new StringBuilder(text.Length * 2);
			var format = uppercase ? "X2" : "x2";
			for (int i = 0; i < text.Length; i++)
				stringBuilder.Append(((byte)text[i]).ToString(format));
			return stringBuilder.ToString();
		}

		public static string ToHexStringCombined(this byte[] bytes, bool uppercase = false)
		{
			var stringBuilder = new StringBuilder(bytes.Length * 2);
			var format = uppercase ? "X2" : "x2";
			for (int i = 0; i < bytes.Length; i++)
				stringBuilder.Append(bytes[i].ToString(format));
			return stringBuilder.ToString();
		}

		public static string ToHexStringCombined(this byte[] bytes, int bytesToRead, bool uppercase = false)
		{
			var stringBuilder = new StringBuilder(bytes.Length * 2);
			var format = uppercase ? "X2" : "x2";
			for (int i = 0; i < bytesToRead; i++)
				stringBuilder.Append(bytes[i].ToString(format));
			return stringBuilder.ToString();
		}

		public static string ToHexStringFancy(this string text, bool uppercase = false)
		{
			if (text == null)
				return null;
			var stringBuilder = new StringBuilder(text.Length * 3);
			var format = uppercase ? "({0})0x{1:X2} " : "({0})0x{1:x2} ";
			for (int i = 0; i < text.Length; i++)
			{
				var value = text[i];
				stringBuilder.AppendFormat(format, char.IsControl(value) ? "" : char.ToString(value), (byte)value);
			}
			return stringBuilder.ToString();
		}

		public static string ToHexStringFancy(this byte[] bytes, bool uppercase = false)
		{
			var stringBuilder = new StringBuilder(bytes.Length * 3);
			var format = uppercase ? "({0})0x{1:X2} " : "({0})0x{1:x2} ";
			for (int i = 0; i < bytes.Length; i++)
			{
				var value = (char)bytes[i];
				stringBuilder.AppendFormat(format, char.IsControl(value) ? "" : char.ToString(value), (byte)value);
			}
			return stringBuilder.ToString();
		}

		public static string ToHexStringFancy(this byte[] bytes, int bytesToRead, bool uppercase = false)
		{
			var stringBuilder = new StringBuilder(bytesToRead * 3);
			var format = uppercase ? "({0})0x{1:X2} " : "({0})0x{1:x2} ";
			for (int i = 0; i < bytesToRead; i++)
			{
				var value = (char)bytes[i];
				stringBuilder.AppendFormat(format, char.IsControl(value) ? "" : char.ToString(value), (byte)value);
			}
			return stringBuilder.ToString();
		}

		#endregion

		#region Conversions - Binary

		public static bool IsBinaryString(this string value)
		{
			for (int i = 0; i < value.Length; i++)
			{
				var character = value[i];
				if (character != '0' && character != '1')
					return false;
			}
			return true;
		}

		public static char ToBinaryCharacter(this bool value) { return value ? '1' : '0'; }
		public static string ToBinaryString(this bool value) { return value ? "1" : "0"; }
		public static string ToBinaryString(this sbyte value) { return Convert.ToString(value, 2); }
		public static string ToBinaryString(this byte value) { return Convert.ToString(value, 2); }
		public static string ToBinaryString(this Int16 value) { return Convert.ToString(value, 2); }
		public static string ToBinaryString(this Int32 value) { return Convert.ToString(value, 2); }
		public static string ToBinaryString(this Int64 value) { return Convert.ToString(value, 2); }
		public static string ToBinaryString(this UInt16 value) { return Convert.ToString((Int16)value, 2); }
		public static string ToBinaryString(this UInt32 value) { return Convert.ToString((Int32)value, 2); }
		public static string ToBinaryString(this UInt64 value) { return Convert.ToString((Int64)value, 2); }

		public static string ToBinaryString(this byte[] bytes)
		{
			var stringBuilder = new StringBuilder(bytes.Length);
			for (int i = 0; i < bytes.Length; i++)
			{
				stringBuilder.Append(bytes[i] != 0 ? "1" : "0");
			}
			return stringBuilder.ToString();
		}

		public static string ToBinaryString(this byte[] bytes, int bytesToRead)
		{
			var stringBuilder = new StringBuilder(bytes.Length);
			for (int i = 0; i < bytesToRead; i++)
			{
				stringBuilder.Append(bytes[i] != 0 ? "1" : "0");
			}
			return stringBuilder.ToString();
		}

		#endregion

		#region Conversions - ASCII

		public static string ConvertToAscii(this string text)
		{
			Encoding ascii = Encoding.ASCII;
			Encoding unicode = Encoding.UTF8;

			var utf8Array = Encoding.UTF8.GetBytes(text);
			var asciiArray = Encoding.Convert(unicode, ascii, utf8Array);
			return ascii.GetString(asciiArray);
		}

		#endregion

		#region Conversions - Bool

		public static string ToIntString(this bool me)
		{
			return me ? "1" : "0";
		}

		#endregion

		#region Conversions - String

		public static string ToEmptyIfNull(this string text)
		{
			if (text == null)
				return "";
			return text;
		}

		#endregion

		#region Conversions - Time

		public static string ToStringMinutesSecondsMillisecondsFromSeconds(this double totalSeconds)
		{
			//TimeSpan t = TimeSpan.FromSeconds(seconds);
			//return string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D2}", t.Hours, t.Minutes, t.Seconds, t.Milliseconds);
			//return string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);

			// Round to milliseconds
			totalSeconds = (totalSeconds * 1000.0).RoundToInt() / 1000.0;

			var minutes = (long)(totalSeconds / 60);
			var seconds = (int)(totalSeconds % 60);
			var milliseconds = ((totalSeconds - Math.Truncate(totalSeconds)) * 1000).RoundToInt();
			return $"{minutes}:{seconds:00}.{milliseconds:000}";
		}

		public static string ToStringMillisecondsFromSeconds(this double totalSeconds)
		{
			return (totalSeconds * 1000).ToString("0.000");
		}

		public static string ToStringMicrosecondsFromSeconds(this double totalSeconds)
		{
			return (totalSeconds * 1000000).ToString("0.000");
		}

		#endregion

		#region Conversions - char[]

		public static string ConvertToString(this char[] chars, int startIndex, int length)
		{
			var stringBuilder = SharedStringBuilder.Value;
			lock (stringBuilder)
			{
				stringBuilder.Clear(); // Make sure it is clean before starting to use.

				var endIndex = startIndex + length;
				for (int i = startIndex; i < chars.Length && i < endIndex && (uint)chars[i] > 0U; ++i)
					stringBuilder.Append(chars[i]);

				var result = stringBuilder.ToString();
				ClearSharedStringBuilder(stringBuilder); // Make sure we will leave it clean after use.
				return result;
			}
		}

		public static int CopyTo(this string value, char[] destination)
		{
			var length = value.Length;
			value.CopyTo(0, destination, 0, length);
			return length;
		}

		#endregion

		#region Conversions - Dictionary

		public static string ToJoinedString<TKey, TValue>(this ICollection<KeyValuePair<TKey, TValue>> dictionary, string itemSeparator = "\n", string keyValueSeparator = ": ")
		{
			var stringBuilder = new StringBuilder();
			var first = true;
			foreach (var item in dictionary)
			{
				if (first)
				{
					first = false;
				}
				else
				{
					stringBuilder.Append(itemSeparator);
				}
				stringBuilder.Append(item.Key + keyValueSeparator + item.Value);
			}
			return stringBuilder.ToString();
		}

		#endregion

		#region Conversions - Base64

		public static string ToBase64(this string plainText)
		{
			var bytes = Encoding.UTF8.GetBytes(plainText);
			return Convert.ToBase64String(bytes);
		}

		public static string FromBase64(this string base64EncodedData)
		{
			var bytes = Convert.FromBase64String(base64EncodedData);
			return Encoding.UTF8.GetString(bytes);
		}

		#endregion

		#region Whitespaces

		/// <summary>
		/// Source: https://www.codeproject.com/Articles/1014073/Fastest-method-to-remove-all-whitespace-from-Strin
		/// </summary>
		public static string TrimAll(this string value)
		{
			if (value == null)
				return null;
			var length = value.Length;
			var copy = value.ToCharArray();

			int iDestination = 0;
			for (int iSource = 0; iSource < length; iSource++)
			{
				var ch = copy[iSource];
				switch (ch)
				{
					case '\u0020':
					case '\u00A0':
					case '\u1680':
					case '\u2000':
					case '\u2001':
					case '\u2002':
					case '\u2003':
					case '\u2004':
					case '\u2005':
					case '\u2006':
					case '\u2007':
					case '\u2008':
					case '\u2009':
					case '\u200A':
					case '\u202F':
					case '\u205F':
					case '\u3000':
					case '\u2028':
					case '\u2029':
					case '\u0009':
					case '\u000A':
					case '\u000B':
					case '\u000C':
					case '\u000D':
					case '\u0085':
						continue;

					default:
						copy[iDestination++] = ch;
						break;
				}
			}
			return new string(copy, 0, iDestination);
		}

		#endregion

		#region Wildcard Matching (Fast)

		// TODO: Need a fast wildcard matching algorithm that will replace CheckWildcardMatchingRegex. Needs extensive testing. Also needs the ability to be precompiled (like in regexp) for extra speed.

		#endregion

		#region Serialization

		public static string Serialize<T>(this T array, char separator = ',', int capacity = 0) where T : ICollection
		{
			if (array.Count == 0)
				return "";

			var stringBuilder = capacity > 0 ? new StringBuilder(capacity) : new StringBuilder();

			int i = array.Count;
			foreach (var elem in array)
			{
				stringBuilder.Append(elem.ToString());
				if (--i > 0)
				{
					stringBuilder.Append(separator);
				}
			}

			return stringBuilder.ToString();
		}

		public static string Serialize<T>(this T[] array, Func<T, string> customSerialize, char separator = ',', int capacity = 0)
		{
			if (array.Length == 0)
				return "";

			var stringBuilder = capacity > 0 ? new StringBuilder(capacity) : new StringBuilder();

			for (int i = 0; i < array.Length; i++)
			{
				stringBuilder.Append(customSerialize(array[i]));
				if (i < array.Length - 1)
				{
					stringBuilder.Append(separator);
				}
			}

			return stringBuilder.ToString();
		}

		public static ICollection<byte> DeserializeByte(this string text, char separator = ',')
		{
			if (text.Length == 0)
				return null;

			string[] listString = text.Split(separator);
			var array = new byte[listString.Length];

			for (int i = 0; i < listString.Length; i++)
				array[i] = byte.Parse(listString[i]);

			return array;
		}

		public static ICollection<int> DeserializeInt(this string text, char separator = ',')
		{
			if (text.Length == 0)
				return null;

			string[] listString = text.Split(separator);
			var array = new int[listString.Length];

			for (int i = 0; i < listString.Length; i++)
				array[i] = Int32.Parse(listString[i]);

			return array;
		}

		public static ICollection<float> DeserializeFloat(this string text, char separator = ',')
		{
			if (text.Length == 0)
				return null;

			string[] listString = text.Split(separator);
			var array = new float[listString.Length];

			for (int i = 0; i < listString.Length; i++)
				array[i] = Single.Parse(listString[i]);

			return array;
		}

		public static ICollection<double> DeserializeDouble(this string text, char separator = ',')
		{
			if (text.Length == 0)
				return null;

			string[] listString = text.Split(separator);
			var array = new double[listString.Length];

			for (int i = 0; i < listString.Length; i++)
				array[i] = Double.Parse(listString[i]);

			return array;
		}

#if UNITY

		public static ICollection<Vector2> DeserializeVector2(this string text, char separator = ',')
		{
			if (text.Length == 0)
				return null;

			string[] listString = text.Split(separator);
			var array = new Vector2[listString.Length];

			for (int i = 0; i < listString.Length; i++)
				array[i] = ParseVector2(listString[i]);

			return array;
		}

		public static ICollection<Vector3> DeserializeVector3(this string text, char separator = ',')
		{
			if (text.Length == 0)
				return null;

			string[] listString = text.Split(separator);
			var array = new Vector3[listString.Length];

			for (int i = 0; i < listString.Length; i++)
				array[i] = ParseVector3(listString[i]);

			return array;
		}

		public static ICollection<Quaternion> DeserializeQuaternion(this string text, char separator = ',')
		{
			if (text.Length == 0)
				return null;

			string[] listString = text.Split(separator);
			var array = new Quaternion[listString.Length];

			for (int i = 0; i < listString.Length; i++)
				array[i] = ParseQuaternion(listString[i]);

			return array;
		}

#endif

		#endregion

		#region Hash

		public static Int64 GetInt64HashCode(this string text)
		{
			if (string.IsNullOrEmpty(text))
				return 0;

			//Unicode Encode Covering all characterset
			byte[] byteContents = Encoding.Unicode.GetBytes(text);

			HashAlgorithm algorithm = MD5.Create();
			byte[] hashText = algorithm.ComputeHash(byteContents);
			return BitConverter.ToInt64(hashText, 3); // Grab 64 bits from somewhere middle

			// SHA256 is not supported on Windows XP
			//SHA256 hash = new SHA256CryptoServiceProvider();
			//byte[] hashText = hash.ComputeHash(byteContents);
			//32Byte hashText separate
			//hashCodeStart = 0~7  8Byte
			//hashCodeMedium = 8~23  8Byte
			//hashCodeEnd = 24~31  8Byte
			//and Fold
			//Int64 hashCodeStart = BitConverter.ToInt64(hashText, 0);
			//Int64 hashCodeMedium = BitConverter.ToInt64(hashText, 8);
			//Int64 hashCodeEnd = BitConverter.ToInt64(hashText, 24);
			//return hashCodeStart ^ hashCodeMedium ^ hashCodeEnd;
		}

		// A hash algorithm that is guaranteed to be never modified in future.
		// The algorithm is the exact copy of string.GetHashCode() grabbed from
		// Reference Source Codes for .NET 4.7.2.
		// https://referencesource.microsoft.com/#mscorlib/system/string.cs
		//
		// Note that .NET implementation may change in future, as it has before.
		// But GetHashCodeGuaranteed is here to stay.
		public static int GetHashCodeGuaranteed([NotNull] this string str)
		{
			if (str == null)
				throw new ArgumentNullException(nameof(str));
			unsafe
			{
				fixed (char* src = str)
				{
					//Contract.Assert(src[this.Length] == '\0', "src[this.Length] == '\\0'");
					//Contract.Assert( ((int)src)%4 == 0, "Managed string should start at 4 bytes boundary");

					//#if WIN32
					int hash1 = (5381 << 16) + 5381;
					//#else
					//int hash1 = 5381;
					//#endif
					int hash2 = hash1;

					//#if WIN32
					// 32 bit machines. 
					int* pint = (int*)src;
					int len = str.Length;
					while (len > 2)
					{
						hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ pint[0];
						hash2 = ((hash2 << 5) + hash2 + (hash2 >> 27)) ^ pint[1];
						pint += 2;
						len -= 4;
					}

					if (len > 0)
					{
						hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ pint[0];
					}
					//#else
					//int c;
					//char* s = src;
					//while ((c = s[0]) != 0)
					//{
					//	hash1 = ((hash1 << 5) + hash1) ^ c;
					//	c = s[1];
					//	if (c == 0)
					//		break;
					//	hash2 = ((hash2 << 5) + hash2) ^ c;
					//	s += 2;
					//}
					//#endif
					//#if DEBUG
					//// We want to ensure we can change our hash function daily.
					//// This is perfectly fine as long as you don't persist the
					//// value from GetHashCode to disk or count on String A 
					//// hashing before string B.  Those are bugs in your code.
					//hash1 ^= ThisAssembly.DailyBuildNumber;
					//#endif
					return hash1 + (hash2 * 1566083941);
				}
			}
		}

		#endregion

		#region Smart Format

		public static void SmartFormat(ref string text, bool trim = true, bool trimEndOfEachLine = true, bool normalizeLineEndings = true, int maxAllowedConsecutiveLineEndings = -1, int maxLength = -1)
		{
			// No null
			if (text == null)
			{
				text = "";
				return;
			}

			// Nothing to do with empty
			if (text.Length == 0)
				return;

			// Trim
			if (trim)
			{
				text = text.Trim();
				if (text.Length == 0)
					return;
			}

			// Normalize line endings
			if (normalizeLineEndings)
			{
				text = text.NormalizeLineEndingsCRLF().Replace("\r\n", "\n");
			}

			// Trim end of each line (do not trim the beginning of the line)
			if (trimEndOfEachLine)
			{
				var lines = text.Split('\n');
				if (lines.Length > 1)
				{
					for (var i = 0; i < lines.Length; i++)
					{
						lines[i] = lines[i].TrimEnd();
					}
					text = string.Join("\n", lines);
				}
				else
				{
					text = text.TrimEnd();
				}
			}

			// Max allowed consecutive line endings
			if (maxAllowedConsecutiveLineEndings >= 0)
			{
				if (maxAllowedConsecutiveLineEndings == 0)
				{
					text = text.Replace("\n", "");
				}
				else
				{
					var oneBig = new string('\n', maxAllowedConsecutiveLineEndings + 1);
					var expected = new string('\n', maxAllowedConsecutiveLineEndings);
					var again = true;
					while (again)
					{
						var newText = text.Replace(oneBig, expected);
						again = newText.Length != text.Length;
						text = newText;
					}
				}
			}

			// Max length (must be done after trimming)
			if (maxLength >= 0)
			{
				if (text.Length > maxLength)
					text = text.Substring(0, maxLength);
			}
		}

		#endregion

		#region Count Lines

		public static Int64 CountLines(this string text)
		{
			if (string.IsNullOrEmpty(text))
				return 0;

			Int64 count = 1;
			for (var i = 0; i < text.Length; i++)
			{
				if (text[i] == '\n')
					count++;
			}
			return count;
		}

		#endregion

		#region Percentage Bar

		public static string ToStringAsPercentageBar(this double value, int barLength = 20)
		{
			return ToStringAsPercentageBar((float)value, barLength);
		}

		public static string ToStringAsPercentageBar(this float value, int barLength = 20)
		{
			var stringBuilder = SharedStringBuilder.Value;
			lock (stringBuilder)
			{
				stringBuilder.Clear(); // Make sure it is clean before starting to use.

				if (barLength > 0)
				{
					stringBuilder.Append('[');
					var clampedValue = value < 0f ? 0f : value > 1f ? 1f : value;
					var filledCount = (int)(clampedValue * barLength);
					if (filledCount == 0 && clampedValue > 0.00001f)
					{
						filledCount = 1; // Always show some bar piece if the value is greater than zero.
					}
					var emptyCount = barLength - filledCount;
					for (int i = 0; i < filledCount; i++)
					{
						stringBuilder.Append('\u2588'); // A full square character
					}
					for (int i = 0; i < emptyCount; i++)
					{
						stringBuilder.Append('\u2500'); // A full left-to-right stretching dash character
					}
					stringBuilder.Append("] ");
				}
				stringBuilder.Append(value.ToString("P1"));

				var result = stringBuilder.ToString();
				ClearSharedStringBuilder(stringBuilder); // Make sure we will leave it clean after use.
				return result;
			}
		}

		#endregion

		#region Singular/Plural

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ToStringWithPluralWord(this int value, string singularWord, string pluralOrZeroWord) { return ToStringWithPluralWord((long)value, singularWord, pluralOrZeroWord); }
		public static string ToStringWithPluralWord(this long value, string singularWord, string pluralOrZeroWord)
		{
			return value.ToString() + " " + (value == 1 ? singularWord : pluralOrZeroWord);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ToStringWithEnglishPluralPostfix(this int value, string postfix) { return ToStringWithEnglishPluralPostfix((long)value, postfix); }

		public static string ToStringWithEnglishPluralPostfix(this long value, string postfix)
		{
			return value == 1
				? value.ToString() + " " + postfix
				: value.ToString() + " " + postfix + "s";
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ToStringWithEnglishPluralPostfix(this int value, string postfix, char valueShell) { return ToStringWithEnglishPluralPostfix((long)value, postfix, valueShell); }

		public static string ToStringWithEnglishPluralPostfix(this long value, string postfix, char valueShell)
		{
			return value == 1
				? valueShell + value.ToString() + valueShell + " " + postfix
				: valueShell + value.ToString() + valueShell + " " + postfix + "s";
		}

		#endregion

		#region Current Culture Info

		static StringTools()
		{
			CurrentCultureInfo = Thread.CurrentThread.CurrentCulture;
			CurrentNumberFormatInfo = NumberFormatInfo.CurrentInfo;
			NumberGroupSeparator = Convert.ToChar(CurrentNumberFormatInfo.NumberGroupSeparator);
			NumberDecimalSeparator = Convert.ToChar(CurrentNumberFormatInfo.NumberDecimalSeparator);

			// TODO: This seems like not working (at least in Unity Editor). Changing the system regional settings does not work. Needs more tests on other platforms.
			//Log.Info($"Separators: {CurrentCultureInfo.NumberFormat.NumberDecimalSeparator}{CurrentCultureInfo.NumberFormat.NumberGroupSeparator}{CurrentNumberFormatInfo.NumberDecimalSeparator}{CurrentNumberFormatInfo.NumberGroupSeparator}");
		}

		public static readonly CultureInfo CurrentCultureInfo;
		public static readonly NumberFormatInfo CurrentNumberFormatInfo;
		public static readonly char NumberGroupSeparator;
		public static readonly char NumberDecimalSeparator;

		#endregion
	}

}
