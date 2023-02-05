//#define EnablePlayerPrefLogging

using System;
using System.Diagnostics;
using Extenity.MessagingToolbox;
using JetBrains.Annotations;
#if UNITY
using UnityEngine;
#endif

namespace Extenity.DataToolbox
{

	public abstract class PlayerPref<T>
	{
		#region Initialization

		public PlayerPref([NotNull]string prefsKey, PathHashPostfix appendPathHashToKey, T defaultValue, Func<PlayerPref<T>, T> defaultValueOverride, Func<T, T> valueTransform, float saveDelay)
		{
			PrefsKey = prefsKey;
			_AppendPathHashToKey = appendPathHashToKey;
			_Value = defaultValue;
			_DefaultValueOverride = defaultValueOverride;
			_ValueTransform = valueTransform;
			SaveDelay = saveDelay;
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

		private readonly Func<PlayerPref<T>, T> _DefaultValueOverride;

		#endregion

		#region Value

#if UNITY
		private bool _IsInitialized;
#endif
		protected T _Value;
		public T Value
		{
			get
			{
#if UNITY
				if (!_IsInitialized)
				{
					_IsInitialized = true;
					if (!PlayerPrefs.HasKey(ProcessedPrefsKey))
					{
						if (_DefaultValueOverride != null)
						{
							_Value = TransformValue(_DefaultValueOverride(this));
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
						_Value = TransformValue(InternalGetValue());
						LogInfo($"Initialized value as '{_Value}'");
					}
				}
				LogInfo($"Got the value '{_Value}'");
				return _Value;
#else
				throw new System.NotImplementedException();
#endif
			}
			set
			{
#if UNITY
				var transformedValue = TransformValue(value);
				if (_IsInitialized)
				{
					var oldValue = Value; // This must be called before setting _IsInitialized to true;
					LogInfo($"Setting value to '{transformedValue}' which previously was '{oldValue}'");
					if (IsSame(oldValue, transformedValue))
						return;
				}
				else
				{
					LogInfo($"Setting value to '{transformedValue}' <b>as initialization</b>");
					_IsInitialized = true;
				}

				_Value = transformedValue;
				InternalSetValue(transformedValue);

				if (SaveDelay > 0f)
				{
					LogInfo($"Saving deferred for '{SaveDelay}' seconds");
					DeferredSave(SaveDelay);
				}
				else
				{
					Save();
				}

				if (_DontEmitNextValueChangedEvent)
				{
					LogInfo("Skipping value change event");
					_DontEmitNextValueChangedEvent = false;
				}
				else
					OnValueChanged.InvokeSafe(transformedValue);
#else
				throw new System.NotImplementedException();
#endif
			}
		}

		#endregion

		#region Value Transform

		private readonly Func<T, T> _ValueTransform;

		private T TransformValue(T value)
		{
			return _ValueTransform == null
				? value
				: _ValueTransform(value);
		}

		#endregion

		#region Value Changed Event

		public class ValueChangedEvent : ExtenityEvent<T> { }
		public readonly ValueChangedEvent OnValueChanged = new ValueChangedEvent();

#pragma warning disable CS0414
		private bool _DontEmitNextValueChangedEvent;
#pragma warning restore CS0414

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

		#region Deferred Saving

		public float SaveDelay = 0f;

		public void Save()
		{
#if UNITY
            UnityEngine.PlayerPrefs.Save();
#else
			throw new System.NotImplementedException();
#endif
		}

		public void DeferredSave(float saveDelay)
		{
			PlayerPrefsTools.DeferredSave(saveDelay);
		}

		#endregion

		#region Log

#if EnablePlayerPrefLogging

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

		[Conditional("EnablePlayerPrefLogging")]
		private void LogInfo(string message)
		{
#if EnablePlayerPrefLogging
			Log.Info(LogPrefix + message);
#endif
		}

		//[Conditional("EnablePlayerPrefLogging")] Do not uncomment this. Always show errors.
		private void LogError(string message)
		{
#if EnablePlayerPrefLogging
			Log.Error(LogPrefix + message);
#endif
		}

		#endregion
	}

}
