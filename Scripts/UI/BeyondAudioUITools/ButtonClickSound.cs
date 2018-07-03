using UnityEngine;
using UnityEngine.UI;

namespace Extenity.BeyondAudio.UI
{

	public class ButtonClickSound : MonoBehaviour
	{
		public Button Button;
		public Toggle Toggle;
		public string EventName = "ButtonClick";

		public static bool IsLoggingEnabled;

		public static bool IsTypeSupported<T>()
		{
			return
				typeof(T) == typeof(Button) ||
				typeof(T) == typeof(Toggle);
		}

		private void OnEnable()
		{
			if (Button)
			{
				if (IsLoggingEnabled)
					Debug.Log($"Registering '{typeof(Button).Name}' click sound for '{gameObject.name}'.", this);
				Button.onClick.AddListener(OnClick);
			}
			else if (Toggle)
			{
				if (IsLoggingEnabled)
					Debug.Log($"Registering '{typeof(Toggle).Name}' click sound for '{gameObject.name}'.", this);
				Toggle.onValueChanged.AddListener(OnValueChanged);
			}
		}

		private void OnDisable()
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

		private void OnValidate()
		{
			// Need to check for both at the same time. We are interested in triggering
			// a heavy GetComponent check only if all of the references are missing.
			if (!Button && !Toggle)
			{
				Button = GetComponent<Button>();
				Toggle = GetComponent<Toggle>();
			}
		}
	}

}
