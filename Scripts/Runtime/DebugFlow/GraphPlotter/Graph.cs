using System;
using System.Collections.Generic;
using Extenity.DataToolbox;
using UnityEngine;

namespace Extenity.DebugFlowTool.GraphPlotting
{

	public class Graph
	{
		#region Initialization

		public Graph(string title, GameObject context = null)
		{
			Title = title;
			Context = context;

			Graphs.Register(this);
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

			// Close all channels first.
			Channel.SafeClose(Channels);

			Graphs.Deregister(this);
		}

		public static void SafeClose(ref Graph graph)
		{
			if (graph != null)
			{
				graph.Close();
				graph = null;
			}
		}

		public static void SafeClose(ref Graph[] graphs)
		{
			if (graphs != null)
			{
				for (int i = 0; i < graphs.Length; i++)
					graphs[i].Close();
				graphs = null;
			}
		}

		public static void SafeClose(IList<Graph> graphs)
		{
			if (graphs != null)
			{
				for (int i = 0; i < graphs.Count; i++)
					graphs[i].Close();
				graphs.Clear();
			}
		}

		public void CheckClosed()
		{
			if (_IsClosed)
			{
				throw new Exception($"Tried to do an operation on closed graph '{Title}'.");
			}
		}

		#endregion

		#region Metadata and Configuration

		public string Title;
		public GameObject Context = null;
		
		public readonly ValueAxisRangeConfiguration Range = ValueAxisRangeConfiguration.CreateAdaptive();

		#endregion

		#region Channels

		public readonly List<Channel> Channels = new List<Channel>();

		internal void RegisterChannel(Channel channel)
		{
			Channels.Add(channel);
		}

		internal void DeregisterChannel(Channel channel)
		{
			Channels.Remove(channel);
		}

		#endregion

		#region Tags

		public readonly List<TagEntry> Tags = new List<TagEntry>();

		private readonly TagEntry _LookupEvent = new TagEntry();
		private readonly TagEntryTimeComparer _EventComparer = new TagEntryTimeComparer();

		public void Add(TagEntry entry)
		{
			CheckClosed();

			var index = Tags.BinarySearch(entry, _EventComparer);
			if (index < 0)
			{
				index = ~index;
			}

			Tags.Insert(index, entry);
		}

		public void GetTagEntries(float minTime, float maxTime, List<TagEntry> result)
		{
			CheckClosed();

			if (Tags.Count == 0)
				return;

			// Find starting index
			_LookupEvent.Time = minTime;
			var index = Tags.BinarySearch(_LookupEvent, _EventComparer);
			if (index < 0)
			{
				index = ~index;
			}

			// Add all entries until maxTime
			for (var i = index; i < Tags.Count; i++)
			{
				var entry = Tags[i];

				if (entry.Time <= maxTime)
				{
					result.Add(entry);
				}
				else
				{
					break;
				}
			}
		}

		#endregion

		#region Timing

		public float LatestTime = 0f;

		public void SetTimeCursor(float time)
		{
			CheckClosed();

			LatestTime = time;
		}

		#endregion

