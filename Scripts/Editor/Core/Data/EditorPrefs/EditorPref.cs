using System;
using Extenity.MessagingToolbox;
using JetBrains.Annotations;
using UnityEditor;

namespace Extenity.DataToolbox.Editor
{

	public enum EditorPrefLogOptions
	{
		NoLogging = 0,

		LogOnRead = 1 << 0,
		LogOnWriteWhenChanged = 1 << 1,
		LogOnWriteWhenNotChanged = 1 << 2,

		FullLogging = LogOnRead | LogOnWriteWhenChanged | LogOnWriteWhenNotChanged,
	}

	internal enum DefaultValueMethodType
	{
		DefaultValue,
		OverrideFunction,
		EnsureKeyExists,
	}

	public class DefaultValueMethod<T>
	{
		internal readonly DefaultValueMethodType Type;
		internal readonly T DefaultValue;
		internal readonly Func<EditorPref<T>, T> FunctionToGetDefaultValue;

		private DefaultValueMethod(DefaultValueMethodType type, T defaultValue, Func<EditorPref<T>, T> functionToGetDefaultValue)
		{
			Type                      = type;
			DefaultValue              = defaultValue;
			FunctionToGetDefaultValue = functionToGetDefaultValue;
		}

		public static DefaultValueMethod<T> Value(T defaultValue)
		{
			return new DefaultValueMethod<T>(DefaultValueMethodType.DefaultValue, defaultValue, null);
		}

		public static DefaultValueMethod<T> Function(Func<EditorPref<T>, T> functionToGetDefaultValue)
		{
			return new DefaultValueMethod<T>(DefaultValueMethodType.OverrideFunction, default, functionToGetDefaultValue);
		}

		public static DefaultValueMethod<T> Function(Func<T> functionToGetDefaultValue)
		{
			return new DefaultValueMethod<T>(DefaultValueMethodType.OverrideFunction, default, _ => functionToGetDefaultValue());
		}

		public static DefaultValueMethod<T> FailIfKeyDoesNotExist()
		{
			return new DefaultValueMethod<T>(DefaultValueMethodType.EnsureKeyExists, default, null);
		}
	}

	public abstract class EditorPref<T>
	{
		#region Initialization

		public EditorPref([NotNull] string prefsKey, PathHashPostfix appendPathHashToKey, DefaultValueMethod<T> defaultValueMethod, EditorPrefLogOptions logOptions)
		{
			PrefsKey            = prefsKey;
			AppendPathHashToKey = appendPathHashToKey;
			DefaultValueMethod  = defaultValueMethod;
			LogOptions          = logOptions;
		}

		#endregion

		#region Key

		public readonly string PrefsKey;
		private readonly PathHashPostfix AppendPathHashToKey;

		private string _ProcessedPrefsKey;

		public string ProcessedPrefsKey
		{
			get
			{
				if (string.IsNullOrEmpty(_ProcessedPrefsKey))
				{
					_ProcessedPrefsKey = PlayerPrefsTools.GenerateKey(PrefsKey, AppendPathHashToKey);
				}

				return _ProcessedPrefsKey;
			}
		}

		#endregion

		#region Default Value

		private readonly DefaultValueMethod<T> DefaultValueMethod;

		#endregion

		#region Value Caching

		// There once was a caching mechanism in EditorPref. But it was making things complicated
		// when the value is changed from outside of the EditorPref. So it's removed.
		// protected T _CachedValue;

		#endregion
		
		#region Value

		public bool GetValueIfSavedBefore(out T value)
		{
			if (EditorPrefs.HasKey(ProcessedPrefsKey))
			{
				value = Value;
				return true;
			}
			else
			{
				value = default;
				return false;
			}
		}

