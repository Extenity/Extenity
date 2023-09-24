using Extenity.MathToolbox;
using UnityEditor;

namespace Extenity.DataToolbox.Editor
{

	public class FloatEditorPref : EditorPref<float>
	{
		public FloatEditorPref(string                    prefsKey,
		                       PathHashPostfix           appendPathHashToKey,
		                       DefaultValueMethod<float> defaultValueMethod,
		                       EditorPrefLogOptions      logOptions)
			: base(prefsKey,
			       appendPathHashToKey,
			       defaultValueMethod,
			       logOptions)
		{
		}

		protected override float InternalGetValue()
		{
			// Default value has no effect here, because it was already handled before calling this function.
			return EditorPrefs.GetFloat(ProcessedPrefsKey, default);
		}

		protected override void InternalSetValue(float value)
		{
			EditorPrefs.SetFloat(ProcessedPrefsKey, value);
		}

		protected override bool IsSame(float oldValue, float newValue)
		{
			return oldValue.IsAlmostEqual(newValue, 1E-05f);
		}
	}

}