		internal void InformNewEntry(float value, float time)
		{
			CheckClosed();

			SetTimeCursor(time);

			switch (Range.Sizing)
			{
				case ValueAxisSizing.Expansive:
					Range.Expand(value);
					break;
				case ValueAxisSizing.Fixed:
				case ValueAxisSizing.Adaptive:
				case ValueAxisSizing.ZeroBasedAdaptive:
					// Nothing to do.
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void GetMinMaxTime(out float minTime, out float maxTime)
		{
			CheckClosed();

			minTime = float.PositiveInfinity;
			maxTime = float.NegativeInfinity;

			for (var i = 0; i < Tags.Count; i++)
			{
				var entry = Tags[i];
				if (minTime > entry.Time)
					minTime = entry.Time;
				if (maxTime < entry.Time)
					maxTime = entry.Time;
			}

			for (var i = 0; i < Channels.Count; i++)
			{
				Channels[i].GetMinMaxTime(out var entryMin, out var entryMax);

				if (minTime > entryMin)
					minTime = entryMin;
				if (maxTime < entryMax)
					maxTime = entryMax;
			}
		}

		public void CalculateValueAxisRangeInTimeWindow(float timeStart, float timeEnd, float min, float max)
		{
			CheckClosed();

			for (var i = 0; i < Channels.Count; i++)
			{
				Channels[i].GetValueRangeInTimeWindow(timeStart, timeEnd, out var channelMin, out var channelMax);
				if (min > channelMin)
					min = channelMin;
				if (max < channelMax)
					max = channelMax;
			}

			Range.Min = min;
			Range.Max = max;
		}

		public void SetRangeConfiguration(ValueAxisRangeConfiguration range)
		{
			CheckClosed();

			Range.CopyFrom(range);
		}

		#region Tools

		public static void SetupGraph(bool activeCondition,
			ref Graph graph, string graphTitle, GameObject graphContext, ValueAxisRangeConfiguration rangeConfiguration)
		{
			if (activeCondition)
			{
				if (graph == null)
				{
					graph = new Graph(graphTitle, graphContext);
				}
				else
				{
					graph.Title = graphTitle;
					graph.Context = graphContext;
				}

				graph.SetRangeConfiguration(rangeConfiguration);
			}
			else
			{
				SafeClose(ref graph);
			}
		}

		public static void SetupGraphWithSingleChannel(bool activeCondition,
			ref Graph graph, string graphTitle, GameObject graphContext, ValueAxisRangeConfiguration rangeConfiguration,
			ref Channel channel, string channelName, Color channelColor)
		{
			SetupGraph(activeCondition, ref graph, graphTitle, graphContext, rangeConfiguration);
			Channel.SetupChannel(activeCondition, graph, ref channel, channelName, channelColor);
		}

		public static void SetupGraphWithXYZChannels(bool activeCondition,
			ref Graph graph, string graphTitle, GameObject graphContext, ValueAxisRangeConfiguration rangeConfiguration,
			ref Channel[] channels, bool channelActiveX, bool channelActiveY, bool channelActiveZ,
			string channelNameX, string channelNameY, string channelNameZ, Color channelColorX, Color channelColorY, Color channelColorZ)
		{
			SetupGraphWithXYZChannels(activeCondition, ref graph, graphTitle, graphContext, rangeConfiguration, ref channels, channelActiveX, channelActiveY, channelActiveZ);
			if (activeCondition)
			{
				channels[0].Description = channelNameX;
				channels[1].Description = channelNameY;
				channels[2].Description = channelNameZ;
				channels[0].Color = channelColorX;
				channels[1].Color = channelColorY;
				channels[2].Color = channelColorZ;
			}
		}

		public static void SetupGraphWithXYZChannels(bool activeCondition,
			ref Graph graph, string graphTitle, GameObject graphContext, ValueAxisRangeConfiguration rangeConfiguration,
			ref Channel[] channels, bool channelActiveX, bool channelActiveY, bool channelActiveZ)
		{
			CollectionTools.ResizeIfRequired(ref channels, 3);
			SetupGraph(activeCondition, ref graph, graphTitle, graphContext, rangeConfiguration);
			Channel.SetupChannel(activeCondition && channelActiveX, graph, ref channels[0], "x", PlotColors.Red);
			Channel.SetupChannel(activeCondition && channelActiveY, graph, ref channels[1], "y", PlotColors.Green);
			Channel.SetupChannel(activeCondition && channelActiveZ, graph, ref channels[2], "z", PlotColors.Blue);
		}

		public static void SetupGraphWithXYChannels(bool activeCondition,
			ref Graph graph, string graphTitle, GameObject graphContext, ValueAxisRangeConfiguration rangeConfiguration,
			ref Channel[] channels, bool channelActiveX, bool channelActiveY,
			string channelNameX, string channelNameY, Color channelColorX, Color channelColorY)
		{
			SetupGraphWithXYChannels(activeCondition, ref graph, graphTitle, graphContext, rangeConfiguration, ref channels, channelActiveX, channelActiveY);
			if (activeCondition)
			{
				channels[0].Description = channelNameX;
				channels[1].Description = channelNameY;
				channels[0].Color = channelColorX;
				channels[1].Color = channelColorY;
			}
		}

		public static void SetupGraphWithXYChannels(bool activeCondition,
			ref Graph graph, string graphTitle, GameObject graphContext, ValueAxisRangeConfiguration rangeConfiguration,
			ref Channel[] channels, bool channelActiveX, bool channelActiveY)
		{
			CollectionTools.ResizeIfRequired(ref channels, 2);
			SetupGraph(activeCondition, ref graph, graphTitle, graphContext, rangeConfiguration);
			Channel.SetupChannel(activeCondition && channelActiveX, graph, ref channels[0], "x", PlotColors.Red);
			Channel.SetupChannel(activeCondition && channelActiveY, graph, ref channels[1], "y", PlotColors.Green);
		}

		#endregion
	}

}