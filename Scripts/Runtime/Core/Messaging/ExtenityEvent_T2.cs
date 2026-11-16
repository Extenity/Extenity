#if UNITY_5_3_OR_NEWER
#define UnityFeatures
#endif

#if UnityFeatures && (DEBUG || UNITY_EDITOR)
#define EnableProfiling
#endif

using System.Collections.Generic;
using System.Text;
using Extenity.DataToolbox;
using System; // Unfortunately this is not possible: using Action = System.Action<T1, T2>;
using Delegate = System.Delegate;
using Exception = System.Exception;
using NotImplementedException = System.NotImplementedException;
using NotSupportedException = System.NotSupportedException;
using ArgumentOutOfRangeException = System.ArgumentOutOfRangeException;
using RuntimeHelpers = System.Runtime.CompilerServices.RuntimeHelpers;

#if EnableProfiling
using Unity.Profiling;
#endif

// This is the way that Log system supports various Context types in different environments like
// both in Unity and in UniversalExtenity. Also don't add 'using UnityEngine' or 'using System'
// in this code file to prevent any possible confusions. Use 'using' selectively, like
// 'using Exception = System.Exception;'
// See 11746845.
#if UNITY_5_3_OR_NEWER
using ContextObject = UnityEngine.Object;
#else
using ContextObject = System.Object;
#endif

namespace Extenity.MessagingToolbox
{

	public class ExtenityEvent<T1, T2>
	{
		#region Profiler Markers

#if EnableProfiling

		// Keyed by method rather than by delegate, so the cache stays bounded by the number of distinct listener
		// methods and does not keep delegate targets alive. The marker is resolved once when the listener is
		// created and stored in the listener, so invoking does not need to look it up.
		private static readonly Dictionary<System.Reflection.MethodInfo, ProfilerMarker> ProfilerMarkerCache = new Dictionary<System.Reflection.MethodInfo, ProfilerMarker>();

		private static ProfilerMarker GetOrCreateProfilerMarker(Action<T1, T2> callback)
		{
			var method = callback.Method;
			if (!ProfilerMarkerCache.TryGetValue(method, out var marker))
			{
				marker = new ProfilerMarker(ExtenityEventTools.GetProfilerMarkerName(method));
				ProfilerMarkerCache[method] = marker;
			}
			return marker;
		}

#endif

		#endregion

		#region Listeners

		/// <summary>
		/// A registered callback and its registration parameters. It stays in the listener list for a short while
		/// after it is removed (see <see cref="IsRemoved"/>), until the list is compacted.
		/// </summary>
		public sealed class Listener
		{
			private Action<T1, T2> _Callback;
			public Action<T1, T2> Callback => _Callback;
#if UnityFeatures
			public readonly bool IsCallbackTargetsUnityObject;
#endif
			public readonly int Order;
			public readonly ListenerLifeSpan LifeSpan;
#if UnityFeatures
			private UnityEngine.Object _LifeSpanTarget;
			public UnityEngine.Object LifeSpanTarget => _LifeSpanTarget;
			public readonly bool IsLifeSpanTargetAssigned;
#endif
#if EnableProfiling
			public readonly ProfilerMarker ProfilerMarker;
#endif

			private bool _IsRemoved;
			/// <summary>
			/// Whether the listener was removed from its event. A removed listener is never called again. It only
			/// waits in the listener list to be dropped when the list is compacted.
			/// </summary>
			public bool IsRemoved => _IsRemoved;

#if UnityFeatures
			public Listener(Action<T1, T2> callback, int order, ListenerLifeSpan lifeSpan, UnityEngine.Object lifeSpanTarget)
#else
			public Listener(Action<T1, T2> callback, int order, ListenerLifeSpan lifeSpan)
#endif
			{
				_Callback = callback;
#if UnityFeatures
				IsCallbackTargetsUnityObject = callback?.Target as UnityEngine.Object; // As in: callback.IsUnityObjectTargeted()
#endif
				Order = order;
				LifeSpan = lifeSpan;
#if UnityFeatures
				_LifeSpanTarget = lifeSpanTarget;
				IsLifeSpanTargetAssigned = lifeSpanTarget != null;
#endif
#if EnableProfiling
				ProfilerMarker = callback != null ? GetOrCreateProfilerMarker(callback) : default;
#endif
			}

