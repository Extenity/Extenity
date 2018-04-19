using Extenity.ReflectionToolbox;
using UnityEditor;

namespace Extenity.BeyondAudio.Editor
{

	[InitializeOnLoad]
	public static class ReferenceFinderDefinitions
	{
		static ReferenceFinderDefinitions()
		{
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(AudioEvent));
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(VolumeControl));
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(WeightedAudioClipGroup));
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(Effects.MotorSound.ClipConfiguration));
		}
	}

}
