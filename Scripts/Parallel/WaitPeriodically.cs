using System;
using UnityEngine;

[Serializable]
public class WaitPeriodically : CustomYieldInstruction
{
	[NonSerialized]
	public float PeriodStartTime;
	public float Period;
	public bool UseRealtime = true;

	public WaitPeriodically()
	{
		if (Period <= 0)
		{
			Period = 1f; // A meaningful default value
		}
		if (Application.isPlaying)
		{
			PeriodStartTime = CurrentTime; // Make sure 'UseRealtime' is assigned before calling 'CurrentTime'.
		}
	}

	public WaitPeriodically(float period, bool useRealtime)
	{
		UseRealtime = useRealtime;
		Period = period;
		if (Application.isPlaying)
		{
			PeriodStartTime = CurrentTime; // Make sure 'UseRealtime' is assigned before calling 'CurrentTime'.
		}
	}

	public float CurrentTime
	{
		get { return UseRealtime ? Time.realtimeSinceStartup : Time.time; }
	}

	public bool IsPassed
	{
		get { return PeriodStartTime + Period < CurrentTime; }
	}

	public override bool keepWaiting
	{
		get
		{
			var currentTime = CurrentTime;

			if (PeriodStartTime + Period < currentTime)
			{
				PeriodStartTime = currentTime;
				return false;
			}
			return true;
		}
	}
}
