using System;

namespace Extenity.SystemToolbox
{

	public static class SystemTools
	{
		#region Action InvokeSafe

		public static bool InvokeSafe(this Action action)
		{
			if (action != null)
			{
				try // Safety belt in case something goes wrong in callback.
				{
					action();
					return true;
				}
				catch (Exception exception)
				{
					LogInvoke.Error(exception);
				}
			}
			return false;
		}

		public static bool InvokeSafe<T1>(this Action<T1> action, T1 arg1)
		{
			if (action != null)
			{
				try // Safety belt in case something goes wrong in callback.
				{
					action(arg1);
					return true;
				}
				catch (Exception exception)
				{
					LogInvoke.Error(exception);
				}
			}
			return false;
		}

		public static bool InvokeSafe<T1, T2>(this Action<T1, T2> action, T1 arg1, T2 arg2)
		{
			if (action != null)
			{
				try // Safety belt in case something goes wrong in callback.
				{
					action(arg1, arg2);
					return true;
				}
				catch (Exception exception)
				{
					LogInvoke.Error(exception);
				}
			}
			return false;
		}

		public static bool InvokeSafe<T1, T2, T3>(this Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3)
		{
			if (action != null)
			{
				try // Safety belt in case something goes wrong in callback.
				{
					action(arg1, arg2, arg3);
					return true;
				}
				catch (Exception exception)
				{
					LogInvoke.Error(exception);
				}
			}
			return false;
		}

		public static bool InvokeSafe<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			if (action != null)
			{
				try // Safety belt in case something goes wrong in callback.
				{
					action(arg1, arg2, arg3, arg4);
					return true;
				}
				catch (Exception exception)
				{
					LogInvoke.Error(exception);
				}
			}
			return false;
		}

