using System.Collections.Generic;
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
		private readonly int CapLogCountAt;
		private bool HasThresholdDuration => ThresholdDurationToConsiderLogging > 0f;

		public static QuickProfilerStopwatch WithLog(string profilerTitle, LogSeverity logSeverity = LogSeverity.Info, int capLogCountAt = 0)
		{
			return new(new Logger("Profiling"), profilerTitle, 0, logSeverity, LogSeverity.None, capLogCountAt);
		}

		public static QuickProfilerStopwatch WithLog(Logger logger, string profilerTitle, LogSeverity logSeverity = LogSeverity.Info, int capLogCountAt = 0)
		{
			return new(logger, profilerTitle, 0, logSeverity, LogSeverity.None, capLogCountAt);
		}

		public static QuickProfilerStopwatch WithThreshold(string profilerTitle, float thresholdDurationToConsiderLogging, LogSeverity logSeverityAboveThreshold = LogSeverity.Warning, LogSeverity logSeverityBelowThreshold = LogSeverity.None, int capLogCountAt = 0)
		{
			return new(new Logger("Profiling"), profilerTitle, thresholdDurationToConsiderLogging, logSeverityAboveThreshold, logSeverityBelowThreshold, capLogCountAt);
		}

		public static QuickProfilerStopwatch WithThreshold(Logger logger, string profilerTitle, float thresholdDurationToConsiderLogging, LogSeverity logSeverityAboveThreshold = LogSeverity.Warning, LogSeverity logSeverityBelowThreshold = LogSeverity.None, int capLogCountAt = 0)
		{
			return new(logger, profilerTitle, thresholdDurationToConsiderLogging, logSeverityAboveThreshold, logSeverityBelowThreshold, capLogCountAt);
		}

		private QuickProfilerStopwatch(Logger logger, string profilerTitle, float thresholdDurationToConsiderLogging, LogSeverity logSeverityAboveThreshold = LogSeverity.Warning, LogSeverity logSeverityBelowThreshold = LogSeverity.None, int capLogCountAt = 0)
		{
			Stopwatch = new ProfilerStopwatch();
			Logger = logger;
			ProfilerTitle = profilerTitle;
			ThresholdDurationToConsiderLogging = thresholdDurationToConsiderLogging;
			LogSeverityAboveThreshold = logSeverityAboveThreshold;
			LogSeverityBelowThreshold = logSeverityBelowThreshold;
			CapLogCountAt = capLogCountAt;

			Stopwatch.Start();
		}

		public void Dispose()
		{
			Stopwatch.End();

			if (HasThresholdDuration)
			{
				if (Stopwatch.Elapsed > ThresholdDurationToConsiderLogging)
				{
					if (ConsumeLogBudgetAndDecideIfShouldLog(LogSeverityAboveThreshold))
					{
						Logger.Any(LogSeverityAboveThreshold, ZString.Concat("Running '", ProfilerTitle, "' took '", Stopwatch.Elapsed.ToStringMinutesSecondsMicrosecondsFromSeconds(), "' which is longer than the expected '", ThresholdDurationToConsiderLogging, "' seconds"));
					}
				}
				else if (ConsumeLogBudgetAndDecideIfShouldLog(LogSeverityBelowThreshold))
				{
					Logger.Any(LogSeverityBelowThreshold, ZString.Concat("Running '", ProfilerTitle, "' took '", Stopwatch.Elapsed.ToStringMinutesSecondsMicrosecondsFromSeconds(), "'"));
				}
			}
			else // No threshold duration specified, so we log everything.
			{
				if (ConsumeLogBudgetAndDecideIfShouldLog(LogSeverityAboveThreshold))
				{
					Logger.Any(LogSeverityAboveThreshold, ZString.Concat("Running '", ProfilerTitle, "' took '", Stopwatch.Elapsed.ToStringMinutesSecondsMicrosecondsFromSeconds(), "'"));
				}
			}
		}

		#region Log Budget

		private static readonly Dictionary<string, int> LogCountsByTitle = new();

		private bool ConsumeLogBudgetAndDecideIfShouldLog(LogSeverity severity)
		{
			if (severity == LogSeverity.None)
			{
				return false;
			}

			if (CapLogCountAt <= 0)
			{
				return true;
			}

			lock (LogCountsByTitle)
			{
				var count = LogCountsByTitle.GetValueOrDefault(ProfilerTitle, 0);
				if (count < CapLogCountAt)
				{
					LogCountsByTitle[ProfilerTitle] = count + 1;
					return true;
				}

				return false;
			}
		}

		#endregion
	}

}
