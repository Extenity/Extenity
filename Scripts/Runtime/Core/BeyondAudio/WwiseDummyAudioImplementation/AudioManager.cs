#if BeyondAudioUsesWwiseDummyAudio

using Extenity.DesignPatternsToolbox;
using UnityEngine;

namespace Extenity.BeyondAudio
{

	public class AudioManager : SingletonUnity<AudioManager>
	{
		#region Singleton

		private static AudioManager InstanceEnsured
		{
			get
			{
				var instance = Instance;
				if (!instance && !IsShuttingDown)
				{
					Log.CriticalError("AudioManager is not initialized yet.");
				}
				return instance;
			}
		}

		#endregion

		#region Initialization

		private void Awake()
		{
			InitializeSingleton(true);
			Log.RegisterPrefix(this, "Audio");
		}

		#endregion

		#region Deinitialization

		internal static bool IsShuttingDown;

		private void OnApplicationQuit()
		{
			IsShuttingDown = true;
		}

		#endregion

		#region Device Volume

		public bool IsDeviceVolumeSupported => false;

		public float GetDeviceVolumeNormalized()
		{
			throw new System.NotImplementedException();
		}

		public float SetDeviceVolumeNormalized(float normalizedVolume)
		{
			throw new System.NotImplementedException();
		}

		#endregion

		#region Master Audio Mixer and Music/Effect Volumes

		public void SetVolumeLoggingForAllControllers(bool isLoggingEnabled)
		{
		}

		public VolumeControl GetVolumeControl(string mixerParameterName)
		{
			return null;
		}

		#endregion

		#region Volume Adjustment Conversion

		public static float DbToNormalizedRange(float db)
		{
			return float.NaN;
		}

		public static float NormalizedToDbRange(float normalized)
		{
			return float.NaN;
		}

		#endregion

		#region Pooled AudioClips

		public static void ReleaseAudioSource(ref GameObject audioSource, string stopEventName = null)
		{
		}

		#endregion

		#region Play One Shot

		public static void Play(string eventName)
		{
		}

		public static void Play(string eventName, GameObject associatedObject)
		{
		}

		public static GameObject PlayAtPosition(string eventName, Vector3 worldPosition)
		{
			return null;
		}

		public static GameObject PlayAttached(string eventName, Transform parent, Vector3 localPosition)
		{
			return null;
		}

		#endregion

		#region Play Music

		public static void PlayMusic(string eventName)
		{
		}

		public static void SetMusicState(string stateGroup, string state)
		{
		}

		#endregion

		#region RTPC

		public static float GetRTPCValue(string rtpcName)
		{
			return float.NaN;
		}

		public static float GetRTPCValue(string rtpcName, GameObject associatedObject)
		{
			return float.NaN;
		}

		public static void SetRTPCValue(string rtpcName, float value)
		{
		}

		public static void SetRTPCValue(string rtpcName, float value, GameObject associatedObject)
		{
		}

		#endregion

		#region State

		public static void SetState(string stateGroup, string state)
		{
		}

		#endregion
	}

}

#endif
