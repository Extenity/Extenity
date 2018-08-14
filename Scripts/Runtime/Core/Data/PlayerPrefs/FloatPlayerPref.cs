using System;
using Extenity.MathToolbox;
using UnityEngine;

namespace Extenity.DataToolbox
{

	public class FloatPlayerPref : PlayerPref<float>
	{
		public FloatPlayerPref(string prefsKey, PathHashPostfix appendPathHashToKey, float defaultValue)
			: base(prefsKey, appendPathHashToKey, defaultValue, null)
		{
		}

		public FloatPlayerPref(string prefsKey, PathHashPostfix appendPathHashToKey, Action<PlayerPref<float>> defaultValueOverride)
			: base(prefsKey, appendPathHashToKey, default(float), defaultValueOverride)
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
