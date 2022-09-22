//#define DisableInfoLogging | Note that this should be defined project wide since LogRep also depends on it.

using System.Diagnostics;
using System.Reflection;
using Extenity.DataToolbox;
using Extenity.DebugToolbox;
using Exception = System.Exception;
using ArgumentOutOfRangeException = System.ArgumentOutOfRangeException;
using IDisposable = System.IDisposable;

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

	// TODO: Investigate: Find a way to pipe Unity logs through this class. So that Prefix system works even on Debug.Log_ calls that pass Context object.
	// TODO: Investigate: Find a way to hide wrapper methods like Info, Warning, etc. from Unity's console stacktrace and make it go to caller's line on double clicking over the log entry.

	public static class Log
	{
		#region Indentation

		public static string IndentationOneLevelString = "    ";

		private static int _Indentation;

		public static int Indentation
		{
			get { return _Indentation < 0 ? 0 : _Indentation; }
			private set
			{
				_Indentation = value;
				CurrentIndentationString = IndentationOneLevelString.Repeat(Indentation);
			}
		}

		private static string CurrentIndentationString;

		public static void IncreaseIndent()
		{
			Indentation++;
		}

		public static void DecreaseIndent()
		{
			Indentation--;
		}

		#endregion

		#region Indentation Using 'Using'

		public class IndentationHandler : IDisposable
		{
			private ContextObject Context;
			private string EndText;

			internal IndentationHandler(ContextObject context = null, string endText = null)
			{
				Context = context;
				EndText = endText;
				IncreaseIndent();
			}

			public void Dispose()
			{
				DecreaseIndent();
				if (!string.IsNullOrEmpty(EndText))
				{
					if (Context == null)
						Info(EndText);
					else
						Info(EndText, Context);
				}
			}
		}

		public static IDisposable Indent()
		{
			return new IndentationHandler();
		}

		public static IDisposable Indent(string startText, string endText = null)
		{
			if (!string.IsNullOrEmpty(startText))
			{
				Info(startText);
			}
			return new IndentationHandler(null, endText);
		}

		public static IDisposable Indent(ContextObject context, string startText, string endText = null)
		{
			if (!string.IsNullOrEmpty(startText))
			{
				Info(startText, context);
			}
			return new IndentationHandler(context, endText);
		}

		#endregion

		#region Create Message

		public static string CreateMessage(string message)
		{
			if (message == null)
				return CurrentIndentationString + "[NullStr]";
			else
				return CurrentIndentationString + message.NormalizeLineEndingsCRLF();
		}

		// Prefix operations are done in LogRep. Keep these codes here commented out for future needs.
		// public static string CreateMessageWithPrefix(string message, string processedPrefix)
		// {
		// 	if (message == null)
		// 		return CurrentIndentationString + processedPrefix + "[NullStr]";
		// 	else
		// 		return CurrentIndentationString + processedPrefix + message.NormalizeLineEndingsCRLF();
		// }

		public static string CreateShallowExceptionMessage(Exception exception)
		{
			if (exception == null)
				return CurrentIndentationString + "[NullExc]";
			else
				return CurrentIndentationString + InternalCreateShallowExceptionMessage(exception).NormalizeLineEndingsCRLF();
		}

		public static string CreateShallowExceptionMessage(Exception exception, string processedPrefix)
		{
			if (exception == null)
				return CurrentIndentationString + processedPrefix + "[NullExc]";
			else
				return CurrentIndentationString + processedPrefix + InternalCreateShallowExceptionMessage(exception).NormalizeLineEndingsCRLF();
		}

		public static string CreateDetailedExceptionMessage(Exception exception)
		{
			if (exception == null)
				return CurrentIndentationString + "[NullExc]";
			else
				return CurrentIndentationString + InternalCreateDetailedExceptionMessage(exception).NormalizeLineEndingsCRLF();
		}

		public static string CreateDetailedExceptionMessage(Exception exception, string processedPrefix)
		{
			if (exception == null)
				return CurrentIndentationString + processedPrefix + "[NullExc]";
			else
				return CurrentIndentationString + processedPrefix + InternalCreateDetailedExceptionMessage(exception).NormalizeLineEndingsCRLF();
		}

		private static string InternalCreateShallowExceptionMessage(Exception exception)
		{
			var message = exception.Message;
			return message;
		}

		private static string InternalCreateDetailedExceptionMessage(Exception exception)
		{
			var message = exception.ToString();
			message += "\r\nInnerException: " + exception.InnerException;
			message += "\r\nMessage: " + exception.Message;
			message += "\r\nSource: " + exception.Source;
			message += "\r\nStackTrace: " + exception.StackTrace;
			message += "\r\nTargetSite: " + exception.TargetSite;
			return message;
		}

		#endregion

		#region Log

		[DebuggerHidden]
		public static void Any(string message, LogCategory category)
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
		public static void Any(string message, LogCategory category, ContextObject context)
		{
			switch (category)
			{
				// @formatter:off
				case LogCategory.Verbose:  Verbose(message, context);       break;
				case LogCategory.Info:     Info(message, context);          break;
				case LogCategory.Warning:  Warning(message, context);       break;
				case LogCategory.Error:    Error(message, context);         break;
				case LogCategory.Critical: CriticalError(message, context); break;
				// @formatter:on
				default:
					throw new ArgumentOutOfRangeException(nameof(category), category, null);
			}
		}

		[Conditional("EnableVerboseLogging")]
		[DebuggerHidden]
		public static void Verbose(string message)
		{
#if UNITY
			UnityEngine.Debug.Log(CreateMessage(message)); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateMessage(message));
#endif
		}

		[Conditional("EnableVerboseLogging")]
		[DebuggerHidden]
		public static void Verbose(string message, ContextObject context)
		{
#if UNITY
			UnityEngine.Debug.Log(CreateMessage(message), context); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateMessage(message));
