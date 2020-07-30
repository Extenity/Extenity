using System;
using System.Collections.Generic;
using Extenity.DataToolbox;
using Extenity.GameObjectToolbox;
using Object = UnityEngine.Object;

namespace Extenity.MessagingToolbox
{

	public class ExtenityEvent<T1, T2>
	{
		#region Callback entries

		public struct Entry
		{
			public readonly Action<T1, T2> Callback;
			public readonly bool IsCallbackTargetsUnityObject;
			public readonly int Order;
			public readonly ListenerLifeSpan LifeSpan;
			public readonly Object LifeSpanTarget;
			public readonly bool IsLifeSpanTargetAssigned;

			public Entry(Action<T1, T2> callback, int order, ListenerLifeSpan lifeSpan, Object lifeSpanTarget)
			{
				Callback = callback;
				IsCallbackTargetsUnityObject = callback?.Target as Object;
				Order = order;
				LifeSpan = lifeSpan;
				LifeSpanTarget = lifeSpanTarget;
				IsLifeSpanTargetAssigned = lifeSpanTarget != null;
				throw new NotImplementedException(); // TODO: Copy ExtenityEvent_T0 into this class
			}

			public bool IsInvalid => Callback == null;

			public bool IsCallbackTargetedUnityObjectDestroyed => IsCallbackTargetsUnityObject && !(Callback.Target as Object);

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
					       IsCallbackTargetedUnityObjectDestroyed ||
					       IsLifeSpanTargetDestroyed;
				}
			}

