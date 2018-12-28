#if BeyondAudioUsesWwiseAudio

using System.Collections.Generic;
using Extenity.DesignPatternsToolbox;
using Extenity.GameObjectToolbox;
using UnityEngine;

namespace Extenity.BeyondAudio
{

	public class AudioManager : SingletonUnity<AudioManager>
	{
		#region Configuration

		public static float CurrentTime { get { return Time.realtimeSinceStartup; } }

		public const float VolumeAdjustmentDb = -80f;
		private const int FreeAudioSourcesInitialCapacity = 25;
		private const int AudioSourceBagInitialCapacity = FreeAudioSourcesInitialCapacity * 4;

		#endregion

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
			InitializeSingleton(this, true);
			Log.RegisterPrefix(this, "Audio");
			InitializeAudioSourceTemplate();
		}

		private void Start()
		{
			// Quick fix for not properly initializing Unity's audio system.
			// Mixer parameters must be set in Start, instead of Awake.
			// Otherwise they will be ignored silently.
			InitializeVolumeControls();
		}

		#endregion

		#region Deinitialization

		internal static bool IsShuttingDown;

		private void OnApplicationQuit()
		{
			IsShuttingDown = true;
		}

		//protected void OnDestroy()
		//{
		//}

		#endregion

		#region Update

		private void Update()
		{
			UpdateReleaseTracker();
		}

		#endregion

		#region Device Volume

#if UNITY_EDITOR_WIN

		public bool IsDeviceVolumeSupported => false;

		public float GetDeviceVolumeNormalized()
		{
			throw new System.NotImplementedException();
		}

		public float SetDeviceVolumeNormalized(float normalizedVolume)
		{
			throw new System.NotImplementedException();
		}

#elif UNITY_ANDROID

		public bool IsDeviceVolumeSupported => false;

		public float GetDeviceVolumeNormalized()
		{
			throw new System.NotImplementedException();
		}

		public float SetDeviceVolumeNormalized(float normalizedVolume)
		{
			throw new System.NotImplementedException();
		}

		// TODO: There is only little left to complete this feature. But it takes so much time because every try needs a build on device.
		/*
		// Source: https://forum.unity.com/threads/accessing-and-changing-the-volume-of-the-device.135907/

		public bool IsDeviceVolumeSupported => true;

		private const int ANDROID_STREAM_MUSIC = 3;
		private const int ANDROID_SETSTREAMVOLUME_FLAGS = 0;

		private RangeInt GetDeviceVolumeRange()
		{
			Log.Info("## GetDeviceVolumeRange");
			var min = AndroidAudioService.Call<int>("getStreamMinVolume", ANDROID_STREAM_MUSIC);
			var max = AndroidAudioService.Call<int>("getStreamMaxVolume", ANDROID_STREAM_MUSIC);
			Log.Info($"Android device volume range: {min}-{max}", this);
			return new RangeInt(min, max - min);
		}

		private int GetDeviceVolume()
		{
			Log.Info("## GetDeviceVolume");
			var volume = AndroidAudioService.Call<int>("getStreamVolume", ANDROID_STREAM_MUSIC);
			Log.Info($"Android device volume: {volume}", this);
			return volume;
		}

		public float GetDeviceVolumeNormalized()
		{
			Log.Info("## GetDeviceVolumeNormalized");
			var range = GetDeviceVolumeRange();
			var volume = GetDeviceVolume();
			var normalizedVolume = (volume - range.start) / (float)range.length;
			Log.Info($"Android device normalized volume: {normalizedVolume}", this);
			return volume;
		}

		/// <summary>
		/// Returns the current volume after the value is set.
		/// </summary>
		private int SetDeviceVolume(int volume)
		{
			Log.Info("## SetDeviceVolume   volume: " + volume);
			AndroidAudioService.Call("setStreamVolume", ANDROID_STREAM_MUSIC, volume, ANDROID_SETSTREAMVOLUME_FLAGS);
			var newVolume = GetDeviceVolume();
			if (newVolume == volume)
			{
				Log.Info($"Android device volume set to: {newVolume}", this);
			}
			else
			{
				Log.Warning($"Failed to set Android device volume to '{volume}'. Currently '{newVolume}'", this);
			}
			return newVolume;
		}

		/// <summary>
		/// Returns the current volume after the value is set.
		/// </summary>
		public float SetDeviceVolumeNormalized(float normalizedVolume)
		{
			Log.Info("## SetDeviceVolumeNormalized   normalizedVolume: " + normalizedVolume);
			var range = GetDeviceVolumeRange();
			var volume = range.start + (int)(range.length * normalizedVolume);
			var newVolume = SetDeviceVolume(volume);
			var newNormalizedVolume = (newVolume - range.start) / (float)range.length;
			return newNormalizedVolume;
		}

		private AndroidJavaObject _AndroidAudioService;
		private AndroidJavaObject AndroidAudioService
		{
			get
			{
				if (_AndroidAudioService == null)
				{
					var up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
					var context = up.GetStatic<AndroidJavaObject>("currentActivity");
					
					// This seems to fail for some reason. Probably caused by Unity.
					// So we use the constant value of this static field, instead of
					// querying its value. See below:
					// https://forum.unity.com/threads/crash-after-call-androidjavaobject-getstatic-2018-1-0f2.531238/
					// https://developer.android.com/reference/android/content/Context#AUDIO_SERVICE
					//var AudioServiceName = context.GetStatic<string>("AUDIO_SERVICE");
					const string AudioServiceName = "audio";

					_AndroidAudioService = context.Call<AndroidJavaObject>("getSystemService", AudioServiceName);
				}
				if (_AndroidAudioService == null)
				{
					throw new System.Exception("Failed to get Android Audio Service.");
				}
				return _AndroidAudioService;
			}
		}
		*/

#elif UNITY_IOS

