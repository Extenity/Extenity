using UnityEngine;
using UnityEngine.UI;

namespace Extenity.BeyondAudio.UI
{

	public class ButtonClickSound : MonoBehaviour
	{
		public Button Button;
		public string EventName = "ButtonClick";

		private void Start()
		{
			Button.onClick.AddListener(OnClick);
		}

		private void OnClick()
		{
			AudioManager.Play(EventName);
		}

		private void OnValidate()
		{
			if (!Button)
			{
				Button = GetComponent<Button>();
			}
		}
	}

}
