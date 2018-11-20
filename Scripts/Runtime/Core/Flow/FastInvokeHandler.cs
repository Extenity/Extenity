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
			public bool UnscaledTime;

			public void Reset()
			{
				NextTime = 0f;
				Behaviour = null;
				Action = null;
				RepeatRate = 0f;
				UnscaledTime = false;
			}

			public void Initialize(Behaviour behaviour, Action action, double invokeTime, double repeatRate, bool unscaledTime)
			{
				var now = unscaledTime
					? (double)Time.unscaledTime
					: (double)Time.time;

				NextTime = now + invokeTime;
				Behaviour = behaviour;
				Action = action;
				RepeatRate = repeatRate;
				UnscaledTime = unscaledTime;
			}
		}

		#region Update

		private void FixedUpdate()
		{
			var now = (double)Time.time;

			while (ScaledInvokeQueue.Count > 0)
			{
				var entry = ScaledInvokeQueue[0];
				if (now >= entry.NextTime)
				{
					// Remove entry if the related object does not exist anymore.
					if (!entry.Behaviour)
					{
						ScaledInvokeQueue.RemoveAt(0);
						PoolEntry(entry);
						continue;
					}

					// It's time to do the action
					if (entry.Behaviour.isActiveAndEnabled)
					{
						try
						{
							entry.Action();
						}
						catch (Exception exception)
						{
							Log.Exception(exception);
						}
					}

					// Remove from queue. But first check if the user manually removed it or not.
					if (ScaledInvokeQueue.Count > 0 && entry == ScaledInvokeQueue[0])
					{
						ScaledInvokeQueue.RemoveAt(0);

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

		private void Update()
		{
			var now = (double)Time.unscaledTime;

			while (UnscaledInvokeQueue.Count > 0)
			{
				var entry = UnscaledInvokeQueue[0];
				if (now >= entry.NextTime)
				{
					// Remove entry if the related object does not exist anymore.
					if (!entry.Behaviour)
					{
						UnscaledInvokeQueue.RemoveAt(0);
						PoolEntry(entry);
						continue;
					}

					// It's time to do the action
					if (entry.Behaviour.isActiveAndEnabled)
					{
						try
						{
							entry.Action();
						}
						catch (Exception exception)
						{
							Log.Exception(exception);
						}
					}

					// Remove from queue. But first check if the user manually removed it or not.
					if (UnscaledInvokeQueue.Count > 0 && entry == UnscaledInvokeQueue[0])
					{
						UnscaledInvokeQueue.RemoveAt(0);

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

		#endregion

		#region Queue

		public readonly List<InvokeEntry> ScaledInvokeQueue = new List<InvokeEntry>(100);
		public readonly List<InvokeEntry> UnscaledInvokeQueue = new List<InvokeEntry>(100);

		private void AddToQueue(InvokeEntry entry)
		{
			if (entry.UnscaledTime)
				_InsertIntoUpdateQueue(entry);
			else
				_InsertIntoFixedUpdateQueue(entry);
		}

		private void _InsertIntoFixedUpdateQueue(InvokeEntry entry)
		{
			if (ScaledInvokeQueue.Count != 0)
			{
				var nextTime = entry.NextTime;
				for (int i = 0; i < ScaledInvokeQueue.Count; i++)
				{
					if (nextTime < ScaledInvokeQueue[i].NextTime)
					{
						ScaledInvokeQueue.Insert(i, entry);
						return;
					}
				}
			}
			ScaledInvokeQueue.Add(entry);
		}

		private void _InsertIntoUpdateQueue(InvokeEntry entry)
		{
			if (UnscaledInvokeQueue.Count != 0)
			{
				var nextTime = entry.NextTime;
				for (int i = 0; i < UnscaledInvokeQueue.Count; i++)
				{
					if (nextTime < UnscaledInvokeQueue[i].NextTime)
					{
						UnscaledInvokeQueue.Insert(i, entry);
						return;
					}
				}
			}
			UnscaledInvokeQueue.Add(entry);
		}

		#endregion

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

		private void RepeatIfNeededOrPool(InvokeEntry entry)
		{
			if (entry.RepeatRate > 0.0)
			{
				entry.NextTime += entry.RepeatRate;
				AddToQueue(entry);
			}
			else
			{
				PoolEntry(entry);
			}
		}

		#endregion

		#region Invoke / Cancel

		internal void Invoke(Behaviour behaviour, Action action, double time, double repeatRate, bool unscaledTime)
		{
			if (!behaviour)
			{
				Log.CriticalError("Tried to invoke for a null behaviour.");
				return;
			}
			var entry = CreateOrGetEntryPool();
			entry.Initialize(behaviour, action, time, repeatRate, unscaledTime);
			AddToQueue(entry);
		}

		internal void Cancel(Behaviour behaviour, Action action)
		{
			if (!behaviour)
			{
				Log.CriticalError("Tried to cancel fast invoke of a null behaviour.");
				return;
			}
			for (int i = 0; i < ScaledInvokeQueue.Count; i++)
			{
				var entry = ScaledInvokeQueue[i];
				if (entry.Behaviour == behaviour && entry.Action == action)
				{
					ScaledInvokeQueue.RemoveAt(i);
					i--;
					PoolEntry(entry);
					//break; Do not break. There may be more than one.
				}
			}
			for (int i = 0; i < UnscaledInvokeQueue.Count; i++)
			{
				var entry = UnscaledInvokeQueue[i];
				if (entry.Behaviour == behaviour && entry.Action == action)
				{
					UnscaledInvokeQueue.RemoveAt(i);
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
				Log.CriticalError("Tried to cancel fast invoke of a null behaviour.");
				return;
			}
			for (int i = 0; i < ScaledInvokeQueue.Count; i++)
			{
				var entry = ScaledInvokeQueue[i];
				if (entry.Behaviour == behaviour)
				{
					ScaledInvokeQueue.RemoveAt(i);
					i--;
					PoolEntry(entry);
					//break; Do not break. There may be more than one.
				}
			}
			for (int i = 0; i < UnscaledInvokeQueue.Count; i++)
			{
				var entry = UnscaledInvokeQueue[i];
				if (entry.Behaviour == behaviour)
				{
					UnscaledInvokeQueue.RemoveAt(i);
					i--;
					PoolEntry(entry);
					//break; Do not break. There may be more than one.
				}
			}
		}

		#endregion

		#region Information

		internal bool IsInvoking(Behaviour behaviour, Action action)
		{
			if (!behaviour)
			{
				Log.CriticalError("Tried to query fast invoke of a null behaviour.");
				return false;
			}
			for (int i = 0; i < ScaledInvokeQueue.Count; i++)
			{
				var entry = ScaledInvokeQueue[i];
				if (entry.Behaviour == behaviour && entry.Action == action)
				{
					return true;
				}
			}
			for (int i = 0; i < UnscaledInvokeQueue.Count; i++)
			{
				var entry = UnscaledInvokeQueue[i];
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
				Log.CriticalError("Tried to query fast invoke of a null behaviour.");
				return false;
			}
			for (int i = 0; i < ScaledInvokeQueue.Count; i++)
			{
				if (ScaledInvokeQueue[i].Behaviour == behaviour)
				{
					return true;
				}
			}
			for (int i = 0; i < UnscaledInvokeQueue.Count; i++)
			{
				if (UnscaledInvokeQueue[i].Behaviour == behaviour)
				{
					return true;
				}
			}
			return false;
		}

		internal bool IsInvokingAny()
		{
			return ScaledInvokeQueue.Count > 0 || UnscaledInvokeQueue.Count > 0;
		}

		internal int TotalInvokeCount()
		{
			return ScaledInvokeQueue.Count + UnscaledInvokeQueue.Count;
		}

		internal int InvokeCount(Behaviour behaviour)
		{
			if (!behaviour)
			{
				Log.CriticalError("Tried to query fast invoke of a null behaviour.");
				return -1;
			}
			var count = 0;
			for (int i = 0; i < ScaledInvokeQueue.Count; i++)
			{
				if (ScaledInvokeQueue[i].Behaviour == behaviour)
				{
					count++;
				}
			}
			for (int i = 0; i < UnscaledInvokeQueue.Count; i++)
			{
				if (UnscaledInvokeQueue[i].Behaviour == behaviour)
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
				Log.CriticalError("Tried to query fast invoke of a null behaviour.");
				return -1;
			}
			var count = 0;
			for (int i = 0; i < ScaledInvokeQueue.Count; i++)
			{
				var entry = ScaledInvokeQueue[i];
				if (entry.Behaviour == behaviour && entry.Action == action)
				{
					count++;
				}
			}
			for (int i = 0; i < UnscaledInvokeQueue.Count; i++)
			{
				var entry = UnscaledInvokeQueue[i];
				if (entry.Behaviour == behaviour && entry.Action == action)
				{
					count++;
				}
			}
			return count;
		}

		internal double RemainingTimeUntilNextInvoke(Behaviour behaviour, Action action)
		{
			if (!behaviour)
			{
				Log.CriticalError("Tried to query fast invoke of a null behaviour.");
				return double.NaN;
			}
			double now = (double)Time.time;
			for (int i = 0; i < ScaledInvokeQueue.Count; i++)
			{
				var entry = ScaledInvokeQueue[i];
				if (entry.Behaviour == behaviour && entry.Action == action)
				{
					return entry.NextTime - now;
				}
			}
			return double.NaN;
		}

		internal double RemainingTimeUntilNextUnscaledInvoke(Behaviour behaviour, Action action)
		{
			if (!behaviour)
			{
				Log.CriticalError("Tried to query fast invoke of a null behaviour.");
				return double.NaN;
			}
			double now = (double)Time.unscaledTime;
			for (int i = 0; i < UnscaledInvokeQueue.Count; i++)
			{
				var entry = UnscaledInvokeQueue[i];
				if (entry.Behaviour == behaviour && entry.Action == action)
				{
					return entry.NextTime - now;
				}
			}
			return double.NaN;
		}

		internal double RemainingTimeUntilNextInvoke(Behaviour behaviour)
		{
			if (!behaviour)
			{
				Log.CriticalError("Tried to query fast invoke of a null behaviour.");
				return double.NaN;
			}
			double now = (double)Time.time;
			for (int i = 0; i < ScaledInvokeQueue.Count; i++)
			{
				var entry = ScaledInvokeQueue[i];
				if (entry.Behaviour == behaviour)
				{
					return entry.NextTime - now;
				}
			}
			return double.NaN;
		}

		internal double RemainingTimeUntilNextUnscaledInvoke(Behaviour behaviour)
		{
			if (!behaviour)
			{
				Log.CriticalError("Tried to query fast invoke of a null behaviour.");
				return double.NaN;
			}
			double now = (double)Time.unscaledTime;
			for (int i = 0; i < UnscaledInvokeQueue.Count; i++)
			{
				var entry = UnscaledInvokeQueue[i];
				if (entry.Behaviour == behaviour)
				{
					return entry.NextTime - now;
				}
			}
			return double.NaN;
		}

		#endregion
	}

}
