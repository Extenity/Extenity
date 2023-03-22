#if ExtenityAudio

using System;
using System.Collections.Generic;
using Extenity.MathToolbox;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;

namespace Extenity.Audio
{

	public enum AudioEventType
	{
		Regular,
		WeightedGroups,
	}

	[Serializable]
	public class AudioEvent
	{
		public string Name;
		public AudioMixerGroup Output;
		public AudioEventType Type = AudioEventType.Regular;

		[ShowIf(nameof(Type), AudioEventType.Regular)]
		public List<AudioClip> Clips; // TODO: Change type to AudioClip[]

		[ShowIf(nameof(Type), AudioEventType.WeightedGroups)]
		public WeightedAudioClipGroup[] WeightedGroups;

		[ShowIf(nameof(HasAnyClips))]
		public bool EnsureNonrecurringRandomness = true;
		private AudioClip LastSelectedAudioClip;

		/// <summary>
		/// CAUTION! This is automatically calculated. Do not change this unless you know what you do.
		/// </summary>
		[NonSerialized]
		internal int ClipCount;

		private bool HasAnyClips => ClipCount > 1;

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
						if (WeightedGroups == null || WeightedGroups.Length == 0)
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

		public AudioClip SelectRandomClip(float selectorPin, bool errorIfNotFound)
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
							if (errorIfNotFound && !clip)
							{
								Log.Error($"There is a null clip in sound event '{Name}'.");
							}
						}
						else
						{
							clip = null;
							if (errorIfNotFound)
							{
								Log.Error($"There are no clips in sound event '{Name}'.");
							}
						}
					}
					break;
				case AudioEventType.WeightedGroups:
					{
						if (ClipCount > 0 && WeightedGroups.Length > 0)
						{
							var weightedGroupIndex = -1;
							if (WeightedGroups.Length == 1)
							{
								weightedGroupIndex = 0;
							}
							else
							{
								if (selectorPin <= 0.001f) // Equal to zero
								{
									weightedGroupIndex = 0;
								}
								else if (selectorPin >= 0.999f) // Equal to one
								{
									weightedGroupIndex = WeightedGroups.Length - 1;
								}
								else
								{
									for (int i = WeightedGroups.Length - 2; i >= 0; i--)
									{
										if (selectorPin < WeightedGroups[i].SeparatorPositionBetweenNextGroup)
										{
											weightedGroupIndex = i;
											break;
										}
									}
									if (weightedGroupIndex < 0)
									{
										weightedGroupIndex = WeightedGroups.Length - 1;
									}
								}
							}

							var weightedGroup = WeightedGroups[weightedGroupIndex];
							clip = EnsureNonrecurringRandomness && weightedGroup.LastSelectedAudioClip
								? weightedGroup.Clips.RandomSelectionFilteredSafe(weightedGroup.LastSelectedAudioClip)
								: weightedGroup.Clips.RandomSelection();
							weightedGroup.LastSelectedAudioClip = clip;

							if (errorIfNotFound && !clip)
							{
								Log.Error($"There is a null clip in weighted group '{weightedGroupIndex}' of sound event '{Name}'.");
							}
						}
						else
						{
							clip = null;
							if (errorIfNotFound)
							{
								Log.Error($"There are no clips in sound event '{Name}'.");
							}
						}
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
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

		#region Log

		private static readonly Logger Log = new(nameof(AudioEvent));

		#endregion
	}

}

#endif
