using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Extenity.DataToolbox;
using Extenity.DebugToolbox;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

//namespace Extenity.DebugToolbox
//{

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
		private Object Context;
		private string EndText;

		internal IndentationHandler(Object context = null, string endText = null)
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

	public static IDisposable Indent(Object context, string startText, string endText = null)
	{
		if (!string.IsNullOrEmpty(startText))
		{
			Info(startText, context);
		}
		return new IndentationHandler(context, endText);
	}

	#endregion

	#region Prefix

	public static Dictionary<Object, string> RegisteredPrefixes = new Dictionary<Object, string>(100);

	public static void RegisterPrefix(Object obj, string prefix)
	{
		ClearDestroyedObjectPrefixes();

		if (RegisteredPrefixes.ContainsKey(obj))
			RegisteredPrefixes[obj] = prefix;
		else
			RegisteredPrefixes.Add(obj, prefix);
	}

	public static void DeregisterPrefix(Object obj)
	{
		RegisteredPrefixes.Remove(obj);
		ClearDestroyedObjectPrefixes();
	}

	private static void ClearDestroyedObjectPrefixes()
	{
		foreach (var item in RegisteredPrefixes)
		{
			if (item.Key == null)
			{
				Log.Info("############### destroyed object: " + item.Key); // TODO: test this
				RegisteredPrefixes.Remove(item.Key);
			}
		}
	}

	#endregion

	#region Disable Logging By Object

	// TODO: Implement.

	#endregion

	#region Create Message

	public static string PrefixSeparator = " | ";

	private static string CreateMessage(object message)
	{
		var messageString = message == null
			? "Null"
			: message.ToString().NormalizeLineEndings();

		return CurrentIndentationString + messageString;
	}

	private static string CreateMessage(object message, Object obj)
	{
		var messageString = message == null
			? "Null"
			: message.ToString().NormalizeLineEndings();

		string prefix;
		if (obj != null && RegisteredPrefixes != null && RegisteredPrefixes.TryGetValue(obj, out prefix))
		{
			return CurrentIndentationString + prefix + PrefixSeparator + messageString;
		}
		else
		{
			return CurrentIndentationString + messageString;
		}
	}

	private static string CreateDetailedExceptionMessage(Exception exception)
	{
		string messageString = exception == null
			? "Null exception"
			: InternalCreateDetailedExceptionMessage(exception).NormalizeLineEndings();

		return CurrentIndentationString + messageString;
	}

	private static string CreateDetailedExceptionMessage(Exception exception, Object obj)
	{
		string messageString = exception == null
			? "Null exception"
			: InternalCreateDetailedExceptionMessage(exception).NormalizeLineEndings();

		string prefix;
		if (obj != null && RegisteredPrefixes != null && RegisteredPrefixes.TryGetValue(obj, out prefix))
		{
			return CurrentIndentationString + prefix + PrefixSeparator + messageString;
		}
		else
		{
			return CurrentIndentationString + messageString;
		}
	}

	private static string InternalCreateDetailedExceptionMessage(Exception exception)
	{
		string message = exception.ToString();
		message += "\r\nInnerException: " + exception.InnerException;
		message += "\r\nMessage: " + exception.Message;
		message += "\r\nSource: " + exception.Source;
		message += "\r\nStackTrace: " + exception.StackTrace;
		message += "\r\nTargetSite: " + exception.TargetSite;
		return message;
	}

	#endregion

	#region Log

	public static void Info(object message)
	{
		Debug.Log(CreateMessage(message), null);
	}

	public static void Info(object message, Object context)
	{
		Debug.Log(CreateMessage(message, context), context);
	}

	public static void Warning(object message)
	{
		Debug.LogWarning(CreateMessage(message), null);
	}

	public static void Warning(object message, Object context)
	{
		Debug.LogWarning(CreateMessage(message, context), context);
	}

	public static void Error(object message)
	{
		Debug.LogError(CreateMessage(message), null);
	}

	public static void Error(object message, Object context)
	{
		Debug.LogError(CreateMessage(message, context), context);
	}

	/// <summary>
	/// Sends error message to Unity Cloud Diagnostics tool without breaking the code flow by throwing an exception.
	/// </summary>
	public static void CriticalError(string message)
	{
		Debug.LogException(new Exception(message));
	}

	/// <summary>
	/// Sends error message to Unity Cloud Diagnostics tool without breaking the code flow by throwing an exception.
	/// </summary>
	public static void CriticalError(string message, Exception innerException)
	{
		Debug.LogException(new Exception(message, innerException));
	}

	/// <summary>
	/// Sends error message to Unity Cloud Diagnostics tool without breaking the code flow by throwing an exception.
	/// </summary>
	public static void CriticalError(string message, Object context)
	{
		Debug.LogException(new Exception(message), context);
	}

	/// <summary>
	/// Sends error message to Unity Cloud Diagnostics tool without breaking the code flow by throwing an exception.
	/// </summary>
	public static void CriticalError(string message, Object context, Exception innerException)
	{
		Debug.LogException(new Exception(message, innerException), context);
	}

	/// <summary>
	/// Internal errors are logged just like critical errors. They will appear in Unity Cloud Diagnostics without breaking the code flow by throwing an exception.
	///
	/// See also 'InternalException'.
	/// </summary>
	public static void InternalError(int errorCode)
	{
		Debug.LogException(new InternalException(errorCode));
	}

	public static void Exception(Exception exception)
	{
		Debug.LogException(exception);
	}

	public static void Exception(Exception exception, Object context)
	{
		Debug.LogException(exception, context);
	}

	public static void ExceptionAsError(Exception exception)
	{
		Debug.LogError(CreateMessage(exception == null ? "Null exception" : exception.ToString()), null);
	}

	public static void ExceptionAsError(Exception exception, Object context)
	{
		Debug.LogError(CreateMessage(exception == null ? "Null exception" : exception.ToString(), context), context);
	}

	public static void ExceptionAsErrorDetailed(this Exception exception)
	{
		Debug.LogError(CreateDetailedExceptionMessage(exception), null);
	}

	public static void ExceptionAsErrorDetailed(this Exception exception, Object context)
	{
		Debug.LogError(CreateDetailedExceptionMessage(exception, context), context);
	}

	#endregion

	#region Debug Log

	[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
	public static void DebugInfo(object message)
	{
		Debug.Log(CreateMessage(message), null);
	}

	[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
	public static void DebugInfo(object message, Object context)
	{
		Debug.Log(CreateMessage(message, context), context);
	}

	[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
	public static void DebugWarning(object message)
	{
		Debug.LogWarning(CreateMessage(message), null);
	}

	[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
	public static void DebugWarning(object message, Object context)
	{
		Debug.LogWarning(CreateMessage(message, context), context);
	}

	[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
	public static void DebugError(object message)
	{
		Debug.LogError(CreateMessage(message), null);
	}

	[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
	public static void DebugError(object message, Object context)
	{
		Debug.LogError(CreateMessage(message, context), context);
	}

	/// <summary>
	/// Internal errors are logged just like critical errors. They will appear in Unity Cloud Diagnostics without breaking the code flow by throwing an exception.
	///
	/// See also 'InternalException'.
	/// </summary>
	[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
	public static void DebugInternalError(int errorCode)
	{
		Debug.LogException(new InternalException(errorCode));
	}

	[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
	public static void DebugException(Exception exception)
	{
		Debug.LogException(exception);
	}

	[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
	public static void DebugException(Exception exception, Object context)
	{
		Debug.LogException(exception, context);
	}

	[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
	public static void DebugExceptionAsError(Exception exception)
	{
		Debug.LogError(CreateMessage(exception == null ? "Null exception" : exception.ToString()), null);
	}

	[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
	public static void DebugExceptionAsError(Exception exception, Object context)
	{
		Debug.LogError(CreateMessage(exception == null ? "Null exception" : exception.ToString(), context), context);
	}

	[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
	public static void DebugExceptionAsErrorDetailed(this Exception exception)
	{
		Debug.LogError(CreateDetailedExceptionMessage(exception), null);
	}

	[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
	public static void DebugExceptionAsErrorDetailed(this Exception exception, Object context)
	{
		Debug.LogError(CreateDetailedExceptionMessage(exception, context), context);
	}

	#endregion

	#region Internal Error Message

	public static string BuildInternalErrorMessage(int errorCode)
	{
		return "Internal error " + errorCode + "!";
	}

	#endregion

	#region Log Tools - Methods

	public static void CurrentMethodNotImplemented()
	{
		Error("Method '" + DebugReflection.PreviousMethodNameWithType + "' is not implemented!");
	}

	public static void CurrentMethod(string additionalText = null)
	{
		Info(string.IsNullOrEmpty(additionalText) ?
			DebugReflection.PreviousMethodNameWithType :
			DebugReflection.PreviousMethodNameWithType + " : " + additionalText);
	}

	public static void PreviousMethod(string additionalText = null)
	{
		Info(string.IsNullOrEmpty(additionalText) ?
			DebugReflection.PrePreviousMethodNameWithType :
			DebugReflection.PrePreviousMethodNameWithType + " : " + additionalText);
	}

	public static void CurrentMethodOfGameObject(this MonoBehaviour me, string additionalText = null)
	{
		Info(string.IsNullOrEmpty(additionalText) ?
			DebugReflection.PreviousMethodNameWithType + " (" + (me == null ? "Null" : me.name) + ")" :
			DebugReflection.PreviousMethodNameWithType + " (" + (me == null ? "Null" : me.name) + ") : " + additionalText);
	}

	public static void PreviousMethodOfGameObject(this MonoBehaviour me, string additionalText = null)
	{
		Info(string.IsNullOrEmpty(additionalText) ?
			DebugReflection.PrePreviousMethodNameWithType + " (" + (me == null ? "Null" : me.name) + ")" :
			DebugReflection.PrePreviousMethodNameWithType + " (" + (me == null ? "Null" : me.name) + ") : " + additionalText);
	}

	#endregion

	#region Log Tools - Stack Trace

	public static void StackTrace(string headerMessage, Object context = null)
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

public static class LogExtensions
{
	#region Simple

	public static void LogSimple<T>(this T obj, string prefix = "")
	{
		if (obj == null)
		{
			Log.Info("Object is null.");
			return;
		}

		if (!string.IsNullOrEmpty(prefix))
			prefix += ": ";
		Log.Info(prefix + obj.ToString());
	}

	public static void LogSimple<T>(this T obj, UnityEngine.Object context, string prefix = "")
	{
		if (obj == null)
		{
			Log.Info("Object is null.");
			return;
		}

		if (!string.IsNullOrEmpty(prefix))
			prefix += ": ";
		Log.Info(prefix + obj.ToString(), context);
	}

	#endregion

	#region Variable Name and Value

	/// <summary>
	/// Usage: DebugLog.LogSimple(() => myVariable);
	/// </summary>
	public static void LogVariable<T>(Expression<Func<T>> expression, string prefix = "")
	{
		if (!string.IsNullOrEmpty(prefix))
			prefix += ": ";
		var body = (MemberExpression)expression.Body;
		var value = ((FieldInfo)body.Member).GetValue(((ConstantExpression)body.Expression).Value);
		Log.Info(prefix + body.Member.Name + ": '" + value + "'");
	}

	/// <summary>
	/// Usage: DebugLog.LogSimple(() => myVariable);
	/// </summary>
	public static void LogVariable<T>(Expression<Func<T>> expression, UnityEngine.Object context, string prefix = "")
	{
		if (!string.IsNullOrEmpty(prefix))
			prefix += ": ";
		var body = (MemberExpression)expression.Body;
		var value = ((FieldInfo)body.Member).GetValue(((ConstantExpression)body.Expression).Value);
		Log.Info(prefix + body.Member.Name + ": '" + value + "'", context);
	}

	#endregion

	#region List

	public static void LogList<T>(this IEnumerable<T> list, string initialLine = null, bool inSeparateLogCalls = false, LogType logType = LogType.Log)
	{
		StringBuilder stringBuilder = !inSeparateLogCalls
			? new StringBuilder()
			: null;

		// Initial line
		if (!string.IsNullOrEmpty(initialLine))
		{
			if (inSeparateLogCalls)
			{
				Debug.unityLogger.Log(logType, initialLine);
			}
			else
			{
				stringBuilder.AppendLine(initialLine);
			}
		}

		// Check if list is null
		if (list == null)
		{
			if (inSeparateLogCalls)
			{
				Debug.unityLogger.Log(logType, "[NullList]");
			}
			else
			{
				stringBuilder.AppendLine("[NullList]");
			}
		}
		else
		{
			// Log list
			foreach (T item in list)
			{
				var line = (item == null ? "[Null]" : item.ToString());
				if (inSeparateLogCalls)
				{
					Debug.unityLogger.Log(logType, line);
				}
				else
				{
					stringBuilder.AppendLine(line);
				}
			}
		}

		if (!inSeparateLogCalls)
		{
			Debug.unityLogger.Log(logType, stringBuilder.ToString());
		}
	}

	public static void LogWarningList<T>(this IEnumerable<T> list, string initialLine = null, bool inSeparateLogCalls = false)
	{
		LogList<T>(list, initialLine, inSeparateLogCalls, LogType.Warning);
	}

	public static void LogErrorList<T>(this IEnumerable<T> list, string initialLine = null, bool inSeparateLogCalls = false)
	{
		LogList<T>(list, initialLine, inSeparateLogCalls, LogType.Error);
	}

	#endregion

	#region Dictionary

	public static void LogDictionary<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, bool inSeparateLogCalls = false)
	{
		if (dictionary == null)
		{
			Log.Info("Dictionary is null.");
			return;
		}

		string text = "";

		foreach (KeyValuePair<TKey, TValue> item in dictionary)
		{
			var line = (item.Key == null ? "Null" : item.Key.ToString()) + ": '" + (item.Value == null ? "Null" : item.Value.ToString()) + "'";
			if (inSeparateLogCalls)
			{
				Log.Info(line);
			}
			else
			{
				text += line + "\n";
			}
		}

		if (!inSeparateLogCalls)
		{
			Log.Info(text.ToString());
		}
	}

	#endregion

	#region Dump Class Data

	public static void LogAllProperties<T>(this T obj, string initialLine = null)
	{
		// Initialize
		var stringBuilder = new StringBuilder();
		if (!string.IsNullOrEmpty(initialLine))
		{
			stringBuilder.AppendLine(initialLine);
		}

		// Do logging
		InternalLogAllProperties(obj, stringBuilder);

		// Finalize
		var text = stringBuilder.ToString();
		Log.Info(text);
	}

	public static void LogAllFields<T>(this T obj, string initialLine = null)
	{
		// Initialize
		var stringBuilder = new StringBuilder();
		if (!string.IsNullOrEmpty(initialLine))
		{
			stringBuilder.AppendLine(initialLine);
		}

		// Do logging
		InternalLogAllFields(obj, stringBuilder);

		// Finalize
		var text = stringBuilder.ToString();
		Log.Info(text);
	}

	public static void LogAllFieldsAndProperties<T>(this T obj, string initialLine = null)
	{
		// Initialize
		var stringBuilder = new StringBuilder();
		if (!string.IsNullOrEmpty(initialLine))
		{
			stringBuilder.AppendLine(initialLine);
		}

		// Do logging
		stringBuilder.AppendLine("Fields:");
		InternalLogAllFields(obj, stringBuilder);
		stringBuilder.AppendLine("Properties:");
		InternalLogAllProperties(obj, stringBuilder);

		// Finalize
		var text = stringBuilder.ToString();
		Log.Info(text);
	}

	private static void InternalLogAllProperties(this object obj, StringBuilder stringBuilder, string indentation = "")
	{
		string nextIndentation = null;
		var i = 0;

		foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(obj))
		{
			var value = descriptor.GetValue(obj);
			stringBuilder.AppendLine(indentation + i + ") " + descriptor.Name + " = " + value);

			// Log enumerables (lists, arrays, etc.)
			if (value != null && descriptor.PropertyType.InheritsOrImplements(typeof(IEnumerable)))
			{
				if (descriptor.PropertyType != typeof(string)) // string is an exception. We don't want to iterate its characters.
				{
					var iInside = 0;

					foreach (var item in (IEnumerable)value)
					{
						if (item == null)
						{
							stringBuilder.AppendLine(indentation + iInside + ") " + "(null)");
						}
						else
						{
							if (nextIndentation == null)
								nextIndentation = indentation + '\t';
							InternalLogAllProperties(item, stringBuilder, nextIndentation + iInside + ".");
						}
						iInside++;
					}
				}
			}

			i++;
		}
	}

	private static void InternalLogAllFields(this object obj, StringBuilder stringBuilder, string indentation = "")
	{
		string nextIndentation = null;
		var i = 0;

		foreach (var fieldInfo in obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
		{
			var value = fieldInfo.GetValue(obj);
			stringBuilder.AppendLine(indentation + i + ") " + fieldInfo.Name + " = " + value);

			// Log enumerables (lists, arrays, etc.)
			if (value != null && fieldInfo.FieldType.InheritsOrImplements(typeof(IEnumerable)))
			{
				if (fieldInfo.FieldType != typeof(string)) // string is an exception. We don't want to iterate its characters.
				{
					var iInside = 0;

					foreach (var item in (IEnumerable)value)
					{
						if (item == null)
						{
							stringBuilder.AppendLine(indentation + iInside + ") " + "(null)");
						}
						else
						{
							if (nextIndentation == null)
								nextIndentation = indentation + '\t';
							InternalLogAllFields(item, stringBuilder, nextIndentation + iInside + ".");
						}
						iInside++;
					}
				}
			}

			i++;
		}
	}

	#endregion
}

public class InternalException : Exception
{
	public InternalException(int errorCode) 
		: base(Log.BuildInternalErrorMessage(errorCode))
	{
	}

	public InternalException(int errorCode, Exception innerException) 
		: base(Log.BuildInternalErrorMessage(errorCode), innerException)
	{
	}
}

//}
