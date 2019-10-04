using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Extenity.MessagingToolbox
{

	public class ExtenityEvent<T1, T2>
	{
		#region Callback entries

		public struct Entry
		{
			public readonly Action<T1, T2> Callback;
			public readonly int Order;
			public readonly ListenerLifeSpan LifeSpan;
			public readonly Object LifeSpanTarget;
			public readonly bool IsLifeSpanTargetAssigned;

			public Entry(Action<T1, T2> callback, int order, ListenerLifeSpan lifeSpan, Object lifeSpanTarget)
			{
				Callback = callback;
				Order = order;
				LifeSpan = lifeSpan;
				LifeSpanTarget = lifeSpanTarget;
				IsLifeSpanTargetAssigned = lifeSpanTarget != null;
			}

			public bool ShouldRemoveAfterEmit
			{
				get { return LifeSpan == ListenerLifeSpan.RemovedAtFirstEmit; }
			}

			public bool IsObjectDestroyed
			{
				get
				{
					return Callback == null ||
					       !(Callback.Target as Object) ||
					       (IsLifeSpanTargetAssigned && !LifeSpanTarget);
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

		/// <summary>
		/// Finds the specified callback in registered callbacks list and tells it's order.
		/// The order is the value that is passed into the system at the time the callback
		/// is registered via AddListener, or when it's changed via ChangeListenerOrder.
		/// </summary>
		/// <returns>The order of specified callback. If the callback is not registered, returns int.MaxValue.</returns>
		public int GetListenerOrder(Action<T1, T2> callback)
		{
			for (var i = 0; i < Callbacks.Count; i++)
			{
				if (Callbacks[i].Callback == callback)
					return Callbacks[i].Order;
			}
			return int.MaxValue;
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
					Log.Info($"Tried to add a null callback with order '{order}' and life span '{lifeSpan}'.");
				return; // Silently ignore
			}
			var alreadyExistingListenerOrder = GetListenerOrder(callback);
			if (alreadyExistingListenerOrder != int.MaxValue)
			{
				if (order != alreadyExistingListenerOrder)
				{
					// The callback is already registered but the order is different.
					// So just change the order.
					// TODO: This is a quick fix. Better just change the order than removing and adding the listener again. See commented out lines below.
					RemoveListener(callback);
					//ChangeListenerOrder(callback, order);
					//return; // Do not proceed to adding a new listener.
				}
				else
				{
					if (ExtenityEventTools.VerboseLogging)
						Log.Info($"Tried to add an already registered callback with order '{order}' and life span '{lifeSpan}' for method '{callback.Method}' of object '{callback.FullNameOfTarget()}'.");
					return; // Silently ignore
				}
			}
			if (!(callback.Target as Object))
			{
				Log.CriticalError("Callbacks on non-Unity or non-existing objects are not supported.");
				return;
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
				Log.Info($"Failed to remove listener for method '{callback.Method}' of object '{callback.FullNameOfTarget()}'.");
			return false;
		}

		public void RemoveAllListeners()
		{
			if (ExtenityEventTools.VerboseLogging)
				Log.Info($"Removing all listeners.");

			Callbacks.Clear();
		}

		#endregion

		#region Invoke

		private bool IsInvoking;
		public bool CleanupRequired;

		[ThreadStatic]
		private static List<Entry> CallbacksCopy;

		public void InvokeOneShot(T1 param1, T2 param2)
		{
			Invoke(param1, param2);
			RemoveAllListeners();
		}

		public void Invoke(T1 param1, T2 param2)
		{
			if (IsInvoking)
			{
				Log.CriticalError("Invoked an event while an invocation is ongoing.");
				return;
			}
			IsInvoking = true;

			try
			{
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
					var callback = CallbacksCopy[i].Callback;
					if (callback != null && (callback.Target as Object)) // Check if the object is not destroyed
					{
						callback(param1, param2);
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
				var callback = CallbacksCopy[i].Callback;
				if (callback != null && (callback.Target as Object)) // Check if the object is not destroyed
				{
					try
					{
						callback(param1, param2);
					}
					catch (Exception exception)
					{
						Log.Exception(exception, callback.Target as Object);
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

		private string _Detailed_OrderForMethodAndObject(int order, Delegate callback)
		{
			return $"order '{order}' for method '{callback.Method}' of object '{callback.FullNameOfTarget()}'";
		}

		private string _Detailed_OrderAndLifeSpanForMethodAndObject(int order, ListenerLifeSpan lifeSpan, Object lifeSpanTarget, Delegate callback)
		{
			return $"order '{order}' and life span '{lifeSpan}{(lifeSpanTarget ? $"with target '{lifeSpanTarget.ToString()}'" : "")}' for method '{callback.Method}' of object '{callback.FullNameOfTarget()}'";
		}

		#endregion
	}

}
