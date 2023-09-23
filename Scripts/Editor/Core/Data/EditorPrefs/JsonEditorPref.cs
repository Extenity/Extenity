using System;
using UnityEditor;

namespace Extenity.DataToolbox.Editor
{

	public class JsonEditorPref<TSerialized> : EditorPref<TSerialized>
	{
		public Func<TSerialized, string> SerializationFunction = value => UnityEngine.JsonUtility.ToJson(value, false);
		public Func<string, TSerialized> DeserializationFunction = value => (TSerialized)UnityEngine.JsonUtility.FromJson(value, typeof(TSerialized));

		public JsonEditorPref(string               prefsKey,
		                      PathHashPostfix      appendPathHashToKey,
		                      TSerialized          defaultValue,
		                      EditorPrefLogOptions logOptions)
			: base(prefsKey,
			       appendPathHashToKey,
			       defaultValue,
			       null,
			       logOptions)
		{
		}

		public JsonEditorPref(string                                     prefsKey,
		                      PathHashPostfix                            appendPathHashToKey,
		                      Func<EditorPref<TSerialized>, TSerialized> defaultValueOverride,
		                      EditorPrefLogOptions                       logOptions)
			: base(prefsKey,
			       appendPathHashToKey,
			       default,
			       defaultValueOverride,
			       logOptions)
		{
		}

		protected override TSerialized InternalGetValue()
		{
			var defaultValueText = SerializationFunction(_Value);

			var text = EditorPrefs.GetString(ProcessedPrefsKey, defaultValueText);
			var json = DeserializationFunction(text);
			return json;
		}

		protected override void InternalSetValue(TSerialized value)
		{
			var json = SerializationFunction(value);
			EditorPrefs.SetString(ProcessedPrefsKey, json);
		}

		protected override bool IsSame(TSerialized oldValue, TSerialized newValue)
		{
			// Converting to json every time just to compare if the values are equal.
			// Not an efficient way of comparing stuff, but it's the best design.
			// Otherwise, the user would have to implement something like IEquatable
			// for every TSerialized class. IsSame is only used when setting the pref.
			// So the overhead is negligible.
			var oldJson = SerializationFunction(oldValue);
			var newJson = SerializationFunction(newValue);
			return oldJson.EqualsOrBothEmpty(newJson, StringComparison.Ordinal);
		}
	}

}