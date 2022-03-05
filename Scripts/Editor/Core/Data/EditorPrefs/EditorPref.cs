//#define EnableEditorPrefLogging

using System;
using System.Diagnostics;
using Extenity.MessagingToolbox;
using JetBrains.Annotations;
using UnityEditor;

namespace Extenity.DataToolbox.Editor
{

	public abstract class EditorPref<T>
	{
		#region Initialization

		public EditorPref([NotNull]string prefsKey, PathHashPostfix appendPathHashToKey, T defaultValue, Func<EditorPref<T>, T> defaultValueOverride)
		{
			PrefsKey = prefsKey;
			_AppendPathHashToKey = appendPathHashToKey;
			_Value = defaultValue;
			_DefaultValueOverride = defaultValueOverride;
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
							LogInfo($"Initialized value from override as '{_Value}'");
						}
						else
						{
							// Default value was already assigned to _Value at construction time. Nothing to do here.
							LogInfo($"Initialized value as default '{_Value}'");
						}
					}
					else
					{
						_Value = InternalGetValue();
						LogInfo($"Initialized value as '{_Value}'");
					}
				}
				LogInfo($"Got the value '{_Value}'");
				return _Value;
			}
			set
			{
				if (_IsInitialized)
				{
					var oldValue = Value; // This must be called before setting _IsInitialized to true;
					LogInfo($"Setting value to '{value}' which previously was '{oldValue}'");
					if (IsSame(oldValue, value))
						return;
				}
				else
				{
					LogInfo($"Setting value to '{value}' <b>as initialization</b>");
					_IsInitialized = true;
				}

				_Value = value;
				InternalSetValue(value);

				if (_DontEmitNextValueChangedEvent)
				{
					LogInfo("Skipping value change event");
					_DontEmitNextValueChangedEvent = false;
				}
				else
					OnValueChanged.InvokeSafe(value);
			}
		}

		#endregion

		#region Value Changed Event

		public class ValueChangedEvent : ExtenityEvent<T> { }
		public readonly ValueChangedEvent OnValueChanged = new ValueChangedEvent();

		private bool _DontEmitNextValueChangedEvent;

		public void AddOnValueChangedListenerAndInvoke(Action<T> listener)
		{
			if (listener == null)
				throw new ArgumentNullException();

			OnValueChanged.AddListener(listener);
			listener.Invoke(Value);
		}

		public void InvokeValueChanged()
		{
			LogInfo("Invoking value change event");
			OnValueChanged.InvokeSafe(Value);
		}

		public void SuppressNextValueChangedEvent()
		{
			LogInfo("Suppressing next value change event");
			_DontEmitNextValueChangedEvent = true;
		}

		#endregion

		#region Saving, Loading and Comparing Values

		protected abstract T InternalGetValue();
		protected abstract void InternalSetValue(T value);
		protected abstract bool IsSame(T oldValue, T newValue);

		#endregion

		#region Log

#if EnableEditorPrefLogging

		private string _LogPrefix;
		private string LogPrefix
		{
			get
			{
				if (_LogPrefix == null)
					_LogPrefix = $"|Pref-{ProcessedPrefsKey}|";
				return _LogPrefix;
			}
		}

#endif

		[Conditional("EnableEditorPrefLogging")]
		private void LogInfo(string message)
		{
#if EnableEditorPrefLogging
			Log.Info(LogPrefix + message);
#endif
		}

		//[Conditional("EnableEditorPrefLogging")] Do not uncomment this. Always show errors.
		private void LogError(string message)
		{
#if EnableEditorPrefLogging
			Log.Error(LogPrefix + message);
#endif
		}

		#endregion
	}

}
