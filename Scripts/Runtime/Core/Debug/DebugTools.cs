#if UNITY

using System.Diagnostics;
using System.Reflection;
using Extenity.ReflectionToolbox;

namespace Extenity.DebugToolbox
{

	public static class DebugTools
	{
		#region Sticky Logs

		[DebuggerHidden]
		public static void LogSticky(int identifier,
		                             UnityEngine.LogType logType,
		                             UnityEngine.LogOption logOptions,
		                             string message,
		                             UnityEngine.Object context = null)
		{
			InitializeStickyLogsIfRequired();
			_LogSticky(identifier, logType, logOptions, message, context);
		}

		[DebuggerHidden]
		public static void RemoveLogEntriesByIdentifier(int identifier)
		{
			InitializeStickyLogsIfRequired();
			_RemoveLogEntriesByIdentifier(identifier);
		}

		#endregion

		#region Sticky Logs - Get Unity Internal Methods

		private static System.Action<int, UnityEngine.LogType, UnityEngine.LogOption, string, UnityEngine.Object> _LogSticky;
		private static System.Action<int> _RemoveLogEntriesByIdentifier;

		private static void InitializeStickyLogsIfRequired()
		{
			if (_LogSticky != null)
				return; // Already initialized.

			// internal static extern void LogSticky(
			// 	int identifier,
			// 	LogType logType,
			// 	LogOption logOptions,
			// 	string message,
			// 	Object context = null);
			{
				var method = typeof(UnityEngine.Debug).GetMethod("LogSticky", BindingFlags.Static | BindingFlags.NonPublic);
				_LogSticky = method.GenerateStaticMethodWith5Parameters<int, UnityEngine.LogType, UnityEngine.LogOption, string, UnityEngine.Object>();
			}

			// internal static extern void RemoveLogEntriesByIdentifier(int identifier)
			{
				var method = typeof(UnityEngine.Debug).GetMethod("RemoveLogEntriesByIdentifier", BindingFlags.Static | BindingFlags.NonPublic);
				_RemoveLogEntriesByIdentifier = method.GenerateStaticMethodWith1Parameter<int>();
			}
		}

		#endregion
	}

}

#endif