		ImplementThis;

#endif

		#endregion

		#region Master Audio Mixer and Music/Effect Volumes

		[Header("Audio Mixers and Volume Controls")]
		public VolumeControl[] MixerVolumeControls =
		{
			new VolumeControl { MixerParameterName = "MusicVolume" },
			new VolumeControl { MixerParameterName = "EffectsVolume" },
		};

		private void InitializeVolumeControls()
		{
			ReassignAllMixerVolumeControlParameters();
		}

		public void ReassignAllMixerVolumeControlParameters()
		{
			for (var i = 0; i < MixerVolumeControls.Length; i++)
			{
				MixerVolumeControls[i].ReassignMixerParameter();
			}
		}

		public void SetVolumeLoggingForAllControllers(bool isLoggingEnabled)
		{
			for (var i = 0; i < MixerVolumeControls.Length; i++)
			{
				MixerVolumeControls[i].LogMixerAndVolumeChanges = isLoggingEnabled;
			}
		}

		public VolumeControl GetVolumeControl(string mixerParameterName)
		{
			for (var i = 0; i < MixerVolumeControls.Length; i++)
			{
				var volumeControl = MixerVolumeControls[i];
				if (volumeControl.MixerParameterName == mixerParameterName)
					return volumeControl;
			}
			Log.CriticalError($"Volume control '{mixerParameterName}' does not exist.", this);
			return null;
		}

		#endregion

		#region Volume Adjustment Conversion

		public static float DbToNormalizedRange(float db)
		{
			var normalized = 1f - (db / VolumeAdjustmentDb);
			if (normalized > 1f)
				return 1f;
			if (normalized < 0f)
				return 0f;
			normalized = normalized * normalized * normalized;
			return normalized;
		}

		public static float NormalizedToDbRange(float normalized)
		{
			normalized = Mathf.Pow(normalized, 1f / 3f);
			var db = (1f - normalized) * VolumeAdjustmentDb;
			if (db < VolumeAdjustmentDb)
				return VolumeAdjustmentDb;
			if (db > 0f)
				return 0f;
			return db;
		}

		#endregion

		#region AudioSource Template

		[Header("Audio Source Configuration")]
		public GameObject AudioSourceTemplate;

		private void InitializeAudioSourceTemplate()
		{
			AudioSourceTemplate.SetActive(false);
		}

		#endregion

		#region Pooled AudioClips

		//public class AllocationEvent : UnityEvent<AudioSource, string> { }
		//public class DeallocationEvent : UnityEvent<AudioSource> { }
		//public readonly AllocationEvent OnAllocatedAudioSource = new AllocationEvent();
		//public readonly DeallocationEvent OnReleasingAudioSource = new DeallocationEvent();

		private readonly List<GameObject> FreeAudioSources = new List<GameObject>(FreeAudioSourcesInitialCapacity);
		private readonly HashSet<GameObject> ActiveAudioSources = new HashSet<GameObject>();
		private readonly GameObjectBag AudioSourceBag = new GameObjectBag(AudioSourceBagInitialCapacity);

