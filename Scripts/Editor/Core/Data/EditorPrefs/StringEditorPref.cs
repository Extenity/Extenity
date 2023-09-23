using System;
using UnityEditor;

namespace Extenity.DataToolbox.Editor
{

	public class StringEditorPref : EditorPref<string>
	{
		public StringEditorPref(string prefsKey, PathHashPostfix appendPathHashToKey, string defaultValue, EditorPrefLoggingOptions logOptions)
			: base(prefsKey, appendPathHashToKey, defaultValue, null, logOptions)
		{
		}

		public StringEditorPref(string prefsKey, PathHashPostfix appendPathHashToKey, Func<EditorPref<string>, string> defaultValueOverride, EditorPrefLoggingOptions logOptions)
			: base(prefsKey, appendPathHashToKey, default(string), defaultValueOverride, logOptions)
		{
		}

		protected override string InternalGetValue()
		{
			return EditorPrefs.GetString(ProcessedPrefsKey, _Value);
		}

		protected override void InternalSetValue(string value)
		{
			EditorPrefs.SetString(ProcessedPrefsKey, value);
		}

		protected override bool IsSame(string oldValue, string newValue)
		{
			return oldValue.EqualsOrBothEmpty(newValue, StringComparison.Ordinal);
		}
	}

}
