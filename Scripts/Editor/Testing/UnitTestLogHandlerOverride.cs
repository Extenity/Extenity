using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Extenity.Testing
{
	/// <summary>
	/// An <see cref="ILogHandler"/> that records every log routed through <see cref="Debug.unityLogger"/>.
	/// <c>Debug.Log*</c> funnel through this one handler.
	/// </summary>
	internal sealed class UnitTestLogHandlerOverride : ILogHandler
	{
		private readonly List<(LogType Type, string Message)> Logs;
		private readonly Func<LogType, string, bool> ShouldRecord;

		public UnitTestLogHandlerOverride(List<(LogType Type, string Message)> logs, Func<LogType, string, bool> shouldRecord)
		{
			Logs = logs ?? throw new ArgumentNullException();
			ShouldRecord = shouldRecord ?? throw new ArgumentNullException();
		}

		public void LogFormat(LogType logType, Object context, string format, params object[] args)
		{
			lock (Logs)
			{
				// Debug.unityLogger.Log(type, message) arrives here as format "{0}" + args[0] = message.
				// A literal format with no args (some direct LogFormat callers) must not go through string.Format.
				var message = (args != null && args.Length > 0) ? string.Format(format, args) : format;

				if (ShouldRecord(logType, message))
				{
					Logs.Add((logType, message));
				}
			}
		}

		public void LogException(Exception exception, Object context)
		{
			lock (Logs)
			{
				var message = exception?.Message ?? string.Empty;
				if (ShouldRecord(LogType.Exception, message))
				{
					Logs.Add((LogType.Exception, message));
				}
			}
		}
	}
}