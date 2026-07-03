using System;
using System.Collections.Generic;
using UnityEngine;

namespace Extenity.Testing
{
	/// <summary>
	/// Captures every log emitted while the scope is alive by swapping <see cref="Debug.unityLogger"/>'s handler
	/// for a <see cref="UnitTestLogHandlerOverride"/>. The original handler is restored on <see cref="Dispose"/>.
	///
	/// Safe because Unity Test Framework runs tests sequentially on the main thread. Only one scope is ever active.
	/// </summary>
	internal sealed class LogCaptureScope : IDisposable
	{
		private readonly ILogHandler PreviousLogHandler;
		private bool IsDisposed;

		public List<(LogType Type, string Message)> Logs { get; }

		public LogCaptureScope(Func<LogType, string, bool> shouldRecord)
		{
			Logs = new List<(LogType, string)>();
			PreviousLogHandler = Debug.unityLogger.logHandler;
			Debug.unityLogger.logHandler = new UnitTestLogHandlerOverride(Logs, shouldRecord);
		}

		public void Dispose()
		{
			if (!IsDisposed)
			{
				IsDisposed = true;
				Debug.unityLogger.logHandler = PreviousLogHandler;
			}
		}

		/// <summary>
		/// Mirrors <c>ExtenityTestBase.MarkLogsAsExpectedThatIncludes</c>.
		/// </summary>
		public int MarkLogsAsExpectedThatIncludes(string includedText)
		{
			var count = 0;
			for (var i = 0; i < Logs.Count; i++)
			{
				if (Logs[i].Message.Contains(includedText, StringComparison.Ordinal))
				{
					Logs.RemoveAt(i);
					i--;
					count++;
				}
			}

			return count;
		}
	}
}