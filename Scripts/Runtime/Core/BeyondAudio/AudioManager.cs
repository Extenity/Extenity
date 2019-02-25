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

		public static void ReleaseAudioSource(ref GameObject audioSource, string stopEventName = null) { if (EnsureIntegrity()) Instance.ReleaseAudioSource(ref audioSource, stopEventName); }

		#endregion

		#region Play One Shot

		public static void Play(string eventName) { if (EnsureIntegrity()) Instance.Play(eventName); }
		public static void Play(string eventName, GameObject associatedObject) { if (EnsureIntegrity()) Instance.Play(eventName, associatedObject); }
		public static GameObject PlayAtPosition(string eventName, Vector3 worldPosition) { if (EnsureIntegrity()) return Instance.PlayAtPosition(eventName, worldPosition); return null; }
		public static GameObject PlayAttached(string eventName, Transform parent, Vector3 localPosition) { if (EnsureIntegrity()) return Instance.PlayAttached(eventName, parent, localPosition); return null; }

		#endregion

		#region Play Music

		public static void PlayMusic(string eventName) { if (EnsureIntegrity()) Instance.PlayMusic(eventName); }

		#endregion

		#region Parameter

		public static float GetFloat(string rtpcName) { if (EnsureIntegrity()) return Instance.GetFloat(rtpcName); return float.NaN; }
		public static float GetFloat(string rtpcName, GameObject associatedObject) { if (EnsureIntegrity()) return Instance.GetFloat(rtpcName, associatedObject); return float.NaN; }
		public static void SetFloat(string rtpcName, float value) { if (EnsureIntegrity()) Instance.SetFloat(rtpcName, value); }
		public static void SetFloat(string rtpcName, float value, GameObject associatedObject) { if (EnsureIntegrity()) Instance.SetFloat(rtpcName, value, associatedObject); }

		#endregion
	}

}
