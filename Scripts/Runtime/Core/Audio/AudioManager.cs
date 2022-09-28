#if ExtenityAudio

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extenity.ApplicationToolbox;
using Extenity.DataToolbox;
using Extenity.DebugToolbox;
using Extenity.DesignPatternsToolbox;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using static Unity.Mathematics.math;

namespace Extenity.Audio
{

	public class AudioManager : SingletonUnity<AudioManager>, ISerializationCallbackReceiver
	{
		#region Configuration

		public static float CurrentTime { get { return Time.realtimeSinceStartup; } }

		public const float VolumeAdjustmentDb = -80f;
		private const int FreeAudioSourcesInitialCapacity = 25;
		// private const int AudioSourceBagInitialCapacity = FreeAudioSourcesInitialCapacity * 4;

		#endregion

		#region Initialization

		protected override void AwakeDerived()
		{
			gameObject.SetAsLogContext(ref Log);

			CalculateEventInternals();
			// InitializeIncidentTracker();
			InitializeAudioSourceTemplate();
		}

		private void Start()
		{
			// Quick fix for not properly initializing Unity's audio system.
			// Mixer parameters must be set in Start, instead of Awake.
			// Otherwise they will be ignored silently.
			InitializeVolumeControls();
		}

		public static bool EnsureIntegrity()
		{
			if (!IsInstanceAvailable)
			{
				if (!ApplicationTools.IsShuttingDown)
				{
					Log.CriticalError("AudioManager is not available.");
				}
				return false;
			}
			return true;
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

		public static readonly bool IsDeviceVolumeSupported = false;

		public static float GetDeviceVolumeNormalized()
		{
			throw new System.NotImplementedException();
		}

		public static float SetDeviceVolumeNormalized(float normalizedVolume)
		{
			throw new System.NotImplementedException();
		}

#elif UNITY_ANDROID

		public static readonly bool IsDeviceVolumeSupported = false;

		public static float GetDeviceVolumeNormalized()
		{
			throw new System.NotImplementedException();
		}

		public static float SetDeviceVolumeNormalized(float normalizedVolume)
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
			var min = AndroidAudioService.Call<int>("getStreamMinVolume", ANDROID_STREAM_MUSIC);
			var max = AndroidAudioService.Call<int>("getStreamMaxVolume", ANDROID_STREAM_MUSIC);
			Log.Verbose($"Android device volume range: {min}-{max}");
			return new RangeInt(min, max - min);
		}

		private int GetDeviceVolume()
		{
			var volume = AndroidAudioService.Call<int>("getStreamVolume", ANDROID_STREAM_MUSIC);
			Log.Verbose($"Android device volume: {volume}");
			return volume;
		}

		public float GetDeviceVolumeNormalized()
		{
			var range = GetDeviceVolumeRange();
			var volume = GetDeviceVolume();
			var normalizedVolume = (volume - range.start) / (float)range.length;
			Log.Verbose($"Android device normalized volume: {normalizedVolume}");
			return volume;
		}

		/// <summary>
		/// Returns the current volume after the value is set.
		/// </summary>
		private int SetDeviceVolume(int volume)
		{
			AndroidAudioService.Call("setStreamVolume", ANDROID_STREAM_MUSIC, volume, ANDROID_SETSTREAMVOLUME_FLAGS);
			var newVolume = GetDeviceVolume();
			if (newVolume == volume)
			{
				Log.Verbose($"Android device volume set to: {newVolume}");
			}
			else
			{
				Log.Warning($"Failed to set Android device volume to '{volume}'. Currently '{newVolume}'");
			}
			return newVolume;
		}

