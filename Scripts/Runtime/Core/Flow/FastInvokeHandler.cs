#if UNITY

//#define EnableOverkillLogging

using System;
using System.Collections.Generic;
using Extenity.DataToolbox;
using Unity.Profiling;
using UnityEngine;
#if EnableOverkillLogging
using System.Linq;
#endif

namespace Extenity.FlowToolbox
{

	public class FastInvokeHandler
	{
		#region Configuration

		public const double Tolerance =
			1.0 / 1000.0 // Milliseconds
				/ 100.0; // A percent of a millisecond

		#endregion

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

			public void Renew(double invokeTime, double repeatRate)
			{
				// Renew is expected to be called from everywhere, including async methods and threads.
				// So caching the timing API is hard. We'll fallback to Unity timing API here.
				// That's why RefreshCacheAndGetTime is used here. 
				var now = Loop.RefreshCacheAndGetTime(UnscaledTime);

				if (invokeTime < 0)
				{
					if (LogWarningForNegativeInvokeTimes)
						Log.Warning($"Received negative invoke time '{invokeTime}'.");
					invokeTime = 0;
				}
				if (repeatRate < 0)
				{
					if (LogWarningForNegativeInvokeTimes)
						Log.Warning($"Received negative repeat rate '{repeatRate}'.");
					repeatRate = 0;
				}

				NextTime = now + invokeTime;
				RepeatRate = repeatRate;

#if EnableOverkillLogging
				Log.Verbose($"New {(UnscaledTime ? "Unscaled-Time" : "Scaled-Time")} Invoke.  Now: {now * 1000}  Delay: {invokeTime * 1000}{(repeatRate > 0f ? $"  Repeat: {repeatRate * 1000}" : "")}");
#endif
			}

			public void Initialize(Behaviour behaviour, Action action, double invokeTime, double repeatRate, bool unscaledTime)
			{
				Behaviour = behaviour;
				Action = action;
				UnscaledTime = unscaledTime;
				Renew(invokeTime, repeatRate);
			}
		}

		#region Shutdown

		internal void Shutdown()
		{
			ScaledInvokeQueue.Clear();
			UnscaledInvokeQueue.Clear();
			QueueInProcess.Clear();
			CurrentlyProcessingEntryAction = null;
			CurrentlyProcessingQueue = InvokeQueue.Unspecified;
		}

		#endregion
		
		#region Update

