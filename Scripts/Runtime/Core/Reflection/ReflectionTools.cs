#if !NET_STANDARD_2_0
#define ListArrayAccessorAvailable
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Extenity.DataToolbox;
using Extenity.GameObjectToolbox;
using Extenity.MathToolbox;
using Extenity.SystemToolbox;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
#if ListArrayAccessorAvailable
using System.Reflection.Emit;
#endif

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

		public static bool IsGetter(this MethodInfo method)
		{
			return method.IsSpecialName &&
			       method.Name.StartsWith("get_");
		}

		public static bool IsSetter(this MethodInfo method)
		{
			return method.IsSpecialName &&
			       method.Name.StartsWith("set_");
		}

		public static bool IsGetterOrSetter(this MethodInfo method)
		{
			return method.IsSpecialName &&
			       (method.Name.StartsWith("get_") || method.Name.StartsWith("set_"));
		}

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

		public static object CallMethodOfTypeByName(string assemblyQualifiedTypeName, string methodName, BindingFlags bindingFlags, object instance, object[] parameters)
		{
			var type = Type.GetType(assemblyQualifiedTypeName);
			if (type == null)
				throw new Exception($"Type '{assemblyQualifiedTypeName}' not found.");

			var method = type.GetMethod(methodName, bindingFlags);
			if (method == null)
				throw new Exception($"Method '{methodName}' of type '{type}' not found.");

			return method.Invoke(instance, parameters);
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

		#region FindType

		public static Type GetTypeInAllAssembliesEnsured(StringFilter typeFullNameFilter)
		{
			var result = SearchTypeInAllAssemblies(typeFullNameFilter);
			if (result.Length > 1)
			{
				throw new Exception("Found more than one type:\n" + string.Join("\n", result.Select(entry => entry.FullName)));
			}
			if (result.Length == 0)
			{
				throw new Exception("Failed to find a type.");
			}
			return result[0];
		}

		public static Type[] SearchTypeInAllAssemblies(StringFilter typeFullNameFilter)
		{
			return (from assembly in AppDomain.CurrentDomain.GetAssemblies()
			        from module in assembly.GetModules()
			        from type in module.GetTypes()
			        where typeFullNameFilter.IsMatching(type.FullName)
			        select type).ToArray();
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
		/// 	Log.Info(indexOfFunc("Hello", 'e'));
		/// 	Log.Info(getByteCountFunc(Encoding.UTF8, "Euro sign: u20ac"));
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

		#region IsOverride

		/// <summary>
		/// Tells if the method is an override, that overrides the method in its base class.
		/// Source: https://stackoverflow.com/questions/2932421/detect-if-a-method-was-overridden-using-reflection-c
		/// </summary>
		public static bool IsOverride(this MethodInfo methodInfo)
		{
			if (methodInfo == null)
				throw new ArgumentNullException();
			return methodInfo.GetBaseDefinition().DeclaringType != methodInfo.DeclaringType;
		}

		public static bool IsMethodOverriden(this object me, string methodName, Type[] methodParameters = default)
		{
			if (methodParameters == null)
			{
				methodParameters = new Type[0];
			}

			var methodInfo = me.GetType().GetMethod(methodName,
			                                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance,
			                                        null, CallingConventions.Any, methodParameters, null);

			if (methodInfo == null)
			{
				throw new Exception($"No method named '{methodName}' with specified parameters found in derived classes.");
			}

			return methodInfo.IsOverride();
		}

		#endregion

		#region SizeOf<T> Enhanced

		private struct TypeSizeProxy<T>
		{
#pragma warning disable 649
			public T PublicField;
#pragma warning restore 649
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

		#region IsAutoProperty

		/// <summary>
		/// Source: https://stackoverflow.com/questions/2210309/how-to-find-out-if-a-property-is-an-auto-implemented-property-with-reflection
		/// </summary>
		public static bool IsAutoProperty(this PropertyInfo prop)
		{
			if (!prop.CanWrite || !prop.CanRead)
				return false;

			return prop.DeclaringType
			           .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
			           .Any(f => f.Name.Contains("<" + prop.Name + ">"));
		}

		#endregion

		#region Get Unity-Serialized Fields

		public static List<(FieldInfo FieldInfo, object Value)> GetUnitySerializedFieldsAndValues(this object obj, bool includeOnlyNonNullFields)
		{
			var fields = obj.GetUnitySerializedFields();
			var result = New.List<(FieldInfo FieldInfo, object Value)>(fields.Count);
			if (includeOnlyNonNullFields)
			{
				foreach (var field in fields)
				{
					var value = field.GetValue(obj);
					if (value.IsNotNullRespectingUnityObject())
					{
						result.Add((field, value));
					}
				}
			}
			else
			{
				foreach (var field in fields)
				{
					var value = field.GetValue(obj);
					result.Add((field, value));
				}
			}
			return result;
		}

		public static List<FieldInfo> GetUnitySerializedFields(this object obj)
		{
			if (obj == null)
				throw new ArgumentNullException(nameof(obj));

			if (obj is GameObject)
			{
				// This is an intentionally placed trap. Getting fields of a GameObject is undefined behaviour.
				// This method designed to be simple with no fancy features. So it does not try to iterate over
				// Components of GameObject.
				//
				// If you see this exception, you probably needed to get fields of all Components of that GameObject.
				// If so, iterate all components manually and call this method for each of them OR use other
				// variations of this method.
				throw new InternalException(11934857);
			}

			var type = obj as Type;
			if (type != null)
			{
				throw new InternalException(11723659); // Encountered an unexpected behaviour. Developer attention is needed. See below.
			}
			type = obj.GetType();
			// This was the old implementation, which I'm not sure if it was a faulty copy-paste mistake as some
			// similar usages exist in some other methods. There is a chance it really means something.
			// If you encounter, figure out what should be done. Trying to get serialized fields of a Type
			// seems to be a dull operation, but maybe I'm wrong.
			//
			// Delete the lines after a year if we don't encounter the error above.
			//if (type == null)
			//	type = obj.GetType();

			var fields = new List<FieldInfo>();
			var allFields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

			for (int i = 0; i < allFields.Length; i++)
			{
				var field = allFields[i];
				var nonSerializedAttributeDefined = Attribute.IsDefined(field, typeof(NonSerializedAttribute));
				var serializeFieldAttributeDefined = Attribute.IsDefined(field, typeof(SerializeField));
				if (nonSerializedAttributeDefined && serializeFieldAttributeDefined)
				{
					throw new Exception($"Don't know what to do about field '{field.Name}' that has both 'NonSerialized' and 'SerializeField' attributes.");
				}
				if (!nonSerializedAttributeDefined && // See if the field is specified as not serialized on purpose
				    !field.IsReadOnly() && // Unity won't serialize readonly fields
				    !field.FieldType.IsDictionary() && // Unity won't serialize Dictionary fields
				    (field.IsPublic || serializeFieldAttributeDefined) // See if the field is meant to be serialized
				)
				{
					fields.Add(field);
				}
			}

			return fields;
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
#if !DisableUnityAudio
			if (type.IsSameOrSubclassOf(typeof(UnityEngine.AudioSource)))
			{
				var referencedAudioSource = referencedObject as UnityEngine.AudioSource;
				if (referencedAudioSource)
				{
					InternalAddReferencedGameObjectToResults(referencedAudioSource.gameObject, result, excludedTypes); // See 57182.
					InternalAddReferencedObjectOfType(referencedAudioSource.clip, result, excludedTypes);
					InternalAddReferencedObjectOfType(referencedAudioSource.outputAudioMixerGroup, result, excludedTypes);
				}
			}
			else if (type.IsSameOrSubclassOf(typeof(UnityEngine.AudioClip)))
			{
				// Does not contain any link to game objects. So we skip.
			}
			else if (type.IsSameOrSubclassOf(typeof(UnityEngine.Audio.AudioMixer)))
			{
				var referencedAudioMixer = referencedObject as UnityEngine.Audio.AudioMixer;
				if (referencedAudioMixer)
				{
					InternalAddReferencedObjectOfType(referencedAudioMixer.outputAudioMixerGroup, result, excludedTypes);
				}
			}
			else if (type.IsSameOrSubclassOf(typeof(UnityEngine.Audio.AudioMixerGroup)))
			{
				var referencedAudioMixerGroup = referencedObject as UnityEngine.Audio.AudioMixerGroup;
				if (referencedAudioMixerGroup)
				{
					// Just as the Component is an inseparable part of a GameObject, an AudioMixerGroup
					// is a part of AudioMixer. So we include the AudioMixer too.
					InternalAddReferencedObjectOfType(referencedAudioMixerGroup.audioMixer, result, excludedTypes);
				}
			}
			else
#endif
			if (type.IsSameOrSubclassOf(typeof(Animator)))
			{
				var referencedAnimator = referencedObject as Animator;
				if (referencedAnimator)
				{
					// An Animator may use it's children without keeping any references to them.
					// So we need to assume they are referenced.
					InternalAddReferencedGameObjectToResults(referencedAnimator.gameObject, result, excludedTypes); // See 57182.
					var children = New.List<GameObject>();
					referencedAnimator.gameObject.ListAllChildrenGameObjects(children, false);
					foreach (var childGO in children)
					{
						InternalAddReferencedGameObjectToResults(childGO, result, excludedTypes);
					}
					Release.List(ref children);
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
#if !DisableUnityTerrain
			else if (type.IsSameOrSubclassOf(typeof(TerrainData)))
			{
				// Does not contain any link to game objects. So we skip.
			}
#endif
#if !DisableUnityAI
			else if (type.IsSameOrSubclassOf(typeof(UnityEngine.AI.OffMeshLink)))
			{
				var referencedOffMeshLink = referencedObject as UnityEngine.AI.OffMeshLink;
				if (referencedOffMeshLink)
				{
					InternalAddReferencedGameObjectToResults(referencedOffMeshLink.gameObject, result, excludedTypes); // See 57182.
					InternalAddReferencedGameObjectToResults(referencedOffMeshLink.startTransform.gameObject, result, excludedTypes);
					InternalAddReferencedGameObjectToResults(referencedOffMeshLink.endTransform.gameObject, result, excludedTypes);
				}
			}
#endif
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
					Log.Warning($"Unknown object of type '{type.FullName}'. See the code for details.");
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
#if UNITY_EDITOR
							typeof(Line.DebugConfigurationData),
							typeof(Spline.DebugConfigurationData),
							typeof(OrientedLine.DebugConfigurationData),
							//typeof(OrientedSpline.DebugConfigurationData), Uncomment when implemented
#endif
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

		#region ToStringDetails

		public static void ToStringDetails(this Assembly assembly, StringBuilder stringBuilder)
		{
			var types = assembly.GetTypes();
			foreach (var type in types)
			{
				stringBuilder.Append("Type: ");
				stringBuilder.Append(type.FullName);
				stringBuilder.AppendLine();
				type.ToStringDetails(stringBuilder);
			}
		}

		public static void ToStringDetails(this Type type, StringBuilder stringBuilder)
		{
			const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static |
			                                  BindingFlags.Public | BindingFlags.NonPublic |
			                                  BindingFlags.DeclaredOnly;
			var fields = type.GetFields(bindingFlags);
			var properties = type.GetProperties(bindingFlags);
			var methods = type.GetMethods(bindingFlags);

			for (var i = 0; i < fields.Length; i++)
			{
				stringBuilder.Append("F ");
				fields[i].ToStringDetails(stringBuilder);
				stringBuilder.AppendLine();
			}

			for (var i = 0; i < properties.Length; i++)
			{
				var getMethod = properties[i].GetMethod;
				if (getMethod != null)
				{
					stringBuilder.Append("P ");
					getMethod.ToStringDetails(stringBuilder);
					stringBuilder.AppendLine();
				}
				var setMethod = properties[i].SetMethod;
				if (setMethod != null)
				{
					stringBuilder.Append("P ");
					setMethod.ToStringDetails(stringBuilder);
					stringBuilder.AppendLine();
				}
			}

			for (var i = 0; i < methods.Length; i++)
			{
				if (!methods[i].IsGetterOrSetter())
				{
					stringBuilder.Append("M ");
					methods[i].ToStringDetails(stringBuilder);
					stringBuilder.AppendLine();
				}
			}
		}

		public static void ToStringDetails(this MethodInfo method, StringBuilder stringBuilder)
		{
			if (method == null)
			{
				stringBuilder.Append("[NA]");
				return;
			}
			if (method.IsPublic)
				stringBuilder.Append("public ");
			if (method.IsPrivate)
				stringBuilder.Append("private ");
			if (method.IsFamily)
				stringBuilder.Append("protected ");
			if (method.IsAbstract)
				stringBuilder.Append("abstract ");
			if (method.IsVirtual)
				stringBuilder.Append("virtual ");
			if (method.IsFinal)
				stringBuilder.Append("final ");
			if (method.IsStatic)
				stringBuilder.Append("static ");
			if (method.IsConstructor)
				stringBuilder.Append("constructor ");
			if (method.IsGenericMethodDefinition)
				stringBuilder.Append("definition ");

			method.ReturnParameter.ToStringDetails(stringBuilder);
			stringBuilder.Append(" ");

			stringBuilder.Append(method.Name);

			if (method.IsGenericMethod)
			{
				method.ToStringGenericArguments(stringBuilder);
			}

			stringBuilder.Append("(");
			method.ToStringParameters(stringBuilder);
			stringBuilder.Append(")");
		}

		public static void ToStringGenericArguments(this MethodInfo method, StringBuilder stringBuilder)
		{
			var genericArguments = method.GetGenericArguments();
			stringBuilder.Append("<");
			if (genericArguments.Length > 0)
			{
				stringBuilder.Append(genericArguments[0].Name);
				for (var i = 1; i < genericArguments.Length; i++)
				{
					stringBuilder.Append(",");
					stringBuilder.Append(genericArguments[i].Name);
				}
			}
			stringBuilder.Append(">");
		}

		public static void ToStringParameters(this MethodInfo method, StringBuilder stringBuilder)
		{
			var parameters = method.GetParameters();
			if (parameters.Length > 0)
			{
				parameters[0].ToStringDetails(stringBuilder);
				for (int i = 1; i < parameters.Length; i++)
				{
					stringBuilder.Append(", ");
					parameters[i].ToStringDetails(stringBuilder);
				}
			}
		}

		public static void ToStringDetails(this ParameterInfo parameter, StringBuilder stringBuilder)
		{
			// parameter.IsRetval
			if (parameter.IsIn)
				stringBuilder.Append("in ");
			if (parameter.IsOut)
				stringBuilder.Append("out ");
			stringBuilder.Append(parameter.ParameterType);
			if (!string.IsNullOrWhiteSpace(parameter.Name))
			{
				stringBuilder.Append(" ");
				stringBuilder.Append(parameter.Name);
			}
		}

		public static void ToStringDetails(this FieldInfo field, StringBuilder stringBuilder)
		{
			if (field == null)
			{
				stringBuilder.Append("[NA]");
				return;
			}
			if (field.IsPublic)
				stringBuilder.Append("public ");
			if (field.IsPrivate)
				stringBuilder.Append("private ");
			if (field.IsFamily)
				stringBuilder.Append("protected ");
			if (field.IsLiteral)
				stringBuilder.Append("const ");
			if (field.IsStatic)
				stringBuilder.Append("static ");
			if (field.IsInitOnly)
				stringBuilder.Append("readonly ");
			// TODO: See if FieldType.Name covers Tuples.
			stringBuilder.Append(field.FieldType.Name + " " + field.Name);
		}

		#endregion

		#region List.GetInternalArray

#if ListArrayAccessorAvailable
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

#endif

		#endregion
	}

}