		/// <summary>
		/// Returns the current volume after the value is set.
		/// </summary>
		public float SetDeviceVolumeNormalized(float normalizedVolume)
		{
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

		public static readonly bool IsDeviceVolumeSupported = false;

		public static float GetDeviceVolumeNormalized()
		{
			throw new System.NotImplementedException();
		}

		public static float SetDeviceVolumeNormalized(float normalizedVolume)
		{
			throw new System.NotImplementedException();
		}

#elif UNITY_STANDALONE_WIN

		public static readonly bool IsDeviceVolumeSupported = false;

		public static float GetDeviceVolumeNormalized()
		{
			throw new System.NotImplementedException();
		}

		public static float SetDeviceVolumeNormalized(float normalizedVolume)
		{
			throw new System.NotImplementedException();
		}

#endif

		#endregion

		#region Master Audio Mixer and Music/Effect Volumes

		[Header("Audio Mixers and Volume Controls")]
		public AudioMixer MasterAudioMixer;
		public VolumeControl[] MixerVolumeControls =
		{
			new VolumeControl { MixerParameterName = "MasterVolume" },
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

		public static VolumeControl GetVolumeControl(string mixerParameterName)
		{
			if (EnsureIntegrity())
				return Instance._GetVolumeControl(mixerParameterName);
			return null;
		}

		private VolumeControl _GetVolumeControl(string mixerParameterName)
		{
			for (var i = 0; i < MixerVolumeControls.Length; i++)
			{
				var volumeControl = MixerVolumeControls[i];
				if (volumeControl.MixerParameterName == mixerParameterName)
					return volumeControl;
			}
			Log.CriticalError($"Volume control '{mixerParameterName}' does not exist.");
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
			normalized = pow(normalized, 1f / 3f);
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
			var audioSource = AudioSourceTemplate.GetComponent<AudioSource>();
			audioSource.playOnAwake = false;
		}

		#endregion

		#region Pooled Audio Sources

		public class AllocationEvent : UnityEvent<AudioSource, string> { }
		public class DeallocationEvent : UnityEvent<AudioSource> { }
		public readonly AllocationEvent OnAllocatedAudioSource = new AllocationEvent();
		public readonly DeallocationEvent OnReleasingAudioSource = new DeallocationEvent();

		private readonly List<AudioSource> FreeAudioSources = new List<AudioSource>(FreeAudioSourcesInitialCapacity);
		private readonly HashSet<AudioSource> ActiveAudioSources = new HashSet<AudioSource>();
		// private readonly ComponentBag<AudioSource> AudioSourceBag = new ComponentBag<AudioSource>(AudioSourceBagInitialCapacity);

		private static int LastCreatedAudioSourceIndex = 0;

		private AudioSource GetOrCreateAudioSource()
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
						Log.Info($"Reusing audio source '{reusedAudioSource.gameObject.FullName()}'.");
					ActiveAudioSources.Add(reusedAudioSource);
					return reusedAudioSource;
				}
			}

			var go = Instantiate(AudioSourceTemplate);
			var audioSource = go.GetComponent<AudioSource>();
			Debug.Assert(audioSource);
			go.name = "Audio Source " + LastCreatedAudioSourceIndex++;
			DontDestroyOnLoad(go);
			ActiveAudioSources.Add(audioSource);
			// AudioSourceBag.Add(go.GetInstanceID(), audioSource);
			if (EnableVerboseLogging)
				Log.Info($"Created audio source '{go.FullName()}'.");
			return audioSource;
		}

		public AudioSource AllocateAudioSourceWithClip(string eventName, float selectorPin, bool errorIfNotFound)
		{
			if (EnableLogging)
				Log.Info($"Allocating audio source for event '{eventName}' with pin '{selectorPin}'.");

			var audioEvent = GetEvent(eventName, errorIfNotFound);
			if (audioEvent == null)
				return null;
			var clip = audioEvent.SelectRandomClip(selectorPin, errorIfNotFound);
			if (!clip)
				return null;
			var audioSource = GetOrCreateAudioSource();
			audioSource.clip = clip;
			audioSource.outputAudioMixerGroup = audioEvent.Output;
			OnAllocatedAudioSource.Invoke(audioSource, eventName);
			return audioSource;
		}

		/// <summary>
		/// Stops the audio on the game object (that contains audio component) and releases the object
		/// to the pool to be used again.
		///
		/// It's okay to send a null audio source, in case the object was destroyed along the way.
		/// A full cleanup will be scheduled in internal lists if that happens.
		/// </summary>
		public static void ReleaseAudioSource(ref AudioSource audioSource)
		{
			if (EnsureIntegrity())
				Instance._ReleaseAudioSource(ref audioSource);
		}

