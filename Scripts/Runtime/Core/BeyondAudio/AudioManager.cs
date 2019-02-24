using Extenity.ApplicationToolbox;
using UnityEngine;

namespace Extenity.BeyondAudio
{

	public static class AudioManager
	{
		#region Instance

		private static IAudioManager Instance;
		private static MonoBehaviour InstanceAsMonoBehaviour;

		/// <summary>
		/// Tells if the audio manager is available for use, like if it's created and if the application is not
		/// currently shutting down.
		///
		/// Can be (and should be) used for checking if the audio manager is still available while deinitializing
		/// an object which accesses the audio manager.
		/// </summary>
		/// <returns>False if audio manager is not available and should not be accessed.</returns>
		public static bool EnsureIntegrity()
		{
			if (!InstanceAsMonoBehaviour)
			{
				if (!ApplicationTools.IsShuttingDown)
				{
					Log.CriticalError("AudioManager is not available.");
				}
				return false;
			}
			return true;
		}

		public static void _SetManager(IAudioManager instance)
		{
			Instance = instance;
			InstanceAsMonoBehaviour = (MonoBehaviour)instance;
		}

		#endregion

		#region Device Volume

		public static bool IsDeviceVolumeSupported { get { if (EnsureIntegrity()) return Instance.IsDeviceVolumeSupported; return false; } }
		public static float GetDeviceVolumeNormalized() { if (EnsureIntegrity()) return Instance.GetDeviceVolumeNormalized(); return 0f; }
		public static float SetDeviceVolumeNormalized(float normalizedVolume) { if (EnsureIntegrity()) return Instance.SetDeviceVolumeNormalized(normalizedVolume); return 0f; }

		#endregion

		#region Master Audio Mixer and Music/Effect Volumes

		public static VolumeControl GetVolumeControl(string mixerParameterName) { if (EnsureIntegrity()) return Instance.GetVolumeControl(mixerParameterName); return null; }

		#endregion

		#region Pooled Audio Sources

		public static void ReleaseAudioSource(ref GameObject audioSource, uint stopEventID = 0) { if (EnsureIntegrity()) Instance.ReleaseAudioSource(ref audioSource, stopEventID); }

		#endregion

		#region Play One Shot

		public static void Play(uint eventID) { if (EnsureIntegrity()) Instance.Play(eventID); }
		public static void Play(uint eventID, GameObject associatedObject) { if (EnsureIntegrity()) Instance.Play(eventID, associatedObject); }
		public static GameObject PlayAtPosition(uint eventID, Vector3 worldPosition) { if (EnsureIntegrity()) return Instance.PlayAtPosition(eventID, worldPosition); return null; }
		public static GameObject PlayAttached(uint eventID, Transform parent, Vector3 localPosition) { if (EnsureIntegrity()) return Instance.PlayAttached(eventID, parent, localPosition); return null; }

		#endregion

		#region Play Music

		public static void PlayMusic(uint eventID) { if (EnsureIntegrity()) Instance.PlayMusic(eventID); }

		#endregion

		#region Parameter

		public static float GetFloat(uint rtpcID) { if (EnsureIntegrity()) return Instance.GetFloat(rtpcID); return float.NaN; }
		public static float GetFloat(uint rtpcID, GameObject associatedObject) { if (EnsureIntegrity()) return Instance.GetFloat(rtpcID, associatedObject); return float.NaN; }
		public static void SetFloat(uint rtpcID, float value) { if (EnsureIntegrity()) Instance.SetFloat(rtpcID, value); }
		public static void SetFloat(uint rtpcID, float value, GameObject associatedObject) { if (EnsureIntegrity()) Instance.SetFloat(rtpcID, value, associatedObject); }

		#endregion
	}

}
