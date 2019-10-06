using System;
using System.Collections.Generic;
using Extenity.DataToolbox;
using Extenity.GameObjectToolbox;
using Object = UnityEngine.Object;

namespace Extenity.MessagingToolbox
{

	// TODO: Use Log.Verbose

	public class ExtenitySwitch
	{
		#region Callback entries

		public struct Entry
		{
			public readonly Action SwitchOnCallback;
			public readonly Action SwitchOffCallback;
			public readonly bool IsSwitchOnCallbackTargetsUnityObject;
			public readonly bool IsSwitchOffCallbackTargetsUnityObject;
			public readonly int Order;
			public readonly ListenerLifeSpan LifeSpan;
			public readonly Object LifeSpanTarget;
			public readonly bool IsLifeSpanTargetAssigned;

			public Entry(Action switchOnCallback, Action switchOffCallback, int order, ListenerLifeSpan lifeSpan, Object lifeSpanTarget)
			{
				SwitchOnCallback = switchOnCallback;
				SwitchOffCallback = switchOffCallback;
				IsSwitchOnCallbackTargetsUnityObject = switchOnCallback?.Target as Object;
				IsSwitchOffCallbackTargetsUnityObject = switchOffCallback?.Target as Object;
				Order = order;
				LifeSpan = lifeSpan;
				LifeSpanTarget = lifeSpanTarget;
				IsLifeSpanTargetAssigned = lifeSpanTarget != null;
			}

			public bool IsInvalid => SwitchOnCallback == null && SwitchOffCallback == null;

			public bool IsSwitchOnCallbackTargetedUnityObjectDestroyed => IsSwitchOnCallbackTargetsUnityObject && !(SwitchOnCallback.Target as Object);
			public bool IsSwitchOffCallbackTargetedUnityObjectDestroyed => IsSwitchOffCallbackTargetsUnityObject && !(SwitchOffCallback.Target as Object);

			public bool IsLifeSpanTargetDestroyed => IsLifeSpanTargetAssigned && !LifeSpanTarget;

			public bool ShouldRemoveAfterEmit
			{
				get { return LifeSpan == ListenerLifeSpan.RemovedAtFirstEmit; }
			}

			public bool IsObjectDestroyed
			{
				get
				{
					return IsInvalid ||
					       (IsSwitchOnCallbackTargetedUnityObjectDestroyed && IsSwitchOffCallbackTargetedUnityObjectDestroyed) ||
					       IsLifeSpanTargetDestroyed;
				}
			}

			public Object LogObject(bool isSwitchedOn)
			{
				// First, try to get callback delegate object. If not available, get the LifeSpanTarget object.
				var callback = GetCallback(isSwitchedOn);
				if (callback != null) // Check if the callback is specified by user. See 11853135.
				{
					var obj = callback.Target as Object;
					if (obj)
						return obj;
				}
				return LifeSpanTarget;
			}

			public Action GetCallback(bool isSwitchedOn)
			{
				return isSwitchedOn
					? SwitchOnCallback
					: SwitchOffCallback;
			}

			public bool HasCallbacks(Action switchOnCallback, Action switchOffCallback)
			{
				return SwitchOnCallback == switchOnCallback &&
				       SwitchOffCallback == switchOffCallback;
			}
		}

		/// <summary>
		/// CAUTION! Do not modify! Use AddListener and RemoveListener instead.
		/// </summary>
		public List<Entry> _Callbacks => Callbacks;
		private readonly List<Entry> Callbacks = new List<Entry>(10);

		public bool IsAnyListenerRegistered => Callbacks.Count > 0;

		public bool IsSwitchOnListenerRegistered(Action callback)
		{
			for (var i = 0; i < Callbacks.Count; i++)
			{
				if (Callbacks[i].SwitchOnCallback == callback)
					return true;
			}
			return false;
		}

		public bool IsSwitchOffListenerRegistered(Action callback)
		{
			for (var i = 0; i < Callbacks.Count; i++)
			{
				if (Callbacks[i].SwitchOffCallback == callback)
					return true;
			}
			return false;
		}

		public Entry GetListenerInfoBySwitchOnCallback(Action callback)
		{
			for (var i = 0; i < Callbacks.Count; i++)
			{
				if (Callbacks[i].SwitchOnCallback == callback)
					return Callbacks[i];
			}
			return default;
		}

		public Entry GetListenerInfoBySwitchOffCallback(Action callback)
		{
			for (var i = 0; i < Callbacks.Count; i++)
			{
				if (Callbacks[i].SwitchOffCallback == callback)
					return Callbacks[i];
			}
			return default;
		}

		public int CallbacksAliveAndWellCount
		{
			get
			{
				var count = 0;
				for (int i = Callbacks.Count - 1; i >= 0; i--)
				{
					if (!Callbacks[i].IsObjectDestroyed) // Check if the object is destroyed
					{
						count++;
					}
				}
				return count;
			}
		}

