using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Extenity.DebugToolbox
{

	public readonly struct LogRep
	{
		#region Setup

		public readonly string Prefix;
		public readonly Object Context;

		#endregion

		#region Initialization

		public LogRep(string prefix, Object context = null)
		{
			Prefix = prefix;
			Context = context;
		}

		public static LogRep CreateStandardPrefix(string prefix, Object context = null)
		{
			return new LogRep($"<b>[{prefix}]</b> ", context);
		}

		#endregion

		#region Log

		/// <remarks>
		/// Creating the log message might require some allocations for formatted messages. To avoid the overhead of
		/// creating the log message, the caller should implement a check if verbose logging is active.
		/// </remarks>
		// [Conditional("EnableVerboseLogging")] Nope! Note that LogRep uses its own configuration that is applied separately to each LogRep.
		public readonly void Verbose(string message)
		{
			// if (VerboseLoggingActive) Nope! Should be done by the caller in a way that prevents message string creation overhead.
			Debug.Log(Prefix + message, Context); // Ignored by Code Correct
		}

		public readonly void Info(string message)
		{
			Debug.Log(Prefix + message, Context); // Ignored by Code Correct
		}

		public readonly void Warning(string message)
		{
			Debug.LogWarning(Prefix + message, Context); // Ignored by Code Correct
		}

		public readonly void Error(string message)
		{
			Debug.LogError(Prefix + message, Context); // Ignored by Code Correct
		}

		public readonly void Critical(string message)
		{
			Debug.LogException(new Exception(Prefix + message), Context); // Ignored by Code Correct
		}

		public readonly void Exception(Exception exception)
		{
			Debug.LogException(exception, Context); // Ignored by Code Correct
		}

		#endregion
	}

}
