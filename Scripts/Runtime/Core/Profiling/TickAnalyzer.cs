#if UNITY

using System;
using Extenity.DataToolbox;
using Extenity.DebugToolbox.GraphPlotting;
using Extenity.MathToolbox;
using Extenity.MessagingToolbox;
using UnityEngine;

namespace Extenity.ProfilingToolbox
{

	[Serializable]
	public class TickPlotter
	{
		public string Title;
		public VerticalRange VerticalRange;
		public GameObject Context;

		[Tooltip("Ticks per second.")]
		public bool OutputTPSGraph = true;
		public string TPSDescription = "TPS";
		public Color TPSColor = new Color(0.2f, 0.9f, 0.2f, 1f);
		[Tooltip("Average ticks per second calculated using the average elapsed time of all samples in history.")]
		public bool OutputAverageTPSGraph = true;
		public string AverageTPSDescription = "Average TPS";
		public Color AverageTPSColor = new Color(0.4f, 0.5f, 0.2f, 1f);

		public TickPlotter(string title, VerticalRange verticalRange, GameObject context = null)
		{
			Title = title;
			VerticalRange = verticalRange;
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

			if (ElapsedTimes == null || ElapsedTimes.ValueCapacity != historySize)
			{
				ElapsedTimes = new RunningHotMeanDouble(historySize);
			}
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

		public readonly ExtenityEvent OnUpdate = new ExtenityEvent();
		public readonly ExtenityEvent OnTick = new ExtenityEvent();

		public void ClearAllEvents()
		{
			OnUpdate.RemoveAllListeners();
			OnTick.RemoveAllListeners();
		}

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
					OnUpdate.InvokeSafe();
				}

				OnTick.InvokeSafe();
			}
		}

		#endregion

		#region Graph

		public bool IsGraphPlottingEnabled => Plotter != null;
		private TickPlotter Plotter;

		public void EnableGraphPlotting(TickPlotter _plotter)
		{
			// Deinitialize previous plotting first.
			DisableGraphPlotting();

			Plotter = _plotter;

			CollectionTools.ResizeIfRequired(ref Plotter._Channels, 2);
			var channels = Plotter._Channels;
			Graph.SetupGraph(true, ref Plotter._Graph, Plotter.Title, Plotter.Context, Plotter.VerticalRange);
			var graph = Plotter._Graph;

			if (Plotter.OutputTPSGraph)
			{
				Channel.SetupChannel(true, graph, ref channels[0], Plotter.TPSDescription, Plotter.TPSColor);
			}
			if (Plotter.OutputAverageTPSGraph)
			{
				Channel.SetupChannel(true, graph, ref channels[1], Plotter.AverageTPSDescription, Plotter.AverageTPSColor);
			}
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
				Plotter._Channels[0].Sample(TicksPerSecond, (float)CurrentPeriodStartTime);
			}
			if (Plotter.OutputAverageTPSGraph)
			{
				Plotter._Channels[1].Sample((float)(1.0 / ElapsedTimes.Mean), (float)CurrentPeriodStartTime);
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

#endif
