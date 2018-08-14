using System;
using UnityEngine;

namespace Extenity.DataToolbox
{

	public class IntPlayerPref : PlayerPref<int>
	{
		public IntPlayerPref(string prefsKey, PathHashPostfix appendPathHashToKey, int defaultValue)
			: base(prefsKey, appendPathHashToKey, defaultValue, null)
		{
		}

		public IntPlayerPref(string prefsKey, PathHashPostfix appendPathHashToKey, Action<PlayerPref<int>> defaultValueOverride)
			: base(prefsKey, appendPathHashToKey, default(int), defaultValueOverride)
		{
		}

		protected override object InternalGetValue()
		{
			return PlayerPrefs.GetInt(ProcessedPrefsKey, _Value);
		}

		protected override void InternalSetValue(object value)
		{
			PlayerPrefs.SetInt(ProcessedPrefsKey, (int)value);
		}

		protected override bool IsSame(int oldValue, int newValue)
		{
			return oldValue == newValue;
		}
	}

}
