using System;
using System.ComponentModel;
using System.IO;
using UnityEngine;
using System.Collections;

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
	/// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
	/// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
	/// <returns>The relative path from the start directory to the end path.</returns>
	/// <exception cref="ArgumentNullException"></exception>
	public static string MakeRelativePath(this string fromPath, string toPath)
	{
		if (string.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
		if (string.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

		var fromUri = new Uri(fromPath);
		var toUri = new Uri(toPath);

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

	#endregion

	#region Comparison

	public static bool PathCompare(this string path1, string path2)
	{
		string fullPath1 = Path.GetFullPath(path1);
		string fullPath2 = Path.GetFullPath(path2);
		return 0 == String.Compare(
			fullPath1,
			fullPath2,
			StringComparison.InvariantCultureIgnoreCase);
	}

	public static bool FileCompare(string filePath1, string filePath2)
	{
		string fullPath1 = Path.GetFullPath(filePath1);
		string fullPath2 = Path.GetFullPath(filePath2);

		// Determine if the same file was referenced two times.
		if (fullPath1 == fullPath2)
		{
			// Return true to indicate that the files are the same.
			return true;
		}

		// Open the two files.
		FileStream fs1 = File.OpenRead(fullPath1);
		FileStream fs2 = File.OpenRead(fullPath2);

		// Check the file sizes. If they are not the same, the files 
		// are not the same.
		if (fs1.Length != fs2.Length)
		{
			// Close the file
			fs1.Close();
			fs2.Close();

			// Return false to indicate files are different
			return false;
		}

		// Read and compare a byte from each file until either a
		// non-matching set of bytes is found or until the end of
		// file1 is reached.
		int file1byte;
		int file2byte;
		do
		{
			// Read one byte from each file.
			file1byte = fs1.ReadByte();
			file2byte = fs2.ReadByte();
		}
		while ((file1byte == file2byte) && (file1byte != -1));

		// Close the files.
		fs1.Close();
		fs2.Close();

		// Return the success of the comparison. "file1byte" is 
		// equal to "file2byte" at this point only if the files are 
		// the same.
		return ((file1byte - file2byte) == 0);
	}

	#endregion
}
