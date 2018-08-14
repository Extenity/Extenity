using System;
using UnityEngine;

namespace Extenity.DataToolbox
{

	public class StringPlayerPref : PlayerPref<string>
	{
		public StringPlayerPref(string prefsKey, PathHashPostfix appendPathHashToKey, string defaultValue)
			: base(prefsKey, appendPathHashToKey, defaultValue, null)
		{
		}

		public StringPlayerPref(string prefsKey, PathHashPostfix appendPathHashToKey, Action<PlayerPref<string>> defaultValueOverride)
			: base(prefsKey, appendPathHashToKey, default(string), defaultValueOverride)
		{
		}

		protected override object InternalGetValue()
		{
			return PlayerPrefs.GetString(ProcessedPrefsKey, _Value);
		}

		protected override void InternalSetValue(object value)
		{
			PlayerPrefs.SetString(ProcessedPrefsKey, (string)value);
		}

		protected override bool IsSame(string oldValue, string newValue)
		{
			return oldValue.EqualsOrBothEmpty(newValue);
		}
	}

}