		private void _ReleaseAudioSource(ref AudioSource audioSource)
		{
			if (!ActiveAudioSources.Contains(audioSource))
			{
				Log.Info($"Tried to release audio source '{(audioSource ? audioSource.gameObject.name : "N/A")}' while it's not active.");
				return;
			}
			if (EnableVerboseLogging)
				Log.Info($"Releasing audio source with clip '{(audioSource && audioSource.clip ? audioSource.clip.name : "N/A")}'.");

			if (!audioSource)
			{
				// Somehow the audio source was already destroyed (or maybe the reference was lost, which we can do nothing about here)
				// TODO: Schedule a full cleanup that will be executed in 100 ms from now. That way we group multiple cleanups together.
				ClearLostReferencesInAllInternalContainers();
				return;
			}

			OnReleasingAudioSource.Invoke(audioSource);

			// TODO: At some point, implement Incident Tracker for UnityAudio, just like Wwise.
			// InformIncidentTracker(stopEventID, OccurrenceType.PostEvent);

			audioSource.Stop();
			audioSource.clip = null;
			audioSource.outputAudioMixerGroup = null;
			audioSource.enabled = false;
			audioSource.gameObject.SetActive(false);
			audioSource.transform.SetParent(null);

			ActiveAudioSources.Remove(audioSource);
			FreeAudioSources.Add(audioSource);

			InternalRemoveFromReleaseTrackerList(audioSource);

			audioSource = null;
		}

		private void ClearLostReferencesInAllInternalContainers()
		{
			if (EnableVerboseLogging)
				Log.Info("Clearing lost references.");

			ClearLostReferencesInActiveAudioSourcesList();
			ClearLostReferencesInFreeAudioSourcesList();
			// ClearLostReferencesInAudioSourceBag();
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

		// private void ClearLostReferencesInAudioSourceBag()
		// {
		// 	AudioSourceBag.ClearLostReferences();
		// }

		#endregion

		#region Events

		[Header("Audio Events")]
		public List<AudioEvent> Events;

		/// <summary>
		/// Exact copy of Events list with only names. This list is automatically generated.
		/// </summary>
		[HideInInspector]
		[SerializeField]
		public string[] EventNames;

		private void RefreshEventNamesList()
		{
			EventNames = Events.Where(item => item != null)
			                   .Select(item => item.Name)
			                   .ToArray();
		}

		public AudioEvent GetEvent(string eventName, bool errorIfNotFound)
		{
			if (string.IsNullOrEmpty(eventName))
				return null;

			if (Events != null)
			{
				for (int i = 0; i < EventNames.Length; i++)
				{
					if (EventNames[i] == eventName)
						return Events[i];
				}
			}
			if (errorIfNotFound)
			{
				Log.Error($"Sound event '{eventName}' does not exist.");
			}
			return null;
		}

		public List<AudioEvent> ListEventsWithUnassignedOutputs()
		{
			List<AudioEvent> list = null;
			if (Events != null)
			{
				for (var i = 0; i < Events.Count; i++)
				{
					var audioEvent = Events[i];
					if (!audioEvent.Output)
					{
						if (list == null)
							list = New.List<AudioEvent>(10);
						list.Add(audioEvent);
					}
				}
			}
			return list;
		}

		public List<AudioEvent> ListEventsWithUnassignedClips()
		{
			List<AudioEvent> list = null;
			if (Events != null)
			{
				for (var i = 0; i < Events.Count; i++)
				{
					var audioEvent = Events[i];
					if (audioEvent.HasAnyUnassignedClip)
					{
						if (list == null)
							list = New.List<AudioEvent>(10);
						list.Add(audioEvent);
					}
				}
			}
			return list;
		}

		private void CalculateEventInternals()
		{
			if (Events != null)
			{
				for (var i = 0; i < Events.Count; i++)
				{
					Events[i].CalculateInternals();
				}
			}
		}

		private void ClearUnnecessaryReferencesInEvents()
		{
			if (Events != null)
			{
				for (var i = 0; i < Events.Count; i++)
				{
					Events[i].ClearUnnecessaryReferences();
				}
			}
		}

		#endregion

		#region AudioSource Release Tracker

		private struct ReleaseTrackerEntry
		{
			public float ReleaseTime;
			public AudioSource AudioSource;

			public ReleaseTrackerEntry(float releaseTime, AudioSource audioSource)
			{
				ReleaseTime = releaseTime;
				AudioSource = audioSource;
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
					ReleaseAudioSource(ref audioSource);
				}
			}
		}

