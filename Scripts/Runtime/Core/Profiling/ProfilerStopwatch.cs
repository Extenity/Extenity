using Extenity.ApplicationToolbox;
using Extenity.DataToolbox;

// This is the way that Log system supports various Context types in different environments like
// both in Unity and in UniversalExtenity. Also don't add 'using UnityEngine' or 'using System'
// in this code file to prevent any possible confusions. Use 'using' selectively, like
// 'using Exception = System.Exception;'
// See 11746845.
#if UNITY
using ContextObject = UnityEngine.Object;
#else
using ContextObject = System.Object;
#endif

namespace Extenity.ProfilingToolbox
{

	public struct ProfilerStopwatch
	{
		public double StartTime { get; private set; }
		public double EndTime { get; private set; }

		public bool IsStarted => EndTime == -1;

		public double Elapsed
		{
			get
			{
				if (IsStarted)
				{
					return CurrentTime - StartTime;
				}
				return EndTime - StartTime;
			}
		}

		public double CurrentTime
		{
			get { return PrecisionTiming.PreciseTime; }
		}

		public void Start()
		{
			if (IsStarted)
			{
				Log.Error("Tried to start profiler stopwatch but it was already started.");
				return;
			}

			EndTime = -1;
			
			// Note that CurrentTime is called at the very end of Start,
			// without doing any other work to allow precise measurements between Start and End.
			StartTime = CurrentTime;
		}

		/// <summary>
		/// Stops the stopwatch and returns elapsed time.
		/// </summary>
		public double End()
		{
			// Note that CurrentTime is called as soon as possible in the first line of End,
			// without doing any other work to allow precise measurements between Start and End.
			var endTime = CurrentTime; 

			if (!IsStarted)
			{
				StartTime = 0;
				EndTime = 0;
				Log.Error("Tried to end profiler stopwatch but it was not started.");
				return 0;
			}

			// Note that EndTime is also used in IsStarted. So we have to set its value after the check above.
			EndTime = endTime;
			var elapsed = Elapsed;

			TotalCalls++;
			CumulativeTime += elapsed;
			return elapsed;
		}

		/// <summary>
		/// Stops the stopwatch if running and starts again. Returns elapsed time.
		/// </summary>
		public double Restart()
		{
			double elapsed;
			if (IsStarted)
			{
				elapsed = End();
			}
			else
			{
				elapsed = 0.0;
			}
			Start();
			return elapsed;
		}

		#region Log

		private static readonly Logger Log = new(nameof(ProfilerStopwatch));

		public string GetLogMessage(string profilerMessageFormat)
		{
			return string.Format(profilerMessageFormat, Elapsed.ToStringMinutesSecondsMillisecondsFromSeconds());
		}

		public string GetCumulativeLogMessage(string profilerMessageFormat)
		{
			return string.Format(profilerMessageFormat, CumulativeTime.ToStringMinutesSecondsMillisecondsFromSeconds());
		}

		#endregion

		#region Cumulative Time

		public int TotalCalls { get; private set; }
		public double CumulativeTime { get; private set; }

		public void ResetCumulativeTime()
		{
			TotalCalls = 0;
			CumulativeTime = 0;
		}

		#endregion
	}

}