		private static int LastCreatedAudioSourceIndex = 0;

		private GameObject GetOrCreateAudioSource()
		{
			while (FreeAudioSources.Count > 0)
			{
				var index = FreeAudioSources.Count - 1;
				var reusedAudioSource = FreeAudioSources[index];
				FreeAudioSources.RemoveAt(index);

				// See if the audio source is still alive.
				// Otherwise, continue to look in FreeAudioSources.
				if (reusedAudioSource)
				{
					if (EnableVerboseLogging)
						Log.Info($"Reusing audio source '{reusedAudioSource.gameObject.FullName()}'.", this);
					ActiveAudioSources.Add(reusedAudioSource);
					return reusedAudioSource;
				}
			}

			var go = Instantiate(AudioSourceTemplate);
			go.name = "Audio Source " + LastCreatedAudioSourceIndex++;
			DontDestroyOnLoad(go);
			ActiveAudioSources.Add(go);
			AudioSourceBag.Add(go);
			if (EnableVerboseLogging)
				Log.Info($"Created audio source '{go.FullName()}'.", this);
			return go;
		}

		/// <summary>
		/// Stops the audio on the game object (that contains audio component) and releases the object
		/// to the pool to be used again.
		///
		/// It's okay to send a null game object, in case the object was destroyed along the way.
		/// A full cleanup will be scheduled in internal lists if that happens.
		/// </summary>
		/// <param name="audioSource"></param>
		public static void ReleaseAudioSource(ref GameObject audioSource, string stopEventName = null)
		{
			var instance = InstanceEnsured;
			if (!instance)
				return;
			if (!instance.ActiveAudioSources.Contains(audioSource))
			{
				Log.Info($"Tried to release audio source '{(audioSource ? audioSource.name : "N/A")}' while it's not active.", instance);
				return;
			}
			if (instance.EnableVerboseLogging)
				Log.Info($"Releasing audio source '{(audioSource ? audioSource.name : "N/A")}'.", instance);

			if (!audioSource)
			{
				// Somehow the audio source was already destroyed (or maybe the reference was lost, which we can do nothing about here)
				// TODO: Schedule a full cleanup that will be executed in 100 ms from now. That way we group multiple cleanups together.
				instance.ClearLostReferencesInAllInternalContainers();
				return;
			}

			//OnReleasingAudioSource.Invoke(audioSource);

			// Stop Wwise
			if (!string.IsNullOrEmpty(stopEventName))
			{
				AkSoundEngine.PostEvent(stopEventName, audioSource);
			}

			audioSource.gameObject.SetActive(false);
			audioSource.transform.SetParent(null);
			DontDestroyOnLoad(audioSource.gameObject);

			instance.ActiveAudioSources.Remove(audioSource);
			instance.FreeAudioSources.Add(audioSource);

			instance.InternalRemoveFromReleaseTrackerList(audioSource);

			audioSource = null;
		}

		private void ClearLostReferencesInAllInternalContainers()
		{
			if (EnableVerboseLogging)
				Log.Info("Clearing lost references.", this);

			ClearLostReferencesInActiveAudioSourcesList();
			ClearLostReferencesInFreeAudioSourcesList();
			ClearLostReferencesInAudioSourceBag();
			ClearLostReferencesInReleaseTrackerList();
		}

		private void ClearLostReferencesInActiveAudioSourcesList()
		{
			ActiveAudioSources.RemoveWhere(item => !item);
		}

		private void ClearLostReferencesInFreeAudioSourcesList()
		{
			for (int i = 0; i < FreeAudioSources.Count; i++)
			{
				if (!FreeAudioSources[i])
				{
					FreeAudioSources.RemoveAt(i);
					i--;
				}
			}
		}

		private void ClearLostReferencesInAudioSourceBag()
		{
			AudioSourceBag.ClearDestroyedGameObjects();
		}

		#endregion

		#region AudioSource Release Tracker

		private struct ReleaseTrackerEntry
		{
			public float ReleaseTime;
			public GameObject AudioSource;
			public string StopEventName;

			public ReleaseTrackerEntry(float releaseTime, GameObject audioSource, string stopEventName)
			{
				ReleaseTime = releaseTime;
				AudioSource = audioSource;
				StopEventName = stopEventName;
			}
		}

		private readonly List<ReleaseTrackerEntry> ReleaseTracker = new List<ReleaseTrackerEntry>(10);

