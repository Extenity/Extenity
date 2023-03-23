#if UNITY
#define UnityFeatures
#endif

using System.Collections.Generic;
using System.Text;
using Extenity.DataToolbox;
using Action = System.Action;
using Delegate = System.Delegate;
using Exception = System.Exception;
using NotSupportedException = System.NotSupportedException;
using ArgumentOutOfRangeException = System.ArgumentOutOfRangeException;

// This is the way that Log system supports various Context types in different environments like
// both in Unity and in UniversalExtenity. Also don't add 'using UnityEngine' or 'using System'
// in this code file to prevent any possible confusions. Use 'using' selectively, like
// 'using Exception = System.Exception;'
// See 11746845.
#if UNITY
using ContextObject = UnityEngine.Object;
#else
using ContextObject = System.Object;
#endif

namespace Extenity.MessagingToolbox
{

	public class ExtenitySwitch
	{
		#region Listeners

		public struct Listener
		{
			public readonly Action SwitchOnCallback;
			public readonly Action SwitchOffCallback;
#if UnityFeatures
			public readonly bool IsSwitchOnCallbackTargetsUnityObject;
			public readonly bool IsSwitchOffCallbackTargetsUnityObject;
#endif
			public readonly int Order;
			public readonly ListenerLifeSpan LifeSpan;
#if UnityFeatures
			public readonly UnityEngine.Object LifeSpanTarget;
			public readonly bool IsLifeSpanTargetAssigned;
#endif

#if UnityFeatures
			public Listener(Action switchOnCallback, Action switchOffCallback, int order, ListenerLifeSpan lifeSpan, UnityEngine.Object lifeSpanTarget)
#else
			public Listener(Action switchOnCallback, Action switchOffCallback, int order, ListenerLifeSpan lifeSpan)
#endif
			{
				SwitchOnCallback = switchOnCallback;
				SwitchOffCallback = switchOffCallback;
#if UnityFeatures
				IsSwitchOnCallbackTargetsUnityObject = switchOnCallback?.Target as UnityEngine.Object; // As in: switchOnCallback.IsUnityObjectTargeted()
				IsSwitchOffCallbackTargetsUnityObject = switchOffCallback?.Target as UnityEngine.Object; // As in: switchOffCallback.IsUnityObjectTargeted()
#endif
				Order = order;
				LifeSpan = lifeSpan;
#if UnityFeatures
				LifeSpanTarget = lifeSpanTarget;
				IsLifeSpanTargetAssigned = lifeSpanTarget != null;
#endif
			}

			public bool IsInvalid => SwitchOnCallback == null && SwitchOffCallback == null;

#if UnityFeatures
			public bool IsSwitchOnCallbackTargetedUnityObjectDestroyed => IsSwitchOnCallbackTargetsUnityObject && !(SwitchOnCallback.Target as UnityEngine.Object); // As in: SwitchOnCallback.IsUnityObjectTargetedAndDestroyed
			public bool IsSwitchOffCallbackTargetedUnityObjectDestroyed => IsSwitchOffCallbackTargetsUnityObject && !(SwitchOffCallback.Target as UnityEngine.Object); // As in: SwitchOffCallback.IsUnityObjectTargetedAndDestroyed
			public bool IsSwitchOnCallbackNullOrTargetedUnityObjectDestroyed => SwitchOnCallback == null || IsSwitchOnCallbackTargetedUnityObjectDestroyed;
			public bool IsSwitchOffCallbackNullOrTargetedUnityObjectDestroyed => SwitchOffCallback == null || IsSwitchOffCallbackTargetedUnityObjectDestroyed;

			public bool IsLifeSpanTargetDestroyed => IsLifeSpanTargetAssigned && !LifeSpanTarget;
#endif

			public bool ShouldRemoveAfterEmit
			{
				get { return LifeSpan == ListenerLifeSpan.RemovedAtFirstEmit; }
			}

#if UnityFeatures
			public bool IsObjectDestroyed
			{
				get
				{
					return IsInvalid ||
					       (IsSwitchOnCallbackNullOrTargetedUnityObjectDestroyed && IsSwitchOffCallbackNullOrTargetedUnityObjectDestroyed) ||
					       IsLifeSpanTargetDestroyed;
				}
			}
#endif

			public ContextObject LogObject(bool isSwitchedOn)
			{
#if UnityFeatures
				// First, try to get callback delegate object. If not available, get the LifeSpanTarget object.
				var Callback = GetCallback(isSwitchedOn);
				if (Callback != null) // Check if the callback is specified by user. See 11853135.
				{
					var obj = Callback.Target as UnityEngine.Object;
					if (obj)
						return obj;
				}
				return LifeSpanTarget;
#else
				var Callback = GetCallback(isSwitchedOn);
				if (Callback != null) // Check if the callback is specified by user. See 11853135.
				{
					return Callback.Target as ContextObject;
				}
				return null;
#endif
			}

