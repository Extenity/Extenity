//#if BeyondAudioUsesUnityAudio

using System;
using System.Collections.Generic;
using Extenity.DesignPatternsToolbox;
using Extenity.MathToolbox;
using UnityEngine;
using UnityEngine.Audio;

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
					Debug.LogErrorFormat("AudioManager is not initialized yet.");
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

		#region Master Audio Mixer

		[Header("Audio Mixers")]
		public AudioMixer MasterAudioMixer;
		public string MasterVolumeParameterName = "MasterVolume";

		public float MasterVolume
		{
			get
			{
				float value;
				MasterAudioMixer.GetFloat(MasterVolumeParameterName, out value);
				return ToNormalizedRange(value);
			}
			set
			{
				MasterAudioMixer.SetFloat(MasterVolumeParameterName, ToDbRange(value));
			}
		}

		public void ToggleMuteMaster()
		{
			var volume = MasterVolume;
			if (volume < 0.05f)
			{
				MasterVolume = 1f;
			}
			else
			{
				MasterVolume = 0f;
			}
		}

		#endregion

		#region Music/Effect Volumes

		public float MusicVolume
		{
			get
			{
				float value;
				MasterAudioMixer.GetFloat("MusicVolume", out value);
				return DbToNormalizedRange(value);
			}
			set { MasterAudioMixer.SetFloat("MusicVolume", NormalizedToDbRange(value)); }
		}

		public float EffectsVolume
		{
			get
			{
				float value;
				MasterAudioMixer.GetFloat("EffectsVolume", out value);
				return DbToNormalizedRange(value);
			}
			set { MasterAudioMixer.SetFloat("EffectsVolume", NormalizedToDbRange(value)); }
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

		public GameObject AudioSourceTemplate;

		private void InitializeAudioSourceTemplate()
		{
			AudioSourceTemplate.SetActive(false);
			var audioSource = AudioSourceTemplate.GetComponent<AudioSource>();
			audioSource.playOnAwake = false;
		}

		#endregion

		#region Pooled AudioClips

		private List<AudioSource> FreeAudioSources = new List<AudioSource>(10);
		private HashSet<AudioSource> ActiveAudioSources = new HashSet<AudioSource>();

		private static int LastCreatedAudioSourceIndex = 0;

		private AudioSource GetOrCreateAudioSource()
		{
			if (FreeAudioSources.Count > 0)
			{
				var index = FreeAudioSources.Count - 1;
				var obj = FreeAudioSources[index];
				FreeAudioSources.RemoveAt(index);
				ActiveAudioSources.Add(obj);
				return obj;
			}
			else
			{
				var go = Instantiate(AudioSourceTemplate);
				go.name = "Audio Source " + LastCreatedAudioSourceIndex++;
				var obj = go.GetComponent<AudioSource>();
				DontDestroyOnLoad(go);
				ActiveAudioSources.Add(obj);
				return obj;
			}
		}

		public static AudioSource AllocateAudioSourceWithClip(string eventName, bool errorIfNotFound)
		{
			var instance = InstanceEnsured;
			if (!instance)
				return null;

			var audioEvent = instance.GetEvent(eventName, errorIfNotFound);
			if (audioEvent == null)
				return null;
			var clip = audioEvent.SelectRandomClip(eventName, errorIfNotFound);
			if (!clip)
				return null;
			var audioSource = instance.GetOrCreateAudioSource();
			audioSource.clip = clip;
			audioSource.outputAudioMixerGroup = audioEvent.Output;
			return audioSource;
		}

		public static void ReleaseAudioSource(AudioSource audioSource)
		{
			var instance = InstanceEnsured;
			if (!instance)
				return;

			audioSource.gameObject.SetActive(false);
			audioSource.clip = null;
			audioSource.outputAudioMixerGroup = null;

			instance.ActiveAudioSources.Remove(audioSource);
			instance.FreeAudioSources.Add(audioSource);

			instance.InternalRemoveFromReleaseTrackerList(audioSource);
		}

		#endregion

		#region Events

		[Serializable]
		public class AudioEvent
		{
			public string Name;
			public AudioMixerGroup Output;
			public List<AudioClip> Clips;

			public bool HasAnyUnassignedClip
			{
				get
				{
					for (int i = 0; i < Clips.Count; i++)
					{
						if (!Clips[i])
							return true;
					}
					return false;
				}
			}

			public AudioClip SelectRandomClip(string eventName, bool errorIfNotFound)
			{
				if (Clips != null && Clips.Count != 0)
				{
					var clip = Clips.RandomSelection();
					if (clip)
					{
						return clip;
					}
					else if (errorIfNotFound)
					{
						Debug.LogErrorFormat("There is a null clip in sound event '{0}'.", eventName);
					}
				}
				else
				{
					if (errorIfNotFound)
					{
						Debug.LogErrorFormat("There is no clip in sound event '{0}'.", eventName);
					}
				}
				return null;
			}
		}

		public List<AudioEvent> Events;

		public AudioEvent GetEvent(string eventName, bool errorIfNotFound)
		{
			if (string.IsNullOrEmpty(eventName))
				return null;

			if (Events != null)
			{
				for (int i = 0; i < Events.Count; i++)
				{
					var audioEvent = Events[i];
					if (audioEvent.Name == eventName)
						return audioEvent;
				}
			}
			if (errorIfNotFound)
			{
				Debug.LogErrorFormat("Failed to find sound event '{0}'.", eventName);
			}
			return null;
		}

		public List<AudioEvent> ListEventsWithUnassignedOutputs()
		{
			List<AudioEvent> list = null;
			for (var i = 0; i < Events.Count; i++)
			{
				var audioEvent = Events[i];
				if (!audioEvent.Output)
				{
					if (list == null)
						list = new List<AudioEvent>(10);
					list.Add(audioEvent);
				}
			}
			return list;
		}


		public List<AudioEvent> ListEventsWithUnassignedClips()
		{
			List<AudioEvent> list = null;
			for (var i = 0; i < Events.Count; i++)
			{
				var audioEvent = Events[i];
				if (audioEvent.HasAnyUnassignedClip)
				{
					if (list == null)
						list = new List<AudioEvent>(10);
					list.Add(audioEvent);
				}
			}
			return list;
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

		#endregion

		#region Play One Shot

		public static AudioSource Play(string eventName, float volume = 1f, float pitch = 1f)
		{
			var audioSource = AllocateAudioSourceWithClip(eventName, true);
			if (!audioSource)
				return null;
			audioSource.transform.position = Vector3.zero;
			audioSource.loop = false;
			audioSource.pitch = pitch;
			audioSource.volume = volume;
			audioSource.spatialBlend = 0f;
			audioSource.gameObject.SetActive(true);
			audioSource.Play();
			Instance.AddToReleaseTracker(audioSource);
			return audioSource;
		}

		public static AudioSource PlayAtPosition(string eventName, Vector3 position, float volume = 1f, float pitch = 1f, float spatialBlend = 1f)
		{
			var audioSource = AllocateAudioSourceWithClip(eventName, true);
			if (!audioSource)
				return null;
			audioSource.transform.position = position;
			audioSource.loop = false;
			audioSource.pitch = pitch;
			audioSource.volume = volume;
			audioSource.spatialBlend = spatialBlend;
			audioSource.gameObject.SetActive(true);
			audioSource.Play();
			Instance.AddToReleaseTracker(audioSource);
			return audioSource;
		}

		public static AudioSource PlayLooped(string eventName, float volume = 1f, float pitch = 1f)
		{
			var audioSource = AllocateAudioSourceWithClip(eventName, true);
			if (!audioSource)
				return null;
			audioSource.transform.position = Vector3.zero;
			audioSource.loop = true;
			audioSource.pitch = pitch;
			audioSource.volume = volume;
			audioSource.spatialBlend = 0f;
			audioSource.gameObject.SetActive(true);
			audioSource.Play();
			return audioSource;
		}

		#endregion

		#region Tools

		public static float ToNormalizedRange(float value)
		{
			return value.Remap(-80f, 0f, 0f, 1f);
		}

		public static float ToDbRange(float value)
		{
			return value.Remap(0f, 1f, -80f, 0f);
		}

		#endregion
	}

}

//#endif
