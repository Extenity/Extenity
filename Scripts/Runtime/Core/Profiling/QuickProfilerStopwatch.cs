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

	public class QuickProfilerStopwatch : IDisposable
	{
		private ProfilerStopwatch Stopwatch = new ProfilerStopwatch();
		private readonly ContextObject Context;
		private readonly string ProfilerMessageFormat;
		private readonly float ThresholdDurationToConsiderLogging;

		public QuickProfilerStopwatch(ContextObject context, string profilerMessageFormat, float thresholdDurationToConsiderLogging = 0f)
		{
			Context = context;
			ProfilerMessageFormat = profilerMessageFormat;
			ThresholdDurationToConsiderLogging = thresholdDurationToConsiderLogging;
			Stopwatch.Start();
		}

		public QuickProfilerStopwatch(string profilerMessageFormat, float thresholdDurationToConsiderLogging = 0f)
		{
			ProfilerMessageFormat = profilerMessageFormat;
			ThresholdDurationToConsiderLogging = thresholdDurationToConsiderLogging;
			Stopwatch.Start();
		}

		public void Dispose()
		{
			Stopwatch.End();
			if (Stopwatch.Elapsed > ThresholdDurationToConsiderLogging)
			{
				Stopwatch.LogInfo(Context, ProfilerMessageFormat);
			}
			Stopwatch = null;
		}
	}

}
