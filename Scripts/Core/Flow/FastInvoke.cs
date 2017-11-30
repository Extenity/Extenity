using System;
using System.Collections.Generic;
using UnityEngine;

namespace Extenity.FlowToolbox
{

	public class FastInvokeHandler : MonoBehaviour
	{
		public class InvokeEntry
		{
			public float NextTime;
			public Behaviour Behaviour;
			public Action Action;
			public float RepeatRate;

			public void Reset()
			{
				NextTime = 0f;
				Behaviour = null;
				Action = null;
				RepeatRate = 0f;
			}

			public void Initialize(Behaviour behaviour, Action action, float time, float repeatRate)
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
			var now = Time.time;

			while (FixedUpdateInvokes.Count > 0)
			{
				var entry = FixedUpdateInvokes[0];
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

					// Remove from queue
					FixedUpdateInvokes.RemoveAt(0);

					// Repeat or delete
					RepeatIfNeededOrPool(entry);
				}
				else
				{
					break;
				}
			}
		}

		private void RepeatIfNeededOrPool(InvokeEntry entry)
		{
			if (entry.RepeatRate > 0f)
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

		internal void Launch(Behaviour behaviour, Action action, float time, float repeatRate)
		{
			if (!behaviour)
			{
				Debug.LogError("Tried to invoke for a null behaviour.");
				return;
			}
			var now = Time.time;
			var entry = CreateOrGetEntryPool();
			entry.Initialize(behaviour, action, now + time, repeatRate);
			AddToFixedUpdateInvokes(entry);
		}

		internal void Cancel(Behaviour behaviour, Action action)
		{
			if (!behaviour)
			{
				Debug.LogError("Tried to cancel an invoke of a null behaviour.");
				return;
			}
			if (FixedUpdateInvokes.Count > 0)
			{
				for (int i = 0; i < FixedUpdateInvokes.Count; i++)
				{
					var entry = FixedUpdateInvokes[i];
					if (entry.Behaviour == behaviour && entry.Action == action)
					{
						FixedUpdateInvokes.RemoveAt(i);
						PoolEntry(entry);
						//break; Do not break. There may be more than one.
					}
				}
			}
		}
	}

	public static class FastInvokeBridge
	{
		#region Singleton Handler

		private static readonly FastInvokeHandler Handler;

		static FastInvokeBridge()
		{
			var go = new GameObject("_FastInvokeHandler");
			GameObject.DontDestroyOnLoad(go);
			go.hideFlags = HideFlags.HideInHierarchy;
			Handler = go.AddComponent<FastInvokeHandler>();
		}

		#endregion

		public static void FastInvoke(this Behaviour behaviour, Action action, float time)
		{
			Handler.Launch(behaviour, action, time, 0f);
		}

		public static void FastInvokeRepeating(this Behaviour behaviour, Action action, float repeatRate)
		{
			Handler.Launch(behaviour, action, repeatRate, repeatRate);
		}

		public static void FastInvokeRepeating(this Behaviour behaviour, Action action, float initialDelay, float repeatRate)
		{
			Handler.Launch(behaviour, action, initialDelay, repeatRate);
		}

		public static void CancelFastInvoke(this Behaviour behaviour, Action action)
		{
			Handler.Cancel(behaviour, action);
		}
	}

}
