using System;
using UnityEditor;

namespace Extenity.DataToolbox.Editor
{

	public class StringEditorPref : EditorPref<string>
	{
		public StringEditorPref(string                     prefsKey,
		                        PathHashPostfix            appendPathHashToKey,
		                        DefaultValueMethod<string> defaultValueMethod,
		                        EditorPrefLogOptions       logOptions)
			: base(prefsKey,
			       appendPathHashToKey,
			       defaultValueMethod,
			       logOptions)
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