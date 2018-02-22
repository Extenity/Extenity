using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Extenity.DataToolbox;
using Extenity.GameObjectToolbox;
using UnityEngine;

namespace Extenity.ReflectionToolbox
{

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

		#region GetField

		private static FieldInfo InternalGetFieldInfo(Type type, string fieldName)
		{
			var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
			if (field == null)
			{
				throw new Exception(string.Format("Type '{0}' does not have the field '{1}'.", type, fieldName));
			}
			return field;
		}

		public static void GetFieldAsFunc<TInstance, TResult>(this Type type, string fieldName, out InstanceFunc<TInstance, TResult> result)
		{
			var field = InternalGetFieldInfo(type, fieldName);
			result = (instance) =>
			{
				var ret = field.GetValue(instance);
				return (TResult)ret;
			};
		}

		public static void GetStaticFieldAsFunc<TResult>(this Type type, string fieldName, out Func<TResult> result)
		{
			var field = InternalGetFieldInfo(type, fieldName);
			result = () =>
			{
				var ret = field.GetValue(null);
				return (TResult)ret;
			};
		}

		#endregion

		#region GetMethod

		private static MethodInfo InternalGetMethodInfo(Type type, string methodName, Type[] types)
		{
			var method = type.GetMethod(methodName,
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
				null, CallingConventions.Any, types, null);
			if (method == null)
			{
				throw new Exception(string.Format("Type '{0}' does not have the method '{1}'.", type, methodName));
			}
			return method;
		}

		private static MethodInfo InternalGetStaticMethodInfo(Type type, string methodName, Type[] types)
		{
			var method = type.GetMethod(methodName,
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
				null, CallingConventions.Any, types, null);
			if (method == null)
			{
				throw new Exception(string.Format("Type '{0}' does not have the static method '{1}'.", type, methodName));
			}
			return method;
		}

		// --------------------------------------------------------------
		// GetMethodAsAction
		// --------------------------------------------------------------

		public static void GetMethodAsAction<TInstance>(this Type type, string methodName, out InstanceAction<TInstance> result, Type[] overrideTypes = null)
		{
			var method = InternalGetMethodInfo(type, methodName, new Type[0]);
			result = instance =>
			{
				method.Invoke(instance, CollectionTools.EmptyObjectArray);
			};
		}

		public static void GetMethodAsAction<TInstance, T1>(this Type type, string methodName, out InstanceAction<TInstance, T1> result, Type[] overrideTypes = null)
		{
			var method = InternalGetMethodInfo(type, methodName, overrideTypes != null ? overrideTypes : new[] { typeof(T1) });
			result = (instance, arg1) =>
			{
				method.Invoke(instance, new object[] { arg1 });
			};
		}

		public static void GetMethodAsAction<TInstance, T1, T2>(this Type type, string methodName, out InstanceAction<TInstance, T1, T2> result, Type[] overrideTypes = null)
		{
			var method = InternalGetMethodInfo(type, methodName, overrideTypes != null ? overrideTypes : new[] { typeof(T1), typeof(T2) });
			result = (instance, arg1, arg2) =>
			{
				method.Invoke(instance, new object[] { arg1, arg2 });
			};
		}

		public static void GetMethodAsAction<TInstance, T1, T2, T3>(this Type type, string methodName, out InstanceAction<TInstance, T1, T2, T3> result, Type[] overrideTypes = null)
		{
			var method = InternalGetMethodInfo(type, methodName, overrideTypes != null ? overrideTypes : new[] { typeof(T1), typeof(T2), typeof(T3) });
			result = (instance, arg1, arg2, arg3) =>
			{
				method.Invoke(instance, new object[] { arg1, arg2, arg3 });
			};
		}

