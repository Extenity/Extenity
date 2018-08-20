using System;
using System.Collections.Generic;
using System.Linq;

namespace Extenity.DataToolbox
{

	public static class EnumTools
	{
		public static T ParseSafe<T>(string value, bool ignoreCase = false)
		{
			var enumType = typeof(T);

			if (!enumType.IsEnum)
				throw new ArgumentException("Generic type must be an enumeration.", "enumType");

			try
			{
				var result = (T)Enum.Parse(enumType, value, ignoreCase);
				return result;
			}
			catch
			{
			}
			return default(T);
		}

		public static IEnumerable<T> GetValues<T>()
		{
			return Enum.GetValues(typeof(T)).Cast<T>();
		}

		public static Dictionary<int, string> GetIntValuesAndNames<T>()
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
	}

}
