using Extenity.ReflectionToolbox;

namespace Extenity.BeyondAudio.Editor
{

	public static partial class ReferenceFinderDefinitions
	{
		private static void AddGeneralReferenceFinderDefinitions()
		{
			// Beyond Audio's general types.
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(VolumeControl));
		}
	}

}
