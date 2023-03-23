using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using Extenity.DataToolbox;

namespace Extenity
{

	// TODO-Log: Convert these ToStream that returns Utf16ValueStringBuilder.
	public static class LogExtensions
	{
		#region List

		public static void LogList<T>(this IEnumerable<T> list, string initialLine = null, bool inSeparateLogCalls = false, LogSeverity severity = LogSeverity.Info)
		{
			var stringBuilder = !inSeparateLogCalls
				? new StringBuilder()
				: null;

			// Initial line
			if (!string.IsNullOrEmpty(initialLine))
			{
				if (inSeparateLogCalls)
				{
					Log.Any(severity, initialLine);
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
					Log.Any(severity, "[NullList]");
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
						Log.Any(severity, line);
					}
					else
					{
						stringBuilder.AppendLine(line);
					}
				}
			}

			if (!inSeparateLogCalls)
			{
				Log.Any(severity, stringBuilder.ToString());
			}
		}

		#endregion

		#region Dictionary

		public static void LogDictionary<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, string initialLine = null, bool inSeparateLogCalls = false, LogSeverity severity = LogSeverity.Info)
		{
			var stringBuilder = !inSeparateLogCalls
				? new StringBuilder()
				: null;

			// Initial line
			if (!string.IsNullOrEmpty(initialLine))
			{
				if (inSeparateLogCalls)
				{
					Log.Any(severity, initialLine);
				}
				else
				{
					stringBuilder.AppendLine(initialLine);
				}
			}

			// Check if dictionary is null
			if (dictionary == null)
			{
				if (inSeparateLogCalls)
				{
					Log.Any(severity, "[NullDict]");
				}
				else
				{
					stringBuilder.AppendLine("[NullDict]");
				}
			}
			else
			{
				// Log dictionary
				foreach (KeyValuePair<TKey, TValue> item in dictionary)
				{
					var line = (item.Key == null ? "[Null]" : item.Key.ToString()) + ": '" + (item.Value == null ? "[Null]" : item.Value.ToString()) + "'";
					if (inSeparateLogCalls)
					{
						Log.Any(severity, line);
					}
					else
					{
						stringBuilder.AppendLine(line);
					}
				}
			}

			if (!inSeparateLogCalls)
			{
				Log.Any(severity, stringBuilder.ToString());
			}
		}

		#endregion

		#region Dump Class Data

		public static void LogAllProperties<T>(this T obj, string initialLine = null, LogSeverity severity = LogSeverity.Info)
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
			Log.Any(severity, text);
		}

		public static void LogAllFields<T>(this T obj, string initialLine = null, LogSeverity severity = LogSeverity.Info)
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
			Log.Any(severity, text);
		}

		public static void LogAllFieldsAndProperties<T>(this T obj, string initialLine = null, LogSeverity severity = LogSeverity.Info)
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
			Log.Any(severity, text);
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

		#region Log

		private static readonly Logger Log = new(nameof(LogExtensions));

		#endregion
	}

}
