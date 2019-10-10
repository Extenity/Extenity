using System;
using System.Collections.Generic;
using System.Text;
using Extenity.DataToolbox;
using Object = UnityEngine.Object;

namespace Extenity.MessagingToolbox
{

	public class ExtenitySwitch
	{
		#region Listeners

		public struct Listener
		{
			public readonly Action SwitchOnCallback;
			public readonly Action SwitchOffCallback;
			public readonly bool IsSwitchOnCallbackTargetsUnityObject;
			public readonly bool IsSwitchOffCallbackTargetsUnityObject;
			public readonly int Order;
			public readonly ListenerLifeSpan LifeSpan;
			public readonly Object LifeSpanTarget;
			public readonly bool IsLifeSpanTargetAssigned;

			public Listener(Action switchOnCallback, Action switchOffCallback, int order, ListenerLifeSpan lifeSpan, Object lifeSpanTarget)
			{
				SwitchOnCallback = switchOnCallback;
				SwitchOffCallback = switchOffCallback;
				IsSwitchOnCallbackTargetsUnityObject = switchOnCallback?.Target as Object; // As in: switchOnCallback.IsUnityObjectTargeted()
				IsSwitchOffCallbackTargetsUnityObject = switchOffCallback?.Target as Object; // As in: switchOffCallback.IsUnityObjectTargeted()
				Order = order;
				LifeSpan = lifeSpan;
				LifeSpanTarget = lifeSpanTarget;
				IsLifeSpanTargetAssigned = lifeSpanTarget != null;
			}

			public bool IsInvalid => SwitchOnCallback == null && SwitchOffCallback == null;

			public bool IsSwitchOnCallbackTargetedUnityObjectDestroyed => IsSwitchOnCallbackTargetsUnityObject && !(SwitchOnCallback.Target as Object); // As in: SwitchOnCallback.IsUnityObjectTargetedAndDestroyed
			public bool IsSwitchOffCallbackTargetedUnityObjectDestroyed => IsSwitchOffCallbackTargetsUnityObject && !(SwitchOffCallback.Target as Object); // As in: SwitchOffCallback.IsUnityObjectTargetedAndDestroyed
			public bool IsSwitchOnCallbackNullOrTargetedUnityObjectDestroyed => SwitchOnCallback == null || IsSwitchOnCallbackTargetedUnityObjectDestroyed;
			public bool IsSwitchOffCallbackNullOrTargetedUnityObjectDestroyed => SwitchOffCallback == null || IsSwitchOffCallbackTargetedUnityObjectDestroyed;

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
					       (IsSwitchOnCallbackNullOrTargetedUnityObjectDestroyed && IsSwitchOffCallbackNullOrTargetedUnityObjectDestroyed) ||
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

