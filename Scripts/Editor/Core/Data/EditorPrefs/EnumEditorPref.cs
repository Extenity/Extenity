using System;
using UnityEditor;

namespace Extenity.DataToolbox.Editor
{

	public class EnumEditorPref<TEnum> : EditorPref<TEnum> where TEnum : Enum
	{
		public EnumEditorPref(string                    prefsKey,
		                      PathHashPostfix           appendPathHashToKey,
		                      DefaultValueMethod<TEnum> defaultValueMethod,
		                      EditorPrefLogOptions      logOptions)
			: base(prefsKey,
			       appendPathHashToKey,
			       defaultValueMethod,
			       logOptions)
		{
		}

		protected override TEnum InternalGetValue()
		{
			// Default value has no effect here, because it was already handled before calling this function.
			var intValue = EditorPrefs.GetInt(ProcessedPrefsKey, default);
			return (TEnum)(object)intValue;
		}

		protected override void InternalSetValue(TEnum value)
		{
			int intValue = (int)Convert.ChangeType(value, typeof(int));
			EditorPrefs.SetInt(ProcessedPrefsKey, intValue);
		}

		protected override bool IsSame(TEnum oldValue, TEnum newValue)
		{
			return object.Equals(oldValue, newValue);
		}
	}

}