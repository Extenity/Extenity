using System;
using UnityEngine;

namespace Extenity.DataToolbox
{

	public class JsonPlayerPref<TSerialized> : PlayerPref<TSerialized>
	{
		public JsonPlayerPref(string prefsKey, PathHashPostfix appendPathHashToKey, TSerialized defaultValue, Func<TSerialized, TSerialized> valueTransform = null, float saveDelay = 0f)
			: base(prefsKey, appendPathHashToKey, defaultValue, null, valueTransform, saveDelay)
		{
		}

		public JsonPlayerPref(string prefsKey, PathHashPostfix appendPathHashToKey, Func<PlayerPref<TSerialized>, TSerialized> defaultValueOverride, Func<TSerialized, TSerialized> valueTransform = null, float saveDelay = 0f)
			: base(prefsKey, appendPathHashToKey, default, defaultValueOverride, valueTransform, saveDelay)
		{
		}

		protected override TSerialized InternalGetValue()
		{
			var defaultValueText = JsonUtility.ToJson(_Value);
			var text = PlayerPrefs.GetString(ProcessedPrefsKey, defaultValueText);
			var json = JsonUtility.FromJson<TSerialized>(text);
			return json;
		}

		protected override void InternalSetValue(TSerialized value)
		{
			var json = JsonUtility.ToJson(value);
			PlayerPrefs.SetString(ProcessedPrefsKey, json);
		}

		protected override bool IsSame(TSerialized oldValue, TSerialized newValue)
		{
			// Converting to json every time just to compare if the values are equal.
			// Not an efficient way of comparing stuff, but it's the best design.
			// Otherwise, the user would have to implement something like IEquatable
			// for every TSerialized class. IsSame is only used when setting the pref.
			// So the overhead is negligible.
			var oldJson = JsonUtility.ToJson(oldValue);
			var newJson = JsonUtility.ToJson(newValue);
			return oldJson.EqualsOrBothEmpty(newJson);
		}
	}

}
