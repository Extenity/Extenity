//#define DisableVerboseLogging | Note that this should be defined project wide since Logger also depends on it.
//#define DisableInfoLogging | Note that this should be defined project wide since Logger also depends on it.

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
	// TODO-Log: Make sure new log system provides these features:
	// Figure out how to know which lines a log entry has in log output file
	//  * Maybe use a special character at the beginning of each line?
	//    * Ensure no other lines would start with that character.
	//    * Maybe use RS (Record Separator) control character in ASCII?
	//      * Are text editors able to handle that?
	//      * Is Jenkins able to handle that?
	// Outputting to Jenkins console, without registering logs to Unity.
	//  * Instant flush of logs to Jenkins console.
	// Custom formatting for Stacktrace
	// Custom formatting for Exception that includes way more info.
	// Auto adding '' around parameters in log messages.
	// Add last 100 log entries (including Verbose) to Warnings and Errors.
	// Better stacktrace:
	//  * Exclude DebuggerHidden attributed methods from stacktrace.
	//  * Find a way to clearly differentiate stacktrace in log output streams
	// Log line format: [Time] [Category] [Severity] [Indentation] [Message]
	// Indentation support
	// Ensures all logs have Category by forcing logs to be made via Log.With or Logger.
	// Full path of context object (if UnityEngine.Object) will also be logged
	// Catch Unity's logs and log them to output streams
	// Multi-threaded logging
	//  * Log methods are locked to prevent any possible multi-threading issues
	// Custom actions for log entries
	//  * Go to context object
	//    * Which also supports logging full UnityEngine.Object path
	//    * If the object no longer exists, "Go to context object" tries to find the object in the scene via its path.
	//  * Simulate click on Unity's menu items
	//  * Open Game window
	//  * Open Scene window
	//  * Open Profiler window
	// In-game and Editor log window
	//  * Find a way to show log window in-game. Maybe use UIToolkit?
	//  * Think about how to implement something like Quantum Console
	//  * Clear button flushes all current logs and creates a new log file
	//  * Filtering
	//  * Metal Search
	//  * Double-click to go to source code
	//  * Hyperlinks on stacktrace lines to go to source code
	//  * Monospace font
	//  * New log entries appear 3 frames per second to increase readability of logs and editor performance
	//  * Background will turn red for errors and yellow for warnings
	//  * Opens stacktrace and log details as right pane of window, almost as a full screen window
	//    * Details window has a big Close button
	//    * Clicking on the log line again will deselect and close details window
	//    * Background will turn red for errors and yellow for warnings
	//  * Ability to copy log line and details to clipboard
	// Log files
	//  * Log files are stored in user folder
	//  * Log files are flushed to disk every 0.2 seconds
	//  * Log files are flushed immediately when quitting the application
	//  * Log flushing will be turned to immediate when quitting the application
	//  * Log file output name format can be modified
	//    * Default format: "Log_YYYY-MM-DD_HH-mm-ss.log"
	//  * Ability to send last 300 lines of active log file to a remote server when an error occurs
	// Viewing log files
	//  * Ability to select log files from a list
	//    * Local log files
	//    * Remote log files
	// Catching unhandled exceptions
	//  * Unhandled exceptions will be logged to Unity console
	//  * Unhandled exceptions will be logged to Jenkins console
	//  * Unhandled exceptions will be logged to log files
	//  * Unhandled exceptions will be logged to log window
	//  * Unhandled exceptions will be flushed to all output targets immediately

	// TODO-Log: High-throughput logging
	//  * Find a way to support concatenation of strings in log methods
	//    * Think about these:
	//        Log.Text("Navigation area ").String(areaName).Text(" does not exist.").Fatal();
	//        Log.Write("Navigation area ").String(areaName).Write(" does not exist.").Fatal();
	//        Log.Fatal("Navigation area ", areaName, " does not exist.");
	//        Log.Fatal("Navigation area {0} does not exist.", areaName);
	//        Log.Fatal($"Navigation area '{areaName}' does not exist.");
	//        Possible other options:
	//          Color, WriteLine
	//          
	//  * Find a way to write directly to Log System's Utf16ValueStringBuilder from caller's code
	//    * Maybe a property in Logger, that can be used like Log.Stream (Make sure it's multi-thread safe)
	//  * Think about if specifying context should be made via InfoWithContext or Log.With 
	//  * Find a way to list the log method usages where callers of log methods use their own formatting, instead of leaving it to log method.

	// TODO-Log: Make sure to do these after the new log system is ready to be used:
	//  * Change all logs that does formatting to use new log system.
	//  * Remove all Debug.Log_ calls from the codebase.
	//  * Figure out how to prevent Debug.Log_ calls from being made in the codebase.
	//  * Think about how the log system should clear old log files
	//  * Think about how the log system should handle log files when multiple instances of the application are running
	//  * Do performance and memory tests
	//  * Ensure Category names does not contain any spaces or special characters. Ensure max length is 30 characters.

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

		public struct IndentationHandler : IDisposable
		{
			private ContextObject Context;
			private string EndText;

			internal IndentationHandler(string endText = null, ContextObject context = default)
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
					Info(EndText, Context);
				}
			}
		}

		public static IndentationHandler Indent()
		{
			return new IndentationHandler();
		}

		public static IndentationHandler Indent(string startText, string endText = null, ContextObject context = default)
		{
			if (!string.IsNullOrEmpty(startText))
			{
				Info(startText, context);
			}
			return new IndentationHandler(endText, context);
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

		// Prefix operations are done in Logger. Keep these codes here commented out for future needs.
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
				case LogCategory.Verbose:  Verbose(message);  break;
				case LogCategory.Info:     Info(message);     break;
				case LogCategory.Warning:  Warning(message);  break;
				case LogCategory.Error:    Error(message);    break;
				case LogCategory.Fatal:    Fatal(message);    break;
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
				case LogCategory.Verbose:  Verbose(message, context);  break;
				case LogCategory.Info:     Info(message, context);     break;
				case LogCategory.Warning:  Warning(message, context);  break;
				case LogCategory.Error:    Error(message, context);    break;
				case LogCategory.Fatal:    Fatal(message, context);    break;
				// @formatter:on
				default:
					throw new ArgumentOutOfRangeException(nameof(category), category, null);
			}
		}

