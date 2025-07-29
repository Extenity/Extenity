using System;

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
#if UNITY_5_3_OR_NEWER
			var defaultValueText = UnityEngine.JsonUtility.ToJson(_Value);
			var text = UnityEngine.PlayerPrefs.GetString(ProcessedPrefsKey, defaultValueText);
			var json = UnityEngine.JsonUtility.FromJson<TSerialized>(text);
			return json;
#else
			throw new System.NotImplementedException();
#endif
		}

		protected override void InternalSetValue(TSerialized value)
		{
#if UNITY_5_3_OR_NEWER
			var json = UnityEngine.JsonUtility.ToJson(value);
            UnityEngine.PlayerPrefs.SetString(ProcessedPrefsKey, json);
#else
			throw new System.NotImplementedException();
#endif
		}

		protected override bool IsSame(TSerialized oldValue, TSerialized newValue)
		{
			// Converting to json every time just to compare if the values are equal.
			// Not an efficient way of comparing stuff, but it's the best design.
			// Otherwise, the user would have to implement something like IEquatable
			// for every TSerialized class. IsSame is only used when setting the pref.
			// So the overhead is negligible.
#if UNITY_5_3_OR_NEWER
			var oldJson = UnityEngine.JsonUtility.ToJson(oldValue);
			var newJson = UnityEngine.JsonUtility.ToJson(newValue);
			return oldJson.EqualsOrBothEmpty(newJson, StringComparison.Ordinal);
#else
			throw new System.NotImplementedException();
#endif
		}
	}

}
