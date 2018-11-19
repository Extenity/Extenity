using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using Extenity.MathToolbox;
using JetBrains.Annotations;
using UnityEngine;

namespace Extenity.DataToolbox
{

	public static class StringTools
	{
		#region Shared Objects

		private static readonly ThreadLocal<StringBuilder> SharedStringBuilder = new ThreadLocal<StringBuilder>(() => new StringBuilder(SharedStringBuilderInitialCapacity));
		private const int SharedStringBuilderInitialCapacity = 1000;
		private const int SharedStringBuilderCapacityTolerance = 100000; // If the StringBuilder exceeds this capacity, it will be freed.

		private static void ClearSharedStringBuilder(StringBuilder stringBuilder)
		{
			if (stringBuilder.Capacity > SharedStringBuilderCapacityTolerance)
			{
				// If this happens regularly, consider increasing the tolerance.
				Log.DebugWarning($"Shared StringBuilder size '{stringBuilder.Capacity}' exceeded the tolerance '{SharedStringBuilderCapacityTolerance}'.");
				stringBuilder.Capacity = SharedStringBuilderInitialCapacity;
			}
			else
			{
				stringBuilder.Clear();
			}
		}

		#endregion

		#region Constants

		public static readonly char[] LineEndingCharacters = { '\r', '\n' };
		public static readonly char[] NumericCharacters = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
		public static readonly char[] HexadecimalCharacters = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

		#endregion

		#region Equals

		public static bool EqualsOrBothEmpty(this string str1, string str2)
		{
			if (string.IsNullOrEmpty(str1))
			{
				return string.IsNullOrEmpty(str2);
			}
			return str1.Equals(str2);
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
			}
			while (i >= 0);
			return count;
		}

		// Got from http://stackoverflow.com/questions/541954/how-would-you-count-occurences-of-a-string-within-a-string-c
		public static int CountSubstrings(this string subtext, string text)
		{
			return (text.Length - text.Replace(subtext, "").Length) / subtext.Length;
		}

		public static bool IsAlphaNumeric(this string str, bool allowSpace = false, bool ensureStartsWithAlpha = false)
		{
			if (string.IsNullOrEmpty(str))
				return false;

			if (ensureStartsWithAlpha)
			{
				if (!char.IsLetter(str[0]))
					return false;
			}

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
				if (char.IsNumber(str[i]) || str[i] == '.' || (allowSpace && str[i] == ' '))
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

		/// <summary>
		/// Allows using StringComparison with Contains check, which is not supported in .NET by default.
		/// </summary>
		public static bool Contains(this string text, string value, StringComparison comparisonType)
		{
			return text.IndexOf(value, comparisonType) >= 0;
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

			lock (SharedStringBuilder)
			{
				// Check if normalization needed. Don't do any operation if not required.
				if (!text.IsLineEndingNormalizationNeededCRLF())
				{
					return text;
				}

				var stringBuilder = SharedStringBuilder.Value;
				stringBuilder.Clear();

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
				ClearSharedStringBuilder(stringBuilder);
				return result;
			}
		}

		#endregion

		#region StringBuilder

		public static void Clear(this StringBuilder value)
		{
			value.Length = 0;
			//value.Capacity = 0; This is not a good idea since we want to reuse the already allocated memory
		}

		#endregion

		#region Conversions - Int ToStringAsCharArray

		/// <summary>
		/// Converts the Int to String in char[] form. This is especially useful where non-alloc conversions needed. 
		/// 
		/// Note that maximum length needed for char array is 10+1 (Digits+Minus).
		/// 
		/// Array length assumed to be enough and won't be checked because of performance concerns.
		/// </summary>
		public static void ToStringAsCharArray(this Int32 value, char[] array, out int startIndex, out int length)
		{
			const int radix = 10;
			var i = array.Length;
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
			length = array.Length - i;
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
			const int radix = 10;
			var i = array.Length;
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
			length = array.Length - i;
		}

		#endregion

		#region Conversions - Int ToStringAsCharArray with ThousandsSeparator

		/// <summary>
		/// Converts the Int to String in char[] form. This is especially useful where non-alloc conversions needed. 
		/// 
		/// Note that maximum length needed for char array is 10+1+3 (Digits+Minus+Commas).
		/// 
		/// Array length assumed to be enough and won't be checked because of performance concerns.
		/// </summary>
		public static void ToStringAsCharArray(this Int32 value, char[] array, char thousandsSeparator, out int startIndex, out int length)
		{
			const int radix = 10;
			var i = array.Length;
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
			length = array.Length - i;
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
			const int radix = 10;
			var i = array.Length;
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
			length = array.Length - i;
		}

		#endregion

		#region Conversions - Int ToStringAsCharArray with Radix

		/// <summary>
		/// Converts the Int to String in char[] form. This is especially useful where non-alloc conversions needed. 
		/// 
		/// Array length assumed to be enough and won't be checked because of performance concerns.
		/// </summary>
		public static void ToStringAsCharArray(this Int32 value, int radix, char[] array, out int startIndex, out int length)
		{
			var i = array.Length;
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
			length = array.Length - i;
		}

		/// <summary>
		/// Converts the Int to String in char[] form. This is especially useful where non-alloc conversions needed. 
		/// 
		/// Array length assumed to be enough and won't be checked because of performance concerns.
		/// </summary>
		public static void ToStringAsCharArray(this Int64 value, int radix, char[] array, out int startIndex, out int length)
		{
			var i = array.Length;
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
			length = array.Length - i;
		}

		#endregion

		#region Conversions - Vector2/Vector3/Quaternion

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
			float angle;
			Vector3 axis;
			val.ToAngleAxis(out angle, out axis);
			return angle + " " + axis.x + " " + axis.y + " " + axis.z;
		}