			/// <summary>
			/// Marks the listener as removed. Releasing the references lets the callback's target be garbage
			/// collected without waiting for the listener list to be compacted.
			/// </summary>
			internal void MarkAsRemoved(bool releaseReferences)
			{
				_IsRemoved = true;
				if (releaseReferences)
				{
					_Callback = null;
#if UnityFeatures
					_LifeSpanTarget = null;
#endif
				}
			}

			public bool IsInvalid => _Callback == null;

#if UnityFeatures
			public bool IsCallbackTargetedUnityObjectDestroyed => IsCallbackTargetsUnityObject && _Callback != null && !(_Callback.Target as UnityEngine.Object); // As in: SwitchOnCallback.IsUnityObjectTargetedAndDestroyed
			public bool IsCallbackNullOrTargetedUnityObjectDestroyed => _Callback == null || IsCallbackTargetedUnityObjectDestroyed;

			public bool IsLifeSpanTargetDestroyed => IsLifeSpanTargetAssigned && !_LifeSpanTarget;
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
					       IsCallbackTargetedUnityObjectDestroyed ||
					       IsLifeSpanTargetDestroyed;
				}
			}
#endif

			public ContextObject LogObject
			{
				get
				{
#if UnityFeatures
					// First, try to get callback delegate object. If not available, get the LifeSpanTarget object.
					if (_Callback != null) // Check if the callback is specified by user. See 11853135.
					{
						var obj = _Callback.Target as UnityEngine.Object;
						if (obj)
							return obj;
					}
					return _LifeSpanTarget;
#else
					if (_Callback != null) // Check if the callback is specified by user. See 11853135.
					{
						return _Callback.Target as ContextObject;
					}
					return null;
#endif
				}
			}

			public Action<T1, T2> GetCallbackAndCheckIfAlive()
			{
				if (_Callback == null)
					return null;

#if UnityFeatures
				if (_Callback.Target is UnityEngine.Object) // The same with: callback.IsUnityObjectTargeted()
				{
					return (_Callback.Target as UnityEngine.Object) // The same with: callback.IsUnityObjectTargetedAndAlive()
						? _Callback
						: null;
				}
				else
#endif
				{
					return _Callback;
					// This was the previous implementation, which failed static non-UnityObject methods.
					// return Callback.Target != null ? Callback : null;
				}
			}

#if UnityFeatures
			/// <summary>
			/// Gives the same result as <see cref="GetCallbackAndCheckIfAlive"/>, but only for a listener that already
			/// passed the <see cref="IsObjectDestroyed"/> check. Skips checking the callback target's liveness again.
			/// </summary>
			public Action<T1, T2> GetCallbackOfNotDestroyedListener()
			{
				// IsObjectDestroyed already ensured that the callback is not null and, if its target was an alive Unity
				// object at registration, that the target is still alive. A target that was an already destroyed Unity
				// object at registration is not covered by IsObjectDestroyed and it can't be alive now.
				if (IsCallbackTargetsUnityObject || !(_Callback.Target is UnityEngine.Object))
					return _Callback;
				return null;
			}
#endif
		}

		/// <summary>
		/// Hashes a callback by the identity of its target object, so the many callbacks of the same method on
		/// different objects spread over the buckets. Callbacks of static methods have no target and are hashed by
		/// their method. Equality is the usual delegate equality: the same target and the same method.
		/// </summary>
		private sealed class CallbackComparer : IEqualityComparer<Action<T1, T2>>
		{
			public static readonly CallbackComparer Instance = new CallbackComparer();

			public bool Equals(Action<T1, T2> x, Action<T1, T2> y)
			{
				return x == y;
			}

			public int GetHashCode(Action<T1, T2> callback)
			{
				var target = callback.Target;
				return target != null
					? RuntimeHelpers.GetHashCode(target)
					: callback.Method.GetHashCode();
			}
		}

		/// <summary>
		/// CAUTION! Do not modify! Use AddListener and RemoveListener instead.
		/// </summary>
		public List<Listener> _Listeners
		{
			get
			{
				if (RemovedListenerCount > 0)
				{
					Compact();
				}
				return Listeners;
			}
		}

		/// <summary>
		/// The listeners in invocation order. Removed listeners stay in the list, marked as removed, until
		/// <see cref="Compact"/> drops them in a single pass. That way removing a listener costs the same however
		/// many listeners there are.
		/// </summary>
		private readonly List<Listener> Listeners = new List<Listener>(10);

		/// <summary>
		/// Finds the listener of a callback without searching the list. Holds the registered listeners only, not the
		/// removed ones that still wait in the list.
		/// </summary>
		private readonly Dictionary<Action<T1, T2>, Listener> ListenersByCallback = new Dictionary<Action<T1, T2>, Listener>(CallbackComparer.Instance);

		/// <summary>How many listeners in the list are marked as removed and wait to be dropped.</summary>
		private int RemovedListenerCount;

		/// <summary>
		/// Outside of invoking, the list is compacted when the removed listeners waiting in it outnumber both this
		/// value and the registered listeners. That keeps the list from growing without bounds when listeners are
		/// added and removed many times without invoking in between, and still costs a constant time per removal
		/// on average.
		/// </summary>
		private const int MinimumRemovedListenerCountToCompact = 32;

		public bool IsCallbackRegistered(Action<T1, T2> callback)
		{
			return callback != null && ListenersByCallback.ContainsKey(callback);
		}

		/// <returns>The listener of the callback, or null if the callback is not registered.</returns>
		public Listener GetListenerByCallback(Action<T1, T2> callback)
		{
			if (callback != null && ListenersByCallback.TryGetValue(callback, out var listener))
				return listener;
			return null;
		}

		public int ListenersCount => ListenersByCallback.Count;

