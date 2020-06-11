using System;
using Newtonsoft.Json;

namespace Extenity.KernelToolbox
{

	/// <summary>
	/// Converts an <see cref="ID"/> to and from a Hex string (e.g. <c>"3FA61"</c>).
	/// </summary>
	public class IDJsonConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value == null)
			{
				writer.WriteNull();
			}
			else if (value is ID castValue)
			{
				writer.WriteValue(castValue.ToHexString());
			}
			else
			{
				throw new JsonSerializationException($"Expected {nameof(ID)} object value");
			}
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			if (reader.TokenType != JsonToken.String)
			{
				throw new JsonSerializationException($"Unexpected token or value when parsing {nameof(ID)}. Token: {reader.TokenType}, Value: {reader.Value}");
			}
			try
			{
				return ID.FromHexString((string)reader.Value);
			}
			catch (Exception exception)
			{
				throw new JsonSerializationException($"Error parsing {nameof(ID)} string: {reader.Value}", exception);
			}
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(ID);
		}
	}

}
