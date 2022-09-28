#if ExtenityAudio

using UnityEngine;

namespace Extenity.Audio
{

	public static class AudioTools
	{
		public static bool IsNotNullAndHasClip(this AudioSource audioSource)
		{
			return audioSource && audioSource.clip;
		}
	}

}

#endif
