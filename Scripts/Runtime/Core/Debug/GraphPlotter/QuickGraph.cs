using System.Collections.Generic;
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
		public static readonly Dictionary<string, Graph> Graphs = new Dictionary<string, Graph>();

		public static void Plot(string graphTitle, ValueAxisRangeConfiguration rangeConfiguration, float time, params QuickChannel[] quickChannels)
		{
			var frame = Time.frameCount;

			// Create graph if necessary
			var newGraph = !Graphs.TryGetValue(graphTitle, out var graph);
			Graph.SetupGraph(true, ref graph, graphTitle, null, rangeConfiguration);
			if (newGraph)
			{
				Graphs.Add(graphTitle, graph);
			}

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
				graph.Channels[i].Sample(quickChannels[i].Value, time, frame);
			}
		}
	}

}