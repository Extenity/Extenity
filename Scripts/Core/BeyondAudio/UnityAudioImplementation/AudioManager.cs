//#if BeyondAudioUsesUnityAudio

using System;
using System.Collections;
using System.Collections.Generic;
using Extenity.DesignPatternsToolbox;
using Extenity.MathToolbox;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

namespace Extenity.BeyondAudio
{

	public class AudioManager : SingletonUnity<AudioManager>, ISerializationCallbackReceiver
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
			CalculateEventInternals();
			InitializeAudioSourceTemplate();
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

		public VolumeControl GetVolumeControl(string mixerParameterName)
		{
			for (var i = 0; i < MixerVolumeControls.Length; i++)
			{
				var volumeControl = MixerVolumeControls[i];
				if (volumeControl.MixerParameterName == mixerParameterName)
					return volumeControl;
			}
			Debug.LogErrorFormat("Volume control '{0}' does not exist.", mixerParameterName);
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
					ActiveAudioSources.Add(reusedAudioSource);
					return reusedAudioSource;
				}
			}

			var go = Instantiate(AudioSourceTemplate);
			go.name = "Audio Source " + LastCreatedAudioSourceIndex++;
			var newAudioSource = go.GetComponent<AudioSource>();
			DontDestroyOnLoad(go);
			ActiveAudioSources.Add(newAudioSource);
			return newAudioSource;
		}

		public static AudioSource AllocateAudioSourceWithClip(string eventName, float selectorPin, bool errorIfNotFound)
		{
			var instance = InstanceEnsured;
			if (!instance)
				return null;

			var audioEvent = instance.GetEvent(eventName, errorIfNotFound);
			if (audioEvent == null)
				return null;
			var clip = audioEvent.SelectRandomClip(eventName, selectorPin, errorIfNotFound);
			if (!clip)
				return null;
			var audioSource = instance.GetOrCreateAudioSource();
			audioSource.clip = clip;
			audioSource.outputAudioMixerGroup = audioEvent.Output;
			instance.OnAllocatedAudioSource.Invoke(audioSource, eventName);
			return audioSource;
		}

		public static void ReleaseAudioSource(AudioSource audioSource)
		{
			var instance = InstanceEnsured;
			if (!instance)
				return;

			if (!audioSource)
			{
				// Somehow the audio source was already destroyed (or maybe the reference was lost, which we can do nothing about here)
				instance.ClearLostReferencesInActiveAudioSourcesList();
				instance.ClearLostReferencesInFreeAudioSourcesList();
				instance.ClearLostReferencesInReleaseTrackerList();
				return;
			}

			instance.OnReleasingAudioSource.Invoke(audioSource);

			audioSource.Stop();
			audioSource.clip = null;
			audioSource.outputAudioMixerGroup = null;
			audioSource.gameObject.SetActive(false);
			audioSource.transform.SetParent(null);

			instance.ActiveAudioSources.Remove(audioSource);
			instance.FreeAudioSources.Add(audioSource);

			instance.InternalRemoveFromReleaseTrackerList(audioSource);
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

		#region Events

		public enum AudioEventType
		{
			Regular,
			WeightedGroups,
		}

		[Serializable]
		public struct WeightedAudioClipGroup
		{
			public AudioClip[] Clips;

			/// <summary>
			/// The weight of this group. The odds will be high with the group that has more weight than others when selecting an AudioClip randomly.
			/// </summary>
			public float Weight;

			/// <summary>
			/// CAUTION! This is automatically calculated. Do not change this unless you know what you do.
			/// </summary>
			[NonSerialized]
			internal float SeparatorPositionBetweenNextGroup;

			/// <summary>
			/// CAUTION! This is automatically calculated. Do not change this unless you know what you do.
			/// </summary>
			[NonSerialized]
			internal int ClipCount;

			public void CalculateInternals()
			{
				ClipCount = Clips == null ? 0 : Clips.Length;

				if (Weight < 0f)
					Weight = 0f;
				else if (Weight > 1000f)
					Weight = 1000f;
			}
		}

		[Serializable]
		public class AudioEvent
		{
			public string Name;
			public AudioMixerGroup Output;
			public AudioEventType Type = AudioEventType.Regular;

			// Type.Regular
			public List<AudioClip> Clips; // TODO: Change type to AudioClip[]

			// Type.WeightedGroups
			public WeightedAudioClipGroup[] WeightedGroups;

			public bool EnsureNonrecurringRandomness = true; // TODO: Show only when assigned more than one clip
			private AudioClip LastSelectedAudioClip;

			/// <summary>
			/// CAUTION! This is automatically calculated. Do not change this unless you know what you do.
			/// </summary>
			[NonSerialized]
			internal int ClipCount;

			public bool HasAnyUnassignedClip
			{
				get
				{
					switch (Type)
					{
						case AudioEventType.Regular:
							{
								for (int i = 0; i < Clips.Count; i++)
								{
									if (!Clips[i])
										return true;
								}
								return false;
							}
						case AudioEventType.WeightedGroups:
							{
								for (var iGroup = 0; iGroup < WeightedGroups.Length; iGroup++)
								{
									for (int i = 0; i < WeightedGroups[iGroup].Clips.Length; i++)
									{
										if (!WeightedGroups[iGroup].Clips[i])
											return true;
									}
								}
								return false;
							}
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
			}

			internal void CalculateInternals()
			{
				switch (Type)
				{
					case AudioEventType.Regular:
						{
							ClipCount = Clips == null ? 0 : Clips.Count;
						}
						break;
					case AudioEventType.WeightedGroups:
						{
							if (WeightedGroups == null)
							{
								ClipCount = 0;
							}
							else
							{
								// Clip count
								int totalCount = 0;
								float totalWeight = 0f;
								for (var i = 0; i < WeightedGroups.Length; i++)
								{
									WeightedGroups[i].CalculateInternals();
									totalCount += WeightedGroups[i].ClipCount;
									totalWeight += WeightedGroups[i].Weight;
								}
								ClipCount = totalCount;

								// Weights
								float oneOverTotalWeight = 1f / totalWeight;
								float processedTotalWeight = 0f;
								for (var i = 0; i < WeightedGroups.Length - 1; i++)
								{
									processedTotalWeight += WeightedGroups[i].Weight;
									WeightedGroups[i].SeparatorPositionBetweenNextGroup = processedTotalWeight * oneOverTotalWeight;
								}
								WeightedGroups[WeightedGroups.Length - 1].SeparatorPositionBetweenNextGroup = 1f;
							}
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			public AudioClip SelectRandomClip(string eventName, float selectorPin, bool errorIfNotFound)
			{
				AudioClip clip;

				switch (Type)
				{
					case AudioEventType.Regular:
						{
							if (ClipCount > 0)
							{
								if (ClipCount > 1)
								{
									clip = EnsureNonrecurringRandomness && LastSelectedAudioClip
										? Clips.RandomSelectionFilteredSafe(LastSelectedAudioClip)
										: Clips.RandomSelection();
								}
								else
								{
									clip = Clips[0];
								}
							}
							else
							{
								clip = null;
							}
						}
						break;
					case AudioEventType.WeightedGroups:
						{
							if (ClipCount > 0)
							{
								if (ClipCount > 1)
								{
									if (WeightedGroups.Length == 1)
									{
										clip = EnsureNonrecurringRandomness && LastSelectedAudioClip
											? WeightedGroups[0].Clips.RandomSelectionFilteredSafe(LastSelectedAudioClip)
											: WeightedGroups[0].Clips.RandomSelection();
									}
									else
									{
										if (selectorPin <= 0.001f) // Equal to zero
										{
											clip = EnsureNonrecurringRandomness && LastSelectedAudioClip
												? WeightedGroups[0].Clips.RandomSelectionFilteredSafe(LastSelectedAudioClip)
												: WeightedGroups[0].Clips.RandomSelection();
										}
										else if (selectorPin >= 0.999f) // Equal to one
										{
											clip = EnsureNonrecurringRandomness && LastSelectedAudioClip
												? WeightedGroups[WeightedGroups.Length - 1].Clips.RandomSelectionFilteredSafe(LastSelectedAudioClip)
												: WeightedGroups[WeightedGroups.Length - 1].Clips.RandomSelection();
										}
										else
										{
											clip = null; // This is here to make compiler happy.
											int i;
											for (i = WeightedGroups.Length - 2; i >= 0; i--)
											{
												if (selectorPin > WeightedGroups[i].SeparatorPositionBetweenNextGroup)
												{
													clip = EnsureNonrecurringRandomness && LastSelectedAudioClip
														? WeightedGroups[i + 1].Clips.RandomSelectionFilteredSafe(LastSelectedAudioClip)
														: WeightedGroups[i + 1].Clips.RandomSelection();
												}
											}
											if (i < 0)
											{
												clip = EnsureNonrecurringRandomness && LastSelectedAudioClip
													? WeightedGroups[0].Clips.RandomSelectionFilteredSafe(LastSelectedAudioClip)
													: WeightedGroups[0].Clips.RandomSelection();
											}
										}
									}
								}
								else
								{
									// Quickly find the single clip that is out there somewhere in WeightedGroups.
									if (WeightedGroups.Length == 1)
									{
										clip = WeightedGroups[0].Clips[0];
									}
									else
									{
										clip = null; // This is here to make compiler happy.
										for (var i = 0; i < WeightedGroups.Length; i++)
										{
											if (WeightedGroups[i].ClipCount > 0)
												clip = WeightedGroups[i].Clips[0];
										}
									}
								}
							}
							else
							{
								clip = null;
							}
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				if (errorIfNotFound && !clip)
				{
					Debug.LogErrorFormat("There is a null clip in sound event '{0}'.", eventName);
				}

				LastSelectedAudioClip = clip;
				return clip;
			}

			internal void ClearUnnecessaryReferences()
			{
				switch (Type)
				{
					case AudioEventType.Regular:
						{
							WeightedGroups = null;
						}
						break;
					case AudioEventType.WeightedGroups:
						{
							Clips = null;
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		[Header("Audio Events")]
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
			if (Events != null)
			{
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
							list = new List<AudioEvent>(10);
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
			var audioSource = AllocateAudioSourceWithClip(eventName, selectorPin, true);
			if (!audioSource)
				return null;
			audioSource.transform.position = Vector3.zero;
			SetAudioSourceParametersAndPlay(audioSource, loop, volume, pitch, 0f);
			if (!loop)
			{
				Instance.AddToReleaseTracker(audioSource);
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
			var audioSource = AllocateAudioSourceWithClip(eventName, selectorPin, true);
			if (!audioSource)
				return null;
			audioSource.transform.position = position;
			SetAudioSourceParametersAndPlay(audioSource, loop, volume, pitch, spatialBlend);
			if (!loop)
			{
				Instance.AddToReleaseTracker(audioSource);
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
			var audioSource = AllocateAudioSourceWithClip(eventName, selectorPin, true);
			if (!audioSource)
				return null;
			audioSource.transform.SetParent(parent);
			audioSource.transform.localPosition = localPosition;
			SetAudioSourceParametersAndPlay(audioSource, loop, volume, pitch, spatialBlend);
			if (!loop)
			{
				Instance.AddToReleaseTracker(audioSource);
			}
			return audioSource;
		}

		public static void Stop(ref AudioSource audioSource)
		{
			if (audioSource)
			{
				audioSource.Stop();
				ReleaseAudioSource(audioSource);
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
					Instance.AddToReleaseTracker(newAudioSource);
				}
			}

			var oldAudioSource = Instance.MusicAudioSource;

			if (doFade)
			{
				Instance.StartCoroutine(DoCrossfade(oldAudioSource, newAudioSource, volume, crossfadeDuration, true));
			}
			else
			{
				if (oldAudioSource)
				{
					ReleaseAudioSource(oldAudioSource);
				}
			}

			Instance.MusicAudioSource = newAudioSource;
			return newAudioSource;
		}

		public static void StopMusic()
		{
			if (!Instance.MusicAudioSource)
				return;
			ReleaseAudioSource(Instance.MusicAudioSource);
		}

		#endregion

		#region Crossfade

		/// <summary>
		/// Fades out one clip while fading in the other clip. It's okay to specify null audio sources
		/// if one of fading in or fading out is not necessary.
		/// </summary>
		public void Crossfade(AudioSource fadingOutSource, AudioSource fadingInSource, float volume, float duration, bool releaseFadedOutAudioSource = true)
		{
			StartCoroutine(DoCrossfade(fadingOutSource, fadingInSource, volume, duration, releaseFadedOutAudioSource));
		}

		private static IEnumerator DoCrossfade(AudioSource fadingOutSource, AudioSource fadingInSource, float volume, float duration, bool releaseFadedOutAudioSource)
		{
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
					ReleaseAudioSource(fadingOutSource);
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

		#region Editor

		private void OnValidate()
		{
			CalculateEventInternals();
		}

		#endregion

		#region Serialization

		public void OnBeforeSerialize()
		{
			ClearUnnecessaryReferencesInEvents();
		}

		public void OnAfterDeserialize()
		{
		}

		#endregion
	}

}

//#endif
