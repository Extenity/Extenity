using System;
using System.Collections.Generic;
using UnityEngine;

namespace Extenity.FlowToolbox
{

	public class FastInvokeHandler : MonoBehaviour
	{
		// TODO: Consider turning this into a struct. Measure the performance impact of both. There is a heavy use of reference comparison with class implementation. Consider adding a unique ID field and use that for comparison when converting into struct.
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

				if (invokeTime < 0)
				{
					Log.Warning($"Received negative invoke time '{invokeTime}'.");
					invokeTime = 0;
				}
				if (repeatRate < 0)
				{
					Log.Warning($"Received negative repeat rate '{repeatRate}'.");
					repeatRate = 0;
				}

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
#if UNITY_EDITOR
			if (VerboseLoggingInEachFixedUpdate)
			{
				Log.DebugInfo(nameof(FastInvokeHandler) + "." + nameof(FixedUpdate));
			}
#endif

			var now = (double)Time.time;

			if (QueueInProcess.Count > 0)
			{
				Log.InternalError(7591128); // Definitely expected to be empty.
				QueueInProcess.Clear();
			}

			// Get all the entries of which their time has come.
			for (int i = 0; i < ScaledInvokeQueue.Count; i++)
			{
				var entry = ScaledInvokeQueue[i];
				if (now >= entry.NextTime)
				{
					QueueInProcess.Add(entry);
				}
				else
				{
					break;
				}
			}

			// Nothing to invoke yet.
			if (QueueInProcess.Count == 0)
				return;

			CurrentlyProcessingQueue = InvokeQueue.Scaled;

			// Remove the ones from queue that we will be calling now.
			ScaledInvokeQueue.RemoveRange(0, QueueInProcess.Count);

			for (int i = 0; i < QueueInProcess.Count; i++)
			{
				var entry = QueueInProcess[i];

				if (entry == null || // Check if the entry was removed by user, in the middle of the process. See 1765136.
					!entry.Behaviour) // Skip entry if the related object does not exist anymore.
				{
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

				// If the entry is a repeating one, add it into the queue again. But first, check if
				// the user manually removed the entry just now. In that case the entry should not be
				// set for repeating anymore. See 1765136.
				if (QueueInProcess[i] != null)
				{
					// Repeat or delete
					RepeatIfNeededOrPool(entry);
				}
			}

			QueueInProcess.Clear();
			CurrentlyProcessingQueue = InvokeQueue.Unspecified;
		}

