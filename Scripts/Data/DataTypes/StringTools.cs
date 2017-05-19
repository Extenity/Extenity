using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Extenity.DataTypes
{

	public static class StringTools
	{
		#region Constants

		public static readonly char[] LineEndingCharacters = { '\r', '\n' };

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

			for (int i = 0; i < str.Length; i++)
			{
				if (char.IsLetter(str[i]) || char.IsNumber(str[i]) || (allowSpace && str[i] == ' '))
					continue;
				return false;
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

		public static string SubstringBetween(this string text, string startTag, string endTag, int startIndex = 0)
		{
			if (text == null)
				throw new ArgumentNullException("text");
			if (string.IsNullOrEmpty(startTag))
				throw new ArgumentNullException("startTag");
			if (string.IsNullOrEmpty(endTag))
				throw new ArgumentNullException("endTag");
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
		public static string ReplaceBetween(this string text, string startTag, string endTag, string newValue, int startIndex = 0)
		{
			if (text == null)
				throw new ArgumentNullException("text");
			if (string.IsNullOrEmpty(startTag))
				throw new ArgumentNullException("startTag");
			if (string.IsNullOrEmpty(endTag))
				throw new ArgumentNullException("endTag");
			// Replacement text can be null. Which means we want to delete the text and not replace with anything.
			//if (string.IsNullOrEmpty(newValue))
			//	throw new ArgumentNullException("newValue");
			if (text.Length == 0)
				return text;

			if (startIndex < 0)
				startIndex = 0;

			var startTagIndex = text.IndexOf(startTag, startIndex);
			if (startTagIndex < 0)
				return text;

			startTagIndex += startTag.Length;

			var endTagIndex = text.IndexOf(endTag, startTagIndex);
			if (endTagIndex <= startTagIndex)
				return text;

			return text.Substring(0, startTagIndex) + newValue + text.Substring(endTagIndex, text.Length - endTagIndex);
		}

		public static int IndexOfNextLineEnding(this string text, int startIndex)
		{
			return text.IndexOfAny(LineEndingCharacters, startIndex);
		}

		public static string NormalizeLineEndings(this string text)
		{
			var builder = new StringBuilder((int)(text.Length * 1.1f));
			bool lastWasCR = false;

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
						builder.Append("\r\n");
						lastWasCR = true;
						break;
					case '\n':
						builder.Append("\r\n");
						break;
					default:
						builder.Append(c);
						break;
				}
			}
			return builder.ToString();
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

		#region StringBuilder

		public static void Clear(this StringBuilder value)
		{
			value.Length = 0;
			//value.Capacity = 0; This is not a good idea since we want to reuse the already allocated memory
		}

		#endregion

		#region Conversions - Vector2/Vector3/Quaternion

		public static string ToString(Vector2 val)
		{
			return val.x + " " + val.y;
		}
		public static string ToString(Vector3 val)
		{
			return val.x + " " + val.y + " " + val.z;
		}
		public static string ToString(Quaternion val)
		{
			return val.x + " " + val.y + " " + val.z + " " + val.w;
		}

		public static Vector2 ParseVector2(string text)
		{
			string[] parts = text.Split(' ');
			return new Vector2(Single.Parse(parts[0]), Single.Parse(parts[1]));
		}
		public static Vector3 ParseVector3(string text)
		{
			string[] parts = text.Split(' ');
			return new Vector3(Single.Parse(parts[0]), Single.Parse(parts[1]), Single.Parse(parts[2]));
		}
		public static Quaternion ParseQuaternion(string text)
		{
			string[] parts = text.Split(' ');
			return new Quaternion(Single.Parse(parts[0]), Single.Parse(parts[1]), Single.Parse(parts[2]), Single.Parse(parts[3]));
		}

		#endregion

		#region Conversions - Hex

		public static bool IsHexString(this string value)
		{
			for (int i = 0; i < value.Length; i++)
			{
				var character = value[i];
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

		#region Time

		static string FormatTime(float seconds)
		{
			TimeSpan t = TimeSpan.FromSeconds(seconds);

			return string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D2}", t.Hours, t.Minutes, t.Seconds, t.Milliseconds);
			//return string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
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

		#endregion
	}

}
