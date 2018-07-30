using Extenity.ReflectionToolbox;
using UnityEditor;

namespace TMPro.Extensions.Editor
{

	[InitializeOnLoad]
	public static class ReferenceFinderDefinitions
	{
		static ReferenceFinderDefinitions()
		{
			// TextMesh Pro types
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(FaceInfo));
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(FontAssetCreationSettings));
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(GlyphValueRecord));
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(TMP_TextInfo));
			//ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(TMP_FontAsset)); This causes an error "CS0012: The type `UnityEngine.ScriptableObject' is defined in an assembly that is not referenced." when building Extenity.
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(TMP_FontWeights));
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(TMP_Glyph));
			//ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(TMP_InputField.SubmitEvent)); This causes an error "CS0012: The type `UnityEngine.Events.UnityEvent`1<string>' is defined in an assembly that is not referenced." when building Extenity.
			//ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(TMP_InputField.SelectionEvent)); This causes an error "CS0012: The type `UnityEngine.Events.UnityEvent`1<string>' is defined in an assembly that is not referenced." when building Extenity.
			//ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(TMP_InputField.TextSelectionEvent)); This causes an error "CS0012: The type `UnityEngine.Events.UnityEvent`3<string,int,int>' is defined in an assembly that is not referenced." when building Extenity.
			//ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(TMP_InputField.OnChangeEvent)); This causes an error "CS0012: The type `UnityEngine.Events.UnityEvent`1<string>' is defined in an assembly that is not referenced." when building Extenity.
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(KerningTable));
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(KerningPair));
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(VertexGradient));
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(VertexGradient));
		}
	}

}
