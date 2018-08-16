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

		public IntPlayerPref(string prefsKey, PathHashPostfix appendPathHashToKey, Func<PlayerPref<int>, int> defaultValueOverride, float saveDelay = 0f)
			: base(prefsKey, appendPathHashToKey, default(int), defaultValueOverride, saveDelay)
		{
		}

		protected override int InternalGetValue()
		{
			return PlayerPrefs.GetInt(ProcessedPrefsKey, _Value);
		}

		protected override void InternalSetValue(int value)
		{
			PlayerPrefs.SetInt(ProcessedPrefsKey, value);
		}

		protected override bool IsSame(int oldValue, int newValue)
		{
			return oldValue == newValue;
		}
	}

}
