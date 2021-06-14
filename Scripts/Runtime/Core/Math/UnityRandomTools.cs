using System;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using Random = UnityEngine.Random;

namespace Extenity.MathToolbox
{

	public static class UnityRandomTools
	{
		#region Randomize Generator

		// Randomize Unity's random number generator by giving it a seed that is created
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
			Random.InitState(seed);
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
					// Alright. Something fishy going on, but we won't stop trying.
					seed = (int)(Time.realtimeSinceStartup * 100000f);

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

		public static int RandomRange(int min, int max) { return Random.Range(min, max); }
		public static int RandomRangeIncludingMax(int min, int max) { return Random.Range(min, max + 1); }

		public static Color RandomColor => new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

		public static float RandomPI => Mathf.PI * Random.value;
		public static float RandomHalfPI => Mathf.PI * 0.5f * Random.value;
		public static float Random180 => 180f * Random.value;
		public static float Random360 => 360f * Random.value;
		public static bool RandomBool => 0.5f > Random.value;
		public static bool RandomBoolRatio(float ratio) { return ratio > Random.value; }
		public static float RandomSign => RandomBool ? -1f : 1f;

		public static Vector2 RandomVector2(float range) { return new Vector2(Random.Range(-range, range), Random.Range(-range, range)); }
		public static Vector3 RandomVector3(float range) { return new Vector3(Random.Range(-range, range), Random.Range(-range, range), Random.Range(-range, range)); }
		public static Vector2 RandomVector2(float rangeX, float rangeY) { return new Vector2(Random.Range(-rangeX, rangeX), Random.Range(-rangeY, rangeY)); }
		public static Vector3 RandomVector3(float rangeX, float rangeY, float rangeZ) { return new Vector3(Random.Range(-rangeX, rangeX), Random.Range(-rangeY, rangeY), Random.Range(-rangeZ, rangeZ)); }
		public static Vector2 RandomVector2(Vector2 range) { return new Vector2(Random.Range(-range.x, range.x), Random.Range(-range.y, range.y)); }
		public static Vector3 RandomVector3(Vector3 range) { return new Vector3(Random.Range(-range.x, range.x), Random.Range(-range.y, range.y), Random.Range(-range.z, range.z)); }
		public static Vector2 RandomUnitVector2 => new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
		public static Vector3 RandomUnitVector3 => new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));

		#region Random Collections

		public static void FillRandom(this IList<byte> data, byte minInclusive = 0, byte maxExclusive = 255, int count = -1)
		{
			if (count < 0)
				count = data.Count;
			for (int i = 0; i < count; i++)
			{
				data[i] = (byte)Random.Range(minInclusive, maxExclusive);
			}
		}

		public static void FillRandomLowerLetters(this IList<char> data, int count = -1)
		{
			if (count < 0)
				count = data.Count;
			for (int i = 0; i < count; i++)
			{
				data[i] = (char)Random.Range((int)'a', (int)'z');
			}
		}

		#endregion

		#region Collection Operations - RandomIndexSelection

		public static int RandomIndexSelection<T>(this T[] list)
		{
			if (list.Length == 0)
				return -1;
			return Random.Range(0, list.Length);
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
			return Random.Range(0, collection.Count);
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
			int index = Random.Range(0, list.Count);
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
			return list[Random.Range(0, list.Length)];
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
			return list[Random.Range(0, list.Count)];
		}

		public static T RandomSelection<T>(this IList<T> list, System.Random random)
		{
			if (list.Count == 0)
				return default(T);
			return list[random.Next(0, list.Count)];
		}

		public static T RandomSelection<T>(this IList<T> list, bool removeFromlist)
		{
			if (list.Count == 0)
				return default(T);
			int index = Random.Range(0, list.Count);
			T val = list[index];
			if (removeFromlist)
				list.RemoveAt(index);
			return val;
		}

		public static T RandomSelection<T>(this IList<T> list, bool removeFromlist, System.Random random)
		{
			if (list.Count == 0)
				return default(T);
			int index = random.Next(0, list.Count);
			T val = list[index];
			if (removeFromlist)
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
				var k = Random.Range(0, n + 1);
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
			return (T)values.GetValue(Random.Range(0, values.Length));
		}

		#endregion
	}

}
