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
			DelayedCalls = new List<TAction>(capacity);
		}

		#endregion

		#region Call List

		public List<TAction> DelayedCalls;

		public void AddDelayedCall(TAction method)
		{
			DelayedCalls.Add(method);
		}

		public void ClearAllDelayedCalls()
		{
			DelayedCalls.Clear();
		}

		#endregion

		#region Call

		public void CallAllDelayedCalls(Action<TAction> caller,  bool clearCallListAfterwards = true)
		{
			if (DelayedCalls.IsNullOrEmpty())
				return;

			foreach (var delayedCall in DelayedCalls)
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
