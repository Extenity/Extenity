#if UNITY // TODO-UniversalExtenity: Implement Messenger for Universal project.

#if ExtenityMessenger && !UseLegacyMessenger

using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Extenity.DataToolbox;
using Extenity.ReflectionToolbox;
using Sirenix.OdinInspector;
using Object = UnityEngine.Object;

namespace Extenity.MessagingToolbox
{

	// TODO: These features are completed. They need to be documented.
	// User should have the ability to order event calls. Maybe by specifying a 'priority' parameter.

	// TODO: Consider these before starting to use Messenger.
	// Event dispatching should be exception proof. A thrown exception should not break the operation. Better yet, this can be controlled by specifying per registration.
	// Event registration and deregistration should not allocate memory frequently.
	// Consider making a class alternative to UnityEvent that can be used as a field to a class. Forget about making it inspector friendly. It should not work in inspector, since this is a really bad design issue. If linking callbacks inside Unity Editor is required, user should use a UnityEvent for this, instead of using Messenger system.
	// Consider making a compile-time type checking mechanism, rather than checking in runtime. Performance would increase drastically.
	// Consider how we can profile callbacks in a Unity friendly way, which would be great to allow selection of specific events, and without causing any performance hit when profiling is disabled.
	// Consider supporting non UnityEngine.Object objects as targets, but only if this won't require considerable amount of work or does not require giving up on any other good feature.
	// Feature: One shot calls.
	// Feature: Ability to deregister from event easily, inside the callback. Something like Messenger.Deregister(gameObject, MyCallback) or better yet, Messenger.DeregisterCurrentCallback(). Note that DeregisterCurrentCallback should be thread safe. Also it's better to check if we really are in the process of calling the callback, and throw an error if called outside of callback.
	// Feature: Deregistering a callback while dispatching the event should not break anything.

	// TODO: Proper Switch cleanup. It would be great to have a functionality that checks the underlying system status.

	public class Messenger : MonoBehaviour
	{
		#region Configuration

		private const int ListenerDelagateListCapacity = 10;

		#endregion

		#region Initialization

		protected void Awake()
		{
			Loop.RegisterLateUpdate(CustomLateUpdate);
		}

		#endregion

		#region Deinitialization

		protected void OnDestroy()
		{
			// TODO: Check if there is any message left to process and warn developer about that.

			Loop.DeregisterLateUpdate(CustomLateUpdate);
		}

		#endregion

		#region Update

		protected void CustomLateUpdate()
		{
			if (MessageListenerListCleanupRequired)
			{
				MessageListenerListCleanupRequired = false;
				CleanUpMessageListenerLists();
			}
			if (SwitchListenerListCleanupRequired)
			{
				SwitchListenerListCleanupRequired = false;
				CleanUpSwitchListenerLists();
			}
		}

		#endregion

		#region Global / Main

		/// <summary>
		/// The main Messenger that is used widespread. The application creates that Messenger and responsible for its
		/// life cycle. The application can also create more than one Messenger independent of this Main Messenger,
		/// for its specific tasks.
		/// </summary>
		public static Messenger Main;

		// Realized that the Messenger is an essential subsystem of the application and should be reachable from
		// anywhere that can reach to Extenity. But still, the application should create its own Messenger and assign
		// it to this Main static field at the very beginning of its life cycle.

		// Previously was:
		/* Nope! Not happening! Singletons are bad for software architecture. The application should create and control its own messengers. There should not be a singleton messenger.
		private static Messenger _Global;
		public static Messenger Global
		{
			get
			{
				if (!_Global)
				{
					var go = new GameObject("_GlobalMessenger", typeof(Messenger));
					go.hideFlags = HideFlags.HideAndDontSave;
					_Global = go.GetComponent<Messenger>();
				}
				return _Global;
			}
		}
		*/

		#endregion

		#region Message - Actions

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

		#region Message - Listeners

		private struct MessageListenerInfo
		{
			public string MessageId;
			public ParameterInfo[] ParameterInfos;
			public List<Delegate> Delegates;

