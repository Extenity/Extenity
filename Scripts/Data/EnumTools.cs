using System;
using System.Collections.Generic;
using System.Linq;

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
}
