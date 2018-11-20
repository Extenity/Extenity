#if BeyondAudioUsesWwiseAudio

using UnityEditor;

namespace Extenity.BeyondAudio.Editor
{

	[InitializeOnLoad]
	public static partial class ReferenceFinderDefinitions
	{
		static ReferenceFinderDefinitions()
		{
			// Beyond Audio's general types.
			AddGeneralReferenceFinderDefinitions();

			// Beyond Audio's Wwise Audio implementation specific types.
			//ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(AudioEvent));
		}
	}

}

#endif