		private void AddToReleaseTracker(AudioSource audioSource)
		{
			var clip = audioSource.clip;
			var duration = clip.length / audioSource.pitch;
			ReleaseTracker.Add(new ReleaseTrackerEntry(CurrentTime + duration, audioSource));
		}

		private void InternalRemoveFromReleaseTrackerList(AudioSource audioSource)
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

		private static void SetAudioSourceParametersAndPlay(AudioSource audioSource, bool loop, float volume, float pitch, float spatialBlend)
		{
			audioSource.loop = loop;
			audioSource.pitch = pitch;
			audioSource.volume = volume;
			audioSource.spatialBlend = spatialBlend;
			audioSource.enabled = true;
			audioSource.gameObject.SetActive(true);
			audioSource.Play();
		}

		/// <summary>
		/// Note that looped events should be stopped using 'Stop' or they have to be manually released using 'ReleaseAudioSource' if stopped manually.
		/// </summary>
		public static AudioSource Play(string eventName, bool loop = false, float volume = 1f, float pitch = 1f)
		{
			return Play(eventName, 0f, loop, volume, pitch);
		}

		/// <summary>
		/// Note that looped events should be stopped using 'Stop' or they have to be manually released using 'ReleaseAudioSource' if stopped manually.
		/// </summary>
		public static AudioSource Play(string eventName, float selectorPin, bool loop = false, float volume = 1f, float pitch = 1f)
		{
			if (EnsureIntegrity())
				return Instance._Play(eventName, selectorPin, loop, volume, pitch);
			return null;
		}

		private AudioSource _Play(string eventName, float selectorPin, bool loop, float volume, float pitch)
		{
			if (EnableLogging)
				Log.Info($"Playing {(loop ? "looped" : "one-shot")} '{eventName}'@{selectorPin:N2} (V:{volume:N2} P:{pitch:N2}).");
			var audioSource = AllocateAudioSourceWithClip(eventName, selectorPin, true);
			if (!audioSource)
				return null;
			audioSource.transform.position = Vector3.zero;
			SetAudioSourceParametersAndPlay(audioSource, loop, volume, pitch, 0f);
			if (!loop)
			{
				AddToReleaseTracker(audioSource);
			}
			return audioSource;
		}

		/// <summary>
		/// Note that looped events should be stopped using 'Stop' or they have to be manually released using 'ReleaseAudioSource' if stopped manually.
		/// </summary>
		public static AudioSource PlayAtPosition(string eventName, Vector3 position, bool loop = false, float volume = 1f, float pitch = 1f, float spatialBlend = 1f)
		{
			return PlayAtPosition(eventName, 0f, position, loop, volume, pitch, spatialBlend);
		}

		/// <summary>
		/// Note that looped events should be stopped using 'Stop' or they have to be manually released using 'ReleaseAudioSource' if stopped manually.
		/// </summary>
		public static AudioSource PlayAtPosition(string eventName, float selectorPin, Vector3 position, bool loop = false, float volume = 1f, float pitch = 1f, float spatialBlend = 1f)
		{
			if (EnsureIntegrity())
				return Instance._PlayAtPosition(eventName, selectorPin, position, loop, volume, pitch, spatialBlend);
			return null;
		}

		private AudioSource _PlayAtPosition(string eventName, float selectorPin, Vector3 position, bool loop, float volume, float pitch, float spatialBlend)
		{
			if (EnableLogging)
				Log.Info($"Playing {(loop ? "looped" : "one-shot")} '{eventName}'@{selectorPin:N2} (V:{volume:N2} P:{pitch:N2}) at position '{position}'.");
			var audioSource = AllocateAudioSourceWithClip(eventName, selectorPin, true);
			if (!audioSource)
				return null;
			audioSource.transform.position = position;
			SetAudioSourceParametersAndPlay(audioSource, loop, volume, pitch, spatialBlend);
			if (!loop)
			{
				AddToReleaseTracker(audioSource);
			}
			return audioSource;
		}

