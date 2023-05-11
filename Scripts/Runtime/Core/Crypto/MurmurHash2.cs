using System;
#if ENABLE_BURST_AOT
using Unity.Burst;
#endif

namespace Extenity.CryptoToolbox
{

	/// <summary>
	/// An optimized implementation of 32 bit MurmurHash2 algorithm that works with Burst compiler.
	///
	/// Based on the work here, but heavily modified by Can Baycay.
	/// Source: http://landman-code.blogspot.com/2009/02/c-superfasthash-and-murmurhash2.html
	/// </summary>
#if ENABLE_BURST_AOT
	[BurstCompile]
#endif
	public static class MurmurHash2
	{
		public static unsafe UInt32 CalculateFromBytes(Byte[] bytes)
		{
			if (bytes == null || bytes.Length == 0)
			{
				return 0;
			}

			fixed (byte* data = bytes)
			{
				CalculateHashOfByteArray(data, bytes.Length, out UInt32 result);
				return result;
			}
		}

		public static unsafe UInt32 CalculateFromString(string text)
		{
			// ReSharper disable once ReplaceWithStringIsNullOrEmpty
			if (text == null || text.Length == 0) // This is faster than string.IsNullOrEmpty
			{
				return 0;
			}

			fixed (char* data = text)
			{
				CalculateHashOfString((byte*)data, text.Length, out UInt32 result);
				return result;
			}
		}

		#region Calculations

		private const UInt32 m = 0x5bd1e995;
		private const Int32 r = 24;

#if ENABLE_BURST_AOT
		// OptimizeFor.Size worked a bit more faster in Editor environment. Needs more benchmarks on devices to be sure.
		[BurstCompile(FloatPrecision.Low, FloatMode.Fast, OptimizeFor = OptimizeFor.Size)]
#endif
		private static unsafe void CalculateHashOfString(byte* data, Int32 length, out UInt32 result)
		{
			UInt32 h = 0xc58f1a7b ^ (UInt32)length;
			Int32 remainingBytes = length & 1; // mod 2
			Int32 iteratorOf4Bytes = length >> 1; // div 2
			UInt32* pointer = (UInt32*)data;
			while (iteratorOf4Bytes != 0)
			{
				UInt32 k = *pointer;
				k *= m;
				k ^= k >> r;
				k *= m;

				h *= m;
				h ^= k;
				iteratorOf4Bytes--;
				pointer++;
			}
			switch (remainingBytes)
			{
				case 1:
					h ^= *((UInt16*)pointer);
					h *= m;
					break;
			}

			// Do a few final mixes of the hash to ensure the last few bytes are well-incorporated.
			h ^= h >> 13;
			h *= m;
			h ^= h >> 15;

			result = h;
		}

#if ENABLE_BURST_AOT
		// OptimizeFor.Size worked a bit more faster in Editor environment. Needs more benchmarks on devices to be sure.
		[BurstCompile(FloatPrecision.Low, FloatMode.Fast, OptimizeFor = OptimizeFor.Size)]
#endif
		private static unsafe void CalculateHashOfByteArray(byte* data, Int32 length, out UInt32 result)
		{
			UInt32 h = 0xc58f1a7b ^ (UInt32)length;
			Int32 remainingBytes = length & 3; // mod 4
			Int32 iteratorOf4Bytes = length >> 2; // div 4
			UInt32* pointer = (UInt32*)data;
			while (iteratorOf4Bytes != 0)
			{
				UInt32 k = *pointer;
				k *= m;
				k ^= k >> r;
				k *= m;

				h *= m;
				h ^= k;
				iteratorOf4Bytes--;
				pointer++;
			}
			switch (remainingBytes)
			{
				case 3:
					h ^= *((UInt16*)pointer);
					h ^= ((UInt32)(*(((Byte*)(pointer)) + 2))) << 16;
					h *= m;
					break;

				case 2:
					h ^= *((UInt16*)pointer);
					h *= m;
					break;

				case 1:
					h ^= *((Byte*)pointer);
					h *= m;
					break;
			}

			// Do a few final mixes of the hash to ensure the last few bytes are well-incorporated.
			h ^= h >> 13;
			h *= m;
			h ^= h >> 15;

			result = h;
		}

		#endregion

		#region Benchmark

		/*
		[MenuItem("TEMP/RunBenchmark_MurmurHash2 &e")]
		[InitializeOnLoadMethod]
		private static async void RunBenchmark_MurmurHash2()
		{
			const string text = "Character/1214/Item/PickOneSpeedUpChest_2/Quantity";

			// Warmup
			await Task.Yield();
			_ = text.GetHashCode();
			_ = text.GetHashCode();
			_ = MurmurHash2.CalculateFromString(text);
			_ = MurmurHash2.CalculateFromString(text);
			await Task.Yield();

			var calculateCount = 1_000_000;
			using (new QuickProfilerStopwatch("string.GetHashCode() \t took {0}"))
			{
				for (int i = 0; i < calculateCount; i++)
				{
					var hash = text.GetHashCode();
				}
			}
			using (new QuickProfilerStopwatch("MurmurHash2        \t took {0}"))
			{
				for (int i = 0; i < calculateCount; i++)
				{
					var hash = MurmurHash2.Calculate(text);
				}
			}
		}
		*/

		#endregion
	}

}
