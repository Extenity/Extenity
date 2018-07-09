using Extenity.DataToolbox;
using UnityEngine;
using UnityEngine.UI;

namespace Extenity.BeyondAudio.UI
{

	public class ButtonClickSound : MonoBehaviour
	{
		public Button Button;
		public Toggle Toggle;
		public string EventName = "ButtonClick";

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
			if (Button)
			{
				if (IsLoggingEnabled)
					Debug.Log($"Registering '{typeof(Button).Name}' click sound for '{gameObject.name}'.", this);
				Button.onClick.RemoveListener(OnClick);
				Button.onClick.AddListener(OnClick);
			}
			else if (Toggle)
			{
				if (IsLoggingEnabled)
					Debug.Log($"Registering '{typeof(Toggle).Name}' click sound for '{gameObject.name}'.", this);
				Toggle.onValueChanged.RemoveListener(OnValueChanged);
				Toggle.onValueChanged.AddListener(OnValueChanged);
			}
		}

		private void DeregisterEvents()
		{
			if (Button)
			{
				if (IsLoggingEnabled)
					Debug.Log($"Deregistering '{typeof(Button).Name}' click sound for '{gameObject.name}'.", this);
				Button.onClick.RemoveListener(OnClick);
			}
			else if (Toggle)
			{
				if (IsLoggingEnabled)
					Debug.Log($"Deregistering '{typeof(Toggle).Name}' click sound for '{gameObject.name}'.", this);
				Toggle.onValueChanged.RemoveListener(OnValueChanged);
			}
		}

		private void OnValueChanged(bool dummy)
		{
			if (IsLoggingEnabled)
				Debug.Log($"Playing click sound of '{gameObject.name}'.", this);
			AudioManager.Play(EventName);
		}

		private void OnClick()
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
