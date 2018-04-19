using Extenity.ReflectionToolbox;
using UnityEditor;

namespace TMPro.Extensions.Editor
{

	[InitializeOnLoad]
	public static class ReferenceFinderDefinitions
	{
		static ReferenceFinderDefinitions()
		{
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(FaceInfo));
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(TMP_TextInfo));
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(TMP_FontAsset));
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(TMP_FontWeights));
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(TMP_Glyph));
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(KerningTable));
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(KerningPair));
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(FontCreationSetting));
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(VertexGradient));
		}
	}

}
