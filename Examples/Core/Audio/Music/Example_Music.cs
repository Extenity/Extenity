#if ExtenityAudio

using Extenity.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace ExtenityExamples.Audio
{

	public class Example_Music : MonoBehaviour
	{
		[Header("Track Event Names")]
		public string EventName_Music1 = "Track-1";
		public string EventName_Music2 = "Track-2";
		public string EventName_Music3 = "Track-3";

		public void PlayMusic(string eventName)
		{
			AudioManager.PlayMusic(eventName,
				LoopToggle.isOn,
				float.Parse(CrossfadeDurationInputField.text),
				CrossfadeStartVolumeSlider.value,
				VolumeSlider.value,
				float.Parse(PitchInputField.text));
		}

		public void PlayMusic1()
		{
			PlayMusic(EventName_Music1);
		}

		public void PlayMusic2()
		{
			PlayMusic(EventName_Music2);
		}

		public void PlayMusic3()
		{
			PlayMusic(EventName_Music3);
		}

		#region UI

		[Header("UI")]
		public Toggle LoopToggle;
		public InputField CrossfadeDurationInputField;
		public Slider CrossfadeStartVolumeSlider;
		public Text CrossfadeStartVolumeSliderValueText;
		public Slider VolumeSlider;
		public Text VolumeSliderValueText;
		public InputField PitchInputField;

		private void Awake()
		{
			UpdateSliderValues(0f);
			CrossfadeStartVolumeSlider.onValueChanged.AddListener(UpdateSliderValues);
			VolumeSlider.onValueChanged.AddListener(UpdateSliderValues);
		}

		private void UpdateSliderValues(float dummy)
		{
			CrossfadeStartVolumeSliderValueText.text = CrossfadeStartVolumeSlider.value.ToString("N2");
			VolumeSliderValueText.text = VolumeSlider.value.ToString("N2");
		}

		#endregion
	}

}

#endif