		public T Value
		{
			get
			{
				if (!EditorPrefs.HasKey(ProcessedPrefsKey))
				{
					// Get default value
					switch (DefaultValueMethod.Type)
					{
						case DefaultValueMethodType.DefaultValue:
						{
							var value = DefaultValueMethod.DefaultValue;
							if (LogOptions.HasFlag(EditorPrefLogOptions.LogOnRead))
								Log.Info($"Reading EditorPref '{ProcessedPrefsKey}'. The preference was not saved before, so the value is initialized with default '{value}'.");
							return value;
						}
						case DefaultValueMethodType.OverrideFunction:
						{
							var value = DefaultValueMethod.FunctionToGetDefaultValue(this);
							if (LogOptions.HasFlag(EditorPrefLogOptions.LogOnRead))
								Log.Info($"Reading EditorPref '{ProcessedPrefsKey}'. The preference was not saved before, so the value is initialized via default value function as '{value}'.");
							return value;
						}
						case DefaultValueMethodType.EnsureKeyExists:
						{
							throw new Exception($"Reading EditorPref '{ProcessedPrefsKey}' failed. Key does not exist.");
						}
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
				else
				{
					var value = InternalGetValue();
					if (LogOptions.HasFlag(EditorPrefLogOptions.LogOnRead))
						Log.Info($"Reading EditorPref '{ProcessedPrefsKey}'. The value is '{value}'.");
					return value;
				}
			}
			set
			{
				if (EditorPrefs.HasKey(ProcessedPrefsKey))
				{
					var oldValue = Value;
					if (IsSame(oldValue, value))
					{
						if (LogOptions.HasFlag(EditorPrefLogOptions.LogOnWriteWhenNotChanged))
							Log.Info($"Writing EditorPref '{ProcessedPrefsKey}'. The value is not changed from '{oldValue}'.");

						return; // Nothing to do here. The value is not changed.
					}
					else
					{
						if (LogOptions.HasFlag(EditorPrefLogOptions.LogOnWriteWhenChanged))
							Log.Info($"Writing EditorPref '{ProcessedPrefsKey}'. The value is changed from '{oldValue}' to '{value}'.");
					}
				}
				else
				{
					if (LogOptions.HasFlag(EditorPrefLogOptions.LogOnWriteWhenChanged))
						Log.Info($"Writing EditorPref '{ProcessedPrefsKey}'. The value is initialized as '{value}'.");
				}

				InternalSetValue(value);

				if (DontEmitNextValueChangedEvent)
				{
					if (LogOptions.HasFlag(EditorPrefLogOptions.LogOnWriteWhenChanged))
						Log.Info($"Value change event of '{ProcessedPrefsKey}' is suppressed.");

					DontEmitNextValueChangedEvent = false;
				}
				else
				{
					OnValueChanged.InvokeSafe(value);
				}
			}
		}

		#endregion

		#region Value Changed Event

		public class ValueChangedEvent : ExtenityEvent<T> { }
		public readonly ValueChangedEvent OnValueChanged = new ValueChangedEvent();

		private bool DontEmitNextValueChangedEvent;

		public void AddOnValueChangedListener(Action<T> listener, bool initializeByInvokingImmediately)
		{
			if (listener == null)
				throw new ArgumentNullException();

			OnValueChanged.AddListener(listener);

			if (initializeByInvokingImmediately)
			{
				// Invoke the registered callback. This is useful where the callback method
				// should be called before the first value change event is emitted.
				listener.Invoke(Value);
			}
		}

		public void InvokeValueChanged(bool log)
		{
			if (log)
			{
				Log.Info($"Invoking value change event of '{ProcessedPrefsKey}' with value '{Value}'");
			}

			OnValueChanged.InvokeSafe(Value);
		}

		public void SuppressNextValueChangedEvent(bool log)
		{
			if (log)
			{
				Log.Info($"Suppressing next value change event of '{ProcessedPrefsKey}'");
			}

			DontEmitNextValueChangedEvent = true;
		}

		#endregion

		#region Saving, Loading and Comparing Values

		protected abstract T InternalGetValue();
		protected abstract void InternalSetValue(T value);
		protected abstract bool IsSame(T oldValue, T newValue);

		#endregion

		#region Log

		private EditorPrefLogOptions LogOptions;

		private static readonly Logger Log = new("EditorPrefs");

		#endregion
	}

}