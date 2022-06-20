using System;
using System.IO;
using System.Security;
using System.Threading;
using Extenity.DataToolbox;
using Extenity.UnityEditorToolbox;

namespace Extenity.FileSystemToolbox
{

	public enum CopyHelper
	{
		Default,
		CreateDestinationDirectory,
	}

	public static class FileTools
	{
		#region String Operations - File Size

		/// <summary>
		/// Returns the human-readable file size for an arbitrary, 64-bit file size 
		/// The default format is "0.### XB", e.g. "4.2 KB" or "1.434 GB"
		/// Source: http://www.somacon.com/p576.php
		/// </summary>
		public static string ToFileSizeString(this long fileSize)
		{
			// Get absolute value
			long absolute_i = (fileSize < 0 ? -fileSize : fileSize);
			// Determine the suffix and readable value
			string suffix;
			double readable;
			if (absolute_i >= 0x1000000000000000) // Exabyte
			{
				suffix = "EB";
				readable = (fileSize >> 50);
			}
			else if (absolute_i >= 0x4000000000000) // Petabyte
			{
				suffix = "PB";
				readable = (fileSize >> 40);
			}
			else if (absolute_i >= 0x10000000000) // Terabyte
			{
				suffix = "TB";
				readable = (fileSize >> 30);
			}
			else if (absolute_i >= 0x40000000) // Gigabyte
			{
				suffix = "GB";
				readable = (fileSize >> 20);
			}
			else if (absolute_i >= 0x100000) // Megabyte
			{
				suffix = "MB";
				readable = (fileSize >> 10);
			}
			else if (absolute_i >= 0x400) // Kilobyte
			{
				suffix = "KB";
				readable = fileSize;
			}
			else
			{
				return fileSize.ToString("0 B"); // Byte
			}
			// Divide by 1024 to get fractional value
			readable = (readable / 1024);
			// Return formatted number with suffix
			return readable.ToString("0.# ") + suffix;
		}

		#endregion

		#region File Content Comparison

		/// <summary>
		/// Compares if two file contents are the same. First it checks for file sizes and immediately tells if file sizes are different. Then it checks byte by byte until a difference found.
		/// </summary>
		/// <returns>True if files are the same.</returns>
		public static bool CompareFileContents(this FileInfo fileInfo1, FileInfo fileInfo2)
		{
			AssetDatabaseRuntimeTools.ReleaseCachedFileHandles(); // Make Unity release the files to prevent any IO errors.

			if (fileInfo1.Length != fileInfo2.Length)
				return false;

			using (var fileStream1 = fileInfo1.OpenRead())
			{
				using (var fileStream2 = fileInfo2.OpenRead())
				{
					return fileStream1.CompareStreamContents(fileStream2);
				}
			}
		}

		#endregion

		#region File Size

		public static bool TryGetFileSize(string filePath, out long size)
		{
			try
			{
				size = new FileInfo(filePath).Length;
			}
			catch
			{
				size = -1;
				return false;
			}
			return true;
		}

		public static long GetFileSize(string filePath)
		{
			return new FileInfo(filePath).Length;
		}

		#endregion

		#region Temp File

		public static string CreateTemporaryFileInTemporaryDirectory()
		{
			var directory = DirectoryTools.CreateTemporaryDirectory();
			return Path.Combine(directory, Path.GetRandomFileName());
		}

		#endregion

		#region File Access

		/// <summary>
		/// Source: http://stackoverflow.com/questions/876473/is-there-a-way-to-check-if-a-file-is-in-use
		/// </summary>
		public static bool IsFileLocked(this FileInfo file)
		{
			FileStream stream = null;

			try
			{
				stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
			}
			catch (FileNotFoundException) // The file is not found. So it's not locked.
			{
				return false;
			}
			catch (DirectoryNotFoundException) // The specified path is invalid, such as being on an unmapped drive.
			{
				return true;
			}
			catch (IOException) // The file is already open.
			{
				return true;
			}
			catch (SecurityException) // The caller does not have the required permission.
			{
				return true;
			}
			catch (UnauthorizedAccessException) // path is read-only or is a directory.
			{
				return true;
			}
			catch (ArgumentNullException) // One or more arguments is null.
			{
				return true;
			}
			catch (ArgumentException) // path is empty or contains only white spaces.
			{
				return true;
			}
			catch // Unknown
			{
				return true;
			}
			finally
			{
				if (stream != null)
					stream.Close();
			}

			return false;
		}

		#endregion

		#region File Exists

		public static string AnyFileExistsWithExtensions(string pathWithoutExtension, string[] extensions)
		{
			for (int i = 0; i < extensions.Length; i++)
			{
				if (File.Exists(pathWithoutExtension + extensions[i]))
					return extensions[i];
			}
			return null;
		}

		#endregion

		#region File Copy

		public static void Copy(string sourceFileName, string destFileName, CopyHelper helper = CopyHelper.CreateDestinationDirectory)
		{
			AssetDatabaseRuntimeTools.ReleaseCachedFileHandles(); // Make Unity release the files to prevent any IO errors.

			if (helper == CopyHelper.CreateDestinationDirectory)
			{
				DirectoryTools.CreateFromFilePath(destFileName);
			}

			File.Copy(sourceFileName, destFileName);
		}

