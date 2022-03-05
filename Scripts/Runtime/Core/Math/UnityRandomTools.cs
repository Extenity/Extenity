// #define UseSystemRandom

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Extenity.DataToolbox;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Extenity.MathToolbox
{

	public static class UnityRandomTools
	{
		#region Random Number Generator

#if !UNITY || UseSystemRandom
		private static System.Random Generator;
#endif

		public static void SetSeed(int seed)
		{
#if !UNITY || UseSystemRandom
			Generator = new System.Random(seed);
#else
			UnityEngine.Random.InitState(seed);
#endif
		}

		private static float _Float
		{
			get
			{
#if !UNITY || UseSystemRandom
				return (float)Generator.NextDouble();
				// return (float)(Generator.Next() / (double)Int32.MaxValue); // Not extensively tested.
#else
				return UnityEngine.Random.value;
#endif
			}
		}

		private static int _Range(int minInclusive, int maxExclusive)
		{
#if !UNITY || UseSystemRandom
			return Generator.Next(minInclusive, maxExclusive);
#else
			return UnityEngine.Random.Range(minInclusive, maxExclusive);
#endif
		}

		private static float _Range(float minInclusive, float maxInclusive)
		{
#if !UNITY || UseSystemRandom
			return (float)((Generator.NextDouble() * ((double)maxInclusive - (double)minInclusive)) + (double)minInclusive);
#else
			return UnityEngine.Random.Range(minInclusive, maxInclusive);
#endif
		}

		#endregion

		#region Randomize Generator

		// Randomize internal random number generator by giving it a seed that is created
		// using system's current timestamp. The algorithm will fall back to alternative
		// timestamp options if required. Also it's safe to call this method rapidly
		// without worrying about getting the same timestamp value because of timestamp
		// resolution. Thanks to the extra added parameter that changes every time a
		// randomization request is made.
		//
		// See the description of GenerateTimestampedSeed for how it performs.
		public static void RandomizeGenerator()
		{
			var seed = GenerateTimestampedSeed();
			SetSeed(seed);
		}

		private static int _Lubricant = 8156491; // It's named lubricant, because why not. The value is just a random value which does not mean anything.

		// Generates a seed using system's timestamp that can be used to initialize a
		// random number generator. It has a robust, yet simple algorithm for picking
		// the best available timestamp method on the platform that the application is
		// running on. Also do not fear calling this method rapidly for the fact that
		// timestamp methods tend to return the same value for rapid consecutive calls.
		// The algorithm prevents that by adding an extra parameter to the seed that
		// changes every time a call to this method is made.
		//
		// Tests show that, on a decent Windows machine, when the algorithm tries to
		// generate 10.000 consecutive seeds rapidly, it generates 99.25% of these seeds
		// until it generates a previously generated seed again twice. In real world,
		// there would be no, or only rare use cases that an application would want to
		// reset RNG generator 10.000 times in fraction of a second. So we may assume
		// this ratio is more than enough.
		//
		// Heck, even 10 times without a collision in a second would be more than enough.
		public static int GenerateTimestampedSeed()
		{
			// Stopwatch.GetTimestamp will try to use QueryPerformanceCounter if available,
			// and fall back to DateTime.UtcNow.Ticks.
			int seed = (int)Stopwatch.GetTimestamp();
			if (seed > -100 && seed < 100)
			{
				// It is so unlikely the code will ever get here. But in case we are here, let's try to use
				// Environment.TickCount for one more chance. It's not precise as performance counters
				// but it will do okay.
				seed = Environment.TickCount;

				if (seed > -100 && seed < 100)
				{
#if UNITY
					// Alright. Something fishy going on, but we won't stop trying.
					seed = (int)(UnityEngine.Time.realtimeSinceStartup * 100000f);
#endif

					if (seed > -100 && seed < 100)
					{
						// Don't know what to do anymore. Just fail miserably.
						throw new Exception("Failed to initialize RNG.");
					}
				}
			}

			// There is a possibility of two quick consecutive calls to timestamp method might get the same
			// value for both calls. So we pour an additional value into the mix, that increments everytime
			// the user randomizes the generator. So even if we get the same timestamp for both consecutive
			// calls, adding that incremented value to the seed makes it unlikely to collide with previously
			// generated seeds.
			//
			// Even better, rather than adding a simple value to the timestamp, it gets harder to tamper with
			// when the extra added value is another randomly generated value. Helps preventing possible hack
			// attempts. Also makes the seed more garbled, rather than looking like a simple timestamp value.
			//
			// Appreciate coding at fabulous level with this whole comment block being extraordinarily neat!
			{
				// Lehmer Algorithm. Grabbed from https://msdn.microsoft.com/en-us/magazine/mt767700.aspx
				const int a = 16807;
				const int m = 2147483647;
				const int q = 127773;
				const int r = 2836;
				int hi = _Lubricant / q;
				int lo = _Lubricant % q;
				_Lubricant = (a * lo) - (r * hi);
				if (_Lubricant <= 0)
					_Lubricant = _Lubricant + m;
				// Apply lubricant to the seed.
				seed ^= _Lubricant;
			}

			return seed;
		}

		#endregion

		public static int Range(int minInclusive, int maxExclusive) { return _Range(minInclusive, maxExclusive); }
		public static int RangeIncludingMax(int minInclusive, int maxInclusive) { return _Range(minInclusive, maxInclusive + 1); }
		public static float Range(float minInclusive, float maxInclusive) { return _Range(minInclusive, maxInclusive); }

#if UNITY
		public static UnityEngine.Color ColorRGB => new(Range(0f, 1f), Range(0f, 1f), Range(0f, 1f));
#endif

		// @formatter:off
		public static float2 RangeFloat2(float min , float max                                                   ) => float2(Range(min , max ), Range(min , max )                   );
		public static float3 RangeFloat3(float min , float max                                                   ) => float3(Range(min , max ), Range(min , max ), Range(min , max ));
		public static float2 RangeFloat2(float minX, float maxX,  float minY, float maxY                         ) => float2(Range(minX, maxX), Range(minY, maxY)                   );
		public static float3 RangeFloat3(float minX, float maxX,  float minY, float maxY,  float minZ, float maxZ) => float3(Range(minX, maxX), Range(minY, maxY), Range(minZ, maxZ));
		public static float  Float      => _Float;
		public static float2 Float2     => float2(_Float, _Float);
		public static float3 Float3     => float3(_Float, _Float, _Float);
		public static float  UnitFloat  => Range      (-1f, 1f);
		public static float2 UnitFloat2 => RangeFloat2(-1f, 1f);
		public static float3 UnitFloat3 => RangeFloat3(-1f, 1f);
		public static bool   Bool       => Float > 0.5f;
		public static float  Sign       => Bool ? -1f : 1f;
		// @formatter:on

		public static float2 InsideUnitCircle
		{
			get
			{
				while (true)
				{
					// Generate a point in unit square. Then make sure it is inside the unit square. If not, try again.
					// This (hopefully) provides homogeneous random distribution.
					var value = UnitFloat2;

					if (lengthsq(value) <= 1f)
					{
						return value;
					}
				}
			}
		}

		public static float3 InsideUnitSphere
		{
			get
			{
				while (true)
				{
					// Generate a point in unit cube. Then make sure it is inside the unit sphere. If not, try again.
					// This (hopefully) provides homogeneous random distribution.
					var value = UnitFloat3;

					if (lengthsq(value) <= 1f)
					{
						return value;
					}
				}
			}
		}

		#region Random Collections

		public static void FillRandom(this IList<byte> data, byte minInclusive = 0, byte maxExclusive = 255, int count = -1)
		{
			if (count < 0)
				count = data.Count;
			for (int i = 0; i < count; i++)
			{
				data[i] = (byte)Range(minInclusive, maxExclusive);
			}
		}

		public static void FillRandomLowerLetters(this IList<char> data, int count = -1)
		{
			if (count < 0)
				count = data.Count;
			for (int i = 0; i < count; i++)
			{
				data[i] = (char)Range((int)'a', (int)'z');
			}
		}

		public static void FillRandomPositionsInSphere(this IList<float3> list, float3 sphereCenter, float sphereRadius)
		{
			for (int i = 0; i < list.Count; i++)
			{
				var position = InsideUnitSphere * sphereRadius + sphereCenter;
				list[i] = position;
			}
		}

		public static bool TryFillRandomPositionsInSphereWithMinimumSeparation(this IList<float3> list, float3 sphereCenter, float sphereRadius, float minimumSeparationBetweenPositions)
		{
			var minimumSeparationBetweenPositionsSqr = minimumSeparationBetweenPositions * minimumSeparationBetweenPositions;

			var tryCountForWholeOperation = 50;
			while (tryCountForWholeOperation-- > 0)
			{
				var failedToFindAvailableSpacesForAll = false;

				for (int i = 0; i < list.Count; i++)
				{
					var foundAnAvailableSpace = false;
					var tryCountForSinglePosition = 50;
					while (tryCountForSinglePosition-- > 0)
					{
						// Select a position randomly.
						var position = InsideUnitSphere * sphereRadius + sphereCenter;

						// See if the selected random position overlaps with any of the previously selected positions.
						var detectedAnOverlapWithPreviousPositions = false;
						for (int iAlreadyPlaced = 0; iAlreadyPlaced < i; iAlreadyPlaced++)
						{
							var distanceSqr = list[iAlreadyPlaced].SqrDistanceTo(position);
							if (distanceSqr < minimumSeparationBetweenPositionsSqr)
							{
								detectedAnOverlapWithPreviousPositions = true;
								break;
							}
						}

						// See if an overlap is detected. If not, accept the position and save it into the list. If there
						// is an overlap, try one more time with a new randomly picked position.
						if (!detectedAnOverlapWithPreviousPositions)
						{
							foundAnAvailableSpace = true;
							list[i] = position;
							break;
						}
					}

					if (!foundAnAvailableSpace)
					{
						// Failed to find an available space for the picked position after many tries. Let's give it
						// a chance by starting the operation from the beginning with freshly picked positions.
						failedToFindAvailableSpacesForAll = true;
						break;
					}
				}

				if (!failedToFindAvailableSpacesForAll)
				{
					return true;
				}
			}

			// So, accept the defeat. Fill whole array with NaN so the caller won't use the list accidentally.
			list.Fill(float3Tools.NaN);
			return false;
		}

		#endregion

		#region Collection Operations - RandomIndexSelection

		public static int RandomIndexSelection<T>(this T[] list)
		{
			if (list.Length == 0)
				return -1;
			return Range(0, list.Length);
		}

		public static int RandomIndexSelection<T>(this T[] list, System.Random random)
		{
			if (list.Length == 0)
				return -1;
			return random.Next(0, list.Length);
		}

		public static int RandomIndexSelection<T>(this ICollection<T> collection)
		{
			if (collection.Count == 0)
				return -1;
			return Range(0, collection.Count);
		}

		public static int RandomIndexSelection<T>(this ICollection<T> collection, System.Random random)
		{
			if (collection.Count == 0)
				return -1;
			return random.Next(0, collection.Count);
		}

		public static int RandomIndexSelection<T>(this IList<T> list, bool removeFromList)
		{
			if (list.Count == 0)
				return -1;
			int index = Range(0, list.Count);
			if (removeFromList)
				list.RemoveAt(index);
			return index;
		}

		public static int RandomIndexSelection<T>(this IList<T> list, bool removeFromList, System.Random random)
		{
			if (list.Count == 0)
				return -1;
			int index = random.Next(0, list.Count);
			if (removeFromList)
				list.RemoveAt(index);
			return index;
		}

		#endregion

		#region Collection Operations - RandomSelection

		public static T RandomSelection<T>(this T[] list)
		{
			if (list.Length == 0)
				return default(T);
			return list[Range(0, list.Length)];
		}

		public static T RandomSelection<T>(this T[] list, System.Random random)
		{
			if (list.Length == 0)
				return default(T);
			return list[random.Next(0, list.Length)];
		}

		public static T RandomSelection<T>(this IList<T> list)
		{
			if (list.Count == 0)
				return default(T);
			return list[Range(0, list.Count)];
		}

		public static T RandomSelection<T>(this IList<T> list, System.Random random)
		{
			if (list.Count == 0)
				return default(T);
			return list[random.Next(0, list.Count)];
		}

		public static T RandomSelection<T>(this IList<T> list, bool removeFromList)
		{
			if (list.Count == 0)
				return default(T);
			int index = Range(0, list.Count);
			T val = list[index];
			if (removeFromList)
				list.RemoveAt(index);
			return val;
		}

		public static T RandomSelection<T>(this IList<T> list, bool removeFromList, System.Random random)
		{
			if (list.Count == 0)
				return default(T);
			int index = random.Next(0, list.Count);
			T val = list[index];
			if (removeFromList)
				list.RemoveAt(index);
			return val;
		}

		#endregion

		#region Collection Operations - RandomSelectionFiltered

		/// <summary>
		/// Selects a random item from list while excluding any items that is equal to excludeItem.
		/// A neat trick would be to pass null as excludeItem, which excludes all null items from
		/// selection if there are any.
		/// 
		/// Note that the method will check for any unwanted situations in exchange for performance.
		/// The method checks if there are any items in the list that is not equal to excludeItem, 
		/// which prevents the algorithm to go into an infinite loop. The cost of this operation 
		/// will be 1 or 2 Equals check most of the time.
		/// 
		/// See also RandomSelectionFilteredUnsafe.
		/// </summary>
		public static T RandomSelectionFilteredSafe<T>(this IList<T> list, T excludeItem, bool returnExcludedIfNoOtherChoice = false)
		{
			if (list.Count == 0)
				return default(T);
			if (list.Count == 1)
			{
				if (returnExcludedIfNoOtherChoice)
					return list[0];
				return list[0].Equals(excludeItem) ? default(T) : list[0];
			}
			// See if the list contains at least one item that is not equal to excludeItem. Otherwise we won't be able to get out from the loop below.
			for (int i = 0; i < list.Count; i++)
			{
				if (!list[i].Equals(excludeItem))
				{
					// Found at least one item that is not excluded. We are good to go for an infinite random trial until we find a non-excluded item.
					while (true)
					{
						var item = list.RandomSelection();
						if (item.Equals(excludeItem))
							continue;
						return item;
					}
				}
			}
			if (returnExcludedIfNoOtherChoice)
				return list.RandomSelection(); // All items are equal to the excluded item. Select one of them randomly.
			return default(T);
		}

		/// <summary>
		/// Selects a random item from list while excluding any items that is equal to excludeItem.
		/// A neat trick would be to pass null as excludeItem, which excludes all null items from
		/// selection if there are any.
		/// 
		/// Note that the method will check for any unwanted situations in exchange for performance.
		/// The method checks if there are any items in the list that is not equal to excludeItem, 
		/// which prevents the algorithm to go into an infinite loop. The cost of this operation 
		/// will be 1 or 2 Equals check most of the time.
		/// 
		/// See also RandomSelectionFilteredUnsafe.
		/// </summary>
		public static T RandomSelectionFilteredSafe<T>(this IList<T> list, T excludeItem, System.Random random, bool returnExcludedIfNoOtherChoice = false)
		{
			if (list.Count == 0)
				return default(T);
			if (list.Count == 1)
			{
				if (returnExcludedIfNoOtherChoice)
					return list[0];
				return list[0].Equals(excludeItem) ? default(T) : list[0];
			}
			// See if the list contains at least one item that is not equal to excludeItem. Otherwise we won't be able to get out from the loop below.
			for (int i = 0; i < list.Count; i++)
			{
				if (!list[i].Equals(excludeItem))
				{
					// Found at least one item that is not excluded. We are good to go for an infinite random trial until we find a non-excluded item.
					while (true)
					{
						var item = list.RandomSelection(random);
						if (item.Equals(excludeItem))
							continue;
						return item;
					}
				}
			}
			if (returnExcludedIfNoOtherChoice)
				return list.RandomSelection(random); // All items are equal to the excluded item. Select one of them randomly.
			return default(T);
		}

		/// <summary>
		/// Selects a random item from list while excluding any items that is equal to excludeItem.
		/// A neat trick would be to pass null as excludeItem, which excludes all null items from
		/// selection if there are any.
		/// 
		/// Note that the method will go into an infinite loop if all the items in the list is equal
		/// to excludeItem. See also RandomSelectionFilteredSafe.
		/// </summary>
		public static T RandomSelectionFilteredUnsafe<T>(this IList<T> list, T excludeItem, bool returnExcludedIfNoOtherChoice = false)
		{
			if (list.Count == 0)
				return default(T);
			if (list.Count == 1)
			{
				if (returnExcludedIfNoOtherChoice)
					return list[0];
				return list[0].Equals(excludeItem) ? default(T) : list[0];
			}
			while (true)
			{
				var item = list.RandomSelection();
				if (item.Equals(excludeItem))
					continue;
				return item;
			}
		}

		/// <summary>
		/// Selects a random item from list while excluding any items that is equal to excludeItem.
		/// A neat trick would be to pass null as excludeItem, which excludes all null items from
		/// selection if there are any.
		/// 
		/// Note that the method will go into an infinite loop if all the items in the list is equal
		/// to excludeItem. See also RandomSelectionFilteredSafe.
		/// </summary>
		public static T RandomSelectionFilteredUnsafe<T>(this IList<T> list, T excludeItem, System.Random random, bool returnExcludedIfNoOtherChoice = false)
		{
			if (list.Count == 0)
				return default(T);
			if (list.Count == 1)
			{
				if (returnExcludedIfNoOtherChoice)
					return list[0];
				return list[0].Equals(excludeItem) ? default(T) : list[0];
			}
			while (true)
			{
				var item = list.RandomSelection(random);
				if (item.Equals(excludeItem))
					continue;
				return item;
			}
		}

		#endregion

		#region Collection Operations - RandomizeOrderFisherYates

		public static void RandomizeOrderFisherYates<T>(this IList<T> list)
		{
			var n = list.Count;
			while (n > 1)
			{
				n--;
				var k = Range(0, n + 1);
				var value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}

		#endregion

		#region Enums

		public static T RandomSelection<T>()
		{
			var values = Enum.GetValues(typeof(T));
			return (T)values.GetValue(Range(0, values.Length));
		}

		#endregion
	}

}
