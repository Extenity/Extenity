using Extenity.MathToolbox;

namespace Extenity.ProfilingToolbox
{

	public class TickAnalyzer
	{
		#region Initialization

		public void Reset(double currentTime)
		{
			CurrentPeriodStartTime = currentTime;
			InternalTickCounter = 0;
			TicksPerSecond = 0;
			LastTickTime = 0;

			ElapsedTimes = new RunningHotMeanDouble(100);
		}

		#endregion

		#region Stats

		public double CurrentPeriodStartTime;
		public double LastTickTime;

		public double LastElapsedTime;
		public double LastElapsedTimeDeviation => LastElapsedTime - MeanElapsedTime;

		private int InternalTickCounter;
		public int TicksPerSecond;

		public double MeanElapsedTime => ElapsedTimes.Mean;

		public RunningHotMeanDouble ElapsedTimes;

		//public double Variance
		//{
		//	get { return standardDeviation.Variance; }
		//}

		//public double StandardDeviation
		//{
		//	get { return standardDeviation.StandardDeviation; }
		//}

		//public double LastFrameDurationDeviationOverStandardDeviation
		//{
		//	get
		//	{
		//		var dev = StandardDeviation;
		//		if (dev > 0)
		//		{
		//			return (double)LastFrameDurationDeviation / dev;
		//		}
		//		return 0;
		//	}
		//}

		#endregion

		#region Events

		public delegate void UpdateAction(TickAnalyzer me);
		public event UpdateAction OnUpdate;
		public delegate void TickAction(TickAnalyzer me);
		public event TickAction OnTick;

		#endregion

		#region Tick

		public void Tick(double currentTime)
		{
			lock (this)
			{
				InternalTickCounter++;

				if (LastTickTime != 0) // To ignore first tick
				{
					var elapsedTime = currentTime - LastTickTime;
					ElapsedTimes.Push(elapsedTime);
					LastElapsedTime = elapsedTime;
				}

				LastTickTime = currentTime;

				while (currentTime - CurrentPeriodStartTime >= 1.0)
				{
					CurrentPeriodStartTime += 1.0;
					TicksPerSecond = InternalTickCounter;
					InternalTickCounter = 0;

					OnUpdate?.Invoke(this);
				}

				OnTick?.Invoke(this);
			}
		}

		#endregion
	}

}
