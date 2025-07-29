#if UNITY_5_3_OR_NEWER

using System;
using UnityEngine;

namespace Extenity.ParallelToolbox
{

	public class WaitUntilResult
	{
		public bool IsTimedOut;
		public float PassedTime;

		public void Clear()
		{
			IsTimedOut = false;
			PassedTime = float.NaN;
		}
	}

	/// <summary>
	///   <para>Suspends the coroutine execution until the supplied delegate evaluates to true OR the timeout passes.</para>
	///   <para>This is an extended implementation of <see cref="UnityEngine.WaitUntil"/>.</para>
	/// </summary>
	public class WaitUntilWithTimeout : CustomYieldInstruction
	{
		public float Timeout { get; set; }
		public Func<bool> Predicate { get; set; }
		public WaitUntilResult Result { get; set; }

		private float StartTime;
		private bool Realtime;

		public WaitUntilWithTimeout(Func<bool> predicate, float timeout, bool realtime = true, WaitUntilResult result = null)
		{
			Predicate = predicate;
			Timeout = timeout;
			Realtime = realtime;
			Result = result;
			StartTime = CurrentTime;

			if (Result != null)
			{
				Result.Clear();
			}
		}

		public override bool keepWaiting
		{
			get
			{
				var passedTime = CurrentTime - StartTime;
				var isTimedOut = passedTime >= Timeout;
				var keepWaiting = !Predicate() && !isTimedOut;
				if (Result != null)
				{
					Result.IsTimedOut = isTimedOut;
					Result.PassedTime = passedTime;
				}
				return keepWaiting;
			}
		}

		private float CurrentTime => Realtime ? Time.realtimeSinceStartup : Time.time;
	}

}

#endif
