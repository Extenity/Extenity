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

		public void StartMeasurement(float now)
		{
			StartTime = now;
		}

		public void FinalizeMeasurement(float now, object obj, string methodName)
		{
			var elapsed = (int)((now - StartTime) * 1000);
			if (elapsed > Tolerance)
			{
				if (now > LastLogTime + MinimumLogInterval)
				{
					LastLogTime = now;
					Log.Warning($"{obj.GetType().Name}.{methodName} took {elapsed} ms to execute.");
				}
			}
		}

		#region Log

		private static readonly Logger Log = new(nameof(PerformanceAlert));

		#endregion
	}

}