		private void UpdateReleaseTracker()
		{
			var now = CurrentTime;

			for (var i = 0; i < ReleaseTracker.Count; i++)
			{
				if (now > ReleaseTracker[i].ReleaseTime)
				{
					var audioSource = ReleaseTracker[i].AudioSource;
					ReleaseTracker.RemoveAt(i);
					i--;
					ReleaseAudioSource(ref audioSource, ReleaseTracker[i].StopEventName);
				}
			}
		}

		private void AddToReleaseTracker(GameObject audioSource)
		{
			Log.Info("#= Release tracker is not implemented yet!", this);
			//var clip = audioSource.clip;
			//var duration = clip.length / audioSource.pitch;
			//ReleaseTracker.Add(new ReleaseTrackerEntry(CurrentTime + duration, audioSource));
		}

		private void InternalRemoveFromReleaseTrackerList(GameObject audioSource)
		{
			for (int i = 0; i < ReleaseTracker.Count; i++)
			{
				if (ReleaseTracker[i].AudioSource == audioSource)
				{
					ReleaseTracker.RemoveAt(i);
					return;
				}
			}
		}

		private void ClearLostReferencesInReleaseTrackerList()
		{
			for (int i = 0; i < ReleaseTracker.Count; i++)
			{
				if (!ReleaseTracker[i].AudioSource)
				{
					ReleaseTracker.RemoveAt(i);
					i--;
				}
			}
		}

		#endregion

		#region Play One Shot

		public static void Play(string eventName)
		{
			var instance = InstanceEnsured;
			if (!instance)
				return;
			if (string.IsNullOrEmpty(eventName))
			{
				if (instance.EnableVerboseLogging)
					Log.Info("Received empty event name for playing.", instance);
				return;
			}
			if (instance.EnableLogging)
				Log.Info($"Playing '{eventName}'.", instance);
			AkSoundEngine.PostEvent(eventName, instance.gameObject);
		}

		public static void Play(string eventName, GameObject associatedObject)
		{
			var instance = InstanceEnsured;
			if (!instance)
				return;
			if (string.IsNullOrEmpty(eventName))
			{
				if (instance.EnableVerboseLogging)
					Log.Info("Received empty event name for playing.", instance);
				return;
			}
			if (instance.EnableLogging)
				Log.Info($"Playing '{eventName}' on object '{associatedObject.FullName()}'.", instance);
			// Need to manually update object position because Wwise is coming one frame behind in that matter, which results playing audio in wrong locations.
			AkSoundEngine.SetObjectPosition(associatedObject, associatedObject.transform);
			AkSoundEngine.PostEvent(eventName, associatedObject);
		}

		public static GameObject PlayAtPosition(string eventName, Vector3 worldPosition)
		{
			var instance = InstanceEnsured;
			if (!instance)
				return null;
			if (string.IsNullOrEmpty(eventName))
			{
				if (instance.EnableVerboseLogging)
					Log.Info($"Received empty event name for playing at position '{worldPosition}'.", instance);
				return null;
			}
			if (instance.EnableLogging)
				Log.Info($"Playing '{eventName}' at position '{worldPosition}'.", instance);
			var audioSource = instance.GetOrCreateAudioSource();
			if (!audioSource)
				return null;
			audioSource.transform.position = worldPosition;
			audioSource.gameObject.SetActive(true);
			// Need to manually update object position because Wwise is coming one frame behind in that matter, which results playing audio in wrong locations.
			AkSoundEngine.SetObjectPosition(audioSource, audioSource.transform);
			AkSoundEngine.PostEvent(eventName, audioSource, (uint)AkCallbackType.AK_EndOfEvent, EndOfEventCallback, (object)null);
			return audioSource;
		}

		public static GameObject PlayAttached(string eventName, Transform parent, Vector3 localPosition)
		{
			var instance = InstanceEnsured;
			if (!instance)
				return null;
			if (string.IsNullOrEmpty(eventName))
			{
				if (instance.EnableVerboseLogging)
					Log.Info($"Received empty event name for playing attached to '{parent.FullGameObjectName()}' at local position '{localPosition}'.", instance);
				return null;
			}
			if (instance.EnableLogging)
				Log.Info($"Playing '{eventName}' attached to '{parent.FullGameObjectName()}' at local position '{localPosition}'.", instance);
			var audioSource = instance.GetOrCreateAudioSource();
			if (!audioSource)
				return null;
			audioSource.transform.SetParent(parent);
			audioSource.transform.localPosition = localPosition;
			audioSource.gameObject.SetActive(true);
			// Need to manually update object position because Wwise is coming one frame behind in that matter, which results playing audio in wrong locations.
			AkSoundEngine.SetObjectPosition(audioSource, audioSource.transform);
			AkSoundEngine.PostEvent(eventName, audioSource, (uint)AkCallbackType.AK_EndOfEvent, EndOfEventCallback, (object)null);
			return audioSource;
		}

