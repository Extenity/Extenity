using System.IO;
using System.Text;

namespace Extenity.DataToolbox
{

	public static class BinaryWriterTools
	{
		public static void WriteNullTerminatedSingleByte(this BinaryWriter writer, string value)
		{
			WriteNullTerminatedSingleByte(writer, value, Encoding.UTF8);
		}

		public static void WriteNullTerminatedSingleByte(this BinaryWriter writer, string value, Encoding encoding)
		{
			byte[] buffer = encoding.GetBytes(value);
			writer.Write(buffer);
			writer.Write(char.MinValue);
		}
	}

	public static class BinaryReaderTools
	{
		public static string ReadStringNullTerminatedSingleByte(this BinaryReader reader)
		{
			var stringBuilder = new StringBuilder();

			char read = reader.ReadChar();

			while (read != char.MinValue)
			{
				stringBuilder.Append(read);
				read = reader.ReadChar();
			}

			return stringBuilder.ToString();
		}
	}

}
