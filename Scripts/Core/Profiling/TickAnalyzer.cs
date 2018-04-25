using System.Diagnostics;
using Extenity.MathToolbox;

namespace Extenity.ProfilingToolbox
{

	public class TickAnalyzer
	{
		private long CurrentPeriodStartTime;
		private long LastTickTime;

		public int LastElapsedTime;
		public float LastElapsedTimeDeviation
		{
			get { return LastElapsedTime - MeanElapsedTime; }
		}

		private int InternalTickCounter;
		public int TicksPerSecond;

		public float MeanElapsedTime
		{
			get { return ElapsedTimes.Mean; }
		}

		public RunningHotMeanInt ElapsedTimes;


		public delegate void UpdateAction(TickAnalyzer me);
		public event UpdateAction OnUpdate;
		public delegate void TickAction(TickAnalyzer me);
		public event TickAction OnTick;


		public TickAnalyzer(bool startImmediately = true)
		{
			if (startImmediately)
			{
				Reset();
			}
		}

		public void Reset()
		{
			if (stopwatch == null)
			{
				stopwatch = new Stopwatch();
				stopwatch.Start();
			}
			CurrentPeriodStartTime = stopwatch.ElapsedMilliseconds;
			InternalTickCounter = 0;
			TicksPerSecond = 0;
			LastTickTime = 0;

			ElapsedTimes = new RunningHotMeanInt(100);
		}

		public void Tick()
		{
			Tick(stopwatch.ElapsedMilliseconds);
		}

		public void Tick(long timeMs)
		{
			lock (this)
			{
				InternalTickCounter++;

				if (LastTickTime != 0) // To ignore first tick
				{
					var elapsedTime = (int)(timeMs - LastTickTime);
					ElapsedTimes.Push(elapsedTime);
					LastElapsedTime = elapsedTime;
				}

				LastTickTime = timeMs;

				while (timeMs - CurrentPeriodStartTime >= 1000)
				{
					CurrentPeriodStartTime += 1000;
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

		private Stopwatch stopwatch;

		public Stopwatch Stopwatch
		{
			get { return stopwatch; }
		}

		public void SetCustomStopwatch(Stopwatch stopwatch)
		{
			this.stopwatch = stopwatch;
		}
	}

}
