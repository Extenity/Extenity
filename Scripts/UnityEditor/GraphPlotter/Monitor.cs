using System.Collections.Generic;
using UnityEngine;

namespace Extenity.UnityEditorToolbox.GraphPlotting
{

	public enum SampleTime
	{
		FixedUpdate,
		Update,

		/// <summary>
		/// Need to manually call Sample.
		/// </summary>
		Custom
	}

	// TODO: Rename to ValueAxisRange
	public enum ValueAxisMode
	{
		Fixed,
		Expansive,
		Adaptive
	};

	public class Monitor
	{
		public string Name;
		public GameObject GameObject = null;

		public ValueAxisMode Mode = ValueAxisMode.Expansive;
		public float Min = float.PositiveInfinity;
		public float Max = float.NegativeInfinity;

		public float LatestTime = 0f;

		private readonly TagEntryTimeComparer _EventComparer = new TagEntryTimeComparer();

		#region Initialization

		public Monitor(string name, GameObject gameObject = null)
		{
			Name = name;
			GameObject = gameObject;

			GraphPlotters.Register(this);
		}

		#endregion

		#region Deinitialization

		public void Close()
		{
			GraphPlotters.Deregister(this);
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

		private TagEntry _LookupEvent = new TagEntry();

		public void Add(TagEntry entry)
		{
			int index = Tags.BinarySearch(entry, _EventComparer);
			if (index < 0)
			{
				index = ~index;
			}

			Tags.Insert(index, entry);
		}

		public void GetTagEntries(float minTime, float maxTime, List<TagEntry> result)
		{
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

		#endregion

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
	}

}