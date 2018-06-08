// ============================================================================
//   Monitor Components v. 1.04 - written by Peter Bruun (twitter.com/ptrbrn)
//   More info on Asset Store: http://u3d.as/9MW
// ============================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MonitorComponents 
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
		private string name;
		private ValueAxisMode mode = ValueAxisMode.Expansive;
		public float latestTime = 0f;

		private float min = float.PositiveInfinity;
		private float max = float.NegativeInfinity;

		public List<MonitorInput> inputs = new List<MonitorInput>();
		public List<MonitorEvent> events = new List<MonitorEvent>();

		private MonitorEventComparer monitorEventComparer = new MonitorEventComparer();

		private GameObject gameObject = null;

		public Monitor(string name)
		{
			this.name = name;

			Monitors.Instance.Add(this);
		}

		public void Resize(float value, float time)
		{
			latestTime = time;

			if(mode == ValueAxisMode.Expansive)
			{
				min = Mathf.Min(min, value);
				max = Mathf.Max(max, value);
			}
			else
			{
				// Do nothing - stay fixed.
			}
		}

		public void Add(MonitorInput monitorInput)
		{
			inputs.Add(monitorInput);
		}

		public void Add(MonitorEvent monitorEvent)
		{
			int index = events.BinarySearch(monitorEvent, monitorEventComparer);
			if (index < 0)
			{
				index = ~index;
			}

			events.Insert(index, monitorEvent);
		}

		private MonitorEvent lookupEvent = new MonitorEvent();

		public void GetEvents(float minTime, float maxTime, List<MonitorEvent> dest)
		{
			if (events.Count == 0)
			{
				return;
			}

			lookupEvent.time = minTime;
			int index = events.BinarySearch(lookupEvent, monitorEventComparer);

			if (index < 0)
			{
				index = ~index;
			}

			for (int i = index; i < events.Count; i++)
			{
				MonitorEvent monitorEvent = events[i];

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

			foreach(MonitorEvent monitorEvent in events)
			{
				minTime = Mathf.Min(minTime, monitorEvent.time);
				maxTime = Mathf.Max(maxTime, monitorEvent.time);	
			}

			foreach(MonitorInput monitorInput in inputs)
			{
				float monitorInputMin;
				float monitorInputMax;

				monitorInput.GetMinMaxTime(out monitorInputMin, out monitorInputMax);

				minTime = Mathf.Min(minTime, monitorInputMin);
				maxTime = Mathf.Max(maxTime, monitorInputMax);
			}
		}

		public void MoveForward(float time)
		{
			latestTime = time;
		}

		public void Remove(MonitorInput monitorInput)
		{
			inputs.Remove(monitorInput);
		}

		public void Close()
		{
			Monitors.Instance.Remove(this);
		}

		public float Min 
		{
			get 
			{
				return min;
			}

			set 
			{
				min = value;
			}
		}

		public float Max
		{
			get 
			{
				return max;
			}

			set 
			{
				max = value;
			}
		}

		public string Name 
		{
			get 
			{
				return name;
			}

			set 
			{
				name = value;
			}
		}

		public ValueAxisMode Mode 
		{
			get 
			{
				return mode;
			}

			set 
			{
				mode = value;
			}
		}

		public GameObject GameObject 
		{
			get {
				return gameObject;
			}
			set {
				gameObject = value;
			}

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