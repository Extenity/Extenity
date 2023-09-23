using System;
using UnityEditor;

namespace Extenity.DataToolbox.Editor
{

	public class IntEditorPref : EditorPref<int>
	{
		public IntEditorPref(string prefsKey, PathHashPostfix appendPathHashToKey, int defaultValue, EditorPrefLoggingOptions logOptions)
			: base(prefsKey, appendPathHashToKey, defaultValue, null, logOptions)
		{
		}

		public IntEditorPref(string prefsKey, PathHashPostfix appendPathHashToKey, Func<EditorPref<int>, int> defaultValueOverride, EditorPrefLoggingOptions logOptions)
			: base(prefsKey, appendPathHashToKey, default(int), defaultValueOverride, logOptions)
		{
		}

		protected override int InternalGetValue()
		{
			return EditorPrefs.GetInt(ProcessedPrefsKey, _Value);
		}

		protected override void InternalSetValue(int value)
		{
			EditorPrefs.SetInt(ProcessedPrefsKey, value);
		}

		protected override bool IsSame(int oldValue, int newValue)
		{
			return oldValue == newValue;
		}
	}

}
