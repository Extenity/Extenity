using UnityEngine;

namespace Extenity.BeyondAudio
{

	public interface IAudioManager
	{
		#region Device Volume

		bool IsDeviceVolumeSupported { get; }
		float GetDeviceVolumeNormalized();
		float SetDeviceVolumeNormalized(float normalizedVolume);

		#endregion

		#region Master Audio Mixer and Music/Effect Volumes

		VolumeControl GetVolumeControl(string mixerParameterName);

		#endregion

		#region Pooled Audio Sources

		void ReleaseAudioSource(ref GameObject audioSource, string stopEventName = null);

		#endregion

		#region Play One Shot

		void Play(string eventName);
		void Play(string eventName, GameObject associatedObject);
		GameObject PlayAtPosition(string eventName, Vector3 worldPosition);
		GameObject PlayAttached(string eventName, Transform parent, Vector3 localPosition);

		#endregion

		#region Play Music
		
		void PlayMusic(string eventName);

		#endregion

		#region Parameter

		float GetFloat(string rtpcName);
		float GetFloat(string rtpcName, GameObject associatedObject);
		void SetFloat(string rtpcName, float value);
		void SetFloat(string rtpcName, float value, GameObject associatedObject);

		#endregion
	}

}