		// CustomUpdate and CustomFixedUpdate methods are almost the same.
		// Do not forget to update both of them if you need to change something.
		// See 11531451
		internal void CustomFixedUpdate(double time)
		{
			if (QueueInProcess.Count > 0)
			{
				Log.InternalError(7591128); // Definitely expected to be empty.
				QueueInProcess.Clear();
			}

			// Get all the entries of which their time has come.
			for (int i = 0; i < ScaledInvokeQueue.Count; i++)
			{
				var entry = ScaledInvokeQueue[i];
				if (time >= entry.NextTime - Tolerance) // Tolerance fixes the floating point calculation errors.
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

#if EnableOverkillLogging
			Log.Verbose($"Processing {CurrentlyProcessingQueue}-Time queue ({QueueInProcess.Count}). Now: {time}:\n" +
			         string.Join("\n", QueueInProcess.Select(entry => $"NextTime: {entry.NextTime * 1000} \t Diff: {(time - entry.NextTime) * 1000}")) + "\n\nLeft in queue:\n" +
			         string.Join("\n", ScaledInvokeQueue.Select(entry => $"NextTime: {entry.NextTime * 1000} \t Diff: {(time - entry.NextTime) * 1000}")));
#endif

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
						CurrentlyProcessingEntryAction = entry.Action;
#if ENABLE_PROFILER
						using (new ProfilerMarker(entry.Behaviour.name).Auto())
#endif
						{
							entry.Action();
						}
					}
					catch (Exception exception)
					{
						Log.ErrorWithContext(entry.Behaviour, exception);
					}
					finally
					{
						CurrentlyProcessingEntryAction = null;
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

		// CustomUpdate and CustomFixedUpdate methods are almost the same.
		// Do not forget to update both of them if you need to change something.
		// See 11531451
		internal void CustomUpdate(double time)
		{
			if (QueueInProcess.Count > 0)
			{
				Log.InternalError(7591128); // Definitely expected to be empty.
				QueueInProcess.Clear();
			}

			// Get all the entries of which their time has come.
			for (int i = 0; i < UnscaledInvokeQueue.Count; i++)
			{
				var entry = UnscaledInvokeQueue[i];
				if (time >= entry.NextTime - Tolerance) // Tolerance fixes the floating point calculation errors.
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

#if EnableOverkillLogging
			Log.Verbose($"Processing {CurrentlyProcessingQueue}-Time queue ({QueueInProcess.Count}). Now: {time}:\n" +
			         string.Join("\n", QueueInProcess.Select(entry => $"NextTime: {entry.NextTime * 1000} \t Diff: {(time - entry.NextTime) * 1000}")) + "\n\nLeft in queue:\n" +
			         string.Join("\n", UnscaledInvokeQueue.Select(entry => $"NextTime: {entry.NextTime * 1000} \t Diff: {(time - entry.NextTime) * 1000}")));
#endif

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
						CurrentlyProcessingEntryAction = entry.Action;
#if ENABLE_PROFILER
						using (new ProfilerMarker(entry.Behaviour.name).Auto())
#endif
						{
							entry.Action();
						}
					}
					catch (Exception exception)
					{
						Log.ErrorWithContext(entry.Behaviour, exception);
					}
					finally
					{
						CurrentlyProcessingEntryAction = null;
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

		#region Queues

		public enum InvokeQueue
		{
			Unspecified,
			Scaled,
			Unscaled,
		}

		private Action CurrentlyProcessingEntryAction;
		private InvokeQueue CurrentlyProcessingQueue = InvokeQueue.Unspecified;

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

		#endregion

		#region Queue Operations - Redirecting to related timescale queues

		private void InsertIntoQueue(InvokeEntry entry)
		{
			if (entry.UnscaledTime)
				InsertIntoQueue(UnscaledInvokeQueue, entry);
			else
				InsertIntoQueue(ScaledInvokeQueue, entry);
		}

		private void MoveEntryAfterItsOrderChangedInQueue(InvokeEntry entry, int index)
		{
			if (entry.UnscaledTime)
				MoveEntryAfterItsOrderChangedInQueue(UnscaledInvokeQueue, entry, index);
			else
				MoveEntryAfterItsOrderChangedInQueue(ScaledInvokeQueue, entry, index);
		}

		/// <summary>
		/// This is the main operation behind the feature of overwriting an existing entry. This method searches if entry lists have any entries
		/// that matches 'behaviour' and 'action'. The search is being made in both <see cref="ScaledInvokeQueue"/> and <see cref="UnscaledInvokeQueue"/>.
		/// 
		/// At the end of the day, the currently processing invoke operation that needed to call this method, definitely needs to add the entry in the list
		/// that is stated by <see cref="currentlyProcessingInvokeCalledForUnscaledTime"/>. So this method first tries to get the existing entry in stated
		/// timescale list if possible. But there are a lot of possibilities which are listed below.
		///		=0 in SAME list and =0 in OTHER list: Returns nothing.
		///		=1 in SAME list and =0 in OTHER list: Returns the one.
		///		=0 in SAME list and =1 in OTHER list: Returns the one.
		///		>1 in SAME list and =0 in OTHER list: Returns the closest one and deletes others.
		///		=0 in SAME list and >1 in OTHER list: Returns the closest one and deletes others.
		///		=1 in SAME list and >1 in OTHER list: Returns the one in SAME and deletes others.
		///		>1 in SAME list and >1 in OTHER list: Returns the closest one in SAME and deletes others.
		/// </summary>
		private bool TryGetSingleEntryAndRemoveOthers(Behaviour behaviour, Action action, bool currentlyProcessingInvokeCalledForUnscaledTime, out InvokeEntry foundEntry, out int foundEntryIndex)
		{
			var sameList = currentlyProcessingInvokeCalledForUnscaledTime ? UnscaledInvokeQueue : ScaledInvokeQueue;
			var otherList = currentlyProcessingInvokeCalledForUnscaledTime ? ScaledInvokeQueue : UnscaledInvokeQueue;

			// Try to find the first entry in the SAME list. While doing that, remove all others.
			var foundInTheSameList = TryGetSingleEntryAndRemoveOthers(sameList, behaviour, action, out foundEntry, out foundEntryIndex);
			if (foundInTheSameList)
			{
				// Remove all found entries in the OTHER list, if any.
				RemoveInQueue(otherList, behaviour, action);
				return true;
			}
			else
			{
				// Try to find the first entry in the OTHER list. While doing that, remove all others.
				var foundInTheOtherList = TryGetSingleEntryAndRemoveOthers(otherList, behaviour, action, out foundEntry, out foundEntryIndex);
				if (foundInTheOtherList)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		#endregion

		#region Queue Operations

		private static void InsertIntoQueue(List<InvokeEntry> queue, InvokeEntry entry)
		{
			if (queue.Count != 0)
			{
				var nextTime = entry.NextTime;
				for (int i = 0; i < queue.Count; i++)
				{
					if (nextTime < queue[i].NextTime)
					{
						queue.Insert(i, entry);
						return;
					}
				}
			}
			queue.Add(entry);
		}

		private static void MoveEntryAfterItsOrderChangedInQueue(List<InvokeEntry> queue, InvokeEntry entry, int index)
		{
			Debug.Assert(entry == queue[index]);
			var nextTime = entry.NextTime;
			if (index > 0 && queue[index - 1].NextTime > nextTime)
			{
				// Should move through the beginning of the list
				var newIndex = index - 1;
				while (newIndex > 0 && queue[newIndex - 1].NextTime > nextTime)
					newIndex--;
				queue.Move(index, newIndex);
			}
			else if (index < queue.Count - 1 && queue[index + 1].NextTime < nextTime)
			{
				// Should move through the end of the list
				var newIndex = index + 1;
				while (newIndex < queue.Count - 1 && queue[newIndex + 1].NextTime < nextTime)
					newIndex++;
				queue.Move(index, newIndex);
			}
			//else
			//{
			//	Nothing to do. The order was not changed.
			//}
		}

		private static void RemoveInQueue(List<InvokeEntry> queue, Behaviour behaviour, Action action)
		{
			for (int i = 0; i < queue.Count; i++)
			{
				var entry = queue[i];
				// The entries in this queue may not have null values. So no need to check for null at the start.
				if (entry.Behaviour == behaviour && entry.Action == action)
				{
					queue.RemoveAt(i);
					i--;
					PoolEntry(entry);
					//break; Do not break. There may be more than one.
				}
			}
		}

		private static void RemoveInQueue(List<InvokeEntry> queue, Behaviour behaviour)
		{
			for (int i = 0; i < queue.Count; i++)
			{
				var entry = queue[i];
				// The entries in this queue may not have null values. So no need to check for null at the start.
				if (entry.Behaviour == behaviour)
				{
					queue.RemoveAt(i);
					i--;
					PoolEntry(entry);
					//break; Do not break. There may be more than one.
				}
			}
		}

		private static void NullifyInQueue(List<InvokeEntry> queue, Behaviour behaviour, Action action)
		{
			for (int i = 0; i < queue.Count; i++)
			{
				var entry = queue[i];
				// The entries in this queue may have null values. So we check for null at the start.
				if (entry != null && entry.Behaviour == behaviour && entry.Action == action)
				{
					queue[i] = null;
					i--;
					PoolEntry(entry);
					//break; Do not break. There may be more than one.
				}
			}
		}

		private static void NullifyInQueue(List<InvokeEntry> queue, Behaviour behaviour)
		{
			for (int i = 0; i < queue.Count; i++)
			{
				var entry = queue[i];
				// The entries in this queue may have null values. So we check for null at the start.
				if (entry != null && entry.Behaviour == behaviour)
				{
					queue[i] = null;
					i--;
					PoolEntry(entry);
					//break; Do not break. There may be more than one.
				}
			}
		}

		private static bool TryGetSingleEntryAndRemoveOthers(List<InvokeEntry> queue, Behaviour behaviour, Action action, out InvokeEntry foundEntry, out int foundEntryIndex)
		{
			foundEntry = null;
			foundEntryIndex = -1;

			for (int i = 0; i < queue.Count; i++)
			{
				var entry = queue[i];
				// The entries in this queue may not have null values. So no need to check for null at the start.
				if (entry.Behaviour == behaviour && entry.Action == action)
				{
					if (foundEntryIndex < 0)
					{
						foundEntry = entry;
						foundEntryIndex = i;
					}
					else
					{
						queue.RemoveAt(i);
						i--;
						PoolEntry(entry);
					}
					//break; Do not break. There may be more than one.
				}
			}

			return foundEntryIndex >= 0;
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
				InsertIntoQueue(entry);
			}
			else
			{
				PoolEntry(entry);
			}
		}

		#endregion

		#region Invoke / Cancel

		internal void Invoke(Behaviour behaviour, Action action, double time, double repeatRate, bool unscaledTime, bool overwriteExisting)
		{
			if (!behaviour)
			{
				// Calling Invoke over a null object is handled just like calling a method of that object, which is going to throw.
				throw new Exception("Tried to invoke over a null behaviour.");
			}

			if (overwriteExisting)
			{
				// The implementation of overwriteExisting might not be completed yet.
				// Make sure you write tests for it right now, before using it.
				// Do not delete this warning before seeing that it is running flawless.
				Log.Warning("FastInvoke overwrite feature is not tested yet.");
			}

			if (overwriteExisting && TryGetSingleEntryAndRemoveOthers(behaviour, action, unscaledTime, out var entry, out var index))
			{
				if (entry.UnscaledTime != unscaledTime)
				{
					// Wow, unscaled time option has changed. This must be rare.
					// Rather than changing the existing entry, we just remove
					// the previous entry from it's list and add the new entry
					// to the other list. Can be further optimized but this would
					// happen rarely, so no need.
					Cancel(behaviour, action);
					Invoke(behaviour, action, time, repeatRate, unscaledTime, false); // Overwrite existing is false. We know we have deleted the existing ones just now. So no need to check for them one more time.
				}
				else
				{
					entry.Renew(time, repeatRate);
					MoveEntryAfterItsOrderChangedInQueue(entry, index);
				}
			}
			else
			{
				entry = CreateOrGetEntryPool();
				entry.Initialize(behaviour, action, time, repeatRate, unscaledTime);
				InsertIntoQueue(entry);
			}
		}

		internal void Cancel(Behaviour behaviour, Action action)
		{
			if (!behaviour)
			{
				// Invoke cancellation is a less critical operation. The invoke should already be cancelled if
				// the object was destroyed before calling Cancel.
				//
				// Not sure about this decision of NOT throwing here, but we will stick with this until some
				// more solid reason comes up. See 117459234.
				Log.Fatal("Tried to cancel fast invoke of a null behaviour.");
				return;
			}
			RemoveInQueue(ScaledInvokeQueue, behaviour, action);
			RemoveInQueue(UnscaledInvokeQueue, behaviour, action);
			NullifyInQueue(QueueInProcess, behaviour, action); // Do not remove! Make it null instead. The process loop depends on QueueInProcess entries not being added or removed in the middle of process. See 1765136.
		}

		internal void CancelAll(Behaviour behaviour)
		{
			if (!behaviour)
			{
				// Invoke cancellation is a less critical operation. The invoke should already be cancelled if
				// the object was destroyed before calling Cancel.
				//
				// Not sure about this decision of NOT throwing here, but we will stick with this until some
				// more solid reason comes up. See 117459234.
				Log.Fatal("Tried to cancel fast invoke of a null behaviour.");
				return;
			}
			RemoveInQueue(ScaledInvokeQueue, behaviour);
			RemoveInQueue(UnscaledInvokeQueue, behaviour);
			NullifyInQueue(QueueInProcess, behaviour); // Do not remove! Make it null instead. The process loop depends on QueueInProcess entries not being added or removed in the middle of process. See 1765136.
		}

		internal void CancelCurrent(Behaviour behaviour)
		{
			if (!behaviour)
			{
				// Invoke cancellation is a less critical operation. The invoke should already be cancelled if
				// the object was destroyed before calling Cancel.
				//
				// Not sure about this decision of NOT throwing here, but we will stick with this until some
				// more solid reason comes up. See 117459234.
				Log.Fatal("Tried to cancel fast invoke of a null behaviour.");
				return;
			}
			if (CurrentlyProcessingEntryAction == null)
			{
				Log.Fatal("Tried to cancel current fast invoke while there is none.");
				return;
			}
			RemoveInQueue(ScaledInvokeQueue, behaviour, CurrentlyProcessingEntryAction);
			RemoveInQueue(UnscaledInvokeQueue, behaviour, CurrentlyProcessingEntryAction);
			NullifyInQueue(QueueInProcess, behaviour, CurrentlyProcessingEntryAction); // Do not remove! Make it null instead. The process loop depends on QueueInProcess entries not being added or removed in the middle of process. See 1765136.
		}

		#endregion

		#region Information

		internal bool IsInvoking(Behaviour behaviour, Action action)
		{
			if (!behaviour)
			{
				// Checking if Invoke is alive feels like a less critical operation. The answer is simply
				// "No, it's not alive".
				//
				// Not sure about this decision of NOT throwing here, but we will stick with this until some
				// more solid reason comes up. See 117459234.
				Log.Fatal("Tried to query fast invoke of a null behaviour.");
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
				// Checking if Invoke is alive feels like a less critical operation. The answer is simply
				// "No, it's not alive".
				//
				// Not sure about this decision of NOT throwing here, but we will stick with this until some
				// more solid reason comes up. See 117459234.
				Log.Fatal("Tried to query fast invoke of a null behaviour.");
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

		internal int TotalActiveInvokeCount()
		{
			return ScaledInvokeQueue.Count + UnscaledInvokeQueue.Count + QueueInProcessNonNullCount;
		}

		internal int InvokeCount(Behaviour behaviour)
		{
			if (!behaviour)
			{
				// Checking for Invoke count feels like a less critical operation. The answer is simply
				// "Zero".
				//
				// Not sure about this decision of NOT throwing here, but we will stick with this until some
				// more solid reason comes up. See 117459234.
				Log.Fatal("Tried to query fast invoke of a null behaviour.");
				return 0;
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
				// Checking for Invoke count feels like a less critical operation. The answer is simply
				// "Zero".
				//
				// Not sure about this decision of NOT throwing here, but we will stick with this until some
				// more solid reason comes up. See 117459234.
				Log.Fatal("Tried to query fast invoke of a null behaviour.");
				return 0;
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
				// Checking for remaining time feels like a less critical operation. The answer is simply
				// "None".
				//
				// Not sure about this decision of NOT throwing here, but we will stick with this until some
				// more solid reason comes up. See 117459234.
				Log.Fatal("Tried to query fast invoke of a null behaviour.");
				return double.NaN;
			}
			return RemainingTimeUntilNextInvoke(ScaledInvokeQueue, InvokeQueue.Scaled, behaviour, action, Loop.Time);
		}

		internal double RemainingTimeUntilNextUnscaledInvoke(Behaviour behaviour, Action action)
		{
			if (!behaviour)
			{
				// Checking for remaining time feels like a less critical operation. The answer is simply
				// "None".
				//
				// Not sure about this decision of NOT throwing here, but we will stick with this until some
				// more solid reason comes up. See 117459234.
				Log.Fatal("Tried to query fast invoke of a null behaviour.");
				return double.NaN;
			}
			return RemainingTimeUntilNextInvoke(UnscaledInvokeQueue, InvokeQueue.Unscaled, behaviour, action, Loop.UnscaledTime);
		}

		internal double RemainingTimeUntilNextInvoke(Behaviour behaviour)
		{
			if (!behaviour)
			{
				// Checking for remaining time feels like a less critical operation. The answer is simply
				// "None".
				//
				// Not sure about this decision of NOT throwing here, but we will stick with this until some
				// more solid reason comes up. See 117459234.
				Log.Fatal("Tried to query fast invoke of a null behaviour.");
				return double.NaN;
			}
			return RemainingTimeUntilNextInvoke(ScaledInvokeQueue, InvokeQueue.Scaled, behaviour, Loop.Time);
		}

		internal double RemainingTimeUntilNextUnscaledInvoke(Behaviour behaviour)
		{
			if (!behaviour)
			{
				// Checking for remaining time feels like a less critical operation. The answer is simply
				// "None".
				//
				// Not sure about this decision of NOT throwing here, but we will stick with this until some
				// more solid reason comes up. See 117459234.
				Log.Fatal("Tried to query fast invoke of a null behaviour.");
				return double.NaN;
			}
			return RemainingTimeUntilNextInvoke(UnscaledInvokeQueue, InvokeQueue.Unscaled, behaviour, Loop.UnscaledTime);
		}

		private double RemainingTimeUntilNextInvoke(List<InvokeEntry> queue, InvokeQueue currentlyProcessingQueue, Behaviour behaviour, Action action, double now)
		{
			for (int i = 0; i < queue.Count; i++)
			{
				var entry = queue[i];
				if (entry.Behaviour == behaviour && entry.Action == action)
				{
					return entry.NextTime - now;
				}
			}
			if (CurrentlyProcessingQueue == currentlyProcessingQueue)
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

		private double RemainingTimeUntilNextInvoke(List<InvokeEntry> queue, InvokeQueue currentlyProcessingQueue, Behaviour behaviour, double now)
		{
			for (int i = 0; i < queue.Count; i++)
			{
				var entry = queue[i];
				if (entry.Behaviour == behaviour)
				{
					return entry.NextTime - now;
				}
			}
			if (CurrentlyProcessingQueue == currentlyProcessingQueue)
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

		#region Log

		public static bool LogWarningForNegativeInvokeTimes = true;

		private static readonly Logger Log = new("FastInvoke");

		#endregion
	}

}

#endif
