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

	public abstract class EditorPref<T>
	{
		#region Initialization

		public EditorPref([NotNull] string prefsKey, PathHashPostfix appendPathHashToKey, T defaultValue, Func<EditorPref<T>, T> defaultValueOverride, EditorPrefLogOptions logOptions)
		{
			PrefsKey              = prefsKey;
			_AppendPathHashToKey  = appendPathHashToKey;
			_Value                = defaultValue;
			_DefaultValueOverride = defaultValueOverride;
			_LogOptions           = logOptions;
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

		private readonly Func<EditorPref<T>, T> _DefaultValueOverride;

		#endregion

		#region Value

		private bool _IsInitialized;
		protected T _Value;

		public T Value
		{
			get
			{
				if (!_IsInitialized)
				{
					_IsInitialized = true;
					if (!EditorPrefs.HasKey(ProcessedPrefsKey))
					{
						if (_DefaultValueOverride != null)
						{
							_Value = _DefaultValueOverride(this);
						}
						else
						{
							// Default value was already assigned to _Value at construction time. Nothing to do here.
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

		#endregion

		#region Log

		private static readonly Logger Log = new("EditorPrefs");

		#endregion
	}

}