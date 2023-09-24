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
			PrefsKey             = prefsKey;
			_AppendPathHashToKey = appendPathHashToKey;
			_Value               = default;
			DefaultValueMethod  = defaultValueMethod;
			_LogOptions          = logOptions;
		}

		#endregion

		#region Key

		public readonly string PrefsKey;
		private readonly PathHashPostfix _AppendPathHashToKey;

		private string _ProcessedPrefsKey;

		public string ProcessedPrefsKey
		{
			get
			{
				if (string.IsNullOrEmpty(_ProcessedPrefsKey))
				{
					_ProcessedPrefsKey = PlayerPrefsTools.GenerateKey(PrefsKey, _AppendPathHashToKey);
				}

				return _ProcessedPrefsKey;
			}
		}

		#endregion

		#region Default Value

		private readonly DefaultValueMethod<T> DefaultValueMethod;

		#endregion

		#region Value

		private bool _IsInitialized;
		protected T _Value;

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
				if (!_IsInitialized)
				{
					_IsInitialized = true;
					if (!EditorPrefs.HasKey(ProcessedPrefsKey))
					{
						// Get default value
						switch (DefaultValueMethod.Type)
						{
							case DefaultValueMethodType.DefaultValue:
								_Value = DefaultValueMethod.DefaultValue;
								break;
							case DefaultValueMethodType.OverrideFunction:
								_Value = DefaultValueMethod.FunctionToGetDefaultValue(this);
								break;
							case DefaultValueMethodType.EnsureKeyExists:
								throw new Exception($"Reading EditorPref '{ProcessedPrefsKey}' failed. Key does not exist.");
							default:
								throw new ArgumentOutOfRangeException();
						}

						if (_LogOptions.HasFlag(EditorPrefLogOptions.LogOnRead))
							Log.Info($"Reading EditorPref '{ProcessedPrefsKey}'. The preference was not saved before, so the value is initialized with default '{_Value}'.");
						return _Value;
					}
					else
					{
						_Value = InternalGetValue();
					}
				}

				if (_LogOptions.HasFlag(EditorPrefLogOptions.LogOnRead))
					Log.Info($"Reading EditorPref '{ProcessedPrefsKey}'. The value is '{_Value}'.");
				return _Value;
			}
			set
			{
				if (_IsInitialized)
				{
					var oldValue = Value; // This must be called before setting _IsInitialized to true;
					if (IsSame(oldValue, value))
					{
						if (_LogOptions.HasFlag(EditorPrefLogOptions.LogOnWriteWhenNotChanged))
							Log.Info($"Writing EditorPref '{ProcessedPrefsKey}'. The value is not changed from '{oldValue}'.");

						return; // Nothing to do here. The value is not changed.
					}
					else
					{
						if (_LogOptions.HasFlag(EditorPrefLogOptions.LogOnWriteWhenChanged))
							Log.Info($"Writing EditorPref '{ProcessedPrefsKey}'. The value is changed from '{oldValue}' to '{value}'.");
					}
				}
				else
				{
					if (_LogOptions.HasFlag(EditorPrefLogOptions.LogOnWriteWhenChanged))
						Log.Info($"Writing EditorPref '{ProcessedPrefsKey}'. The value is initialized as '{value}'.");

					_IsInitialized = true;
				}

				_Value = value;
				InternalSetValue(value);

				if (_DontEmitNextValueChangedEvent)
				{
					if (_LogOptions.HasFlag(EditorPrefLogOptions.LogOnWriteWhenChanged))
						Log.Info($"Value change event of '{ProcessedPrefsKey}' is suppressed.");

					_DontEmitNextValueChangedEvent = false;
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

		private bool _DontEmitNextValueChangedEvent;

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

			_DontEmitNextValueChangedEvent = true;
		}

		#endregion

		#region Saving, Loading and Comparing Values

		protected abstract T InternalGetValue();
		protected abstract void InternalSetValue(T value);
		protected abstract bool IsSame(T oldValue, T newValue);

		#endregion

		#region Log

		private EditorPrefLogOptions _LogOptions;

		private static readonly Logger Log = new("EditorPrefs");

		#endregion
	}

}