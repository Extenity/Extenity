using UnityEditor;

namespace Extenity.DataToolbox.Editor
{

	public class IntEditorPref : EditorPref<int>
	{
		public IntEditorPref(string                  prefsKey,
		                     PathHashPostfix         appendPathHashToKey,
		                     DefaultValueMethod<int> defaultValueMethod,
		                     EditorPrefLogOptions    logOptions)
			: base(prefsKey,
			       appendPathHashToKey,
			       defaultValueMethod,
			       logOptions)
		{
		}

		protected override int InternalGetValue()
		{
			// Default value has no effect here, because it was already handled before calling this function.
			return EditorPrefs.GetInt(ProcessedPrefsKey, default);
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