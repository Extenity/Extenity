using System.Text;
using System.Collections;

namespace Extenity.DataToolbox
{

	public static class BitArrayTools
	{
		public static int CountSetBits(this BitArray bitArray)
		{
			int count = 0;
			foreach (bool bit in bitArray)
			{
				if (bit)
				{
					count++;
				}
			}
			return count;
		}

		public static int CountUnsetBits(this BitArray bitArray)
		{
			int count = 0;
			foreach (bool bit in bitArray)
			{
				if (!bit)
				{
					count++;
				}
			}
			return count;
		}

		public static string ToString01(this BitArray bitArray)
		{
			var stringBuilder = new StringBuilder(bitArray.Count);

			foreach (bool bit in bitArray)
			{
				stringBuilder.Append(bit ? "1" : "0");
			}

			return stringBuilder.ToString();
		}

		public static byte[] ToByteArray(this BitArray bitArray)
		{
			int numBytes = bitArray.Count / 8;
			if (bitArray.Count % 8 != 0)
				numBytes++;

			byte[] byteArray = new byte[numBytes];
			bitArray.CopyTo(byteArray, 0);
			return byteArray;
		}

		public static byte[] ToByteArrayInverted(this BitArray bitArray)
		{
			int numBytes = bitArray.Count / 8;
			if (bitArray.Count % 8 != 0)
				numBytes++;

			byte[] bytes = new byte[numBytes];
			int byteIndex = 0;
			int bitIndex = 0;

			for (int i = 0; i < bitArray.Count; i++)
			{
				if (bitArray[i])
					bytes[byteIndex] |= (byte)(1 << (7 - bitIndex));
				//bytes[byteIndex] |= (byte)(1 << (0 + bitIndex));

				bitIndex++;
				if (bitIndex == 8)
				{
					bitIndex = 0;
					byteIndex++;
				}
			}

			return bytes;
		}
	}

}
