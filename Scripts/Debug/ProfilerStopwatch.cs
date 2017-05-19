using System;
using UnityEngine;
using Object = UnityEngine.Object;
using Stopwatch = System.Diagnostics.Stopwatch;

public class ProfilerStopwatch
{
	private readonly Stopwatch Stopwatch = new Stopwatch();
	private bool IsStopwatchStarted { get { return Stopwatch.IsRunning; } }

	public void StartStopwatch()
	{
		if (IsStopwatchStarted)
		{
			Debug.LogError("Tried to start profiler stopwatch but it was already started.");
			return;
		}

		Stopwatch.Reset();
		Stopwatch.Start();
	}

	public TimeSpan EndStopwatch()
	{
		if (!IsStopwatchStarted)
		{
			Debug.LogError("Tried to end profiler stopwatch but it was not started.");
			return TimeSpan.Zero;
		}

		Stopwatch.Stop();
		var elapsed = Stopwatch.Elapsed;

		TotalCalls++;
		CumulativeTime += elapsed;
		return elapsed;
	}

	public void EndStopwatchAndLog(string profilerMessageFormat)
	{
		EndStopwatch();
		Log(profilerMessageFormat);
	}

	public void EndStopwatchAndLog(Object context, string profilerMessageFormat)
	{
		EndStopwatch();
		Log(context, profilerMessageFormat);
	}

	#region Log

	public void Log(string profilerMessageFormat)
	{
		Debug.LogFormat(profilerMessageFormat, Stopwatch.Elapsed.ToStringMinutesSecondsMilliseconds());
	}

	public void Log(Object context, string profilerMessageFormat)
	{
		Debug.LogFormat(context, profilerMessageFormat, Stopwatch.Elapsed.ToStringMinutesSecondsMilliseconds());
	}

	public void LogCumulative(string profilerMessageFormat)
	{
		Debug.LogFormat(profilerMessageFormat, CumulativeTime.ToStringMinutesSecondsMilliseconds());
	}

	public void LogCumulative(Object context, string profilerMessageFormat)
	{
		Debug.LogFormat(context, profilerMessageFormat, CumulativeTime.ToStringMinutesSecondsMilliseconds());
	}

	#endregion

	#region Cumulative Time

	public int TotalCalls;
	public TimeSpan CumulativeTime { get; private set; }

	public void ResetCumulativeTime()
	{
		TotalCalls = 0;
		CumulativeTime = default(TimeSpan);
	}

	#endregion
}
