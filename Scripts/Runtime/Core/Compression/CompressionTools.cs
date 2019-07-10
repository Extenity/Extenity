using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Extenity.DataToolbox;

namespace Extenity.CompressionToolbox
{

	public enum CompressionMethod
	{
		NoCompression,

		/// <summary>
		/// Compression levels:
		///    0 : Same as NoCompression
		///    1 : CompressionLevel.Fastest
		///    2 : CompressionLevel.Optimal
		/// </summary>
		GZip,

		/// <summary>
		/// Compression levels:
		///    0 : Same as NoCompression
		///    1 : CompressionLevel.Fastest
		///    2 : CompressionLevel.Optimal
		/// </summary>
		DeltaBytesGZip,
	}

	public static class CompressionTools
	{
		public static string CompressString(this string text, CompressionMethod compressionMethod, int compressionLevel)
		{
			var bytes = Encoding.UTF8.GetBytes(text);
			return Convert.ToBase64String(CompressBytes(bytes, compressionMethod, compressionLevel));
		}

		public static string DecompressString(this string text, CompressionMethod compressionMethod)
		{
			var bytes = Convert.FromBase64String(text);
			return Encoding.UTF8.GetString(DecompressBytes(bytes, compressionMethod));
		}

		public static byte[] CompressBytes(this byte[] bytes, CompressionMethod compressionMethod, int compressionLevel)
		{
			if (bytes == null || bytes.Length == 0 || compressionLevel <= 0)
				return bytes;

			switch (compressionMethod)
			{
				case CompressionMethod.NoCompression:
					return bytes;

				case CompressionMethod.GZip:
					return InternalCompressGZip(bytes, compressionLevel);

				case CompressionMethod.DeltaBytesGZip:
					var deltaBytes = CollectionTools.ConvertToDeltaBytes(bytes);
					return InternalCompressGZip(deltaBytes, compressionLevel);

				default:
					throw new ArgumentOutOfRangeException(nameof(compressionMethod), compressionMethod, null);
			}
		}

		public static byte[] DecompressBytes(this byte[] bytes, CompressionMethod compressionMethod)
		{
			if (bytes == null || bytes.Length == 0)
				return bytes;

			switch (compressionMethod)
			{
				case CompressionMethod.NoCompression:
					return bytes;

				case CompressionMethod.GZip:
					return InternalDecompressGZip(bytes);

				case CompressionMethod.DeltaBytesGZip:
					bytes = InternalDecompressGZip(bytes);
					return CollectionTools.ConvertFromDeltaBytes(bytes);

				default:
					throw new ArgumentOutOfRangeException(nameof(compressionMethod), compressionMethod, null);
			}
		}

		#region GZip Compression

		private static byte[] InternalCompressGZip(byte[] bytes, int compressionLevel)
		{
			using (var msi = new MemoryStream(bytes))
			using (var mso = new MemoryStream())
			{
				var compression = compressionLevel == 1
					? CompressionLevel.Fastest
					: CompressionLevel.Optimal;
				using (var zip = new GZipStream(mso, compression))
				{
					StreamTools.CopyTo(msi, zip);
				}
				return mso.ToArray();
			}
		}

		private static byte[] InternalDecompressGZip(byte[] bytes)
		{
			using (var msi = new MemoryStream(bytes))
			using (var mso = new MemoryStream())
			{
				using (var zip = new GZipStream(msi, CompressionMode.Decompress))
				{
					StreamTools.CopyTo(zip, mso);
				}
				return mso.ToArray();
			}
		}

		#endregion
	}

}
