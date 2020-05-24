using System;
using Newtonsoft.Json;

namespace Extenity.JsonToolbox
{

	/// <summary>
	/// Formats the specified type of array to make it always serialize in a single line.
	/// </summary>
	public class OneLinerArrayConverter<T> : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			// Source: https://stackoverflow.com/a/37158962/2129382

			var previousFormatting = writer.Formatting;
			writer.WriteStartArray();
			try
			{
				writer.Formatting = Formatting.None;
				foreach (object childValue in ((System.Collections.IEnumerable)value))
				{
					serializer.Serialize(writer, childValue);
				}
			}
			finally
			{
				writer.WriteEndArray();
				writer.Formatting = previousFormatting;
			}
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			throw new NotSupportedException();
		}

		public override bool CanRead => false;
		public override bool CanWrite => true;

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(T[]);
		}
	}

}
