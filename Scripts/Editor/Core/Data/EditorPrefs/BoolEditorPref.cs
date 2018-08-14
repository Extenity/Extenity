using UnityEditor;

namespace Extenity.DataToolbox.Editor
{

	public class BoolEditorPref
	{
		public BoolEditorPref(string prefsKey, bool defaultValue)
		{
			PrefsKey = prefsKey;
			_Value = defaultValue;
		}

		public readonly string PrefsKey;

		private bool _IsInitialized;
		private bool _Value;
		public bool Value
		{
			get
			{
				if (!_IsInitialized)
				{
					_IsInitialized = true;
					_Value = EditorPrefs.GetBool(PrefsKey, _Value);
				}
				return _Value;
			}
			set
			{
				_IsInitialized = true;
				_Value = value;
				EditorPrefs.SetBool(PrefsKey, value);
			}
		}

		public bool Toggle()
		{
			Value = !Value;
			return Value;
		}
	}

}