#endif
		}

#if DisableInfoLogging
		[Conditional("DummyConditionThatNeverExists")]
#endif
		[DebuggerHidden]
		public static void Info(string message)
		{
#if UNITY
			UnityEngine.Debug.Log(CreateMessage(message)); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateMessage(message));
#endif
		}

#if DisableInfoLogging
		[Conditional("DummyConditionThatNeverExists")]
#endif
		[DebuggerHidden]
		public static void Info(string message, ContextObject context)
		{
#if UNITY
			UnityEngine.Debug.Log(CreateMessage(message), context); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateMessage(message));
#endif
		}

		[DebuggerHidden]
		public static void Severe(string message, SeverityCategory severity)
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
		public static void Severe(string message, SeverityCategory severity, ContextObject context)
		{
			switch (severity)
			{
				// @formatter:off
				case SeverityCategory.Warning:  Warning(message, context);       break;
				case SeverityCategory.Error:    Error(message, context);         break;
				case SeverityCategory.Critical: CriticalError(message, context); break;
				// @formatter:on
				default:
					throw new ArgumentOutOfRangeException(nameof(severity), severity, null);
			}
		}

		[DebuggerHidden]
		public static void Warning(string message)
		{
#if UNITY
			UnityEngine.Debug.LogWarning(CreateMessage(message)); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateMessage(message));
#endif
		}

		[DebuggerHidden]
		public static void Warning(string message, ContextObject context)
		{
#if UNITY
			UnityEngine.Debug.LogWarning(CreateMessage(message), context); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateMessage(message));
#endif
		}

		[DebuggerHidden]
		public static void Error(string message)
		{
#if UNITY
			UnityEngine.Debug.LogError(CreateMessage(message)); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateMessage(message));
#endif
		}

		[DebuggerHidden]
		public static void Error(string message, ContextObject context)
		{
#if UNITY
			UnityEngine.Debug.LogError(CreateMessage(message), context); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateMessage(message));
