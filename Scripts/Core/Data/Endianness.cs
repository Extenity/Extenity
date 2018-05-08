using System;

namespace Extenity.DataToolbox
{

	public static class Endianness
	{
		#region Array Conversion

		public static void EndianReverseBytes(this byte[] inArray)
		{
			int highCtr = inArray.Length - 1;

			for (int ctr = 0; ctr < inArray.Length / 2; ctr++)
			{
				var temp = inArray[ctr];
				inArray[ctr] = inArray[highCtr];
				inArray[highCtr] = temp;
				highCtr -= 1;
			}
		}

		public static void EndianReverseBytes(this byte[] inArray, int length)
		{
			int highCtr = length - 1;

			for (int ctr = 0; ctr < length / 2; ctr++)
			{
				var temp = inArray[ctr];
				inArray[ctr] = inArray[highCtr];
				inArray[highCtr] = temp;
				highCtr -= 1;
			}
		}

		public static void EndianReverseBytes(this byte[] inArray, int start, int length)
		{
			int highCtr = start + length - 1;

			for (int ctr = start; ctr < start + (length / 2); ctr++)
			{
				var temp = inArray[ctr];
				inArray[ctr] = inArray[highCtr];
				inArray[highCtr] = temp;
				highCtr -= 1;
			}
		}

		#endregion

		#region General Data Types

		public static Int16 EndianReversed(this Int16 value)
		{
			byte[] valueBytes = BitConverter.GetBytes(value);
			valueBytes.EndianReverseBytes();
			return BitConverter.ToInt16(valueBytes, 0);
		}

		public static Int32 EndianReversed(this Int32 value)
		{
			byte[] valueBytes = BitConverter.GetBytes(value);
			valueBytes.EndianReverseBytes();
			return BitConverter.ToInt32(valueBytes, 0);
		}

		public static Int64 EndianReversed(this Int64 value)
		{
			byte[] valueBytes = BitConverter.GetBytes(value);
			valueBytes.EndianReverseBytes();
			return BitConverter.ToInt64(valueBytes, 0);
		}

		public static float EndianReversed(this float value)
		{
			byte[] valueBytes = BitConverter.GetBytes(value);
			valueBytes.EndianReverseBytes();
			return BitConverter.ToSingle(valueBytes, 0);
		}

		public static double EndianReversed(this double value)
		{
			byte[] valueBytes = BitConverter.GetBytes(value);
			valueBytes.EndianReverseBytes();
			return BitConverter.ToDouble(valueBytes, 0);
		}

		#endregion

		#region GUID

		public static byte[] ToByteArrayReverseEndianness(this System.Guid guid)
		{
			var bytes = guid.ToByteArray();

			// Reverse the endianness of GUID
			//if (!BitConverter.IsLittleEndian)
			{
				bytes.EndianReverseBytes(4);
				bytes.EndianReverseBytes(4, 2);
				bytes.EndianReverseBytes(6, 2);
			}

			return bytes;
		}

		public static System.Guid ToGuidReverseEndianness(this byte[] bytes)
		{
			// Reverse the endianness of GUID
			//if (!BitConverter.IsLittleEndian)
			{
				bytes.EndianReverseBytes(4);
				bytes.EndianReverseBytes(4, 2);
				bytes.EndianReverseBytes(6, 2);
			}

			return new System.Guid(bytes);
		}

		#endregion
	}

}
