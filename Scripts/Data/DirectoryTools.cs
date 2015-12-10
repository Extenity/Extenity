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
}

#endif