			public bool IsValid
			{
				get { return !string.IsNullOrEmpty(MessageId); }
			}
			public bool IsNotEmpty
			{
				get { return Delegates != null && Delegates.Count > 0; }
			}
			public bool IsValidAndNotEmpty
			{
				get { return IsValid && IsNotEmpty; }
			}
		}

		private readonly Dictionary<string, MessageListenerInfo> MessageListenerInfoDictionary = new Dictionary<string, MessageListenerInfo>();

		private MessageListenerInfo GetMessageListenerInfo(string messageId)
		{
			MessageListenerInfoDictionary.TryGetValue(messageId, out var listenerInfo);
			return listenerInfo;
		}

		private List<Delegate> GetMessageDelegates(string messageId)
		{
			if (MessageListenerInfoDictionary.TryGetValue(messageId, out var listenerInfo))
			{
				if (listenerInfo.IsNotEmpty)
				{
					return listenerInfo.Delegates;
				}
			}
			return null;
		}

		#endregion

		#region Message - Listeners Cleanup

		[NonSerialized]
		public bool MessageListenerListCleanupRequired;

		private void CleanUpMessageListenerLists()
		{
			foreach (var listenerInfo in MessageListenerInfoDictionary.Values)
			{
				if (!listenerInfo.IsValidAndNotEmpty)
					continue;
				_DelegateCleanup(listenerInfo.Delegates);
			}
		}

		#endregion

		#region Message - Add Listener

		/// See <see cref="AddMessageListener(string,Delegate)"/>
		public void AddMessageListener(string messageId, MessengerAction listener) { AddMessageListener(messageId, (Delegate)listener); }

		public void AddMessageListener(string messageId, MessengerAction<bool> listener) { AddMessageListener(messageId, (Delegate)listener); }
		public void AddMessageListener(string messageId, MessengerAction<byte> listener) { AddMessageListener(messageId, (Delegate)listener); }
		public void AddMessageListener(string messageId, MessengerAction<Int16> listener) { AddMessageListener(messageId, (Delegate)listener); }
		public void AddMessageListener(string messageId, MessengerAction<Int32> listener) { AddMessageListener(messageId, (Delegate)listener); }
		public void AddMessageListener(string messageId, MessengerAction<Int64> listener) { AddMessageListener(messageId, (Delegate)listener); }
		public void AddMessageListener(string messageId, MessengerAction<UInt16> listener) { AddMessageListener(messageId, (Delegate)listener); }
		public void AddMessageListener(string messageId, MessengerAction<UInt32> listener) { AddMessageListener(messageId, (Delegate)listener); }
		public void AddMessageListener(string messageId, MessengerAction<UInt64> listener) { AddMessageListener(messageId, (Delegate)listener); }
		public void AddMessageListener(string messageId, MessengerAction<float> listener) { AddMessageListener(messageId, (Delegate)listener); }
		public void AddMessageListener(string messageId, MessengerAction<double> listener) { AddMessageListener(messageId, (Delegate)listener); }
		public void AddMessageListener(string messageId, MessengerAction<char> listener) { AddMessageListener(messageId, (Delegate)listener); }
		public void AddMessageListener(string messageId, MessengerAction<string> listener) { AddMessageListener(messageId, (Delegate)listener); }
		public void AddMessageListener<TParam1>(string messageId, MessengerAction<TParam1> listener) { AddMessageListener(messageId, (Delegate)listener); }
		public void AddMessageListener<TParam1, TParam2>(string messageId, MessengerAction<TParam1, TParam2> listener) { AddMessageListener(messageId, (Delegate)listener); }
		public void AddMessageListener<TParam1, TParam2, TParam3>(string messageId, MessengerAction<TParam1, TParam2, TParam3> listener) { AddMessageListener(messageId, (Delegate)listener); }
		public void AddMessageListener<TParam1, TParam2, TParam3, TParam4>(string messageId, MessengerAction<TParam1, TParam2, TParam3, TParam4> listener) { AddMessageListener(messageId, (Delegate)listener); }
		public void AddMessageListener<TParam1, TParam2, TParam3, TParam4, TParam5>(string messageId, MessengerAction<TParam1, TParam2, TParam3, TParam4, TParam5> listener) { AddMessageListener(messageId, (Delegate)listener); }
		public void AddMessageListener<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(string messageId, MessengerAction<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> listener) { AddMessageListener(messageId, (Delegate)listener); }
		public void AddMessageListener<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(string messageId, MessengerAction<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> listener) { AddMessageListener(messageId, (Delegate)listener); }
		public void AddMessageListener<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(string messageId, MessengerAction<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> listener) { AddMessageListener(messageId, (Delegate)listener); }
		public void AddMessageListener<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>(string messageId, MessengerAction<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9> listener) { AddMessageListener(messageId, (Delegate)listener); }

