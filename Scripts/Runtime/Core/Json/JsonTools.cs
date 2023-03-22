using System;
using Extenity.FileSystemToolbox;
using Extenity.JsonToolbox.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Extenity.JsonToolbox
{

	public static class JsonTools
	{
		#region Serialization Configuration

		/// <summary>
		/// A template JsonSerializerSettings to be used in applications. Feel free to modify it as required.
		/// </summary>
		public static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
		{
			ContractResolver = new ContractResolver(),
			Converters = new JsonConverter[]
			{
				// Unity related converters
#if UNITY
				new BoundsConverter(),
				new ColorConverter(),
				new Matrix4x4Converter(),
				new QuaternionConverter(),
				new RectConverter(),
				new RectOffsetConverter(),
				new Vector2Converter(),
				new Vector3Converter(),
				new Vector4Converter(),
#endif

				// .NET related converters
				new DictionaryConverter(),
				new VersionConverter(),
				// new StringEnumConverter(), Explicitly decided to not use this. At first, serializing an enum as String rather than an Integer looks like a good idea. But it breaks things when a field of that enum is renamed.

				// Quality of Life improvement converters
				new OneLinerArrayConverter<bool>(),
				new OneLinerArrayConverter<char>(),
				new OneLinerArrayConverter<byte>(),
				new OneLinerArrayConverter<sbyte>(),
				new OneLinerArrayConverter<Int16>(),
				new OneLinerArrayConverter<Int32>(),
				new OneLinerArrayConverter<Int64>(),
				new OneLinerArrayConverter<UInt16>(),
				new OneLinerArrayConverter<UInt32>(),
				new OneLinerArrayConverter<UInt64>(),
				new OneLinerArrayConverter<float>(),
				new OneLinerArrayConverter<double>(),
				// new OneLinerArrayConverter<string>(), Explicitly decided to not use this. They might be long. Better have strings line by line.
			},
		};

		#endregion

		#region Serialize/Deserialize Crosschecked

		/// <summary>
		/// Serializes the object to json string. Then deserializes and serializes again to make sure the serialization
		/// and deserialization code works the same. If there are differences in serialized and reserialized json
		/// outputs, you may want to inspect the output to see what part of serialization logic needs attention.
		/// </summary>
		public static string SerializeCrosschecked<T>(T obj, JsonSerializerSettings settings)
		{
			if (obj == null)
				return null;
			string serializedJson = SerializeObject(obj, Formatting.Indented, settings);
			string reserializedJson = null;
			try
			{
				T copy = DeserializeObject<T>(serializedJson, settings);
				reserializedJson = SerializeObject(copy, Formatting.Indented, settings);

				if (!serializedJson.Equals(reserializedJson, StringComparison.Ordinal))
				{
					throw new Exception("The reserialized json does not match the serialized json.");
				}

				return serializedJson;
			}
			catch (Exception exception)
			{
				var serializedJsonPath = FileTools.WriteAllTextToTempDirectory("SerializationFailure/Serialized.json", serializedJson);
				var reserializedJsonPath = FileTools.WriteAllTextToTempDirectory("SerializationFailure/Reserialized.json", reserializedJson);
				Log.Error($"Serialization failure (Error part 1/3). See '{serializedJsonPath}' The serialized json is:\n" + serializedJson);
				Log.Error($"Serialization failure (Error part 2/3). See '{reserializedJsonPath}' The reserialized json is:\n" + reserializedJson);
#if UNITY_EDITOR
				// TODO:
				// BuildTools.LaunchBeyondCompareFileComparison(serializedJsonPath, reserializedJsonPath);
#endif
				throw new Exception("Serialization crosscheck failed (Error part 3/3). See previous errors for details.", exception);
			}
		}

		/// <summary>
		/// It's generally a good idea to upgrade the data after deserializing it. This method combines upgrading with
		/// deserialization. Then it crosschecks by reserializing the upgraded data to see that everything goes smooth
		/// in serialization and deserialization logic.
		///
		/// Note that crosschecking depends on the upgraded data, rather than the original json data that is still valid
		/// but may possibly be in an outdated schema.
		///
		/// It's crucial to write upgrade logic properly. Having an upgrade logic ensures the data that is saved in a
		/// previous version of the application will be handled properly in later versions. Always doing a crosscheck
		/// when anytime a serialization is required ensures the application will always receive the data in good health.
		/// </summary>
		public static T DeserializeAndUpgradeCrosschecked<T>(string json, Action<JObject> upgradeMethod, JsonSerializerSettings settings)
		{
			var upgradedJson = JObject.Parse(json, new JsonLoadSettings()
			{
				CommentHandling = CommentHandling.Ignore,
				DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error,
				LineInfoHandling = LineInfoHandling.Load,
			});
			upgradeMethod(upgradedJson);

			var dataContainer = DeserializeJObject<T>(upgradedJson, settings);
			JObject reserializedJson = null;
			try
			{
				reserializedJson = SerializeJObject(dataContainer, Formatting.Indented, settings);

				// if (!JToken.DeepEquals(upgradedJson, reserializedJson)) // This was the ideal solution but failed due to the bad influence of field orders.
				var upgradedJsonText = upgradedJson.ToString();
				var reserializedJsonText = reserializedJson.ToString();
				if (!upgradedJsonText.Equals(reserializedJsonText, StringComparison.Ordinal))
				{
					throw new Exception("The reserialized json does not match the upgraded json.");
				}

				return dataContainer;
			}
			catch (Exception exception)
			{
				var originalJsonPath = FileTools.WriteAllTextToTempDirectory("DeserializationFailure/Original.json", json);
				var upgradedJsonPath = FileTools.WriteAllTextToTempDirectory("DeserializationFailure/Upgraded.json", upgradedJson?.ToString());
				var reserializedJsonPath = FileTools.WriteAllTextToTempDirectory("DeserializationFailure/Reserialized.json", reserializedJson?.ToString());
				Log.Error($"Deserialization failure (Error part 1/4). See '{originalJsonPath}' The original json is:\n" + json);
				Log.Error($"Deserialization failure (Error part 2/4). See '{upgradedJsonPath}' The upgraded json is:\n" + upgradedJson?.ToString());
				Log.Error($"Deserialization failure (Error part 3/4). See '{reserializedJsonPath}' The reserialized json is:\n" + reserializedJson?.ToString());
#if UNITY_EDITOR
				// TODO:
				// BuildTools.LaunchBeyondCompareFileComparison(upgradedJsonPath, reserializedJsonPath);
#endif
				throw new Exception("Deserialization crosscheck failed (Error part 4/4). See previous errors for details.", exception);
			}
		}

		#endregion

		#region Serialize/Deserialize

		public static string SerializeObject<T>(in T obj, Formatting formatting, JsonSerializerSettings settings)
		{
			return JsonConvert.SerializeObject(obj, formatting, settings);
		}

		public static T DeserializeObject<T>(string json, JsonSerializerSettings settings)
		{
			return JsonConvert.DeserializeObject<T>(json, settings);
		}

		public static T DeserializeAndUpgradeObject<T>(string json, Action<JObject> upgradeMethod, JsonSerializerSettings settings)
		{
			var upgradedJson = JObject.Parse(json, new JsonLoadSettings()
			{
				CommentHandling = CommentHandling.Ignore,
				DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error,
				LineInfoHandling = LineInfoHandling.Load,
			});
			upgradeMethod(upgradedJson);

			return DeserializeJObject<T>(upgradedJson, settings);
		}

		public static JObject SerializeJObject<T>(in T obj, Formatting formatting, JsonSerializerSettings settings)
		{
			JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(settings);
			jsonSerializer.Formatting = formatting;
			return JObject.FromObject(obj, jsonSerializer);
		}

		public static T DeserializeJObject<T>(JObject json, JsonSerializerSettings settings)
		{
			JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(settings);
			return json.ToObject<T>(jsonSerializer);
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(JsonTools));

		#endregion
	}

}
