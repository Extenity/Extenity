using System;

namespace Extenity.DataToolbox
{

	public class IntPlayerPref : PlayerPref<int>
	{
		public IntPlayerPref(string prefsKey, PathHashPostfix appendPathHashToKey, int defaultValue, Func<int, int> valueTransform = null, float saveDelay = 0f)
			: base(prefsKey, appendPathHashToKey, defaultValue, null, valueTransform, saveDelay)
		{
		}

		public IntPlayerPref(string prefsKey, PathHashPostfix appendPathHashToKey, Func<PlayerPref<int>, int> defaultValueOverride, Func<int, int> valueTransform = null, float saveDelay = 0f)
			: base(prefsKey, appendPathHashToKey, default(int), defaultValueOverride, valueTransform, saveDelay)
		{
		}

		protected override int InternalGetValue()
		{
#if UNITY_5_3_OR_NEWER
			return UnityEngine.PlayerPrefs.GetInt(ProcessedPrefsKey, _Value);
#else
			throw new System.NotImplementedException();
#endif
		}

		protected override void InternalSetValue(int value)
		{
#if UNITY_5_3_OR_NEWER
            UnityEngine.PlayerPrefs.SetInt(ProcessedPrefsKey, value);
#else
			throw new System.NotImplementedException();
#endif
		}

		protected override bool IsSame(int oldValue, int newValue)
		{
			return oldValue == newValue;
		}
	}

}
