using System;
using Extenity.DataToolbox;

namespace TMPro.Extensions
{

	public static class TextMeshProTools
	{
		private static readonly char[] Buffer = new char[120];

		public static void SetCharArrayForInt(this TextMeshProUGUI text, Int32 value)
		{
			lock (Buffer)
			{
				int startIndex, length;
				value.ToStringAsCharArray(Buffer, out startIndex, out length);
				text.SetCharArray(Buffer, startIndex, length);
			}
		}

		public static void SetCharArrayForInt(this TextMeshProUGUI text, Int64 value)
		{
			lock (Buffer)
			{
				int startIndex, length;
				value.ToStringAsCharArray(Buffer, out startIndex, out length);
				text.SetCharArray(Buffer, startIndex, length);
			}
		}

		public static void SetCharArrayForInt(this TextMeshProUGUI text, Int32 value, char thousandsSeparator)
		{
			lock (Buffer)
			{
				int startIndex, length;
				value.ToStringAsCharArray(Buffer, thousandsSeparator, out startIndex, out length);
				text.SetCharArray(Buffer, startIndex, length);
			}
		}

		public static void SetCharArrayForInt(this TextMeshProUGUI text, Int64 value, char thousandsSeparator)
		{
			lock (Buffer)
			{
				int startIndex, length;
				value.ToStringAsCharArray(Buffer, thousandsSeparator, out startIndex, out length);
				text.SetCharArray(Buffer, startIndex, length);
			}
		}

		public static void SetCharArrayForIntWithPrefix(this TextMeshProUGUI text, Int32 value, char prefix)
		{
			lock (Buffer)
			{
				int startIndex, length;
				value.ToStringAsCharArray(Buffer, out startIndex, out length);
				startIndex--;
				length++;
				Buffer[startIndex] = prefix;
				text.SetCharArray(Buffer, startIndex, length);
			}
		}

		public static void SetCharArrayForIntWithPrefix(this TextMeshProUGUI text, Int64 value, char prefix)
		{
			lock (Buffer)
			{
				int startIndex, length;
				value.ToStringAsCharArray(Buffer, out startIndex, out length);
				startIndex--;
				length++;
				Buffer[startIndex] = prefix;
				text.SetCharArray(Buffer, startIndex, length);
			}
		}

	}

}
