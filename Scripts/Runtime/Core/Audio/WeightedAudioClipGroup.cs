#if ExtenityAudio

using System;
using UnityEngine;

namespace Extenity.Audio
{

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

		[NonSerialized]
		internal AudioClip LastSelectedAudioClip;

		public void CalculateInternals()
		{
			ClipCount = Clips == null ? 0 : Clips.Length;

			if (Weight < 0f)
				Weight = 0f;
			else if (Weight > 1000f)
				Weight = 1000f;
		}
	}

}

#endif
