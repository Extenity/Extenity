using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Extenity.DataToolbox;
using Extenity.GameObjectToolbox;
using Extenity.MathToolbox;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Extenity.ReflectionToolbox
{

	public static class ReflectionTools
	{
		public static object GetFieldValue(object source, string fieldName)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));
			if (string.IsNullOrEmpty(fieldName))
				throw new ArgumentNullException(nameof(fieldName));

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
				throw new ArgumentNullException(nameof(source));
			if (string.IsNullOrEmpty(fieldName))
				throw new ArgumentNullException(nameof(fieldName));
			if (index < 0)
				throw new ArgumentOutOfRangeException(nameof(index), index, null);

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
				throw new ArgumentNullException(nameof(params1));
			if (params2 == null)
				throw new ArgumentNullException(nameof(params2));

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
				throw new Exception($"Type '{type}' does not have the field '{fieldName}'.");
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
				throw new Exception($"Type '{type}' does not have the method '{methodName}' with arguments '{string.Join(", ", types.Select(item => item.Name).ToArray())}'.");
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
				throw new Exception($"Type '{type}' does not have the static method '{methodName}' with arguments '{string.Join(", ", types.Select(item => item.Name).ToArray())}'.");
			}
			return method;
		}

		// --------------------------------------------------------------
		// GetMethodAsAction
		// --------------------------------------------------------------

		public static void GetMethodAsAction<TInstance>(this Type type, string methodName, out InstanceAction<TInstance> result)
		{
			var method = InternalGetMethodInfo(type, methodName, new Type[0]);
			result = instance =>
			{
				method.Invoke(instance, CollectionTools.EmptyObjectArray);
			};
		}

		public static void GetMethodAsAction<TInstance, T1>(this Type type, string methodName, out InstanceAction<TInstance, T1> result, Type[] overrideArgumentTypes = null)
		{
			if (overrideArgumentTypes != null)
			{
				if (overrideArgumentTypes.Length != 1)
					throw new Exception("Overriden argument type count mismatch.");
			}
			else
			{
				overrideArgumentTypes = new[] { typeof(T1) };
			}
			var method = InternalGetMethodInfo(type, methodName, overrideArgumentTypes);
			result = (instance, arg1) =>
			{
				method.Invoke(instance, new object[] { arg1 });
			};
		}

		public static void GetMethodAsAction<TInstance, T1, T2>(this Type type, string methodName, out InstanceAction<TInstance, T1, T2> result, Type[] overrideArgumentTypes = null)
		{
			if (overrideArgumentTypes != null)
			{
				if (overrideArgumentTypes.Length != 2)
					throw new Exception("Overriden argument type count mismatch.");
			}
			else
			{
				overrideArgumentTypes = new[] { typeof(T1), typeof(T2) };
			}
			var method = InternalGetMethodInfo(type, methodName, overrideArgumentTypes);
			result = (instance, arg1, arg2) =>
			{
				method.Invoke(instance, new object[] { arg1, arg2 });
			};
		}

		public static void GetMethodAsAction<TInstance, T1, T2, T3>(this Type type, string methodName, out InstanceAction<TInstance, T1, T2, T3> result, Type[] overrideArgumentTypes = null)
		{
			if (overrideArgumentTypes != null)
			{
				if (overrideArgumentTypes.Length != 3)
					throw new Exception("Overriden argument type count mismatch.");
			}
			else
			{
				overrideArgumentTypes = new[] { typeof(T1), typeof(T2), typeof(T3) };
			}
			var method = InternalGetMethodInfo(type, methodName, overrideArgumentTypes);
			result = (instance, arg1, arg2, arg3) =>
			{
				method.Invoke(instance, new object[] { arg1, arg2, arg3 });
			};
		}

		public static void GetMethodAsAction<TInstance, T1, T2, T3, T4>(this Type type, string methodName, out InstanceAction<TInstance, T1, T2, T3, T4> result, Type[] overrideArgumentTypes = null)
		{
			if (overrideArgumentTypes != null)
			{
				if (overrideArgumentTypes.Length != 4)
					throw new Exception("Overriden argument type count mismatch.");
			}
			else
			{
				overrideArgumentTypes = new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) };
			}
			var method = InternalGetMethodInfo(type, methodName, overrideArgumentTypes);
			result = (instance, arg1, arg2, arg3, arg4) =>
			{
				method.Invoke(instance, new object[] { arg1, arg2, arg3, arg4 });
			};
		}

		public static void GetMethodAsAction<TInstance, T1, T2, T3, T4, T5>(this Type type, string methodName, out InstanceAction<TInstance, T1, T2, T3, T4, T5> result, Type[] overrideArgumentTypes = null)
		{
			if (overrideArgumentTypes != null)
			{
				if (overrideArgumentTypes.Length != 5)
					throw new Exception("Overriden argument type count mismatch.");
			}
			else
			{
				overrideArgumentTypes = new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) };
			}
			var method = InternalGetMethodInfo(type, methodName, overrideArgumentTypes);
			result = (instance, arg1, arg2, arg3, arg4, arg5) =>
			{
				method.Invoke(instance, new object[] { arg1, arg2, arg3, arg4, arg5 });
			};
		}

		public static void GetMethodAsAction<TInstance, T1, T2, T3, T4, T5, T6>(this Type type, string methodName, out InstanceAction<TInstance, T1, T2, T3, T4, T5, T6> result, Type[] overrideArgumentTypes = null)
		{
			if (overrideArgumentTypes != null)
			{
				if (overrideArgumentTypes.Length != 6)
					throw new Exception("Overriden argument type count mismatch.");
			}
			else
			{
				overrideArgumentTypes = new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) };
			}
			var method = InternalGetMethodInfo(type, methodName, overrideArgumentTypes);
			result = (instance, arg1, arg2, arg3, arg4, arg5, arg6) =>
			{
				method.Invoke(instance, new object[] { arg1, arg2, arg3, arg4, arg5, arg6 });
			};
		}

		public static void GetMethodAsAction<TInstance, T1, T2, T3, T4, T5, T6, T7>(this Type type, string methodName, out InstanceAction<TInstance, T1, T2, T3, T4, T5, T6, T7> result, Type[] overrideArgumentTypes = null)
		{
			if (overrideArgumentTypes != null)
			{
				if (overrideArgumentTypes.Length != 7)
					throw new Exception("Overriden argument type count mismatch.");
			}
			else
			{
				overrideArgumentTypes = new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7) };
			}
			var method = InternalGetMethodInfo(type, methodName, overrideArgumentTypes);
			result = (instance, arg1, arg2, arg3, arg4, arg5, arg6, arg7) =>
			{
				method.Invoke(instance, new object[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7 });
			};
		}

		public static void GetMethodAsAction<TInstance, T1, T2, T3, T4, T5, T6, T7, T8>(this Type type, string methodName, out InstanceAction<TInstance, T1, T2, T3, T4, T5, T6, T7, T8> result, Type[] overrideArgumentTypes = null)
		{
			if (overrideArgumentTypes != null)
			{
				if (overrideArgumentTypes.Length != 8)
					throw new Exception("Overriden argument type count mismatch.");
			}
			else
			{
				overrideArgumentTypes = new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8) };
			}
			var method = InternalGetMethodInfo(type, methodName, overrideArgumentTypes);
			result = (instance, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) =>
			{
				method.Invoke(instance, new object[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 });
			};
		}

		public static void GetMethodAsAction<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Type type, string methodName, out InstanceAction<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9> result, Type[] overrideArgumentTypes = null)
		{
			if (overrideArgumentTypes != null)
			{
				if (overrideArgumentTypes.Length != 9)
					throw new Exception("Overriden argument type count mismatch.");
			}
			else
			{
				overrideArgumentTypes = new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9) };
			}
			var method = InternalGetMethodInfo(type, methodName, overrideArgumentTypes);
			result = (instance, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9) =>
			{
				method.Invoke(instance, new object[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9 });
			};
		}

		// --------------------------------------------------------------
		// GetMethodAsFunc
		// --------------------------------------------------------------

		public static void GetMethodAsFunc<TInstance, TResult>(this Type type, string methodName, out InstanceFunc<TInstance, TResult> result)
		{
			var method = InternalGetMethodInfo(type, methodName, new Type[0]);
			result = (instance) =>
			{
				var ret = method.Invoke(instance, CollectionTools.EmptyObjectArray);
				return (TResult)ret;
			};
		}

		public static void GetMethodAsFunc<TInstance, T1, TResult>(this Type type, string methodName, out InstanceFunc<TInstance, T1, TResult> result, Type[] overrideArgumentTypes = null)
		{
			if (overrideArgumentTypes != null)
			{
				if (overrideArgumentTypes.Length != 1)
					throw new Exception("Overriden argument type count mismatch.");
			}
			else
			{
				overrideArgumentTypes = new[] { typeof(T1) };
			}
			var method = InternalGetMethodInfo(type, methodName, overrideArgumentTypes);
			result = (instance, arg1) =>
			{
				var ret = method.Invoke(instance, new object[] { arg1 });
				return (TResult)ret;
			};
		}

		public static void GetMethodAsFunc<TInstance, T1, T2, TResult>(this Type type, string methodName, out InstanceFunc<TInstance, T1, T2, TResult> result, Type[] overrideArgumentTypes = null)
		{
			if (overrideArgumentTypes != null)
			{
				if (overrideArgumentTypes.Length != 2)
					throw new Exception("Overriden argument type count mismatch.");
			}
			else
			{
				overrideArgumentTypes = new[] { typeof(T1), typeof(T2) };
			}
			var method = InternalGetMethodInfo(type, methodName, overrideArgumentTypes);
			result = (instance, arg1, arg2) =>
			{
				var ret = method.Invoke(instance, new object[] { arg1, arg2 });
				return (TResult)ret;
			};
		}

		public static void GetMethodAsFunc<TInstance, T1, T2, T3, TResult>(this Type type, string methodName, out InstanceFunc<TInstance, T1, T2, T3, TResult> result, Type[] overrideArgumentTypes = null)
		{
			if (overrideArgumentTypes != null)
			{
				if (overrideArgumentTypes.Length != 3)
					throw new Exception("Overriden argument type count mismatch.");
			}
			else
			{
				overrideArgumentTypes = new[] { typeof(T1), typeof(T2), typeof(T3) };
			}
			var method = InternalGetMethodInfo(type, methodName, overrideArgumentTypes);
			result = (instance, arg1, arg2, arg3) =>
			{
				var ret = method.Invoke(instance, new object[] { arg1, arg2, arg3 });
				return (TResult)ret;
			};
		}

		public static void GetMethodAsFunc<TInstance, T1, T2, T3, T4, TResult>(this Type type, string methodName, out InstanceFunc<TInstance, T1, T2, T3, T4, TResult> result, Type[] overrideArgumentTypes = null)
		{
			if (overrideArgumentTypes != null)
			{
				if (overrideArgumentTypes.Length != 4)
					throw new Exception("Overriden argument type count mismatch.");
			}
			else
			{
				overrideArgumentTypes = new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) };
			}
			var method = InternalGetMethodInfo(type, methodName, overrideArgumentTypes);
			result = (instance, arg1, arg2, arg3, arg4) =>
			{
				var ret = method.Invoke(instance, new object[] { arg1, arg2, arg3, arg4 });
				return (TResult)ret;
			};
		}

		public static void GetMethodAsFunc<TInstance, T1, T2, T3, T4, T5, TResult>(this Type type, string methodName, out InstanceFunc<TInstance, T1, T2, T3, T4, T5, TResult> result, Type[] overrideArgumentTypes = null)
		{
			if (overrideArgumentTypes != null)
			{
				if (overrideArgumentTypes.Length != 5)
					throw new Exception("Overriden argument type count mismatch.");
			}
			else
			{
				overrideArgumentTypes = new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) };
			}
			var method = InternalGetMethodInfo(type, methodName, overrideArgumentTypes);
			result = (instance, arg1, arg2, arg3, arg4, arg5) =>
			{
				var ret = method.Invoke(instance, new object[] { arg1, arg2, arg3, arg4, arg5 });
				return (TResult)ret;
			};
		}

		public static void GetMethodAsFunc<TInstance, T1, T2, T3, T4, T5, T6, TResult>(this Type type, string methodName, out InstanceFunc<TInstance, T1, T2, T3, T4, T5, T6, TResult> result, Type[] overrideArgumentTypes = null)
		{
			if (overrideArgumentTypes != null)
			{
				if (overrideArgumentTypes.Length != 6)
					throw new Exception("Overriden argument type count mismatch.");
			}
			else
			{
				overrideArgumentTypes = new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) };
			}
			var method = InternalGetMethodInfo(type, methodName, overrideArgumentTypes);
			result = (instance, arg1, arg2, arg3, arg4, arg5, arg6) =>
			{
				var ret = method.Invoke(instance, new object[] { arg1, arg2, arg3, arg4, arg5, arg6 });
				return (TResult)ret;
			};
		}

		public static void GetMethodAsFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, TResult>(this Type type, string methodName, out InstanceFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, TResult> result, Type[] overrideArgumentTypes = null)
		{
			if (overrideArgumentTypes != null)
			{
				if (overrideArgumentTypes.Length != 7)
					throw new Exception("Overriden argument type count mismatch.");
			}
			else
			{
				overrideArgumentTypes = new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7) };
			}
			var method = InternalGetMethodInfo(type, methodName, overrideArgumentTypes);
			result = (instance, arg1, arg2, arg3, arg4, arg5, arg6, arg7) =>
			{
				var ret = method.Invoke(instance, new object[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7 });
				return (TResult)ret;
			};
		}

		public static void GetMethodAsFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, TResult>(this Type type, string methodName, out InstanceFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, TResult> result, Type[] overrideArgumentTypes = null)
		{
			if (overrideArgumentTypes != null)
			{
				if (overrideArgumentTypes.Length != 8)
					throw new Exception("Overriden argument type count mismatch.");
			}
			else
			{
				overrideArgumentTypes = new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8) };
			}
			var method = InternalGetMethodInfo(type, methodName, overrideArgumentTypes);
			result = (instance, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) =>
			{
				var ret = method.Invoke(instance, new object[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 });
				return (TResult)ret;
			};
		}

		public static void GetMethodAsFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(this Type type, string methodName, out InstanceFunc<TInstance, T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> result, Type[] overrideArgumentTypes = null)
		{
			if (overrideArgumentTypes != null)
			{
				if (overrideArgumentTypes.Length != 9)
					throw new Exception("Overriden argument type count mismatch.");
			}
			else
			{
				overrideArgumentTypes = new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9) };
			}
			var method = InternalGetMethodInfo(type, methodName, overrideArgumentTypes);
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

		public static void GetStaticMethodAsAction<T1>(this Type type, string methodName, out Action<T1> result, Type[] overrideArgumentTypes = null)
		{
			if (overrideArgumentTypes != null)
			{
				if (overrideArgumentTypes.Length != 1)
					throw new Exception("Overriden argument type count mismatch.");
			}
			else
			{
				overrideArgumentTypes = new[] { typeof(T1) };
			}
			var method = InternalGetStaticMethodInfo(type, methodName, overrideArgumentTypes);
			result = (arg1) =>
			{
				method.Invoke(null, new object[] { arg1 });
			};
		}

		public static void GetStaticMethodAsAction<T1, T2>(this Type type, string methodName, out Action<T1, T2> result, Type[] overrideArgumentTypes = null)
		{
			if (overrideArgumentTypes != null)
			{
				if (overrideArgumentTypes.Length != 2)
					throw new Exception("Overriden argument type count mismatch.");
			}
			else
			{
				overrideArgumentTypes = new[] { typeof(T1), typeof(T2) };
			}
			var method = InternalGetStaticMethodInfo(type, methodName, overrideArgumentTypes);
			result = (arg1, arg2) =>
			{
				method.Invoke(null, new object[] { arg1, arg2 });
			};
		}

		public static void GetStaticMethodAsAction<T1, T2, T3>(this Type type, string methodName, out Action<T1, T2, T3> result, Type[] overrideArgumentTypes = null)
		{
			if (overrideArgumentTypes != null)
			{
				if (overrideArgumentTypes.Length != 3)
					throw new Exception("Overriden argument type count mismatch.");
			}
			else
			{
				overrideArgumentTypes = new[] { typeof(T1), typeof(T2), typeof(T3) };
			}
			var method = InternalGetStaticMethodInfo(type, methodName, overrideArgumentTypes);
			result = (arg1, arg2, arg3) =>
			{
				method.Invoke(null, new object[] { arg1, arg2, arg3 });
			};
		}

		public static void GetStaticMethodAsAction<T1, T2, T3, T4>(this Type type, string methodName, out Action<T1, T2, T3, T4> result, Type[] overrideArgumentTypes = null)
		{
			if (overrideArgumentTypes != null)
			{
				if (overrideArgumentTypes.Length != 4)
					throw new Exception("Overriden argument type count mismatch.");
			}
			else
			{
				overrideArgumentTypes = new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) };
			}
			var method = InternalGetStaticMethodInfo(type, methodName, overrideArgumentTypes);
			result = (arg1, arg2, arg3, arg4) =>
			{
				method.Invoke(null, new object[] { arg1, arg2, arg3, arg4 });
			};
		}

		public static void GetStaticMethodAsAction<T1, T2, T3, T4, T5>(this Type type, string methodName, out Action<T1, T2, T3, T4, T5> result, Type[] overrideArgumentTypes = null)
		{
			if (overrideArgumentTypes != null)
			{
				if (overrideArgumentTypes.Length != 5)
					throw new Exception("Overriden argument type count mismatch.");
			}
			else
			{
				overrideArgumentTypes = new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) };
			}
			var method = InternalGetStaticMethodInfo(type, methodName, overrideArgumentTypes);
			result = (arg1, arg2, arg3, arg4, arg5) =>
			{
				method.Invoke(null, new object[] { arg1, arg2, arg3, arg4, arg5 });
			};
		}

		public static void GetStaticMethodAsAction<T1, T2, T3, T4, T5, T6>(this Type type, string methodName, out Action<T1, T2, T3, T4, T5, T6> result, Type[] overrideArgumentTypes = null)
		{
			if (overrideArgumentTypes != null)
			{
				if (overrideArgumentTypes.Length != 6)
					throw new Exception("Overriden argument type count mismatch.");
			}
			else
			{
				overrideArgumentTypes = new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) };
			}
			var method = InternalGetStaticMethodInfo(type, methodName, overrideArgumentTypes);
			result = (arg1, arg2, arg3, arg4, arg5, arg6) =>
			{
				method.Invoke(null, new object[] { arg1, arg2, arg3, arg4, arg5, arg6 });
			};
		}

		public static void GetStaticMethodAsAction<T1, T2, T3, T4, T5, T6, T7>(this Type type, string methodName, out Action<T1, T2, T3, T4, T5, T6, T7> result, Type[] overrideArgumentTypes = null)
		{
			if (overrideArgumentTypes != null)
			{
				if (overrideArgumentTypes.Length != 7)
					throw new Exception("Overriden argument type count mismatch.");
			}
			else
			{
				overrideArgumentTypes = new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7) };
			}
			var method = InternalGetStaticMethodInfo(type, methodName, overrideArgumentTypes);
			result = (arg1, arg2, arg3, arg4, arg5, arg6, arg7) =>
			{
				method.Invoke(null, new object[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7 });
			};
		}

		public static void GetStaticMethodAsAction<T1, T2, T3, T4, T5, T6, T7, T8>(this Type type, string methodName, out Action<T1, T2, T3, T4, T5, T6, T7, T8> result, Type[] overrideArgumentTypes = null)
		{
			if (overrideArgumentTypes != null)
			{
				if (overrideArgumentTypes.Length != 8)
					throw new Exception("Overriden argument type count mismatch.");
			}
			else
			{
				overrideArgumentTypes = new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8) };
			}
			var method = InternalGetStaticMethodInfo(type, methodName, overrideArgumentTypes);
			result = (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) =>
			{
				method.Invoke(null, new object[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 });
			};
		}

		public static void GetStaticMethodAsAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Type type, string methodName, out Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> result, Type[] overrideArgumentTypes = null)
		{
			if (overrideArgumentTypes != null)
			{
				if (overrideArgumentTypes.Length != 9)
					throw new Exception("Overriden argument type count mismatch.");
			}
			else
			{
				overrideArgumentTypes = new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9) };
			}
			var method = InternalGetStaticMethodInfo(type, methodName, overrideArgumentTypes);
			result = (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9) =>
			{
				method.Invoke(null, new object[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9 });
			};
		}

		// --------------------------------------------------------------
		// GetStaticMethodAsFunc
		// --------------------------------------------------------------

		public static void GetStaticMethodAsFunc<TResult>(this Type type, string methodName, out Func<TResult> result)
		{
			var method = InternalGetStaticMethodInfo(type, methodName, new Type[0]);
			result = () =>
			{
				var ret = method.Invoke(null, CollectionTools.EmptyObjectArray);
				return (TResult)ret;
			};
		}

		public static void GetStaticMethodAsFunc<T1, TResult>(this Type type, string methodName, out Func<T1, TResult> result, Type[] overrideArgumentTypes = null)
		{
			if (overrideArgumentTypes != null)
			{
				if (overrideArgumentTypes.Length != 1)
					throw new Exception("Overriden argument type count mismatch.");
			}
			else
			{
				overrideArgumentTypes = new[] { typeof(T1) };
			}
			var method = InternalGetStaticMethodInfo(type, methodName, overrideArgumentTypes);
			result = (arg1) =>
			{
				var ret = method.Invoke(null, new object[] { arg1 });
				return (TResult)ret;
			};
		}

		public static void GetStaticMethodAsFunc<T1, T2, TResult>(this Type type, string methodName, out Func<T1, T2, TResult> result, Type[] overrideArgumentTypes = null)
		{
			if (overrideArgumentTypes != null)
			{
				if (overrideArgumentTypes.Length != 2)
					throw new Exception("Overriden argument type count mismatch.");
			}
			else
			{
				overrideArgumentTypes = new[] { typeof(T1), typeof(T2) };
			}
			var method = InternalGetStaticMethodInfo(type, methodName, overrideArgumentTypes);
			result = (arg1, arg2) =>
			{
				var ret = method.Invoke(null, new object[] { arg1, arg2 });
				return (TResult)ret;
			};
		}

		public static void GetStaticMethodAsFunc<T1, T2, T3, TResult>(this Type type, string methodName, out Func<T1, T2, T3, TResult> result, Type[] overrideArgumentTypes = null)
		{
			if (overrideArgumentTypes != null)
			{
				if (overrideArgumentTypes.Length != 3)
					throw new Exception("Overriden argument type count mismatch.");
			}
			else
			{
				overrideArgumentTypes = new[] { typeof(T1), typeof(T2), typeof(T3) };
			}
			var method = InternalGetStaticMethodInfo(type, methodName, overrideArgumentTypes);
			result = (arg1, arg2, arg3) =>
			{
				var ret = method.Invoke(null, new object[] { arg1, arg2, arg3 });
				return (TResult)ret;
			};
		}

		public static void GetStaticMethodAsFunc<T1, T2, T3, T4, TResult>(this Type type, string methodName, out Func<T1, T2, T3, T4, TResult> result, Type[] overrideArgumentTypes = null)
		{
			if (overrideArgumentTypes != null)
			{
				if (overrideArgumentTypes.Length != 4)
					throw new Exception("Overriden argument type count mismatch.");
			}
			else
			{
				overrideArgumentTypes = new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) };
			}
			var method = InternalGetStaticMethodInfo(type, methodName, overrideArgumentTypes);
			result = (arg1, arg2, arg3, arg4) =>
			{
				var ret = method.Invoke(null, new object[] { arg1, arg2, arg3, arg4 });
				return (TResult)ret;
			};
		}

		public static void GetStaticMethodAsFunc<T1, T2, T3, T4, T5, TResult>(this Type type, string methodName, out Func<T1, T2, T3, T4, T5, TResult> result, Type[] overrideArgumentTypes = null)
		{
			if (overrideArgumentTypes != null)
			{
				if (overrideArgumentTypes.Length != 5)
					throw new Exception("Overriden argument type count mismatch.");
			}
			else
			{
				overrideArgumentTypes = new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) };
			}
			var method = InternalGetStaticMethodInfo(type, methodName, overrideArgumentTypes);
			result = (arg1, arg2, arg3, arg4, arg5) =>
			{
				var ret = method.Invoke(null, new object[] { arg1, arg2, arg3, arg4, arg5 });
				return (TResult)ret;
			};
		}

		public static void GetStaticMethodAsFunc<T1, T2, T3, T4, T5, T6, TResult>(this Type type, string methodName, out Func<T1, T2, T3, T4, T5, T6, TResult> result, Type[] overrideArgumentTypes = null)
		{
			if (overrideArgumentTypes != null)
			{
				if (overrideArgumentTypes.Length != 6)
					throw new Exception("Overriden argument type count mismatch.");
			}
			else
			{
				overrideArgumentTypes = new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) };
			}
			var method = InternalGetStaticMethodInfo(type, methodName, overrideArgumentTypes);
			result = (arg1, arg2, arg3, arg4, arg5, arg6) =>
			{
				var ret = method.Invoke(null, new object[] { arg1, arg2, arg3, arg4, arg5, arg6 });
				return (TResult)ret;
			};
		}

		public static void GetStaticMethodAsFunc<T1, T2, T3, T4, T5, T6, T7, TResult>(this Type type, string methodName, out Func<T1, T2, T3, T4, T5, T6, T7, TResult> result, Type[] overrideArgumentTypes = null)
		{
			if (overrideArgumentTypes != null)
			{
				if (overrideArgumentTypes.Length != 7)
					throw new Exception("Overriden argument type count mismatch.");
			}
			else
			{
				overrideArgumentTypes = new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7) };
			}
			var method = InternalGetStaticMethodInfo(type, methodName, overrideArgumentTypes);
			result = (arg1, arg2, arg3, arg4, arg5, arg6, arg7) =>
			{
				var ret = method.Invoke(null, new object[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7 });
				return (TResult)ret;
			};
		}

		public static void GetStaticMethodAsFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(this Type type, string methodName, out Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> result, Type[] overrideArgumentTypes = null)
		{
			if (overrideArgumentTypes != null)
			{
				if (overrideArgumentTypes.Length != 8)
					throw new Exception("Overriden argument type count mismatch.");
			}
			else
			{
				overrideArgumentTypes = new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8) };
			}
			var method = InternalGetStaticMethodInfo(type, methodName, overrideArgumentTypes);
			result = (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) =>
			{
				var ret = method.Invoke(null, new object[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 });
				return (TResult)ret;
			};
		}

		public static void GetStaticMethodAsFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(this Type type, string methodName, out Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> result, Type[] overrideArgumentTypes = null)
		{
			if (overrideArgumentTypes != null)
			{
				if (overrideArgumentTypes.Length != 9)
					throw new Exception("Overriden argument type count mismatch.");
			}
			else
			{
				overrideArgumentTypes = new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9) };
			}
			var method = InternalGetStaticMethodInfo(type, methodName, overrideArgumentTypes);
			result = (arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9) =>
			{
				var ret = method.Invoke(null, new object[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9 });
				return (TResult)ret;
			};
		}

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

			// Now call it. The null argument is because it's a static method.
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

		#region FindAllReferencedGameObjects...

		public static void FindAllReferencedGameObjectsInScene(this Scene scene, HashSet<GameObject> result, Type[] excludedTypes)
		{
			if (!scene.IsValid())
				throw new Exception("Scene is not valid.");
			foreach (var gameObject in scene.ListAllGameObjectsInScene())
			{
				FindAllReferencedGameObjectsInGameObject(gameObject, result, excludedTypes);
			}
		}

		public static void FindAllReferencedGameObjectsInComponent<T>(this T component, HashSet<GameObject> result, Type[] excludedTypes) where T : Component
		{
			if (!component)
				throw new ArgumentNullException(nameof(component));
			if (CheckIfTypeExcluded(component.GetType(), excludedTypes))
				return;
			foreach (var serializedField in component.GetUnitySerializedFields())
			{
				var referencedObject = serializedField.GetValue(component);
				InternalAddReferencedObjectOfType(referencedObject, result, excludedTypes);
			}
		}

		public static void FindAllReferencedGameObjectsInGameObject(this GameObject gameObject, HashSet<GameObject> result, Type[] excludedTypes)
		{
			if (!gameObject)
				throw new ArgumentNullException(nameof(gameObject));
			if (CheckIfTypeExcluded(gameObject.GetType(), excludedTypes)) // It's unlikely that an object will derive from GameObject, but here we check for this anyway.
				return;
			foreach (var component in gameObject.GetComponents<Component>())
			{
				FindAllReferencedGameObjectsInComponent(component, result, excludedTypes);
			}
		}

		public static void FindAllReferencedGameObjectsInUnityObject(this Object unityObject, HashSet<GameObject> result, Type[] excludedTypes)
		{
			if (unityObject == null)
				throw new ArgumentNullException(nameof(unityObject));
			InternalAddReferencedObjectOfType(unityObject, result, excludedTypes);
		}

		public static void FindAllReferencedGameObjectsInObject(this object obj, HashSet<GameObject> result, Type[] excludedTypes)
		{
			if (obj == null)
				throw new ArgumentNullException(nameof(obj));
			InternalAddReferencedObjectOfType(obj, result, excludedTypes);
		}

		private static void InternalAddReferencedObjectOfType(object referencedObject, HashSet<GameObject> result, Type[] excludedTypes)
		{
			if (referencedObject == null)
				return; // Nothing to do about this object.

			var type = referencedObject.GetType();

			// See if this object is an array or a list. This method should be called for each item, that we do recursively.
			if (type.IsArray)
			{
				var array = referencedObject as Array;
				if (array != null)
				{
					foreach (var item in array)
					{
						InternalAddReferencedObjectOfType(item, result, excludedTypes);
					}
				}
				return;
			}
			if (type.IsGenericList())
			{
				var list = referencedObject as IList;
				if (list != null)
				{
					foreach (var item in list)
					{
						InternalAddReferencedObjectOfType(item, result, excludedTypes);
					}
				}
				return;
			}

			// See if the type is excluded
			if (CheckIfTypeExcluded(type, excludedTypes))
				return;

			// Decide how to include referenced game objects based on referenced object's type
			if (type.IsSameOrSubclassOf(typeof(AudioSource)))
			{
				var referencedAudioSource = referencedObject as AudioSource;
				if (referencedAudioSource)
				{
					InternalAddReferencedGameObjectToResults(referencedAudioSource.gameObject, result, excludedTypes); // See 57182.
					InternalAddReferencedObjectOfType(referencedAudioSource.clip, result, excludedTypes);
					InternalAddReferencedObjectOfType(referencedAudioSource.outputAudioMixerGroup, result, excludedTypes);
				}
			}
			else if (type.IsSameOrSubclassOf(typeof(AudioClip)))
			{
				// Does not contain any link to game objects. So we skip.
			}
			else if (type.IsSameOrSubclassOf(typeof(AudioMixer)))
			{
				var referencedAudioMixer = referencedObject as AudioMixer;
				if (referencedAudioMixer)
				{
					InternalAddReferencedObjectOfType(referencedAudioMixer.outputAudioMixerGroup, result, excludedTypes);
				}
			}
			else if (type.IsSameOrSubclassOf(typeof(AudioMixerGroup)))
			{
				var referencedAudioMixerGroup = referencedObject as AudioMixerGroup;
				if (referencedAudioMixerGroup)
				{
					// Just as the Component is an inseparable part of a GameObject, an AudioMixerGroup
					// is a part of AudioMixer. So we include the AudioMixer too.
					InternalAddReferencedObjectOfType(referencedAudioMixerGroup.audioMixer, result, excludedTypes);
				}
			}
			else if (type.IsSameOrSubclassOf(typeof(Animator)))
			{
				var referencedAnimator = referencedObject as Animator;
				if (referencedAnimator)
				{
					// An Animator may use it's children without keeping any references to them.
					// So we need to assume they are referenced.
					InternalAddReferencedGameObjectToResults(referencedAnimator.gameObject, result, excludedTypes); // See 57182.
					var children = new List<GameObject>();
					referencedAnimator.gameObject.ListAllChildrenGameObjects(children, false);
					foreach (var childGO in children)
					{
						InternalAddReferencedGameObjectToResults(childGO, result, excludedTypes);
					}
				}
			}
			else if (type.IsSameOrSubclassOf(typeof(Mesh)))
			{
				// Does not contain any link to game objects. So we skip.
			}
			else if (type.IsSameOrSubclassOf(typeof(Material)))
			{
				// Does not contain any link to game objects. So we skip.
			}
			else if (type.IsSameOrSubclassOf(typeof(Texture)))
			{
				// Does not contain any link to game objects. So we skip.
			}
			else if (type.IsSameOrSubclassOf(typeof(TerrainData)))
			{
				// Does not contain any link to game objects. So we skip.
			}
			else if (type.IsSameOrSubclassOf(typeof(OffMeshLink)))
			{
				var referencedOffMeshLink = referencedObject as OffMeshLink;
				if (referencedOffMeshLink)
				{
					InternalAddReferencedGameObjectToResults(referencedOffMeshLink.gameObject, result, excludedTypes); // See 57182.
					InternalAddReferencedGameObjectToResults(referencedOffMeshLink.startTransform.gameObject, result, excludedTypes);
					InternalAddReferencedGameObjectToResults(referencedOffMeshLink.endTransform.gameObject, result, excludedTypes);
				}
			}
			else if (type.IsSameOrSubclassOf(typeof(UnityEvent)))
			{
				var unityEvent = (UnityEvent)referencedObject;
				var eventCount = unityEvent.GetPersistentEventCount();

				for (int i = 0; i < eventCount; i++)
				{
					var eventTarget = unityEvent.GetPersistentTarget(i);
					InternalAddReferencedObjectOfType(eventTarget, result, excludedTypes);
				}
			}
			else if (type.IsSameOrSubclassOf(typeof(Component)))
			{
				var referencedComponent = referencedObject as Component;
				if (referencedComponent)
				{
					// Component is an inseparable part of a GameObject. A reference to a
					// Component means it is also referencing the GameObject as a whole.
					// So we process the GameObject that has this Component. See 57182.
					var referencedGameObject = referencedComponent.gameObject;
					InternalAddReferencedGameObjectToResults(referencedGameObject, result, excludedTypes);
				}
			}
			else if (type.IsSameOrSubclassOf(typeof(GameObject)))
			{
				var referencedGameObject = referencedObject as GameObject;
				InternalAddReferencedGameObjectToResults(referencedGameObject, result, excludedTypes);
			}
			else if (
				// These types can't keep a reference to an object. So we skip.
				type.HasAttribute<SerializableAttribute>() && // Only interested in Serializable objects.
				!type.IsPrimitiveType() && // Primitive types can't keep a reference to an object in any way.
				!type.IsEnum // Enum types can't keep a reference to an object in any way.
			)
			{
				// If we encounter this log line, this means we encountered a type that has never been thought of before.
				// We should see if this class needs special care like Component, GameObject, UnityEvent above.
				// In most cases, the class is a user defined class and most of the time it's okay to define it as
				// a known type (See Known'TypesOfGameObjectReferenceFinder') and move on. But fore some rare types,
				// special care needs to be taken that we should define another 'if' case like above.
				//
				// The code below handles all serialized fields of this unknown object but if the object keeps a reference
				// to another object in a non-standard way, it's time to handle the object manually. See how Component,
				// GameObject and UnityEvent is handled in their own way and figure out how to get referenced objects
				// out of this unknown type likewise.
				if (!KnownTypesOfGameObjectReferenceFinder.Contains(type))
				{
					Debug.LogWarningFormat("Unknown object of type '{0}'. See the code for details.", type.FullName);
				}

				foreach (var serializedField in referencedObject.GetUnitySerializedFields())
				{
					var referencedObjectInObject = serializedField.GetValue(referencedObject);
					InternalAddReferencedObjectOfType(referencedObjectInObject, result, excludedTypes);
				}
			}
		}

		private static void InternalAddReferencedGameObjectToResults(GameObject referencedGameObject, HashSet<GameObject> result, Type[] excludedTypes)
		{
			if (referencedGameObject)
			{
				var isAdded = result.Add(referencedGameObject);
				// Check if the gameobject was added before, which means we have already processed the gameobject.
				// This will also prevent going into an infinite loop where there are circular references.
				if (isAdded)
				{
					referencedGameObject.FindAllReferencedGameObjectsInGameObject(result, excludedTypes);
				}
			}
		}

		private static bool CheckIfTypeExcluded(Type type, Type[] excludedTypes)
		{
			if (excludedTypes != null)
			{
				for (int i = 0; i < excludedTypes.Length; i++)
				{
					if (type.IsSameOrSubclassOf(excludedTypes[i]))
					{
						return true;
					}
				}
			}
			return false;
		}

		#endregion

		#region FindAllReferencedGameObjects... Unknown Type Ignore List

		private static HashSet<Type> _KnownTypesOfGameObjectReferenceFinder;
		/// <summary>
		/// For detailed explanation, see where it's used in ReflectionTools.cs.
		/// </summary>
		public static HashSet<Type> KnownTypesOfGameObjectReferenceFinder
		{
			get
			{
				if (_KnownTypesOfGameObjectReferenceFinder == null)
				{
					_KnownTypesOfGameObjectReferenceFinder = new HashSet<Type>(
						new[]
						{
							// Unity types
							typeof(AnimationCurve),
							typeof(Bounds),
							typeof(Color),
							typeof(Color32),
							typeof(LayerMask),
							typeof(Matrix4x4),
							typeof(Quaternion),
							typeof(Rect),
							typeof(RectOffset),
							//typeof(Texture), Commented out because let's not assume all derived classes should not have any link to an object
							typeof(Texture2D),
							typeof(Texture2DArray),
							typeof(Texture3D),
							typeof(Vector2),
							typeof(Vector3),
							typeof(Vector4),
							typeof(Vector2Int),
							typeof(Vector3Int),

							// Extenity types
							typeof(Bounds2),
							typeof(Bounds2Int),
							typeof(Bounds2IntRevised),
							typeof(ClampedInt),
							typeof(ClampedFloat),
							typeof(PathPoint),
							typeof(PIDConfiguration),
						}
					);
				}
				return _KnownTypesOfGameObjectReferenceFinder;
			}
		}

		#endregion

		#region Referenced Object Checks

		public static bool IsFieldReferencesUnityObject(this Object unityObject, FieldInfo fieldOfUnityObject, Object expectedUnityObject)
		{
			if (!expectedUnityObject)
				throw new ArgumentNullException(nameof(expectedUnityObject));

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
