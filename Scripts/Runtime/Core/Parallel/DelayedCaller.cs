﻿using System;
using System.Collections.Generic;
using Extenity.DataToolbox;

namespace Extenity.ParallelToolbox
{

	public class DelayedCaller
	{
		#region Initialization

		public DelayedCaller(int capacity = 10)
		{
			delayedCalls = new List<Action>(capacity);
		}

		#endregion

		#region Call List

		public List<Action> delayedCalls;

		public void AddDelayedCall(Action method)
		{
			delayedCalls.Add(method);
		}

		public void ClearAllDelayedCalls()
		{
			delayedCalls.Clear();
		}

		#endregion

		#region Call

		public void CallAllDelayedCalls(bool clearCallListAfterwards = true)
		{
			if (delayedCalls.IsNullOrEmpty())
				return;

			foreach (var delayedCall in delayedCalls)
			{
				delayedCall();
			}

			if (clearCallListAfterwards)
			{
				ClearAllDelayedCalls();
			}
		}

		#endregion
	}

}