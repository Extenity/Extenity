using System;
using System.Collections.Generic;
using UnityEngine;

namespace Extenity.FlowToolbox
{

	public class FastInvokeHandler : MonoBehaviour
	{
		public class InvokeEntry
		{
			public double NextTime;
			public Behaviour Behaviour;
			public Action Action;
			public double RepeatRate;

			public void Reset()
			{
				NextTime = 0f;
				Behaviour = null;
				Action = null;
				RepeatRate = 0f;
			}

			public void Initialize(Behaviour behaviour, Action action, double time, double repeatRate)
			{
				NextTime = time;
				Behaviour = behaviour;
				Action = action;
				RepeatRate = repeatRate;
			}
		}

		public readonly List<InvokeEntry> FixedUpdateInvokes = new List<InvokeEntry>(100);

		private void FixedUpdate()
		{
			var now = (double)Time.time;

			while (FixedUpdateInvokes.Count > 0)
			{
				var entry = FixedUpdateInvokes[0];
				//Debug.LogFormat("now: {0}      \tnexttime: {1}", now, entry.NextTime);
				if (now >= entry.NextTime)
				{
					// Remove entry if the related object does not exist anymore.
					if (!entry.Behaviour)
					{
						FixedUpdateInvokes.RemoveAt(0);
						PoolEntry(entry);
						continue;
					}

					// It's time to do the action
					if (entry.Behaviour.isActiveAndEnabled)
					{
						entry.Action();
					}

					// Remove from queue. But first check if the user manually removed it or not.
					if (entry == FixedUpdateInvokes[0])
					{
						FixedUpdateInvokes.RemoveAt(0);

						// Repeat or delete
						RepeatIfNeededOrPool(entry);
					}
				}
				else
				{
					break;
				}
			}
		}

		private void RepeatIfNeededOrPool(InvokeEntry entry)
		{
			if (entry.RepeatRate > 0.0)
			{
				entry.NextTime += entry.RepeatRate;
				AddToFixedUpdateInvokes(entry);
			}
			else
			{
				PoolEntry(entry);
			}
		}

		private void AddToFixedUpdateInvokes(InvokeEntry entry)
		{
			if (FixedUpdateInvokes.Count != 0)
			{
				var nextTime = entry.NextTime;
				for (int i = 0; i < FixedUpdateInvokes.Count; i++)
				{
					if (nextTime < FixedUpdateInvokes[i].NextTime)
					{
						FixedUpdateInvokes.Insert(i, entry);
						return;
					}
				}
			}

			FixedUpdateInvokes.Add(entry);
		}

		#region Entry Pooling

		private const int MaxEntryPoolSize = 100;
		private static readonly List<InvokeEntry> EntryPool = new List<InvokeEntry>(MaxEntryPoolSize);

		public static InvokeEntry CreateOrGetEntryPool()
		{
			if (EntryPool.Count > 0)
			{
				var index = EntryPool.Count - 1;
				var entry = EntryPool[index];
				EntryPool.RemoveAt(index);
				return entry;
			}
			return new InvokeEntry();
		}

		public static void PoolEntry(InvokeEntry entry)
		{
			entry.Reset();

			if (EntryPool.Count < MaxEntryPoolSize)
			{
				EntryPool.Add(entry);
			}
		}

		#endregion

		internal void Launch(Behaviour behaviour, Action action, double time, double repeatRate)
		{
			if (!behaviour)
			{
				Debug.LogError("Tried to invoke for a null behaviour.");
				return;
			}
			var now = (double)Time.time;
			var entry = CreateOrGetEntryPool();
			entry.Initialize(behaviour, action, now + time, repeatRate);
			AddToFixedUpdateInvokes(entry);
		}

		internal void Cancel(Behaviour behaviour, Action action)
		{
			if (!behaviour)
			{
				Debug.LogError("Tried to cancel fast invoke of a null behaviour.");
				return;
			}
			for (int i = 0; i < FixedUpdateInvokes.Count; i++)
			{
				var entry = FixedUpdateInvokes[i];
				if (entry.Behaviour == behaviour && entry.Action == action)
				{
					FixedUpdateInvokes.RemoveAt(i);
					i--;
					PoolEntry(entry);
					//break; Do not break. There may be more than one.
				}
			}
		}