			public Action GetCallback(bool isSwitchedOn)
			{
				return isSwitchedOn
					? SwitchOnCallback
					: SwitchOffCallback;
			}

			public Action GetCallbackAndCheckIfAlive(bool isSwitchedOn)
			{
				var Callback = GetCallback(isSwitchedOn);

				if (Callback == null)
					return null;

#if UnityFeatures
				if (Callback.Target is UnityEngine.Object) // The same with: callback.IsUnityObjectTargeted()
				{
					return (Callback.Target as UnityEngine.Object) // The same with: callback.IsUnityObjectTargetedAndAlive()
						? Callback
						: null;
				}
				else
#endif
				{
					return Callback;
					// This was the previous implementation, which failed static non-UnityObject methods.
					// return Callback.Target != null ? Callback : null;
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

#if UnityFeatures
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
#endif

		public bool IsAnyListenerRegistered => Listeners.Count > 0;

#if UnityFeatures
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
#endif

		#endregion

		#region Add / Remove Listener

		/// <param name="order">Lesser ordered callback gets called earlier. Callbacks that have the same order gets called in the order of AddListener calls. Negative values are allowed.</param>
#if UnityFeatures
		public void AddListener(Action switchOnCallback, Action switchOffCallback, int order = 0, ListenerLifeSpan lifeSpan = ListenerLifeSpan.Permanent, UnityEngine.Object lifeSpanTarget = null)
#else
		public void AddListener(Action switchOnCallback, Action switchOffCallback, int order = 0, ListenerLifeSpan lifeSpan = ListenerLifeSpan.Permanent)
#endif
		{
			if (IsInvoking)
				throw new NotSupportedException("Adding listener while invoking is not supported."); // See 117418312.
			if (order == int.MinValue || order == int.MaxValue) // These values are reserved for internal use.
				throw new ArgumentOutOfRangeException(nameof(order), order, "");
			if (switchOnCallback == null && switchOffCallback == null)
			{
				if (ExtenityEventTools.VerboseLogging)
				{
#if UnityFeatures
					Log.Verbose($"Tried to add switch listener specifying both callbacks null with {_Detailed_OrderAndLifeSpan(order, lifeSpan, lifeSpanTarget)}.");
#else
					Log.Verbose($"Tried to add switch listener specifying both callbacks null with {_Detailed_OrderAndLifeSpan(order, lifeSpan)}.");
#endif
				}
				return; // Silently ignore
			}
			if (ExtenityEventTools.VerboseLogging && switchOnCallback == null && lifeSpan == ListenerLifeSpan.RemovedAtFirstEmit)
			{
#if UnityFeatures
				Log.Verbose($"Not usual to add listener with no SwitchOn callback when giving it a life span of {ListenerLifeSpan.RemovedAtFirstEmit}. See the listener with {_Detailed_OrderAndLifeSpanForMethodAndObject(new Listener(switchOnCallback, switchOffCallback, order, lifeSpan, lifeSpanTarget))}.");
#else
				Log.Verbose($"Not usual to add listener with no SwitchOn callback when giving it a life span of {ListenerLifeSpan.RemovedAtFirstEmit}. See the listener with {_Detailed_OrderAndLifeSpanForMethodAndObject(new Listener(switchOnCallback, switchOffCallback, order, lifeSpan))}.");
#endif
			}

			// See if the callback pair was already registered.
			for (var i = 0; i < Listeners.Count; i++)
			{
				if (Listeners[i].HasCallbacks(switchOnCallback, switchOffCallback))
				{
					// The callback is already registered. See if there is a change in its parameters.
					if (Listeners[i].Order != order ||
					    Listeners[i].LifeSpan != lifeSpan
#if UnityFeatures
					    || Listeners[i].LifeSpanTarget != lifeSpanTarget
#endif
					    )
					{
						// Trying to add the same callback with different parameters. Just remove the existing one and
						// create a new one with new parameters. That should happen rarely, so no need to optimize this.
						Listeners.RemoveAt(i);
					}
					else
					{
						if (ExtenityEventTools.VerboseLogging)
							Log.Verbose($"Tried to add an already registered callback pair with {_Detailed_OrderAndLifeSpanForMethodAndObject(Listeners[i])}.");
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
						InvokingSwitchOnCallback = switchOnCallback;
						InvokingSwitchOffCallback = switchOffCallback;
						switchOnCallback();
						InvokingSwitchOnCallback = null;
						InvokingSwitchOffCallback = null;
					}
					catch (Exception exception)
					{
						Log.Error(exception);
					}
					finally
					{
						IsInvoking = false;
						InvokingSwitchOnCallback = null;
						InvokingSwitchOffCallback = null;
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
#if UnityFeatures
			var listener = new Listener(switchOnCallback, switchOffCallback, order, lifeSpan, lifeSpanTarget);
#else
			var listener = new Listener(switchOnCallback, switchOffCallback, order, lifeSpan);
#endif
			var done = false;
			if (Listeners.Count == 0 || // Line 1
			    order >= Listeners[Listeners.Count - 1].Order) // Line 2
			{
				if (ExtenityEventTools.VerboseLogging)
					Log.Verbose($"Adding listener with {_Detailed_OrderAndLifeSpanForMethodAndObject(listener)} at the end, resulting '{Listeners.Count + 1}' listener(s).");
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
							Log.Verbose($"Adding listener with {_Detailed_OrderAndLifeSpanForMethodAndObject(listener)} at index '{i}', resulting '{Listeners.Count + 1}' listener(s).");
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
					InvokingSwitchOnCallback = listener.SwitchOnCallback;
					InvokingSwitchOffCallback = listener.SwitchOffCallback;
					callback();
					InvokingSwitchOnCallback = null;
					InvokingSwitchOffCallback = null;
				}
				catch (Exception exception)
				{
					Log.Error(exception);
				}
				finally
				{
					IsInvoking = false;
					InvokingSwitchOnCallback = null;
					InvokingSwitchOffCallback = null;
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

		public void RemoveCurrentListener()
		{
			if (!IsInvoking)
				throw new Exception("Tried to remove current listener outside of listener callback.");

			// There is a possibility that user may call RemoveCurrentListener multiple times. So we must handle that
			// too. Thankfully RemoveListener checks if the callback exists.
			RemoveListener(InvokingSwitchOnCallback, InvokingSwitchOffCallback);
		}

		public void RemoveAllListeners()
		{
			if (IsInvoking)
				throw new NotSupportedException("Operations while invoking are not supported."); // See 117418312.
			if (ExtenityEventTools.VerboseLogging)
				Log.Verbose("Removing all listeners.");

			Listeners.Clear();
		}

		// TODO: Needs testing (Only roughly tested). See RemoveListener(Action callback) tests and apply the same where possible. Should test with both System.Object and UnityEngine.Object targets.
		// TODO: Copy to other ExtenityEvent generic implementations
		public void RemoveAllListenersThatTargets(object callbackTarget)
		{
			if (IsInvoking)
				throw new NotSupportedException("Operations while invoking are not supported."); // See 117418312.
			if (ExtenityEventTools.VerboseLogging)
				Log.Verbose($"Removing all listeners with {_Detailed_CallbackTarget(callbackTarget)}.");

			// if (callbackTarget == null) Nope! Removing null callback targets are also supported to allow removing static method callbacks.
			// 	return; // Silently ignore

			var removedCount = 0;

			for (var i = 0; i < Listeners.Count; i++)
			{
				if ((Listeners[i].SwitchOnCallback != null &&
				     Listeners[i].SwitchOnCallback.Target == callbackTarget)
				    ||
				    (Listeners[i].SwitchOffCallback != null &&
				     Listeners[i].SwitchOffCallback.Target == callbackTarget))
				{
					if (ExtenityEventTools.VerboseLogging)
						Log.Verbose($"Removing listener with {_Detailed_CallbackTarget(callbackTarget)} at index '{i}', resulting '{Listeners.Count - 1}' listener(s).");

					// Also shift the iteration index if currently invoking. Note that InvokeIndex will be -1 if not currently invoking.
					if (InvokeIndex >= i)
					{
						InvokeIndex--;
					}

					Listeners.RemoveAt(i);
					i--;
					removedCount++;
				}
			}
			if (removedCount == 0)
			{
				if (ExtenityEventTools.VerboseLogging)
					Log.Verbose($"Failed to remove any listeners for {_Detailed_CallbackTarget(callbackTarget)}.");
			}
		}

		#endregion

		#region Switch

		public bool IsSwitchedOn { get; private set; }

		private bool IsInvoking = false;
		private int InvokeIndex = -1;
		private Action InvokingSwitchOnCallback;
		private Action InvokingSwitchOffCallback;

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
				Log.Fatal($"Invoked switch {(isSwitchedOn ? "on" : "off")} while an invocation is ongoing.");
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
					var listener = Listeners[InvokeIndex];
#if UnityFeatures
					if (listener.IsObjectDestroyed)
					{
						Listeners.RemoveAt(InvokeIndex);
						continue;
					}
#endif
					if (isSwitchedOn && listener.ShouldRemoveAfterEmit)
					{
						// Remove the callback just before calling it. So that the caller can act like it's removed.
						//
						// Removing before the call also ensures that the callback will be removed even though
						// an exception is thrown inside the callback.
						Listeners.RemoveAt(InvokeIndex--);
					}

					var callback = listener.GetCallbackAndCheckIfAlive(isSwitchedOn);
					if (callback != null) // Check if the callback is specified by user. See 11853135.
					{
						InvokingSwitchOnCallback = listener.SwitchOnCallback;
						InvokingSwitchOffCallback = listener.SwitchOffCallback;
						callback();
						InvokingSwitchOnCallback = null;
						InvokingSwitchOffCallback = null;
					}

					InvokeIndex++;
				}
			}
			finally
			{
				IsInvoking = false;
				InvokeIndex = -1;
				InvokingSwitchOnCallback = null;
				InvokingSwitchOffCallback = null;
			}
		}

		public void SwitchSafe(bool isSwitchedOn)
		{
			if (IsInvoking)
			{
				Log.Fatal($"Invoked switch {(isSwitchedOn ? "on" : "off")} while an invocation is ongoing.");
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
				var listener = Listeners[InvokeIndex];
#if UnityFeatures
				if (listener.IsObjectDestroyed)
				{
					Listeners.RemoveAt(InvokeIndex);
					continue;
				}
#endif
				if (isSwitchedOn && listener.ShouldRemoveAfterEmit)
				{
					// Remove the callback just before calling it. So that the caller can act like it's removed.
					//
					// Removing before the call also ensures that the callback will be removed even though
					// an exception is thrown inside the callback.
					Listeners.RemoveAt(InvokeIndex--);
				}

				var callback = listener.GetCallbackAndCheckIfAlive(isSwitchedOn);
				if (callback != null) // Check if the callback is specified by user. See 11853135.
				{
					try
					{
						InvokingSwitchOnCallback = listener.SwitchOnCallback;
						InvokingSwitchOffCallback = listener.SwitchOffCallback;
						callback();
						InvokingSwitchOnCallback = null;
						InvokingSwitchOffCallback = null;
					}
					catch (Exception exception)
					{
						Log.ErrorWithContext(listener.LogObject(isSwitchedOn), exception);
					}
				}

				InvokeIndex++;
			}

			IsInvoking = false;
			InvokeIndex = -1;
			InvokingSwitchOnCallback = null;
			InvokingSwitchOffCallback = null;
		}

		#endregion

		#region Log

		private static readonly Logger Log = new("ExtenitySwitch");

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
				var listener = Listeners[i];

				stringBuilder.Append(linePrefix);
#if UnityFeatures
				if (listener.IsObjectDestroyed)
				{
					stringBuilder.Append("(Unavailable) ");
				}
#endif
				stringBuilder.AppendLine(_Detailed_OrderAndLifeSpanForMethodAndObject(listener));
			}
		}

		private string _Detailed_CallbackTarget(object callbackTarget)
		{
			return $"callback target '{callbackTarget.FullObjectName()}'";
		}

		private string _Detailed_MethodAndObject(Delegate switchOnCallback, Delegate switchOffCallback)
		{
			return $"SwitchOn method '{switchOnCallback.FullNameOfTargetAndMethod()}' and SwitchOff method '{switchOffCallback.FullNameOfTargetAndMethod()}'";
		}

		private string _Detailed_OrderForMethodAndObject(int order, Delegate switchOnCallback, Delegate switchOffCallback)
		{
			return $"order '{order}' for {_Detailed_MethodAndObject(switchOnCallback, switchOffCallback)}";
		}

		private string _Detailed_OrderAndLifeSpan(in Listener listener)
		{
#if UnityFeatures
			return _Detailed_OrderAndLifeSpan(listener.Order, listener.LifeSpan, listener.LifeSpanTarget);
#else
			return _Detailed_OrderAndLifeSpan(listener.Order, listener.LifeSpan);
#endif
		}

#if UnityFeatures
		private string _Detailed_OrderAndLifeSpan(int order, ListenerLifeSpan lifeSpan, UnityEngine.Object lifeSpanTarget)
		{
			return $"order '{order}' and life span '{lifeSpan}{(lifeSpanTarget ? $" with target '{lifeSpanTarget.FullObjectName()}'" : "")}'";
		}
#else
		private string _Detailed_OrderAndLifeSpan(int order, ListenerLifeSpan lifeSpan)
		{
			return $"order '{order}' and life span '{lifeSpan}'";
		}
#endif

		private string _Detailed_OrderAndLifeSpanForMethodAndObject(in Listener listener)
		{
			return $"{_Detailed_OrderAndLifeSpan(listener)} for {_Detailed_MethodAndObject(listener.SwitchOnCallback, listener.SwitchOffCallback)}";
		}

		#endregion
	}

}
