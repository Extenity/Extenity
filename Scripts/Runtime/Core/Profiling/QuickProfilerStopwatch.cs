using System;
using Object = UnityEngine.Object;

namespace Extenity.ProfilingToolbox
{

	public class QuickProfilerStopwatch : IDisposable
	{
		private ProfilerStopwatch Stopwatch = new ProfilerStopwatch();
		private readonly Object Context;
		private readonly string ProfilerMessageFormat;
		private readonly float ThresholdDurationToConsiderLogging;

		public QuickProfilerStopwatch(Object context, string profilerMessageFormat, float thresholdDurationToConsiderLogging = 0f)
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
