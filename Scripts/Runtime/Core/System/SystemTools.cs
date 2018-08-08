using System;
using UnityEngine;

namespace Extenity.SystemToolbox
{

	public static class SystemTools
	{
		#region Action InvokeSafe

		public static void InvokeSafe(this Action action)
		{
			if (action != null)
			{
				try // Safety belt in case something goes wrong in callback.
				{
					action();
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			}
		}

		public static void InvokeSafe<T1>(this Action<T1> action, T1 arg1)
		{
			if (action != null)
			{
				try // Safety belt in case something goes wrong in callback.
				{
					action(arg1);
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			}
		}

		public static void InvokeSafe<T1, T2>(this Action<T1, T2> action, T1 arg1, T2 arg2)
		{
			if (action != null)
			{
				try // Safety belt in case something goes wrong in callback.
				{
					action(arg1, arg2);
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			}
		}

		public static void InvokeSafe<T1, T2, T3>(this Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3)
		{
			if (action != null)
			{
				try // Safety belt in case something goes wrong in callback.
				{
					action(arg1, arg2, arg3);
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			}
		}

		public static void InvokeSafe<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			if (action != null)
			{
				try // Safety belt in case something goes wrong in callback.
				{
					action(arg1, arg2, arg3, arg4);
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			}
		}

		public static void InvokeSafe<T1, T2, T3, T4, T5>(this Action<T1, T2, T3, T4, T5> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
		{
			if (action != null)
			{
				try // Safety belt in case something goes wrong in callback.
				{
					action(arg1, arg2, arg3, arg4, arg5);
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			}
		}

		#endregion
	}

}