		public void AddMessageListener(string messageId, Delegate listener)
		{
			if (listener == null)
				throw new ArgumentNullException(nameof(listener));
			if (string.IsNullOrEmpty(messageId))
				throw new ArgumentOutOfRangeException(nameof(messageId), "Message ID should not be empty.");

			// At this point, we may want to check for any return and input parameter inconsistencies in the future.
			//listener.Method.ReturnParameter
			//listener.Method.GetParameters()

			if ((listener.Target as Object) == null)
			{
				LogError_AddingNonUnityObjectAsMessageListener();
				return;
			}

			// Is this the first time we add a listener for this messageId?
			if (!MessageListenerInfoDictionary.TryGetValue(messageId, out var listenerInfo))
			{
				// Do the initialization for this messageId
				{
					listenerInfo.MessageId = messageId;

					// Create a brand new delegate list and add listener to list
					listenerInfo.Delegates = new List<Delegate>(ListenerDelagateListCapacity);
					listenerInfo.Delegates.Add(listener);

					// Optimization ID-150827532:
					// Get method parameters of this initially added listener. This parameter info 
					// will be used to check if parameters of following registered listeners matches
					// the parameters of this first added method. This way we can get rid of one
					// 'Method.GetParameters()' call in every listener registration.
					listenerInfo.ParameterInfos = listener.Method.GetParameters();
				}

				MessageListenerInfoDictionary.Add(messageId, listenerInfo);

				// Instantly return without getting into further consistency checks.
				return;
			}

			_AddDelegateEnsuringNoDuplicates(ref listenerInfo.Delegates, listener);

			// Make sure all listener methods are identical (that is, recently added method is identical with the first added method in listeners list)
			{
				// Optimization ID-150827532:
				var newListenerParameters = listener.Method.GetParameters(); // This call is bad for performance but no other workaround exists for comparing parameters of two methods.
				if (!listenerInfo.ParameterInfos.CompareMethodParameters(newListenerParameters, false))
				{
					LogError_BadMessageListenerParameters();
				}
			}
		}