		private void Update()
		{
#if UNITY_EDITOR
			if (VerboseLoggingInEachUpdate)
			{
				Log.DebugInfo(nameof(FastInvokeHandler) + "." + nameof(Update));
			}
#endif

			var now = (double)Time.unscaledTime;

			if (QueueInProcess.Count > 0)
			{
				Log.InternalError(7591128); // Definitely expected to be empty.
				QueueInProcess.Clear();
			}

			// Get all the entries of which their time has come.
			for (int i = 0; i < UnscaledInvokeQueue.Count; i++)
			{
				var entry = UnscaledInvokeQueue[i];
				if (now >= entry.NextTime)
				{
					QueueInProcess.Add(entry);
				}
				else
				{
					break;
				}
			}

			// Nothing to invoke yet.
			if (QueueInProcess.Count == 0)
				return;

			CurrentlyProcessingQueue = InvokeQueue.Unscaled;

			// Remove the ones from queue that we will be calling now.
			UnscaledInvokeQueue.RemoveRange(0, QueueInProcess.Count);

			for (int i = 0; i < QueueInProcess.Count; i++)
			{
				var entry = QueueInProcess[i];

				if (entry == null || // Check if the entry was removed by user, in the middle of the process. See 1765136.
					!entry.Behaviour) // Skip entry if the related object does not exist anymore.
				{
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

				// If the entry is a repeating one, add it into the queue again. But first, check if
				// the user manually removed the entry just now. In that case the entry should not be
				// set for repeating anymore. See 1765136.
				if (QueueInProcess[i] != null)
				{
					// Repeat or delete
					RepeatIfNeededOrPool(entry);
				}
			}

			QueueInProcess.Clear();
			CurrentlyProcessingQueue = InvokeQueue.Unspecified;
		}

		#endregion

		#region Queue

		public enum InvokeQueue
		{
			Unspecified,
			Scaled,
			Unscaled,
		}

		public InvokeQueue CurrentlyProcessingQueue = InvokeQueue.Unspecified;

		public readonly List<InvokeEntry> ScaledInvokeQueue = new List<InvokeEntry>(100);
		public readonly List<InvokeEntry> UnscaledInvokeQueue = new List<InvokeEntry>(100);

		/// <summary>
		/// A temporary buffer that contains currently processing entries. This is required so that modifying the queue
		/// in the middle of an ongoing process won't break things. The entries are immediately moved into this buffer
		/// from processed queue right at the start of the process. So an entry never exists both on this temporary buffer
		/// and on it's own queue.
		/// </summary>
		private readonly List<InvokeEntry> QueueInProcess = new List<InvokeEntry>(100);

		private bool IsQueueInProcessContainsAnyNonNull
		{
			get
			{
				for (var i = 0; i < QueueInProcess.Count; i++)
				{
					if (QueueInProcess[i] != null)
						return true;
				}
				return false;
			}
		}

		private int QueueInProcessNonNullCount
		{
			get
			{
				var count = 0;
				for (var i = 0; i < QueueInProcess.Count; i++)
				{
					if (QueueInProcess[i] != null)
						count++;
				}
				return count;
			}
		}

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
			for (int i = 0; i < QueueInProcess.Count; i++)
			{
				var entry = QueueInProcess[i];
				if (entry != null && entry.Behaviour == behaviour && entry.Action == action)
				{
					//QueueInProcess.RemoveAt(i); Do not remove! Make it null instead. The process loop depends on QueueInProcess entries not being added or removed in the middle of process. See 1765136.
					QueueInProcess[i] = null;
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
			for (int i = 0; i < QueueInProcess.Count; i++)
			{
				var entry = QueueInProcess[i];
				if (entry != null && entry.Behaviour == behaviour)
				{
					//QueueInProcess.RemoveAt(i); Do not remove! Make it null instead. The process loop depends on QueueInProcess entries not being added or removed in the middle of process. See 1765136.
					QueueInProcess[i] = null;
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
			for (int i = 0; i < QueueInProcess.Count; i++)
			{
				var entry = QueueInProcess[i];
				if (entry != null && entry.Behaviour == behaviour && entry.Action == action)
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
			for (int i = 0; i < QueueInProcess.Count; i++)
			{
				var entry = QueueInProcess[i];
				if (entry != null && entry.Behaviour == behaviour)
				{
					return true;
				}
			}
			return false;
		}

		internal bool IsInvokingAny()
		{
			return ScaledInvokeQueue.Count > 0 || UnscaledInvokeQueue.Count > 0 || IsQueueInProcessContainsAnyNonNull;
		}

		internal int TotalInvokeCount()
		{
			return ScaledInvokeQueue.Count + UnscaledInvokeQueue.Count + QueueInProcessNonNullCount;
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
			for (int i = 0; i < QueueInProcess.Count; i++)
			{
				var entry = QueueInProcess[i];
				if (entry != null && entry.Behaviour == behaviour)
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
			for (int i = 0; i < QueueInProcess.Count; i++)
			{
				var entry = QueueInProcess[i];
				if (entry != null && entry.Behaviour == behaviour && entry.Action == action)
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
			if (CurrentlyProcessingQueue == InvokeQueue.Scaled)
			{
				for (int i = 0; i < QueueInProcess.Count; i++)
				{
					var entry = QueueInProcess[i];
					if (entry != null && entry.Behaviour == behaviour && entry.Action == action)
					{
						return entry.NextTime - now;
					}
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
			if (CurrentlyProcessingQueue == InvokeQueue.Unscaled)
			{
				for (int i = 0; i < QueueInProcess.Count; i++)
				{
					var entry = QueueInProcess[i];
					if (entry != null && entry.Behaviour == behaviour && entry.Action == action)
					{
						return entry.NextTime - now;
					}
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
			if (CurrentlyProcessingQueue == InvokeQueue.Scaled)
			{
				for (int i = 0; i < QueueInProcess.Count; i++)
				{
					var entry = QueueInProcess[i];
					if (entry != null && entry.Behaviour == behaviour)
					{
						return entry.NextTime - now;
					}
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
			if (CurrentlyProcessingQueue == InvokeQueue.Unscaled)
			{
				for (int i = 0; i < QueueInProcess.Count; i++)
				{
					var entry = QueueInProcess[i];
					if (entry != null && entry.Behaviour == behaviour)
					{
						return entry.NextTime - now;
					}
				}
			}
			return double.NaN;
		}

		#endregion

		#region Verbose Logging

#if UNITY_EDITOR
		public static bool VerboseLoggingInEachUpdate;
		public static bool VerboseLoggingInEachFixedUpdate;
#endif

		#endregion
	}

}
