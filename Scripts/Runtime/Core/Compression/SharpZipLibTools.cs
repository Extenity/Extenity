using System.Collections.Generic;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace ICSharpCode.SharpZipLib.Extensions
{

	public static class SharpZipLibTools
	{
		#region Initialization

		// Keep this as a reference. This might be needed in future.
		// static SharpZipLibTools()
		// {
		// 	// To solve the problem: https://web.archive.org/web/20190922190319/http://community.sharpdevelop.net/forums/t/21795.aspx
		// 	ZipStrings.CodePage = 0;
		// }

		#endregion

		#region Compress

		public static void CompressFiles(string outputZipFilePath, int compressionRatio, string filesBasePath, IEnumerable<string> filesRelativePaths, string packedFilesBasePath = "")
		{
			CreateDirectoryFromFilePath(outputZipFilePath);
			using (var zipFileStream = File.Create(outputZipFilePath))
			{
				using (var outputZipStream = new ZipOutputStream(zipFileStream))
				{
					outputZipStream.SetLevel(compressionRatio);
					foreach (var relativePath in filesRelativePaths)
					{
						var fullPath = Path.Combine(filesBasePath, relativePath);
						var packedFullPath = Path.Combine(packedFilesBasePath, relativePath);
						CompressFile(outputZipStream, fullPath, packedFullPath);
					}
					outputZipStream.IsStreamOwner = true; // Makes the Close also close the underlying stream since this stream is a file.
					outputZipStream.Close();
				}
			}
		}

		public static void CompressFile(string outputZipFilePath, int compressionRatio, string fileFullPath, string packedFilePath = null)
		{
			if (string.IsNullOrEmpty(packedFilePath))
			{
				packedFilePath = Path.GetFileName(fileFullPath);
			}

			CreateDirectoryFromFilePath(outputZipFilePath);
			using (var zipFileStream = File.Create(outputZipFilePath))
			{
				using (var outputZipStream = new ZipOutputStream(zipFileStream))
				{
					outputZipStream.SetLevel(compressionRatio);
					CompressFile(outputZipStream, fileFullPath, packedFilePath);
					outputZipStream.IsStreamOwner = true; // Makes the Close also close the underlying stream since this stream is a file.
					outputZipStream.Close();
				}
			}
		}

		public static void CompressFile(ZipOutputStream outputZipStream, string filePath, string filePathInPack)
		{
			ZipEntry newZipEntry = new ZipEntry(filePathInPack);
			newZipEntry.Size = new FileInfo(filePath).Length;
			outputZipStream.PutNextEntry(newZipEntry);
			var buffer = new byte[4096];
			using (FileStream streamReader = File.OpenRead(filePath))
			{
				StreamUtils.Copy(streamReader, outputZipStream, buffer);
			}
			outputZipStream.CloseEntry();
		}

		public static void CompressFileFromData(ZipOutputStream outputZipStream, string data, string filePathInPack)
		{
			CompressFileFromData(outputZipStream, Encoding.UTF8.GetBytes(data), filePathInPack);
		}

		public static void CompressFileFromData(ZipOutputStream outputZipStream, byte[] data, string filePathInPack)
		{
			ZipEntry newZipEntry = new ZipEntry(filePathInPack);
			newZipEntry.Size = data.Length;
			outputZipStream.PutNextEntry(newZipEntry);
			outputZipStream.Write(data, 0, data.Length);
			outputZipStream.CloseEntry();
		}

		#endregion

		#region Extract

		public static void ExtractSingleFileEnsured(Stream stream, MemoryStream memoryStream, string password = null)
		{
			using (var zipFile = new ZipFile(stream))
			{
				ExtractSingleFileEnsured(zipFile, memoryStream, password);
			}
		}

		public static void ExtractSingleFileEnsured(Stream stream, string outputFilePath, string password = null)
		{
			using (var zipFile = new ZipFile(stream))
			{
				ExtractSingleFileEnsured(zipFile, outputFilePath, password);
			}
		}

		public static void ExtractFiles(Stream stream, string outputDirectoryPath, string password = null)
		{
			using (var zipFile = new ZipFile(stream))
			{
				ExtractFiles(zipFile, outputDirectoryPath, password);
			}
		}

		public static void ExtractSingleFileEnsured(string filePath, MemoryStream memoryStream, string password = null)
		{
			using (var zipFileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				using (var zipFile = new ZipFile(zipFileStream))
				{
					ExtractSingleFileEnsured(zipFile, memoryStream, password);
				}
			}
		}

		public static void ExtractSingleFileEnsured(string filePath, string outputFilePath, string password = null)
		{
			using (var zipFileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				using (var zipFile = new ZipFile(zipFileStream))
				{
					ExtractSingleFileEnsured(zipFile, outputFilePath, password);
				}
			}
		}

		public static void ExtractFiles(string filePath, string outputDirectoryPath, string password = null)
		{
			using (var zipFileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				using (var zipFile = new ZipFile(zipFileStream))
				{
					ExtractFiles(zipFile, outputDirectoryPath, password);
				}
			}
		}

		public static void ExtractFiles(ZipFile zipFile, string outputDirectoryPath, string password = null)
		{
			if (!string.IsNullOrEmpty(password))
			{
				zipFile.Password = password; // AES encrypted entries are handled automatically
			}

			foreach (ZipEntry zipEntry in zipFile)
			{
				if (!zipEntry.IsFile)
				{
					continue; // Ignore directories
				}
				var entryFileName = zipEntry.Name;
				// to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
				// Optionally match entrynames against a selection list here to skip as desired.
				// The unpacked length is available in the zipEntry.Size property.

				var buffer = new byte[4096]; // 4K is optimum
				var zipStream = zipFile.GetInputStream(zipEntry);

				var fullZipToPath = Path.Combine(outputDirectoryPath, entryFileName);
				var directoryName = Path.GetDirectoryName(fullZipToPath);
				if (directoryName.Length > 0)
					Directory.CreateDirectory(directoryName);

				// Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
				// of the file, but does not waste memory.
				// The "using" will close the stream even if an exception occurs.
				using (var streamWriter = File.Create(fullZipToPath))
				{
					StreamUtils.Copy(zipStream, streamWriter, buffer);
				}
			}
		}

		public static void ExtractSingleFileEnsured(ZipFile zipFile, MemoryStream memoryStream, string password = null)
		{
			if (!string.IsNullOrEmpty(password))
			{
				zipFile.Password = password; // AES encrypted entries are handled automatically
			}

			if (zipFile.Count == 1)
			{
				var zipEntry = zipFile[0];
				if (zipEntry.IsFile)
				{
					var buffer = new byte[4096]; // 4K is optimum
					var zipStream = zipFile.GetInputStream(zipEntry);

					// Expand memory stream as required
					if (memoryStream.Length - memoryStream.Position < zipEntry.Size)
					{
						memoryStream.SetLength(zipEntry.Size + memoryStream.Position);
					}

					// Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
					// of the file, but does not waste memory.
					StreamUtils.Copy(zipStream, memoryStream, buffer);
				}
			}
		}

		public static void ExtractSingleFileEnsured(ZipFile zipFile, string outputFilePath, string password = null)
		{
			if (!string.IsNullOrEmpty(password))
			{
				zipFile.Password = password; // AES encrypted entries are handled automatically
			}

			if (zipFile.Count == 1)
			{
				var zipEntry = zipFile[0];
				if (zipEntry.IsFile)
				{
					// The unpacked length is available in the zipEntry.Size property.

					var buffer = new byte[4096]; // 4K is optimum
					var zipStream = zipFile.GetInputStream(zipEntry);

					var directoryName = Path.GetDirectoryName(outputFilePath);
					if (directoryName.Length > 0)
						Directory.CreateDirectory(directoryName);

					// Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
					// of the file, but does not waste memory.
					// The "using" will close the stream even if an exception occurs.
					using (var streamWriter = File.Create(outputFilePath))
					{
						StreamUtils.Copy(zipStream, streamWriter, buffer);
					}
				}
			}
		}

		#endregion

		#region Tools

		private static void CreateDirectoryFromFilePath(string filePath)
		{
			var directoryPath = Path.GetDirectoryName(filePath);
			if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
			}
		}

		#endregion
	}

}
