using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Extenity.DataToolbox;
using Extenity.GameObjectToolbox;
using UnityEngine;
using Object = UnityEngine.Object;

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

		#region GetType

		public static Type GetTypeSafe(this object me)
		{
			return me == null
				? null
				: me.GetType();
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

		#region FindAllReferencedGameObjectsInComponents

		// TODO:
		//public static void FindAllReferencedObjectsInScene(this Scene scene, ...)
		//{
		//	...
		//}

		public static void FindAllReferencedGameObjectsInComponents<T>(this IEnumerable<T> components, HashSet<GameObject> result, bool includeChildren) where T : Component
		{
			foreach (var component in components)
			{
				if (component)
					component.FindAllReferencedGameObjectsInComponent(result, includeChildren);
			}
		}

		public static void FindAllReferencedGameObjectsInComponent<T>(this T component, HashSet<GameObject> result, bool includeChildren) where T : Component
		{
			var serializedFields = component.GetUnitySerializedFields();
			component.FindAllReferencedGameObjectsInSerializedFields(serializedFields, result, includeChildren);
		}

		public static void FindAllReferencedGameObjectsInGameObject(this GameObject gameObject, HashSet<GameObject> result, bool includeChildren)
		{
			var components = gameObject.GetComponents<Component>();
			components.FindAllReferencedGameObjectsInComponents(result, includeChildren);
		}

		public static void FindAllReferencedGameObjectsInUnityObject(this Object unityObject, HashSet<GameObject> result, bool includeChildren)
		{
			var serializedFields = unityObject.GetUnitySerializedFields();
			unityObject.FindAllReferencedGameObjectsInSerializedFields(serializedFields, result, includeChildren);
		}

		public static void FindAllReferencedGameObjectsInSerializedFields(this Object unityObject, IEnumerable<FieldInfo> serializedFields, HashSet<GameObject> result, bool includeChildren)
		{
			foreach (var serializedField in serializedFields)
			{
				unityObject.FindAllReferencedGameObjectsInSerializedFields(serializedField, result, includeChildren);
			}
		}

		public static void FindAllReferencedGameObjectsInSerializedFields(this Object unityObject, FieldInfo serializedField, HashSet<GameObject> result, bool includeChildren)
		{
			var serializedFieldType = serializedField.FieldType;

			if (serializedFieldType.IsArray)
			{
				var array = serializedField.GetValue(unityObject) as Array;
				if (array != null)
				{
					foreach (var item in array)
					{
						var itemAsObject = item as Object;
						if (itemAsObject)
						{
							itemAsObject.FindAllReferencedGameObjectsInUnityObject(result, includeChildren);
						}
					}
				}
			}
			else
			{
				GameObject referencedGameObject = null;
				if (serializedFieldType.IsSameOrSubclassOf(typeof(Component)))
				{
					var referencedComponent = serializedField.GetValue(unityObject) as Component;
					if (referencedComponent)
					{
						referencedGameObject = referencedComponent.gameObject;
					}
				}
				else if (serializedFieldType.IsSameOrSubclassOf(typeof(GameObject)))
				{
					referencedGameObject = serializedField.GetValue(unityObject) as GameObject;
				}
				else if (serializedFieldType.IsSameOrSubclassOf(typeof(Mesh)))
				{
					// Does not contain any link to game objects. So we skip.
				}
				else if (serializedFieldType.IsSameOrSubclassOf(typeof(Material)))
				{
					// Does not contain any link to game objects. So we skip.
				}
				else if (serializedFieldType.IsSameOrSubclassOf(typeof(Texture)))
				{
					// Does not contain any link to game objects. So we skip.
				}
				else if (serializedFieldType.IsSameOrSubclassOf(typeof(TerrainData)))
				{
					// Does not contain any link to game objects. So we skip.
				}
				else if (serializedFieldType.IsSubclassOf(typeof(Object))) // Other objects
				{
					// If we encounter this log line, we should define another 'if' case like Component and GameObject above.
					// The commented out code below should handle serialized fields of this unknown object but it's safer 
					// to handle the object manually. See how Component and GameObject is handled in their own way and
					// figure out how to handle this unknown type likewise.
					Debug.LogWarningFormat("----- Found an unknown object of type '{0}' in one of the fields. See the code for details.", serializedFieldType.FullName);

					// These lines are intentionally commented out. See the comment above.
					//var referencedObject = serializedField.GetValue(unityObject) as Object;
					//if (referencedObject) 
					//{
					//	referencedObject.FindAllReferencedObjectsInUnityObject(result, true);
					//	return;
					//}
				}

				if (referencedGameObject)
				{
					var isAdded = result.Add(referencedGameObject);
					// Check if the gameobject was added before, which means we have already processed the gameobject.
					// This will also prevent going into an infinite loop where there are circular references.
					if (includeChildren && isAdded)
					{
						referencedGameObject.FindAllReferencedGameObjectsInGameObject(result, includeChildren);
					}
				}
			}
		}

		#endregion

		#region FindAllReferencedObjectsInComponents

		// TODO:
		//public static void FindAllReferencedObjectsInScene(this Scene scene, ...)
		//{
		//	...
		//}

		public static void FindAllReferencedObjectsInComponents<T>(this IEnumerable<T> components, HashSet<Object> result, bool includeChildren) where T : Component
		{
			foreach (var component in components)
			{
				if (component)
					component.FindAllReferencedObjectsInComponent(result, includeChildren);
			}
		}

		public static void FindAllReferencedObjectsInComponent<T>(this T component, HashSet<Object> result, bool includeChildren) where T : Component
		{
			var serializedFields = component.GetUnitySerializedFields();
			component.FindAllReferencedObjectsInSerializedFields(serializedFields, result, includeChildren);
		}

		public static void FindAllReferencedObjectsInGameObject(this GameObject gameObject, HashSet<Object> result, bool includeChildren)
		{
			var components = gameObject.GetComponents<Component>();
			components.FindAllReferencedObjectsInComponents(result, includeChildren);
		}

		public static void FindAllReferencedObjectsInUnityObject(this Object unityObject, HashSet<Object> result, bool includeChildren)
		{
			var serializedFields = unityObject.GetUnitySerializedFields();
			unityObject.FindAllReferencedObjectsInSerializedFields(serializedFields, result, includeChildren);
		}

		//public static void FindAllReferencedObjectsInUnityObject<TObject>(this Object unityObject, HashSet<GameObject> result, bool includeChildren) where TObject : Object
		//{
		//	var allObjects = new HashSet<Object>();
		//	unityObject.FindAllReferencedObjectsInUnityObject(allGameObjects, )
		//}

		public static void FindAllReferencedObjectsInSerializedFields(this Object unityObject, IEnumerable<FieldInfo> serializedFields, HashSet<Object> result, bool includeChildren)
		{
			foreach (var serializedField in serializedFields)
			{
				unityObject.FindAllReferencedObjectsInSerializedFields(serializedField, result, includeChildren);
			}
		}

		public static void FindAllReferencedObjectsInSerializedFields(this Object unityObject, FieldInfo serializedField, HashSet<Object> result, bool includeChildren)
		{
			var serializedFieldType = serializedField.FieldType;

			if (serializedFieldType.IsArray)
			{
				var array = serializedField.GetValue(unityObject) as Array;
				if (array != null)
				{
					foreach (var item in array)
					{
						var itemAsObject = item as Object;
						if (itemAsObject)
						{
							itemAsObject.FindAllReferencedObjectsInUnityObject(result, includeChildren);
						}
					}
				}
			}
			else
			{
				Object referencedObject = null;
				if (serializedFieldType.IsSameOrSubclassOf(typeof(Component)))
				{
					referencedObject = null; // Better leave referenced object as null because it will be added in the process below.
					var component = serializedField.GetValue(unityObject) as Component;
					// A Component is also part of a GameObject. And this GameObject also contains other Components.
					// Process all of them!
					if (component)
					{
						component.gameObject.FindAllReferencedObjectsInGameObject(result, includeChildren);
					}
				}
				else if (serializedFieldType.IsSameOrSubclassOf(typeof(GameObject)))
				{
					referencedObject = null; // Better leave referenced object as null because it will be added in the process below.
					var gameObject = serializedField.GetValue(unityObject) as GameObject;
					// GameObject also contains other Components.
					// Process all of them!
					if (gameObject)
					{
						gameObject.FindAllReferencedObjectsInGameObject(result, includeChildren);
					}
				}
				else if (serializedFieldType.IsSameOrSubclassOf(typeof(Mesh)))
				{
					referencedObject = serializedField.GetValue(unityObject) as Mesh;
				}
				else if (serializedFieldType.IsSameOrSubclassOf(typeof(Material)))
				{
					referencedObject = serializedField.GetValue(unityObject) as Material;
				}
				else if (serializedFieldType.IsSameOrSubclassOf(typeof(Texture)))
				{
					referencedObject = serializedField.GetValue(unityObject) as Texture;
				}
				else if (serializedFieldType.IsSameOrSubclassOf(typeof(TerrainData)))
				{
					referencedObject = serializedField.GetValue(unityObject) as TerrainData;
				}
				else if (serializedFieldType.IsSubclassOf(typeof(Object))) // Other objects
				{
					// If we encounter this log line, we should define another 'if' case like Component and GameObject above.
					// The commented out code below should handle serialized fields of this unknown object but it's safer 
					// to handle the object manually. See how Component and GameObject is handled in their own way and
					// figure out how to handle this unknown type likewise.
					Debug.LogWarningFormat("----- Found an unknown object of type '{0}' in one of the fields. See the code for details.", serializedFieldType.FullName);

					// These lines are intentionally commented out. See the comment above.
					//var referencedObject = serializedField.GetValue(unityObject) as Object;
					//if (referencedObject) 
					//{
					//	referencedObject.FindAllReferencedObjectsInUnityObject(result, true);
					//	return;
					//}
				}

				if (referencedObject)
				{
					var isAdded = result.Add(referencedObject);
					// Check if the object was added before, which means we have already processed the object.
					// This will also prevent going into an infinite loop where there are circular references.
					if (includeChildren && isAdded)
					{
						referencedObject.FindAllReferencedObjectsInUnityObject(result, includeChildren);
					}
				}
			}
		}

		#endregion

		#region Referenced Object Checks

		public static bool IsFieldReferencesUnityObject(this Object unityObject, FieldInfo fieldOfUnityObject, Object expectedUnityObject)
		{
			if (!expectedUnityObject)
				throw new ArgumentNullException("expectedUnityObject");

			if (fieldOfUnityObject.FieldType.IsArray)
			{
				var array = fieldOfUnityObject.GetValue(unityObject) as Array;
				if (array == null)
					return false;
				foreach (var item in array)
				{
					var itemAsObject = item as Object;
					if (itemAsObject == expectedUnityObject)
						return true;
				}
			}
			else
			{
				var item = fieldOfUnityObject.GetValue(unityObject) as Object;
				if (item == expectedUnityObject)
					return true;
			}
			return false;
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
