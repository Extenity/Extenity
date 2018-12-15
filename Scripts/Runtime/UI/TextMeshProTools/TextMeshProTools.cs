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
				value.ToStringAsCharArray(Buffer, out var startIndex, out var length);
				text.SetCharArray(Buffer, startIndex, length);
			}
		}

		public static void SetCharArrayForInt(this TextMeshProUGUI text, Int64 value)
		{
			lock (Buffer)
			{
				value.ToStringAsCharArray(Buffer, out var startIndex, out var length);
				text.SetCharArray(Buffer, startIndex, length);
			}
		}

		public static void SetCharArrayForInt(this TextMeshProUGUI text, Int32 value, char thousandsSeparator)
		{
			lock (Buffer)
			{
				value.ToStringAsCharArray(Buffer, thousandsSeparator, out var startIndex, out var length);
				text.SetCharArray(Buffer, startIndex, length);
			}
		}

		public static void SetCharArrayForInt(this TextMeshProUGUI text, Int64 value, char thousandsSeparator)
		{
			lock (Buffer)
			{
				value.ToStringAsCharArray(Buffer, thousandsSeparator, out var startIndex, out var length);
				text.SetCharArray(Buffer, startIndex, length);
			}
		}

		public static void SetCharArrayForIntWithPrefix(this TextMeshProUGUI text, Int32 value, char prefix)
		{
			lock (Buffer)
			{
				value.ToStringAsCharArray(Buffer, out var startIndex, out var length);
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
				value.ToStringAsCharArray(Buffer, out var startIndex, out var length);
				startIndex--;
				length++;
				Buffer[startIndex] = prefix;
				text.SetCharArray(Buffer, startIndex, length);
			}
		}

		#region SetCharArrayFormatted

		public static void SetCharArrayForValue(this TextMeshProUGUI text, string format, uint value)
		{
			lock (Buffer)
			{
				var length = value.ToStringAsCharArray(format, Buffer);
				text.SetCharArray(Buffer, 0, length);
			}
		}

		public static void SetCharArrayForValue(this TextMeshProUGUI text, string format, int value)
		{
			lock (Buffer)
			{
				var length = value.ToStringAsCharArray(format, Buffer);
				text.SetCharArray(Buffer, 0, length);
			}
		}

		public static void SetCharArrayForValue(this TextMeshProUGUI text, string format, ulong value)
		{
			lock (Buffer)
			{
				var length = value.ToStringAsCharArray(format, Buffer);
				text.SetCharArray(Buffer, 0, length);
			}
		}

		public static void SetCharArrayForValue(this TextMeshProUGUI text, string format, long value)
		{
			lock (Buffer)
			{
				var length = value.ToStringAsCharArray(format, Buffer);
				text.SetCharArray(Buffer, 0, length);
			}
		}

		public static void SetCharArrayForValue(this TextMeshProUGUI text, string format, float value)
		{
			lock (Buffer)
			{
				var length = value.ToStringAsCharArray(format, Buffer);
				text.SetCharArray(Buffer, 0, length);
			}
		}

		public static void SetCharArrayForValue(this TextMeshProUGUI text, string format, double value)
		{
			lock (Buffer)
			{
				var length = value.ToStringAsCharArray(format, Buffer);
				text.SetCharArray(Buffer, 0, length);
			}
		}

		public static void SetCharArrayForValue(this TextMeshProUGUI text, string format, decimal value)
		{
			lock (Buffer)
			{
				var length = value.ToStringAsCharArray(format, Buffer);
				text.SetCharArray(Buffer, 0, length);
			}
		}

		#endregion
	}

}