			public Object LogObject
			{
				get
				{
					// First, try to get callback delegate object. If not available, get the LifeSpanTarget object.
					if (Callback != null)
					{
						var obj = Callback.Target as Object;
						if (obj)
							return obj;
					}
					return LifeSpanTarget;
				}
			}
		}

		/// <summary>
		/// CAUTION! Do not modify! Use AddListener and RemoveListener instead.
		/// </summary>
		public readonly List<Entry> Callbacks = new List<Entry>(10);

		public bool IsAnyListenerRegistered => Callbacks.Count > 0;

		public bool IsListenerRegistered(Action<T1, T2> callback)
		{
			for (var i = 0; i < Callbacks.Count; i++)
			{
				if (Callbacks[i].Callback == callback)
					return true;
			}
			return false;
		}

		public Entry GetListenerInfo(Action<T1, T2> callback)
		{
			for (var i = 0; i < Callbacks.Count; i++)
			{
				if (Callbacks[i].Callback == callback)
					return Callbacks[i];
			}
			return default;
		}

		public void Clear()
		{
			for (int i = Callbacks.Count - 1; i >= 0; i--)
			{
				if (Callbacks[i].IsObjectDestroyed) // Check if the object is destroyed
				{
					Callbacks.RemoveAt(i);
					i--;
				}
			}
		}

		public void ClearIfRequired()
		{
			if (CleanupRequired)
				Clear();
		}

		#endregion

		#region Add / Remove Listener

		/// <param name="order">Lesser ordered callback gets called earlier. Callbacks that have the same order gets called in the order of AddListener calls. Negative values are allowed.</param>
		public void AddListener(Action<T1, T2> callback, int order = 0, ListenerLifeSpan lifeSpan = ListenerLifeSpan.Permanent, Object lifeSpanTarget = null)
		{
			if (order == int.MinValue || order == int.MaxValue) // These values are reserved for internal use.
				throw new ArgumentOutOfRangeException(nameof(order), order, "");
			if (callback == null)
			{
				if (ExtenityEventTools.VerboseLogging)
					Log.Info($"Tried to add a null callback with {_Detailed_OrderAndLifeSpan(order, lifeSpan, lifeSpanTarget)}.");
				return; // Silently ignore
			}

			// See if the callback was already registered.
			for (var i = 0; i < Callbacks.Count; i++)
			{
				if (Callbacks[i].Callback == callback)
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
							Log.Info($"Tried to add an already registered callback with {_Detailed_OrderAndLifeSpanForMethodAndObject(order, lifeSpan, lifeSpanTarget, callback)}.");
						return; // Silently ignore
					}

					break; // No need to iterate others. Impossible to add a delegate more than once.
				}
			}

			// Cover all the shortcuts that we can add the listener at the end of the list and move on.
			//
			// Line 1: If there is no other listener registered, don't bother the ordering. Just add it and move on.
			//
			// Line 2: If trying to add a callback with 0 order and the last item is ordered as 0, fear not!
			// Just add it to the end and move on. While doing that, also cover the possibility that the order
			// is greater(or equal) than the last item's order.
			if (Callbacks.Count == 0 || // Line 1
			    order >= Callbacks[Callbacks.Count - 1].Order) // Line 2
			{
				if (ExtenityEventTools.VerboseLogging)
					Log.Info($"Adding listener with {_Detailed_OrderAndLifeSpanForMethodAndObject(order, lifeSpan, lifeSpanTarget, callback)} as the last entry, resulting '{Callbacks.Count + 1}' listener(s).");
				Callbacks.Add(new Entry(callback, order, lifeSpan, lifeSpanTarget));
				return;
			}

			for (int i = 0; i < Callbacks.Count; i++)
			{
				if (order < Callbacks[i].Order)
				{
					if (ExtenityEventTools.VerboseLogging)
						Log.Info($"Adding listener with {_Detailed_OrderAndLifeSpanForMethodAndObject(order, lifeSpan, lifeSpanTarget, callback)} at index '{i}', resulting '{Callbacks.Count + 1}' listener(s).");
					Callbacks.Insert(i, new Entry(callback, order, lifeSpan, lifeSpanTarget));
					return;
				}
			}
		}

		public bool RemoveListener(Action<T1, T2> callback)
		{
			if (callback == null)
				return false; // Silently ignore

			for (var i = 0; i < Callbacks.Count; i++)
			{
				if (Callbacks[i].Callback == callback)
				{
					if (ExtenityEventTools.VerboseLogging)
						Log.Info($"Removing listener with {_Detailed_OrderForMethodAndObject(Callbacks[i].Order, callback)} at index '{i}', resulting '{Callbacks.Count - 1}' listener(s).");
					Callbacks.RemoveAt(i);
					return true;
				}
			}
			if (ExtenityEventTools.VerboseLogging)
				Log.Info($"Failed to remove listener for {_Detailed_MethodAndObject(callback)}.");
			return false;
		}

		private void _RemoveListener(int index)
		{
			Callbacks.RemoveAt(index);
		}

		public void RemoveAllListeners()
		{
			if (ExtenityEventTools.VerboseLogging)
				Log.Info("Removing all listeners.");

			Callbacks.Clear();
		}

		#endregion

		#region Invoke

		private bool IsInvoking;
		public bool CleanupRequired;

		[ThreadStatic]
		private static List<Entry> CallbacksCopy;

		public void InvokeOneShotUnsafe(T1 param1, T2 param2)
		{
			InvokeUnsafe(param1, param2);
			RemoveAllListeners();
		}

		public void InvokeUnsafe(T1 param1, T2 param2)
		{
			if (IsInvoking)
			{
				Log.CriticalError("Invoked an event while an invocation is ongoing.");
				return;
			}
			IsInvoking = true;

			try
			{
				// TODO OPTIMIZATION: Do not copy the list at first. Only copy it lazily when the user callback needs to change the callbacks list and then continue to iterate over that copy.
				// Copy the list to allow adding and removing callbacks while processing the invoke.
				if (CallbacksCopy == null)
					CallbacksCopy = new List<Entry>(Callbacks.Count);
				CallbacksCopy.Clear();
				CallbacksCopy.AddRange(Callbacks);

				// After copying the callbacks, remove the ones that are set to be removed when emitted.
				for (int i = Callbacks.Count - 1; i >= 0; i--)
				{
					if (Callbacks[i].ShouldRemoveAfterEmit)
					{
						Callbacks.RemoveAt(i);
					}
				}

				for (int i = 0; i < CallbacksCopy.Count; i++)
				{
					if (!CallbacksCopy[i].IsObjectDestroyed)
					{
						CallbacksCopy[i].Callback(param1, param2);
					}
					else
					{
						CleanupRequired = true;
					}
				}
			}
			finally
			{
				IsInvoking = false;
				CallbacksCopy.Clear();
			}
		}

		public void InvokeOneShotSafe(T1 param1, T2 param2)
		{
			InvokeSafe(param1, param2);
			RemoveAllListeners();
		}

		public void InvokeSafe(T1 param1, T2 param2)
		{
			if (IsInvoking)
			{
				Log.CriticalError("Invoked an event while an invocation is ongoing.");
				return;
			}
			IsInvoking = true;

			// TODO OPTIMIZATION: Do not copy the list at first. Only copy it lazily when the user callback needs to change the callbacks list and then continue to iterate over that copy.
			// Copy the list to allow adding and removing callbacks while processing the invoke.
			if (CallbacksCopy == null)
				CallbacksCopy = new List<Entry>(Callbacks.Count);
			CallbacksCopy.Clear();
			CallbacksCopy.AddRange(Callbacks);

			// After copying the callbacks, remove the ones that are set to be removed when emitted.
			for (int i = Callbacks.Count - 1; i >= 0; i--)
			{
				if (Callbacks[i].ShouldRemoveAfterEmit)
				{
					Callbacks.RemoveAt(i);
				}
			}

			for (int i = 0; i < CallbacksCopy.Count; i++)
			{
				if (!CallbacksCopy[i].IsObjectDestroyed)
				{
					try
					{
						CallbacksCopy[i].Callback(param1, param2);
					}
					catch (Exception exception)
					{
						Log.Exception(exception, CallbacksCopy[i].LogObject);
					}
				}
				else
				{
					CleanupRequired = true;
				}
			}

			IsInvoking = false;
			CallbacksCopy.Clear();
		}

		#endregion

		#region Log

		private string _Detailed_MethodAndObject(Delegate callback)
		{
			return $"method '{callback.FullNameOfTargetAndMethod()}'";
		}

		private string _Detailed_OrderForMethodAndObject(int order, Delegate callback)
		{
			return $"order '{order}' for {_Detailed_MethodAndObject(callback)}";
		}

		private string _Detailed_OrderAndLifeSpan(int order, ListenerLifeSpan lifeSpan, Object lifeSpanTarget)
		{
			return $"order '{order}' and life span '{lifeSpan}{(lifeSpanTarget ? $" with target '{lifeSpanTarget.FullObjectName()}'" : "")}'";
		}

		private string _Detailed_OrderAndLifeSpanForMethodAndObject(int order, ListenerLifeSpan lifeSpan, Object lifeSpanTarget, Delegate callback)
		{
			return $"{_Detailed_OrderAndLifeSpan(order, lifeSpan, lifeSpanTarget)} for {_Detailed_MethodAndObject(callback)}";
		}

		#endregion
	}

}
