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
	}

}
