using System;
using Newtonsoft.Json;

namespace Extenity.Kernel
{

	/// <summary>
	/// Converts a <see cref="Ref"/> to and from a Hex string (e.g. <c>"3FA61"</c>).
	/// </summary>
	public class RefJsonConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value == null)
			{
				writer.WriteNull();
			}
			else if (value is Ref castValue)
			{
				writer.WriteValue(castValue.ToHexString());
			}
			else
			{
				throw new JsonSerializationException($"Expected {nameof(Ref)} object value");
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
				throw new JsonSerializationException($"Unexpected token or value when parsing {nameof(Ref)}. Token: {reader.TokenType}, Value: {reader.Value}");
			}
			try
			{
				return Ref.FromHexString((string)reader.Value);
			}
			catch (Exception exception)
			{
				throw new JsonSerializationException($"Error parsing {nameof(Ref)} string: {reader.Value}", exception);
			}
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(Ref);
		}
	}

}
