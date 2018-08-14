using System;
using UnityEngine;

namespace Extenity.DataToolbox
{

	public class IntPlayerPref : PlayerPref<int>
	{
		public IntPlayerPref(string prefsKey, PathHashPostfix appendPathHashToKey, int defaultValue, float saveDelay = 0f)
			: base(prefsKey, appendPathHashToKey, defaultValue, null, saveDelay)
		{
		}

		public IntPlayerPref(string prefsKey, PathHashPostfix appendPathHashToKey, Action<PlayerPref<int>> defaultValueOverride, float saveDelay = 0f)
			: base(prefsKey, appendPathHashToKey, default(int), defaultValueOverride, saveDelay)
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
