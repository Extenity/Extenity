using System;
using UnityEngine;
using Extenity.Logging;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Logger = Extenity.Logging.Logger;
using Debug = UnityEngine.Debug;

namespace Extenity.Messaging
{

	public class Messenger : MonoBehaviour
	{
		#region Initialization

		protected virtual void Awake()
		{
			AddReceiver(1, Method_NoParam);

			AddReceiver(1, Method_1Param_String);

			//AddReceiver(3, Method_1Param_CustomType);
			//AddReceiver<string>(3, Method_1Param_CustomType);
			AddReceiver<CustomType>(3, Method_1Param_CustomType);

			//AddReceiver(4, Method_1Param_OutInt);
			//AddReceiver(5, Method_1Param_RefInt);

			Emit(1);
			Emit(1, "hehee");
			Emit(2, "hehee");
			Emit(3, new CustomType() { Integro = 151 });

			Emit(1, 3525);
			Emit(2, 46326);
			Emit(3, 737);
		}


		public class CustomType
		{
			public int Integro;
		}

		protected void Method_NoParam()
		{
			Logger.Log("###### Method_NoParam");
		}

		protected void Method_1Param_String(string text)
		{
			Logger.Log("###### Method_1Param_String    text: " + text);
		}

		protected void Method_1Param_OutInt(out int value)
		{
			value = 3;
			Logger.Log("###### Method_1Param_OutInt    value: " + value);
		}

		protected void Method_1Param_RefInt(ref int value)
		{
			Logger.Log("###### Method_1Param_RefInt    value: " + value);
			value = 3;
		}

		protected void Method_1Param_CustomType(CustomType data)
		{
			Logger.Log("###### Method_1Param_CustomType    data.Integro: " + data.Integro);
		}

		#endregion

		#region Deinitialization

		//protected virtual void OnDestroy()
		//{
		//}

		#endregion

		#region Update

		//protected virtual void Update()
		//{
		//}

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

		public void AddReceiver(int messageId, MessengerAction receiver)
		{
			AddReceiver(messageId, (Delegate)receiver);
		}

		public void AddReceiver(int messageId, Delegate receiver)
		{
			// TODO: throw if ReturnParameter != void
			//receiver.Method.ReturnParameter
			//receiver.Method.GetParameters()

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

		private List<Delegate> GetReceivers(int messageId)
		{
			List<Delegate> receivers;
			Receivers.TryGetValue(messageId, out receivers);
			return receivers;
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
					castReceiver.Invoke();
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
					castReceiver.Invoke(param1);
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
					castReceiver.Invoke(param1, param2);
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
					castReceiver.Invoke(param1, param2, param3);
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
					castReceiver.Invoke(param1, param2, param3, param4);
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
					castReceiver.Invoke(param1, param2, param3, param4, param5);
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
					castReceiver.Invoke(param1, param2, param3, param4, param5, param6);
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
					castReceiver.Invoke(param1, param2, param3, param4, param5, param6, param7);
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
					castReceiver.Invoke(param1, param2, param3, param4, param5, param6, param7, param8);
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
					castReceiver.Invoke(param1, param2, param3, param4, param5, param6, param7, param8, param9);
				else
					LogBadEmitParameters();
			}
		}

		#endregion

		#region Log Errors

		private void LogBadEmitParameters()
		{
			Debug.LogError("Mismatching parameter type(s) between message receiver and emit request.");
		}

		private void LogBadReceiverParameters()
		{
			Debug.LogError("Mismatching parameter type(s) between recently adding message receiver and already added message receivers.");
		}

		#endregion

		#region TEST - Dynamic Invoke Performance

		/*
		private void TEST_DynamicInvokePerformance()
		{
			if (Input.GetKeyDown(KeyCode.F1))
			{
				Func<int, int> twice = (x) => x * 2;
				const int LOOP = 500000;

				var watch = Stopwatch.StartNew();
				for (int i = 0; i < LOOP; i++)
				{
					twice(3);
				}
				watch.Stop();
				UnityEngine.Debug.LogFormat("Call: {0}ms", watch.Elapsed.TotalMilliseconds);

				watch = Stopwatch.StartNew();
				for (int i = 0; i < LOOP; i++)
				{
					twice.Invoke(3);
				}
				watch.Stop();
				UnityEngine.Debug.LogFormat("Invoke: {0}ms", watch.Elapsed.TotalMilliseconds);

				watch = Stopwatch.StartNew();
				for (int i = 0; i < LOOP; i++)
				{
					twice.DynamicInvoke(3);
				}
				watch.Stop();
				UnityEngine.Debug.LogFormat("DynamicInvoke: {0}ms", watch.Elapsed.TotalMilliseconds);
			}
		}
		*/

		#endregion
	}

}
