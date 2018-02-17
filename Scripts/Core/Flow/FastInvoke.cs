using System;
using UnityEngine;

namespace Extenity.FlowToolbox
{

	public static class Invoker
	{
		#region Singleton Handler

		private static readonly FastInvokeHandler Handler;

		static Invoker()
		{
			var go = new GameObject("_FastInvokeHandler");
			GameObject.DontDestroyOnLoad(go);
			go.hideFlags = HideFlags.HideInHierarchy;
			Handler = go.AddComponent<FastInvokeHandler>();
		}

		#endregion

		#region Methods on Behaviour

		public static void FastInvoke(this Behaviour behaviour, Action action, double time)
		{
			Handler.Launch(behaviour, action, time, 0f);
		}

		public static void FastInvokeRepeating(this Behaviour behaviour, Action action, double repeatRate)
		{
			Handler.Launch(behaviour, action, repeatRate, repeatRate);
		}

		public static void FastInvokeRepeating(this Behaviour behaviour, Action action, double initialDelay, double repeatRate)
		{
			Handler.Launch(behaviour, action, initialDelay, repeatRate);
		}

		public static void CancelFastInvoke(this Behaviour behaviour, Action action)
		{
			Handler.Cancel(behaviour, action);
		}

		public static void CancelAllFastInvokes(this Behaviour behaviour)
		{
			Handler.CancelAll(behaviour);
		}

		public static bool IsFastInvoking(this Behaviour behaviour, Action action)
		{
			return Handler.IsInvoking(behaviour, action);
		}

		public static bool IsFastInvoking(this Behaviour behaviour)
		{
			return Handler.IsInvoking(behaviour);
		}

		#endregion

		#region Static Methods

		public static bool IsFastInvokingAny()
		{
			return Handler.IsInvokingAny();
		}

		#endregion
	}

}
