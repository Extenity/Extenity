#if ExtenityAudio

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ExtenityExamples.Audio
{

	public class Example_ActiveAudioDisplayItem : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
	{
		public Text EventName;
		public Text ClipName;
		public Slider VolumeSlider;
		public Text VolumeSliderValueText;
		public Slider PitchSlider;
		public Text PitchSliderValueText;
		public GameObject ReleasedIndicator;
		public GameObject ErrorIndicator;

		[NonSerialized]
		public AudioSource AudioSource;
		[NonSerialized]
		public bool IsReleased = false;

		private void Awake()
		{
			ReleasedIndicator.gameObject.SetActive(false);
			ErrorIndicator.gameObject.SetActive(false);
		}

		private void LateUpdate()
		{
			UpdateValues();
		}

		public void InformReleased()
		{
			if (IsReleased)
				throw new Exception();
			IsReleased = true;
			ReleasedIndicator.gameObject.SetActive(true);
		}

		public void Set(AudioSource audioSource, string eventName)
		{
			AudioSource = audioSource;
			EventName.text = eventName;
		}

		private void UpdateValues()
		{
			if (IsReleased)
				return;

			if (AudioSource)
			{
				var volume = AudioSource.volume;
				var pitch = AudioSource.pitch;

				VolumeSlider.maxValue = volume > 1f ? volume : 1f;
				VolumeSlider.minValue = volume < 0f ? volume : 0f;
				VolumeSlider.value = volume;
				VolumeSliderValueText.text = volume.ToString("N2");

				PitchSlider.maxValue = pitch > 1f ? pitch : 1f;
				PitchSlider.minValue = pitch < 0f ? pitch : 0f;
				PitchSlider.value = pitch;
				PitchSliderValueText.text = pitch.ToString("N2");

				var clip = AudioSource.clip;
				ClipName.text = clip ? clip.name : "[No Clip]";
			}
			else
			{
				ErrorIndicator.gameObject.SetActive(true);
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (AudioSource)
			{
#if UNITY_EDITOR
				UnityEditor.EditorGUIUtility.PingObject(AudioSource);
#endif
			}
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (AudioSource)
			{
#if UNITY_EDITOR
				UnityEditor.Selection.activeGameObject = AudioSource.gameObject;
#endif
			}
		}
	}

}

#endif
