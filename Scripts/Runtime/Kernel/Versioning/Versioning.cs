using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Extenity.MessagingToolbox;

namespace Extenity.Kernel
{

	public class VersionEvent : ExtenityEvent
	{
	}

	public static class Versioning
	{
		#region Versions

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Invalidate(int id)
		{
			if (Ref.IsOwnerBitSet(id))
			{
				Log.CriticalError("Received an ID that has its ownership bit set. ID: " + id.ToString("X"));
			}

			_EnqueueVersionChangedEvent(id);
		}

		#endregion

		#region Events

		private static readonly Dictionary<int, VersionEvent> Events = new Dictionary<int, VersionEvent>(10000);

		private static VersionEvent _GetVersionEventByID(int id)
		{
			// At this point, we assume 'id' has its ownership info filtered out by the caller of this method. See 118546802.
			if (!Events.TryGetValue(id, out var versionEvent))
			{
				versionEvent = new VersionEvent();
				Events.Add(id, versionEvent);
			}

			return versionEvent;
		}

		public static void RegisterForVersionChanges(Ref id, Action callback, int order = 0)
		{
			// Note that we are filtering out the ownership info of Ref. See 118546802.
			_GetVersionEventByID(id.ID).AddListener(callback, order);
		}

		public static void DeregisterForVersionChanges(Ref id, Action callback)
		{
			// Note that we are filtering out the ownership info of Ref. See 118546802.
			if (Events.TryGetValue(id.ID, out var versionEvent))
			{
				versionEvent.RemoveListener(callback);
			}
		}

		#endregion

		#region Version Change Event Queue

		public static readonly Queue<int> VersionChangeEventQueue = new Queue<int>();

		public static void EnqueueVersionChangedEventForAllRegisteredIDs()
		{
			// Note that we know Events.Keys already has its members ownership info filtered out. See 118546802.
			foreach (var id in Events.Keys)
			{
				_EnqueueVersionChangedEvent(id);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void _EnqueueVersionChangedEvent(int id)
		{
			// At this point, we assume 'id' has its ownership info filtered out by the caller of this method. See 118546802.
			VersionChangeEventQueue.Enqueue(id);
		}

		public static void EmitEventsInQueue()
		{
			while (VersionChangeEventQueue.Count > 0)
			{
				var id = VersionChangeEventQueue.Dequeue();

				if (Events.TryGetValue(id, out var versionEvent))
				{
					versionEvent.InvokeSafe();
				}
				else
				{
					Log.Warning($"Event is missing for ID:{id}.");
				}
			}
		}

		#endregion
	}

}
