#if BeyondAudioUsesWwiseAudio

using UnityEditor;

namespace Extenity.BeyondAudio.Editor
{

	[InitializeOnLoad]
	public static class WwiseReferenceFinderDefinitions
	{
		static WwiseReferenceFinderDefinitions()
		{
			// Beyond Audio's Wwise Audio implementation specific types.
			//ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(AudioEvent));
		}
	}

}

#endif