		/// <summary>
		/// Note that looped events should be stopped using 'Stop' or they have to be manually released using 'ReleaseAudioSource' if stopped manually.
		/// </summary>
		public static AudioSource PlayAttached(string eventName, Transform parent, Vector3 localPosition, bool loop = false, float volume = 1f, float pitch = 1f, float spatialBlend = 1f)
		{
			return PlayAttached(eventName, 0f, parent, localPosition, loop, volume, pitch, spatialBlend);
		}

		/// <summary>
		/// Note that looped events should be stopped using 'Stop' or they have to be manually released using 'ReleaseAudioSource' if stopped manually.
		/// </summary>
		public static AudioSource PlayAttached(string eventName, float selectorPin, Transform parent, Vector3 localPosition, bool loop = false, float volume = 1f, float pitch = 1f, float spatialBlend = 1f)
		{
			if (EnsureIntegrity())
				return Instance._PlayAttached(eventName, selectorPin, parent, localPosition, loop, volume, pitch, spatialBlend);
			return null;
		}

		private AudioSource _PlayAttached(string eventName, float selectorPin, Transform parent, Vector3 localPosition, bool loop, float volume, float pitch, float spatialBlend)
		{
			if (EnableLogging)
				Log.Info($"Playing {(loop ? "looped" : "one-shot")} '{eventName}'@{selectorPin:N2} (V:{volume:N2} P:{pitch:N2}) attached to '{parent.FullName()}' at local position '{localPosition}'.");
			var audioSource = AllocateAudioSourceWithClip(eventName, selectorPin, true);
			if (!audioSource)
				return null;
			audioSource.transform.SetParent(parent);
			audioSource.transform.localPosition = localPosition;
			SetAudioSourceParametersAndPlay(audioSource, loop, volume, pitch, spatialBlend);
			if (!loop)
			{
				AddToReleaseTracker(audioSource);
			}
			return audioSource;
		}

		public static void Stop(ref AudioSource audioSource)
		{
			if (EnsureIntegrity())
				Instance._Stop(ref audioSource);
		}

		private void _Stop(ref AudioSource audioSource)
		{
			if (audioSource)
			{
				if (EnableLogging)
					Log.Info($"Stopping audio source '{audioSource.gameObject.FullName()}' with clip '{audioSource.clip}'.");

				ReleaseAudioSource(ref audioSource);
			}
		}

		#endregion