		public bool IsAnyAliveAndWellCallbackExists
		{
			get
			{
				for (int i = Callbacks.Count - 1; i >= 0; i--)
				{
					if (!Callbacks[i].IsObjectDestroyed) // Check if the object is destroyed
					{
						return true;
					}
				}
				return false;
			}
		}

		public void Clear()
		{
			if (IsInvoking)
				throw new Exception("Cleanup is not allowed while invoking.");

			for (int i = Callbacks.Count - 1; i >= 0; i--)
			{
				if (Callbacks[i].IsObjectDestroyed) // Check if the object is destroyed
				{
					Callbacks.RemoveAt(i);
					i--;
				}
			}
		}

		#endregion

		#region Add / Remove Listener

		/// <param name="order">Lesser ordered callback gets called earlier. Callbacks that have the same order gets called in the order of AddListener calls. Negative values are allowed.</param>
		public void AddListener(Action switchOnCallback, Action switchOffCallback, int order = 0, ListenerLifeSpan lifeSpan = ListenerLifeSpan.Permanent, Object lifeSpanTarget = null)
		{
			if (IsInvoking)
				throw new NotSupportedException("Operations while invoking are not supported."); // See 117418312.
			if (order == int.MinValue || order == int.MaxValue) // These values are reserved for internal use.
				throw new ArgumentOutOfRangeException(nameof(order), order, "");
			if (switchOnCallback == null && switchOffCallback == null)
			{
				if (ExtenityEventTools.VerboseLogging)
					Log.Info($"Tried to add a null callback with {_Detailed_OrderAndLifeSpan(order, lifeSpan, lifeSpanTarget)}.");
				return; // Silently ignore
			}
			if (ExtenityEventTools.VerboseLogging && switchOnCallback == null && lifeSpan == ListenerLifeSpan.RemovedAtFirstEmit)
				Log.Info($"Not usual to add listener with no SwitchOn callback when giving it a life span of {ListenerLifeSpan.RemovedAtFirstEmit}. See the listener with {_Detailed_OrderAndLifeSpanForMethodAndObject(order, lifeSpan, lifeSpanTarget, switchOnCallback, switchOffCallback)}.");

			// See if the callback was already registered.
			for (var i = 0; i < Callbacks.Count; i++)
			{
				if (Callbacks[i].HasCallbacks(switchOnCallback, switchOffCallback))
				{
					// The callback is already registered. See if there is a change in its parameters.
					if (Callbacks[i].Order != order ||
					    Callbacks[i].LifeSpan != lifeSpan ||
					    Callbacks[i].LifeSpanTarget != lifeSpanTarget)
					{
						// Trying to add the same callback with different parameters. Just remove the existing one and
						// create a new one with new parameters. That should happen rarely, so no need to optimize this.
						_RemoveListener(i);
					}
					else
					{
						if (ExtenityEventTools.VerboseLogging)
							Log.Info($"Tried to add an already registered callback with {_Detailed_OrderAndLifeSpanForMethodAndObject(order, lifeSpan, lifeSpanTarget, switchOnCallback, switchOffCallback)}.");
						return; // Silently ignore
					}

					break; // No need to iterate others. Impossible to add a delegate more than once.
				}
			}

			// If the switch is On and the caller needs to know about that, we don't need to register the listeners
			// to callback list. Just call the SwitchOn callback and move on.
			var canFastTrack = lifeSpan == ListenerLifeSpan.RemovedAtFirstEmit && IsSwitchedOn;

			if (canFastTrack)
			{
				// See 118512052. Call the new callback instantly. It will be a safe call and there is no real reason
				// to provide an unsafe call option for just a single callback.
				if (switchOnCallback != null) // Check if the callback is specified by user. See 11853135.
				{
					try
					{
						IsInvoking = true;
						switchOnCallback();
					}
					catch (Exception exception)
					{
						Log.Exception(exception);
					}
					finally
					{
						IsInvoking = false;
					}
				}
				return; // Go no further. Below is the callback registration part, which we don't need.
			}

			// Cover all the shortcuts that we can add the listener at the end of the list and move on.
			//
			// Line 1: If there is no other listener registered, don't bother the ordering. Just add it and move on.
			//
			// Line 2: If trying to add a callback with 0 order and the last item is ordered as 0, fear not!
			// Just add it to the end and move on. While doing that, also cover the possibility that the order
			// is greater(or equal) than the last item's order.
			var entry = new Entry(switchOnCallback, switchOffCallback, order, lifeSpan, lifeSpanTarget);
			var done = false;
			if (Callbacks.Count == 0 || // Line 1
			    order >= Callbacks[Callbacks.Count - 1].Order) // Line 2
			{
				if (ExtenityEventTools.VerboseLogging)
					Log.Info($"Adding listener with {_Detailed_OrderAndLifeSpanForMethodAndObject(order, lifeSpan, lifeSpanTarget, switchOnCallback, switchOffCallback)} as the last entry, resulting '{Callbacks.Count + 1}' listener(s).");
				Callbacks.Add(entry);
				done = true;
			}
			else
			{
				for (int i = 0; i < Callbacks.Count; i++)
				{
					if (order < Callbacks[i].Order)
					{
						if (ExtenityEventTools.VerboseLogging)
							Log.Info($"Adding listener with {_Detailed_OrderAndLifeSpanForMethodAndObject(order, lifeSpan, lifeSpanTarget, switchOnCallback, switchOffCallback)} at index '{i}', resulting '{Callbacks.Count + 1}' listener(s).");
						Callbacks.Insert(i, entry);
						done = true;
						break;
					}
				}
			}
			// This is a safety belt for the developer. Whatever optimization is going on inside the lines above should
			// only useful if it leaves us with the entry added into the list at the end.
			if (!done)
			{
				throw new InternalException(117357191); // The code should not reach here.
			}

			// See 118512052. Call the new callback instantly. It will be a safe call and there is no real reason
			// to provide an unsafe call option for just a single callback.
			var callback = entry.GetCallback(IsSwitchedOn);
			if (callback != null) // Check if the callback is specified by user. See 11853135.
			{
				try
				{
					IsInvoking = true;
					callback();
				}
				catch (Exception exception)
				{
					Log.Exception(exception);
				}
				finally
				{
					IsInvoking = false;
				}
			}
		}