		private static void EndOfEventCallback(object in_cookie, AkCallbackType in_type, AkCallbackInfo in_info)
		{
			var instance = InstanceEnsured;
			if (!instance)
				return;

			var info = (AkEventCallbackInfo)in_info;
			var gameObjectID = info.gameObjID;
			if (gameObjectID == AkSoundEngine.AK_INVALID_GAME_OBJECT)
			{
				if (instance.EnableWarningLogging)
					Log.Warning($"Received 'EndOfEvent' callback for an invalid game object.", instance);
				return;
			}

			if (!instance.AudioSourceBag.InstanceMap.TryGetValue((int)gameObjectID, out var gameObject))
			{
				if (instance.EnableWarningLogging)
					Log.Warning($"Received 'EndOfEvent' callback for an unknown game object with id '{gameObjectID}', which probably was destroyed.", instance);
				return;
			}
			// It's okay to send it a null game object, if the object was destroyed along the way. 
			ReleaseAudioSource(ref gameObject);
		}

		#endregion

		#region Play Music

		public static void PlayMusic(string eventName)
		{
			var instance = InstanceEnsured;
			if (!instance)
				return;
			if (string.IsNullOrEmpty(eventName))
			{
				if (instance.EnableVerboseLogging)
					Log.Info("Received empty event name for playing music.", instance);
				return;
			}
			if (instance.EnableLogging)
				Log.Info($"Playing music '{eventName}'.", instance);
			AkSoundEngine.PostEvent(eventName, instance.gameObject);
		}

		public static void SetMusicState(string stateGroup, string state)
		{
			var instance = InstanceEnsured;
			if (!instance)
				return;
			if (instance.EnableLogging)
				Log.Info($"Setting music state '{state}' of group '{stateGroup}'.", instance);
			AkSoundEngine.SetState(stateGroup, state);
		}

		// Don't know if we will ever need this functionality. But keep it here in case we need it in future.
		/*
		[NonSerialized]
		public AudioSource MusicAudioSource;

		/// <summary>
		/// Starts to play a music. Currently played music will be crossfaded.
		/// 
		/// It's okay to quickly switch between tracks without waiting for crossfade to finish, as long as the crossfade duration is not lesser than the previously triggered crossfade.
		/// </summary>
		/// <param name="crossfadeDuration">Duration of the crossfade in seconds. Can be '0' for instantly stopping the old music and starting the new one without a crossfade.</param>
		public static AudioSource PlayMusic(string eventName, bool loop = true, float crossfadeDuration = 3f, float fadeStartVolume = 0f, float volume = 1f, float pitch = 1f)
		{
			return PlayMusic(eventName, 0f, loop, crossfadeDuration, fadeStartVolume, volume, pitch);
		}

		/// <summary>
		/// Starts to play a music. Currently played music will be crossfaded.
		/// 
		/// It's okay to quickly switch between tracks without waiting for crossfade to finish, as long as the crossfade duration is not lesser than the previously triggered crossfade.
		/// </summary>
		/// <param name="crossfadeDuration">Duration of the crossfade in seconds. Can be '0' for instantly stopping the old music and starting the new one without a crossfade.</param>
		public static AudioSource PlayMusic(string eventName, float selectorPin, bool loop = true, float crossfadeDuration = 3f, float fadeStartVolume = 0f, float volume = 1f, float pitch = 1f)
		{
			var instance = InstanceEnsured;
			if (!instance)
				return null;
			if (instance.EnableLogging)
				Log($"Playing {(loop ? "looped" : "one-shot")} music '{eventName}'@{selectorPin:N2} (V:{volume:N2} P:{pitch:N2}).");

			var doFade = crossfadeDuration > 0f;

			var newAudioSource = instance.AllocateAudioSourceWithClip(eventName, selectorPin, true);
			if (newAudioSource)
			{
				newAudioSource.transform.position = Vector3.zero;
				newAudioSource.loop = loop;
				newAudioSource.pitch = pitch;
				newAudioSource.volume = doFade
					? fadeStartVolume
					: volume;
				newAudioSource.spatialBlend = 0f;
				newAudioSource.gameObject.SetActive(true);
				newAudioSource.Play();
				if (!loop)
				{
					instance.AddToReleaseTracker(newAudioSource);
				}
			}

			var oldAudioSource = instance.MusicAudioSource;

			if (doFade)
			{
				instance.StartCoroutine(DoCrossfade(oldAudioSource, newAudioSource, volume, crossfadeDuration, true));
			}
			else
			{
				if (oldAudioSource)
				{
					instance.ReleaseAudioSource(oldAudioSource);
				}
			}

			instance.MusicAudioSource = newAudioSource;
			return newAudioSource;
		}

		public static void StopMusic()
		{
			var instance = InstanceEnsured;
			if (!instance)
				return;
			if (instance.EnableLogging)
				Log("Stopping music.");
			if (!instance.MusicAudioSource)
				return;
			instance.ReleaseAudioSource(instance.MusicAudioSource);
		}
		*/

