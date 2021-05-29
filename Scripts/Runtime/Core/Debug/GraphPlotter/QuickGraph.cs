using System.Collections.Generic;
using Extenity.DataToolbox;
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

		public static void Plot(string graphTitle, VerticalRange verticalRange, float time,
		                        string channelName, Color channelColor, float channelValue)
		{
			var channels = New.List<QuickChannel>(1);
			channels.Add(new QuickChannel(channelName, channelColor, channelValue));
			Plot(graphTitle, verticalRange, time, channels);
			Release.List(ref channels);
		}

		public static void Plot(string graphTitle, VerticalRange verticalRange, float time,
		                        string channelName1, Color channelColor1, float channelValue1,
		                        string channelName2, Color channelColor2, float channelValue2)
		{
			var channels = New.List<QuickChannel>(2);
			channels.Add(new QuickChannel(channelName1, channelColor1, channelValue1));
			channels.Add(new QuickChannel(channelName2, channelColor2, channelValue2));
			Plot(graphTitle, verticalRange, time, channels);
			Release.List(ref channels);
		}

		public static void Plot(string graphTitle, VerticalRange verticalRange, float time,
		                        string channelName1, Color channelColor1, float channelValue1,
		                        string channelName2, Color channelColor2, float channelValue2,
		                        string channelName3, Color channelColor3, float channelValue3)
		{
			var channels = New.List<QuickChannel>(3);
			channels.Add(new QuickChannel(channelName1, channelColor1, channelValue1));
			channels.Add(new QuickChannel(channelName2, channelColor2, channelValue2));
			channels.Add(new QuickChannel(channelName3, channelColor3, channelValue3));
			Plot(graphTitle, verticalRange, time, channels);
			Release.List(ref channels);
		}

		public static void Plot(string graphTitle, VerticalRange verticalRange, float time,
		                        string channelName1, Color channelColor1, float channelValue1,
		                        string channelName2, Color channelColor2, float channelValue2,
		                        string channelName3, Color channelColor3, float channelValue3,
		                        string channelName4, Color channelColor4, float channelValue4)
		{
			var channels = New.List<QuickChannel>(4);
			channels.Add(new QuickChannel(channelName1, channelColor1, channelValue1));
			channels.Add(new QuickChannel(channelName2, channelColor2, channelValue2));
			channels.Add(new QuickChannel(channelName3, channelColor3, channelValue3));
			channels.Add(new QuickChannel(channelName4, channelColor4, channelValue4));
			Plot(graphTitle, verticalRange, time, channels);
			Release.List(ref channels);
		}

		public static void Plot(string graphTitle, VerticalRange verticalRange, float time, List<QuickChannel> quickChannels)
		{
			// Create graph if necessary
			Graph graph = null;
			Graph.SetupGraph(true, ref graph, graphTitle, null, verticalRange);

			// Create channels if necessary
			if (graph.Channels.Count != quickChannels.Count)
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
			for (var i = 0; i < quickChannels.Count; i++)
			{
				graph.Channels[i].Sample(quickChannels[i].Value, time);
			}
		}

		#endregion

		#region Plot - Float

		public static readonly Color FloatColor = new Color(0f, 1f, 0f, 1f);

		public static void Plot(string graphTitle, VerticalRange verticalRange, float time, float value)
		{
			Plot(graphTitle, verticalRange, time, "Value", FloatColor, value);
		}

		#endregion

		#region Plot - Vector

		public static readonly Color VectorXColor = new Color(0f, 1f, 0f, 1f);
		public static readonly Color VectorYColor = new Color(0.52f, 0f, 0f, 1f);
		public static readonly Color VectorZColor = new Color(1f, 0.63f, 0f, 1f);

		public static void Plot(string graphTitle, VerticalRange verticalRange, float time, Vector3 value)
		{
			Plot(graphTitle, verticalRange, time,
			     "X", VectorXColor, value.x,
			     "Y", VectorYColor, value.y,
			     "Z", VectorZColor, value.z
			);
		}

		public static void Plot(string graphTitle, VerticalRange verticalRange, float time, Vector2 value)
		{
			Plot(graphTitle, verticalRange, time,
			     "X", VectorXColor, value.x,
			     "Y", VectorYColor, value.y
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
			     VerticalRange.Fixed(
				     (float)pid.OutMin * PIDPaddingFactor,
				     (float)pid.OutMax * PIDPaddingFactor
			     ),
			     time,
			     "Input", PIDInputColor, (float)pid.Input,
			     "Output", PIDOutputColor, (float)pid.Output,
			     "Target", PIDTargetColor, (float)pid.Target
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
