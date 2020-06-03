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
			VersionChangeEventQueue.Enqueue(id);
		}

		public static void InvalidateAllRegisteredIDs()
		{
			foreach (var id in Events.Keys)
			{
				VersionChangeEventQueue.Enqueue(id);
			}
		}

		#endregion

		#region Events

		private static readonly Dictionary<int, VersionEvent> Events = new Dictionary<int, VersionEvent>(10000);

		private static VersionEvent _GetVersionEventByID(int id)
		{
			if (!Events.TryGetValue(id, out var versionEvent))
			{
				versionEvent = new VersionEvent();
				Events.Add(id, versionEvent);
			}

			return versionEvent;
		}

		public static void RegisterForVersionChanges(int id, Action callback, int order = 0)
		{
			_GetVersionEventByID(id).AddListener(callback, order);
		}

		public static void DeregisterForVersionChanges(int id, Action callback)
		{
			if (Events.TryGetValue(id, out var versionEvent))
			{
				versionEvent.RemoveListener(callback);
			}
		}

		#endregion

		#region Version Change Event Queue

		public static readonly Queue<int> VersionChangeEventQueue = new Queue<int>();

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
