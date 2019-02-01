using System;
using Extenity.DebugToolbox.GraphPlotting;
using Extenity.MathToolbox;
using UnityEngine;

namespace Extenity.ProfilingToolbox
{

	[Serializable]
	public class TickPlotter
	{
		public string Title;
		public ValueAxisRangeConfiguration RangeConfiguration;
		public GameObject Context;

		[Tooltip("Ticks per second.")]
		public bool OutputTPSGraph = true;
		public Color TPSColor = new Color(0.2f, 0.9f, 0.2f, 1f);
		[Tooltip("Average ticks per second calculated using the average elapsed time of all samples in history.")]
		public bool OutputAverageTPSGraph = true;
		public Color AverageTPSColor = new Color(0.4f, 0.5f, 0.2f, 1f);

		public TickPlotter(string title, ValueAxisRangeConfiguration rangeConfiguration, GameObject context = null)
		{
			Title = title;
			RangeConfiguration = rangeConfiguration;
			Context = context;
		}

		internal Graph _Graph;
		internal Channel[] _Channels;
	}

	public class TickAnalyzer
	{
		#region Initialization

		public TickAnalyzer()
		{
		}

		public TickAnalyzer(double currentTime, int historySize = 100)
		{
			Reset(currentTime, historySize);
		}

		public TickAnalyzer(TickPlotter plotter, double currentTime, int historySize = 100)
			: this(currentTime, historySize)
		{
			EnableGraphPlotting(plotter);
		}

		public void Reset(double currentTime, int historySize = 100)
		{
			CurrentPeriodStartTime = currentTime;
			InternalTickCounter = 0;
			TicksPerSecond = 0;
			LastTickTime = 0;

			ElapsedTimes = new RunningHotMeanDouble(historySize);
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

		public delegate void AnalyzerAction(TickAnalyzer me);
		public event AnalyzerAction OnUpdate;
		public event AnalyzerAction OnTick;

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

					OutputToGraph();
					OnUpdate?.Invoke(this);
				}

				OnTick?.Invoke(this);
			}
		}

		#endregion

		#region Graph

		public bool IsGraphPlottingEnabled => Plotter != null;
		private TickPlotter Plotter;

		public void EnableGraphPlotting(TickPlotter plotter)
		{
			// Deinitialize previous plotting first.
			DisableGraphPlotting();

			Plotter = plotter;

			Graph.SetupGraphWithXYChannels(true, ref Plotter._Graph, Plotter.Title, Plotter.Context,
				Plotter.RangeConfiguration, ref Plotter._Channels, true, true,
				"TPS", "Average TPS", Plotter.TPSColor, Plotter.AverageTPSColor);
		}

		private void DisableGraphPlotting()
		{
			if (IsGraphPlottingEnabled)
			{
				Graph.SafeClose(ref Plotter._Graph);
			}

			Plotter = null;
		}

		private void OutputToGraph()
		{
			if (Plotter == null)
				return;

			if (Plotter.OutputTPSGraph)
			{
				Plotter._Channels[0].Sample(TicksPerSecond, (float)CurrentPeriodStartTime, Time.frameCount);
			}
			if (Plotter.OutputAverageTPSGraph)
			{
				Plotter._Channels[1].Sample((float)(1.0 / ElapsedTimes.Mean), (float)CurrentPeriodStartTime, Time.frameCount);
			}
		}

		#endregion

		#region Tools

		public static int HistorySizeFor(float expectedFPS, float durationForAverageCalculation)
		{
			return (int)(expectedFPS * durationForAverageCalculation);
		}

		#endregion
	}

}