		public static void Copy(string sourceFileName, string destFileName, bool overwrite, CopyHelper helper = CopyHelper.CreateDestinationDirectory)
		{
			AssetDatabaseRuntimeTools.ReleaseCachedFileHandles(); // Make Unity release the files to prevent any IO errors.

			if (helper == CopyHelper.CreateDestinationDirectory)
			{
				DirectoryTools.CreateFromFilePath(destFileName);
			}

			File.Copy(sourceFileName, destFileName, overwrite);
		}

		#endregion

		#region File Delete

		private static void _Delete(string path)
		{
			try
			{
				File.Delete(path);
			}
			catch (IOException) 
			{
				Thread.Sleep(1); // Allow system to release file handles by waiting and then try once more
				File.Delete(path);
			}
			catch (UnauthorizedAccessException)
			{
				Thread.Sleep(1); // Allow system to release file handles by waiting and then try once more
				File.Delete(path);
			}
		}

		/// <returns>Returns true if File.Delete operation succeeds. If file is checked for existence and turns out file is not there, returns false. May throw exceptions for all other cases.</returns>
		public static bool Delete(string path, bool checkIfExists = false, bool autoRemoveReadOnlyAttribute = true)
		{
			return Delete(new FileInfo(path), checkIfExists, autoRemoveReadOnlyAttribute);
		}

		/// <returns>Returns true if File.Delete operation succeeds. If file is checked for existence and turns out file is not there, returns false. May throw exceptions for all other cases.</returns>
		public static bool Delete(this FileInfo fileInfo, bool checkIfExists = false, bool autoRemoveReadOnlyAttribute = true)
		{
			AssetDatabaseRuntimeTools.ReleaseCachedFileHandles(); // Make Unity release the files to prevent any IO errors.

			if (checkIfExists && !fileInfo.Exists)
			{
				return false;
			}

			// Try to remove readonly attribute because File.Delete() fails if file is readonly.
			if (fileInfo.IsReadOnly && autoRemoveReadOnlyAttribute)
			{
				fileInfo.Attributes = fileInfo.Attributes & ~FileAttributes.ReadOnly;

				// This was the old implementation, which was more intrusive.
				// fileInfo.Attributes = FileAttributes.Normal;
			}

			_Delete(fileInfo.FullName);
			return true;
		}

		#endregion

		#region Unique File

		public static string GenerateUniqueFilePath(this string path, string numberPrefix = " (", string numberPostfix = ")", int maxTries = 10000)
		{
			var fileName = Path.GetFileNameWithoutExtension(path);
			if (string.IsNullOrEmpty(fileName))
			{
				throw new Exception("Could not find the file name in path.");
			}
			var fileExtension = Path.GetExtension(path);
			var filePathWithoutExtension = string.IsNullOrEmpty(fileExtension)
				? path
				: path.Substring(0, path.Length - fileExtension.Length);

			//Log.Info("path : " + path);
			//Log.Info("fileName : " + fileName);
			//Log.Info("extension : " + fileExtension);
			//Log.Info("filePathWithoutExtension : " + filePathWithoutExtension);

			return GenerateUniqueNumberedName(
				filePathWithoutExtension,
				checkingFilePath => File.Exists(checkingFilePath + fileExtension),
				numberPrefix, numberPostfix, maxTries) + fileExtension;
		}

		public static string GenerateUniqueNumberedName(this string name, Predicate<string> doesExistCheck, string numberPrefix = " (", string numberPostfix = ")", int maxTries = 10000)
		{
			if (string.IsNullOrEmpty(name))
				throw new NullReferenceException("name");

			if (!doesExistCheck(name))
				return name;

			//Log.Info("---- Name already exists: " + name);

			int number = 1;
			string nameWithPrefix = null;

			// Detect current number of name
			if (name.EndsWith(numberPostfix))
			{
				var startIndex = name.LastIndexOf(numberPrefix, StringComparison.InvariantCulture);
				if (startIndex > 0)
				{
					startIndex += numberPrefix.Length;
					var numberString = name.Substring(startIndex, name.Length - startIndex - numberPostfix.Length);
					if (!string.IsNullOrEmpty(numberString) && numberString.IsNumeric(false))
					{
						// There is already a number at the end of the name. So use it instead of generating a new postfix to the name.
						number = int.Parse(numberString);
						number++;
						nameWithPrefix = name.Substring(0, startIndex);
					}

				}
			}

			if (nameWithPrefix == null)
			{
				nameWithPrefix = name + numberPrefix;
			}

			var tryCount = 0;
			var newName = nameWithPrefix + number + numberPostfix;
			while (doesExistCheck(newName))
			{
				//Log.Info("---- Tried name already exists: " + newName);
				number++;
				newName = nameWithPrefix + number + numberPostfix;
				if (++tryCount > maxTries)
					throw new Exception("Failed to find a unique name in maximum allowed tries.");
			}

			return newName;
		}

		#endregion

		#region Temp File

		public static string WriteAllTextToTempDirectory(string relativeFilePath, string content)
		{
			var tempDirectory = DirectoryTools.CreateTemporaryDirectory();
			var path = Path.Combine(tempDirectory, relativeFilePath);
			DirectoryTools.CreateFromFilePath(path);
			File.WriteAllText(path, content);
			return path;
		}

		#endregion
	}

}