			public Action GetCallbackAndCheckIfAlive(bool isSwitchedOn)
			{
				var callback = isSwitchedOn
					? SwitchOnCallback
					: SwitchOffCallback;

				if (callback == null)
					return null;

				if (callback.Target is Object) // The same with: callback.IsUnityObjectTargeted()
				{
					return callback.Target as Object // The same with: callback.IsUnityObjectTargetedAndAlive()
						? callback
						: null;
				}
				else
				{
					return callback.Target != null
						? callback
						: null;
				}
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
		public List<Listener> _Listeners => Listeners;
		private readonly List<Listener> Listeners = new List<Listener>(10);

		public bool IsSwitchOnCallbackRegistered(Action callback)
		{
			for (var i = 0; i < Listeners.Count; i++)
			{
				if (Listeners[i].SwitchOnCallback == callback)
					return true;
			}
			return false;
		}

		public bool IsSwitchOffCallbackRegistered(Action callback)
		{
			for (var i = 0; i < Listeners.Count; i++)
			{
				if (Listeners[i].SwitchOffCallback == callback)
					return true;
			}
			return false;
		}

		public Listener GetListenerBySwitchOnCallback(Action callback)
		{
			for (var i = 0; i < Listeners.Count; i++)
			{
				if (Listeners[i].SwitchOnCallback == callback)
					return Listeners[i];
			}
			return default;
		}

		public Listener GetListenerBySwitchOffCallback(Action callback)
		{
			for (var i = 0; i < Listeners.Count; i++)
			{
				if (Listeners[i].SwitchOffCallback == callback)
					return Listeners[i];
			}
			return default;
		}

		public int ListenersCount => Listeners.Count;

		public int ListenersAliveCount
		{
			get
			{
				var count = 0;
				for (int i = Listeners.Count - 1; i >= 0; i--)
				{
					if (!Listeners[i].IsObjectDestroyed) // Check if the targeted object is destroyed
					{
						count++;
					}
				}
				return count;
			}
		}

		public bool IsAnyListenerRegistered => Listeners.Count > 0;

		public bool IsAnyAliveListenerRegistered
		{
			get
			{
				for (int i = Listeners.Count - 1; i >= 0; i--)
				{
					if (!Listeners[i].IsObjectDestroyed) // Check if the targeted object is destroyed
					{
						return true;
					}
				}
				return false;
			}
		}

		public void CleanUp()
		{
			if (IsInvoking)
				throw new Exception("Cleanup is not allowed while invoking.");

			for (int i = Listeners.Count - 1; i >= 0; i--)
			{
				if (Listeners[i].IsObjectDestroyed) // Check if the targeted object is destroyed
				{
					Listeners.RemoveAt(i);
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
				throw new NotSupportedException("Adding listener while invoking is not supported."); // See 117418312.
			if (order == int.MinValue || order == int.MaxValue) // These values are reserved for internal use.
				throw new ArgumentOutOfRangeException(nameof(order), order, "");
			if (switchOnCallback == null && switchOffCallback == null)
			{
				if (ExtenityEventTools.VerboseLogging)
					Log.Verbose($"Tried to add switch listener specifying both callbacks null with {_Detailed_OrderAndLifeSpan(order, lifeSpan, lifeSpanTarget)}.");
				return; // Silently ignore
			}
			if (ExtenityEventTools.VerboseLogging && switchOnCallback == null && lifeSpan == ListenerLifeSpan.RemovedAtFirstEmit)
				Log.Verbose($"Not usual to add listener with no SwitchOn callback when giving it a life span of {ListenerLifeSpan.RemovedAtFirstEmit}. See the listener with {_Detailed_OrderAndLifeSpanForMethodAndObject(order, lifeSpan, lifeSpanTarget, switchOnCallback, switchOffCallback)}.");

			// See if the callback pair was already registered.
			for (var i = 0; i < Listeners.Count; i++)
			{
				if (Listeners[i].HasCallbacks(switchOnCallback, switchOffCallback))
				{
					// The callback is already registered. See if there is a change in its parameters.
					if (Listeners[i].Order != order ||
					    Listeners[i].LifeSpan != lifeSpan ||
					    Listeners[i].LifeSpanTarget != lifeSpanTarget)
					{
						// Trying to add the same callback with different parameters. Just remove the existing one and
						// create a new one with new parameters. That should happen rarely, so no need to optimize this.
						Listeners.RemoveAt(i);
					}
					else
					{
						if (ExtenityEventTools.VerboseLogging)
							Log.Verbose($"Tried to add an already registered callback pair with {_Detailed_OrderAndLifeSpanForMethodAndObject(order, lifeSpan, lifeSpanTarget, switchOnCallback, switchOffCallback)}.");
						return; // Silently ignore
					}

					break; // No need to iterate others. Impossible to add a delegate more than once.
				}
			}

			// If the switch is On and the caller needs to be RemovedAtFirstEmit, we don't need to register the listener
			// to Listeners list. Just call the SwitchOn callback and move on.
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
			var listener = new Listener(switchOnCallback, switchOffCallback, order, lifeSpan, lifeSpanTarget);
			var done = false;
			if (Listeners.Count == 0 || // Line 1
			    order >= Listeners[Listeners.Count - 1].Order) // Line 2
			{
				if (ExtenityEventTools.VerboseLogging)
					Log.Verbose($"Adding listener with {_Detailed_OrderAndLifeSpanForMethodAndObject(order, lifeSpan, lifeSpanTarget, switchOnCallback, switchOffCallback)} at the end, resulting '{Listeners.Count + 1}' listener(s).");
				Listeners.Add(listener);
				done = true;
			}
			else
			{
				for (int i = 0; i < Listeners.Count; i++)
				{
					if (order < Listeners[i].Order)
					{
						if (ExtenityEventTools.VerboseLogging)
							Log.Verbose($"Adding listener with {_Detailed_OrderAndLifeSpanForMethodAndObject(order, lifeSpan, lifeSpanTarget, switchOnCallback, switchOffCallback)} at index '{i}', resulting '{Listeners.Count + 1}' listener(s).");
						Listeners.Insert(i, listener);
						done = true;
						break;
					}
				}
			}
			// This is a safety belt for the developer. Whatever optimization is going on inside the lines above would
			// only be useful if it leaves us with the listener added or inserted into the list no matter what.
			if (!done)
			{
				throw new InternalException(117357191); // The code should not reach here.
			}

			// See 118512052. Call the new callback instantly. It will be a safe call and there is no real reason
			// to provide an unsafe call option for just a single callback.
			var callback = listener.GetCallbackAndCheckIfAlive(IsSwitchedOn);
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
			if (switchOnCallback == null && switchOffCallback == null)
				return false; // Silently ignore

			for (var i = 0; i < Listeners.Count; i++)
			{
				if (Listeners[i].HasCallbacks(switchOnCallback, switchOffCallback))
				{
					if (ExtenityEventTools.VerboseLogging)
						Log.Verbose($"Removing listener with {_Detailed_OrderForMethodAndObject(Listeners[i].Order, switchOnCallback, switchOffCallback)} at index '{i}', resulting '{Listeners.Count - 1}' listener(s).");

					// Also shift the iteration index if currently invoking. Note that InvokeIndex will be -1 if not currently invoking.
					if (InvokeIndex >= i)
					{
						InvokeIndex--;
					}

					Listeners.RemoveAt(i);
					return true;
				}
			}
			if (ExtenityEventTools.VerboseLogging)
				Log.Verbose($"Failed to remove listener for {_Detailed_MethodAndObject(switchOnCallback, switchOffCallback)}.");
			return false;
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
				Log.Verbose("Removing all listeners.");

			Listeners.Clear();
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
				while (InvokeIndex < Listeners.Count)
				{
					var entry = Listeners[InvokeIndex];
					if (entry.IsObjectDestroyed)
					{
						Listeners.RemoveAt(InvokeIndex);
						continue;
					}
					if (isSwitchedOn && entry.ShouldRemoveAfterEmit)
					{
						// Remove the callback just before calling it. So that the caller can act like it's removed.
						//
						// Removing before the call also ensures that the callback will be removed even though
						// an exception is thrown inside the callback.
						Listeners.RemoveAt(InvokeIndex--);
					}

					var callback = entry.GetCallbackAndCheckIfAlive(isSwitchedOn);
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

			while (InvokeIndex < Listeners.Count)
			{
				var entry = Listeners[InvokeIndex];
				if (entry.IsObjectDestroyed)
				{
					Listeners.RemoveAt(InvokeIndex);
					continue;
				}
				if (isSwitchedOn && entry.ShouldRemoveAfterEmit)
				{
					// Remove the callback just before calling it. So that the caller can act like it's removed.
					//
					// Removing before the call also ensures that the callback will be removed even though
					// an exception is thrown inside the callback.
					Listeners.RemoveAt(InvokeIndex--);
				}

				var callback = entry.GetCallbackAndCheckIfAlive(isSwitchedOn);
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

		public string GetSwitchListenerDebugInfo(string linePrefix)
		{
			var stringBuilder = StringTools.SharedStringBuilder.Value;
			lock (stringBuilder)
			{
				stringBuilder.Clear(); // Make sure it is clean before starting to use.

				GetSwitchListenerDebugInfo(stringBuilder, linePrefix);

				var result = stringBuilder.ToString();
				StringTools.ClearSharedStringBuilder(stringBuilder); // Make sure we will leave it clean after use.
				return result;
			}
		}

		public void GetSwitchListenerDebugInfo(StringBuilder stringBuilder, string linePrefix)
		{
			for (var i = 0; i < Listeners.Count; i++)
			{
				var entry = Listeners[i];

				stringBuilder.Append(linePrefix);
				if (entry.IsObjectDestroyed)
				{
					stringBuilder.Append("(Unavailable) ");
				}
				stringBuilder.AppendLine(_Detailed_OrderAndLifeSpanForMethodAndObject(entry.Order, entry.LifeSpan, entry.LifeSpanTarget, entry.SwitchOnCallback, entry.SwitchOffCallback));
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
