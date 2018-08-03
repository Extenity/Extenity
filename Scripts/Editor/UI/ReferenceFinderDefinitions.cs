using Extenity.ReflectionToolbox;
using UnityEditor;
using UnityEngine.UI;

namespace Extenity.UIToolbox.Editor
{

	[InitializeOnLoad]
	public static class ReferenceFinderDefinitions
	{
		static ReferenceFinderDefinitions()
		{
			// Unity UI types
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(FontData));
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(InputField.SubmitEvent));
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(InputField.OnChangeEvent));
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(Slider.SliderEvent));
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(Toggle.ToggleEvent));

			// Extenity UI types
			ReflectionTools.KnownTypesOfGameObjectReferenceFinder.Add(typeof(UISimpleAnimationOrchestrator.Entry));
		}
	}

}