		public static bool InvokeSafe<T1, T2, T3, T4, T5>(this Action<T1, T2, T3, T4, T5> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
		{
			if (action != null)
			{
				try // Safety belt in case something goes wrong in callback.
				{
					action(arg1, arg2, arg3, arg4, arg5);
					return true;
				}
				catch (Exception exception)
				{
					LogInvoke.Error(exception);
				}
			}
			return false;
		}

		public static bool InvokeSafe<T1, T2, T3, T4, T5, T6>(this Action<T1, T2, T3, T4, T5, T6> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
		{
			if (action != null)
			{
				try // Safety belt in case something goes wrong in callback.
				{
					action(arg1, arg2, arg3, arg4, arg5, arg6);
					return true;
				}
				catch (Exception exception)
				{
					LogInvoke.Error(exception);
				}
			}
			return false;
		}

		public static bool InvokeSafe<T1, T2, T3, T4, T5, T6, T7>(this Action<T1, T2, T3, T4, T5, T6, T7> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
		{
			if (action != null)
			{
				try // Safety belt in case something goes wrong in callback.
				{
					action(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
					return true;
				}
				catch (Exception exception)
				{
					LogInvoke.Error(exception);
				}
			}
			return false;
		}

		public static bool InvokeSafe<T1, T2, T3, T4, T5, T6, T7, T8>(this Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
		{
			if (action != null)
			{
				try // Safety belt in case something goes wrong in callback.
				{
					action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
					return true;
				}
				catch (Exception exception)
				{
					LogInvoke.Error(exception);
				}
			}
			return false;
		}

		public static bool InvokeSafe<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
		{
			if (action != null)
			{
				try // Safety belt in case something goes wrong in callback.
				{
					action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
					return true;
				}
				catch (Exception exception)
				{
					LogInvoke.Error(exception);
				}
			}
			return false;
		}

		#endregion

		#region Func InvokeSafe

		public static bool InvokeSafe<TResult>(this Func<TResult> action, out TResult result)
		{
			if (action != null)
			{
				try // Safety belt in case something goes wrong in callback.
				{
					result = action();
					return true;
				}
				catch (Exception exception)
				{
					LogInvoke.Error(exception);
				}
			}
			result = default(TResult);
			return false;
		}

		public static bool InvokeSafe<T1, TResult>(this Func<T1, TResult> action, T1 arg1, out TResult result)
		{
			if (action != null)
			{
				try // Safety belt in case something goes wrong in callback.
				{
					result = action(arg1);
					return true;
				}
				catch (Exception exception)
				{
					LogInvoke.Error(exception);
				}
			}
			result = default(TResult);
			return false;
		}

		public static bool InvokeSafe<T1, T2, TResult>(this Func<T1, T2, TResult> action, T1 arg1, T2 arg2, out TResult result)
		{
			if (action != null)
			{
				try // Safety belt in case something goes wrong in callback.
				{
					result = action(arg1, arg2);
					return true;
				}
				catch (Exception exception)
				{
					LogInvoke.Error(exception);
				}
			}
			result = default(TResult);
			return false;
		}

		public static bool InvokeSafe<T1, T2, T3, TResult>(this Func<T1, T2, T3, TResult> action, T1 arg1, T2 arg2, T3 arg3, out TResult result)
		{
			if (action != null)
			{
				try // Safety belt in case something goes wrong in callback.
				{
					result = action(arg1, arg2, arg3);
					return true;
				}
				catch (Exception exception)
				{
					LogInvoke.Error(exception);
				}
			}
			result = default(TResult);
			return false;
		}

		public static bool InvokeSafe<T1, T2, T3, T4, TResult>(this Func<T1, T2, T3, T4, TResult> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, out TResult result)
		{
			if (action != null)
			{
				try // Safety belt in case something goes wrong in callback.
				{
					result = action(arg1, arg2, arg3, arg4);
					return true;
				}
				catch (Exception exception)
				{
					LogInvoke.Error(exception);
				}
			}
			result = default(TResult);
			return false;
		}

		public static bool InvokeSafe<T1, T2, T3, T4, T5, TResult>(this Func<T1, T2, T3, T4, T5, TResult> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, out TResult result)
		{
			if (action != null)
			{
				try // Safety belt in case something goes wrong in callback.
				{
					result = action(arg1, arg2, arg3, arg4, arg5);
					return true;
				}
				catch (Exception exception)
				{
					LogInvoke.Error(exception);
				}
			}
			result = default(TResult);
			return false;
		}

		public static bool InvokeSafe<T1, T2, T3, T4, T5, T6, TResult>(this Func<T1, T2, T3, T4, T5, T6, TResult> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, out TResult result)
		{
			if (action != null)
			{
				try // Safety belt in case something goes wrong in callback.
				{
					result = action(arg1, arg2, arg3, arg4, arg5, arg6);
					return true;
				}
				catch (Exception exception)
				{
					LogInvoke.Error(exception);
				}
			}
			result = default(TResult);
			return false;
		}

		public static bool InvokeSafe<T1, T2, T3, T4, T5, T6, T7, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, TResult> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, out TResult result)
		{
			if (action != null)
			{
				try // Safety belt in case something goes wrong in callback.
				{
					result = action(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
					return true;
				}
				catch (Exception exception)
				{
					LogInvoke.Error(exception);
				}
			}
			result = default(TResult);
			return false;
		}

		public static bool InvokeSafe<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, out TResult result)
		{
			if (action != null)
			{
				try // Safety belt in case something goes wrong in callback.
				{
					result = action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
					return true;
				}
				catch (Exception exception)
				{
					LogInvoke.Error(exception);
				}
			}
			result = default(TResult);
			return false;
		}

		public static bool InvokeSafe<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(this Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, out TResult result)
		{
			if (action != null)
			{
				try // Safety belt in case something goes wrong in callback.
				{
					result = action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
					return true;
				}
				catch (Exception exception)
				{
					LogInvoke.Error(exception);
				}
			}
			result = default(TResult);
			return false;
		}

		#endregion

		#region Unity Object Null Check

#if UNITY

		public static bool IsNullRespectingUnityObject(this object obj)
		{
			if (obj is UnityEngine.Object cast)
			{
				return !cast;
			}
			return obj == null;
		}

		public static bool IsNotNullRespectingUnityObject(this object obj)
		{
			if (obj is UnityEngine.Object cast)
			{
				return cast;
			}
			return obj != null;
		}

#endif

		#endregion

		#region Log

		private static readonly Logger LogInvoke = new("Invoke");

		#endregion
	}

}
