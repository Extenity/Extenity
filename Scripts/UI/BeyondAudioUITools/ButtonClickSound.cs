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

		private void Start()
		{
			if (Button)
			{
				Button.onClick.AddListener(OnClick);
			}
			else if (Toggle)
			{
				Toggle.onValueChanged.AddListener(OnValueChanged);
			}
		}

		private void OnValueChanged(bool dummy)
		{
			AudioManager.Play(EventName);
		}

		private void OnClick()
		{
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