		#endregion

		#region RTPC

		public static float GetRTPCValue(string rtpcName)
		{
			var instance = InstanceEnsured;
			if (!instance)
				return float.NaN;
			if (string.IsNullOrEmpty(rtpcName))
			{
				if (instance.EnableVerboseLogging)
					Log.Info($"Tried to get RTPC with empty name.", instance);
				return float.NaN;
			}

			int valueType = (int)AkQueryRTPCValue.RTPCValue_Global;
			AkSoundEngine.GetRTPCValue(rtpcName, null, 0, out var value, ref valueType);
			if (instance.EnableRTPCLogging)
				Log.Info($"Getting RTPC '{rtpcName}' value '{value}'.", instance);
			return value;
		}

		public static float GetRTPCValue(string rtpcName, GameObject associatedObject)
		{
			var instance = InstanceEnsured;
			if (!instance)
				return float.NaN;
			if (string.IsNullOrEmpty(rtpcName))
			{
				if (instance.EnableVerboseLogging)
					Log.Info($"Tried to get RTPC with empty name on object '{associatedObject.FullName()}'.", instance);
				return float.NaN;
			}

			int valueType = (int)AkQueryRTPCValue.RTPCValue_GameObject;
			AkSoundEngine.GetRTPCValue(rtpcName, associatedObject, 0, out var value, ref valueType);
			if (instance.EnableRTPCLogging)
				Log.Info($"Getting RTPC '{rtpcName}' value '{value}' on object '{associatedObject.FullName()}'.", instance);
			return value;
		}

		public static void SetRTPCValue(string rtpcName, float value)
		{
			var instance = InstanceEnsured;
			if (!instance)
				return;
			if (string.IsNullOrEmpty(rtpcName))
			{
				if (instance.EnableVerboseLogging)
					Log.Info($"Tried to set RTPC with empty name and value '{value}'.", instance);
				return;
			}
			if (instance.EnableRTPCLogging)
				Log.Info($"Setting RTPC '{rtpcName}' to '{value}'.", instance);
			AkSoundEngine.SetRTPCValue(rtpcName, value);
		}

		public static void SetRTPCValue(string rtpcName, float value, GameObject associatedObject)
		{
			var instance = InstanceEnsured;
			if (!instance)
				return;
			if (string.IsNullOrEmpty(rtpcName))
			{
				if (instance.EnableVerboseLogging)
					Log.Info($"Tried to set RTPC with empty name and value '{value}' on object '{associatedObject.FullName()}'.", instance);
				return;
			}
			if (instance.EnableRTPCLogging)
				Log.Info($"Setting RTPC '{rtpcName}' to '{value}' on object '{associatedObject.FullName()}'.", instance);
			AkSoundEngine.SetRTPCValue(rtpcName, value, associatedObject);
		}

		#endregion

		#region State

		public static void SetState(string stateGroup, string state)
		{
			var instance = InstanceEnsured;
			if (!instance)
				return;
			if (instance.EnableLogging)
				Log.Info($"Setting state '{state}' of group '{stateGroup}'.", instance);
			AkSoundEngine.SetState(stateGroup, state);
		}

		#endregion

		#region Debug

		[Header("Debug")]
		public bool EnableLogging = false;
		public bool EnableRTPCLogging = false;
		public bool EnableVerboseLogging = false;
		public bool EnableWarningLogging = true;

		#endregion
	}

}

#endif
