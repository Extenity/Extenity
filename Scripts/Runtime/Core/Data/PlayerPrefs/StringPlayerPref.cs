using System;

namespace Extenity.DataToolbox
{

	public class StringPlayerPref : PlayerPref<string>
	{
		public StringPlayerPref(string prefsKey, PathHashPostfix appendPathHashToKey, string defaultValue, Func<string, string> valueTransform = null, float saveDelay = 0f)
			: base(prefsKey, appendPathHashToKey, defaultValue, null, valueTransform, saveDelay)
		{
		}

		public StringPlayerPref(string prefsKey, PathHashPostfix appendPathHashToKey, Func<PlayerPref<string>, string> defaultValueOverride, Func<string, string> valueTransform = null, float saveDelay = 0f)
			: base(prefsKey, appendPathHashToKey, default(string), defaultValueOverride, valueTransform, saveDelay)
		{
		}

		protected override string InternalGetValue()
		{
#if UNITY_5_3_OR_NEWER
			return UnityEngine.PlayerPrefs.GetString(ProcessedPrefsKey, _Value);
#else
			throw new System.NotImplementedException();
#endif
		}

		protected override void InternalSetValue(string value)
		{
#if UNITY_5_3_OR_NEWER
			UnityEngine.PlayerPrefs.SetString(ProcessedPrefsKey, value);
#else
			throw new System.NotImplementedException();
#endif
		}

		protected override bool IsSame(string oldValue, string newValue)
		{
			return oldValue.EqualsOrBothEmpty(newValue, StringComparison.Ordinal);
		}
	}

}
