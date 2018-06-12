using System;
using System.Collections.Generic;
using UnityEngine;

namespace Extenity.UnityEditorToolbox.GraphPlotting
{

	public class Graph
	{
		public string Title;
		public GameObject Context = null;

		public readonly ValueAxisRangeConfiguration Range = new ValueAxisRangeConfiguration(ValueAxisSizing.Adaptive, float.PositiveInfinity, float.NegativeInfinity);

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
				float entryMin;
				float entryMax;
				Channels[i].GetMinMaxTime(out entryMin, out entryMax);

				if (minTime > entryMin)
					minTime = entryMin;
				if (maxTime < entryMax)
					maxTime = entryMax;
			}
		}

		public void CalculateValueAxisRangeInTimeWindow(float timeStart, float timeEnd)
		{
			CheckClosed();

			var min = float.PositiveInfinity;
			var max = float.NegativeInfinity;

			for (var i = 0; i < Channels.Count; i++)
			{
				float channelMin, channelMax;
				Channels[i].GetValueRangeInTimeWindow(timeStart, timeEnd, out channelMin, out channelMax);
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

		public static void SetupGraphWithSingleChannel(bool activeCondition,
			ref Graph graph, string graphTitle, GameObject graphContext, ValueAxisRangeConfiguration rangeConfiguration,
			ref Channel channel, string channelName, Color channelColor)
		{
			if (activeCondition)
			{
				if (graph == null)
				{
					graph = new Graph(graphTitle, graphContext);
					graph.SetRangeConfiguration(rangeConfiguration);
				}

				if (channel == null)
				{
					channel = new Channel(graph, channelName, channelColor);
				}
			}
			else
			{
				channel = null;
				Graph.SafeClose(ref graph);
			}
		}

		#endregion
	}

}