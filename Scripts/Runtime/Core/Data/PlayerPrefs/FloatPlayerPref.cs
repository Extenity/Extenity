using System;
using Extenity.MathToolbox;

namespace Extenity.DataToolbox
{

	public class FloatPlayerPref : PlayerPref<float>
	{
		public FloatPlayerPref(string prefsKey, PathHashPostfix appendPathHashToKey, float defaultValue, Func<float, float> valueTransform = null, float saveDelay = 0f)
			: base(prefsKey, appendPathHashToKey, defaultValue, null, valueTransform, saveDelay)
		{
		}

		public FloatPlayerPref(string prefsKey, PathHashPostfix appendPathHashToKey, Func<PlayerPref<float>, float> defaultValueOverride, Func<float, float> valueTransform = null, float saveDelay = 0f)
			: base(prefsKey, appendPathHashToKey, default(float), defaultValueOverride, valueTransform, saveDelay)
		{
		}

		protected override float InternalGetValue()
		{
#if UNITY
			return UnityEngine.PlayerPrefs.GetFloat(ProcessedPrefsKey, _Value);
#else
			throw new System.NotImplementedException();
#endif
        }

		protected override void InternalSetValue(float value)
		{
#if UNITY
			UnityEngine.PlayerPrefs.SetFloat(ProcessedPrefsKey, value);
#else
			throw new System.NotImplementedException();
#endif
		}

		protected override bool IsSame(float oldValue, float newValue)
		{
			return oldValue.IsAlmostEqual(newValue, 1E-05f);
		}
	}

}
