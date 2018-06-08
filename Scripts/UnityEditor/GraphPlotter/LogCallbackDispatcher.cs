// ============================================================================
//   Monitor Components v. 1.04 - written by Peter Bruun (twitter.com/ptrbrn)
//   More info on Asset Store: http://u3d.as/9MW
// ============================================================================

using UnityEngine;
using System.Collections.Generic;

namespace MonitorComponents 
{
	public class LogCallbackDispatcher 
	{
		private static LogCallbackDispatcher instance = null;

		public delegate void LogCallback(string logString, string stackTrace, LogType type);

		private HashSet<LogCallback> callbacks = new HashSet<LogCallback>();

		public void Add(LogCallback callback)
		{
			if (callbacks.Count == 0)
			{
#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6
				Application.RegisterLogCallback(HandleLog);
#else
				Application.logMessageReceived += HandleLog;
#endif
			}

			callbacks.Add(callback);
		}

		public void Remove(LogCallback callback)
		{
			callbacks.Remove(callback);

			if (callbacks.Count == 0)
			{
#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6
				Application.RegisterLogCallback(null);
#else
				Application.logMessageReceived -= HandleLog;
#endif
			}
		}

		private void HandleLog(string logString, string stackTrace, LogType type) 
		{
			foreach(var callback in callbacks)
			{
				callback(logString, stackTrace, type);
			}
		}

		public static LogCallbackDispatcher Instance
		{
			get 
			{
				if (instance == null)
				{
					instance = new LogCallbackDispatcher();
				}

				return instance;
			}
		}
	}
}