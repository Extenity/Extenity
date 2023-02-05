using System;
using System.Diagnostics;
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

		public readonly string RawPrefix;
		public readonly string ProcessedPrefix;
		public readonly ContextObject DefaultContext;

		#endregion

		#region Initialization

		public LogRep(string prefix, ContextObject context = default)
		{
			RawPrefix = prefix;
			ProcessedPrefix = $"<b>[{prefix}]</b> ";
			DefaultContext = context;
		}

		public static void SetContext(ref LogRep logRep, ContextObject context)
		{
			logRep = new LogRep(logRep.RawPrefix, context);
		}

		#endregion

		#region Log

		[DebuggerHidden]
		public void Any(string message, LogCategory category)
		{
			switch (category)
			{
				// @formatter:off
				case LogCategory.Verbose:  Verbose(message);       break;
				case LogCategory.Info:     Info(message);          break;
				case LogCategory.Warning:  Warning(message);       break;
				case LogCategory.Error:    Error(message);         break;
				case LogCategory.Critical: CriticalError(message); break;
				// @formatter:on
				default:
					throw new ArgumentOutOfRangeException(nameof(category), category, null);
			}
		}

		[DebuggerHidden]
		public void Any(string message, LogCategory category, ContextObject overriddenContext)
		{
			switch (category)
			{
				// @formatter:off
				case LogCategory.Verbose:  Verbose(message, overriddenContext);       break;
				case LogCategory.Info:     Info(message, overriddenContext);          break;
				case LogCategory.Warning:  Warning(message, overriddenContext);       break;
				case LogCategory.Error:    Error(message, overriddenContext);         break;
				case LogCategory.Critical: CriticalError(message, overriddenContext); break;
				// @formatter:on
				default:
					throw new ArgumentOutOfRangeException(nameof(category), category, null);
			}
		}

		[Conditional("EnableVerboseLogging")]
		[DebuggerHidden]
		public void Verbose(string message)
		{
			Log.Verbose(ProcessedPrefix + message, DefaultContext);
		}

		[Conditional("EnableVerboseLogging")]
		[DebuggerHidden]
		public void Verbose(string message, ContextObject overriddenContext)
		{
			Log.Verbose(ProcessedPrefix + message, overriddenContext);
		}

#if DisableInfoLogging
		[Conditional("DummyConditionThatNeverExists")]
#endif
		[DebuggerHidden]
		public void Info(string message)
		{
			Log.Info(ProcessedPrefix + message, DefaultContext);
		}

#if DisableInfoLogging
		[Conditional("DummyConditionThatNeverExists")]
