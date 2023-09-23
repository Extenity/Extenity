using System;
using Extenity.MathToolbox;
using UnityEditor;

namespace Extenity.DataToolbox.Editor
{

	public class FloatEditorPref : EditorPref<float>
	{
		public FloatEditorPref(string prefsKey, PathHashPostfix appendPathHashToKey, float defaultValue, EditorPrefLogOptions logOptions)
			: base(prefsKey, appendPathHashToKey, defaultValue, null, logOptions)
		{
		}

		public FloatEditorPref(string prefsKey, PathHashPostfix appendPathHashToKey, Func<EditorPref<float>, float> defaultValueOverride, EditorPrefLogOptions logOptions)
			: base(prefsKey, appendPathHashToKey, default(float), defaultValueOverride, logOptions)
		{
		}

		protected override float InternalGetValue()
		{
			return EditorPrefs.GetFloat(ProcessedPrefsKey, _Value);
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
