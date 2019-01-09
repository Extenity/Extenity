using System;
using Object = UnityEngine.Object;

namespace Extenity.ProfilingToolbox
{

	public class QuickProfilerStopwatch : IDisposable
	{
		private ProfilerStopwatch Stopwatch = new ProfilerStopwatch();
		private readonly Object Context;
		private readonly string ProfilerMessageFormat;

		public QuickProfilerStopwatch(Object context, string profilerMessageFormat)
		{
			Context = context;
			ProfilerMessageFormat = profilerMessageFormat;
			Stopwatch.Start();
		}

		public QuickProfilerStopwatch(string profilerMessageFormat)
		{
			ProfilerMessageFormat = profilerMessageFormat;
			Stopwatch.Start();
		}

		public void Dispose()
		{
			Stopwatch.EndAndLog(Context, ProfilerMessageFormat);
			Stopwatch = null;
		}
	}

}
