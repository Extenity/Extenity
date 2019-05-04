using System;
using UnityEditor;

namespace Extenity.DataToolbox.Editor
{

	public class IntEditorPref : EditorPref<int>
	{
		public IntEditorPref(string prefsKey, PathHashPostfix appendPathHashToKey, int defaultValue)
			: base(prefsKey, appendPathHashToKey, defaultValue, null)
		{
		}

		public IntEditorPref(string prefsKey, PathHashPostfix appendPathHashToKey, Func<EditorPref<int>, int> defaultValueOverride)
			: base(prefsKey, appendPathHashToKey, default(int), defaultValueOverride)
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