		internal void CancelAll(Behaviour behaviour)
		{
			if (!behaviour)
			{
				Debug.LogError("Tried to cancel fast invoke of a null behaviour.");
				return;
			}
			for (int i = 0; i < FixedUpdateInvokes.Count; i++)
			{
				var entry = FixedUpdateInvokes[i];
				if (entry.Behaviour == behaviour)
				{
					FixedUpdateInvokes.RemoveAt(i);
					i--;
					PoolEntry(entry);
					//break; Do not break. There may be more than one.
				}
			}
		}

		internal bool IsInvoking(Behaviour behaviour, Action action)
		{
			if (!behaviour)
			{
				Debug.LogError("Tried to query fast invoke of a null behaviour.");
				return false;
			}
			for (int i = 0; i < FixedUpdateInvokes.Count; i++)
			{
				var entry = FixedUpdateInvokes[i];
				if (entry.Behaviour == behaviour && entry.Action == action)
				{
					return true;
				}
			}
			return false;
		}

		internal bool IsInvoking(Behaviour behaviour)
		{
			if (!behaviour)
			{
				Debug.LogError("Tried to query fast invoke of a null behaviour.");
				return false;
			}
			for (int i = 0; i < FixedUpdateInvokes.Count; i++)
			{
				var entry = FixedUpdateInvokes[i];
				if (entry.Behaviour == behaviour)
				{
					return true;
				}
			}
			return false;
		}

		internal bool IsInvokingAny()
		{
			return FixedUpdateInvokes.Count > 0;
		}

		internal int TotalInvokeCount()
		{
			return FixedUpdateInvokes.Count;
		}

		internal int InvokeCount(Behaviour behaviour)
		{
			if (!behaviour)
			{
				Debug.LogError("Tried to query fast invoke of a null behaviour.");
				return -1;
			}
			var count = 0;
			for (int i = 0; i < FixedUpdateInvokes.Count; i++)
			{
				var entry = FixedUpdateInvokes[i];
				if (entry.Behaviour == behaviour)
				{
					count++;
				}
			}
			return count;
		}

		internal int InvokeCount(Behaviour behaviour, Action action)
		{
			if (!behaviour)
			{
				Debug.LogError("Tried to query fast invoke of a null behaviour.");
				return -1;
			}
			var count = 0;
			for (int i = 0; i < FixedUpdateInvokes.Count; i++)
			{
				var entry = FixedUpdateInvokes[i];
				if (entry.Behaviour == behaviour && entry.Action == action)
				{
					count++;
				}
			}
			return count;
		}

		// TODO: See if this can be optimized by returning the first found entry.
		internal double RemainingTimeUntilNextInvoke(Behaviour behaviour, Action action)
		{
			if (!behaviour)
			{
				Debug.LogError("Tried to query fast invoke of a null behaviour.");
				return double.NaN;
			}
			double now = (double)Time.time;
			double minimumRemaining = double.MaxValue;
			for (int i = 0; i < FixedUpdateInvokes.Count; i++)
			{
				var entry = FixedUpdateInvokes[i];
				if (entry.Behaviour == behaviour && entry.Action == action)
				{
					var remaining = entry.NextTime - now;
					if (minimumRemaining > remaining)
					{
						minimumRemaining = remaining;
					}
				}
			}
			return minimumRemaining;
		}

		// TODO: See if this can be optimized by returning the first found entry.
		internal double RemainingTimeUntilNextInvoke(Behaviour behaviour)
		{
			if (!behaviour)
			{
				Debug.LogError("Tried to query fast invoke of a null behaviour.");
				return double.NaN;
			}
			double now = (double)Time.time;
			double minimumRemaining = double.MaxValue;
			for (int i = 0; i < FixedUpdateInvokes.Count; i++)
			{
				var entry = FixedUpdateInvokes[i];
				if (entry.Behaviour == behaviour)
				{
					var remaining = entry.NextTime - now;
					if (minimumRemaining > remaining)
					{
						minimumRemaining = remaining;
					}
				}
			}
			return minimumRemaining == double.MaxValue ? double.NaN : minimumRemaining;
		}
	}

}
