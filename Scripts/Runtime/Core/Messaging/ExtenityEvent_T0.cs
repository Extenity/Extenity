using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Extenity.Messaging
{

	public class ExtenityEvent
	{
		public delegate void MethodDefinition();

		public struct Entry
		{
			public MethodDefinition Callback;
			public int Order;

			public Entry(MethodDefinition callback, int order)
			{
				Callback = callback;
				Order = order;
			}
		}

		private readonly List<Entry> Callbacks = new List<Entry>(10);

		/// <summary>
		/// CAUTION! Returned list should not be modified.
		/// </summary>
		public List<Entry> GetListeners()
		{
			return Callbacks;
		}

		public bool IsListenerRegistered(MethodDefinition callback)
		{
			for (var i = 0; i < Callbacks.Count; i++)
			{
				if (Callbacks[i].Callback == callback)
					return true;
			}
			return false;
		}

		/// <param name="order">Lesser ordered callback gets called earlier. Callbacks that have the same order gets called in the order of AddListener calls. Negative values are allowed.</param>
		public void AddListener(MethodDefinition callback, int order = 0)
		{
			if (callback == null || IsListenerRegistered(callback))
				return; // Silently ignore
			if (!(callback.Target as Object))
			{
				Debug.LogError("Callbacks on non-Unity objects are not supported.");
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
				Callbacks.Add(new Entry(callback, order));
				return;
			}

			for (int i = 0; i < Callbacks.Count; i++)
			{
				if (order < Callbacks[i].Order)
				{
					Callbacks.Insert(i, new Entry(callback, order));
					return;
				}
			}
		}

		public bool RemoveListener(MethodDefinition callback)
		{
			if (callback == null)
				return false; // Silently ignore

			for (var i = 0; i < Callbacks.Count; i++)
			{
				if (Callbacks[i].Callback == callback)
				{
					Callbacks.RemoveAt(i);
					return true;
				}
			}
			return false;
		}

		public void Invoke()
		{
			for (int i = 0; i < Callbacks.Count; i++)
			{
				var callback = Callbacks[i].Callback;
				if (callback != null)
				{
					if (callback.Target as Object) // Check if the object is not destroyed
					{
						callback.Invoke();
					}
					else
					{
						Callbacks.RemoveAt(i);
						i--;
					}
				}
				else
				{
					Callbacks.RemoveAt(i);
					i--;
				}
			}
		}

		public void InvokeSafe()
		{
			for (int i = 0; i < Callbacks.Count; i++)
			{
				var callback = Callbacks[i].Callback;
				if (callback != null)
				{
					if (callback.Target as Object) // Check if the object is not destroyed
					{
						try
						{
							callback.Invoke();
						}
						catch (Exception exception)
						{
							Debug.LogException(exception, callback.Target as Object);
						}
					}
					else
					{
						Callbacks.RemoveAt(i);
						i--;
					}
				}
				else
				{
					Callbacks.RemoveAt(i);
					i--;
				}
			}
		}
	}

}
