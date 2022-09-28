#if ExtenityKernel

// #define DisableVersioningStats

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Extenity.DataToolbox;
using Extenity.MessagingToolbox;
using Sirenix.OdinInspector;

namespace Extenity.KernelToolbox
{

	public class VersionEvent : ExtenityEvent
	{
	}

	public class Versioning
	{
		#region Versions

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Invalidate(UInt32 id)
		{
			if (id == 0)
			{
				Log.Warning("Tried to invalidate an item with ID '0'.");
				return;
			}

			VersionChangeEventQueue.Enqueue(id);
		}

		public void InvalidateAllRegisteredIDs()
		{
			foreach (var id in Events.Keys)
			{
				VersionChangeEventQueue.Enqueue(id);
			}
		}

		#endregion

		#region Events

		private readonly Dictionary<UInt32, VersionEvent> Events = new Dictionary<UInt32, VersionEvent>(10000);

		private VersionEvent _GetVersionEventByID(UInt32 id)
		{
			if (!Events.TryGetValue(id, out var versionEvent))
			{
				versionEvent = new VersionEvent();
				Events.Add(id, versionEvent);
			}

			return versionEvent;
		}

		public void RegisterForVersionChanges(UInt32 id, Action callback, int order = 0)
		{
			_GetVersionEventByID(id).AddListener(callback, order);
		}

		public bool DeregisterForVersionChanges(UInt32 id, Action callback)
		{
			if (Events.TryGetValue(id, out var versionEvent))
			{
				return versionEvent.RemoveListener(callback);
			}
			return false;
		}

		#endregion

		#region Version Change Event Queue

		private readonly Queue<UInt32> VersionChangeEventQueue = new Queue<UInt32>();

		public void EmitEventsInQueue()
		{
			while (VersionChangeEventQueue.Count > 0)
			{
				var id = VersionChangeEventQueue.Dequeue();

				if (Events.TryGetValue(id, out var versionEvent))
				{
					if (versionEvent.IsAnyAliveListenerRegistered)
					{
						versionEvent.InvokeSafe();
					}
					else
					{
						InformBlankShot(id);
					}
				}
				else
				{
					InformBlankShot(id);
				}
			}
		}

		#endregion

		#region Stats

#if !DisableVersioningStats

		public class VersioningStats
		{
			/// <summary>
			/// First parameter: Invalidated ID.
			/// Second parameter: Blank shot counts. How many times there wasn't any listener at the time of emitting the invalidated ID.
			/// </summary>
			[InfoBox("Blank shot counts. Left value: Invalidated event ID. How many times there wasn't any listener at the time of emitting the invalidated ID.")]
			public readonly Dictionary<UInt32, int> BlankShotCounts = new Dictionary<UInt32, int>();
		}

		[HorizontalGroup("StatsGroup", Order = 3), ReadOnly]
		public VersioningStats Stats = new VersioningStats();

		public void InformBlankShot(UInt32 id)
		{
			Stats.BlankShotCounts.AddOrIncrease(id);
		}

		[HorizontalGroup("StatsGroup", Width = 150)]
		[Button(ButtonSizes.Large, Name = "Clear Stats")]
		public void ClearStats()
		{
			Stats = new VersioningStats();
		}

#else
		public class VersioningStats
		{
		}

		public VersioningStats Stats = new VersioningStats();

		[System.Diagnostics.Conditional("FALSE")]
		public void InformBlankShot(UInt32 id)
		{
		}

		[System.Diagnostics.Conditional("FALSE")]
		public void ClearStats()
		{
		}

#endif

		#endregion
	}

}

#endif