#if DisableVerboseLogging
		[Conditional("DummyConditionThatNeverExists")]
#endif
		[DebuggerHidden]
		public static void Verbose(string message)
		{
#if UNITY
			UnityEngine.Debug.Log(CreateMessage(message)); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateMessage(message));
#endif
		}

#if DisableVerboseLogging
		[Conditional("DummyConditionThatNeverExists")]
#endif
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
		public static void Error(Exception exception)
		{
#if UNITY
			UnityEngine.Debug.LogError(CreateDetailedExceptionMessage(exception)); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateDetailedExceptionMessage(exception));
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

		[DebuggerHidden]
		public static void Error(Exception exception, ContextObject context)
		{
#if UNITY
			UnityEngine.Debug.LogError(CreateDetailedExceptionMessage(exception), context); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateDetailedExceptionMessage(exception));
#endif
		}

		/// <summary>
		/// Sends error message to Unity Cloud Diagnostics tool without breaking the code flow by throwing an exception.
		/// </summary>
		[DebuggerHidden]
		public static void Fatal(string message)
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
		public static void Fatal(Exception exception)
		{
#if UNITY
			UnityEngine.Debug.LogException(exception); // Ignored by Code Correct
#else
			System.Console.WriteLine(exception.ToString());
#endif
		}

		/// <summary>
		/// Sends error message to Unity Cloud Diagnostics tool without breaking the code flow by throwing an exception.
		/// </summary>
		[DebuggerHidden]
		public static void Fatal(string message, ContextObject context)
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
		public static void Fatal(Exception exception, ContextObject context)
		{
#if UNITY
			UnityEngine.Debug.LogException(exception, context); // Ignored by Code Correct
#else
			System.Console.WriteLine(exception.ToString());
#endif
		}

		/// <summary>
		/// Internal errors are logged just like Fatal errors. They will appear in Unity Cloud Diagnostics without breaking the code flow by throwing an exception.
		///
		/// See also 'InternalException'.
		/// </summary>
		[DebuggerHidden]
		internal static void _InternalError(string category, ContextObject context, int errorCode)
		{
#if UNITY
			UnityEngine.Debug.LogException(new InternalException(errorCode, category), context); // Ignored by Code Correct
#else
			System.Console.WriteLine(new InternalException(errorCode, category).ToString());
#endif
		}

		/// <summary>
		/// Internal errors are logged just like Fatal errors. They will appear in Unity Cloud Diagnostics without breaking the code flow by throwing an exception.
		///
		/// See also 'InternalException'.
		/// </summary>
		[DebuggerHidden]
		internal static void _InternalError(string category, ContextObject context, int errorCode, Exception innerException)
		{
#if UNITY
			UnityEngine.Debug.LogException(new InternalException(errorCode, category, innerException), context); // Ignored by Code Correct
#else
			System.Console.WriteLine(new InternalException(errorCode, category, innerException).ToString());
#endif
		}

		#endregion

		#region Internal Error Message

		public static string BuildInternalErrorMessage(int errorCode)
		{
			return $"Internal error {errorCode}.";
		}

		public static string BuildInternalErrorMessage(int errorCode, string category)
		{
			return $"Internal error {errorCode} in {category}.";
		}

		#endregion

		// TODO-Log: Move these into Logger
		#region Log Tools - Methods

		[DebuggerHidden]
		public static void CurrentMethodNotImplemented()
		{
			Fatal("Method '" + DebugReflection.PreviousMethodNameWithType + "' is not implemented!");
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

		public static void StackTrace(string headerMessage, ContextObject context = default)
		{
			using (Indent(headerMessage, null, context))
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
