#if BeyondAudioUsesUnityAudio

using UnityEngine;

namespace Extenity.BeyondAudio
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
