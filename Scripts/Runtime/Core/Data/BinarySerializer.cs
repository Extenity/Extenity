using System;
using System.Runtime.InteropServices;

namespace Extenity.DataToolbox
{

	public static class BinarySerializer
	{
		#region Binary Serialization

		public static byte[] SerializeBinary(object obj)
		{
			if (obj == null)
				throw new ArgumentNullException();
			var rawSize = Marshal.SizeOf(obj);
			var buffer = Marshal.AllocHGlobal(rawSize);
			Marshal.StructureToPtr(obj, buffer, false);
			var rawData = new byte[rawSize];
			Marshal.Copy(buffer, rawData, 0, rawSize);
			Marshal.FreeHGlobal(buffer);
			return rawData;
		}

		public static T DeserializeBinary<T>(byte[] rawData, int position)
		{
			if (rawData == null)
				throw new ArgumentNullException();
			var rawSize = Marshal.SizeOf(typeof(T));
			if (rawSize > rawData.Length - position)
				throw new ArgumentException($"Not enough data to fill the struct. Trying to deserialize {rawData.Length - position} bytes, whereas {typeof(T).Name} struct size is {rawSize} bytes.");
			var buffer = Marshal.AllocHGlobal(rawSize);
			Marshal.Copy(rawData, position, buffer, rawSize);
			var result = (T)Marshal.PtrToStructure(buffer, typeof(T));
			Marshal.FreeHGlobal(buffer);
			return result;
		}

		#endregion
	}

}
