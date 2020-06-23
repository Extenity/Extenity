using System.Collections.Generic;
using Extenity.MathToolbox;
using UnityEngine;

namespace Extenity.DebugToolbox.GraphPlotting
{

	public struct QuickChannel
	{
		public string Name;
		public Color Color;
		public float Value;

		public QuickChannel(string name, Color color, float value)
		{
			Name = name;
			Color = color;
			Value = value;
		}
	}

	public class QuickGraph
	{
		#region Plot - General

		public static void Plot(string graphTitle, VerticalRangeConfiguration verticalRangeConfiguration, float time, params QuickChannel[] quickChannels)
		{
			// Create graph if necessary
			Graph graph = null;
			Graph.SetupGraph(true, ref graph, graphTitle, null, verticalRangeConfiguration);

			// Create channels if necessary
			if (graph.Channels.Count != quickChannels.Length)
			{
				// Close all open channels first.
				foreach (var channel in graph.Channels)
				{
					channel.Close();
				}

				// Create channels
				foreach (var quickChannel in quickChannels)
				{
					Channel dummy = null;
					Channel.SetupChannel(true, graph, ref dummy, quickChannel.Name, quickChannel.Color);
				}
			}

			// Plot
			for (var i = 0; i < quickChannels.Length; i++)
			{
				graph.Channels[i].Sample(quickChannels[i].Value, time);
			}
		}

		#endregion

		#region Plot - Vector

		public static readonly Color VectorXColor = new Color(0f, 1f, 0f, 1f);
		public static readonly Color VectorYColor = new Color(0.52f, 0f, 0f, 1f);
		public static readonly Color VectorZColor = new Color(1f, 0.63f, 0f, 1f);

		public static void Plot(string graphTitle, VerticalRangeConfiguration verticalRangeConfiguration, float time, Vector3 value)
		{
			Plot(graphTitle, verticalRangeConfiguration, time,
			     new QuickChannel("X", VectorXColor, value.x),
			     new QuickChannel("Y", VectorYColor, value.y),
			     new QuickChannel("Z", VectorZColor, value.z)
			);
		}

		public static void Plot(string graphTitle, VerticalRangeConfiguration verticalRangeConfiguration, float time, Vector2 value)
		{
			Plot(graphTitle, verticalRangeConfiguration, time,
			     new QuickChannel("X", VectorXColor, value.x),
			     new QuickChannel("Y", VectorYColor, value.y)
			);
		}

		#endregion

		#region Plot - PID

		public static readonly Color PIDInputColor = new Color(0f, 1f, 0f, 1f);
		public static readonly Color PIDOutputColor = new Color(0.52f, 0f, 0f, 1f);
		public static readonly Color PIDTargetColor = new Color(1f, 0.63f, 0f, 1f);
		public static readonly float PIDPaddingFactor = 1.1f;

		/// <summary>
		/// First: PID instance ID.
		/// Second: PID last computation time.
		/// </summary>
		private static readonly Dictionary<int, double> PIDLogTimes = new Dictionary<int, double>();

		public static void Plot(string graphTitle, float time, PID pid)
		{
			// Check if results was logged for current computation step
			{
				var instanceID = pid.InstanceID;
				if (PIDLogTimes.TryGetValue(instanceID, out var lastLoggedComputationTime))
				{
					if (pid.LastComputationTime == lastLoggedComputationTime)
					{
						return;
					}
					PIDLogTimes[instanceID] = pid.LastComputationTime;
				}
				else
				{
					PIDLogTimes.Add(instanceID, pid.LastComputationTime);
				}
			}

			Plot(graphTitle,
				VerticalRangeConfiguration.CreateFixed(
					(float)pid.OutMin * PIDPaddingFactor,
					(float)pid.OutMax * PIDPaddingFactor
				),
				time,
				new QuickChannel("Input", PIDInputColor, (float)pid.Input),
				new QuickChannel("Output", PIDOutputColor, (float)pid.Output),
				new QuickChannel("Target", PIDTargetColor, (float)pid.Target)
 			);
		}

		#endregion

		#region Clear

		public static void Clear(string graphTitle)
		{
			Graph graph = null;
			graph = Graphs.GetGraphByTitleAndContext(graphTitle, null);
			if (graph != null)
			{
				Graph.SafeClose(ref graph);
			}
		}

		#endregion
	}

}