#if UnityFeatures
		public int ListenersAliveCount
		{
			get
			{
				var count = 0;
				for (int i = Listeners.Count - 1; i >= 0; i--)
				{
					var listener = Listeners[i];
					if (!listener.IsRemoved && !listener.IsObjectDestroyed) // Check if the targeted object is destroyed
					{
						count++;
					}
				}
				return count;
			}
		}
#endif

		public bool IsAnyListenerRegistered => ListenersByCallback.Count > 0;

#if UnityFeatures
		public bool IsAnyAliveListenerRegistered
		{
			get
			{
				for (int i = Listeners.Count - 1; i >= 0; i--)
				{
					var listener = Listeners[i];
					if (!listener.IsRemoved && !listener.IsObjectDestroyed) // Check if the targeted object is destroyed
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

			for (var i = 0; i < Listeners.Count; i++)
			{
				var listener = Listeners[i];
				if (!listener.IsRemoved && listener.IsObjectDestroyed)
				{
					MarkAsRemoved(listener);
				}
			}
			if (RemovedListenerCount > 0)
			{
				Compact();
			}
		}
#endif

		/// <summary>
		/// Takes the listener out of the registered listeners and marks it as removed. It stays in the list until
		/// <see cref="Compact"/>.
		/// </summary>
		private void MarkAsRemoved(Listener listener)
		{
			if (!ListenersByCallback.Remove(listener.Callback))
			{
				throw new InternalException(117357192); // The list and the lookup must hold the same listeners.
			}
			// The ongoing invocation may still need the callback and the life span target of the listener it is
			// calling (for example to log an exception thrown by a callback that removed itself). Compacting at the
			// end of the invocation drops the listener anyway.
			listener.MarkAsRemoved(releaseReferences: !IsInvoking);
			RemovedListenerCount++;
		}

		/// <summary>
		/// Drops the listeners that are marked as removed from the list, in a single pass. Allowed while invoking.
		/// </summary>
		private void Compact()
		{
			var writeIndex = 0;
			var invokeIndex = InvokeIndex; // -1 if not currently invoking.
			for (var readIndex = 0; readIndex < Listeners.Count; readIndex++)
			{
				var listener = Listeners[readIndex];
				if (listener.IsRemoved)
				{
					// Keep the invocation at the same listener. If that one is being dropped, the invocation
					// continues with the next listener that stays.
					if (readIndex <= InvokeIndex)
					{
						invokeIndex--;
					}
					continue;
				}
				Listeners[writeIndex++] = listener;
			}
			Listeners.RemoveRange(writeIndex, Listeners.Count - writeIndex);
			RemovedListenerCount = 0;
			InvokeIndex = invokeIndex;
		}

		#endregion

		#region Add / Remove Listener

		/// <summary>
		/// Adding a callback that is already registered logs a warning. With the same parameters, the second
		/// registration is ignored. With different parameters, it replaces the existing registration.
		/// </summary>
		/// <param name="order">Lesser ordered callbacks are called earlier. Negative values are allowed. Callbacks that have the same order are called in registration order. You can easily see order of all callbacks in Tools>Extenity>Application>Loop window.</param>
#if UnityFeatures
		public void AddListener(Action<T1, T2> callback, int order = 0, ListenerLifeSpan lifeSpan = ListenerLifeSpan.Permanent, UnityEngine.Object lifeSpanTarget = null)
#else
		public void AddListener(Action<T1, T2> callback, int order = 0, ListenerLifeSpan lifeSpan = ListenerLifeSpan.Permanent)
#endif
		{
			if (IsInvoking)
				throw new NotSupportedException("Adding listener while invoking is not supported."); // See 117418312.
			if (order == int.MinValue || order == int.MaxValue) // These values are reserved for internal use.
				throw new ArgumentOutOfRangeException(nameof(order), order, "");
			if (callback == null)
			{
				if (ExtenityEventTools.VerboseLogging)
				{
#if UnityFeatures
					Log.Verbose($"Tried to add event listener specifying null callback with {_Detailed_OrderAndLifeSpan(order, lifeSpan, lifeSpanTarget)}.");
#else
					Log.Verbose($"Tried to add event listener specifying null callback with {_Detailed_OrderAndLifeSpan(order, lifeSpan)}.");
#endif
				}
				return; // Silently ignore
			}

			// See if the callback was already registered.
			if (ListenersByCallback.TryGetValue(callback, out var existingListener))
			{
				var isSameParameters = existingListener.Order == order &&
				                       existingListener.LifeSpan == lifeSpan
#if UnityFeatures
				                       && existingListener.LifeSpanTarget == lifeSpanTarget
#endif
					;
				if (isSameParameters)
				{
					Log.WarningWithContext(existingListener.LogObject, $"Tried to add an already registered callback with {_Detailed_OrderAndLifeSpanForMethodAndObject(existingListener)}. The second registration is ignored.");
					return;
				}

				// Trying to add the same callback with different parameters. Remove the existing one and create a
				// new one with new parameters.
#if UnityFeatures
				Log.WarningWithContext(existingListener.LogObject, $"Tried to add an already registered callback with {_Detailed_OrderAndLifeSpan(order, lifeSpan, lifeSpanTarget)}, which was registered with {_Detailed_OrderAndLifeSpanForMethodAndObject(existingListener)}. The new registration replaces the existing one.");
#else
				Log.WarningWithContext(existingListener.LogObject, $"Tried to add an already registered callback with {_Detailed_OrderAndLifeSpan(order, lifeSpan)}, which was registered with {_Detailed_OrderAndLifeSpanForMethodAndObject(existingListener)}. The new registration replaces the existing one.");
#endif
				MarkAsRemoved(existingListener);
			}

#if UnityFeatures
			var listener = new Listener(callback, order, lifeSpan, lifeSpanTarget);
#else
			var listener = new Listener(callback, order, lifeSpan);
#endif
			ListenersByCallback.Add(callback, listener);

			// Cover all the shortcuts that we can add the listener at the end of the list and move on.
			//
			// Line 1: If there is no other listener registered, don't bother the ordering. Just add it and move on.
			//
			// Line 2: If trying to add a callback with 0 order and the last item is ordered as 0, fear not!
			// Just add it to the end and move on. While doing that, also cover the possibility that the order
			// is greater(or equal) than the last item's order.
			//
			// Note that the listeners that are marked as removed keep their order. So the list is always sorted by
			// order, whether they are dropped yet or not.
			if (Listeners.Count == 0 || // Line 1
			    order >= Listeners[Listeners.Count - 1].Order) // Line 2
			{
				if (ExtenityEventTools.VerboseLogging)
					Log.Verbose($"Adding listener with {_Detailed_OrderAndLifeSpanForMethodAndObject(listener)} at the end, resulting '{ListenersByCallback.Count}' listener(s).");
				Listeners.Add(listener);
			}
			else
			{
				// Binary search for the first listener that has a greater order. Inserting there puts the listener
				// after the ones that have the same order, which keeps the registration order among them.
				var low = 0;
				var high = Listeners.Count;
				while (low < high)
				{
					var middle = low + (high - low) / 2;
					if (Listeners[middle].Order <= order)
					{
						low = middle + 1;
					}
					else
					{
						high = middle;
					}
				}
				if (ExtenityEventTools.VerboseLogging)
					Log.Verbose($"Adding listener with {_Detailed_OrderAndLifeSpanForMethodAndObject(listener)} in between, resulting '{ListenersByCallback.Count}' listener(s).");
				Listeners.Insert(low, listener);
			}
		}

		public bool RemoveListener(Action<T1, T2> callback)
		{
			if (callback == null)
				return false; // Silently ignore

			if (!ListenersByCallback.TryGetValue(callback, out var listener))
			{
				if (ExtenityEventTools.VerboseLogging)
					Log.Verbose($"Failed to remove listener for {_Detailed_MethodAndObject(callback)}.");
				return false;
			}

			if (ExtenityEventTools.VerboseLogging)
				Log.Verbose($"Removing listener with {_Detailed_OrderForMethodAndObject(listener.Order, callback)}, resulting '{ListenersByCallback.Count - 1}' listener(s).");

			MarkAsRemoved(listener);

			// An ongoing invocation compacts the list when it ends.
			if (!IsInvoking &&
			    RemovedListenerCount >= MinimumRemovedListenerCountToCompact &&
			    RemovedListenerCount > ListenersByCallback.Count)
			{
				Compact();
			}
			return true;
		}

		public void RemoveCurrentListener()
		{
			if (!IsInvoking)
				throw new Exception("Tried to remove current listener outside of listener callback.");

			// There is a possibility that user may call RemoveCurrentListener multiple times. So we must handle that
			// too. Thankfully RemoveListener checks if the callback exists.
			RemoveListener(InvokingCallback);
		}

		public void RemoveAllListeners()
		{
			if (IsInvoking)
				throw new NotSupportedException("Operations while invoking are not supported."); // See 117418312.
			if (ExtenityEventTools.VerboseLogging)
				Log.Verbose("Removing all listeners.");

			Listeners.Clear();
			ListenersByCallback.Clear();
			RemovedListenerCount = 0;
		}

		// TODO: Needs testing (Only roughly tested). See RemoveListener tests and apply the same where possible. Should test with both System.Object and UnityEngine.Object targets.
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
				var listener = Listeners[i];
				if (!listener.IsRemoved &&
				    listener.Callback != null &&
				    listener.Callback.Target == callbackTarget)
				{
					if (ExtenityEventTools.VerboseLogging)
						Log.Verbose($"Removing listener with {_Detailed_CallbackTarget(callbackTarget)}, resulting '{ListenersByCallback.Count - 1}' listener(s).");

					MarkAsRemoved(listener);
					removedCount++;
				}
			}
			if (removedCount == 0)
			{
				if (ExtenityEventTools.VerboseLogging)
					Log.Verbose($"Failed to remove any listeners for {_Detailed_CallbackTarget(callbackTarget)}.");
			}
			else
			{
				Compact();
			}
		}

		#endregion

		#region Invoke

		private bool IsInvoking = false;
		private int InvokeIndex = -1;
		private Action<T1, T2> InvokingCallback;

		public void InvokeUnsafe(T1 param1, T2 param2)
		{
			if (IsInvoking)
			{
				// See 11861024.
				throw new NotSupportedException("Invoked event while an invocation is ongoing.");
			}
			IsInvoking = true;
			InvokeIndex = 0;

			try
			{
				while (InvokeIndex < Listeners.Count)
				{
					var listener = Listeners[InvokeIndex];
					if (listener.IsRemoved)
					{
						InvokeIndex++;
						continue;
					}
#if UnityFeatures
					if (listener.IsObjectDestroyed)
					{
						// Dropped from the list in a single pass after invoking. Dropping here would shift the rest
						// of the list for each destroyed listener, which costs quadratic time when many objects are
						// destroyed.
						MarkAsRemoved(listener);
						InvokeIndex++;
						continue;
					}
#endif
					if (listener.ShouldRemoveAfterEmit)
					{
						// Remove the callback just before calling it. So that the caller can act like it's removed.
						//
						// Removing before the call also ensures that the callback will be removed even though
						// an exception is thrown inside the callback.
						MarkAsRemoved(listener);
					}

#if UnityFeatures
					var callback = listener.GetCallbackOfNotDestroyedListener(); // IsObjectDestroyed is checked above.
#else
					var callback = listener.GetCallbackAndCheckIfAlive();
#endif
					if (callback != null) // Check if the callback is specified by user. See 11853135.
					{
						InvokingCallback = listener.Callback;
#if EnableProfiling
						using (listener.ProfilerMarker.Auto())
#endif
						{
							callback(param1, param2);
						}
						InvokingCallback = null;
					}

					InvokeIndex++;
				}
			}
			finally
			{
				IsInvoking = false;
				InvokeIndex = -1;
				InvokingCallback = null;
				if (RemovedListenerCount > 0)
				{
					Compact();
				}
			}
		}

		public void InvokeSafe(T1 param1, T2 param2)
		{
			if (IsInvoking)
			{
				// See 11861024.
				throw new NotSupportedException("Invoked event while an invocation is ongoing.");
			}
			IsInvoking = true;
			InvokeIndex = 0;

			while (InvokeIndex < Listeners.Count)
			{
				var listener = Listeners[InvokeIndex];
				if (listener.IsRemoved)
				{
					InvokeIndex++;
					continue;
				}
#if UnityFeatures
				if (listener.IsObjectDestroyed)
				{
					// Dropped from the list in a single pass after invoking. Dropping here would shift the rest of
					// the list for each destroyed listener, which costs quadratic time when many objects are
					// destroyed.
					MarkAsRemoved(listener);
					InvokeIndex++;
					continue;
				}
#endif
				if (listener.ShouldRemoveAfterEmit)
				{
					// Remove the callback just before calling it. So that the caller can act like it's removed.
					//
					// Removing before the call also ensures that the callback will be removed even though
					// an exception is thrown inside the callback.
					MarkAsRemoved(listener);
				}

#if UnityFeatures
				var callback = listener.GetCallbackOfNotDestroyedListener(); // IsObjectDestroyed is checked above.
#else
				var callback = listener.GetCallbackAndCheckIfAlive();
#endif
				if (callback != null) // Check if the callback is specified by user. See 11853135.
				{
					try
					{
						InvokingCallback = listener.Callback;
#if EnableProfiling
						using (listener.ProfilerMarker.Auto())
#endif
						{
							callback(param1, param2);
						}
						InvokingCallback = null;
					}
					// Let these exceptions pass through. It means there is an unexpected developer error,
					// rather than an expected runtime error. The developer must be alerted about this.
					// ExtenityEvent does not assume such exceptions are 'Safe' to ignore in InvokeSafe.
					// See 11861024.
					//
					// The listeners that were removed until here stay in the list, marked as removed. The next
					// invocation skips and drops them.
					catch (NotImplementedException)
					{
						IsInvoking = false;
						InvokeIndex = -1;
						InvokingCallback = null;
						throw;
					}
					catch (NotSupportedException)
					{
						IsInvoking = false;
						InvokeIndex = -1;
						InvokingCallback = null;
						throw;
					}
					catch (Exception exception)
					{
						Log.ErrorWithContext(listener.LogObject, exception);
					}
				}

				InvokeIndex++;
			}

			IsInvoking = false;
			InvokeIndex = -1;
			InvokingCallback = null;
			if (RemovedListenerCount > 0)
			{
				Compact();
			}
		}

		#endregion

		#region Log

		private static readonly Logger Log = new("ExtenityEvent");

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
				if (listener.IsRemoved)
					continue;

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

		private string _Detailed_MethodAndObject(Delegate callback)
		{
			return $"method '{callback.FullNameOfTargetAndMethod()}'";
		}

		private string _Detailed_OrderForMethodAndObject(int order, Delegate callback)
		{
			return $"order '{order}' for {_Detailed_MethodAndObject(callback)}";
		}

		private string _Detailed_OrderAndLifeSpan(Listener listener)
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

		private string _Detailed_OrderAndLifeSpanForMethodAndObject(Listener listener)
		{
			return $"{_Detailed_OrderAndLifeSpan(listener)} for {_Detailed_MethodAndObject(listener.Callback)}";
		}

		#endregion
	}

}
