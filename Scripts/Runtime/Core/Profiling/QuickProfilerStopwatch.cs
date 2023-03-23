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
		private readonly string ProfilerMessageFormat;
		private readonly float ThresholdDurationToConsiderLogging;

		public QuickProfilerStopwatch(Logger logger, string profilerMessageFormat, float thresholdDurationToConsiderLogging = 0f)
		{
			Stopwatch = new ProfilerStopwatch();
			Logger = logger;
			ProfilerMessageFormat = profilerMessageFormat;
			ThresholdDurationToConsiderLogging = thresholdDurationToConsiderLogging;
			Stopwatch.Start();
		}

		public void Dispose()
		{
			Stopwatch.End();

			var message = Stopwatch.GetLogMessage(ProfilerMessageFormat);

			if (Stopwatch.Elapsed > ThresholdDurationToConsiderLogging)
			{
				if (ThresholdDurationToConsiderLogging > 0f)
				{
					Logger.Warning(message);
				}
				else
				{
					Logger.Info(message);
				}
			}
			else
			{
				Logger.Verbose(message);
			}
		}
	}

}
