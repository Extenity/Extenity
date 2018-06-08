using UnityEngine;
using System.Collections.Generic;

namespace Extenity.UnityEditorToolbox.GraphPlotting
{

	public enum ValueAxisMode
	{
		Fixed,
		Expansive,
		Adaptive
	};

	public class MonitorEvent
	{
		public float time;
		public string text;
	}

	public class Monitor
	{
		public string Name;
		public ValueAxisMode Mode = ValueAxisMode.Expansive;
		public float LatestTime = 0f;

		public float Min = float.PositiveInfinity;
		public float Max = float.NegativeInfinity;

		public List<MonitorInput> Inputs = new List<MonitorInput>();
		public List<MonitorEvent> Events = new List<MonitorEvent>();

		private MonitorEventComparer EventComparer = new MonitorEventComparer();

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

		public void Add(MonitorInput monitorInput)
		{
			Inputs.Add(monitorInput);
		}

		public void Add(MonitorEvent monitorEvent)
		{
			int index = Events.BinarySearch(monitorEvent, EventComparer);
			if (index < 0)
			{
				index = ~index;
			}

			Events.Insert(index, monitorEvent);
		}

		private MonitorEvent _LookupEvent = new MonitorEvent();

		public void GetEvents(float minTime, float maxTime, List<MonitorEvent> dest)
		{
			if (Events.Count == 0)
			{
				return;
			}

			_LookupEvent.time = minTime;
			int index = Events.BinarySearch(_LookupEvent, EventComparer);

			if (index < 0)
			{
				index = ~index;
			}

			for (int i = index; i < Events.Count; i++)
			{
				var monitorEvent = Events[i];

				if (monitorEvent.time <= maxTime)
				{
					dest.Add(monitorEvent);
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

			foreach (var entry in Events)
			{
				if (minTime > entry.time)
					minTime = entry.time;
				if (maxTime < entry.time)
					maxTime = entry.time;
			}

			foreach (var entry in Inputs)
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

		public void Remove(MonitorInput monitorInput)
		{
			Inputs.Remove(monitorInput);
		}

		public void Close()
		{
			GraphPlotters.Deregister(this);
		}

		private class MonitorEventComparer : IComparer<MonitorEvent>
		{
			public int Compare(MonitorEvent a, MonitorEvent b)
			{
				return a.time.CompareTo(b.time);
			}
		}

	}

}