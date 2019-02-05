#if BeyondAudioUsesWwiseAudio

using System;
using System.Collections.Generic;
using Extenity.DesignPatternsToolbox;
using Extenity.FlowToolbox;
using Extenity.GameObjectToolbox;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.BeyondAudio
{

	public class WwiseAudioManager : SingletonUnity<WwiseAudioManager>, IAudioManager
	{
		#region Configuration

		public static float CurrentTime { get { return Time.realtimeSinceStartup; } }

		public const float VolumeAdjustmentDb = -80f;
		private const int FreeAudioSourcesInitialCapacity = 25;
		private const int AudioSourceBagInitialCapacity = FreeAudioSourcesInitialCapacity * 4;

		#endregion

		#region Initialization

		private void Awake()
		{
			InitializeSingleton(true);
			AudioManager._SetManager(this);
			Log.RegisterPrefix(this, "Audio");

			InitializeIncidentTracker();
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

		public bool IsDeviceVolumeSupported => false;

		public float GetDeviceVolumeNormalized()
		{
			throw new System.NotImplementedException();
		}

		public float SetDeviceVolumeNormalized(float normalizedVolume)
		{
			throw new System.NotImplementedException();
		}

#elif UNITY_STANDALONE_WIN

		public bool IsDeviceVolumeSupported => false;

		public float GetDeviceVolumeNormalized()
		{
			throw new System.NotImplementedException();
		}

		public float SetDeviceVolumeNormalized(float normalizedVolume)
		{
			throw new System.NotImplementedException();
		}

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

		#region Pooled Audio Sources

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
		public void ReleaseAudioSource(ref GameObject audioSource, string stopEventName = null)
		{
			if (!ActiveAudioSources.Contains(audioSource))
			{
				Log.Info($"Tried to release audio source '{(audioSource ? audioSource.name : "N/A")}' while it's not active.", this);
				return;
			}
			if (EnableVerboseLogging)
				Log.Info($"Releasing audio source '{(audioSource ? audioSource.name : "N/A")}'.", this);

			if (!audioSource)
			{
				// Somehow the audio source was already destroyed (or maybe the reference was lost, which we can do nothing about here)
				// TODO: Schedule a full cleanup that will be executed in 100 ms from now. That way we group multiple cleanups together.
				ClearLostReferencesInAllInternalContainers();
				return;
			}

			//OnReleasingAudioSource.Invoke(audioSource);

			// Stop Wwise
			if (!string.IsNullOrEmpty(stopEventName))
			{
				AkSoundEngine.PostEvent(stopEventName, audioSource);
				InformIncidentTracker(stopEventName, OccurrenceType.PostEvent);
			}

			audioSource.gameObject.SetActive(false);
			audioSource.transform.SetParent(null);
			DontDestroyOnLoad(audioSource.gameObject);

			ActiveAudioSources.Remove(audioSource);
			FreeAudioSources.Add(audioSource);

			InternalRemoveFromReleaseTrackerList(audioSource);

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

		public void Play(string eventName)
		{
			if (string.IsNullOrEmpty(eventName))
			{
				if (EnableVerboseLogging)
					Log.Info("Received empty event name for playing.", this);
				return;
			}
			if (EnableLogging)
				Log.Info($"Playing '{eventName}'.", this);
			AkSoundEngine.PostEvent(eventName, gameObject);
			InformIncidentTracker(eventName, OccurrenceType.PostEvent);
		}

		public void Play(string eventName, GameObject associatedObject)
		{
			if (string.IsNullOrEmpty(eventName))
			{
				if (EnableVerboseLogging)
					Log.Info("Received empty event name for playing.", this);
				return;
			}
			if (EnableLogging)
				Log.Info($"Playing '{eventName}' on object '{associatedObject.FullName()}'.", this);
			// Need to manually update object position because Wwise is coming one frame behind in that matter, which results playing audio in wrong locations.
			AkSoundEngine.SetObjectPosition(associatedObject, associatedObject.transform);
			AkSoundEngine.PostEvent(eventName, associatedObject);
			InformIncidentTracker(eventName, OccurrenceType.PostEvent);
		}

		public GameObject PlayAtPosition(string eventName, Vector3 worldPosition)
		{
			if (string.IsNullOrEmpty(eventName))
			{
				if (EnableVerboseLogging)
					Log.Info($"Received empty event name for playing at position '{worldPosition}'.", this);
				return null;
			}
			if (EnableLogging)
				Log.Info($"Playing '{eventName}' at position '{worldPosition}'.", this);
			var audioSource = GetOrCreateAudioSource();
			if (!audioSource)
				return null;
			audioSource.transform.position = worldPosition;
			audioSource.gameObject.SetActive(true);
			// Need to manually update object position because Wwise is coming one frame behind in that matter, which results playing audio in wrong locations.
			AkSoundEngine.SetObjectPosition(audioSource, audioSource.transform);
			AkSoundEngine.PostEvent(eventName, audioSource, (uint)AkCallbackType.AK_EndOfEvent, EndOfEventCallback, (object)null);
			InformIncidentTracker(eventName, OccurrenceType.PostEvent);
			return audioSource;
		}

		public GameObject PlayAttached(string eventName, Transform parent, Vector3 localPosition)
		{
			if (string.IsNullOrEmpty(eventName))
			{
				if (EnableVerboseLogging)
					Log.Info($"Received empty event name for playing attached to '{parent.FullGameObjectName()}' at local position '{localPosition}'.", this);
				return null;
			}
			if (EnableLogging)
				Log.Info($"Playing '{eventName}' attached to '{parent.FullGameObjectName()}' at local position '{localPosition}'.", this);
			var audioSource = GetOrCreateAudioSource();
			if (!audioSource)
				return null;
			audioSource.transform.SetParent(parent);
			audioSource.transform.localPosition = localPosition;
			audioSource.gameObject.SetActive(true);
			// Need to manually update object position because Wwise is coming one frame behind in that matter, which results playing audio in wrong locations.
			AkSoundEngine.SetObjectPosition(audioSource, audioSource.transform);
			AkSoundEngine.PostEvent(eventName, audioSource, (uint)AkCallbackType.AK_EndOfEvent, EndOfEventCallback, (object)null);
			InformIncidentTracker(eventName, OccurrenceType.PostEvent);
			return audioSource;
		}

		private void EndOfEventCallback(object in_cookie, AkCallbackType in_type, AkCallbackInfo in_info)
		{
			var info = (AkEventCallbackInfo)in_info;
			var gameObjectID = info.gameObjID;
			if (gameObjectID == AkSoundEngine.AK_INVALID_GAME_OBJECT)
			{
				if (EnableWarningLogging)
					Log.Warning($"Received 'EndOfEvent' callback for an invalid game object.", this);
				return;
			}

			if (!AudioSourceBag.InstanceMap.TryGetValue((int)gameObjectID, out var gameObject))
			{
				if (EnableWarningLogging)
					Log.Warning($"Received 'EndOfEvent' callback for an unknown game object with id '{gameObjectID}', which probably was destroyed.", this);
				return;
			}
			// It's okay to send it a null game object, if the object was destroyed along the way. 
			ReleaseAudioSource(ref gameObject);
		}

		#endregion

		#region Play Music

		public void PlayMusic(string eventName)
		{
			if (string.IsNullOrEmpty(eventName))
			{
				if (EnableVerboseLogging)
					Log.Info("Received empty event name for playing music.", this);
				return;
			}
			if (EnableLogging)
				Log.Info($"Playing music '{eventName}'.", this);
			AkSoundEngine.PostEvent(eventName, gameObject);
			InformIncidentTracker(eventName, OccurrenceType.PostEvent);
		}

		public static void SetMusicState(string stateGroup, string state)
		{
			if (AudioManager.EnsureIntegrity())
				Instance._SetMusicState(stateGroup, state);
		}

		private void _SetMusicState(string stateGroup, string state)
		{
			if (EnableLogging)
				Log.Info($"Setting music state '{state}' of group '{stateGroup}'.", this);
			AkSoundEngine.SetState(stateGroup, state);
			InformIncidentTracker(stateGroup, OccurrenceType.SetState);
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
			if (AudioManager.EnsureIntegrity())
				return Instance._GetRTPCValue(rtpcName);
			return float.NaN;
		}

		private float _GetRTPCValue(string rtpcName)
		{
			if (string.IsNullOrEmpty(rtpcName))
			{
				if (EnableVerboseLogging)
					Log.Info($"Tried to get RTPC with empty name.", this);
				return float.NaN;
			}

			int valueType = (int)AkQueryRTPCValue.RTPCValue_Global;
			AkSoundEngine.GetRTPCValue(rtpcName, null, 0, out var value, ref valueType);
			InformIncidentTracker(rtpcName, OccurrenceType.GetRTPC);
			if (EnableRTPCLogging)
				Log.Info($"Getting RTPC '{rtpcName}' value '{value}'.", this);
			return value;
		}

		public static float GetRTPCValue(string rtpcName, GameObject associatedObject)
		{
			if (AudioManager.EnsureIntegrity())
				return Instance._GetRTPCValue(rtpcName, associatedObject);
			return float.NaN;
		}

		private float _GetRTPCValue(string rtpcName, GameObject associatedObject)
		{
			if (string.IsNullOrEmpty(rtpcName))
			{
				if (EnableVerboseLogging)
					Log.Info($"Tried to get RTPC with empty name on object '{associatedObject.FullName()}'.", this);
				return float.NaN;
			}

			int valueType = (int)AkQueryRTPCValue.RTPCValue_GameObject;
			AkSoundEngine.GetRTPCValue(rtpcName, associatedObject, 0, out var value, ref valueType);
			InformIncidentTracker(rtpcName, OccurrenceType.GetRTPC);
			if (EnableRTPCLogging)
				Log.Info($"Getting RTPC '{rtpcName}' value '{value}' on object '{associatedObject.FullName()}'.", this);
			return value;
		}

		public static void SetRTPCValue(string rtpcName, float value)
		{
			if (AudioManager.EnsureIntegrity())
				Instance._SetRTPCValue(rtpcName, value);
		}

		private void _SetRTPCValue(string rtpcName, float value)
		{
			if (string.IsNullOrEmpty(rtpcName))
			{
				if (EnableVerboseLogging)
					Log.Info($"Tried to set RTPC with empty name and value '{value}'.", this);
				return;
			}
			if (EnableRTPCLogging)
				Log.Info($"Setting RTPC '{rtpcName}' to '{value}'.", this);
			AkSoundEngine.SetRTPCValue(rtpcName, value);
			InformIncidentTracker(rtpcName, OccurrenceType.SetRTPC);
		}

		public static void SetRTPCValue(string rtpcName, float value, GameObject associatedObject)
		{
			if (AudioManager.EnsureIntegrity())
				Instance._SetRTPCValue(rtpcName, value, associatedObject);
		}

		private void _SetRTPCValue(string rtpcName, float value, GameObject associatedObject)
		{
			if (string.IsNullOrEmpty(rtpcName))
			{
				if (EnableVerboseLogging)
					Log.Info($"Tried to set RTPC with empty name and value '{value}' on object '{associatedObject.FullName()}'.", this);
				return;
			}
			if (EnableRTPCLogging)
				Log.Info($"Setting RTPC '{rtpcName}' to '{value}' on object '{associatedObject.FullName()}'.", this);
			AkSoundEngine.SetRTPCValue(rtpcName, value, associatedObject);
			InformIncidentTracker(rtpcName, OccurrenceType.SetRTPC);
		}

		#endregion

		#region State

		public static void SetState(string stateGroup, string state)
		{
			if (AudioManager.EnsureIntegrity())
				Instance._SetState(stateGroup, state);
		}

		private void _SetState(string stateGroup, string state)
		{
			if (EnableLogging)
				Log.Info($"Setting state '{state}' of group '{stateGroup}'.", this);
			AkSoundEngine.SetState(stateGroup, state);
			InformIncidentTracker(stateGroup, OccurrenceType.SetState);
		}

		#endregion

		#region Incident Tracker

		public enum OccurrenceType
		{
			PostEvent,
			GetRTPC,
			SetRTPC,
			GetState,
			SetState,
		}

		public static int OccurrenceTypeCount => (int)OccurrenceType.SetState + 1;

		/// <summary>
		/// The list keeps a dictionary for each OccurrenceType. Dictionaries keep the names of occurrences (events, RTPCs, state groups, etc.) and keeps the count to see how frequent they happen.
		/// </summary>
		[NonSerialized]
		[ShowInInspector, ReadOnly]
		public Dictionary<string, int>[] OccurrenceCounts;
		[NonSerialized]
		[ShowInInspector, ReadOnly]
		public readonly HashSet<(string occurrenceName, OccurrenceType occurrenceType)> ReportedIncidents = new HashSet<(string occurrenceName, OccurrenceType occurrenceType)>();

		[Header("Incident Tracker")]
		public int InitialIncidentTrackerDictionaryCapacity = 50 * 4; // 50 is an average event type count. 4 is the magic number that makes the dictionary work at it's best condition.
		public int[] IncidentTrackerOccurrenceToleranceInLastSecond;
		public bool EnableIncidentLogging = true;
		public LogCategory IncidentLogTypeInEditor = LogCategory.Error;
		public LogCategory IncidentLogTypeInBuild = LogCategory.Critical;

		private LogCategory IncidentLogType
		{
			get
			{
#if UNITY_EDITOR
				return IncidentLogTypeInEditor;
#else
				return IncidentLogTypeInBuild;
#endif
			}
		}

		private void InitializeIncidentTracker()
		{
			Debug.Assert(OccurrenceTypeCount == Enum.GetNames(typeof(OccurrenceType)).Length);

			OccurrenceCounts = new Dictionary<string, int>[OccurrenceTypeCount];
			for (int i = 0; i < OccurrenceTypeCount; i++)
			{
				OccurrenceCounts[i] = new Dictionary<string, int>(InitialIncidentTrackerDictionaryCapacity);
			}

			this.FastInvokeRepeating(ClearIncidentTrackerCounts, 1f, 1f, true);
		}

		private void ClearIncidentTrackerCounts()
		{
			for (int i = 0; i < OccurrenceCounts.Length; i++)
			{
				OccurrenceCounts[i].Clear();
			}
		}

		private void InformIncidentTracker(string occurrenceName, OccurrenceType occurrenceType)
		{
			var dict = OccurrenceCounts[(int)occurrenceType];
			dict.TryGetValue(occurrenceName, out int occurrenceCount);
			occurrenceCount++;
			dict[occurrenceName] = occurrenceCount;

			if (occurrenceCount == IncidentTrackerOccurrenceToleranceInLastSecond[(int)occurrenceType])
			{
				if (EnableIncidentLogging)
				{
					if (ReportedIncidents.Add((occurrenceName, occurrenceType))) // This is here to block repetitive logs. Do not log an incident more than once.
					{
						Log.Any($"Heavy use of '{occurrenceType}' for '{occurrenceName}' detected.", IncidentLogType, this);
					}
				}
			}
		}

		#endregion

		#region Editor

#if UNITY_EDITOR

		[OnInspectorGUI]
		private void OnInspectorGUI()
		{
			Sirenix.Utilities.Editor.GUIHelper.RequestRepaint();
		}

		protected void OnValidate()
		{
			if (IncidentTrackerOccurrenceToleranceInLastSecond.Length != OccurrenceTypeCount)
			{
				Array.Resize(ref IncidentTrackerOccurrenceToleranceInLastSecond, OccurrenceTypeCount);
			}
		}

#endif

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
