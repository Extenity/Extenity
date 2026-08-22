using Extenity.ApplicationToolbox;
using Extenity.DataToolbox;

namespace Extenity.ProfilingToolbox
{
	public readonly struct ProfilerStopwatch
	{
		public readonly double StartTime;

		public double ElapsedSecondsDouble => PrecisionTiming.PreciseTime - StartTime;
		public float ElapsedSeconds => (float)(PrecisionTiming.PreciseTime - StartTime);
		public float ElapsedMilliseconds => 0.001f * (float)(PrecisionTiming.PreciseTime - StartTime);

		#region Initialization

		public static ProfilerStopwatch Start()
		{
			return new ProfilerStopwatch(PrecisionTiming.PreciseTime);
		}

		public static void GetSecondsAndRestart(ref ProfilerStopwatch reference, out float currentElapsedSeconds)
		{
			currentElapsedSeconds = reference.ElapsedSeconds;
			reference = Start();
		}

		public static void GetMillisecondsAndRestart(ref ProfilerStopwatch reference, out float currentElapsedMilliseconds)
		{
			currentElapsedMilliseconds = reference.ElapsedMilliseconds;
			reference = Start();
		}

		private ProfilerStopwatch(double startTime)
		{
			StartTime = startTime;
		}

		#endregion

		#region Log

		public string GetLogMessage(string profilerMessageFormat)
		{
			return string.Format(profilerMessageFormat, ElapsedSecondsDouble.ToStringMinutesSecondsMillisecondsFromSeconds());
		}

		#endregion
	}
}