using System;
using System.IO;
using System.Security;

namespace Extenity.DataToolbox
{

	public static class FileTools
	{
		#region String Operations - Alter File Name

		public static string AddPrefixToFileName(this string path, string prefix)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException(nameof(path));
			if (string.IsNullOrEmpty(prefix))
				throw new ArgumentNullException(nameof(prefix));
			var fileNameWithExtension = Path.GetFileName(path);
			return path.Substring(0, path.Length - fileNameWithExtension.Length) + prefix + fileNameWithExtension;
		}

		public static string AddSuffixToFileName(this string path, string suffix)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException(nameof(path));
			if (string.IsNullOrEmpty(suffix))
				throw new ArgumentNullException(nameof(suffix));
			var fileNameWithExtension = Path.GetFileName(path);
			var fileName = Path.GetFileNameWithoutExtension(path);
			if (fileName.Length == fileNameWithExtension.Length)
				return path + suffix;
			var extension = fileNameWithExtension.Substring(fileName.Length);
			return path.Substring(0, path.Length - extension.Length) + suffix + extension;
		}

		#endregion

		#region String Operations - Remove First/Last Directory

		/// <summary>
		/// Removes the first directory in path while keeping root intact.
		/// </summary>
		public static string RemoveFirstDirectoryFromPath(this string path)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException(path);

			string root;
			string directory;
			string fileName;
			path.SplitPath(out root, out directory, out fileName);

			if (string.IsNullOrEmpty(directory))
			{
				throw new InvalidOperationException($"There was no directory to remove from path '{path}'.");
			}
			var index = directory.IndexOfStartingDirectorySeparatorChar();
			if (index < 0 || index == directory.Length - 1)
			{
				// No more directories left after removing this one. Just use root and filename.
				return root + fileName;
			}

			var result = directory.Substring(index + 1, directory.Length - index - 1);
			return root + result + fileName;
		}

		public static string RemoveLastDirectoryFromPath(this string path)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException(path);

			string root;
			string directory;
			string fileName;
			path.SplitPath(out root, out directory, out fileName);

			if (string.IsNullOrEmpty(directory))
			{
				throw new InvalidOperationException($"There was no directory to remove from path '{path}'.");
			}
			var startIndex = directory.IsEndingWithDirectorySeparatorChar()
				? directory.Length - 2
				: directory.Length - 1;
			var index = directory.IndexOfEndingDirectorySeparatorChar(startIndex);
			if (index < 0)
			{
				// No more directories left after removing this one. Just use root and filename.
				return root + fileName;
			}

			var result = directory.Substring(0, index + 1);
			return root + result + fileName;
		}

		#endregion

		#region String Operations - Relative Path

		/// <summary>
		/// Creates a relative path from one file or directory to another.
		/// </summary>
		/// <param name="basePath">Contains the directory that defines the start of the relative path.</param>
		/// <param name="filePath">Contains the path that defines the endpoint of the relative path.</param>
		/// <returns>The relative path from the start directory to the end path.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static string MakeRelativePath(this string basePath, string filePath)
		{
			if (string.IsNullOrEmpty(basePath)) throw new ArgumentNullException(nameof(basePath));
			if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));

			var fromUri = new Uri(basePath);
			var toUri = new Uri(filePath);

			var relativeUri = fromUri.MakeRelativeUri(toUri);
			var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

			return FixDirectorySeparatorChars(relativePath);
		}

		public static bool IsRelativePath(this string path)
		{
			if (string.IsNullOrEmpty(path))
				return false;
			return !Path.IsPathRooted(path);
		}

		public static bool IsFullPath(this string path)
		{
			if (string.IsNullOrEmpty(path))
				return false;
			return Path.IsPathRooted(path);
		}

		#endregion

		#region String Operations - Directory Separator

		public static char DirectorySeparatorChar
		{
			get { return Path.DirectorySeparatorChar; }
		}

		public static char OtherDirectorySeparatorChar
		{
			get { return Path.AltDirectorySeparatorChar; }
		}

		public static bool IsDirectorySeparatorChar(this char c)
		{
			return c == '/' || c == '\\';
		}

		public static bool IsEndingWithDirectorySeparatorChar(this string path)
		{
			if (string.IsNullOrEmpty(path))
				return false;
			return path[path.Length - 1].IsDirectorySeparatorChar();
		}

		public static string RemoveEndingDirectorySeparatorChar(this string path)
		{
			if (path == null)
				return null;
			if (path.IsEndingWithDirectorySeparatorChar())
			{
				return path.Substring(0, path.Length - 1);
			}
			return path;
		}

		public static int IndexOfStartingDirectorySeparatorChar(this string path)
		{
			if (path == null)
				return -1;
			return IndexOfStartingDirectorySeparatorChar(path, 0);
		}

		public static int IndexOfStartingDirectorySeparatorChar(this string path, int startIndex)
		{
			if (path == null)
				return -1;
			int index = path.IndexOf('/', startIndex);
			if (index < 0)
				index = path.IndexOf('\\', startIndex);
			return index;
		}

		public static int IndexOfEndingDirectorySeparatorChar(this string path)
		{
			if (path == null)
				return -1;
			return IndexOfEndingDirectorySeparatorChar(path, path.Length - 1);
		}

		public static int IndexOfEndingDirectorySeparatorChar(this string path, int startIndex)
		{
			if (path == null)
				return -1;
			int index = path.LastIndexOf('/', startIndex);
			if (index < 0)
				index = path.LastIndexOf('\\', startIndex);
			return index;
		}

		public static string FixDirectorySeparatorChars(this string path)
		{
			if (path == null)
				return null;
			return path.Replace(OtherDirectorySeparatorChar, DirectorySeparatorChar);
		}

		public static string FixDirectorySeparatorChars(this string path, char separator)
		{
			if (path == null)
				return null;
			switch (separator)
			{
				case '\\':
					return path.Replace('/', separator);
				case '/':
					return path.Replace('\\', separator);
				default:
					return path.Replace('/', separator).Replace('\\', separator);
			}
		}

		public static string AddDirectorySeparatorToEnd(this string path)
		{
			if (path == null)
				return new string(DirectorySeparatorChar, 1);
			if (path.IsEndingWithDirectorySeparatorChar())
				return path;
			return path + DirectorySeparatorChar;
		}

		public static string AddDirectorySeparatorToEnd(this string path, char separator)
		{
			if (path == null)
				return new string(separator, 1);
			if (path.IsEndingWithDirectorySeparatorChar())
				return path;
			return path + separator;
		}

		#endregion

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
			return readable.ToString("0.### ") + suffix;
		}

		#endregion

		#region Path Info

		public static void SplitPath(this string path, out string root, out string directoryWithoutRoot, out string fileName)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException(nameof(path));

			var rootReported = Path.GetPathRoot(path);
			var fileNameReported = Path.GetFileName(path);

			root = path.Substring(0, rootReported.Length);
			fileName = path.Substring(path.Length - fileNameReported.Length);
			directoryWithoutRoot = path.Substring(rootReported.Length, path.Length - rootReported.Length - fileNameReported.Length);
		}

		#endregion

		#region Extension

		public static string AddFileExtension(this string path, string extension, bool ignoreIfAlreadyThere = true)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException(nameof(path));
			if (string.IsNullOrEmpty(extension))
				throw new ArgumentNullException(nameof(extension));

			var extensionWithDot = extension[0] == '.'
				? extension
				: '.' + extension;

			if (ignoreIfAlreadyThere)
			{
				return path.EndsWith(extensionWithDot, StringComparison.OrdinalIgnoreCase)
					? path
					: path + extensionWithDot;
			}
			else
			{
				return path + extensionWithDot;
			}
		}

		#endregion

		#region Comparison

		public static bool PathCompare(this string path1, string path2)
		{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
			var fullPath1 = Path.GetFullPath(path1);
			var fullPath2 = Path.GetFullPath(path2);
			return 0 == String.Compare(
				fullPath1,
				fullPath2,
				StringComparison.InvariantCultureIgnoreCase);
#else
		// Make sure casing won't be a problem for other platforms
		throw new NotImplementedException();
#endif
		}

		/// <summary>
		/// Compares if two file contents are the same. First it checks for file sizes and immediately tells if file sizes are different. Then it checks byte by byte until a difference found.
		/// </summary>
		/// <returns>True if files are the same.</returns>
		public static bool CompareFileContents(this FileInfo fileInfo1, FileInfo fileInfo2)
		{
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

		#region Combine

		/// <summary>
		/// Works just like Path.Combine, with an additional 'separator' parameter that is used in-between merged paths.
		/// </summary>
		public static string Combine(string path1, string path2, char separator)
		{
			if (path1 == null || path2 == null)
				throw new ArgumentNullException((path1 == null) ? "path1" : "path2");
			//CheckInvalidPathChars(path1);
			//CheckInvalidPathChars(path2);

			if (path2.Length == 0)
				return path1;
			if (path1.Length == 0)
				return path2;
			if (Path.IsPathRooted(path2))
				return path2;

			var ch = path1[path1.Length - 1];
			if (ch != Path.DirectorySeparatorChar && ch != Path.AltDirectorySeparatorChar && ch != Path.VolumeSeparatorChar)
				return path1 + separator + path2;
			return path1 + path2;
		}

		#endregion

		#region File Size

		public static bool TryGetFileSize(this string filePath, out long size)
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

		public static long GetFileSize(this string filePath)
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

			//Debug.Log("path : " + path);
			//Debug.Log("fileName : " + fileName);
			//Debug.Log("extension : " + fileExtension);
			//Debug.Log("filePathWithoutExtension : " + filePathWithoutExtension);

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

			//Debug.Log("---- Name already exists: " + name);

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
				//Debug.Log("---- Tried name already exists: " + newName);
				number++;
				newName = nameWithPrefix + number + numberPostfix;
				if (++tryCount > maxTries)
					throw new Exception("Failed to find a unique name in maximum allowed tries.");
			}

			return newName;
		}

		#endregion
	}

}
