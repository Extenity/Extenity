using System;
using System.Reflection;
using UnityEngine;
using System.Collections;

public static class ReflectionTools
{
	public static object GetFieldValue(object source, string fieldName)
	{
		if (source == null)
			throw new ArgumentNullException("source");
		if (string.IsNullOrEmpty(fieldName))
			throw new ArgumentException("fieldName");

		var type = source.GetType();
		var fieldInfo = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
		if (fieldInfo == null)
		{
			var propertyInfo = type.GetProperty(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
			if (propertyInfo == null)
				return null;
			return propertyInfo.GetValue(source, null);
		}
		return fieldInfo.GetValue(source);
	}

	public static object GetFieldValueFromArray(object source, string fieldName, int index)
	{
		if (source == null)
			throw new ArgumentNullException("source");
		if (string.IsNullOrEmpty(fieldName))
			throw new ArgumentException("fieldName");
		if (index < 0)
			throw new ArgumentException("index");

		var enumerable = GetFieldValue(source, fieldName) as IEnumerable;
		var enumerator = enumerable.GetEnumerator();
		while (index-- >= 0)
			enumerator.MoveNext();
		return enumerator.Current;
	}

	#region SizeOf<T> Enhanced

	private struct TypeSizeProxy<T>
	{
		public T PublicField;
	}

	public static int SizeOf<T>()
	{
		try
		{
			return System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
		}
		catch (ArgumentException)
		{
			return System.Runtime.InteropServices.Marshal.SizeOf(new TypeSizeProxy<T>());
		}
	}

	#endregion
}