#endif
		}

		/// <summary>
		/// Sends error message to Unity Cloud Diagnostics tool without breaking the code flow by throwing an exception.
		/// </summary>
		[DebuggerHidden]
		public static void CriticalError(string message)
		{
#if UNITY
			UnityEngine.Debug.LogException(new Exception(message)); // Ignored by Code Correct
#else
			System.Console.WriteLine(new Exception(message).ToString());
#endif
		}

		/// <summary>
		/// Sends error message to Unity Cloud Diagnostics tool without breaking the code flow by throwing an exception.
		/// </summary>
		[DebuggerHidden]
		public static void CriticalError(string message, Exception innerException)
		{
#if UNITY
			UnityEngine.Debug.LogException(new Exception(message, innerException)); // Ignored by Code Correct
#else
			System.Console.WriteLine(new Exception(message, innerException).ToString());
#endif
		}

		/// <summary>
		/// Sends error message to Unity Cloud Diagnostics tool without breaking the code flow by throwing an exception.
		/// </summary>
		[DebuggerHidden]
		public static void CriticalError(string message, ContextObject context)
		{
#if UNITY
			UnityEngine.Debug.LogException(new Exception(message), context); // Ignored by Code Correct
#else
			System.Console.WriteLine(new Exception(message).ToString());
#endif
		}

		/// <summary>
		/// Sends error message to Unity Cloud Diagnostics tool without breaking the code flow by throwing an exception.
		/// </summary>
		[DebuggerHidden]
		public static void CriticalError(string message, ContextObject context, Exception innerException)
		{
#if UNITY
			UnityEngine.Debug.LogException(new Exception(message, innerException), context); // Ignored by Code Correct
#else
			System.Console.WriteLine(new Exception(message, innerException).ToString());
#endif
		}

		/// <summary>
		/// Internal errors are logged just like critical errors. They will appear in Unity Cloud Diagnostics without breaking the code flow by throwing an exception.
		///
		/// See also 'InternalException'.
		/// </summary>
		[DebuggerHidden]
		public static void InternalError(int errorCode)
		{
#if UNITY
			UnityEngine.Debug.LogException(new InternalException(errorCode)); // Ignored by Code Correct
#else
			System.Console.WriteLine(new InternalException(errorCode).ToString());
#endif
		}

		/// <summary>
		/// Internal errors are logged just like critical errors. They will appear in Unity Cloud Diagnostics without breaking the code flow by throwing an exception.
		///
		/// See also 'InternalException'.
		/// </summary>
		[DebuggerHidden]
		public static void InternalError(int errorCode, ContextObject context)
		{
#if UNITY
			UnityEngine.Debug.LogException(new InternalException(errorCode), context); // Ignored by Code Correct
#else
			System.Console.WriteLine(new InternalException(errorCode).ToString());
#endif
		}

		[DebuggerHidden]
		public static void Exception(Exception exception)
		{
#if UNITY
			UnityEngine.Debug.LogException(exception); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateDetailedExceptionMessage(exception));
#endif
		}

		[DebuggerHidden]
		public static void Exception(Exception exception, ContextObject context)
		{
#if UNITY
			UnityEngine.Debug.LogException(exception, context); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateDetailedExceptionMessage(exception));
#endif
		}

		[DebuggerHidden]
		public static void Exception(Exception exception, string processedPrefix)
		{
#if UNITY
			UnityEngine.Debug.LogException(exception); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateDetailedExceptionMessage(exception, processedPrefix));
#endif
		}

		[DebuggerHidden]
		public static void Exception(Exception exception, string processedPrefix, ContextObject context)
		{
#if UNITY
			UnityEngine.Debug.LogException(exception, context); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateDetailedExceptionMessage(exception, processedPrefix));
#endif
		}

		[DebuggerHidden]
		public static void ExceptionAsError(Exception exception)
		{
#if UNITY
			UnityEngine.Debug.LogError(CreateShallowExceptionMessage(exception)); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateShallowExceptionMessage(exception));
#endif
		}

		[DebuggerHidden]
		public static void ExceptionAsError(Exception exception, ContextObject context)
		{
#if UNITY
			UnityEngine.Debug.LogError(CreateShallowExceptionMessage(exception), context); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateShallowExceptionMessage(exception));
#endif
		}

		[DebuggerHidden]
		public static void ExceptionAsError(Exception exception, string processedPrefix)
		{
#if UNITY
			UnityEngine.Debug.LogError(CreateShallowExceptionMessage(exception, processedPrefix)); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateShallowExceptionMessage(exception, processedPrefix));
#endif
		}

		[DebuggerHidden]
		public static void ExceptionAsError(Exception exception, string processedPrefix, ContextObject context)
		{
#if UNITY
			UnityEngine.Debug.LogError(CreateShallowExceptionMessage(exception, processedPrefix), context); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateShallowExceptionMessage(exception, processedPrefix));
#endif
		}

		[DebuggerHidden]
		public static void ExceptionAsErrorDetailed(this Exception exception)
		{
#if UNITY
			UnityEngine.Debug.LogError(CreateDetailedExceptionMessage(exception)); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateDetailedExceptionMessage(exception));
#endif
		}

		[DebuggerHidden]
		public static void ExceptionAsErrorDetailed(this Exception exception, ContextObject context)
		{
#if UNITY
			UnityEngine.Debug.LogError(CreateDetailedExceptionMessage(exception), context); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateDetailedExceptionMessage(exception));
#endif
		}

		[DebuggerHidden]
		public static void ExceptionAsErrorDetailed(this Exception exception, string processedPrefix)
		{
#if UNITY
			UnityEngine.Debug.LogError(CreateDetailedExceptionMessage(exception, processedPrefix)); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateDetailedExceptionMessage(exception, processedPrefix));
