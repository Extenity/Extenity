#if BeyondAudioUsesWwiseAudio

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extenity.DesignPatternsToolbox;
using Extenity.GameObjectToolbox;
using UnityEngine;
using UnityEngine.Events;

namespace Extenity.BeyondAudio
{

	public class AudioManager : SingletonUnity<AudioManager>
	{
		#region Configuration

		public static float CurrentTime { get { return Time.realtimeSinceStartup; } }

		public const float VolumeAdjustmentDb = -80f;

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
			var audioSource = AudioSourceTemplate.GetComponent<AudioSource>();
			audioSource.playOnAwake = false;
		}

		#endregion

		#region Pooled AudioClips

		public class AllocationEvent : UnityEvent<AudioSource, string> { }
		public class DeallocationEvent : UnityEvent<AudioSource> { }
		public readonly AllocationEvent OnAllocatedAudioSource = new AllocationEvent();
		public readonly DeallocationEvent OnReleasingAudioSource = new DeallocationEvent();

		private List<AudioSource> FreeAudioSources = new List<AudioSource>(10);
		private HashSet<AudioSource> ActiveAudioSources = new HashSet<AudioSource>();

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
					if (EnableLogging)
						Log($"Reusing audio source '{reusedAudioSource.gameObject.FullName()}'.");
					ActiveAudioSources.Add(reusedAudioSource);
					return reusedAudioSource;
				}
			}

			var go = Instantiate(AudioSourceTemplate);
			go.name = "Audio Source " + LastCreatedAudioSourceIndex++;
			var newAudioSource = go.GetComponent<AudioSource>();
			DontDestroyOnLoad(go);
			ActiveAudioSources.Add(newAudioSource);
			if (EnableLogging)
				Log($"Created audio source '{go.FullName()}'.");
			return newAudioSource;
		}

		public AudioSource AllocateAudioSourceWithClip(string eventName, float selectorPin, bool errorIfNotFound)
		{
			if (EnableLogging)
				Log($"Allocating audio source for event '{eventName}' with pin '{selectorPin}'.");

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

		public void ReleaseAudioSource(AudioSource audioSource)
		{
			if (EnableLogging)
				Log($"Releasing audio source with clip '{(audioSource && audioSource.clip ? audioSource.clip.name : "N/A")}'.");

			if (!audioSource)
			{
				// Somehow the audio source was already destroyed (or maybe the reference was lost, which we can do nothing about here)
				if (EnableLogging)
					Log("Clearing lost references.");
				ClearLostReferencesInActiveAudioSourcesList();
				ClearLostReferencesInFreeAudioSourcesList();
				ClearLostReferencesInReleaseTrackerList();
				return;
			}

			OnReleasingAudioSource.Invoke(audioSource);

			audioSource.Stop();
			audioSource.clip = null;
			audioSource.outputAudioMixerGroup = null;
			audioSource.gameObject.SetActive(false);
			audioSource.transform.SetParent(null);

			ActiveAudioSources.Remove(audioSource);
			FreeAudioSources.Add(audioSource);

			InternalRemoveFromReleaseTrackerList(audioSource);
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
					ReleaseAudioSource(audioSource);
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
			var instance = InstanceEnsured;
			if (!instance)
				return null;
			if (instance.EnableLogging)
				Log($"Playing {(loop ? "looped" : "one-shot")} '{eventName}'@{selectorPin:N2} (V:{volume:N2} P:{pitch:N2}).");
			var audioSource = instance.AllocateAudioSourceWithClip(eventName, selectorPin, true);
			if (!audioSource)
				return null;
			audioSource.transform.position = Vector3.zero;
			SetAudioSourceParametersAndPlay(audioSource, loop, volume, pitch, 0f);
			if (!loop)
			{
				instance.AddToReleaseTracker(audioSource);
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
			var instance = InstanceEnsured;
			if (!instance)
				return null;
			if (instance.EnableLogging)
				Log($"Playing {(loop ? "looped" : "one-shot")} '{eventName}'@{selectorPin:N2} (V:{volume:N2} P:{pitch:N2}) at position '{position}'.");
			var audioSource = instance.AllocateAudioSourceWithClip(eventName, selectorPin, true);
			if (!audioSource)
				return null;
			audioSource.transform.position = position;
			SetAudioSourceParametersAndPlay(audioSource, loop, volume, pitch, spatialBlend);
			if (!loop)
			{
				instance.AddToReleaseTracker(audioSource);
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
			var instance = InstanceEnsured;
			if (!instance)
				return null;
			if (instance.EnableLogging)
				Log($"Playing {(loop ? "looped" : "one-shot")} '{eventName}'@{selectorPin:N2} (V:{volume:N2} P:{pitch:N2}) attached to '{parent.FullName()}' at local position '{localPosition}'.");
			var audioSource = instance.AllocateAudioSourceWithClip(eventName, selectorPin, true);
			if (!audioSource)
				return null;
			audioSource.transform.SetParent(parent);
			audioSource.transform.localPosition = localPosition;
			SetAudioSourceParametersAndPlay(audioSource, loop, volume, pitch, spatialBlend);
			if (!loop)
			{
				instance.AddToReleaseTracker(audioSource);
			}
			return audioSource;
		}

		public static void Stop(ref AudioSource audioSource)
		{
			var instance = InstanceEnsured;
			if (!instance)
				return;

			if (audioSource)
			{
				if (instance.EnableLogging)
					Log($"Stopping audio source '{audioSource.gameObject.FullName()}' with clip '{audioSource.clip}'.");

				instance.ReleaseAudioSource(audioSource);
				audioSource = null;
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

		#endregion

		#region Debug

		[Header("Debug")]
		public bool EnableLogging;

		/// <summary>
		/// Check for 'EnableLogging' before each Log call to prevent unnecessary string creation.
		/// </summary>
		private static void Log(string message)
		{
			// This must be checked before each Log call manually.
			//if (!EnableLogging)
			//	return;

			Debug.Log("<i><b>AUDIO | </b></i>" + message, Instance);
		}

		#endregion
	}

}

#endif
