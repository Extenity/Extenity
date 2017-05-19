using System.IO;
using System.Text;

namespace Extenity.DataToolbox
{

	public static class MemoryStreamExt
	{
		public static void Append(this MemoryStream stream, char value, Encoding encoding)
		{
			stream.Append(encoding.GetBytes(new[] { value }));
		}

		public static void Append(this MemoryStream stream, byte value)
		{
			stream.Append(new[] { value });
		}

		public static void Append(this MemoryStream stream, byte[] values)
		{
			stream.Write(values, 0, values.Length);
		}

		public static void Append(this MemoryStream stream, byte[] values, int offset, int count)
		{
			stream.Write(values, offset, count);
		}
	}

}
