using System.Collections.Generic;
using UnityEngine;

namespace Extenity.UnityEditorToolbox.GraphPlotting
{

	public enum ValueAxisMode
	{
		Fixed,
		Expansive,
		Adaptive
	};

	public class Monitor
	{
		public string Name;
		public ValueAxisMode Mode = ValueAxisMode.Expansive;
		public float LatestTime = 0f;

		public float Min = float.PositiveInfinity;
		public float Max = float.NegativeInfinity;

		public List<Channel> Channels = new List<Channel>();
		public List<TagEntry> Tags = new List<TagEntry>();

		private TagEntryTimeComparer EventComparer = new TagEntryTimeComparer();

		public GameObject GameObject = null;

		public Monitor(string name, GameObject gameObject = null)
		{
			Name = name;
			GameObject = gameObject;

			GraphPlotters.Register(this);
		}

		public void Resize(float value, float time)
		{
			LatestTime = time;

			if (Mode == ValueAxisMode.Expansive)
			{
				Min = Mathf.Min(Min, value);
				Max = Mathf.Max(Max, value);
			}
			else
			{
				// Do nothing - stay fixed.
			}
		}

		public void Add(Channel channel)
		{
			Channels.Add(channel);
		}

		public void Add(TagEntry entry)
		{
			int index = Tags.BinarySearch(entry, EventComparer);
			if (index < 0)
			{
				index = ~index;
			}

			Tags.Insert(index, entry);
		}

		private TagEntry _LookupEvent = new TagEntry();

		public void GetTagEntries(float minTime, float maxTime, List<TagEntry> result)
		{
			if (Tags.Count == 0)
				return;

			// Find starting index
			_LookupEvent.Time = minTime;
			int index = Tags.BinarySearch(_LookupEvent, EventComparer);
			if (index < 0)
			{
				index = ~index;
			}

			for (int i = index; i < Tags.Count; i++)
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

		public void GetMinMaxTime(out float minTime, out float maxTime)
		{
			minTime = float.PositiveInfinity;
			maxTime = float.NegativeInfinity;

			foreach (var entry in Tags)
			{
				if (minTime > entry.Time)
					minTime = entry.Time;
				if (maxTime < entry.Time)
					maxTime = entry.Time;
			}

			foreach (var entry in Channels)
			{
				float entryMin;
				float entryMax;
				entry.GetMinMaxTime(out entryMin, out entryMax);

				if (minTime > entryMin)
					minTime = entryMin;
				if (maxTime < entryMax)
					maxTime = entryMax;
			}
		}

		public void MoveForward(float time)
		{
			LatestTime = time;
		}

		public void Remove(Channel channel)
		{
			Channels.Remove(channel);
		}

		public void Close()
		{
			GraphPlotters.Deregister(this);
		}

	}

}