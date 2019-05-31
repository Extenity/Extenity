using System;
using Extenity.MathToolbox;
using UnityEngine;

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
			return PlayerPrefs.GetFloat(ProcessedPrefsKey, _Value);
		}

		protected override void InternalSetValue(float value)
		{
			PlayerPrefs.SetFloat(ProcessedPrefsKey, value);
		}

		protected override bool IsSame(float oldValue, float newValue)
		{
			return oldValue.IsAlmostEqual(newValue, 1E-05f);
		}
	}

}
