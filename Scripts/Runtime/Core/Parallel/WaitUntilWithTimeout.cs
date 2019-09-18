using System;
using UnityEngine;

namespace Extenity.ParallelToolbox
{

	/// <summary>
	///   <para>Suspends the coroutine execution until the supplied delegate evaluates to true OR the timeout passes.</para>
	///   <para>This is an extended implementation of <see cref="UnityEngine.WaitUntil"/>.</para>
	/// </summary>
	public class WaitUntilWithTimeout : CustomYieldInstruction
	{
		public float Timeout { get; set; }
		public Func<bool> Predicate { get; set; }

		private float StartTime;
		private bool Realtime;

		public WaitUntilWithTimeout(Func<bool> predicate, float timeout, bool realtime = true)
		{
			Predicate = predicate;
			Timeout = timeout;
			Realtime = realtime;
			StartTime = CurrentTime;
		}

		public override bool keepWaiting
		{
			get { return !Predicate() && CurrentTime < StartTime + Timeout; }
		}

		private float CurrentTime => Realtime ? Time.realtimeSinceStartup : Time.time;
	}

}
