using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Extenity.JsonToolbox.Converters
{

	/// <summary>
	/// Custom <c>Newtonsoft.Json.JsonConverter</c> for <c>System.Collections.Generic.Dictionary</c>.
	/// Source: wanzyeestudio.blogspot.com
	/// </summary>
	public class DictionaryConverter : JsonConverter
	{
		/// <summary>
		/// Determine if the type is <c>System.Collections.Generic.Dictionary</c>.
		/// </summary>
		/// <param name="objectType">Type of the object.</param>
		/// <returns><c>true</c> if this can convert the specified type; otherwise, <c>false</c>.</returns>
		public override bool CanConvert(Type objectType)
		{
			if (!objectType.IsGenericType) return false;

			var type = objectType.GetGenericTypeDefinition();

			return typeof(Dictionary<,>) == type || typeof(IDictionary<,>) == type;
		}

		/// <summary>
		/// Read as <c>System.Collections.Generic.KeyValuePair</c> array to rebuild a dictionary.
		/// </summary>
		/// <returns>The object value.</returns>
		/// <param name="reader">The <c>Newtonsoft.Json.JsonReader</c> to read from.</param>
		/// <param name="objectType">Type of the object.</param>
		/// <param name="existingValue">The existing value of object being read.</param>
		/// <param name="serializer">The calling serializer.</param>
		public override object ReadJson(
			JsonReader reader,
			Type objectType,
			object existingValue,
			JsonSerializer serializer
		)
		{
			if (JsonToken.Null == reader.TokenType) return null;

			var result = Activator.CreateInstance(objectType) as IDictionary;
			var args = objectType.GetGenericArguments();

			foreach (JObject pair in JArray.Load(reader))
			{
				var key = pair["Key"].ToObject(args[0], serializer);
				var value = pair["Value"].ToObject(args[1], serializer);

				if (!result.Contains(key)) result.Add(key, value);
				else throw new Exception($"Dictionary has a repeating key: {pair.ToString(Formatting.None)}");
			}

			return result;
		}

		/// <summary>
		/// Write as <c>System.Collections.Generic.KeyValuePair</c> array.
		/// </summary>
		/// <param name="writer">The <c>Newtonsoft.Json.JsonWriter</c> to write to.</param>
		/// <param name="value">The value.</param>
		/// <param name="serializer">The calling serializer.</param>
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, (value as IDictionary).Cast<object>());
		}
	}

}