		private void _AddDelegateEnsuringNoDuplicates(ref List<Delegate> delegates, Delegate listener)
		{
			// Create new list if necessary
			if (delegates == null)
			{
				delegates = new List<Delegate>(ListenerDelagateListCapacity);
			}

			// Prevent duplicate entries
			if (delegates.Contains(listener))
			{
				return;
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

		#endregion

		#region Message - Remove Listener

		/// See <see cref="RemoveMessageListener(string,Delegate)"/>
		public bool RemoveMessageListener(string messageId, MessengerAction listener) { return RemoveMessageListener(messageId, (Delegate)listener); }

		public bool RemoveMessageListener(string messageId, MessengerAction<bool> listener) { return RemoveMessageListener(messageId, (Delegate)listener); }
		public bool RemoveMessageListener(string messageId, MessengerAction<byte> listener) { return RemoveMessageListener(messageId, (Delegate)listener); }
		public bool RemoveMessageListener(string messageId, MessengerAction<Int16> listener) { return RemoveMessageListener(messageId, (Delegate)listener); }
		public bool RemoveMessageListener(string messageId, MessengerAction<Int32> listener) { return RemoveMessageListener(messageId, (Delegate)listener); }
		public bool RemoveMessageListener(string messageId, MessengerAction<Int64> listener) { return RemoveMessageListener(messageId, (Delegate)listener); }
		public bool RemoveMessageListener(string messageId, MessengerAction<UInt16> listener) { return RemoveMessageListener(messageId, (Delegate)listener); }
		public bool RemoveMessageListener(string messageId, MessengerAction<UInt32> listener) { return RemoveMessageListener(messageId, (Delegate)listener); }
		public bool RemoveMessageListener(string messageId, MessengerAction<UInt64> listener) { return RemoveMessageListener(messageId, (Delegate)listener); }
		public bool RemoveMessageListener(string messageId, MessengerAction<float> listener) { return RemoveMessageListener(messageId, (Delegate)listener); }
		public bool RemoveMessageListener(string messageId, MessengerAction<double> listener) { return RemoveMessageListener(messageId, (Delegate)listener); }
		public bool RemoveMessageListener(string messageId, MessengerAction<char> listener) { return RemoveMessageListener(messageId, (Delegate)listener); }
		public bool RemoveMessageListener(string messageId, MessengerAction<string> listener) { return RemoveMessageListener(messageId, (Delegate)listener); }
		public bool RemoveMessageListener<TParam1>(string messageId, MessengerAction<TParam1> listener) { return RemoveMessageListener(messageId, (Delegate)listener); }
		public bool RemoveMessageListener<TParam1, TParam2>(string messageId, MessengerAction<TParam1, TParam2> listener) { return RemoveMessageListener(messageId, (Delegate)listener); }
		public bool RemoveMessageListener<TParam1, TParam2, TParam3>(string messageId, MessengerAction<TParam1, TParam2, TParam3> listener) { return RemoveMessageListener(messageId, (Delegate)listener); }
		public bool RemoveMessageListener<TParam1, TParam2, TParam3, TParam4>(string messageId, MessengerAction<TParam1, TParam2, TParam3, TParam4> listener) { return RemoveMessageListener(messageId, (Delegate)listener); }
		public bool RemoveMessageListener<TParam1, TParam2, TParam3, TParam4, TParam5>(string messageId, MessengerAction<TParam1, TParam2, TParam3, TParam4, TParam5> listener) { return RemoveMessageListener(messageId, (Delegate)listener); }
		public bool RemoveMessageListener<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(string messageId, MessengerAction<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> listener) { return RemoveMessageListener(messageId, (Delegate)listener); }
		public bool RemoveMessageListener<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(string messageId, MessengerAction<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> listener) { return RemoveMessageListener(messageId, (Delegate)listener); }
		public bool RemoveMessageListener<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(string messageId, MessengerAction<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> listener) { return RemoveMessageListener(messageId, (Delegate)listener); }
		public bool RemoveMessageListener<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>(string messageId, MessengerAction<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9> listener) { return RemoveMessageListener(messageId, (Delegate)listener); }

		public bool RemoveMessageListener(string messageId, Delegate listener)
		{
			if (listener == null)
				throw new ArgumentNullException(nameof(listener));
			if (string.IsNullOrEmpty(messageId))
				throw new ArgumentOutOfRangeException(nameof(messageId), "Message ID should not be empty.");

			if (!MessageListenerInfoDictionary.TryGetValue(messageId, out var listenerInfo))
				return false;

			if (listenerInfo.Delegates != null)
			{
				lock (listenerInfo.Delegates)
				{
					return listenerInfo.Delegates.Remove(listener);
				}
			}
			return false;
		}

		#endregion

		#region Message - Emit

		public void EmitMessage(string messageId)
		{
			var delegates = GetMessageDelegates(messageId);
			if (delegates == null)
				return;
			for (int i = 0; i < delegates.Count; i++)
			{
				var castListener = delegates[i] as MessengerAction;
				if (castListener != null)
				{
					if (castListener.Target as Object) // Check if the object is not destroyed
					{
						try
						{
							castListener.Invoke();
						}
						catch (Exception exception)
						{
							Log.Exception(exception, castListener.Target as Object);
						}
					}
					else
						MessageListenerListCleanupRequired = true;
				}
				else
					LogError_BadMessageEmitParameters();
			}
		}

		public void EmitMessage<T1>(string messageId, T1 param1)
		{
			var delegates = GetMessageDelegates(messageId);
			if (delegates == null)
				return;
			for (int i = 0; i < delegates.Count; i++)
			{
				var castListener = delegates[i] as MessengerAction<T1>;
				if (castListener != null)
				{
					if (castListener.Target as Object) // Check if the object is not destroyed
					{
						try
						{
							castListener.Invoke(param1);
						}
						catch (Exception exception)
						{
							Log.Exception(exception, castListener.Target as Object);
						}
					}
					else
						MessageListenerListCleanupRequired = true;
				}
				else
					LogError_BadMessageEmitParameters();
			}
		}

		public void EmitMessage<T1, T2>(string messageId, T1 param1, T2 param2)
		{
			var delegates = GetMessageDelegates(messageId);
			if (delegates == null)
				return;
			for (int i = 0; i < delegates.Count; i++)
			{
				var castListener = delegates[i] as MessengerAction<T1, T2>;
				if (castListener != null)
				{
					if (castListener.Target as Object) // Check if the object is not destroyed
					{
						try
						{
							castListener.Invoke(param1, param2);
						}
						catch (Exception exception)
						{
							Log.Exception(exception, castListener.Target as Object);
						}
					}
					else
						MessageListenerListCleanupRequired = true;
				}
				else
					LogError_BadMessageEmitParameters();
			}
		}

		public void EmitMessage<T1, T2, T3>(string messageId, T1 param1, T2 param2, T3 param3)
		{
			var delegates = GetMessageDelegates(messageId);
			if (delegates == null)
				return;
			for (int i = 0; i < delegates.Count; i++)
			{
				var castListener = delegates[i] as MessengerAction<T1, T2, T3>;
				if (castListener != null)
				{
					if (castListener.Target as Object) // Check if the object is not destroyed
					{
						try
						{
							castListener.Invoke(param1, param2, param3);
						}
						catch (Exception exception)
						{
							Log.Exception(exception, castListener.Target as Object);
						}
					}
					else
						MessageListenerListCleanupRequired = true;
				}
				else
					LogError_BadMessageEmitParameters();
			}
		}

		public void EmitMessage<T1, T2, T3, T4>(string messageId, T1 param1, T2 param2, T3 param3, T4 param4)
		{
			var delegates = GetMessageDelegates(messageId);
			if (delegates == null)
				return;
			for (int i = 0; i < delegates.Count; i++)
			{
				var castListener = delegates[i] as MessengerAction<T1, T2, T3, T4>;
				if (castListener != null)
				{
					if (castListener.Target as Object) // Check if the object is not destroyed
					{
						try
						{
							castListener.Invoke(param1, param2, param3, param4);
						}
						catch (Exception exception)
						{
							Log.Exception(exception, castListener.Target as Object);
						}
					}
					else
						MessageListenerListCleanupRequired = true;
				}
				else
					LogError_BadMessageEmitParameters();
			}
		}

		public void EmitMessage<T1, T2, T3, T4, T5>(string messageId, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
		{
			var delegates = GetMessageDelegates(messageId);
			if (delegates == null)
				return;
			for (int i = 0; i < delegates.Count; i++)
			{
				var castListener = delegates[i] as MessengerAction<T1, T2, T3, T4, T5>;
				if (castListener != null)
				{
					if (castListener.Target as Object) // Check if the object is not destroyed
					{
						try
						{
							castListener.Invoke(param1, param2, param3, param4, param5);
						}
						catch (Exception exception)
						{
							Log.Exception(exception, castListener.Target as Object);
						}
					}
					else
						MessageListenerListCleanupRequired = true;
				}
				else
					LogError_BadMessageEmitParameters();
			}
		}

		public void EmitMessage<T1, T2, T3, T4, T5, T6>(string messageId, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6)
		{
			var delegates = GetMessageDelegates(messageId);
			if (delegates == null)
				return;
			for (int i = 0; i < delegates.Count; i++)
			{
				var castListener = delegates[i] as MessengerAction<T1, T2, T3, T4, T5, T6>;
				if (castListener != null)
				{
					if (castListener.Target as Object) // Check if the object is not destroyed
					{
						try
						{
							castListener.Invoke(param1, param2, param3, param4, param5, param6);
						}
						catch (Exception exception)
						{
							Log.Exception(exception, castListener.Target as Object);
						}
					}
					else
						MessageListenerListCleanupRequired = true;
				}
				else
					LogError_BadMessageEmitParameters();
			}
		}

		public void EmitMessage<T1, T2, T3, T4, T5, T6, T7>(string messageId, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7)
		{
			var delegates = GetMessageDelegates(messageId);
			if (delegates == null)
				return;
			for (int i = 0; i < delegates.Count; i++)
			{
				var castListener = delegates[i] as MessengerAction<T1, T2, T3, T4, T5, T6, T7>;
				if (castListener != null)
				{
					if (castListener.Target as Object) // Check if the object is not destroyed
					{
						try
						{
							castListener.Invoke(param1, param2, param3, param4, param5, param6, param7);
						}
						catch (Exception exception)
						{
							Log.Exception(exception, castListener.Target as Object);
						}
					}
					else
						MessageListenerListCleanupRequired = true;
				}
				else
					LogError_BadMessageEmitParameters();
			}
		}

		public void EmitMessage<T1, T2, T3, T4, T5, T6, T7, T8>(string messageId, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8)
		{
			var delegates = GetMessageDelegates(messageId);
			if (delegates == null)
				return;
			for (int i = 0; i < delegates.Count; i++)
			{
				var castListener = delegates[i] as MessengerAction<T1, T2, T3, T4, T5, T6, T7, T8>;
				if (castListener != null)
				{
					if (castListener.Target as Object) // Check if the object is not destroyed
					{
						try
						{
							castListener.Invoke(param1, param2, param3, param4, param5, param6, param7, param8);
						}
						catch (Exception exception)
						{
							Log.Exception(exception, castListener.Target as Object);
						}
					}
					else
						MessageListenerListCleanupRequired = true;
				}
				else
					LogError_BadMessageEmitParameters();
			}
		}

		public void EmitMessage<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string messageId, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, T9 param9)
		{
			var delegates = GetMessageDelegates(messageId);
			if (delegates == null)
				return;
			for (int i = 0; i < delegates.Count; i++)
			{
				var castListener = delegates[i] as MessengerAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>;
				if (castListener != null)
				{
					if (castListener.Target as Object) // Check if the object is not destroyed
					{
						try
						{
							castListener.Invoke(param1, param2, param3, param4, param5, param6, param7, param8, param9);
						}
						catch (Exception exception)
						{
							Log.Exception(exception, castListener.Target as Object);
						}
					}
					else
						MessageListenerListCleanupRequired = true;
				}
				else
					LogError_BadMessageEmitParameters();
			}
		}

		#endregion

		#region Message - Log Errors

		private void LogError_AddingNonUnityObjectAsMessageListener()
		{
			Log.Fatal("Messaging system only allows adding methods of a Unity object (MonoBehaviour, GameObject, Component, etc.) as listener delegates.", gameObject);
		}

		private void LogError_BadMessageEmitParameters()
		{
			Log.Fatal("Mismatching parameter type(s) between message listener and emit request.", gameObject);
		}

		private void LogError_BadMessageListenerParameters()
		{
			Log.Fatal("Mismatching parameter type(s) between recently adding message listener and already added message listeners.", gameObject);
		}

		#endregion

		#region Message - Debug

		public void DebugLogListAllMessageListeners()
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("Listing all {0} Message listeners\n", MessageListenerInfoDictionary.Count);

			foreach (var listenerInfo in MessageListenerInfoDictionary.Values)
			{
				stringBuilder.AppendFormat("   Message ID: {0}    Listeners: {1}\n",
				                           listenerInfo.MessageId,
				                           listenerInfo.Delegates?.Count ?? 0);
				WriteDelegateDetails(stringBuilder, listenerInfo.Delegates);
			}

			Log.Info(stringBuilder.ToString(), gameObject);
		}

		#endregion

		#region Switch - Listeners

		private struct SwitchListenerInfo
		{
			public string SwitchId;
			public ExtenitySwitch Switch;

			public bool IsValid => !string.IsNullOrEmpty(SwitchId);
		}

		private readonly Dictionary<string, SwitchListenerInfo> SwitchListenerInfoDictionary = new Dictionary<string, SwitchListenerInfo>();

		public bool GetSwitch(string switchId)
		{
			if (SwitchListenerInfoDictionary.TryGetValue(switchId, out var listenerInfo))
			{
				return listenerInfo.Switch.IsSwitchedOn;
			}
			return false; // Default value of Switch is false.
		}

		#endregion

		#region Switch - Listeners Cleanup

		[NonSerialized]
		public bool SwitchListenerListCleanupRequired;

		public void CleanUpSwitchListenerLists()
		{
			foreach (var listenerInfo in SwitchListenerInfoDictionary.Values)
			{
				listenerInfo.Switch.CleanUp();
			}

			if (EnableVerboseSwitchLoggingInEveryModification)
				DebugLogSwitchStatus();
		}

		#endregion

		#region Switch - Registering Listeners

		public void AddSwitchListener(string switchId, Action switchOnCallback, Action switchOffCallback, int order = 0, ListenerLifeSpan lifeSpan = ListenerLifeSpan.Permanent, Object lifeSpanTarget = null)
		{
			// It's alright to not have the listeners.
			// if (switchOnListener == null)
			// 	throw new ArgumentNullException(nameof(switchOnListener));
			// if (switchOffListener == null)
			// 	throw new ArgumentNullException(nameof(switchOffListener));
			if (string.IsNullOrEmpty(switchId))
				throw new ArgumentOutOfRangeException(nameof(switchId), "Switch ID should not be empty.");

			// Is this the first time we add a listener for this switchId?
			if (!SwitchListenerInfoDictionary.TryGetValue(switchId, out var listenerInfo))
			{
				// WARNING! Intentional code duplication. See 1124618247.
				// Do the initialization for this switchId. Note that this is a struct, so treat carefully.
				listenerInfo.SwitchId = switchId;
				listenerInfo.Switch = new ExtenitySwitch();
				SwitchListenerInfoDictionary.Add(switchId, listenerInfo);
			}

			listenerInfo.Switch.AddListener(switchOnCallback, switchOffCallback, order, lifeSpan, lifeSpanTarget);
			if (EnableVerboseSwitchLoggingInEveryModification)
				DebugLogSwitchStatus();
		}

		public void RemoveSwitchListener(string switchId, Action switchOnCallback, Action switchOffCallback)
		{
			// It's alright to not have the listeners.
			// if (switchOnListener == null)
			// 	throw new ArgumentNullException(nameof(switchOnListener));
			// if (switchOffListener == null)
			// 	throw new ArgumentNullException(nameof(switchOffListener));
			if (string.IsNullOrEmpty(switchId))
				throw new ArgumentOutOfRangeException(nameof(switchId), "Switch ID should not be empty.");

			if (!SwitchListenerInfoDictionary.TryGetValue(switchId, out var listenerInfo))
				return;

			listenerInfo.Switch.RemoveListener(switchOnCallback, switchOffCallback);
			if (EnableVerboseSwitchLoggingInEveryModification)
				DebugLogSwitchStatus();
		}

		public bool IsAnySwitchListenerRegistered(string switchId)
		{
			if (!SwitchListenerInfoDictionary.TryGetValue(switchId, out var listenerInfo))
				return false;
			return listenerInfo.Switch.IsAnyAliveListenerRegistered;
		}

		public bool CheckIfAnySwitchIsOnOrHasRegisteredListeners()
		{
			return SwitchListenerInfoDictionary
			       .Values
			       .Any(listenerInfo => listenerInfo.Switch.IsSwitchedOn ||
			                            listenerInfo.Switch.IsAnyAliveListenerRegistered);
		}

		#endregion

		#region Switch - Switch On/Off

		public void SwitchOn(string switchId)
		{
			Switch(switchId, true);
		}

		public void SwitchOff(string switchId)
		{
			Switch(switchId, false);
		}

		public void Switch(string switchId, bool isSwitchedOn)
		{
			if (!SwitchListenerInfoDictionary.TryGetValue(switchId, out var listenerInfo))
			{
				// There is no listener registered before. So no delegate to call. But if switching on, we must keep
				// that information for future listeners.
				if (isSwitchedOn)
				{
					// WARNING! Intentional code duplication. See 1124618247.
					// Do the initialization for this switchId. Note that this is a struct, so treat carefully.
					listenerInfo.SwitchId = switchId;
					listenerInfo.Switch = new ExtenitySwitch();
					SwitchListenerInfoDictionary.Add(switchId, listenerInfo);

					listenerInfo.Switch.SwitchOnSafe();
				}
				if (EnableVerboseSwitchLoggingInEveryModification)
					DebugLogSwitchStatus();
				return;
			}
			listenerInfo.Switch.SwitchSafe(isSwitchedOn);
			if (EnableVerboseSwitchLoggingInEveryModification)
				DebugLogSwitchStatus();
		}

		#endregion

		#region Switch - Debug

		[FoldoutGroup("Debug")]
		public bool EnableVerboseSwitchLoggingInEveryModification = false;

		public string GetSwitchListenerDebugInfo(string switchId, string linePrefix)
		{
			if (!SwitchListenerInfoDictionary.TryGetValue(switchId, out var listenerInfo))
				return "";
			return listenerInfo.Switch.GetSwitchListenerDebugInfo(linePrefix);
		}

		public string GetSwitchStatusDebugInfo()
		{
			var stringBuilder = StringTools.SharedStringBuilder.Value;
			lock (stringBuilder)
			{
				stringBuilder.Clear(); // Make sure it is clean before starting to use.

				stringBuilder.AppendLine($"Listing {SwitchListenerInfoDictionary.Count} Switch entries:");
				foreach (var listenerInfo in SwitchListenerInfoDictionary.Values)
				{
					stringBuilder.AppendLine($"SwitchID: {listenerInfo.SwitchId} \t Status: {listenerInfo.Switch.IsSwitchedOn} \t Listeners: {listenerInfo.Switch.ListenersAliveCount} ({listenerInfo.Switch.ListenersCount} including unavailable)");
					listenerInfo.Switch.GetSwitchListenerDebugInfo(stringBuilder, "\t");
				}

				var result = stringBuilder.ToString();
				StringTools.ClearSharedStringBuilder(stringBuilder); // Make sure we will leave it clean after use.
				return result;
			}
		}

		public void DebugLogSwitchStatus()
		{
			Log.Verbose(GetSwitchStatusDebugInfo(), this);
		}

		#endregion

		#region General - Debug

		private static void WriteDelegateDetails(StringBuilder stringBuilder, List<Delegate> delegates)
		{
			if (delegates == null)
				return;

			for (int i = 0; i < delegates.Count; i++)
			{
				var item = delegates[i];
				if (item != null)
				{
					var target = item.Target as Object;
					stringBuilder.AppendFormat("      Target: {0}      \tMethod: {1}\n", target == null ? "null" : target.name, item.Method.Name);
				}
				else
				{
					stringBuilder.Append("      null\n");
				}
			}
		}

		#endregion

		#region General - Listener Cleanup

		private static void _DelegateCleanup(List<Delegate> delegates)
		{
			if (delegates == null || delegates.Count == 0)
				return;

			// TODO OPTIMIZATION: This is not the most efficient way to clear the list.
			var done = false;
			while (!done)
			{
				done = true;
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

		#endregion
	}

}

#endif

#endif
