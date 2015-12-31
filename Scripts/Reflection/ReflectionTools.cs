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

	#region Method

	public static bool CompareMethodParameters(this ParameterInfo[] params1, ParameterInfo[] params2, bool compareParameterNames = false)
	{
		if (params1 == null)
			throw new ArgumentNullException("params1");
		if (params2 == null)
			throw new ArgumentNullException("params2");

		if (params1.Length != params2.Length)
			return false;

		for (int i = 0; i < params1.Length; i++)
		{
			var param1 = params1[i];
			var param2 = params2[i];

			if (param1.ParameterType != param2.ParameterType)
				return false;

			if (compareParameterNames)
			{
				if (param1.Name != param2.Name)
					return false;
			}
		}

		return true;
	}

	#endregion

	#region MethodInfo to Method in runtime

	/// <example>
	/// <code>
	/// protected void Test_ConvertToFuncWithSingleParameter()
	/// {
	/// 	MethodInfo indexOf = typeof(string).GetMethod("IndexOf", new Type[] { typeof(char) });
	/// 	MethodInfo getByteCount = typeof(Encoding).GetMethod("GetByteCount", new Type[] { typeof(string) });
	/// 
	/// 	Func<string, object, object> indexOfFunc = indexOf.ConvertToFuncWithSingleParameter<string>();
	/// 	Func<Encoding, object, object> getByteCountFunc = getByteCount.ConvertToFuncWithSingleParameter<Encoding>();
	/// 
	/// 	Debug.Log(indexOfFunc("Hello", 'e'));
	/// 	Debug.Log(getByteCountFunc(Encoding.UTF8, "Euro sign: u20ac"));
	/// }
	/// </code>
	/// </example>
	/// <remarks>http://codeblog.jonskeet.uk/2008/08/09/making-reflection-fly-and-exploring-delegates/</remarks>
	public static Func<TInstance, object, object> ConvertToFuncWithSingleParameter<TInstance>(this MethodInfo method)
	{
		// First fetch the generic form
		MethodInfo genericHelper = typeof(UberBehaviour).GetMethod("ConvertToFuncWithSingleParameterHelper", BindingFlags.Static | BindingFlags.NonPublic);

		// Now supply the type arguments
		MethodInfo constructedHelper = genericHelper.MakeGenericMethod(typeof(TInstance), method.GetParameters()[0].ParameterType, method.ReturnType);

		// Now call it. The null argument is because it’s a static method.
		object ret = constructedHelper.Invoke(null, new object[] { method });

		// Cast the result to the right kind of delegate and return it
		return (Func<TInstance, object, object>)ret;
	}

	private static Func<TTarget, object, object> ConvertToFuncWithSingleParameterHelper<TTarget, TParam, TReturn>(MethodInfo method)
	{
		// Convert the slow MethodInfo into a fast, strongly typed, open delegate
		var func = (Func<TTarget, TParam, TReturn>)Delegate.CreateDelegate(typeof(Func<TTarget, TParam, TReturn>), method);

		// Now create a more weakly typed delegate which will call the strongly typed one
		Func<TTarget, object, object> ret = (TTarget target, object param) => func(target, (TParam)param);
		return ret;
	}

	#endregion

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
