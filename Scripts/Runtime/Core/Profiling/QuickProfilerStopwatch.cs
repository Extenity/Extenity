using Cysharp.Text;
using Extenity.DataToolbox;
using IDisposable = System.IDisposable;

namespace Extenity.ProfilingToolbox
{

	public struct QuickProfilerStopwatch : IDisposable
	{
		private ProfilerStopwatch Stopwatch;
		private readonly Logger Logger;
		private readonly string ProfilerTitle;
		private readonly float ThresholdDurationToConsiderLogging;
		private readonly LogSeverity LogSeverityAboveThreshold;
		private readonly LogSeverity LogSeverityBelowThreshold;
		private bool HasThresholdDuration => ThresholdDurationToConsiderLogging > 0f;

		public static QuickProfilerStopwatch WithLog(string profilerTitle, LogSeverity logSeverity = LogSeverity.Info)
		{
			return new(new Logger("Profiling"), profilerTitle, 0, logSeverity, LogSeverity.None);
		}

		public static QuickProfilerStopwatch WithLog(Logger logger, string profilerTitle, LogSeverity logSeverity = LogSeverity.Info)
		{
			return new(logger, profilerTitle, 0, logSeverity, LogSeverity.None);
		}

		public static QuickProfilerStopwatch WithThreshold(string profilerTitle, float thresholdDurationToConsiderLogging, LogSeverity logSeverityAboveThreshold = LogSeverity.Warning, LogSeverity logSeverityBelowThreshold = LogSeverity.None)
		{
			return new(new Logger("Profiling"), profilerTitle, thresholdDurationToConsiderLogging, logSeverityAboveThreshold, logSeverityBelowThreshold);
		}

		public static QuickProfilerStopwatch WithThreshold(Logger logger, string profilerTitle, float thresholdDurationToConsiderLogging, LogSeverity logSeverityAboveThreshold = LogSeverity.Warning, LogSeverity logSeverityBelowThreshold = LogSeverity.None)
		{
			return new(logger, profilerTitle, thresholdDurationToConsiderLogging, logSeverityAboveThreshold, logSeverityBelowThreshold);
		}

		private QuickProfilerStopwatch(Logger logger, string profilerTitle, float thresholdDurationToConsiderLogging, LogSeverity logSeverityAboveThreshold = LogSeverity.Warning, LogSeverity logSeverityBelowThreshold = LogSeverity.None)
		{
			Stopwatch = new ProfilerStopwatch();
			Logger = logger;
			ProfilerTitle = profilerTitle;
			ThresholdDurationToConsiderLogging = thresholdDurationToConsiderLogging;
			LogSeverityAboveThreshold = logSeverityAboveThreshold;
			LogSeverityBelowThreshold = logSeverityBelowThreshold;

			Stopwatch.Start();
		}

		public void Dispose()
		{
			Stopwatch.End();

			if (HasThresholdDuration)
			{
				if (Stopwatch.Elapsed > ThresholdDurationToConsiderLogging)
				{
					Logger.Any(LogSeverityAboveThreshold, ZString.Concat("Running '", ProfilerTitle, "' took '", Stopwatch.Elapsed.ToStringMinutesSecondsMicrosecondsFromSeconds(), "' which is longer than the expected '", ThresholdDurationToConsiderLogging, "' seconds"));
				}
				else
				{
					Logger.Any(LogSeverityBelowThreshold, ZString.Concat("Running '", ProfilerTitle, "' took '", Stopwatch.Elapsed.ToStringMinutesSecondsMicrosecondsFromSeconds(), "'"));
				}
			}
			else // No threshold duration specified, so we log everything.
			{
				Logger.Any(LogSeverityAboveThreshold, ZString.Concat("Running '", ProfilerTitle, "' took '", Stopwatch.Elapsed.ToStringMinutesSecondsMicrosecondsFromSeconds(), "'"));
			}
		}
	}

}
