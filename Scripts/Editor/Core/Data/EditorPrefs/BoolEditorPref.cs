using System;
using UnityEditor;

namespace Extenity.DataToolbox.Editor
{

	public class BoolEditorPref : EditorPref<bool>
	{
		public BoolEditorPref(string prefsKey, PathHashPostfix appendPathHashToKey, bool defaultValue, EditorPrefLoggingOptions logOptions)
			: base(prefsKey, appendPathHashToKey, defaultValue, null, logOptions)
		{
		}

		public BoolEditorPref(string prefsKey, PathHashPostfix appendPathHashToKey, Func<EditorPref<bool>, bool> defaultValueOverride, EditorPrefLoggingOptions logOptions)
			: base(prefsKey, appendPathHashToKey, default(bool), defaultValueOverride, logOptions)
		{
		}

		protected override bool InternalGetValue()
		{
			return EditorPrefs.GetBool(ProcessedPrefsKey, _Value);
		}

		protected override void InternalSetValue(bool value)
		{
			EditorPrefs.SetBool(ProcessedPrefsKey, value);
		}

		protected override bool IsSame(bool oldValue, bool newValue)
		{
			return oldValue == newValue;
		}
	}

}
