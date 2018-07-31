using System;
using Extenity.DataToolbox;
using Extenity.UIToolbox;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Extenity.BeyondAudio.UI
{

	public enum ButtonClickSoundAction
	{
		Up,
		Down,
	}

	public class ButtonClickSound : MonoBehaviour
	{
		public Button Button;
		public Toggle Toggle;
		public string EventName = "ButtonClick";
		public ButtonClickSoundAction Action = ButtonClickSoundAction.Up;

		public static bool IsTypeSupported<T>()
		{
			return
				typeof(T) == typeof(Button) ||
				typeof(T) == typeof(Toggle);
		}

		protected void OnEnable()
		{
			// Wow! RegisterEvents does not get called on some buttons for some reason. Don't know why, and don't care for now. It's not about FastInvoke because Invoke does not work too.
			//// Allow everything to be initialized first. The user would not likely press the button in 100 ms.
			//this.FastInvoke("RegisterEvents", 0.1f);

			RegisterEvents();
		}

		protected void OnDisable()
		{
			DeregisterEvents();
		}

		private void RegisterEvents()
		{
			if (string.IsNullOrEmpty(EventName))
				return; // Do not even bother registering to events.

			if (Button)
			{
				if (IsLoggingEnabled)
					Debug.Log($"Registering '{typeof(Button).Name}' click sound for '{gameObject.name}'.", this);
				switch (Action)
				{
					case ButtonClickSoundAction.Up:
						Button.onClick.RemoveListener(OnClick);
						Button.onClick.AddListener(OnClick);
						break;
					case ButtonClickSoundAction.Down:
						Button.DeregisterFromEvent(EventTriggerType.PointerDown, OnCustom);
						Button.RegisterToEvent(EventTriggerType.PointerDown, OnCustom);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			else if (Toggle)
			{
				if (IsLoggingEnabled)
					Debug.Log($"Registering '{typeof(Toggle).Name}' click sound for '{gameObject.name}'.", this);
				switch (Action)
				{
					case ButtonClickSoundAction.Up:
						Toggle.onValueChanged.RemoveListener(OnValueChanged);
						Toggle.onValueChanged.AddListener(OnValueChanged);
						break;
					case ButtonClickSoundAction.Down:
						Toggle.DeregisterFromEvent(EventTriggerType.PointerDown, OnCustom);
						Toggle.RegisterToEvent(EventTriggerType.PointerDown, OnCustom);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		private void DeregisterEvents()
		{
			if (Button)
			{
				if (IsLoggingEnabled)
					Debug.Log($"Deregistering '{typeof(Button).Name}' click sound for '{gameObject.name}'.", this);
				switch (Action)
				{
					case ButtonClickSoundAction.Up:
						Button.onClick.RemoveListener(OnClick);
						break;
					case ButtonClickSoundAction.Down:
						Button.DeregisterFromEvent(EventTriggerType.PointerDown, OnCustom);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			else if (Toggle)
			{
				if (IsLoggingEnabled)
					Debug.Log($"Deregistering '{typeof(Toggle).Name}' click sound for '{gameObject.name}'.", this);
				switch (Action)
				{
					case ButtonClickSoundAction.Up:
						Toggle.onValueChanged.RemoveListener(OnValueChanged);
						break;
					case ButtonClickSoundAction.Down:
						Toggle.DeregisterFromEvent(EventTriggerType.PointerDown, OnCustom);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		private void OnValueChanged(bool dummy)
		{
			Play();
		}

		private void OnClick()
		{
			Play();
		}

		private void OnCustom(BaseEventData dummy)
		{
			Play();
		}

		public void Play()
		{
			if (IsLoggingEnabled)
				Debug.Log($"Playing click sound of '{gameObject.name}'.", this);
			AudioManager.Play(EventName);
		}

		protected void OnValidate()
		{
			// Need to check for both at the same time. We are interested in triggering
			// a heavy GetComponent check only if all of the references are missing.
			if (!Button && !Toggle)
			{
				Button = GetComponent<Button>();
				Toggle = GetComponent<Toggle>();
			}
		}

		#region Log

		private const string LoggingPrefKey = "EnableButtonClickSoundLogging";

		private static bool _IsLoggingInitialized;

		private static bool _IsLoggingEnabled;
		public static bool IsLoggingEnabled
		{
			get
			{
				if (!_IsLoggingInitialized)
				{
					_IsLoggingEnabled = PlayerPrefsTools.GetBool(LoggingPrefKey, false);
					_IsLoggingInitialized = true;
				}
				return _IsLoggingEnabled;
			}
			set
			{
				_IsLoggingEnabled = value;
				_IsLoggingInitialized = true;
				if (value)
				{
					PlayerPrefsTools.SetBool(LoggingPrefKey, value);
				}
				else
				{
					// This is a debugging tool. No need to keep the registry key around.
					PlayerPrefs.DeleteKey(LoggingPrefKey);
				}
				PlayerPrefs.Save();
			}
		}

		#endregion
	}

}
