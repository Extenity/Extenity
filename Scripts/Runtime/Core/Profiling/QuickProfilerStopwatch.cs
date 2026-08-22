using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Extenity.ApplicationToolbox;
using Extenity.DataToolbox;

namespace Extenity.ProfilingToolbox
{
	public enum QuickProfilerLoggingType
	{
		InfoLog = 0,
		WarningLog,
		InfoLogAboveThreshold,
		WarningLogAboveThreshold,
		WarningLogAboveAndInfoLogBelowThreshold,
		WarningLogAboveThresholdCappedAt5,
	}

	/// <summary>
	/// Times a scope and logs on disposal. Construct only through static methods.
	/// </summary>
	public readonly struct QuickProfilerStopwatch : IDisposable
	{
		private readonly string ProfilerTitle;
		private readonly double StartTime;
		private readonly float ThresholdDurationToConsiderLogging;
		private readonly QuickProfilerLoggingType LoggingType;

		private const int LogCountCap = 5;

		private static double CurrentTime
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => PrecisionTiming.PreciseTime;
		}

		public static QuickProfilerStopwatch WithInfoLog(string profilerTitle)
		{
			return new(profilerTitle, 0, QuickProfilerLoggingType.InfoLog);
		}

		public static QuickProfilerStopwatch WithWarningLog(string profilerTitle)
		{
			return new(profilerTitle, 0, QuickProfilerLoggingType.WarningLog);
		}

		public static QuickProfilerStopwatch WithInfoLogAboveThreshold(string profilerTitle, float thresholdDurationToConsiderLogging)
		{
			return new(profilerTitle, thresholdDurationToConsiderLogging, QuickProfilerLoggingType.InfoLogAboveThreshold);
		}

		public static QuickProfilerStopwatch WithWarningLogAboveThreshold(string profilerTitle, float thresholdDurationToConsiderLogging)
		{
			return new(profilerTitle, thresholdDurationToConsiderLogging, QuickProfilerLoggingType.WarningLogAboveThreshold);
		}

		public static QuickProfilerStopwatch WithWarningLogAboveAndInfoLogBelowThreshold(string profilerTitle, float thresholdDurationToConsiderLogging)
		{
			return new(profilerTitle, thresholdDurationToConsiderLogging, QuickProfilerLoggingType.WarningLogAboveAndInfoLogBelowThreshold);
		}

		public static QuickProfilerStopwatch WithWarningLogAboveThresholdCappedAt5(string profilerTitle, float thresholdDurationToConsiderLogging)
		{
			return new(profilerTitle, thresholdDurationToConsiderLogging, QuickProfilerLoggingType.WarningLogAboveThresholdCappedAt5);
		}

		private QuickProfilerStopwatch(string profilerTitle,
									   float thresholdDurationToConsiderLogging,
									   QuickProfilerLoggingType loggingType)
		{
			ProfilerTitle = profilerTitle;
			ThresholdDurationToConsiderLogging = thresholdDurationToConsiderLogging;
			LoggingType = loggingType;

			StartTime = CurrentTime;
		}

		// Hot path. Dispose must not be called more than once, or it will report twice.
		// Default constructor should not be used, or Dispose will throw.
		public void Dispose()
		{
			var elapsedTime = CurrentTime - StartTime;

			switch (LoggingType)
			{
				case QuickProfilerLoggingType.InfoLog:
				{
					Logger.Info($"Running '{ProfilerTitle}' took '{elapsedTime.ToStringMinutesSecondsMicrosecondsFromSeconds()}'");
					break;
				}
				case QuickProfilerLoggingType.WarningLog:
				{
					Logger.Warning($"Running '{ProfilerTitle}' took '{elapsedTime.ToStringMinutesSecondsMicrosecondsFromSeconds()}'");
					break;
				}
				case QuickProfilerLoggingType.InfoLogAboveThreshold:
				{
					if (elapsedTime > ThresholdDurationToConsiderLogging)
					{
						Logger.Info($"Running '{ProfilerTitle}' took '{elapsedTime.ToStringMinutesSecondsMicrosecondsFromSeconds()}' which is longer than the expected '{ThresholdDurationToConsiderLogging}' seconds");
					}

					break;
				}
				case QuickProfilerLoggingType.WarningLogAboveThreshold:
				{
					if (elapsedTime > ThresholdDurationToConsiderLogging)
					{
						Logger.Warning($"Running '{ProfilerTitle}' took '{elapsedTime.ToStringMinutesSecondsMicrosecondsFromSeconds()}' which is longer than the expected '{ThresholdDurationToConsiderLogging}' seconds");
					}

					break;
				}
				case QuickProfilerLoggingType.WarningLogAboveAndInfoLogBelowThreshold:
				{
					if (elapsedTime > ThresholdDurationToConsiderLogging)
					{
						Logger.Warning($"Running '{ProfilerTitle}' took '{elapsedTime.ToStringMinutesSecondsMicrosecondsFromSeconds()}' which is longer than the expected '{ThresholdDurationToConsiderLogging}' seconds");
					}
					else
					{
						Logger.Info($"Running '{ProfilerTitle}' took '{elapsedTime.ToStringMinutesSecondsMicrosecondsFromSeconds()}'");
					}

					break;
				}
				case QuickProfilerLoggingType.WarningLogAboveThresholdCappedAt5:
				{
					if (elapsedTime > ThresholdDurationToConsiderLogging && ConsumeLogBudgetAndDecideIfShouldLog())
					{
						Logger.Warning($"Running '{ProfilerTitle}' took '{elapsedTime.ToStringMinutesSecondsMicrosecondsFromSeconds()}' which is longer than the expected '{ThresholdDurationToConsiderLogging}' seconds");
					}

					break;
				}
				default:
					throw new ArgumentOutOfRangeException(nameof(LoggingType));
			}
		}

		#region Log

		private static readonly Logger Logger = new("Profiling");

		#endregion

		#region Log Budget

		private static readonly Dictionary<string, byte> LogCountsByTitle = new(0);

		private bool ConsumeLogBudgetAndDecideIfShouldLog()
		{
			lock (LogCountsByTitle)
			{
				var count = LogCountsByTitle.GetValueOrDefault(ProfilerTitle, default);
				if (count < LogCountCap)
				{
					LogCountsByTitle[ProfilerTitle] = (byte)(count + 1);
					return true;
				}

				return false;
			}
		}

		#endregion
	}
}