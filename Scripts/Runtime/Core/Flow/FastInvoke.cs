#if UNITY

using System;
using UnityEngine;

namespace Extenity.FlowToolbox
{

	public static class Invoker
	{
		#region Singleton Handler and Initialization

		public static FastInvokeHandler Handler;

		// static Invoker()
		// {
		// 	InitializeSystem();
		// }

		internal static void InitializeSystem()
		{
			DeinitializeSystem();

			Handler = new FastInvokeHandler();
		}

		internal static void DeinitializeSystem()
		{
			if (Handler != null)
			{
				Handler.Shutdown();
				Handler = null;
			}
		}

		#endregion

		#region Methods on Behaviour

		public static void FastInvoke(this Behaviour behaviour, Action action, double time, bool unscaledTime = false, bool overwriteExisting = false)
		{
			Handler.Invoke(behaviour, action, time, 0.0, unscaledTime, overwriteExisting);
		}

		public static void FastInvokeRepeating(this Behaviour behaviour, Action action, double repeatRate, bool unscaledTime = false, bool overwriteExisting = false)
		{
			Handler.Invoke(behaviour, action, repeatRate, repeatRate, unscaledTime, overwriteExisting);
		}

		public static void FastInvokeRepeating(this Behaviour behaviour, Action action, double initialDelay, double repeatRate, bool unscaledTime = false, bool overwriteExisting = false)
		{
			Handler.Invoke(behaviour, action, initialDelay, repeatRate, unscaledTime, overwriteExisting);
		}

		/// <summary>
		/// Cancels all awaiting invokes to specified action.
		/// </summary>
		public static void CancelFastInvoke(this Behaviour behaviour, Action action)
		{
			Handler.Cancel(behaviour, action);
		}

		/// <summary>
		/// Cancels all awaiting invokes to all actions.
		/// </summary>
		public static void CancelAllFastInvokes(this Behaviour behaviour)
		{
			Handler.CancelAll(behaviour);
		}

		public static void CancelCurrentFastInvoke(this Behaviour behaviour)
		{
			Handler.CancelCurrent(behaviour);
		}

		public static bool IsFastInvoking(this Behaviour behaviour, Action action)
		{
			return Handler.IsInvoking(behaviour, action);
		}

		public static bool IsFastInvoking(this Behaviour behaviour)
		{
			return Handler.IsInvoking(behaviour);
		}

		public static double RemainingTimeUntilNextFastInvoke(this Behaviour behaviour, Action action)
		{
			return Handler.RemainingTimeUntilNextInvoke(behaviour, action);
		}

		public static double RemainingTimeUntilNextFastInvoke(this Behaviour behaviour)
		{
			return Handler.RemainingTimeUntilNextInvoke(behaviour);
		}

		public static double RemainingTimeUntilNextUnscaledFastInvoke(this Behaviour behaviour, Action action)
		{
			return Handler.RemainingTimeUntilNextUnscaledInvoke(behaviour, action);
		}

		public static double RemainingTimeUntilNextUnscaledFastInvoke(this Behaviour behaviour)
		{
			return Handler.RemainingTimeUntilNextUnscaledInvoke(behaviour);
		}

		public static int FastInvokeCount(this Behaviour behaviour, Action action)
		{
			return Handler.InvokeCount(behaviour, action);
		}

		public static int FastInvokeCount(this Behaviour behaviour)
		{
			return Handler.InvokeCount(behaviour);
		}

		#endregion

		#region Stats

		public static bool IsFastInvokingAny()
		{
			return Handler.IsInvokingAny();
		}

		public static int TotalActiveFastInvokeCount()
		{
			return Handler.TotalActiveInvokeCount();
		}

		#endregion
	}

}

#endif
