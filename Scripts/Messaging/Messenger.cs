using System;
using UnityEngine;
using Extenity.Logging;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Extenity.Messaging
{

	public class Messenger : MonoBehaviour
	{
		#region Global

		private static Messenger _Global;
		public static Messenger Global
		{
			get
			{
				if (_Global == null)
				{
					var go = new GameObject("_GlobalMessenger", typeof(Messenger));
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

		#region Message Receivers

		private Dictionary<int, List<Delegate>> Receivers = new Dictionary<int, List<Delegate>>();

		private List<Delegate> GetReceivers(int messageId)
		{
			List<Delegate> receivers;
			Receivers.TryGetValue(messageId, out receivers);
			return receivers;
		}

		#endregion

		#region Add Receiver

		public void AddReceiver(int messageId, MessengerAction<bool> receiver) { AddReceiver(messageId, (Delegate)receiver); }
		public void AddReceiver(int messageId, MessengerAction<byte> receiver) { AddReceiver(messageId, (Delegate)receiver); }
		public void AddReceiver(int messageId, MessengerAction<Int16> receiver) { AddReceiver(messageId, (Delegate)receiver); }
		public void AddReceiver(int messageId, MessengerAction<Int32> receiver) { AddReceiver(messageId, (Delegate)receiver); }
		public void AddReceiver(int messageId, MessengerAction<Int64> receiver) { AddReceiver(messageId, (Delegate)receiver); }
		public void AddReceiver(int messageId, MessengerAction<UInt16> receiver) { AddReceiver(messageId, (Delegate)receiver); }
		public void AddReceiver(int messageId, MessengerAction<UInt32> receiver) { AddReceiver(messageId, (Delegate)receiver); }
		public void AddReceiver(int messageId, MessengerAction<UInt64> receiver) { AddReceiver(messageId, (Delegate)receiver); }
		public void AddReceiver(int messageId, MessengerAction<float> receiver) { AddReceiver(messageId, (Delegate)receiver); }
		public void AddReceiver(int messageId, MessengerAction<double> receiver) { AddReceiver(messageId, (Delegate)receiver); }
		public void AddReceiver(int messageId, MessengerAction<char> receiver) { AddReceiver(messageId, (Delegate)receiver); }
		public void AddReceiver(int messageId, MessengerAction<string> receiver) { AddReceiver(messageId, (Delegate)receiver); }
		public void AddReceiver<TParam1>(int messageId, MessengerAction<TParam1> receiver) { AddReceiver(messageId, (Delegate)receiver); }
		public void AddReceiver<TParam1, TParam2>(int messageId, MessengerAction<TParam1, TParam2> receiver) { AddReceiver(messageId, (Delegate)receiver); }
		public void AddReceiver<TParam1, TParam2, TParam3>(int messageId, MessengerAction<TParam1, TParam2, TParam3> receiver) { AddReceiver(messageId, (Delegate)receiver); }
		public void AddReceiver<TParam1, TParam2, TParam3, TParam4>(int messageId, MessengerAction<TParam1, TParam2, TParam3, TParam4> receiver) { AddReceiver(messageId, (Delegate)receiver); }
		public void AddReceiver<TParam1, TParam2, TParam3, TParam4, TParam5>(int messageId, MessengerAction<TParam1, TParam2, TParam3, TParam4, TParam5> receiver) { AddReceiver(messageId, (Delegate)receiver); }
		public void AddReceiver<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(int messageId, MessengerAction<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> receiver) { AddReceiver(messageId, (Delegate)receiver); }
		public void AddReceiver<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7>(int messageId, MessengerAction<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> receiver) { AddReceiver(messageId, (Delegate)receiver); }
		public void AddReceiver<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8>(int messageId, MessengerAction<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> receiver) { AddReceiver(messageId, (Delegate)receiver); }
		public void AddReceiver<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9>(int messageId, MessengerAction<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9> receiver) { AddReceiver(messageId, (Delegate)receiver); }

		public void AddReceiver(int messageId, MessengerAction receiver)
		{
			AddReceiver(messageId, (Delegate)receiver);
		}

		public void AddReceiver(int messageId, Delegate receiver)
		{
			// At this point, we may want to check for any return and input parameter inconsistencies in the future.
			//receiver.Method.ReturnParameter
			//receiver.Method.GetParameters()

			if ((receiver.Target as Object) == null)
			{
				LogAddNonUnityObject();
				return;
			}

			List<Delegate> delegates;
			if (!Receivers.TryGetValue(messageId, out delegates))
			{
				delegates = new List<Delegate>(50);
				Receivers.Add(messageId, delegates);

				// Do the initialization for this messageId
				{
					// Optimization ID-150827532:
					// Create a method that will tell us about receiver method's parameter structure.
					// Then add the method to receivers list as first item. We will use this first
					// -special- method to check if parameters of following registered methods matches
					// the parameters of this first added method. This way we can get rid of one
					// 'Method.GetParameters()' call in every receiver registration.
					var cached = receiver.Method.GetParameters();
					delegates.Add((Func<ParameterInfo[]>)delegate { return cached; });
				}

				// Add receiver to list and instantly return without getting into further consistency checks.
				delegates.Add(receiver);
				return;
			}

			// Prevent duplicate entries
			if (!delegates.Contains(receiver))
			{
				// Make sure all receiver methods are identical (that is, recently added method is identical with the first added method in receivers list)
				if (delegates.Count > 0)
				{
					// Optimization ID-150827532:
					// First parameter in receivers list is always a special method that tells about
					// parameter structure of receivers for this messageId.
					var parameters = ((Func<ParameterInfo[]>)delegates[0])();
					var newReceiverParameters = receiver.Method.GetParameters(); // This call is bad for performance but no other workaround exists for comparing two methods' parameters.

					if (!parameters.CompareMethodParameters(newReceiverParameters, false))
					{
						LogBadReceiverParameters();
					}
				}

				delegates.Add(receiver);
			}
		}

		#endregion

		#region Remove Receiver

		public bool RemoveReceiver(int messageId, Delegate receiver)
		{
			List<Delegate> delegates;
			if (!Receivers.TryGetValue(messageId, out delegates))
				return false;
			return delegates.Remove(receiver);
		}

		#endregion

		#region Emit Message

		public void Emit(int messageId)
		{
			var receivers = GetReceivers(messageId);
			if (receivers == null)
				return;
			if (receivers.Count <= 1) // Optimization ID-150827532:
				return;
			const int startIndex = 1; // Optimization ID-150827532:
			for (int i = startIndex; i < receivers.Count; i++)
			{
				var castReceiver = receivers[i] as MessengerAction;
				if (castReceiver != null)
				{
					if ((castReceiver.Target as MonoBehaviour) != null) // Check if the object is still alive
						castReceiver.Invoke();
				}
				else
					LogBadEmitParameters();
			}
		}

		public void Emit<T1>(int messageId, T1 param1)
		{
			var receivers = GetReceivers(messageId);
			if (receivers == null)
				return;
			if (receivers.Count <= 1) // Optimization ID-150827532:
				return;
			const int startIndex = 1; // Optimization ID-150827532:
			for (int i = startIndex; i < receivers.Count; i++)
			{
				var castReceiver = receivers[i] as MessengerAction<T1>;
				if (castReceiver != null)
				{
					if ((castReceiver.Target as MonoBehaviour) != null) // Check if the object is still alive
						castReceiver.Invoke(param1);
				}
				else
					LogBadEmitParameters();
			}
		}

		public void Emit<T1, T2>(int messageId, T1 param1, T2 param2)
		{
			var receivers = GetReceivers(messageId);
			if (receivers == null)
				return;
			if (receivers.Count <= 1) // Optimization ID-150827532:
				return;
			const int startIndex = 1; // Optimization ID-150827532:
			for (int i = startIndex; i < receivers.Count; i++)
			{
				var castReceiver = receivers[i] as MessengerAction<T1, T2>;
				if (castReceiver != null)
				{
					if ((castReceiver.Target as MonoBehaviour) != null) // Check if the object is still alive
						castReceiver.Invoke(param1, param2);
				}
				else
					LogBadEmitParameters();
			}
		}

		public void Emit<T1, T2, T3>(int messageId, T1 param1, T2 param2, T3 param3)
		{
			var receivers = GetReceivers(messageId);
			if (receivers == null)
				return;
			if (receivers.Count <= 1) // Optimization ID-150827532:
				return;
			const int startIndex = 1; // Optimization ID-150827532:
			for (int i = startIndex; i < receivers.Count; i++)
			{
				var castReceiver = receivers[i] as MessengerAction<T1, T2, T3>;
				if (castReceiver != null)
				{
					if ((castReceiver.Target as MonoBehaviour) != null) // Check if the object is still alive
						castReceiver.Invoke(param1, param2, param3);
				}
				else
					LogBadEmitParameters();
			}
		}

		public void Emit<T1, T2, T3, T4>(int messageId, T1 param1, T2 param2, T3 param3, T4 param4)
		{
			var receivers = GetReceivers(messageId);
			if (receivers == null)
				return;
			if (receivers.Count <= 1) // Optimization ID-150827532:
				return;
			const int startIndex = 1; // Optimization ID-150827532:
			for (int i = startIndex; i < receivers.Count; i++)
			{
				var castReceiver = receivers[i] as MessengerAction<T1, T2, T3, T4>;
				if (castReceiver != null)
				{
					if ((castReceiver.Target as MonoBehaviour) != null) // Check if the object is still alive
						castReceiver.Invoke(param1, param2, param3, param4);
				}
				else
					LogBadEmitParameters();
			}
		}

		public void Emit<T1, T2, T3, T4, T5>(int messageId, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
		{
			var receivers = GetReceivers(messageId);
			if (receivers == null)
				return;
			if (receivers.Count <= 1) // Optimization ID-150827532:
				return;
			const int startIndex = 1; // Optimization ID-150827532:
			for (int i = startIndex; i < receivers.Count; i++)
			{
				var castReceiver = receivers[i] as MessengerAction<T1, T2, T3, T4, T5>;
				if (castReceiver != null)
				{
					if ((castReceiver.Target as MonoBehaviour) != null) // Check if the object is still alive
						castReceiver.Invoke(param1, param2, param3, param4, param5);
				}
				else
					LogBadEmitParameters();
			}
		}

		public void Emit<T1, T2, T3, T4, T5, T6>(int messageId, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6)
		{
			var receivers = GetReceivers(messageId);
			if (receivers == null)
				return;
			if (receivers.Count <= 1) // Optimization ID-150827532:
				return;
			const int startIndex = 1; // Optimization ID-150827532:
			for (int i = startIndex; i < receivers.Count; i++)
			{
				var castReceiver = receivers[i] as MessengerAction<T1, T2, T3, T4, T5, T6>;
				if (castReceiver != null)
				{
					if ((castReceiver.Target as MonoBehaviour) != null) // Check if the object is still alive
						castReceiver.Invoke(param1, param2, param3, param4, param5, param6);
				}
				else
					LogBadEmitParameters();
			}
		}

		public void Emit<T1, T2, T3, T4, T5, T6, T7>(int messageId, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7)
		{
			var receivers = GetReceivers(messageId);
			if (receivers == null)
				return;
			if (receivers.Count <= 1) // Optimization ID-150827532:
				return;
			const int startIndex = 1; // Optimization ID-150827532:
			for (int i = startIndex; i < receivers.Count; i++)
			{
				var castReceiver = receivers[i] as MessengerAction<T1, T2, T3, T4, T5, T6, T7>;
				if (castReceiver != null)
				{
					if ((castReceiver.Target as MonoBehaviour) != null) // Check if the object is still alive
						castReceiver.Invoke(param1, param2, param3, param4, param5, param6, param7);
				}
				else
					LogBadEmitParameters();
			}
		}

		public void Emit<T1, T2, T3, T4, T5, T6, T7, T8>(int messageId, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8)
		{
			var receivers = GetReceivers(messageId);
			if (receivers == null)
				return;
			if (receivers.Count <= 1) // Optimization ID-150827532:
				return;
			const int startIndex = 1; // Optimization ID-150827532:
			for (int i = startIndex; i < receivers.Count; i++)
			{
				var castReceiver = receivers[i] as MessengerAction<T1, T2, T3, T4, T5, T6, T7, T8>;
				if (castReceiver != null)
				{
					if ((castReceiver.Target as MonoBehaviour) != null) // Check if the object is still alive
						castReceiver.Invoke(param1, param2, param3, param4, param5, param6, param7, param8);
				}
				else
					LogBadEmitParameters();
			}
		}

		public void Emit<T1, T2, T3, T4, T5, T6, T7, T8, T9>(int messageId, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8, T9 param9)
		{
			var receivers = GetReceivers(messageId);
			if (receivers == null)
				return;
			const int startIndex = 1; // Optimization ID-150827532:
			for (int i = startIndex; i < receivers.Count; i++)
			{
				var castReceiver = receivers[i] as MessengerAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>;
				if (castReceiver != null)
				{
					if ((castReceiver.Target as MonoBehaviour) != null) // Check if the object is still alive
						castReceiver.Invoke(param1, param2, param3, param4, param5, param6, param7, param8, param9);
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
			Debug.LogError("Mismatching parameter type(s) between message receiver and emit request.");
		}

		private void LogBadReceiverParameters()
		{
			Debug.LogError("Mismatching parameter type(s) between recently adding message receiver and already added message receivers.");
		}

		#endregion
	}

}
