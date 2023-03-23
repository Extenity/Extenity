using Cysharp.Text;
using Extenity.DataToolbox;
using IDisposable = System.IDisposable;

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

	public struct QuickProfilerStopwatch : IDisposable
	{
		private ProfilerStopwatch Stopwatch;
		private readonly Logger Logger;
		private readonly string ProfilerTitle;
		private readonly float ThresholdDurationToConsiderLogging;

		public QuickProfilerStopwatch(Logger logger, string profilerTitle, float thresholdDurationToConsiderLogging = 0f)
		{
			Stopwatch = new ProfilerStopwatch();
			Logger = logger;
			ProfilerTitle = profilerTitle;
			ThresholdDurationToConsiderLogging = thresholdDurationToConsiderLogging;
			Stopwatch.Start();
		}

		public QuickProfilerStopwatch(string profilerTitle, float thresholdDurationToConsiderLogging = 0f)
		{
			Stopwatch = new ProfilerStopwatch();
			Logger = new Logger("Profiling");
			ProfilerTitle = profilerTitle;
			ThresholdDurationToConsiderLogging = thresholdDurationToConsiderLogging;
			Stopwatch.Start();
		}

		public void Dispose()
		{
			Stopwatch.End();

			if (Stopwatch.Elapsed > ThresholdDurationToConsiderLogging)
			{
				if (ThresholdDurationToConsiderLogging > 0f)
				{
					Logger.Warning(ZString.Concat("Running '", ProfilerTitle, "' took '", Stopwatch.Elapsed.ToStringMinutesSecondsMillisecondsFromSeconds(), "' which is longer than the expected '", ThresholdDurationToConsiderLogging, "' seconds"));
				}
				else
				{
					Logger.Info(ZString.Concat("Running '", ProfilerTitle, "' took '", Stopwatch.Elapsed.ToStringMinutesSecondsMillisecondsFromSeconds(), "'"));
				}
			}
			else
			{
				Logger.Verbose(ZString.Concat("Running '", ProfilerTitle, "' took '", Stopwatch.Elapsed.ToStringMinutesSecondsMillisecondsFromSeconds(), "'"));
			}
		}
	}

}
