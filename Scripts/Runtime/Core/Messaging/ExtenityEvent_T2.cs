using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Extenity.MessagingToolbox
{

	public class ExtenityEvent<T1, T2>
	{
		#region Callback entries

		public delegate void MethodDefinition(T1 param1, T2 param2);

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

		/// <summary>
		/// CAUTION! Do not modify! Use AddListener and RemoveListener instead.
		/// </summary>
		public readonly List<Entry> Callbacks = new List<Entry>(10);

		public bool IsListenerRegistered(MethodDefinition callback)
		{
			for (var i = 0; i < Callbacks.Count; i++)
			{
				if (Callbacks[i].Callback == callback)
					return true;
			}
			return false;
		}

		public void Clear()
		{
			for (int i = 0; i < Callbacks.Count; i++)
			{
				var callback = Callbacks[i].Callback;
				if (callback == null || !(callback.Target as Object)) // Check if the object is destroyed
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
		public void AddListener(MethodDefinition callback, int order = 0)
		{
			if (callback == null)
			{
				if (ExtenityEventTools.VerboseLogging)
					ExtenityEventTools.LogVerbose($"Tried to add a null callback with order '{order}'.");
				return; // Silently ignore
			}
			if (IsListenerRegistered(callback))
			{
				if (ExtenityEventTools.VerboseLogging)
					ExtenityEventTools.LogVerbose($"Tried to add an already registered callback with order '{order}' for method '{callback.Method}' of object '{callback.FullNameOfTarget()}'.");
				return; // Silently ignore
			}
			if (!(callback.Target as Object))
			{
				ExtenityEventTools.LogError("Callbacks on non-Unity or non-existing objects are not supported.");
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
					ExtenityEventTools.LogVerbose($"Adding listener with order '{order}' for method '{callback.Method}' of object '{callback.FullNameOfTarget()}' as the last entry, resulting '{Callbacks.Count + 1}' listener(s).");
				Callbacks.Add(new Entry(callback, order));
				return;
			}

			for (int i = 0; i < Callbacks.Count; i++)
			{
				if (order < Callbacks[i].Order)
				{
					if (ExtenityEventTools.VerboseLogging)
						ExtenityEventTools.LogVerbose($"Adding listener with order '{order}' for method '{callback.Method}' of object '{callback.FullNameOfTarget()}' at index '{i}', resulting '{Callbacks.Count + 1}' listener(s).");
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
					if (ExtenityEventTools.VerboseLogging)
						ExtenityEventTools.LogVerbose($"Removing listener with order '{Callbacks[i].Order}' for method '{callback.Method}' of object '{callback.FullNameOfTarget()}' at index '{i}', resulting '{Callbacks.Count - 1}' listener(s).");
					Callbacks.RemoveAt(i);
					return true;
				}
			}
			if (ExtenityEventTools.VerboseLogging)
				ExtenityEventTools.LogVerbose($"Failed to remove listener for method '{callback.Method}' of object '{callback.FullNameOfTarget()}'.");
			return false;
		}

		public void RemoveAllListeners()
		{
			if (ExtenityEventTools.VerboseLogging)
				ExtenityEventTools.LogVerbose($"Removing all listeners.");

			Callbacks.Clear();
		}

		#endregion

		#region Invoke

		private bool IsInvoking;
		public bool CleanupRequired;

		[ThreadStatic]
		private static List<Entry> CallbacksCopy;

		public void Invoke(T1 param1, T2 param2)
		{
			if (IsInvoking)
			{
				ExtenityEventTools.LogError("Invoked an event while an invocation is ongoing.");
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
			}
		}

		public void InvokeSafe(T1 param1, T2 param2)
		{
			if (IsInvoking)
			{
				ExtenityEventTools.LogError("Invoked an event while an invocation is ongoing.");
				return;
			}
			IsInvoking = true;

			// Copy the list to allow adding and removing callbacks while processing the invoke.
			if (CallbacksCopy == null)
				CallbacksCopy = new List<Entry>(Callbacks.Count);
			CallbacksCopy.Clear();
			CallbacksCopy.AddRange(Callbacks);

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
						Debug.LogException(exception, callback.Target as Object);
					}
				}
				else
				{
					CleanupRequired = true;
				}
			}

			IsInvoking = false;
		}

		#endregion
	}

}
