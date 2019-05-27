using System;
using System.Collections.Generic;
using Extenity.DataToolbox;

namespace Extenity.ParallelToolbox
{

	public class DelayedCaller<TAction>
	{
		#region Initialization

		public DelayedCaller(int capacity = 10)
		{
			delayedCalls = new List<TAction>(capacity);
		}

		#endregion

		#region Call List

		public List<TAction> delayedCalls;

		public void AddDelayedCall(TAction method)
		{
			delayedCalls.Add(method);
		}

		public void ClearAllDelayedCalls()
		{
			delayedCalls.Clear();
		}

		#endregion

		#region Call

		public void CallAllDelayedCalls(Action<TAction> caller,  bool clearCallListAfterwards = true)
		{
			if (delayedCalls.IsNullOrEmpty())
				return;

			foreach (var delayedCall in delayedCalls)
			{
				caller(delayedCall);
			}

			if (clearCallListAfterwards)
			{
				ClearAllDelayedCalls();
			}
		}

		#endregion
	}

}