		public bool RemoveListener(Action switchOnCallback, Action switchOffCallback)
		{
			if (IsInvoking)
				throw new NotSupportedException("Operations while invoking are not supported."); // See 117418312.

			if (switchOnCallback == null && switchOffCallback == null)
				return false; // Silently ignore

			for (var i = 0; i < Callbacks.Count; i++)
			{
				if (Callbacks[i].HasCallbacks(switchOnCallback, switchOffCallback))
				{
					if (ExtenityEventTools.VerboseLogging)
						Log.Info($"Removing listener with {_Detailed_OrderForMethodAndObject(Callbacks[i].Order, switchOnCallback, switchOffCallback)} at index '{i}', resulting '{Callbacks.Count - 1}' listener(s).");
					Callbacks.RemoveAt(i);
					return true;
				}
			}
			if (ExtenityEventTools.VerboseLogging)
				Log.Info($"Failed to remove listener for {_Detailed_MethodAndObject(switchOnCallback, switchOffCallback)}.");
			return false;
		}

		private void _RemoveListener(int index)
		{
			Callbacks.RemoveAt(index);
		}

		[Obsolete("Not implemented yet! If you really need this feature, find a way to implement it.")]
		public void RemoveCurrentListener()
		{
			if (!IsInvoking)
				throw new Exception("Tried to remove current listener outside of listener callback.");

			// There is a possibility that user may call RemoveCurrentListener multiple times. So we must handle that
			// too. Consider using something like Entry.RegistryID to first look if the list has the
			// CurrentListenerRegistryID.
			throw new NotImplementedException();
		}

		public void RemoveAllListeners()
		{
			if (IsInvoking)
				throw new NotSupportedException("Operations while invoking are not supported."); // See 117418312.
			if (ExtenityEventTools.VerboseLogging)
				Log.Info("Removing all listeners.");

			Callbacks.Clear();
		}

		#endregion

		#region Switch

		public bool IsSwitchedOn { get; private set; }

		private bool IsInvoking = false;
		private int InvokeIndex = -1;

		public void SwitchOnUnsafe()
		{
			SwitchUnsafe(true);
		}

		public void SwitchOffUnsafe()
		{
			SwitchUnsafe(false);
		}

		public void SwitchOnSafe()
		{
			SwitchSafe(true);
		}

		public void SwitchOffSafe()
		{
			SwitchSafe(false);
		}