#endif
		}

		[DebuggerHidden]
		public static void ExceptionAsErrorDetailed(this Exception exception, string processedPrefix, ContextObject context)
		{
#if UNITY
			UnityEngine.Debug.LogError(CreateDetailedExceptionMessage(exception, processedPrefix), context); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateDetailedExceptionMessage(exception, processedPrefix));
#endif
		}

		#endregion

		#region Debug Log

#if EnableVerboseLogging
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] // This translates to: EnableVerboseLogging && (UNITY_EDITOR || DEBUG)
#else
		[Conditional("DummyConditionThatNeverExists")]
#endif
		[DebuggerHidden]
		public static void DebugVerbose(string message)
		{
#if UNITY
			UnityEngine.Debug.Log(CreateMessage(message)); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateMessage(message));
#endif
		}

#if EnableVerboseLogging
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")] // This translates to: EnableVerboseLogging && (UNITY_EDITOR || DEBUG)
#else
		[Conditional("DummyConditionThatNeverExists")]
#endif
		[DebuggerHidden]
		public static void DebugVerbose(string message, ContextObject context)
		{
#if UNITY
			UnityEngine.Debug.Log(CreateMessage(message), context); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateMessage(message));
#endif
		}

#if DisableInfoLogging
		[Conditional("DummyConditionThatNeverExists")]
#else
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
#endif
		[DebuggerHidden]
		public static void DebugInfo(string message)
		{
#if UNITY
			UnityEngine.Debug.Log(CreateMessage(message)); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateMessage(message));
#endif
		}

#if DisableInfoLogging
		[Conditional("DummyConditionThatNeverExists")]
#else
		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
#endif
		[DebuggerHidden]
		public static void DebugInfo(string message, ContextObject context)
		{
#if UNITY
			UnityEngine.Debug.Log(CreateMessage(message), context); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateMessage(message));
#endif
		}

		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
		[DebuggerHidden]
		public static void DebugWarning(string message)
		{
#if UNITY
			UnityEngine.Debug.LogWarning(CreateMessage(message)); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateMessage(message));
#endif
		}

		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
		[DebuggerHidden]
		public static void DebugWarning(string message, ContextObject context)
		{
#if UNITY
			UnityEngine.Debug.LogWarning(CreateMessage(message), context); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateMessage(message));
#endif
		}

		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
		[DebuggerHidden]
		public static void DebugError(string message)
		{
#if UNITY
			UnityEngine.Debug.LogError(CreateMessage(message)); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateMessage(message));
#endif
		}

		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
		[DebuggerHidden]
		public static void DebugError(string message, ContextObject context)
		{
#if UNITY
			UnityEngine.Debug.LogError(CreateMessage(message), context); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateMessage(message));
#endif
		}

		#endregion

		#region Internal Error Message

		public static string BuildInternalErrorMessage(int errorCode)
		{
			return $"Internal error {errorCode}!";
		}

		#endregion

		#region Log Tools - Methods

		[DebuggerHidden]
		public static void CurrentMethodNotImplemented()
		{
			CriticalError("Method '" + DebugReflection.PreviousMethodNameWithType + "' is not implemented!");
		}

		[DebuggerHidden]
		public static void CurrentMethod(string additionalText = null, LogCategory category = LogCategory.Info)
		{
			Any(string.IsNullOrEmpty(additionalText)
				    ? DebugReflection.PreviousMethodNameWithType
				    : DebugReflection.PreviousMethodNameWithType + " : " + additionalText,
			    category);
		}

		[DebuggerHidden]
		public static void PreviousMethod(string additionalText = null, LogCategory category = LogCategory.Info)
		{
			Any(string.IsNullOrEmpty(additionalText)
				    ? DebugReflection.PrePreviousMethodNameWithType
				    : DebugReflection.PrePreviousMethodNameWithType + " : " + additionalText,
			    category);
		}

		#endregion

		#region Log Tools - Stack Trace

		public static void StackTrace(string headerMessage, ContextObject context = null)
		{
			using (Indent(context, headerMessage))
			{
				var frames = new StackTrace(1).GetFrames();

				for (int i = 0; i < frames.Length; i++)
				{
					MethodBase method = frames[i].GetMethod();

					var reflectedTypeName = method.ReflectedType != null ? method.ReflectedType.Name : string.Empty;
					Info(reflectedTypeName + "::" + method.Name, context);
				}
			}
		}

		#endregion
	}

}
