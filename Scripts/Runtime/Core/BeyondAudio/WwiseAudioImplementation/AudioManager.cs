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
					Debug.LogError("AudioManager is not initialized yet.");
				}
				return instance;
			}
		}

		#endregion

		#region Initialization

		private void Awake()
		{
			InitializeSingleton(this, true);
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
			Debug.LogError($"Volume control '{mixerParameterName}' does not exist.");
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
					if (EnableVolatileLogging)
						LogVolatile($"Reusing audio source '{reusedAudioSource.gameObject.FullName()}'.");
					ActiveAudioSources.Add(reusedAudioSource);
					return reusedAudioSource;
				}
			}

			var go = Instantiate(AudioSourceTemplate);
			go.name = "Audio Source " + LastCreatedAudioSourceIndex++;
			DontDestroyOnLoad(go);
			ActiveAudioSources.Add(go);
			AudioSourceBag.Add(go);
			if (EnableVolatileLogging)
				LogVolatile($"Created audio source '{go.FullName()}'.");
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
				LogVolatile($"Tried to release audio source '{(audioSource ? audioSource.name : "N/A")}' while it's not active.");
				return;
			}
			if (instance.EnableVolatileLogging)
				LogVolatile($"Releasing audio source '{(audioSource ? audioSource.name : "N/A")}'.");

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
			if (EnableVolatileLogging)
				LogVolatile("Clearing lost references.");

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
			Log("#= Release tracker is not implemented yet!");
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
				if (instance.EnableVolatileLogging)
					LogVolatile("Received empty event name for playing.");
				return;
			}
			if (instance.EnableLogging)
				Log($"Playing '{eventName}'.");
			AkSoundEngine.PostEvent(eventName, instance.gameObject);
		}

		public static void Play(string eventName, GameObject associatedObject)
		{
			var instance = InstanceEnsured;
			if (!instance)
				return;
			if (string.IsNullOrEmpty(eventName))
			{
				if (instance.EnableVolatileLogging)
					LogVolatile("Received empty event name for playing.");
				return;
			}
			if (instance.EnableLogging)
				Log($"Playing '{eventName}' on object '{associatedObject.FullName()}'.");
			AkSoundEngine.PostEvent(eventName, associatedObject);
		}

		public static GameObject PlayAtPosition(string eventName, Vector3 worldPosition)
		{
			var instance = InstanceEnsured;
			if (!instance)
				return null;
			if (string.IsNullOrEmpty(eventName))
			{
				if (instance.EnableVolatileLogging)
					LogVolatile($"Received empty event name for playing at position '{worldPosition}'.");
				return null;
			}
			if (instance.EnableLogging)
				Log($"Playing '{eventName}' at position '{worldPosition}'.");
			var audioSource = instance.GetOrCreateAudioSource();
			if (!audioSource)
				return null;
			audioSource.transform.position = worldPosition;
			audioSource.gameObject.SetActive(true);
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
				if (instance.EnableVolatileLogging)
					LogVolatile($"Received empty event name for playing attached to '{parent.FullGameObjectName()}' at local position '{localPosition}'.");
				return null;
			}
			if (instance.EnableLogging)
				Log($"Playing '{eventName}' attached to '{parent.FullGameObjectName()}' at local position '{localPosition}'.");
			var audioSource = instance.GetOrCreateAudioSource();
			if (!audioSource)
				return null;
			audioSource.transform.SetParent(parent);
			audioSource.transform.localPosition = localPosition;
			audioSource.gameObject.SetActive(true);
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
					LogWarning($"Received 'EndOfEvent' callback for an invalid game object.");
				return;
			}
			GameObject gameObject;
			if (!instance.AudioSourceBag.InstanceMap.TryGetValue((int)gameObjectID, out gameObject))
			{
				if (instance.EnableWarningLogging)
					LogWarning($"Received 'EndOfEvent' callback for an unknown game object with id '{gameObjectID}', which probably was destroyed.");
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
				if (instance.EnableVolatileLogging)
					LogVolatile($"Received empty event name for playing music.");
				return;
			}
			if (instance.EnableLogging)
				Log($"Playing music '{eventName}'.");
			AkSoundEngine.PostEvent(eventName, instance.gameObject);
		}

		public static void SetMusicState(string stateGroup, string state)
		{
			var instance = InstanceEnsured;
			if (!instance)
				return;
			if (instance.EnableLogging)
				Log($"Setting music state '{state}' of group '{stateGroup}'.");
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
				if (instance.EnableVolatileLogging)
					LogVolatile($"Tried to get RTPC with empty name.");
				return float.NaN;
			}
			float value;
			int valueType = (int)AkQueryRTPCValue.RTPCValue_Global;
			AkSoundEngine.GetRTPCValue(rtpcName, null, 0, out value, ref valueType);
			if (instance.EnableLogging)
				Log($"Getting RTPC '{rtpcName}' value '{value}'.");
			return value;
		}

		public static float GetRTPCValue(string rtpcName, GameObject associatedObject)
		{
			var instance = InstanceEnsured;
			if (!instance)
				return float.NaN;
			if (string.IsNullOrEmpty(rtpcName))
			{
				if (instance.EnableVolatileLogging)
					LogVolatile($"Tried to get RTPC with empty name on object '{associatedObject.FullName()}'.");
				return float.NaN;
			}
			float value;
			int valueType = (int)AkQueryRTPCValue.RTPCValue_GameObject;
			AkSoundEngine.GetRTPCValue(rtpcName, associatedObject, 0, out value, ref valueType);
			if (instance.EnableLogging)
				Log($"Getting RTPC '{rtpcName}' value '{value}' on object '{associatedObject.FullName()}'.");
			return value;
		}

		public static void SetRTPCValue(string rtpcName, float value)
		{
			var instance = InstanceEnsured;
			if (!instance)
				return;
			if (string.IsNullOrEmpty(rtpcName))
			{
				if (instance.EnableVolatileLogging)
					LogVolatile($"Tried to set RTPC with empty name and value '{value}'.");
				return;
			}
			if (instance.EnableLogging)
				Log($"Setting RTPC '{rtpcName}' to '{value}'.");
			AkSoundEngine.SetRTPCValue(rtpcName, value);
		}

		public static void SetRTPCValue(string rtpcName, float value, GameObject associatedObject)
		{
			var instance = InstanceEnsured;
			if (!instance)
				return;
			if (string.IsNullOrEmpty(rtpcName))
			{
				if (instance.EnableVolatileLogging)
					LogVolatile($"Tried to set RTPC with empty name and value '{value}' on object '{associatedObject.FullName()}'.");
				return;
			}
			if (instance.EnableLogging)
				Log($"Setting RTPC '{rtpcName}' to '{value}' on object '{associatedObject.FullName()}'.");
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
				Log($"Setting state '{state}' of group '{stateGroup}'.");
			AkSoundEngine.SetState(stateGroup, state);
		}

		#endregion

		#region Debug

		[Header("Debug")]
		public bool EnableLogging = false;
		public bool EnableVolatileLogging = false;
		public bool EnableWarningLogging = true;

		/// <summary>
		/// Check for 'EnableLogging' before each Log call to prevent unnecessary string creation.
		/// </summary>
		private static void Log(string message)
		{
			// This must be checked before each Log call manually.
			//if (!EnableLogging)
			//	return;

			Debug.Log("|AUDIO|" + message, Instance);
		}

		private static void LogVolatile(string message)
		{
			// This must be checked before each Log call manually.
			//if (!EnableVolatileLogging)
			//	return;

			Debug.Log("|AUDIO|" + message, Instance);
		}

		private static void LogWarning(string message)
		{
			// This must be checked before each Log call manually.
			//if (!EnableWarningLogging)
			//	return;

			Debug.LogWarning("|AUDIO|" + message, Instance);
		}

		private static void LogError(string message)
		{
			Debug.LogError("|AUDIO|" + message, Instance);
		}

		#endregion
	}

}

#endif
