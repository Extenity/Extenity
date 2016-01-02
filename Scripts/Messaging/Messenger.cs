using System;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Extenity.Messaging
{

	public class Messenger : MonoBehaviour
	{
		#region Update

		protected void LateUpdate()
		{
			if (CleanupRequired)
			{
				CleanupRequired = false;
				CleanUpListenerDelegateLists();
			}
		}

		#endregion

		#region Global

		private static Messenger _Global;
		public static Messenger Global
		{
			get
			{
				if (_Global == null)
				{
					var go = new GameObject("_GlobalMessenger", typeof(Messenger));
					go.hideFlags = HideFlags.HideAndDontSave;
					_Global = go.GetComponent<Messenger>();
				}
				return _Global;
			}
		}

		#endregion

		#region Actions

		public delegate void MessengerAction();
		public delegate void MessengerAction<T1>(T1 arg1);
		public delegate void MessengerAction<T1, T2>(T1 arg1, T2 arg2);
		public delegate void MessengerAction<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);
		public delegate void MessengerAction<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
		public delegate void MessengerAction<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
		public delegate void MessengerAction<T1, T2, T3, T4, T5, T6>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
		public delegate void MessengerAction<T1, T2, T3, T4, T5, T6, T7>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
		public delegate void MessengerAction<T1, T2, T3, T4, T5, T6, T7, T8>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);
		public delegate void MessengerAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9);

		#endregion

		#region Message Listeners

		private struct ListenerInfo
		{
			public int MessageId;
			public ParameterInfo[] ParameterInfos;
			public List<Delegate> Delegates;

			public bool IsValid
			{
				get { return MessageId > 0; }
			}
		}

		private Dictionary<int, ListenerInfo> ListenerInfoDictionary = new Dictionary<int, ListenerInfo>();

		private ListenerInfo GetListenerInfo(int messageId)
		{
			ListenerInfo listenerInfo;
			ListenerInfoDictionary.TryGetValue(messageId, out listenerInfo);
			return listenerInfo;
		}

		#endregion

		#region Message Listeners Cleanup

		public bool CleanupRequired;

		private void CleanUpListenerDelegateLists()
		{
			if (ListenerInfoDictionary == null || ListenerInfoDictionary.Count == 0)
				return;

			foreach (var listenerInfo in ListenerInfoDictionary.Values)
			{
				if (!listenerInfo.IsValid)
					continue;

				// This is not the most efficient way to clear the list.
				var done = false;
				while (!done)
				{
					done = true;
					var delegates = listenerInfo.Delegates;
					if (delegates == null || delegates.Count == 0)
						break;
					for (int i = 0; i < delegates.Count; i++)
					{
						var item = delegates[i];
						if (item == null || (item.Target as Object) == null)
						{
							done = false;
							delegates.RemoveAt(i);
							break;
						}
					}
				}
			}
		}

		#endregion

		#region Add Listener

		public void AddListener(int messageId, MessengerAction listener) { AddListener(messageId, (Delegate)listener); }
		public void AddListener(int messageId, MessengerAction<bool> listener) { AddListener(messageId, (Delegate)listener); }
		public void AddListener(int messageId, MessengerAction<byte> listener) { AddListener(messageId, (Delegate)listener); }
		public void AddListener(int messageId, MessengerAction<Int16> listener) { AddListener(messageId, (Delegate)listener); }
		public void AddListener(int messageId, MessengerAction<Int32> listener) { AddListener(messageId, (Delegate)listener); }
		public void AddListener(int messageId, MessengerAction<Int64> listener) { AddListener(messageId, (Delegate)listener); }
		public void AddListener(int messageId, MessengerAction<UInt16> listener) { AddListener(messageId, (Delegate)listener); }
		public void AddListener(int messageId, MessengerAction<UInt32> listener) { AddListener(messageId, (Delegate)listener); }
		public void AddListener(int messageId, MessengerAction<UInt64> listener) { AddListener(messageId, (Delegate)listener); }
		public void AddListener(int messageId, MessengerAction<float> listener) { AddListener(messageId, (Delegate)listener); }
		public void AddListener(int messageId, MessengerAction<double> listener) { AddListener(messageId, (Delegate)listener); }
		public void AddListener(int messageId, MessengerAction<char> listener) { AddListener(messageId, (Delegate)listener); }
		public void AddListener(int messageId, MessengerAction<string> listener) { AddListener(messageId, (Delegate)listener); }
		public void AddListener<TParam1>(int messageId, MessengerAction<TParam1> listener) { AddListener(messageId, (Delegate)listener); }
		public void AddListener<TParam1, TParam2>(int messageId, MessengerAction<TParam1, TParam2> listener) { AddListener(messageId, (Delegate)listener); }
		public void AddListener<TParam1, TParam2, TParam3>(int messageId, MessengerAction<TParam1, TParam2, TParam3> listener) { AddListener(messageId, (Delegate)listener); }
		public void AddListener<TParam1, TParam2, TParam3, TParam4>(int messageId, MessengerAction<TParam1, TParam2, TParam3, TParam4> listener) { AddListener(messageId, (Delegate)listener); }
		public void AddListener<TParam1, TParam2, TParam3, TParam4, TParam5>(int messageId, MessengerAction<TParam1, TParam2, TParam3, TParam4, TParam5> listener) { AddListener(messageId, (Delegate)listener); }
		public void AddListener<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(int messageId, MessengerAction<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> listener) { AddListener(messageId, (Delegate)listener); }
		public void AddListener<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(int messageId, MessengerAction<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> listener) { AddListener(messageId, (Delegate)listener); }
		public void AddListener<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(int messageId, MessengerAction<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> listener) { AddListener(messageId, (Delegate)listener); }
		public void AddListener<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>(int messageId, MessengerAction<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9> listener) { AddListener(messageId, (Delegate)listener); }

		public void AddListener(int messageId, Delegate listener)
		{
			// At this point, we may want to check for any return and input parameter inconsistencies in the future.
			//listener.Method.ReturnParameter
			//listener.Method.GetParameters()

			if ((listener.Target as Object) == null)
			{
				LogAddNonUnityObject();
				return;
			}

			ListenerInfo listenerInfo;
			// Is this the first time we add a listener for this messageId?
			if (!ListenerInfoDictionary.TryGetValue(messageId, out listenerInfo))
			{
				// Do the initialization for this messageId
				{
					listenerInfo.MessageId = messageId;

					// Create a brand new delegate list
					listenerInfo.Delegates = new List<Delegate>(50);
					// Add listener to list
					listenerInfo.Delegates.Add(listener);

					// Optimization ID-150827532:
					// Get method parameters of this initially added listener. This parameter info 
					// will be used to check if parameters of following registered listeners matches
					// the parameters of this first added method. This way we can get rid of one
					// 'Method.GetParameters()' call in every listener registration.
					listenerInfo.ParameterInfos = listener.Method.GetParameters();
				}

				ListenerInfoDictionary.Add(messageId, listenerInfo);

				// Instantly return without getting into further consistency checks.
				return;
			}

			var delegates = listenerInfo.Delegates;

			// Create new list if necessary
			if (delegates == null)
			{
				delegates = new List<Delegate>(50);
				listenerInfo.Delegates = delegates;
			}

			// Prevent duplicate entries
			if (!delegates.Contains(listener))
			{
				// Make sure all listener methods are identical (that is, recently added method is identical with the first added method in listeners list)
				if (delegates.Count > 0)
				{
					// Optimization ID-150827532:
					var newListenerParameters = listener.Method.GetParameters(); // This call is bad for performance but no other workaround exists for comparing two methods' parameters.
					if (!listenerInfo.ParameterInfos.CompareMethodParameters(newListenerParameters, false))
					{
						LogBadListenerParameters();
					}
				}

				// No need for this anymore. We now have CleanupRequired mechanism.
				//{
				//	// First, check to see if we can overwrite an entry that contains delegate to destroyed object.
				//	// Only add a new entry if there is nothing to overwrite.
				//	for (int i = 0; i < delegates.Count; i++)
				//	{
				//		if ((delegates[i].Target as Object) == null) // Check if the object is destroyed
				//		{
				//			delegates[i] = listener;
				//			return;
				//		}
				//	}
				//}

				delegates.Add(listener);
			}
		}

		#endregion

		#region Remove Listener

		public void RemoveListener(int messageId, MessengerAction listener) { RemoveListener(messageId, (Delegate)listener); }
		public void RemoveListener(int messageId, MessengerAction<bool> listener) { RemoveListener(messageId, (Delegate)listener); }
		public void RemoveListener(int messageId, MessengerAction<byte> listener) { RemoveListener(messageId, (Delegate)listener); }
		public void RemoveListener(int messageId, MessengerAction<Int16> listener) { RemoveListener(messageId, (Delegate)listener); }
		public void RemoveListener(int messageId, MessengerAction<Int32> listener) { RemoveListener(messageId, (Delegate)listener); }
		public void RemoveListener(int messageId, MessengerAction<Int64> listener) { RemoveListener(messageId, (Delegate)listener); }
		public void RemoveListener(int messageId, MessengerAction<UInt16> listener) { RemoveListener(messageId, (Delegate)listener); }
		public void RemoveListener(int messageId, MessengerAction<UInt32> listener) { RemoveListener(messageId, (Delegate)listener); }
		public void RemoveListener(int messageId, MessengerAction<UInt64> listener) { RemoveListener(messageId, (Delegate)listener); }
		public void RemoveListener(int messageId, MessengerAction<float> listener) { RemoveListener(messageId, (Delegate)listener); }
		public void RemoveListener(int messageId, MessengerAction<double> listener) { RemoveListener(messageId, (Delegate)listener); }
		public void RemoveListener(int messageId, MessengerAction<char> listener) { RemoveListener(messageId, (Delegate)listener); }
		public void RemoveListener(int messageId, MessengerAction<string> listener) { RemoveListener(messageId, (Delegate)listener); }
		public void RemoveListener<TParam1>(int messageId, MessengerAction<TParam1> listener) { RemoveListener(messageId, (Delegate)listener); }
		public void RemoveListener<TParam1, TParam2>(int messageId, MessengerAction<TParam1, TParam2> listener) { RemoveListener(messageId, (Delegate)listener); }
		public void RemoveListener<TParam1, TParam2, TParam3>(int messageId, MessengerAction<TParam1, TParam2, TParam3> listener) { RemoveListener(messageId, (Delegate)listener); }
		public void RemoveListener<TParam1, TParam2, TParam3, TParam4>(int messageId, MessengerAction<TParam1, TParam2, TParam3, TParam4> listener) { RemoveListener(messageId, (Delegate)listener); }
		public void RemoveListener<TParam1, TParam2, TParam3, TParam4, TParam5>(int messageId, MessengerAction<TParam1, TParam2, TParam3, TParam4, TParam5> listener) { RemoveListener(messageId, (Delegate)listener); }
		public void RemoveListener<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(int messageId, MessengerAction<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> listener) { RemoveListener(messageId, (Delegate)listener); }
		public void RemoveListener<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(int messageId, MessengerAction<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> listener) { RemoveListener(messageId, (Delegate)listener); }
		public void RemoveListener<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(int messageId, MessengerAction<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> listener) { RemoveListener(messageId, (Delegate)listener); }
		public void RemoveListener<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>(int messageId, MessengerAction<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9> listener) { RemoveListener(messageId, (Delegate)listener); }

		public bool RemoveListener(int messageId, Delegate listener)
		{
			ListenerInfo listenerInfo;
			if (!ListenerInfoDictionary.TryGetValue(messageId, out listenerInfo))
				return false;
			if (listenerInfo.Delegates == null)
				return false;
			return listenerInfo.Delegates.Remove(listener);
		}

		#endregion

		#region Emit Message

		public void Emit(int messageId)
		{
			var listenerInfo = GetListenerInfo(messageId);
			var delegates = listenerInfo.Delegates;
			if (!listenerInfo.IsValid || delegates == null || delegates.Count == 0)
				return;
			for (int i = 0; i < delegates.Count; i++)
			{
				var castListener = delegates[i] as MessengerAction;
				if (castListener != null)
				{
					if ((castListener.Target as Object) != null) // Check if the object is not destroyed
						castListener.Invoke();
					else
						CleanupRequired = true;
				}
				else
					LogBadEmitParameters();
			}
		}

		public void Emit<T1>(int messageId, T1 param1)
		{
			var listenerInfo = GetListenerInfo(messageId);
			var delegates = listenerInfo.Delegates;
			if (!listenerInfo.IsValid || delegates == null || delegates.Count == 0)
				return;
			for (int i = 0; i < delegates.Count; i++)
			{
				var castListener = delegates[i] as MessengerAction<T1>;
				if (castListener != null)
				{
					if ((castListener.Target as Object) != null) // Check if the object is not destroyed
						castListener.Invoke(param1);
					else
						CleanupRequired = true;
				}
				else
					LogBadEmitParameters();
			}
		}

		public void Emit<T1, T2>(int messageId, T1 param1, T2 param2)
		{
			var listenerInfo = GetListenerInfo(messageId);
			var delegates = listenerInfo.Delegates;
			if (!listenerInfo.IsValid || delegates == null || delegates.Count == 0)
				return;
			for (int i = 0; i < delegates.Count; i++)
			{
				var castListener = delegates[i] as MessengerAction<T1, T2>;
				if (castListener != null)
				{
					if ((castListener.Target as Object) != null) // Check if the object is not destroyed
						castListener.Invoke(param1, param2);
					else
						CleanupRequired = true;
				}
				else
					LogBadEmitParameters();
			}
		}

		public void Emit<T1, T2, T3>(int messageId, T1 param1, T2 param2, T3 param3)
		{
			var listenerInfo = GetListenerInfo(messageId);
			var delegates = listenerInfo.Delegates;
			if (!listenerInfo.IsValid || delegates == null || delegates.Count == 0)
				return;
			for (int i = 0; i < delegates.Count; i++)
			{
				var castListener = delegates[i] as MessengerAction<T1, T2, T3>;
				if (castListener != null)
				{
					if ((castListener.Target as Object) != null) // Check if the object is not destroyed
						castListener.Invoke(param1, param2, param3);
					else
						CleanupRequired = true;
				}
				else
					LogBadEmitParameters();
			}
		}

		public void Emit<T1, T2, T3, T4>(int messageId, T1 param1, T2 param2, T3 param3, T4 param4)
		{
			var listenerInfo = GetListenerInfo(messageId);
			var delegates = listenerInfo.Delegates;
			if (!listenerInfo.IsValid || delegates == null || delegates.Count == 0)
				return;
			for (int i = 0; i < delegates.Count; i++)
			{
				var castListener = delegates[i] as MessengerAction<T1, T2, T3, T4>;
				if (castListener != null)
				{
					if ((castListener.Target as Object) != null) // Check if the object is not destroyed
						castListener.Invoke(param1, param2, param3, param4);
					else
						CleanupRequired = true;
				}
				else
					LogBadEmitParameters();
			}
		}

		public void Emit<T1, T2, T3, T4, T5>(int messageId, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
		{
			var listenerInfo = GetListenerInfo(messageId);
			var delegates = listenerInfo.Delegates;
			if (!listenerInfo.IsValid || delegates == null || delegates.Count == 0)
				return;
			for (int i = 0; i < delegates.Count; i++)
			{
				var castListener = delegates[i] as MessengerAction<T1, T2, T3, T4, T5>;
				if (castListener != null)
				{
					if ((castListener.Target as Object) != null) // Check if the object is not destroyed
						castListener.Invoke(param1, param2, param3, param4, param5);
					else
						CleanupRequired = true;
				}
				else
					LogBadEmitParameters();
			}
		}

		public void Emit<T1, T2, T3, T4, T5, T6>(int messageId, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6)
		{
			var listenerInfo = GetListenerInfo(messageId);
			var delegates = listenerInfo.Delegates;
			if (!listenerInfo.IsValid || delegates == null || delegates.Count == 0)
				return;
			for (int i = 0; i < delegates.Count; i++)
			{
				var castListener = delegates[i] as MessengerAction<T1, T2, T3, T4, T5, T6>;
				if (castListener != null)
				{
					if ((castListener.Target as Object) != null) // Check if the object is not destroyed
						castListener.Invoke(param1, param2, param3, param4, param5, param6);
					else
						CleanupRequired = true;
				}
				else
					LogBadEmitParameters();
			}
		}

		public void Emit<T1, T2, T3, T4, T5, T6, T7>(int messageId, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7)
		{
			var listenerInfo = GetListenerInfo(messageId);
			var delegates = listenerInfo.Delegates;
			if (!listenerInfo.IsValid || delegates == null || delegates.Count == 0)
				return;
			for (int i = 0; i < delegates.Count; i++)
			{
				var castListener = delegates[i] as MessengerAction<T1, T2, T3, T4, T5, T6, T7>;
				if (castListener != null)
				{
					if ((castListener.Target as Object) != null) // Check if the object is not destroyed
						castListener.Invoke(param1, param2, param3, param4, param5, param6, param7);
					else
						CleanupRequired = true;
				}
				else
					LogBadEmitParameters();
			}
		}

		public void Emit<T1, T2, T3, T4, T5, T6, T7, T8>(int messageId, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8)
		{
			var listenerInfo = GetListenerInfo(messageId);
			var delegates = listenerInfo.Delegates;
			if (!listenerInfo.IsValid || delegates == null || delegates.Count == 0)
				return;
			for (int i = 0; i < delegates.Count; i++)
			{
				var castListener = delegates[i] as MessengerAction<T1, T2, T3, T4, T5, T6, T7, T8>;
				if (castListener != null)
				{
					if ((castListener.Target as Object) != null) // Check if the object is not destroyed
						castListener.Invoke(param1, param2, param3, param4, param5, param6, param7, param8);
					else
						CleanupRequired = true;
				}
				else
					LogBadEmitParameters();
			}
		}

		public void Emit<T1, T2, T3, T4, T5, T6, T7, T8, T9>(int messageId, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, T9 param9)
		{
			var listenerInfo = GetListenerInfo(messageId);
			var delegates = listenerInfo.Delegates;
			if (!listenerInfo.IsValid || delegates == null || delegates.Count == 0)
				return;
			for (int i = 0; i < delegates.Count; i++)
			{
				var castListener = delegates[i] as MessengerAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>;
				if (castListener != null)
				{
					if ((castListener.Target as Object) != null) // Check if the object is not destroyed
						castListener.Invoke(param1, param2, param3, param4, param5, param6, param7, param8, param9);
					else
						CleanupRequired = true;
				}
				else
					LogBadEmitParameters();
			}
		}

		#endregion

		#region Log Errors

		private void LogAddNonUnityObject()
		{
			Debug.LogError("Messaging system only allows adding methods of a Unity object (MonoBehaviour, GameObject, Component, etc.) as listener delegates.");
		}

		private void LogBadEmitParameters()
		{
			Debug.LogError("Mismatching parameter type(s) between message listener and emit request.");
		}

		private void LogBadListenerParameters()
		{
			Debug.LogError("Mismatching parameter type(s) between recently adding message listener and already added message listeners.");
		}

		#endregion

		#region Debug

		public void DebugLogListAllListeners()
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("Listing all listeners (message count: {0})\n",
				ListenerInfoDictionary == null ? 0 : ListenerInfoDictionary.Count);

			foreach (var listenerInfo in ListenerInfoDictionary.Values)
			{
				var delegates = listenerInfo.Delegates;
				stringBuilder.AppendFormat("   Message ID: {0}    Listeners: {1}\n",
					listenerInfo.MessageId,
					delegates == null ? 0 : delegates.Count);

				if (delegates == null)
					continue;

				for (int i = 0; i < delegates.Count; i++)
				{
					var item = delegates[i];
					if (item != null)
					{
						var target = item.Target as Object;
						stringBuilder.AppendFormat("      Target: {0}      \tMethod: {1}\n",
							target == null ? "null" : target.name,
							item.Method.Name);
					}
					else
					{
						stringBuilder.Append("      null\n");
					}
				}
			}

			Debug.Log(stringBuilder.ToString());
		}

		#endregion
	}

}
