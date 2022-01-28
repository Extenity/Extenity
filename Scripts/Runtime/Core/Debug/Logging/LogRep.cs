using Exception = System.Exception;

// This is the way that Log system supports various Context types in different environments like
// both in Unity and in UniversalExtenity. Also don't add 'using UnityEngine' or 'using System'
// in this code file to prevent any possible confusions. Use 'using' selectively, like
// 'using Exception = System.Exception;'
// See 11746845.
#if UNITY
using ContextObject = UnityEngine.Object;
#else
using ContextObject = System.Object;
#endif

namespace Extenity.DebugToolbox
{

	public readonly struct LogRep
	{
		#region Setup

		public readonly string Prefix;
		public readonly ContextObject Context;

		#endregion

		#region Initialization

		public LogRep(string prefix, ContextObject context = null)
		{
			Prefix = prefix;
			Context = context;
		}

		public static LogRep CreateStandardPrefix(string prefix, ContextObject context = null)
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
			UnityEngine.Debug.Log(Prefix + message, Context); // Ignored by Code Correct
		}

		public readonly void Info(string message)
		{
			UnityEngine.Debug.Log(Prefix + message, Context); // Ignored by Code Correct
		}

		public readonly void Warning(string message)
		{
			UnityEngine.Debug.LogWarning(Prefix + message, Context); // Ignored by Code Correct
		}

		public readonly void Error(string message)
		{
			UnityEngine.Debug.LogError(Prefix + message, Context); // Ignored by Code Correct
		}

		public readonly void Critical(string message)
		{
			UnityEngine.Debug.LogException(new Exception(Prefix + message), Context); // Ignored by Code Correct
		}

		public readonly void Exception(Exception exception)
		{
			UnityEngine.Debug.LogException(exception, Context); // Ignored by Code Correct
		}

		#endregion
	}

}
