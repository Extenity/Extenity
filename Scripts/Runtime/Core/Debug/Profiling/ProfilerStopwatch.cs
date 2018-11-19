using Extenity.ApplicationToolbox;
using Extenity.DataToolbox;
using Object = UnityEngine.Object;

namespace Extenity.DebugToolbox
{

	public class ProfilerStopwatch
	{
		public double StartTime { get; private set; }
		public bool IsStarted { get; private set; }

		public double EndTime { get; private set; }

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
			IsStarted = true;

			StartTime = CurrentTime;
			EndTime = 0;
		}

		/// <summary>
		/// Stops the stopwatch and returns elapsed time.
		/// </summary>
		public double End()
		{
			EndTime = CurrentTime;

			if (!IsStarted)
			{
				StartTime = 0;
				EndTime = 0;
				Log.Error("Tried to end profiler stopwatch but it was not started.");
				return 0;
			}

			var elapsed = Elapsed;

			TotalCalls++;
			CumulativeTime += elapsed;
			IsStarted = false;
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

		public void EndAndLog(string profilerMessageFormat)
		{
			End();
			LogInfo(profilerMessageFormat);
		}

		public void EndAndLog(Object context, string profilerMessageFormat)
		{
			End();
			LogInfo(context, profilerMessageFormat);
		}

		#region Log

		public void LogInfo(string profilerMessageFormat)
		{
			Log.Info(string.Format(profilerMessageFormat, Elapsed.ToStringMinutesSecondsMillisecondsFromSeconds()));
		}

		public void LogInfo(Object context, string profilerMessageFormat)
		{
			Log.Info(string.Format(profilerMessageFormat, Elapsed.ToStringMinutesSecondsMillisecondsFromSeconds()), context);
		}

		public void LogInfoCumulative(string profilerMessageFormat)
		{
			Log.Info(string.Format(profilerMessageFormat, CumulativeTime.ToStringMinutesSecondsMillisecondsFromSeconds()));
		}

		public void LogInfoCumulative(Object context, string profilerMessageFormat)
		{
			Log.Info(string.Format(profilerMessageFormat, CumulativeTime.ToStringMinutesSecondsMillisecondsFromSeconds()), context);
		}

		#endregion

		#region Cumulative Time

		public int TotalCalls;
		public double CumulativeTime { get; private set; }

		public void ResetCumulativeTime()
		{
			TotalCalls = 0;
			CumulativeTime = 0;
		}

		#endregion
	}

}
