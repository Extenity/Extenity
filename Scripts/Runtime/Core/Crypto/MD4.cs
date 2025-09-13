using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Extenity.CryptoToolbox
{

	// Taken from http://www.superstarcoders.com/blogs/posts/md4-hash-algorithm-in-c-sharp.aspx
	// Probably not the best implementation of MD4, but it works.
	public class MD4 : HashAlgorithm
	{
		private uint A;
		private uint B;
		private uint C;
		private uint D;
		private readonly uint[] X;
		private int BytesProcessed;

		public MD4()
		{
			X = new uint[16];

			Initialize();
		}

		public override void Initialize()
		{
			A = 0x67452301;
			B = 0xefcdab89;
			C = 0x98badcfe;
			D = 0x10325476;

			BytesProcessed = 0;
		}

		protected override void HashCore(byte[] array, int offset, int length)
		{
			ProcessMessage(Bytes(array, offset, length));
		}

		protected override byte[] HashFinal()
		{
			try
			{
				ProcessMessage(Padding());

				return new[] { A, B, C, D }.SelectMany(word => Bytes(word)).ToArray();
			}
			finally
			{
				Initialize();
			}
		}

		private void ProcessMessage(IEnumerable<byte> bytes)
		{
			foreach (byte b in bytes)
			{
				int c = BytesProcessed & 63;
				int i = c >> 2;
				int s = (c & 3) << 3;

				X[i] = (X[i] & ~((uint)255 << s)) | ((uint)b << s);

				if (c == 63)
				{
					Process16WordBlock();
				}

				BytesProcessed++;
			}
		}

		private static IEnumerable<byte> Bytes(byte[] bytes, int offset, int length)
		{
			for (int i = offset; i < length; i++)
			{
				yield return bytes[i];
			}
		}

		private IEnumerable<byte> Bytes(uint word)
		{
			yield return (byte)(word & 255);
			yield return (byte)((word >> 8) & 255);
			yield return (byte)((word >> 16) & 255);
			yield return (byte)((word >> 24) & 255);
		}

		private IEnumerable<byte> Repeat(byte value, int count)
		{
			for (int i = 0; i < count; i++)
			{
				yield return value;
			}
		}

		private IEnumerable<byte> Padding()
		{
			return Repeat(128, 1)
			   .Concat(Repeat(0, ((BytesProcessed + 8) & 0x7fffffc0) + 55 - BytesProcessed))
			   .Concat(Bytes((uint)BytesProcessed << 3))
			   .Concat(Repeat(0, 4));
		}

		private void Process16WordBlock()
		{
			uint aa = A;
			uint bb = B;
			uint cc = C;
			uint dd = D;

			foreach (int k in new[] { 0, 4, 8, 12 })
			{
				aa = Round1Operation(aa, bb, cc, dd, X[k], 3);
				dd = Round1Operation(dd, aa, bb, cc, X[k + 1], 7);
				cc = Round1Operation(cc, dd, aa, bb, X[k + 2], 11);
				bb = Round1Operation(bb, cc, dd, aa, X[k + 3], 19);
			}

			foreach (int k in new[] { 0, 1, 2, 3 })
			{
				aa = Round2Operation(aa, bb, cc, dd, X[k], 3);
				dd = Round2Operation(dd, aa, bb, cc, X[k + 4], 5);
				cc = Round2Operation(cc, dd, aa, bb, X[k + 8], 9);
				bb = Round2Operation(bb, cc, dd, aa, X[k + 12], 13);
			}

			foreach (int k in new[] { 0, 2, 1, 3 })
			{
				aa = Round3Operation(aa, bb, cc, dd, X[k], 3);
				dd = Round3Operation(dd, aa, bb, cc, X[k + 8], 9);
				cc = Round3Operation(cc, dd, aa, bb, X[k + 4], 11);
				bb = Round3Operation(bb, cc, dd, aa, X[k + 12], 15);
			}

			unchecked
			{
				A += aa;
				B += bb;
				C += cc;
				D += dd;
			}
		}

		// ReSharper disable once InconsistentNaming
		private static uint ROL(uint value, int numberOfBits)
		{
			return (value << numberOfBits) | (value >> (32 - numberOfBits));
		}

		private static uint Round1Operation(uint a, uint b, uint c, uint d, uint xk, int s)
		{
			unchecked
			{
				return ROL(a + ((b & c) | (~b & d)) + xk, s);
			}
		}

		private static uint Round2Operation(uint a, uint b, uint c, uint d, uint xk, int s)
		{
			unchecked
			{
				return ROL(a + ((b & c) | (b & d) | (c & d)) + xk + 0x5a827999, s);
			}
		}

		private static uint Round3Operation(uint a, uint b, uint c, uint d, uint xk, int s)
		{
			unchecked
			{
				return ROL(a + (b ^ c ^ d) + xk + 0x6ed9eba1, s);
			}
		}
	}

}
