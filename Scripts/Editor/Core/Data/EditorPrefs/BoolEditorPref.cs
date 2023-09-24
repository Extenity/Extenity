using UnityEditor;

namespace Extenity.DataToolbox.Editor
{

	public class BoolEditorPref : EditorPref<bool>
	{
		public BoolEditorPref(string                   prefsKey,
		                      PathHashPostfix          appendPathHashToKey,
		                      DefaultValueMethod<bool> defaultValueMethod,
		                      EditorPrefLogOptions     logOptions)
			: base(prefsKey,
			       appendPathHashToKey,
			       defaultValueMethod,
			       logOptions)
		{
		}

		protected override bool InternalGetValue()
		{
			// Default value has no effect here, because it was already handled before calling this function.
			return EditorPrefs.GetBool(ProcessedPrefsKey, default);
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