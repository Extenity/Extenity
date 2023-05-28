//#define DisableVerboseLogging | Note that this should be defined project wide since Logger also depends on it.
//#define DisableInfoLogging | Note that this should be defined project wide since Logger also depends on it.

using System.Diagnostics;
using Cysharp.Text;
using Extenity.DataToolbox;
using Exception = System.Exception;
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
		#region Logger

		public static Logger With(string category)
		{
			return new Logger(category);
		}

		public static Logger With(string category, ContextObject context)
		{
			return new Logger(category, context);
		}

		#endregion

		#region Indentation

		private static string IndentationOneLevelString = "    ";

		// TODO: This should be TheadStatic to support multi-threaded logging.
		private static int _Indentation;

		private static int Indentation
		{
			get { return _Indentation < 0 ? 0 : _Indentation; }
			set
			{
				_Indentation = value;
				CurrentIndentationString = IndentationOneLevelString.Repeat(Indentation);
			}
		}

		private static string CurrentIndentationString;

		internal static void _IncreaseIndent()
		{
			Indentation++;
		}

		internal static void _DecreaseIndent()
		{
			Indentation--;
		}

		#endregion

		#region Indentation Using 'Using'

		public struct IndentationHandler : IDisposable
		{
			public void Dispose()
			{
				_DecreaseIndent();
			}
		}

		internal static IndentationHandler _IndentedScope
		{
			get
			{
				_IncreaseIndent();
				return new IndentationHandler();
			}
		}

		#endregion

		#region Create Message

		private static string CreateMessageWithCategoryAndIndentation(string category, string message)
		{
			if (message == null)
				return ZString.Concat("[", category, "] ", CurrentIndentationString, "[NullString]");
			else
				return ZString.Concat("[", category, "] ", CurrentIndentationString, message.NormalizeLineEndingsCRLF());
		}


		private static string CreateMessageWithCategory(string category, string message)
		{
			if (message == null)
				return ZString.Concat("[", category, "] ", "[NullString]");
			else
				return ZString.Concat("[", category, "] ", message.NormalizeLineEndingsCRLF());
		}

		private static string CreateDetailedExceptionMessage(string category, Exception exception)
		{
			if (exception == null)
				return ZString.Concat("[", category, "] ", "[NullException]");
			else
				return ZString.Concat("[", category, "] ", InternalCreateDetailedExceptionMessage(exception).NormalizeLineEndingsCRLF());
		}

		private static string InternalCreateDetailedExceptionMessage(Exception exception)
		{
			// TODO-Log: Use ZString
			var message = exception.ToString();
			message += "\r\nInnerException: " + exception.InnerException; // TODO-Log: This should be recursive
			message += "\r\nMessage: " + exception.Message;
			message += "\r\nSource: " + exception.Source;
			message += "\r\nStackTrace: " + exception.StackTrace;
			message += "\r\nTargetSite: " + exception.TargetSite;
			return message;
		}

		#endregion

		#region Log

#if DisableVerboseLogging
		[Conditional("DummyConditionThatNeverExists")]
#endif
		[DebuggerHidden]
		internal static void _Verbose(string category, ContextObject context, string message)
		{
#if UNITY
			UnityEngine.Debug.Log(CreateMessageWithCategoryAndIndentation(category, message), context); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateMessageWithCategoryAndIndentation(category, message));
#endif
		}

#if DisableInfoLogging
		[Conditional("DummyConditionThatNeverExists")]
#endif
		[DebuggerHidden]
		internal static void _Info(string category, ContextObject context, string message)
		{
#if UNITY
			UnityEngine.Debug.Log(CreateMessageWithCategoryAndIndentation(category, message), context); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateMessageWithCategoryAndIndentation(category, message));
#endif
		}

		[DebuggerHidden]
		internal static void _Warning(string category, ContextObject context, string message)
		{
#if UNITY
			UnityEngine.Debug.LogWarning(CreateMessageWithCategoryAndIndentation(category, message), context); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateMessageWithCategoryAndIndentation(category, message));
#endif
		}

		[DebuggerHidden]
		internal static void _Error(string category, ContextObject context, string message)
		{
#if UNITY
			UnityEngine.Debug.LogError(CreateMessageWithCategoryAndIndentation(category, message), context); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateMessageWithCategoryAndIndentation(category, message));
#endif
		}

		[DebuggerHidden]
		internal static void _Error(string category, ContextObject context, Exception exception)
		{
#if UNITY
			UnityEngine.Debug.LogError(CreateDetailedExceptionMessage(category, exception), context); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateDetailedExceptionMessage(category, exception));
#endif
		}

		/// <summary>
		/// Sends error message to Unity Cloud Diagnostics tool without breaking the code flow by throwing an exception.
		/// </summary>
		[DebuggerHidden]
		internal static void _Fatal(string category, ContextObject context, string message)
		{
#if UNITY
			UnityEngine.Debug.LogException(new Exception(CreateMessageWithCategory(category, message)), context); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateMessageWithCategory(category, message));
#endif
		}

		/// <summary>
		/// Sends error message to Unity Cloud Diagnostics tool without breaking the code flow by throwing an exception.
		/// </summary>
		[DebuggerHidden]
		internal static void _Fatal(string category, ContextObject context, Exception exception)
		{
#if UNITY
			UnityEngine.Debug.LogException(new Exception(CreateDetailedExceptionMessage(category, exception), exception), context); // Ignored by Code Correct
#else
			System.Console.WriteLine(CreateDetailedExceptionMessage(category, exception));
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
		/*
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
		*/
	}

}
