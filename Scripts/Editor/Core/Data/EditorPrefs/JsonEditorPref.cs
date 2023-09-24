using System;
using UnityEditor;

namespace Extenity.DataToolbox.Editor
{

	public class JsonEditorPref<TSerialized> : EditorPref<TSerialized>
	{
		public Func<TSerialized, string> SerializationFunction = value => UnityEngine.JsonUtility.ToJson(value, false);
		public Func<string, TSerialized> DeserializationFunction = value => (TSerialized)UnityEngine.JsonUtility.FromJson(value, typeof(TSerialized));

		public JsonEditorPref(string                          prefsKey,
		                      PathHashPostfix                 appendPathHashToKey,
		                      DefaultValueMethod<TSerialized> defaultValueMethod,
		                      EditorPrefLogOptions            logOptions,
		                      Func<TSerialized, string>       serializationFunction,
		                      Func<string, TSerialized>       deserializationFunction)
			: base(prefsKey,
			       appendPathHashToKey,
			       defaultValueMethod,
			       logOptions)
		{
			if (serializationFunction != null)
				SerializationFunction = serializationFunction;
			if (deserializationFunction != null)
				DeserializationFunction = deserializationFunction;
		}

		protected override TSerialized InternalGetValue()
		{
			// Default value has no effect here, because it was already handled before calling this function.
			var text = EditorPrefs.GetString(ProcessedPrefsKey, default);
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