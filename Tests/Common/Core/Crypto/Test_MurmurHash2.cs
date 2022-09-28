using System;
using System.Text;
using Extenity.CryptoToolbox;
using NUnit.Framework;

namespace ExtenityTests.CryptoToolbox
{

	public class Test_MurmurHash2
	{
		private const int RepeatCount = 1_000;
		
		[Test, Repeat(RepeatCount)]
		public void Simple()
		{
			CheckNotZero(MurmurHash2.Calculate("Test text"));
		}

		[Test, Repeat(RepeatCount)]
		public void EmptyOrNullStringGeneratesZeroHash()
		{
			CheckZero(MurmurHash2.Calculate(""));
			CheckZero(MurmurHash2.Calculate(string.Empty));
			CheckZero(MurmurHash2.Calculate((string)null));
		}

		[Test, Repeat(RepeatCount)]
		public void EmptyOrNullBytesGeneratesZeroHash()
		{
			CheckZero(MurmurHash2.Calculate(new byte[0]));
			CheckZero(MurmurHash2.Calculate(Array.Empty<byte>()));
			CheckZero(MurmurHash2.Calculate((byte[])null));
		}

		[Test, Repeat(RepeatCount)]
		public void DifferentInstancesGenerateTheSameHash()
		{
			Check(() => Concat('a'));
			Check(() => Concat('a', 'b'));
			Check(() => Concat('a', 'b', 'c'));
			Check(() => Concat('a', 'b', 'c', 'd'));
			Check(() => Concat('a', 'b', 'c', 'd', 'e'));
			Check(() => Concat('a', 'b', 'c', 'd', 'e', 'f'));

			Check(() => Concat('a', 'Ğ'));
			Check(() => Concat('a', 'b', 'Ğ'));
			Check(() => Concat('a', 'b', 'c', 'Ğ'));
			Check(() => Concat('a', 'b', 'c', 'd', 'Ğ'));
			Check(() => Concat('a', 'b', 'c', 'd', 'e', 'Ğ'));
			Check(() => Concat('a', 'b', 'c', 'd', 'e', 'f', 'Ğ'));

			Check(() => Concat('a', 'Ğ', 'k'));
			Check(() => Concat('a', 'b', 'Ğ', 'k'));
			Check(() => Concat('a', 'b', 'c', 'Ğ', 'k'));
			Check(() => Concat('a', 'b', 'c', 'd', 'Ğ', 'k'));
			Check(() => Concat('a', 'b', 'c', 'd', 'e', 'Ğ', 'k'));
			Check(() => Concat('a', 'b', 'c', 'd', 'e', 'f', 'Ğ', 'k'));

			Check(() => Concat('Ğ', 'a'));
			Check(() => Concat('Ğ', 'a', 'b'));
			Check(() => Concat('Ğ', 'a', 'b', 'c'));
			Check(() => Concat('Ğ', 'a', 'b', 'c', 'd'));
			Check(() => Concat('Ğ', 'a', 'b', 'c', 'd', 'e'));
			Check(() => Concat('Ğ', 'a', 'b', 'c', 'd', 'e', 'f'));

			Check(() => Concat('Ğ'));
		}

		#region Tools

		private string Concat(params char[] chars)
		{
			return string.Concat(chars);
		}

		private void CheckEqualHashes(string text1, string text2)
		{
			Assert.IsTrue(!ReferenceEquals(text1, text2)); // Ensure the strings are not referencing to the same object.
			Assert.AreEqual(text1, text2);
			var ASCIIBytes1 = Encoding.ASCII.GetBytes(text1);
			var ASCIIBytes2 = Encoding.ASCII.GetBytes(text2);
			Assert.AreEqual(ASCIIBytes1, ASCIIBytes2);
			var UTF8Bytes1 = Encoding.UTF8.GetBytes(text1);
			var UTF8Bytes2 = Encoding.UTF8.GetBytes(text2);
			Assert.AreEqual(UTF8Bytes1, UTF8Bytes2);
			CheckEqualHashes(MurmurHash2.Calculate(text1), MurmurHash2.Calculate(text2));
			CheckEqualHashes(MurmurHash2.Calculate(ASCIIBytes1), MurmurHash2.Calculate(ASCIIBytes2));
			CheckEqualHashes(MurmurHash2.Calculate(UTF8Bytes1), MurmurHash2.Calculate(UTF8Bytes2));
		}

		private void CheckEqualHashes(uint hash1, uint hash2)
		{
			CheckNotZero(hash1);
			CheckNotZero(hash2);
			Assert.AreEqual(hash1, hash2);
		}

		private void Check(Func<string> generateText)
		{
			var text1 = generateText();
			var text2 = generateText();
			CheckEqualHashes(text1, text2);
		}

		private void CheckNotZero(uint hash)
		{
			Assert.NotZero(hash);
		}

		private void CheckZero(uint hash)
		{
			Assert.Zero(hash);
		}

		#endregion
	}

}
