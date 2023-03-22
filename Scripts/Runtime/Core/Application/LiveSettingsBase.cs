using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Extenity.CryptoToolbox;
using Extenity.DataToolbox;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Extenity.ApplicationToolbox
{

	[JsonObject(MemberSerialization.OptIn)]
	public abstract class LiveSettingsBase<TDerived> where TDerived : LiveSettingsBase<TDerived>
	{
		#region Initialization

		protected LiveSettingsBase()
		{
#if UNITY_EDITOR
			CheckConsistency();
#endif
		}

		#endregion

		#region Data Structure

		private static List<(FieldInfo FieldInfo, string Key)> GetFields()
		{
			return typeof(TDerived)
				.GetFields(
					BindingFlags.Instance |
					BindingFlags.FlattenHierarchy |
					BindingFlags.Public |
					BindingFlags.NonPublic)
				.Where(field => field.GetCustomAttribute<JsonPropertyAttribute>() != null)
				.Select(field => (field, field.GetCustomAttribute<JsonPropertyAttribute>().PropertyName))
				.ToList();
		}

#if UNITY_EDITOR

		private void CheckDataStructureConsistency()
		{
			var fields = GetFields();

			// Check if there are duplicate keys.
			{
				var duplicates = fields.Select(field => field.Key).Duplicates(EqualityComparer<string>.Default);
				foreach (var duplicate in duplicates)
				{
					Log.Error($"There are duplicate keys for '{duplicate}' in {GetType().Name}.");
				}
			}
		}

#endif

		#endregion

		#region Serialize

		public string Serialize(string key)
		{
			var json = ToJson(false);
			return SimpleTwoWayEncryptorAES.EncryptBase64WithIV(json, key);
		}

		public string ToJson(bool pretty = false)
		{
			return JsonConvert.SerializeObject(this, pretty ? Formatting.Indented : Formatting.None);
		}

		#endregion

		#region Deserialize

		public static TDerived Deserialize(string serialized, out string json, string key)
		{
			if (string.IsNullOrEmpty(serialized))
				throw new ArgumentNullException();
			json = SimpleTwoWayEncryptorAES.DecryptBase64WithIV(serialized, key);
			return JsonConvert.DeserializeObject<TDerived>(json);
		}


		/* This was the old implementation. Now we use Json serialization.
		public void Parse(Dictionary<string, string> keyValueStore)
		{
			var fields = GetFields();
			if (fields.Count == 0)
				throw new InternalException(117457212);

			foreach (var field in fields)
			{
				var fieldType = field.FieldInfo.FieldType;
				if (fieldType == typeof(float))
				{
					ParseFloat(keyValueStore, field.Key, field.FieldInfo);
				}
				else if (fieldType == typeof(int))
				{
					ParseInt(keyValueStore, field.Key, field.FieldInfo);
				}
				else
				{
					// Field type is not implemented. Probably the type was not needed before. Just see the lines above and implement it.
					throw new InternalException(11845362, new Exception(fieldType.Name));
				}
			}
		}

		/// <summary>
		/// Tries to parse the value inside keyValueStore. Leaves the value intact when it fails to parse.
		/// </summary>
		private void ParseInt(Dictionary<string, string> keyValueStore, string key, FieldInfo fieldInfo)
		{
			if (keyValueStore.TryGetValue(key, out var text))
			{
				if (int.TryParse(text, out var valueInSettings))
				{
					fieldInfo.SetValue(this, valueInSettings);
				}
			}
		}

		/// <summary>
		/// Tries to parse the value inside keyValueStore. Leaves the value intact when it fails to parse.
		/// </summary>
		private void ParseFloat(Dictionary<string, string> keyValueStore, string key, FieldInfo fieldInfo)
		{
			if (keyValueStore.TryGetValue(key, out var text))
			{
				if (float.TryParse(text, out var valueInSettings))
				{
					fieldInfo.SetValue(this, valueInSettings);
				}
			}
		}
		*/

		#endregion

		#region Diff

#if UNITY_EDITOR

		private static void FindJsonDiff(JToken Current, JToken Model, StringBuilder result, string linePrefix)
		{
			if (JToken.DeepEquals(Current, Model))
				return;

			var fields = GetFields();

			switch (Current.Type)
			{
				case JTokenType.Object:
				{
					var current = Current as JObject;
					var model = Model as JObject;
					var addedKeys = current.Properties().Select(c => c.Name).Except(model.Properties().Select(c => c.Name));
					var removedKeys = model.Properties().Select(c => c.Name).Except(current.Properties().Select(c => c.Name));
					var unchangedKeys = current.Properties().Where(c => JToken.DeepEquals(c.Value, Model[c.Name])).Select(c => c.Name);
					foreach (var k in addedKeys)
					{
						var key = fields.FirstOrDefault(item => item.Key == k).FieldInfo?.Name ?? k;
						var originalValue = "NEW";
						var modifiedValue = Current[k];
						result.AppendLine($"{linePrefix}{key} \t: {originalValue}  =>  {modifiedValue}");
					}
					foreach (var k in removedKeys)
					{
						var key = fields.FirstOrDefault(item => item.Key == k).FieldInfo?.Name ?? k;
						var originalValue = Model[k];
						var modifiedValue = "DELETED";
						result.AppendLine($"{linePrefix}{key} \t: {originalValue}  =>  {modifiedValue}");
					}
					var potentiallyModifiedKeys = current.Properties().Select(c => c.Name).Except(addedKeys).Except(unchangedKeys);
					foreach (var k in potentiallyModifiedKeys)
					{
						var key = fields.FirstOrDefault(item => item.Key == k).FieldInfo?.Name ?? k;
						var newPrefix = string.IsNullOrWhiteSpace(linePrefix)
							? linePrefix + key
							: linePrefix + "/" + key;
						FindJsonDiff(current[k], model[k], result, newPrefix);
					}
				}
					break;
				case JTokenType.Array:
				{
					result.AppendLine("WARNING! Arrays are not implemented.");
					// var current = Current as JArray;
					// var model = Model as JArray;
					// diff["+"] = new JArray(current.Except(model));
					// diff["-"] = new JArray(model.Except(current));
				}
					break;
				default:
				{
					var originalValue = Model.ToString();
					var modifiedValue = Current.ToString();
					result.AppendLine($"{linePrefix} \t: {originalValue}  =>  {modifiedValue}");
				}
					break;
			}
		}

		public static string Diff(string originalAsJsonText, LiveSettingsBase<TDerived> modifiedAsLiveSettingsObject, string linePrefix = "\t")
		{
			if (string.IsNullOrWhiteSpace(originalAsJsonText))
				throw new ArgumentNullException(nameof(originalAsJsonText));
			if (modifiedAsLiveSettingsObject == null)
				throw new ArgumentNullException(nameof(modifiedAsLiveSettingsObject));

			var result = new StringBuilder();

			var originalJson = JToken.Parse(originalAsJsonText);
			var modifiedJson = JToken.Parse(modifiedAsLiveSettingsObject.ToJson());

			FindJsonDiff(modifiedJson, originalJson, result, linePrefix);

			var resultText = result.Length == 0
				? $"{linePrefix}No difference."
				: result.ToString();

			Log.Info("LiveSettings diff result:\n" + resultText);

			return resultText;
		}

#endif

		#endregion

		#region Consistency

#if UNITY_EDITOR

		private void CheckConsistency()
		{
			CheckDataStructureConsistency();
		}

#endif

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(LiveSettingsBase<TDerived>));

		#endregion
	}

}
