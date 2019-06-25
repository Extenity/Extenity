using System;
using System.IO;

namespace Extenity.FileSystemToolbox
{

	public static class PathTools
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

			path.SplitPath(out string root, out string directory, out string fileName);

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

			path.SplitPath(out string root, out string directory, out string fileName);

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

		#region String Operations - Get Parent Directory

		public static string GetParentDirectoryName(this string path)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException(path);

			path.SplitPath(out _, out string directory, out _);

			var lastSeparator = directory.IndexOfEndingDirectorySeparatorChar();
			if (lastSeparator > 0)
			{
				var previousSeparator = directory.IndexOfEndingDirectorySeparatorChar(lastSeparator - 1);
				if (previousSeparator < 0)
				{
					return directory.Substring(0, lastSeparator);
				}
				else
				{
					return directory.Substring(previousSeparator + 1, lastSeparator - previousSeparator - 1);
				}
			}
			throw new InvalidOperationException($"Failed to find parent directory of path '{path}'.");
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
			int index1 = path.IndexOf('/', startIndex);
			int index2 = path.IndexOf('\\', startIndex);
			if (index1 < 0) return index2;
			if (index2 < 0) return index1;
			return index1 < index2 ? index1 : index2;
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
			int index1 = path.LastIndexOf('/', startIndex);
			int index2 = path.LastIndexOf('\\', startIndex);
			if (index1 < 0) return index2;
			if (index2 < 0) return index1;
			return index1 > index2 ? index1 : index2;
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

		#region Split Path

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

		public static string ChangeFileExtension(this string path, string extension)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException(nameof(path));
			if (string.IsNullOrEmpty(extension))
				throw new ArgumentNullException(nameof(extension));

			var extensionWithDot = extension[0] == '.'
				? extension
				: '.' + extension;

			if (path.EndsWith(extensionWithDot, StringComparison.OrdinalIgnoreCase))
				return path;

			var directory = Path.GetDirectoryName(path);
			var fileName = Path.GetFileNameWithoutExtension(path) + extensionWithDot;
			return Path.Combine(directory, fileName);
		}

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

		#region Path Comparison

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

		#region Check For Case-Sensitivity Of Platform

		// Source: https://stackoverflow.com/questions/430256/how-do-i-determine-whether-the-filesystem-is-case-sensitive-in-net
		private static bool _IsFileSystemCaseSensitiveChecked = false;
		private static bool _IsFileSystemCaseSensitive;
		public static bool IsFileSystemCaseSensitive
		{
			get
			{
				if (!_IsFileSystemCaseSensitiveChecked)
				{
					var tmp = Path.GetTempPath();
					_IsFileSystemCaseSensitive =
						!Directory.Exists(tmp.ToUpper()) ||
						!Directory.Exists(tmp.ToLower());
					_IsFileSystemCaseSensitiveChecked = true;
				}
				return _IsFileSystemCaseSensitive;
			}
		}

		#endregion
	}

}
