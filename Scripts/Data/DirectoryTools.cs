using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR || !UNITY_WEBPLAYER

public static class DirectoryTools
{
	public static bool IsDirectoryEmpty(string path)
	{
#if UNITY_WEBPLAYER
		var directories = Directory.GetDirectories(path);
		var files = Directory.GetFiles(path);
		return directories.Length == 0 && files.Length == 0;
#else
		return Directory.GetFileSystemEntries(path).Length == 0;
#endif
	}

	public static void Create(string filePath)
	{
		string directoryPath = Path.GetDirectoryName(filePath);
		if (directoryPath.Length > 0 && !Directory.Exists(directoryPath))
		{
			Directory.CreateDirectory(directoryPath);
		}
	}

	#region Copy Directory

	public class CopyFailReason
	{
		public string FilePath;
		public Exception Exception;
	}

	public class CopyResult
	{
		public int CopiedFileCount;
		public List<CopyFailReason> FailedFiles;
	}

	/// <returns>Total number of copied files.</returns>
	public static void Copy(string sourceDirectory, string targetDirectory, string fileSearchPattern = "*", bool overwrite = true, CopyResult result = null)
	{
		var sourceDirectoryInfo = new DirectoryInfo(sourceDirectory);
		var targetDirectoryInfo = new DirectoryInfo(targetDirectory);
		Copy(sourceDirectoryInfo, targetDirectoryInfo, fileSearchPattern, overwrite, result);
	}

	/// <returns>Total number of copied files.</returns>
	public static void Copy(DirectoryInfo source, DirectoryInfo target, string fileSearchPattern = "*", bool overwrite = true, CopyResult result = null)
	{
		if (source == null)
			throw new ArgumentNullException("source");
		if (target == null)
			throw new ArgumentNullException("target");

		// Do nothing if source directory does not exist
		if (!source.Exists)
			return;

		bool directoryCreated = false;

		// Copy each file into the new directory.
		foreach (var fileInfo in source.GetFiles(fileSearchPattern))
		{
			try
			{
				// Create target directory if required
				if (!directoryCreated)
				{
					Directory.CreateDirectory(target.FullName);
					directoryCreated = true;
				}

				// Copy file
				fileInfo.CopyTo(Path.Combine(target.FullName, fileInfo.Name), overwrite);
				if (result != null)
					result.CopiedFileCount++;
			}
			catch (Exception e)
			{
				if (result != null)
				{
					if (result.FailedFiles == null)
					{
						result.FailedFiles = new List<CopyFailReason>();
					}
					result.FailedFiles.Add(new CopyFailReason { FilePath = fileInfo.FullName, Exception = e });
				}
				else
				{
					// Ignore
				}
			}
		}

		// Copy each subdirectory using recursion.
		foreach (var sourceSubDirectory in source.GetDirectories())
		{
			var targetSubDirectory = new DirectoryInfo(Path.Combine(target.FullName, sourceSubDirectory.Name));
			Copy(sourceSubDirectory, targetSubDirectory, fileSearchPattern, overwrite, result);
		}
	}

	#endregion

	#region Delete Directory

	public static void Delete(string path)
	{
		if (Directory.Exists(path))
		{
			Directory.Delete(path, true);
		}
	}

	#endregion
}

#endif
