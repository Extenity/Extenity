#if BeyondAudioUsesUnityAudio

using Extenity.ReflectionToolbox;
using UnityEditor;

namespace Extenity.BeyondAudio.Editor
{

	[InitializeOnLoad]
	public static class ReferenceFinderDefinitions
	{
		static ReferenceFinderDefinitions()
		{
			// Beyond Audio's general types.
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(VolumeControl));

			// Beyond Audio's Unity Audio implementation specific types.
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(AudioEvent));
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(WeightedAudioClipGroup));
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(Effects.MotorSound.ClipConfiguration));
		}
	}

}

#endif
