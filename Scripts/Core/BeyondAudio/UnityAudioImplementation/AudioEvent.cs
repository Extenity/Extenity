//#if BeyondAudioUsesUnityAudio

using System;
using System.Collections.Generic;
using Extenity.MathToolbox;
using UnityEngine;
using UnityEngine.Audio;

namespace Extenity.BeyondAudio
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

}

//#endif
