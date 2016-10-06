using System;
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

	public static void CreateFromFilePath(string filePath)
	{
		var directoryPath = Path.GetDirectoryName(filePath);
		if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
		{
			Directory.CreateDirectory(directoryPath);
		}
	}

	#region Copy Directory

	public class CopyResult
	{
		public struct FailReason
		{
			public string FilePath;
			public Exception Exception;
		}

		public List<FailReason> FailedFiles = null;
		public int FailedFileCount { get { return FailedFiles == null ? 0 : FailedFiles.Count; } }
		public List<string> CopiedFiles = null;
		public int CopiedFileCount = 0; // Does not directly return CopiedFiles.Count because CreateCopiedFileList can be false.
		public readonly bool CreateCopiedFileList = false;
		public bool IsOK { get { return FailedFiles == null || FailedFiles.Count == 0; } }

		public CopyResult()
		{
		}

		public CopyResult(bool createCopiedFileList)
		{
			CreateCopiedFileList = createCopiedFileList;
		}

		public void Reset()
		{
			CopiedFileCount = 0;
			if (CopiedFiles != null)
				CopiedFiles.Clear();
			if (FailedFiles != null)
				FailedFiles.Clear();
		}

		public void AddCopiedFile(string path)
		{
			CopiedFileCount++;
			if (CreateCopiedFileList)
			{
				if (CopiedFiles == null)
					CopiedFiles = new List<string>();
				CopiedFiles.Add(path);
			}
		}

		public void AddFailedFile(FailReason failReason)
		{
			if (FailedFiles == null)
				FailedFiles = new List<FailReason>();
			FailedFiles.Add(failReason);
		}
	}

	public static bool Copy(string sourceDirectory, string targetDirectory,
		string[] includeFilters = null, string[] excludeFilters = null,
		SearchOption searchOption = SearchOption.AllDirectories,
		bool overwrite = true, bool throwOnError = true, bool continueOnError = false,
		CopyResult result = null)
	{
		if (sourceDirectory == null)
			throw new ArgumentNullException("sourceDirectory");
		if (targetDirectory == null)
			throw new ArgumentNullException("targetDirectory");
		if (File.Exists(targetDirectory))
			throw new ArgumentException("Target directory points to a file.");

		HashSet<string> filesToCopy = new HashSet<string>();

		// Include files
		if (includeFilters.IsAllNullOrEmpty())
		{
			try
			{
				var list = Directory.GetFiles(sourceDirectory, "*", searchOption);
				for (int i = 0; i < list.Length; i++)
					filesToCopy.Add(list[i]);
			}
			catch (Exception)
			{
				if (throwOnError)
					throw;
				return false; // We could check continueOnError but it won't be useful at that point since we don't know what files to include or exclude.
			}
		}
		else
		{
			foreach (var includeFilter in includeFilters)
			{
				try
				{
					if (string.IsNullOrEmpty(includeFilter))
						continue;
					var list = Directory.GetFiles(sourceDirectory, includeFilter, searchOption);
					for (int i = 0; i < list.Length; i++)
						filesToCopy.Add(list[i]);
				}
				catch (Exception)
				{
					if (throwOnError)
						throw;
					return false; // We could check continueOnError but it won't be useful at that point since we don't know what files to include or exclude.
				}
			}
		}

		if (filesToCopy.Count == 0)
			return true; // No files to copy

		// Exclude files
		if (excludeFilters != null)
		{
			foreach (var excludeFilter in excludeFilters)
			{
				try
				{
					var list = Directory.GetFiles(sourceDirectory, excludeFilter, searchOption);
					for (int i = 0; i < list.Length; i++)
						filesToCopy.Remove(list[i]);
				}
				catch (Exception)
				{
					if (throwOnError)
						throw;
					return false; // We could check continueOnError but it won't be useful at that point since we don't know what files to include or exclude.
				}
			}
		}

		if (filesToCopy.Count == 0)
			return true; // No files to copy

		// Copy files
		var successful = true;
		foreach (var sourceFilePath in filesToCopy)
		{
			try
			{
				// Make relative path
				string relativeSourceFilePath;
				{
					//relativeFilePath = FileTools.MakeRelativePath(sourceDirectory, sourceFilePath); // This method only works between two absolute paths.

					if (!sourceFilePath.StartsWith(sourceDirectory))
					{
						throw new Exception("Relative file path could not be calculated.");
					}
					relativeSourceFilePath = sourceFilePath.Remove(0, sourceDirectory.Length + 1); // +1 is for directory separator at the end
				}

				var targetFilePath = Path.Combine(targetDirectory, relativeSourceFilePath);

				// Create directory if does not exist
				var directory = Path.GetDirectoryName(targetFilePath);
				Directory.CreateDirectory(directory);

				// Copy file
				File.Copy(sourceFilePath, targetFilePath, overwrite);

				if (result != null)
					result.AddCopiedFile(sourceFilePath);
			}
			catch (Exception e)
			{
				if (result != null)
				{
					result.AddFailedFile(new CopyResult.FailReason { FilePath = sourceFilePath, Exception = e });
				}
				if (throwOnError)
					throw;
				successful = false;
				if (!continueOnError)
					return false;
			}
		}
		return successful;
	}

	/*
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
		foreach (var fileInfo in source.GetFiles(fileSearchPattern, ))
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
						result.FailedFiles = new List<CopyResult.FailReason>();
					}
					result.FailedFiles.Add(new CopyResult.FailReason { FilePath = fileInfo.FullName, Exception = e });
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
	*/

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

	#region Get Root Directory

	public static string GetRootDirectory(this string path)
	{
		path = path.FixDirectorySeparatorChars();
		var root = Path.GetPathRoot(path);
		var pathWithoutRoot = path.Substring(root.Length);
		var split = pathWithoutRoot.Split(Path.DirectorySeparatorChar);
		if (split.Length == 0)
			return "";
		return split[0];
	}

	#endregion

	#region Temp Directory

	public static string CreateTemporaryDirectory()
	{
		var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		Directory.CreateDirectory(path);
		return path;
	}

	#endregion
}

#endif
