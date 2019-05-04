using System;
using UnityEditor;

namespace Extenity.DataToolbox.Editor
{

	public class BoolEditorPref : EditorPref<bool>
	{
		public BoolEditorPref(string prefsKey, PathHashPostfix appendPathHashToKey, bool defaultValue)
			: base(prefsKey, appendPathHashToKey, defaultValue, null)
		{
		}

		public BoolEditorPref(string prefsKey, PathHashPostfix appendPathHashToKey, Func<EditorPref<bool>, bool> defaultValueOverride)
			: base(prefsKey, appendPathHashToKey, default(bool), defaultValueOverride)
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
