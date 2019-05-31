using System;

namespace Extenity.DataToolbox
{

	public class BoolPlayerPref : PlayerPref<bool>
	{
		public BoolPlayerPref(string prefsKey, PathHashPostfix appendPathHashToKey, bool defaultValue, Func<bool, bool> valueTransform = null, float saveDelay = 0f)
			: base(prefsKey, appendPathHashToKey, defaultValue, null, valueTransform, saveDelay)
		{
		}

		public BoolPlayerPref(string prefsKey, PathHashPostfix appendPathHashToKey, Func<PlayerPref<bool>, bool> defaultValueOverride, Func<bool, bool> valueTransform = null, float saveDelay = 0f)
			: base(prefsKey, appendPathHashToKey, default(bool), defaultValueOverride, valueTransform, saveDelay)
		{
		}

		protected override bool InternalGetValue()
		{
			return PlayerPrefsTools.GetBool(ProcessedPrefsKey, _Value);
		}

		protected override void InternalSetValue(bool value)
		{
			PlayerPrefsTools.SetBool(ProcessedPrefsKey, value);
		}

		protected override bool IsSame(bool oldValue, bool newValue)
		{
			return oldValue == newValue;
		}
	}

}
