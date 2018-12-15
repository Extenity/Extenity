using System;
using Extenity.DebugFlowTool.GraphPlotting;
using Extenity.MathToolbox;
using UnityEngine;

namespace Extenity.ProfilingToolbox
{

	public class TickAnalyzer
	{
		#region Initialization

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

		[Serializable]
		public class GraphPlottingConfiguration
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

			public GraphPlottingConfiguration(string title, ValueAxisRangeConfiguration rangeConfiguration, GameObject context = null)
			{
				Title = title;
				RangeConfiguration = rangeConfiguration;
				Context = context;
			}

			internal Graph _Graph;
			internal Channel[] _Channels;
		}

		public bool IsGraphPlottingEnabled => PlottingConfiguration != null;
		private GraphPlottingConfiguration PlottingConfiguration;

		public void EnableGraphPlotting(GraphPlottingConfiguration configuration)
		{
			// Deinitialize previous plotting first.
			DisableGraphPlotting();

			PlottingConfiguration = configuration;

			Graph.SetupGraphWithXYChannels(true, ref PlottingConfiguration._Graph, PlottingConfiguration.Title, PlottingConfiguration.Context,
				PlottingConfiguration.RangeConfiguration, ref PlottingConfiguration._Channels, true, true,
				"TPS", "Average TPS", PlottingConfiguration.TPSColor, PlottingConfiguration.AverageTPSColor);
		}

		private void DisableGraphPlotting()
		{
			if (IsGraphPlottingEnabled)
			{
				Graph.SafeClose(ref PlottingConfiguration._Graph);
			}

			PlottingConfiguration = null;
		}

		private void OutputToGraph()
		{
			var configuration = PlottingConfiguration;
			if (configuration == null)
				return;

			if (configuration.OutputTPSGraph)
			{
				configuration._Channels[0].Sample(TicksPerSecond, (float)CurrentPeriodStartTime, Time.frameCount);
			}
			if (configuration.OutputAverageTPSGraph)
			{
				configuration._Channels[1].Sample((float)(1.0 / ElapsedTimes.Mean), (float)CurrentPeriodStartTime, Time.frameCount);
			}
		}

		#endregion
	}

}
