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
		private readonly ContextObject Context;
		private readonly string ProfilerMessageFormat;
		private readonly LogCategory LogCategory;
		private readonly float ThresholdDurationToConsiderLogging;

		public QuickProfilerStopwatch(ContextObject context, string profilerMessageFormat, float thresholdDurationToConsiderLogging = 0f, LogCategory logCategory = LogCategory.Info)
		{
			Stopwatch = new ProfilerStopwatch();
			Context = context;
			ProfilerMessageFormat = profilerMessageFormat;
			LogCategory = logCategory;
			ThresholdDurationToConsiderLogging = thresholdDurationToConsiderLogging;
			Stopwatch.Start();
		}

		public QuickProfilerStopwatch(string profilerMessageFormat, float thresholdDurationToConsiderLogging = 0f, LogCategory logCategory = LogCategory.Info)
		{
			Stopwatch = new ProfilerStopwatch();
			Context = default;
			ProfilerMessageFormat = profilerMessageFormat;
			LogCategory = logCategory;
			ThresholdDurationToConsiderLogging = thresholdDurationToConsiderLogging;
			Stopwatch.Start();
		}

		public void Dispose()
		{
			Stopwatch.End();
			if (Stopwatch.Elapsed > ThresholdDurationToConsiderLogging)
			{
				Stopwatch.Log(Context, ProfilerMessageFormat, LogCategory);
			}
		}
	}

}