		public static void GetMethodAsAction<TInstance, T1, T2, T3, T4>(this Type type, string methodName, out InstanceAction<TInstance, T1, T2, T3, T4> result, Type[] overrideTypes = null)
		{
			var method = InternalGetMethodInfo(type, methodName, overrideTypes != null ? overrideTypes : new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) });
			result = (instance, arg1, arg2, arg3, arg4) =>
			{
				method.Invoke(instance, new object[] { arg1, arg2, arg3, arg4 });
			};
		}

		public static void GetMethodAsAction<TInstance, T1, T2, T3, T4, T5>(this Type type, string methodName, out InstanceAction<TInstance, T1, T2, T3, T4, T5> result, Type[] overrideTypes = null)
		{
			var method = InternalGetMethodInfo(type, methodName, overrideTypes != null ? overrideTypes : new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) });
			result = (instance, arg1, arg2, arg3, arg4, arg5) =>
			{
				method.Invoke(instance, new object[] { arg1, arg2, arg3, arg4, arg5 });
			};
		}

		public static void GetMethodAsAction<TInstance, T1, T2, T3, T4, T5, T6>(this Type type, string methodName, out InstanceAction<TInstance, T1, T2, T3, T4, T5, T6> result, Type[] overrideTypes = null)
		{
			var method = InternalGetMethodInfo(type, methodName, overrideTypes != null ? overrideTypes : new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) });
			result = (instance, arg1, arg2, arg3, arg4, arg5, arg6) =>
			{
				method.Invoke(instance, new object[] { arg1, arg2, arg3, arg4, arg5, arg6 });
			};
		}

		public static void GetMethodAsAction<TInstance, T1, T2, T3, T4, T5, T6, T7>(this Type type, string methodName, out InstanceAction<TInstance, T1, T2, T3, T4, T5, T6, T7> result, Type[] overrideTypes = null)
		{
			var method = InternalGetMethodInfo(type, methodName, overrideTypes != null ? overrideTypes : new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7) });
			result = (instance, arg1, arg2, arg3, arg4, arg5, arg6, arg7) =>
			{
				method.Invoke(instance, new object[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7 });
			};
		}

		public static void GetMethodAsAction<TInstance, T1, T2, T3, T4, T5, T6, T7, T8>(this Type type, string methodName, out InstanceAction<TInstance, T1, T2, T3, T4, T5, T6, T7, T8> result, Type[] overrideTypes = null)
		{
			var method = InternalGetMethodInfo(type, methodName, overrideTypes != null ? overrideTypes : new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8) });
			result = (instance, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) =>
			{
				method.Invoke(instance, new object[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 });
			};
		}

		public static void GetMethodAsAction<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Type type, string methodName, out InstanceAction<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9> result, Type[] overrideTypes = null)
		{
			var method = InternalGetMethodInfo(type, methodName, overrideTypes != null ? overrideTypes : new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9) });
			result = (instance, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9) =>
			{
				method.Invoke(instance, new object[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9 });
			};
		}

		// --------------------------------------------------------------
		// GetMethodAsFunc
		// --------------------------------------------------------------

		public static void GetMethodAsFunc<TInstance, TResult>(this Type type, string methodName, out InstanceFunc<TInstance, TResult> result, Type[] overrideTypes = null)
		{
			var method = InternalGetMethodInfo(type, methodName, new Type[0]);
			result = (instance) =>
			{
				var ret = method.Invoke(instance, CollectionTools.EmptyObjectArray);
				return (TResult)ret;
			};
		}

		public static void GetMethodAsFunc<TInstance, T1, TResult>(this Type type, string methodName, out InstanceFunc<TInstance, T1, TResult> result, Type[] overrideTypes = null)
		{
			var method = InternalGetMethodInfo(type, methodName, overrideTypes != null ? overrideTypes : new[] { typeof(T1) });
			result = (instance, arg1) =>
			{
				var ret = method.Invoke(instance, new object[] { arg1 });
				return (TResult)ret;
			};
		}

		public static void GetMethodAsFunc<TInstance, T1, T2, TResult>(this Type type, string methodName, out InstanceFunc<TInstance, T1, T2, TResult> result, Type[] overrideTypes = null)
		{
			var method = InternalGetMethodInfo(type, methodName, overrideTypes != null ? overrideTypes : new[] { typeof(T1), typeof(T2) });
			result = (instance, arg1, arg2) =>
			{
				var ret = method.Invoke(instance, new object[] { arg1, arg2 });
				return (TResult)ret;
			};
		}

		public static void GetMethodAsFunc<TInstance, T1, T2, T3, TResult>(this Type type, string methodName, out InstanceFunc<TInstance, T1, T2, T3, TResult> result, Type[] overrideTypes = null)
		{
			var method = InternalGetMethodInfo(type, methodName, overrideTypes != null ? overrideTypes : new[] { typeof(T1), typeof(T2), typeof(T3) });
			result = (instance, arg1, arg2, arg3) =>
			{
				var ret = method.Invoke(instance, new object[] { arg1, arg2, arg3 });
				return (TResult)ret;
			};
		}

		public static void GetMethodAsFunc<TInstance, T1, T2, T3, T4, TResult>(this Type type, string methodName, out InstanceFunc<TInstance, T1, T2, T3, T4, TResult> result, Type[] overrideTypes = null)
		{
			var method = InternalGetMethodInfo(type, methodName, overrideTypes != null ? overrideTypes : new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) });
			result = (instance, arg1, arg2, arg3, arg4) =>
			{
				var ret = method.Invoke(instance, new object[] { arg1, arg2, arg3, arg4 });
				return (TResult)ret;
			};
		}

		public static void GetMethodAsFunc<TInstance, T1, T2, T3, T4, T5, TResult>(this Type type, string methodName, out InstanceFunc<TInstance, T1, T2, T3, T4, T5, TResult> result, Type[] overrideTypes = null)
		{
			var method = InternalGetMethodInfo(type, methodName, overrideTypes != null ? overrideTypes : new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) });
			result = (instance, arg1, arg2, arg3, arg4, arg5) =>
			{
				var ret = method.Invoke(instance, new object[] { arg1, arg2, arg3, arg4, arg5 });
				return (TResult)ret;
			};
		}

		public static void GetMethodAsFunc<TInstance, T1, T2, T3, T4, T5, T6, TResult>(this Type type, string methodName, out InstanceFunc<TInstance, T1, T2, T3, T4, T5, T6, TResult> result, Type[] overrideTypes = null)
		{
			var method = InternalGetMethodInfo(type, methodName, overrideTypes != null ? overrideTypes : new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) });
			result = (instance, arg1, arg2, arg3, arg4, arg5, arg6) =>
			{
				var ret = method.Invoke(instance, new object[] { arg1, arg2, arg3, arg4, arg5, arg6 });
				return (TResult)ret;
			};
		}

		public static void GetMethodAsFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, TResult>(this Type type, string methodName, out InstanceFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, TResult> result, Type[] overrideTypes = null)
		{
			var method = InternalGetMethodInfo(type, methodName, overrideTypes != null ? overrideTypes : new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7) });
			result = (instance, arg1, arg2, arg3, arg4, arg5, arg6, arg7) =>
			{
				var ret = method.Invoke(instance, new object[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7 });
				return (TResult)ret;
			};
		}

		public static void GetMethodAsFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, TResult>(this Type type, string methodName, out InstanceFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, TResult> result, Type[] overrideTypes = null)
		{
			var method = InternalGetMethodInfo(type, methodName, overrideTypes != null ? overrideTypes : new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8) });
			result = (instance, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) =>
			{
				var ret = method.Invoke(instance, new object[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 });
				return (TResult)ret;
			};
		}

		public static void GetMethodAsFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(this Type type, string methodName, out InstanceFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> result, Type[] overrideTypes = null)
		{
			var method = InternalGetMethodInfo(type, methodName, overrideTypes != null ? overrideTypes : new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9) });
			result = (instance, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9) =>
			{
				var ret = method.Invoke(instance, new object[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9 });
				return (TResult)ret;
			};
		}

		// --------------------------------------------------------------
		// GetStaticMethodAsAction
		// --------------------------------------------------------------

		public static void GetStaticMethodAsAction(this Type type, string methodName, out Action result)
		{
			var method = InternalGetStaticMethodInfo(type, methodName, new Type[0]);
			result = () =>
			{
				method.Invoke(null, new object[] { CollectionTools.EmptyObjectArray });
			};
		}

		public static void GetStaticMethodAsAction<T1>(this Type type, string methodName, out Action<T1> result, Type[] overrideTypes = null)
		{
			var method = InternalGetStaticMethodInfo(type, methodName, overrideTypes != null ? overrideTypes : new[] { typeof(T1) });
			result = (arg1) =>
			{
				method.Invoke(null, new object[] { arg1 });
			};
		}

		public static void GetStaticMethodAsAction<T1, T2>(this Type type, string methodName, out Action<T1, T2> result, Type[] overrideTypes = null)
		{
			var method = InternalGetStaticMethodInfo(type, methodName, overrideTypes != null ? overrideTypes : new[] { typeof(T1), typeof(T2) });
			result = (arg1, arg2) =>
			{
				method.Invoke(null, new object[] { arg1, arg2 });
			};
		}

		public static void GetStaticMethodAsAction<T1, T2, T3>(this Type type, string methodName, out Action<T1, T2, T3> result, Type[] overrideTypes = null)
		{
			var method = InternalGetStaticMethodInfo(type, methodName, overrideTypes != null ? overrideTypes : new[] { typeof(T1), typeof(T2), typeof(T3) });
			result = (arg1, arg2, arg3) =>
			{
				method.Invoke(null, new object[] { arg1, arg2, arg3 });
			};
		}

		public static void GetStaticMethodAsAction<T1, T2, T3, T4>(this Type type, string methodName, out Action<T1, T2, T3, T4> result, Type[] overrideTypes = null)
		{
			var method = InternalGetStaticMethodInfo(type, methodName, overrideTypes != null ? overrideTypes : new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) });
			result = (arg1, arg2, arg3, arg4) =>
			{
				method.Invoke(null, new object[] { arg1, arg2, arg3, arg4 });
			};
		}

		// TODO: Uncomment after Unity gets Mono update.
		/*
		public static void GetStaticMethodAsAction<T1, T2, T3, T4, T5>(this Type type, string methodName, out Action<T1, T2, T3, T4, T5> result, Type[] overrideTypes = null)
		{
			var method = InternalGetStaticMethodInfo(type, methodName, overrideTypes != null ? overrideTypes : new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) });
			result = (arg1, arg2, arg3, arg4, arg5) =>
			{
				method.Invoke(null, new object[] { arg1, arg2, arg3, arg4, arg5 });
			};
		}

		public static void GetStaticMethodAsAction<T1, T2, T3, T4, T5, T6>(this Type type, string methodName, out Action<T1, T2, T3, T4, T5, T6> result, Type[] overrideTypes = null)
		{
			var method = InternalGetStaticMethodInfo(type, methodName, overrideTypes != null ? overrideTypes : new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) });
			result = (arg1, arg2, arg3, arg4, arg5, arg6) =>
			{
				method.Invoke(null, new object[] { arg1, arg2, arg3, arg4, arg5, arg6 });
			};
		}

		public static void GetStaticMethodAsAction<T1, T2, T3, T4, T5, T6, T7>(this Type type, string methodName, out Action<T1, T2, T3, T4, T5, T6, T7> result, Type[] overrideTypes = null)
		{
			var method = InternalGetStaticMethodInfo(type, methodName, overrideTypes != null ? overrideTypes : new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7) });
			result = (arg1, arg2, arg3, arg4, arg5, arg6, arg7) =>
			{
				method.Invoke(null, new object[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7 });
			};
		}

		public static void GetStaticMethodAsAction<T1, T2, T3, T4, T5, T6, T7, T8>(this Type type, string methodName, out Action<T1, T2, T3, T4, T5, T6, T7, T8> result, Type[] overrideTypes = null)
		{
			var method = InternalGetStaticMethodInfo(type, methodName, overrideTypes != null ? overrideTypes : new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8) });
			result = (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) =>
			{
				method.Invoke(null, new object[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 });
			};
		}

		public static void GetStaticMethodAsAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Type type, string methodName, out Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> result, Type[] overrideTypes = null)
		{
			var method = InternalGetStaticMethodInfo(type, methodName, overrideTypes != null ? overrideTypes : new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9) });
			result = (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9) =>
			{
				method.Invoke(null, new object[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9 });
			};
		}
		*/

		// --------------------------------------------------------------
		// GetStaticMethodAsFunc
		// --------------------------------------------------------------

		public static void GetStaticMethodAsFunc<TResult>(this Type type, string methodName, out Func<TResult> result, Type[] overrideTypes = null)
		{
			var method = InternalGetStaticMethodInfo(type, methodName, new Type[0]);
			result = () =>
			{
				var ret = method.Invoke(null, CollectionTools.EmptyObjectArray);
				return (TResult)ret;
			};
		}

		public static void GetStaticMethodAsFunc<T1, TResult>(this Type type, string methodName, out Func<T1, TResult> result, Type[] overrideTypes = null)
		{
			var method = InternalGetStaticMethodInfo(type, methodName, overrideTypes != null ? overrideTypes : new[] { typeof(T1) });
			result = (arg1) =>
			{
				var ret = method.Invoke(null, new object[] { arg1 });
				return (TResult)ret;
			};
		}

		public static void GetStaticMethodAsFunc<T1, T2, TResult>(this Type type, string methodName, out Func<T1, T2, TResult> result, Type[] overrideTypes = null)
		{
			var method = InternalGetStaticMethodInfo(type, methodName, overrideTypes != null ? overrideTypes : new[] { typeof(T1), typeof(T2) });
			result = (arg1, arg2) =>
			{
				var ret = method.Invoke(null, new object[] { arg1, arg2 });
				return (TResult)ret;
			};
		}

		public static void GetStaticMethodAsFunc<T1, T2, T3, TResult>(this Type type, string methodName, out Func<T1, T2, T3, TResult> result, Type[] overrideTypes = null)
		{
			var method = InternalGetStaticMethodInfo(type, methodName, overrideTypes != null ? overrideTypes : new[] { typeof(T1), typeof(T2), typeof(T3) });
			result = (arg1, arg2, arg3) =>
			{
				var ret = method.Invoke(null, new object[] { arg1, arg2, arg3 });
				return (TResult)ret;
			};
		}

		public static void GetStaticMethodAsFunc<T1, T2, T3, T4, TResult>(this Type type, string methodName, out Func<T1, T2, T3, T4, TResult> result, Type[] overrideTypes = null)
		{
			var method = InternalGetStaticMethodInfo(type, methodName, overrideTypes != null ? overrideTypes : new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) });
			result = (arg1, arg2, arg3, arg4) =>
			{
				var ret = method.Invoke(null, new object[] { arg1, arg2, arg3, arg4 });
				return (TResult)ret;
			};
		}

		// TODO: Uncomment after Unity gets Mono update.
		/*
		public static void GetStaticMethodAsFunc<T1, T2, T3, T4, T5, TResult>(this Type type, string methodName, out Func<T1, T2, T3, T4, T5, TResult> result, Type[] overrideTypes = null)
		{
			var method = InternalGetStaticMethodInfo(type, methodName, overrideTypes != null ? overrideTypes : new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) });
			result = (arg1, arg2, arg3, arg4, arg5) =>
			{
				var ret = method.Invoke(null, new object[] { arg1, arg2, arg3, arg4, arg5 });
				return (TResult)ret;
			};
		}

		public static void GetStaticMethodAsFunc<T1, T2, T3, T4, T5, T6, TResult>(this Type type, string methodName, out Func<T1, T2, T3, T4, T5, T6, TResult> result, Type[] overrideTypes = null)
		{
			var method = InternalGetStaticMethodInfo(type, methodName, overrideTypes != null ? overrideTypes : new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) });
			result = (arg1, arg2, arg3, arg4, arg5, arg6) =>
			{
				var ret = method.Invoke(null, new object[] { arg1, arg2, arg3, arg4, arg5, arg6 });
				return (TResult)ret;
			};
		}

		public static void GetStaticMethodAsFunc<T1, T2, T3, T4, T5, T6, T7, TResult>(this Type type, string methodName, out Func<T1, T2, T3, T4, T5, T6, T7, TResult> result, Type[] overrideTypes = null)
		{
			var method = InternalGetStaticMethodInfo(type, methodName, overrideTypes != null ? overrideTypes : new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7) });
			result = (arg1, arg2, arg3, arg4, arg5, arg6, arg7) =>
			{
				var ret = method.Invoke(null, new object[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7 });
				return (TResult)ret;
			};
		}

		public static void GetStaticMethodAsFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(this Type type, string methodName, out Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> result, Type[] overrideTypes = null)
		{
			var method = InternalGetStaticMethodInfo(type, methodName, overrideTypes != null ? overrideTypes : new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8) });
			result = (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) =>
			{
				var ret = method.Invoke(null, new object[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 });
				return (TResult)ret;
			};
		}

		public static void GetStaticMethodAsFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(this Type type, string methodName, out Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> result, Type[] overrideTypes = null)
		{
			var method = InternalGetStaticMethodInfo(type, methodName, overrideTypes != null ? overrideTypes : new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9) });
			result = (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9) =>
			{
				var ret = method.Invoke(null, new object[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9 });
				return (TResult)ret;
			};
		}
		*/

		#endregion

		#region MethodInfo to Method in runtime

		/// <example>
		/// <code>
		/// protected void Test_ConvertToFuncWithSingleParameter()
		/// {
		/// 	MethodInfo indexOf = typeof(string).GetMethod("IndexOf", new Type[] { typeof(char) });
		/// 	MethodInfo getByteCount = typeof(Encoding).GetMethod("GetByteCount", new Type[] { typeof(string) });
		/// 
		/// 	Func&lt;string, object, object&gt; indexOfFunc = indexOf.ConvertToFuncWithSingleParameter&lt;string&gt;();
		/// 	Func&lt;Encoding, object, object&gt; getByteCountFunc = getByteCount.ConvertToFuncWithSingleParameter&lt;Encoding&gt;();
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
			MethodInfo genericHelper = typeof(ReflectionTools).GetMethod("ConvertToFuncWithSingleParameterHelper", BindingFlags.Static | BindingFlags.NonPublic);

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

		#region FindAllReferencedObjectsInComponents

		public static void FindAllReferencedObjectsInComponents<T>(this IEnumerable<T> components, HashSet<GameObject> result, bool includeChildren) where T : Component
		{
			foreach (var component in components)
			{
				FindAllReferencedObjectsInComponent(component, result, includeChildren);
			}
		}

		public static void FindAllReferencedObjectsInComponent<T>(this T component, HashSet<GameObject> result, bool includeChildren) where T : Component
		{
			var serializedFields = component.GetUnitySerializedFields();
			foreach (var field in serializedFields)
			{
				GameObject referencedGameObject = null;
				if (field.FieldType.IsSubclassOf(typeof(Component)))
				{
					var referencedComponent = field.GetValue(component) as Component;
					if (referencedComponent)
					{
						referencedGameObject = referencedComponent.gameObject;
					}
				}
				else if (field.FieldType.IsSubclassOf(typeof(GameObject)))
				{
					referencedGameObject = field.GetValue(component) as GameObject;
				}

				if (referencedGameObject)
				{
					var isAdded = result.Add(referencedGameObject);
					// Check if the gameobject was added before, which means we have already processed the gameobject.
					// This will also prevent going into an infinite loop where there are circular references.
					if (includeChildren && isAdded)
					{
						FindAllReferencedObjectsInComponents(referencedGameObject.GetComponents<Component>(), result, includeChildren);
					}
				}
			}
		}

		#endregion

		#region List.GetInternalArray

		// Source: https://stackoverflow.com/questions/4972951/listt-to-t-without-copying
		private static class ArrayAccessor<T>
		{
			public static Func<List<T>, T[]> Getter;

			static ArrayAccessor()
			{
				var dm = new DynamicMethod("get", MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, typeof(T[]), new Type[] { typeof(List<T>) }, typeof(ArrayAccessor<T>), true);
				var il = dm.GetILGenerator();
				il.Emit(OpCodes.Ldarg_0); // Load List<T> argument
				il.Emit(OpCodes.Ldfld, typeof(List<T>).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance)); // Replace argument by field
				il.Emit(OpCodes.Ret); // Return field
				Getter = (Func<List<T>, T[]>)dm.CreateDelegate(typeof(Func<List<T>, T[]>));
			}
		}

		public static T[] GetInternalArray<T>(this List<T> list)
		{
			return ArrayAccessor<T>.Getter(list);
		}

		#endregion
	}

}
