using UnityEngine;

namespace Extenity.ProfilingToolbox
{

	public class PerformanceAlert
	{
		public int Tolerance = 50; // Milliseconds. Below this tolerance is acceptable.
		public float MinimumLogInterval = 1f; // Milliseconds. Minimum time until another performance alert is logged.

		//private Stopwatch Stopwatch = new Stopwatch(); This has caused incorrect measurements. Probably Unity's old mono version problems.
		private float LastLogTime = -100000f;
		public float StartTime { get; private set; }

		public PerformanceAlert()
		{
		}

		public PerformanceAlert(int tolerance)
		{
			Tolerance = tolerance;
		}

		public PerformanceAlert(int tolerance, float minimumLogInterval)
		{
			Tolerance = tolerance;
			MinimumLogInterval = minimumLogInterval;
		}

		public void StartMeasurement()
		{
			StartTime = Time.realtimeSinceStartup;
		}

		public void FinalizeMeasurement(object obj, string methodName)
		{
			var now = Time.realtimeSinceStartup;
			var elapsed = (int)((now - StartTime) * 1000);
			if (elapsed > Tolerance)
			{
				if (now > LastLogTime + MinimumLogInterval)
				{
					LastLogTime = now;
					Debug.LogWarningFormat("{0}.{1} took {2} ms to execute.", obj.GetType().Name, methodName, elapsed);
				}
			}
		}
	}

}
