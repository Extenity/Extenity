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

namespace Extenity
{

	public readonly struct Logger
	{
		#region Setup

		public readonly string Category;
		public readonly ContextObject Context;

		#endregion

		#region Initialization

		public Logger(string category, ContextObject context = default)
		{
			Category = category;
			Context = context;
		}

		public static void SetContext(ref Logger logger, ContextObject context)
		{
			logger = new Logger(logger.Category, context);
		}

		#endregion

		#region Indent

		public Log.IndentationHandler IndentedScope => Log._IndentedScope;

		#endregion

		#region Log

		[DebuggerHidden]
		public void Any(LogSeverity severity, string message)
		{
			switch (severity)
			{
				// @formatter:off
				case LogSeverity.Verbose:  Log._Verbose(Category, Context, message);  break;
				case LogSeverity.Info   :  Log._Info   (Category, Context, message);  break;
				case LogSeverity.Warning:  Log._Warning(Category, Context, message);  break;
				case LogSeverity.Error  :  Log._Error  (Category, Context, message);  break;
				case LogSeverity.Fatal  :  Log._Fatal  (Category, Context, message);  break;
				// @formatter:on
				default:
					throw new ArgumentOutOfRangeException(nameof(severity), severity, null);
			}
		}

		[DebuggerHidden]
		public void AnyWithContext(ContextObject overriddenContext, LogSeverity severity, string message)
		{
			switch (severity)
			{
				// @formatter:off
				case LogSeverity.Verbose:  Log._Verbose(Category, overriddenContext, message);  break;
				case LogSeverity.Info   :  Log._Info   (Category, overriddenContext, message);  break;
				case LogSeverity.Warning:  Log._Warning(Category, overriddenContext, message);  break;
				case LogSeverity.Error  :  Log._Error  (Category, overriddenContext, message);  break;
				case LogSeverity.Fatal  :  Log._Fatal  (Category, overriddenContext, message);  break;
				// @formatter:on
				default:
					throw new ArgumentOutOfRangeException(nameof(severity), severity, null);
			}
		}

#if DisableVerboseLogging
		[Conditional("DummyConditionThatNeverExists")]
#endif
		[DebuggerHidden]
		public void Verbose(string message)
		{
			Log._Verbose(Category, Context, message);
		}

#if DisableVerboseLogging
		[Conditional("DummyConditionThatNeverExists")]
#endif
		[DebuggerHidden]
		public void VerboseWithContext(ContextObject overriddenContext, string message)
		{
			Log._Verbose(Category, overriddenContext, message);
		}

#if DisableInfoLogging
		[Conditional("DummyConditionThatNeverExists")]
#endif
		[DebuggerHidden]
		public void Info(string message)
		{
			Log._Info(Category, Context, message);
		}

#if DisableInfoLogging
		[Conditional("DummyConditionThatNeverExists")]
#endif
		[DebuggerHidden]
		public void InfoWithContext(ContextObject overriddenContext, string message)
		{
			Log._Info(Category, overriddenContext, message);
		}

		[DebuggerHidden]
		public void Warning(string message)
		{
			Log._Warning(Category, Context, message);
		}

		[DebuggerHidden]
		public void WarningWithContext(ContextObject overriddenContext, string message)
		{
			Log._Warning(Category, overriddenContext, message);
		}

		[DebuggerHidden]
		public void Error(string message)
		{
			Log._Error(Category, Context, message);
		}

		[DebuggerHidden]
		public void Error(Exception exception)
		{
			Log._Error(Category, Context, exception);
		}

		[DebuggerHidden]
		public void ErrorWithContext(ContextObject overriddenContext, string message)
		{
			Log._Error(Category, overriddenContext, message);
		}

		[DebuggerHidden]
		public void ErrorWithContext(ContextObject overriddenContext, Exception exception)
		{
			Log._Error(Category, overriddenContext, exception);
		}

		/// <summary>
		/// Sends error message to Unity Cloud Diagnostics tool without breaking the code flow by throwing an exception.
		/// </summary>
		[DebuggerHidden]
		public void Fatal(string message)
		{
			Log._Fatal(Category, Context, message);
		}

		/// <summary>
		/// Sends error message to Unity Cloud Diagnostics tool without breaking the code flow by throwing an exception.
		/// </summary>
		[DebuggerHidden]
		public void Fatal(Exception exception)
		{
			Log._Fatal(Category, Context, exception);
		}

		/// <summary>
		/// Sends error message to Unity Cloud Diagnostics tool without breaking the code flow by throwing an exception.
		/// </summary>
		[DebuggerHidden]
		public void FatalWithContext(ContextObject overriddenContext, string message)
		{
			Log._Fatal(Category, overriddenContext, message);
		}

		/// <summary>
		/// Sends error message to Unity Cloud Diagnostics tool without breaking the code flow by throwing an exception.
		/// </summary>
		[DebuggerHidden]
		public void FatalWithContext(ContextObject overriddenContext, Exception exception)
		{
			Log._Fatal(Category, overriddenContext, exception);
		}

		/// <summary>
		/// Internal errors are logged just like Fatal errors. They will appear in Unity Cloud Diagnostics without breaking the code flow by throwing an exception.
		///
		/// See also 'InternalException'.
		/// </summary>
		[DebuggerHidden]
		public void InternalError(int errorCode)
		{
			Log._InternalError(Category, Context, errorCode);
		}

		/// <summary>
		/// Internal errors are logged just like Fatal errors. They will appear in Unity Cloud Diagnostics without breaking the code flow by throwing an exception.
		///
		/// See also 'InternalException'.
		/// </summary>
		[DebuggerHidden]
		public void InternalError(int errorCode, Exception innerException)
		{
			Log._InternalError(Category, Context, errorCode, innerException);
		}

		/// <summary>
		/// Internal errors are logged just like Fatal errors. They will appear in Unity Cloud Diagnostics without breaking the code flow by throwing an exception.
		///
		/// See also 'InternalException'.
		/// </summary>
		[DebuggerHidden]
		public void InternalErrorWithContext(ContextObject overriddenContext, int errorCode)
		{
			Log._InternalError(Category, overriddenContext, errorCode);
		}

		/// <summary>
		/// Internal errors are logged just like Fatal errors. They will appear in Unity Cloud Diagnostics without breaking the code flow by throwing an exception.
		///
		/// See also 'InternalException'.
		/// </summary>
		[DebuggerHidden]
		public void InternalErrorWithContext(ContextObject overriddenContext, int errorCode, Exception innerException)
		{
			Log._InternalError(Category, overriddenContext, errorCode, innerException);
		}

		#endregion
	}

#if UNITY
	public static class LoggerTools
	{
		public static void SetAsLogContext(this UnityEngine.GameObject go, ref Logger logger)
		{
			Logger.SetContext(ref logger, go);
		}
	}
#endif

}
