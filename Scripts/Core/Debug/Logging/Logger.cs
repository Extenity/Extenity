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
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Extenity.DebugToolbox
{

	public static class Logger
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
						Log(EndText);
					else
						Log(EndText, Context);
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
				Log(startText);
			}
			return new IndentationHandler(null, endText);
		}

		public static IDisposable IndentFormat(string startText, params object[] startTextArgs)
		{
			if (!string.IsNullOrEmpty(startText))
			{
				LogFormat(startText, startTextArgs);
			}
			return new IndentationHandler();
		}

		public static IDisposable Indent(Object context, string startText, string endText = null)
		{
			if (!string.IsNullOrEmpty(startText))
			{
				Log(startText, context);
			}
			return new IndentationHandler(context, endText);
		}

		public static IDisposable IndentFormat(Object context, string startText, params object[] startTextArgs)
		{
			if (!string.IsNullOrEmpty(startText))
			{
				LogFormat(context, startText, startTextArgs);
			}
			return new IndentationHandler();
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
					Debug.Log("############### destroyed object: " + item.Key); // TODO: test this
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

		public static void Log(object message)
		{
			Debug.Log(CreateMessage(message), null);
		}

		public static void Log(object message, Object context)
		{
			Debug.Log(CreateMessage(message, context), context);
		}

		public static void LogFormat(string format, params object[] args)
		{
			Debug.Log(CreateMessage(string.Format(format, args)), null);
		}

		public static void LogFormat(Object context, string format, params object[] args)
		{
			Debug.Log(CreateMessage(string.Format(format, args), context), context);
		}

		public static void LogWarning(object message)
		{
			Debug.LogWarning(CreateMessage(message), null);
		}

		public static void LogWarning(object message, Object context)
		{
			Debug.LogWarning(CreateMessage(message, context), context);
		}

		public static void LogWarningFormat(string format, params object[] args)
		{
			Debug.LogWarning(CreateMessage(string.Format(format, args)), null);
		}

		public static void LogWarningFormat(Object context, string format, params object[] args)
		{
			Debug.LogWarning(CreateMessage(string.Format(format, args), context), context);
		}

		public static void LogError(object message)
		{
			Debug.LogError(CreateMessage(message), null);
		}

		public static void LogError(object message, Object context)
		{
			Debug.LogError(CreateMessage(message, context), context);
		}

		public static void LogErrorFormat(string format, params object[] args)
		{
			Debug.LogError(CreateMessage(string.Format(format, args)), null);
		}

		public static void LogErrorFormat(Object context, string format, params object[] args)
		{
			Debug.LogError(CreateMessage(string.Format(format, args), context), context);
		}

		public static void LogException(Exception exception)
		{
			Debug.LogError(CreateMessage(exception == null ? "Null exception" : exception.ToString()), null);
		}

		public static void LogException(Exception exception, Object context)
		{
			Debug.LogError(CreateMessage(exception == null ? "Null exception" : exception.ToString(), context), context);
		}

		public static void LogExceptionDetailed(this Exception exception)
		{
			Debug.LogError(CreateDetailedExceptionMessage(exception), null);
		}

		public static void LogExceptionDetailed(this Exception exception, Object context)
		{
			Debug.LogError(CreateDetailedExceptionMessage(exception, context), context);
		}

		#endregion

		#region Debug Log

		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
		public static void DebugLog(object message)
		{
			Debug.Log(CreateMessage(message), null);
		}

		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
		public static void DebugLog(object message, Object context)
		{
			Debug.Log(CreateMessage(message, context), context);
		}

		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
		public static void DebugLogFormat(string format, params object[] args)
		{
			Debug.Log(CreateMessage(string.Format(format, args)), null);
		}

		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
		public static void DebugLogFormat(Object context, string format, params object[] args)
		{
			Debug.Log(CreateMessage(string.Format(format, args), context), context);
		}

		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
		public static void DebugLogWarning(object message)
		{
			Debug.LogWarning(CreateMessage(message), null);
		}

		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
		public static void DebugLogWarning(object message, Object context)
		{
			Debug.LogWarning(CreateMessage(message, context), context);
		}

		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
		public static void DebugLogWarningFormat(string format, params object[] args)
		{
			Debug.LogWarning(CreateMessage(string.Format(format, args)), null);
		}

		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
		public static void DebugLogWarningFormat(Object context, string format, params object[] args)
		{
			Debug.LogWarning(CreateMessage(string.Format(format, args), context), context);
		}

		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
		public static void DebugLogError(object message)
		{
			Debug.LogError(CreateMessage(message), null);
		}

		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
		public static void DebugLogError(object message, Object context)
		{
			Debug.LogError(CreateMessage(message, context), context);
		}

		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
		public static void DebugLogErrorFormat(string format, params object[] args)
		{
			Debug.LogError(CreateMessage(string.Format(format, args)), null);
		}

		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
		public static void DebugLogErrorFormat(Object context, string format, params object[] args)
		{
			Debug.LogError(CreateMessage(string.Format(format, args), context), context);
		}

		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
		public static void DebugLogException(Exception exception)
		{
			Debug.LogError(CreateMessage(exception == null ? "Null exception" : exception.ToString()), null);
		}

		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
		public static void DebugLogException(Exception exception, Object context)
		{
			Debug.LogError(CreateMessage(exception == null ? "Null exception" : exception.ToString(), context), context);
		}

		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
		public static void DebugLogExceptionDetailed(this Exception exception)
		{
			Debug.LogError(CreateDetailedExceptionMessage(exception), null);
		}

		[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
		public static void DebugLogExceptionDetailed(this Exception exception, Object context)
		{
			Debug.LogError(CreateDetailedExceptionMessage(exception, context), context);
		}

		#endregion

		#region Log Tools - Methods

		public static void LogCurrentMethodNotImplemented()
		{
			LogError("Method '" + DebugReflection.PreviousMethodNameWithType + "' is not implemented!");
		}

		public static void LogCurrentMethod(string additionalText = null)
		{
			Log(string.IsNullOrEmpty(additionalText) ?
				DebugReflection.PreviousMethodNameWithType :
				DebugReflection.PreviousMethodNameWithType + " : " + additionalText);
		}

		public static void LogPreviousMethod(string additionalText = null)
		{
			Log(string.IsNullOrEmpty(additionalText) ?
				DebugReflection.PrePreviousMethodNameWithType :
				DebugReflection.PrePreviousMethodNameWithType + " : " + additionalText);
		}

		public static void LogCurrentMethodOfGameObject(this MonoBehaviour me, string additionalText = null)
		{
			Log(string.IsNullOrEmpty(additionalText) ?
				DebugReflection.PreviousMethodNameWithType + " (" + (me == null ? "Null" : me.name) + ")" :
				DebugReflection.PreviousMethodNameWithType + " (" + (me == null ? "Null" : me.name) + ") : " + additionalText);
		}

		public static void LogPreviousMethodOfGameObject(this MonoBehaviour me, string additionalText = null)
		{
			Log(string.IsNullOrEmpty(additionalText) ?
				DebugReflection.PrePreviousMethodNameWithType + " (" + (me == null ? "Null" : me.name) + ")" :
				DebugReflection.PrePreviousMethodNameWithType + " (" + (me == null ? "Null" : me.name) + ") : " + additionalText);
		}

		#endregion

		#region Log Tools - Stack Trace

		public static void LogStackTrace(string headerMessage, Object context = null)
		{
			using (Indent(context, headerMessage))
			{
				var frames = new StackTrace(1).GetFrames();

				for (int i = 0; i < frames.Length; i++)
				{
					MethodBase method = frames[i].GetMethod();

					var reflectedTypeName = method.ReflectedType != null ? method.ReflectedType.Name : string.Empty;
					Log(reflectedTypeName + "::" + method.Name, context);
				}
			}
		}

		#endregion
	}

	public static class LoggerExtensions
	{
		#region Simple

		public static void LogSimple<T>(this T obj, string prefix = "")
		{
			if (obj == null)
			{
				Logger.Log("Object is null.");
				return;
			}

			if (!string.IsNullOrEmpty(prefix))
				prefix += ": ";
			Logger.Log(prefix + obj.ToString());
		}

		public static void LogSimple<T>(this T obj, UnityEngine.Object context, string prefix = "")
		{
			if (obj == null)
			{
				Logger.Log("Object is null.");
				return;
			}

			if (!string.IsNullOrEmpty(prefix))
				prefix += ": ";
			Logger.Log(prefix + obj.ToString(), context);
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
			Logger.Log(prefix + body.Member.Name + ": '" + value + "'");
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
			Logger.Log(prefix + body.Member.Name + ": '" + value + "'", context);
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
				Logger.Log("Dictionary is null.");
				return;
			}

			string text = "";

			foreach (KeyValuePair<TKey, TValue> item in dictionary)
			{
				var line = (item.Key == null ? "Null" : item.Key.ToString()) + ": '" + (item.Value == null ? "Null" : item.Value.ToString()) + "'";
				if (inSeparateLogCalls)
				{
					Logger.Log(line);
				}
				else
				{
					text += line + "\n";
				}
			}

			if (!inSeparateLogCalls)
			{
				Logger.Log(text.ToString());
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
			Debug.Log(text);
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
			Debug.Log(text);
		}

		private static void InternalLogAllProperties(this object obj, StringBuilder stringBuilder, string indentation = "")
		{
			var nextIndentation = indentation + '\t';
			var i = 0;

			foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(obj))
			{
				var value = descriptor.GetValue(obj);
				stringBuilder.AppendLine(indentation + i + ") " + descriptor.Name + " = " + value);

				// Log enumerables (lists, arrays, etc.)
				if (value != null && descriptor.PropertyType.InheritsOrImplements(typeof(IEnumerable)))
				{
					if (descriptor.PropertyType != typeof(string)) // string is an exception
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
								InternalLogAllFields(item, stringBuilder, nextIndentation + iInside + ".");
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
			var nextIndentation = indentation + '\t';
			var i = 0;

			foreach (var fieldInfo in obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
			{
				var value = fieldInfo.GetValue(obj);
				stringBuilder.AppendLine(indentation + i + ") " + fieldInfo.Name + " = " + value);

				// Log enumerables (lists, arrays, etc.)
				if (value != null && fieldInfo.FieldType.InheritsOrImplements(typeof(IEnumerable)))
				{
					if (fieldInfo.FieldType != typeof(string)) // string is an exception
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

}