		public static string ToStringAngleAxisDecorated(this Quaternion val)
		{
			float angle;
			Vector3 axis;
			val.ToAngleAxis(out angle, out axis);
			return "Angle " + angle.ToString("N1") + " Axis " + axis.ToString();
		}

		public static string ToStringAngleAxisEulerDecorated(this Quaternion val, string separatorBeforeEuler = "\t")
		{
			float angle;
			Vector3 axis;
			val.ToAngleAxis(out angle, out axis);
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

		#endregion

		#region Conversions - Hex

		public static bool IsHexString(this string text)
		{
			if (text == null)
				return false;
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
			Encoding unicode = Encoding.Unicode;

			var utf8Array = Encoding.UTF8.GetBytes(text);
			var asciiArray = Encoding.Convert(unicode, ascii, utf8Array);
			return ascii.GetString(asciiArray);
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
			totalSeconds = MathTools.FastRoundToInt(totalSeconds * 1000.0) / 1000.0;

			var minutes = (long)(totalSeconds / 60);
			var seconds = (int)(totalSeconds % 60);
			var milliseconds = MathTools.FastRoundToInt((totalSeconds - Math.Truncate(totalSeconds)) * 1000);
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

		private static StringBuilder ReusedStringBuilder;

		public static string ConvertToString(this char[] chars, int startIndex, int length)
		{
			if (ReusedStringBuilder == null)
				ReusedStringBuilder = new StringBuilder();

			var endIndex = startIndex + length;
			for (int i = startIndex; i < chars.Length && i < endIndex && (uint)chars[i] > 0U; ++i)
				ReusedStringBuilder.Append(chars[i]);
			var result = ReusedStringBuilder.ToString();
			ReusedStringBuilder.Clear();

			// Hard limit to prevent memory bogging. We probably accept the consequences of reallocation when working that big.
			if (ReusedStringBuilder.Capacity > 1000000)
				ReusedStringBuilder = null;

			return result;
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

		#region Percentage Bar

		private static StringBuilder PercentageBarStringBuilder;

		public static string ToStringAsPercentageBar(this double value, int barLength = 20)
		{
			return ToStringAsPercentageBar((float)value, barLength);
		}

		public static string ToStringAsPercentageBar(this float value, int barLength = 20)
		{
			if (PercentageBarStringBuilder == null)
			{
				// ReSharper disable once InconsistentlySynchronizedField
				PercentageBarStringBuilder = new StringBuilder(barLength + 10);
			}
			lock (PercentageBarStringBuilder)
			{
				PercentageBarStringBuilder.Length = 0;
				if (barLength > 0)
				{
					PercentageBarStringBuilder.Append('[');
					var clampedValue = value < 0f ? 0f : value > 1f ? 1f : value;
					var filledCount = (int)(clampedValue * barLength);
					if (filledCount == 0 && clampedValue > 0.00001f)
					{
						filledCount = 1; // Always show some bar piece if the value is greater than zero.
					}
					var emptyCount = barLength - filledCount;
					for (int i = 0; i < filledCount; i++)
					{
						PercentageBarStringBuilder.Append('\u2588'); // A full square character
					}
					for (int i = 0; i < emptyCount; i++)
					{
						PercentageBarStringBuilder.Append('\u2500'); // A full left-to-right stretching dash character
					}
					PercentageBarStringBuilder.Append("] ");
				}
				PercentageBarStringBuilder.Append(value.ToString("P1"));
				return PercentageBarStringBuilder.ToString();
			}
		}

		#endregion

		#region Singular/Plural

		public static string ToStringWithEnglishPluralPostfix(this int value, string prefix)
		{
			return value == 1
				? value.ToString() + " " + prefix
				: value.ToString() + " " + prefix + "s";
		}

		public static string ToStringWithEnglishPluralPostfix(this int value, string prefix, char valueShell)
		{
			return value == 1
				? valueShell + value.ToString() + valueShell + " " + prefix
				: valueShell + value.ToString() + valueShell + " " + prefix + "s";
		}

		#endregion
	}

}
