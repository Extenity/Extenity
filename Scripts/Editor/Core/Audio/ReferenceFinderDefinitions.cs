#if ExtenityAudio

using Extenity.ReflectionToolbox;
using UnityEditor;

namespace Extenity.Audio.Editor
{

	[InitializeOnLoad]
	public static class UnityAudioReferenceFinderDefinitions
	{
		static UnityAudioReferenceFinderDefinitions()
		{
			// Beyond Audio's Unity Audio implementation specific types.
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(AudioEvent));
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(WeightedAudioClipGroup));
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(Effects.MotorSound.ClipConfiguration));
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(VolumeControl));
		}
	}

}

#endif
