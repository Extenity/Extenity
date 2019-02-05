using Extenity.ReflectionToolbox;
using UnityEditor;

namespace Extenity.BeyondAudio.Editor
{

	[InitializeOnLoad]
	public static class ReferenceFinderDefinitions
	{
		private static void AddGeneralReferenceFinderDefinitions()
		{
			// Beyond Audio's general types.
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(VolumeControl));
		}
	}

}