		#region Play Music

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
			if (EnsureIntegrity())
				return Instance._PlayMusic(eventName, selectorPin, loop, crossfadeDuration, fadeStartVolume, volume, pitch);
			return null;
		}

		private AudioSource _PlayMusic(string eventName, float selectorPin, bool loop, float crossfadeDuration, float fadeStartVolume, float volume, float pitch)
		{
			if (EnableLogging)
				Log.Info($"Playing {(loop ? "looped" : "one-shot")} music '{eventName}'@{selectorPin:N2} (V:{volume:N2} P:{pitch:N2}).");

			var doFade = crossfadeDuration > 0f;

			var newAudioSource = AllocateAudioSourceWithClip(eventName, selectorPin, true);
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
					AddToReleaseTracker(newAudioSource);
				}
			}

			var oldAudioSource = MusicAudioSource;

			if (doFade)
			{
				StartCoroutine(DoCrossfade(oldAudioSource, newAudioSource, volume, crossfadeDuration, true));
			}
			else
			{
				if (oldAudioSource)
				{
					ReleaseAudioSource(ref oldAudioSource);
				}
			}

			MusicAudioSource = newAudioSource;
			return newAudioSource;
		}

		public static void StopMusic()
		{
			if (EnsureIntegrity())
				Instance._StopMusic();
		}

		private void _StopMusic()
		{
			if (EnableLogging)
				Log.Info("Stopping music.");
			if (!MusicAudioSource)
				return;
			ReleaseAudioSource(ref MusicAudioSource);
		}

		#endregion

		#region Crossfade

		/// <summary>
		/// Fades out one clip while fading in the other clip. It's okay to specify null audio sources
		/// if one of fading in or fading out is not necessary.
		/// </summary>
		public void Crossfade(AudioSource fadingOutSource, AudioSource fadingInSource, float volume, float duration, bool releaseFadedOutAudioSource = true)
		{
			if (EnsureIntegrity())
				Instance.StartCoroutine(Instance.DoCrossfade(fadingOutSource, fadingInSource, volume, duration, releaseFadedOutAudioSource));
		}

		private IEnumerator DoCrossfade(AudioSource fadingOutSource, AudioSource fadingInSource, float volume, float duration, bool releaseFadedOutAudioSource)
		{
			if (!EnsureIntegrity())
				yield break;

			var timeLeft = duration;
			var oneOverDuration = 1f / duration;
			var doFadeOut = fadingOutSource;
			var doFadeIn = fadingInSource;
			var initialFadingOutSourceVolume = doFadeOut ? fadingOutSource.volume : 0f;
			var initialFadingInSourceVolume = doFadeIn ? fadingInSource.volume : 0f;
			var fadingInVolumeRange = volume - initialFadingInSourceVolume;
			while (timeLeft > 0)
			{
				timeLeft -= Time.deltaTime;
				if (timeLeft < 0)
				{
					break;
				}

				// Update audio source volumes
				var ratio = timeLeft * oneOverDuration;
				if (doFadeOut)
					fadingOutSource.volume = initialFadingOutSourceVolume * ratio;
				if (doFadeIn)
					fadingInSource.volume = initialFadingInSourceVolume + fadingInVolumeRange * (1f - ratio);

				// Wait for next update
				yield return null;
			}

			// Finalize
			if (doFadeOut)
			{
				if (releaseFadedOutAudioSource)
				{
					if (EnsureIntegrity()) // Need to be sure that AudioManager still exists after time passed.
						ReleaseAudioSource(ref fadingOutSource);
				}
				else
				{
					fadingOutSource.Stop();
					fadingOutSource.volume = 0f;
				}
			}
			if (doFadeIn)
			{
				fadingInSource.volume = volume;
			}
		}

		#endregion

		#region Incident Tracker

		/*
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
		public Dictionary<uint, int>[] OccurrenceCounts;
		[NonSerialized]
		[ShowInInspector, ReadOnly]
		public readonly HashSet<(uint occurrenceID, OccurrenceType occurrenceType)> ReportedIncidents = new HashSet<(uint occurrenceID, OccurrenceType occurrenceType)>();

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

			OccurrenceCounts = new Dictionary<uint, int>[OccurrenceTypeCount];
			for (int i = 0; i < OccurrenceTypeCount; i++)
			{
				OccurrenceCounts[i] = new Dictionary<uint, int>(InitialIncidentTrackerDictionaryCapacity);
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

		private void InformIncidentTracker(uint occurrenceID, OccurrenceType occurrenceType)
		{
			var dict = OccurrenceCounts[(int)occurrenceType];
			dict.TryGetValue(occurrenceID, out int occurrenceCount);
			occurrenceCount++;
			dict[occurrenceID] = occurrenceCount;

			if (occurrenceCount == IncidentTrackerOccurrenceToleranceInLastSecond[(int)occurrenceType])
			{
				if (EnableIncidentLogging)
				{
					if (ReportedIncidents.Add((occurrenceID, occurrenceType))) // This is here to block repetitive logs. Do not log an incident more than once.
					{
						Log.Any($"Heavy use of '{occurrenceType}' for '{BuildLogString(occurrenceID)}' detected.", IncidentLogType);
					}
				}
			}
		}
		*/

		#endregion

		#region Editor

#if UNITY_EDITOR

		private void OnValidate()
		{
			RefreshEventNamesList();
			CalculateEventInternals();
		}

#endif

		#endregion

		#region Debug

		[Header("Debug")]
		public bool EnableLogging = false;
		public bool EnableVerboseLogging = false;
		public bool EnableWarningLogging = true;

		#endregion

		#region Log

		private static LogRep Log = new LogRep("Audio");

		#endregion

		#region Serialization

		public void OnBeforeSerialize()
		{
			RefreshEventNamesList();
			ClearUnnecessaryReferencesInEvents();
		}

		public void OnAfterDeserialize()
		{
		}

		#endregion
	}

}

#endif