#endif
		[DebuggerHidden]
		public void Info(string message, ContextObject overriddenContext)
		{
			Log.Info(ProcessedPrefix + message, overriddenContext);
		}

		[DebuggerHidden]
		public void Severe(string message, SeverityCategory severity)
		{
			switch (severity)
			{
				// @formatter:off
				case SeverityCategory.Warning:  Warning(message);       break;
				case SeverityCategory.Error:    Error(message);         break;
				case SeverityCategory.Critical: CriticalError(message); break;
				// @formatter:on
				default:
					throw new ArgumentOutOfRangeException(nameof(severity), severity, null);
			}
		}

		[DebuggerHidden]
		public void Severe(string message, SeverityCategory severity, ContextObject overriddenContext)
		{
			switch (severity)
			{
				// @formatter:off
				case SeverityCategory.Warning:  Warning(message, overriddenContext);       break;
				case SeverityCategory.Error:    Error(message, overriddenContext);         break;
				case SeverityCategory.Critical: CriticalError(message, overriddenContext); break;
				// @formatter:on
				default:
					throw new ArgumentOutOfRangeException(nameof(severity), severity, null);
			}
		}

		[DebuggerHidden]
		public void Warning(string message)
		{
			Log.Warning(ProcessedPrefix + message, DefaultContext);
		}

		[DebuggerHidden]
		public void Warning(string message, ContextObject overriddenContext)
		{
			Log.Warning(ProcessedPrefix + message, overriddenContext);
		}

		[DebuggerHidden]
		public void Error(string message)
		{
			Log.Error(ProcessedPrefix + message, DefaultContext);
		}

		[DebuggerHidden]
		public void Error(string message, ContextObject overriddenContext)
		{
			Log.Error(ProcessedPrefix + message, overriddenContext);
		}

		/// <summary>
		/// Sends error message to Unity Cloud Diagnostics tool without breaking the code flow by throwing an exception.
		/// </summary>
		[DebuggerHidden]
		public void CriticalError(string message)
		{
			Log.CriticalError(ProcessedPrefix + message, DefaultContext);
		}

		/// <summary>
		/// Sends error message to Unity Cloud Diagnostics tool without breaking the code flow by throwing an exception.
		/// </summary>
		[DebuggerHidden]
		public void CriticalError(string message, Exception innerException)
		{
			Log.CriticalError(ProcessedPrefix + message, DefaultContext, innerException);
		}

		/// <summary>
		/// Sends error message to Unity Cloud Diagnostics tool without breaking the code flow by throwing an exception.
		/// </summary>
		[DebuggerHidden]
		public void CriticalError(string message, ContextObject overriddenContext)
		{
			Log.CriticalError(ProcessedPrefix + message, overriddenContext);
		}

		/// <summary>
		/// Sends error message to Unity Cloud Diagnostics tool without breaking the code flow by throwing an exception.
		/// </summary>
		[DebuggerHidden]
		public void CriticalError(string message, ContextObject overriddenContext, Exception innerException)
		{
			Log.CriticalError(ProcessedPrefix + message, overriddenContext, innerException);
		}

		/// <summary>
		/// Internal errors are logged just like critical errors. They will appear in Unity Cloud Diagnostics without breaking the code flow by throwing an exception.
		///
		/// See also 'InternalException'.
		/// </summary>
		[DebuggerHidden]
		public void InternalError(int errorCode)
		{
			Log.InternalError(errorCode, DefaultContext);
		}

		/// <summary>
		/// Internal errors are logged just like critical errors. They will appear in Unity Cloud Diagnostics without breaking the code flow by throwing an exception.
		///
		/// See also 'InternalException'.
		/// </summary>
		[DebuggerHidden]
		public void InternalError(int errorCode, ContextObject overriddenContext)
		{
			Log.InternalError(errorCode, overriddenContext);
		}

		[DebuggerHidden]
		public void Exception(Exception exception)
		{
			Log.Exception(exception, ProcessedPrefix, DefaultContext);
		}

		[DebuggerHidden]
		public void Exception(Exception exception, ContextObject overriddenContext)
		{
			Log.Exception(exception, ProcessedPrefix, overriddenContext);
		}

		[DebuggerHidden]
		public void ExceptionAsError(Exception exception)
		{
			Log.ExceptionAsError(exception, ProcessedPrefix, DefaultContext);
		}

		[DebuggerHidden]
		public void ExceptionAsError(Exception exception, ContextObject overriddenContext)
		{
			Log.ExceptionAsError(exception, ProcessedPrefix, overriddenContext);
		}

		[DebuggerHidden]
		public void ExceptionAsErrorDetailed(Exception exception)
		{
			Log.ExceptionAsErrorDetailed(exception, ProcessedPrefix, DefaultContext);
		}

		[DebuggerHidden]
		public void ExceptionAsErrorDetailed(Exception exception, ContextObject overriddenContext)
		{
			Log.ExceptionAsErrorDetailed(exception, ProcessedPrefix, overriddenContext);
		}

		#endregion
	}

#if UNITY
	public static class LogRepTools
	{
		public static void SetAsLogContext(this UnityEngine.GameObject go, ref LogRep logRep)
		{
			LogRep.SetContext(ref logRep, go);
		}
	}
#endif

}
