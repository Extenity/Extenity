using System;
using System.Collections.Generic;
using UnityEngine;

namespace Extenity.DebugToolbox.GraphPlotting
{

	public struct Sample
	{
		public readonly float AxisX;
		public readonly float AxisY;

		public Sample(float axisX, float axisY)
		{
			AxisX = axisX;
			AxisY = axisY;
		}

		public static readonly Sample Default = new Sample(float.NaN, float.NaN);
	}

	public class Channel
	{
		#region Initialization

		public Channel(Graph graph, string description) :
			this(graph, description, Color.red)
		{
		}

		public Channel(Graph graph, string description, Color color)
		{
			Graph = graph;

			InitializeMetadata(description, color);
			InitializeData();

			Graph.RegisterChannel(this);
		}

		#endregion

		#region Deinitialization

		private bool _IsClosed;
		public bool IsClosed => _IsClosed;

		public void Close()
		{
			if (_IsClosed)
				return;
			_IsClosed = true;
			Graph.DeregisterChannel(this);
			Graph = null;
		}

		public static void SafeClose(ref Channel channel)
		{
			if (channel != null)
			{
				channel.Close();
				channel = null;
			}
		}

		public static void SafeClose(ref Channel[] channels)
		{
			if (channels != null)
			{
				for (int i = 0; i < channels.Length; i++)
					channels[i].Close();
				channels = null;
			}
		}

		public static void SafeClose(IList<Channel> channels)
		{
			if (channels != null)
			{
				for (int i = 0; i < channels.Count; i++)
					channels[i].Close();
				channels.Clear();
			}
		}

		public void CheckClosed()
		{
			if (_IsClosed)
			{
				throw new Exception($"Tried to do an operation on closed channel '{Description}'.");
			}
		}

		#endregion

		#region Graph

		private Graph Graph;

		#endregion

		#region Metadata

		public string Description;
		public Color Color;

		private void InitializeMetadata(string description, Color color)
		{
			Description = description;
			Color = color;
		}

		#endregion

		#region Data

		public int CurrentSampleIndex;
		public int SampleBufferSize = 150 * 20; // Approximately 150 ticks-per-second over 20 seconds.
		public Sample[] Samples;

		private void InitializeData()
		{
			CurrentSampleIndex = 0;
			Samples = new Sample[SampleBufferSize];

			for (int i = 0; i < Samples.Length; i++)
			{
				Samples[i] = GraphPlotting.Sample.Default;
			}
		}

		#endregion

		#region Data - Add

		public void Sample(float value)
		{
			Sample(value, Time.time);
		}

		public void Sample(float value, float time)
		{
			CheckClosed();

			Samples[CurrentSampleIndex] = new Sample(time, value);

			Graph.InformNewEntry(value, time);

			CurrentSampleIndex = (CurrentSampleIndex + 1) % SampleBufferSize;
		}

		#endregion

		#region Data - Get

		public void GetValueRangeInTimeWindow(float timeStart, float timeEnd, out float min, out float max)
		{
			CheckClosed();

			min = float.PositiveInfinity;
			max = float.NegativeInfinity;

			for (int i = 0; i < Samples.Length; i++)
			{
				var time = Samples[i].AxisX;
				if (time >= timeStart && time <= timeEnd)
				{
					var value = Samples[i].AxisY;
					if (min > value)
						min = value;
					if (max < value)
						max = value;
				}
			}
		}

		public void GetMinMaxTime(out float timeStart, out float timeEnd)
		{
			CheckClosed();

			var start = float.PositiveInfinity;
			var end = float.NegativeInfinity;

			for (int i = 0; i < Samples.Length; i++)
			{
				var time = Samples[i].AxisX;
				if (float.IsNaN(time))
					continue;

				if (start > time)
					start = time;
				if (end < time)
					end = time;
			}

			timeStart = start;
			timeEnd = end;
		}

		#endregion

		#region Tools

		public static void SetupChannel(bool activeCondition, Graph graph, ref Channel channel, string channelName, Color channelColor)
		{
			if (activeCondition)
			{
				if (channel == null)
				{
					// Try to find a channel with the same Name in graph
					channel = graph.GetChannelByName(channelName);

					if (channel == null)
					{
						channel = new Channel(graph, channelName, channelColor);
					}
				}
				else
				{
					channel.Description = channelName;
					channel.Color = channelColor;
				}
			}
			else
			{
				SafeClose(ref channel);
			}
		}

		#endregion
	}

}
