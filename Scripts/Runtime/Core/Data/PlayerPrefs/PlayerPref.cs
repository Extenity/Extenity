using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace Extenity.DataToolbox
{

	public abstract class PlayerPref<T>
	{
		#region Initialization

		public PlayerPref([NotNull]string prefsKey, PathHashPostfix appendPathHashToKey, T defaultValue, Action<PlayerPref<T>> defaultValueOverride, float saveDelay)
		{
			PrefsKey = prefsKey;
			_AppendPathHashToKey = appendPathHashToKey;
			_Value = defaultValue;
			_DefaultValueOverride = defaultValueOverride;
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

		private readonly Action<PlayerPref<T>> _DefaultValueOverride;

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
					if (!PlayerPrefs.HasKey(ProcessedPrefsKey))
					{
						if (_DefaultValueOverride != null)
						{
							_DefaultValueOverride(this);
						}
						//else
						//{
						//	Default value was already assigned to _Value at construction time.
						//}
					}
					else
					{
						_Value = (T)InternalGetValue();
					}
				}
				return _Value;
			}
			set
			{
				_IsInitialized = true;
				if (IsSame(Value, value))
					return;
				InternalSetValue(value);

				if (SaveDelay > 0f)
				{
					PlayerPrefsTools.DeferredSave(SaveDelay);
				}
				else
				{
					PlayerPrefs.Save();
				}

				if (_DontEmitNextValueChangedEvent)
					_DontEmitNextValueChangedEvent = false;
				else
					OnValueChanged.Invoke(value);
			}
		}


		#endregion

		#region Value Changed Event

		public class ValueChangedEvent : UnityEvent<T> { }
		public readonly ValueChangedEvent OnValueChanged = new ValueChangedEvent();

		private bool _DontEmitNextValueChangedEvent;

		public void SuppressNextValueChangedEvent()
		{
			_DontEmitNextValueChangedEvent = true;
		}

		public void InvokeValueChanged()
		{
			OnValueChanged.Invoke(Value);
		}

		#endregion

		#region Saving, Loading and Comparing Values

		protected abstract object InternalGetValue();
		protected abstract void InternalSetValue(object value);
		protected abstract bool IsSame(T oldValue, T newValue);

		#endregion

		#region Deferred Saving

		public float SaveDelay = 0f;

		#endregion
	}

}
