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

	public static void CreateDirectory(string filePath)
	{
		string directoryPath = Path.GetDirectoryName(filePath);
		if (directoryPath.Length > 0 && !Directory.Exists(directoryPath))
		{
			Directory.CreateDirectory(directoryPath);
		}
	}

	#region Copy Directory

	public static void Copy(string sourceDirectory, string targetDirectory, string fileSearchPattern = "*", bool overwrite = true)
	{
		var sourceDirectoryInfo = new DirectoryInfo(sourceDirectory);
		var targetDirectoryInfo = new DirectoryInfo(targetDirectory);
		Copy(sourceDirectoryInfo, targetDirectoryInfo, fileSearchPattern, overwrite);
	}

	public static void Copy(DirectoryInfo source, DirectoryInfo target, string fileSearchPattern = "*", bool overwrite = true)
	{
		bool directoryCreated = false;

		// Copy each file into the new directory.
		foreach (var fileInfo in source.GetFiles(fileSearchPattern))
		{
			//Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);

			if (!directoryCreated)
			{
				Directory.CreateDirectory(target.FullName);
			}

			fileInfo.CopyTo(Path.Combine(target.FullName, fileInfo.Name), overwrite);
		}

		// Copy each subdirectory using recursion.
		foreach (var sourceSubDirectory in source.GetDirectories())
		{
			var targetSubDirectory = target.CreateSubdirectory(sourceSubDirectory.Name);
			Copy(sourceSubDirectory, targetSubDirectory);
		}
	}

	#endregion
}

#endif
