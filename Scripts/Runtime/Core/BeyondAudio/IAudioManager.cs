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

		void ReleaseAudioSource(ref GameObject audioSource, uint stopEventID = 0);

		#endregion

		#region Play One Shot

		void Play(uint eventID);
		void Play(uint eventID, GameObject associatedObject);
		GameObject PlayAtPosition(uint eventID, Vector3 worldPosition);
		GameObject PlayAttached(uint eventID, Transform parent, Vector3 localPosition);

		#endregion

		#region Play Music
		
		void PlayMusic(uint eventID);

		#endregion

		#region Parameter

		float GetFloat(uint rtpcID);
		float GetFloat(uint rtpcID, GameObject associatedObject);
		void SetFloat(uint rtpcID, float value);
		void SetFloat(uint rtpcID, float value, GameObject associatedObject);

		#endregion
	}

}
