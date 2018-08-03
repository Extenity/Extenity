using Extenity.MathToolbox;

namespace Extenity.ProfilingToolbox
{

	public class TickAnalyzer
	{
		public double CurrentPeriodStartTime;
		public double LastTickTime;

		public double LastElapsedTime;
		public double LastElapsedTimeDeviation
		{
			get { return LastElapsedTime - MeanElapsedTime; }
		}

		private int InternalTickCounter;
		public int TicksPerSecond;

		public double MeanElapsedTime
		{
			get { return ElapsedTimes.Mean; }
		}

		public RunningHotMeanDouble ElapsedTimes;


		public delegate void UpdateAction(TickAnalyzer me);
		public event UpdateAction OnUpdate;
		public delegate void TickAction(TickAnalyzer me);
		public event TickAction OnTick;


		public void Reset(double currentTime)
		{
			CurrentPeriodStartTime = currentTime;
			InternalTickCounter = 0;
			TicksPerSecond = 0;
			LastTickTime = 0;

			ElapsedTimes = new RunningHotMeanDouble(100);
		}

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

					if (OnUpdate != null)
					{
						OnUpdate(this);
					}
				}

				if (OnTick != null)
				{
					OnTick(this);
				}
			}
		}

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
	}

}
