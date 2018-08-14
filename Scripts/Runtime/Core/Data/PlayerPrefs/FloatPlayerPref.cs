using System;
using Extenity.MathToolbox;
using UnityEngine;

namespace Extenity.DataToolbox
{

	public class FloatPlayerPref : PlayerPref<float>
	{
		public FloatPlayerPref(string prefsKey, PathHashPostfix appendPathHashToKey, float defaultValue, float saveDelay = 0f)
			: base(prefsKey, appendPathHashToKey, defaultValue, null, saveDelay)
		{
		}

		public FloatPlayerPref(string prefsKey, PathHashPostfix appendPathHashToKey, Action<PlayerPref<float>> defaultValueOverride, float saveDelay = 0f)
			: base(prefsKey, appendPathHashToKey, default(float), defaultValueOverride, saveDelay)
		{
		}

		protected override object InternalGetValue()
		{
			return PlayerPrefs.GetFloat(ProcessedPrefsKey, _Value);
		}

		protected override void InternalSetValue(object value)
		{
			PlayerPrefs.SetFloat(ProcessedPrefsKey, (float)value);
		}

		protected override bool IsSame(float oldValue, float newValue)
		{
			return oldValue.IsAlmostEqual(newValue, 1E-05f);
		}
	}

}