		public void SwitchUnsafe(bool isSwitchedOn)
		{
			if (IsInvoking)
			{
				Log.CriticalError($"Invoked switch {(isSwitchedOn ? "on" : "off")} while an invocation is ongoing.");
				return;
			}
			if (IsSwitchedOn == isSwitchedOn) // Note! Make sure we check for that after "already invoking" check above and before starting to do anything, even before marking the IsInvoking field.
			{
				return;
			}
			IsInvoking = true;
			InvokeIndex = 0;

			// Note that isSwitchedOn will be used inside this method instead of IsSwitchedOn. It tells the state at
			// the time of calling this method, while IsSwitchedOn might change before completing this method.
			IsSwitchedOn = isSwitchedOn;

			try
			{
				while (InvokeIndex < Callbacks.Count)
				{
					var entry = Callbacks[InvokeIndex];
					if (entry.IsObjectDestroyed)
					{
						Callbacks.RemoveAt(InvokeIndex);
						continue;
					}
					if (isSwitchedOn && entry.ShouldRemoveAfterEmit)
					{
						// Remove the callback just before calling it. So that the caller can act like it's removed.
						//
						// Removing before the call also ensures that the callback will be removed even though
						// an exception is thrown inside the callback.
						Callbacks.RemoveAt(InvokeIndex--);
					}

					var callback = entry.GetCallback(isSwitchedOn);
					if (callback != null) // Check if the callback is specified by user. See 11853135.
					{
						callback();
					}

					InvokeIndex++;
				}
			}
			finally
			{
				IsInvoking = false;
				InvokeIndex = -1;
			}
		}

		public void SwitchSafe(bool isSwitchedOn)
		{
			if (IsInvoking)
			{
				Log.CriticalError($"Invoked switch {(isSwitchedOn ? "on" : "off")} while an invocation is ongoing.");
				return;
			}
			if (IsSwitchedOn == isSwitchedOn) // Note! Make sure we check for that after "already invoking" check above and before starting to do anything, even before marking the IsInvoking field.
			{
				return;
			}
			IsInvoking = true;
			InvokeIndex = 0;

			// Note that isSwitchedOn will be used inside this method instead of IsSwitchedOn. It tells the state at
			// the time of calling this method, while IsSwitchedOn might change before completing this method.
			IsSwitchedOn = isSwitchedOn;

			while (InvokeIndex < Callbacks.Count)
			{
				var entry = Callbacks[InvokeIndex];
				if (entry.IsObjectDestroyed)
				{
					Callbacks.RemoveAt(InvokeIndex);
					continue;
				}
				if (isSwitchedOn && entry.ShouldRemoveAfterEmit)
				{
					// Remove the callback just before calling it. So that the caller can act like it's removed.
					//
					// Removing before the call also ensures that the callback will be removed even though
					// an exception is thrown inside the callback.
					Callbacks.RemoveAt(InvokeIndex--);
				}

				var callback = entry.GetCallback(isSwitchedOn);
				if (callback != null) // Check if the callback is specified by user. See 11853135.
				{
					try
					{
						callback();
					}
					catch (Exception exception)
					{
						Log.Exception(exception, entry.LogObject(isSwitchedOn));
					}
				}

				InvokeIndex++;
			}

			IsInvoking = false;
			InvokeIndex = -1;
		}

		#endregion

		#region Log

		public string GetSwitchCallbackDebugInfo()
		{
			var stringBuilder = StringTools.SharedStringBuilder.Value;
			lock (stringBuilder)
			{
				stringBuilder.Clear(); // Make sure it is clean before starting to use.

				for (var i = 0; i < Callbacks.Count; i++)
				{
					var entry = Callbacks[i];
					stringBuilder.AppendLine(_Detailed_OrderAndLifeSpanForMethodAndObject(entry.Order, entry.LifeSpan, entry.LifeSpanTarget, entry.SwitchOnCallback, entry.SwitchOffCallback));
				}

				var result = stringBuilder.ToString();
				StringTools.ClearSharedStringBuilder(stringBuilder); // Make sure we will leave it clean after use.
				return result;
			}
		}

		private string _Detailed_MethodAndObject(Delegate switchOnCallback, Delegate switchOffCallback)
		{
			return $"SwitchOn method '{switchOnCallback.FullNameOfTargetAndMethod()}' and SwitchOff method '{switchOffCallback.FullNameOfTargetAndMethod()}'";
		}

		private string _Detailed_OrderForMethodAndObject(int order, Delegate switchOnCallback, Delegate switchOffCallback)
		{
			return $"order '{order}' for {_Detailed_MethodAndObject(switchOnCallback, switchOffCallback)}";
		}

		private string _Detailed_OrderAndLifeSpan(int order, ListenerLifeSpan lifeSpan, Object lifeSpanTarget)
		{
			return $"order '{order}' and life span '{lifeSpan}{(lifeSpanTarget ? $" with target '{lifeSpanTarget.FullObjectName()}'" : "")}'";
		}

		private string _Detailed_OrderAndLifeSpanForMethodAndObject(int order, ListenerLifeSpan lifeSpan, Object lifeSpanTarget, Delegate switchOnCallback, Delegate switchOffCallback)
		{
			return $"{_Detailed_OrderAndLifeSpan(order, lifeSpan, lifeSpanTarget)} for {_Detailed_MethodAndObject(switchOnCallback, switchOffCallback)}";
		}

		#endregion
	}

}
