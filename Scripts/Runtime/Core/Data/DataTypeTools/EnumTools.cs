using System;
using System.Collections.Generic;
using System.Linq;

namespace Extenity.DataToolbox
{

	public static class EnumTools
	{
		public static T ParseSafe<T>(string value, bool ignoreCase = false) where T : Enum
		{
			try
			{
				var result = (T)Enum.Parse(typeof(T), value, ignoreCase);
				return result;
			}
			catch
			{
			}
			return default;
		}

		public static IEnumerable<T> GetValues<T>() where T : Enum
		{
			return Enum.GetValues(typeof(T)).Cast<T>();
		}

		public static Dictionary<int, string> GetIntValuesAndNames<T>() where T : Enum
		{
			var enumEntries = Enum.GetValues(typeof(T));
			var dictionary = new Dictionary<int, string>(enumEntries.Length);
			foreach (var enumEntry in enumEntries)
			{
				var value = (int)enumEntry;
				var name = enumEntry.ToString();
				dictionary.Add(value, name);
			}
			return dictionary;
		}

		public static bool ContainsValue<T>(int value) where T : Enum
		{
			return Enum.GetValues(typeof(T)).Cast<int>().Contains(value);
		}

		public static bool IsAny<T>(this T value, params T[] comparedValues) where T : Enum
		{
			foreach (var comparedValue in comparedValues)
			{
				if (value.Equals(comparedValue))
					return true;
			}
			return false;
		}

		#region Enum String Cache

		// Cache for enum string representations to avoid repeated allocations
		private static readonly Dictionary<Type, Dictionary<object, string>> EnumStringCache = new();

		/// <summary>
		/// Fast enum to string conversion with caching for better performance.
		/// Use this instead of ToString() for frequently accessed enum values.
		/// </summary>
		public static string ToStringFast<T>(this T enumValue) where T : Enum
		{
			var enumType = typeof(T);

			// Get or create cache for this enum type
			if (!EnumStringCache.TryGetValue(enumType, out var typeCache))
			{
				typeCache = new Dictionary<object, string>();
				EnumStringCache[enumType] = typeCache;

				// Pre-populate cache with all enum values
				foreach (var value in Enum.GetValues(enumType))
				{
					typeCache[value] = value.ToString();
				}
			}

			// Return cached string
			return typeCache[enumValue];
		}

		/// <summary>
		/// Non-generic version of ToStringFast for when you only have the enum type at runtime.
		/// </summary>
		public static string ToStringFast(this Enum enumValue)
		{
			var enumType = enumValue.GetType();

			// Get or create cache for this enum type
			if (!EnumStringCache.TryGetValue(enumType, out var typeCache))
			{
				typeCache = new Dictionary<object, string>();
				EnumStringCache[enumType] = typeCache;

				// Pre-populate cache with all enum values
				foreach (var value in Enum.GetValues(enumType))
				{
					typeCache[value] = value.ToString();
				}
			}

			// Return cached string
			return typeCache[enumValue];
		}

		/// <summary>
		/// Clears the string cache for a specific enum type.
		/// Useful if you need to free memory for a specific enum.
		/// </summary>
		public static void ClearStringCache<T>() where T : Enum
		{
			EnumStringCache.Remove(typeof(T));
		}

		/// <summary>
		/// Clears all string caches.
		/// Useful for freeing memory when enums are no longer needed.
		/// </summary>
		public static void ClearAllStringCaches()
		{
			EnumStringCache.Clear();
		}

		#endregion
	}

}
