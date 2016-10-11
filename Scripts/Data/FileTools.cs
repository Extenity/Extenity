using System;
using System.ComponentModel;
using System.IO;
using System.Security;

public static class FileTools
{
	#region String Operations - Remove Last Directory

	public static string RemoveLastDirectoryFromPath(this string path)
	{
		if (string.IsNullOrEmpty(path))
			return "";

		path = path.RemoveEndingDirectorySeparatorChar();

		var index = IndexOfEndingDirectorySeparatorChar(path);
		return index < 0 ? "" : path.Substring(0, index);
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
		if (string.IsNullOrEmpty(basePath)) throw new ArgumentNullException("basePath");
		if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException("filePath");

		var fromUri = new Uri(basePath);
		var toUri = new Uri(filePath);

		var relativeUri = fromUri.MakeRelativeUri(toUri);
		var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

		return FixDirectorySeparatorChars(relativePath);
	}

	#endregion

	#region String Operations - Directory Separator

	public static char DirectorySeparatorChar
	{
		get { return Path.DirectorySeparatorChar; }
	}

	public static char OtherDirectorySeparatorChar
	{
		get
		{
			switch (DirectorySeparatorChar)
			{
				case '\\': return '/';
				case '/': return '\\';
				default: throw new InvalidEnumArgumentException("System directory separator char is not known.");
			}
		}
	}

	public static bool IsDirectorySeparatorChar(this char c)
	{
		return c == '/' || c == '\\';
	}

	public static bool IsEndingWithDirectorySeparatorChar(this string path)
	{
		return path[path.Length - 1].IsDirectorySeparatorChar();
	}

	public static string RemoveEndingDirectorySeparatorChar(this string path)
	{
		if (path.IsEndingWithDirectorySeparatorChar())
		{
			return path.Substring(0, path.Length - 1);
		}
		return path;
	}

	public static int IndexOfStartingDirectorySeparatorChar(this string path)
	{
		int index = path.IndexOf('/');
		if (index < 0)
			index = path.IndexOf('\\');
		return index;
	}

	public static int IndexOfEndingDirectorySeparatorChar(this string path)
	{
		int index = path.LastIndexOf('/');
		if (index < 0)
			index = path.LastIndexOf('\\');
		return index;
	}

	public static string FixDirectorySeparatorChars(this string path)
	{
		return path.Replace(OtherDirectorySeparatorChar, DirectorySeparatorChar);
	}

	public static string FixDirectorySeparatorChars(this string path, char separator)
	{
		if (separator == '\\')
		{
			return path.Replace('/', separator);
		}
		else if (separator == '/')
		{
			return path.Replace('\\', separator);
		}
		else
		{
			return path.Replace('/', separator).Replace('\\', separator);
		}
	}

	public static string AddDirectorySeparatorToEnd(this string path)
	{
		if (path.IsEndingWithDirectorySeparatorChar())
			return path;
		return path + DirectorySeparatorChar;
	}

	public static string AddDirectorySeparatorToEnd(this string path, char separator)
	{
		if (path.IsEndingWithDirectorySeparatorChar())
			return path;
		return path + separator;
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
	/// Compares if two file contents are the same. First it checks for file sizes and immediatelly tells if file sizes are different. Then it checks byte by byte until a difference found.
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
}
