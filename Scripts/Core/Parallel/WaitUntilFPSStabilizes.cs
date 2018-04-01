using Extenity.MathToolbox;
using UnityEngine;

namespace Extenity.ParallelToolbox
{

	public class WaitUntilFPSStabilizes : CustomYieldInstruction
	{
		#region Configuration

		private const float ClippedFPS = 40f;
		private const float AllowedRangeMinRatio = 0.7f;
		private const float AllowedRangeMaxRatio = 1.3f;

		#endregion

		private RunningHotMeanFloat MeanFPS = new RunningHotMeanFloat(6);
		//private List<float> FullLog = new List<float>(1000);

		public float MaxWaitTime;
		private float StartTime;
		private float LastCheckTime;

		public WaitUntilFPSStabilizes(float maxWaitTime = 0f)
		{
			MaxWaitTime = maxWaitTime;
			Start();
		}

		public void Start()
		{
			StartTime = Time.realtimeSinceStartup;
			LastCheckTime = StartTime;
			MeanFPS.Clear();
			//FullLog.Clear();
		}

		public override bool keepWaiting
		{
			get
			{
				var now = Time.realtimeSinceStartup;
				if (now - StartTime > MaxWaitTime)
				{
					//FullLog.LogList("FPS stabilization timed out");
					return false;
				}
				var deltaTime = now - LastCheckTime;
				LastCheckTime = now;
				var fps = 1f / deltaTime;
				// We expect stabilization in only low FPS.
				// FPS above ClippedFPS will be considered as okay whether it oscillates or not.
				if (fps > ClippedFPS)
					fps = ClippedFPS;

				MeanFPS.Push(fps);
				//FullLog.Add(fps);

				if (MeanFPS.Values.IsCapacityFilled)
				{
					var average = (float)MeanFPS.Mean;
					var allowedRangeMin = average * AllowedRangeMinRatio;
					var allowedRangeMax = average * AllowedRangeMaxRatio;
					var values = MeanFPS.Values.Items;
					for (var i = 0; i < values.Length; i++)
					{
						if (values[i] - average > allowedRangeMax ||
						    average - values[i] > allowedRangeMin)
						{
							return true;
						}
					}
					//FullLog.LogList("FPS stabilization log");
					return false;
				}
				return true;
			}
		}
	}

}
