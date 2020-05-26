using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Extenity.DataToolbox;
using Extenity.UnityEditorToolbox;

namespace Extenity.FileSystemToolbox
{

	public static class DirectoryTools
	{
		public static bool IsDirectoryEmpty(string path)
		{
			return Directory.GetFileSystemEntries(path).Length == 0;
		}

		public static void Create(string directoryPath)
		{
			AssetDatabaseRuntimeTools.ReleaseCachedFileHandles(); // Make Unity release the files to prevent any IO errors.

			if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
			}
		}

		public static void CreateFromFilePath(string filePath)
		{
			AssetDatabaseRuntimeTools.ReleaseCachedFileHandles(); // Make Unity release the files to prevent any IO errors.

			var directoryPath = Path.GetDirectoryName(filePath);
			if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
			}
		}

		public static string CreateSubDirectory(string parentDirectory, string subDirectory)
		{
			// AssetDatabaseRuntimeTools.ReleaseCachedFileHandles(); Already done in Create() below

			var path = Path.Combine(parentDirectory, subDirectory);
			Create(path);
			return path;
		}

		#region Folder Name List To Be Created

		public static List<string> GenerateFolderNameListToBeCreatedFromFilePaths(IEnumerable<string> filePaths)
		{
			return GenerateFolderNameListToBeCreatedFromFilePaths(filePaths, PathTools.DirectorySeparatorChar);
		}

		public static List<string> GenerateFolderNameListToBeCreatedFromFilePaths(IEnumerable<string> filePaths, char directorySeparator)
		{
			var folderOfFilePaths = new HashSet<string>();
			foreach (var filePath in filePaths)
			{
				var directoryName = Path.GetDirectoryName(filePath).RemoveEndingDirectorySeparatorChar().FixDirectorySeparatorChars(directorySeparator);
				if (!string.IsNullOrEmpty(directoryName))
				{
					folderOfFilePaths.Add(directoryName);
				}
			}
			var slicedFolders = new HashSet<string>();
			foreach (var folder in folderOfFilePaths)
			{
				var indexOfSeparator = -1;
				do
				{
					indexOfSeparator = folder.IndexOf(directorySeparator, indexOfSeparator + 1);
					if (indexOfSeparator > 0)
					{
						var slicedFolder = folder.Substring(0, indexOfSeparator);
						slicedFolders.Add(slicedFolder);
					}
				} while (indexOfSeparator >= 0);

				slicedFolders.Add(folder);
			}
			var list = new List<string>(slicedFolders.Count);
			foreach (var folder in slicedFolders)
			{
				list.Add(folder);
			}
			list.Sort(Comparer<string>.Default);
			return list;
		}

		#endregion

		#region Get File List

		public static HashSet<string> ListFilesInDirectory(string sourceDirectory, SearchOption searchOption,
			string[] includeFilters = null, string[] excludeFilters = null,
			bool throwOnError = true)
		{
			if (sourceDirectory == null)
				throw new ArgumentNullException(nameof(sourceDirectory));
			if (File.Exists(sourceDirectory))
				throw new ArgumentException("Source directory points to a file.");
			if (!Directory.Exists(sourceDirectory))
				return new HashSet<string>();

			AssetDatabaseRuntimeTools.ReleaseCachedFileHandles(); // Make Unity release the files to prevent any IO errors.

			sourceDirectory = sourceDirectory.AddDirectorySeparatorToEnd().FixDirectorySeparatorChars();

			var files = new HashSet<string>();

			// Include files
			if (includeFilters.IsAllNullOrEmpty())
			{
				try
				{
					var list = Directory.GetFiles(sourceDirectory, "*", searchOption);
					for (int i = 0; i < list.Length; i++)
						files.Add(list[i]);
				}
				catch (Exception)
				{
					if (throwOnError)
						throw;
					return new HashSet<string>(); // We could check continueOnError but it won't be useful at that point since we don't know what files to include or exclude.
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
							files.Add(list[i]);
					}
					catch (Exception)
					{
						if (throwOnError)
							throw;
						return new HashSet<string>(); // We could check continueOnError but it won't be useful at that point since we don't know what files to include or exclude.
					}
				}
			}

			if (files.Count == 0)
				return files; // No files to include

			// Exclude files
			if (excludeFilters != null)
			{
				foreach (var excludeFilter in excludeFilters)
				{
					try
					{
						var list = Directory.GetFiles(sourceDirectory, excludeFilter, searchOption);
						for (int i = 0; i < list.Length; i++)
							files.Remove(list[i]);
					}
					catch (Exception)
					{
						if (throwOnError)
							throw;
						return new HashSet<string>(); // We could check continueOnError but it won't be useful at that point since we don't know what files to include or exclude.
					}
				}
			}

			return files;
		}

		#endregion

		#region Copy Directory

		public class CopyResult
		{
			public struct FailReason
			{
				public string FilePath;
				public Exception Exception;
			}
			public struct CopiedFile
			{
				public string SourcePath;
				public string TargetPath;

				public CopiedFile(string sourcePath, string targetPath)
				{
					SourcePath = sourcePath;
					TargetPath = targetPath;
				}

				public override string ToString()
				{
					return "From \"" + SourcePath + "\" to \"" + TargetPath + "\"";
				}
			}

			private List<FailReason> _FailedFiles = null;
			public List<FailReason> FailedFiles { get { return _FailedFiles; } }
			public int FailedFileCount { get { return _FailedFiles == null ? 0 : _FailedFiles.Count; } }
			private List<CopiedFile> _CopiedFiles = null;
			/// <summary>
			/// Make sure CreateCopiedFileList set to true for accessing this list.
			/// </summary>
			public List<CopiedFile> CopiedFiles { get { return _CopiedFiles; } }
			// Does not directly return CopiedFiles.Count because CreateCopiedFileList can be false.
			public int CopiedFileCount = 0;
			/// <summary>
			/// Allows you to choose not to cause overhead for creating the copied files list if you are not going to use it.
			/// </summary>
			public readonly bool CreateCopiedFileList = false;
			public bool IsOK { get { return _FailedFiles == null || _FailedFiles.Count == 0; } }

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
				if (_CopiedFiles != null)
					_CopiedFiles.Clear();
				if (_FailedFiles != null)
					_FailedFiles.Clear();
			}

			public void AddCopiedFile(string sourceFilePath, string targetFilePath)
			{
				CopiedFileCount++;
				if (CreateCopiedFileList)
				{
					if (_CopiedFiles == null)
						_CopiedFiles = new List<CopiedFile>();
					_CopiedFiles.Add(new CopiedFile(sourceFilePath, targetFilePath));
				}
			}

			public void AddFailedFile(FailReason failReason)
			{
				if (_FailedFiles == null)
					_FailedFiles = new List<FailReason>();
				_FailedFiles.Add(failReason);
			}
		}

		public static bool Copy(string sourceDirectory, SearchOption searchOption, string targetDirectory,
			string[] includeFilters = null, string[] excludeFilters = null,
			bool overwrite = true, bool throwOnError = true, bool continueOnError = false,
			CopyResult result = null)
		{
			if (sourceDirectory == null)
				throw new ArgumentNullException(nameof(sourceDirectory));
			if (targetDirectory == null)
				throw new ArgumentNullException(nameof(targetDirectory));
			if (File.Exists(targetDirectory))
				throw new ArgumentException("Target directory points to a file.");

			AssetDatabaseRuntimeTools.ReleaseCachedFileHandles(); // Make Unity release the files to prevent any IO errors.

			sourceDirectory = sourceDirectory.AddDirectorySeparatorToEnd().FixDirectorySeparatorChars();
			targetDirectory = targetDirectory.AddDirectorySeparatorToEnd().FixDirectorySeparatorChars();

			var filesToCopy = ListFilesInDirectory(sourceDirectory, searchOption, includeFilters, excludeFilters, throwOnError);

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
						relativeSourceFilePath = sourceFilePath.Remove(0, sourceDirectory.Length);
					}

					var targetFilePath = Path.Combine(targetDirectory, relativeSourceFilePath);

					// Create directory if does not exist
					var directory = Path.GetDirectoryName(targetFilePath);
					Directory.CreateDirectory(directory);

					// Copy file
					File.Copy(sourceFilePath, targetFilePath, overwrite);

					if (result != null)
						result.AddCopiedFile(sourceFilePath, targetFilePath);
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

		public static bool Delete(string path)
		{
			AssetDatabaseRuntimeTools.ReleaseCachedFileHandles(); // Make Unity release the files to prevent any IO errors.

			if (Directory.Exists(path))
			{
				Directory.Delete(path, true);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Deletes empty subdirectories and returns a list of failed subdirectories if there are any.
		/// </summary>
		public static List<string> DeleteEmptySubdirectories(string directoryPath)
		{
			AssetDatabaseRuntimeTools.ReleaseCachedFileHandles(); // Make Unity release the files to prevent any IO errors.

			List<string> failedDirectories = null;

			foreach (var subdirectory in Directory.GetDirectories(directoryPath))
			{
				var failedDirectoriesInSubdirectory = DeleteEmptySubdirectories(subdirectory);
				if (failedDirectoriesInSubdirectory != null)
				{
					if (failedDirectories == null)
						failedDirectories = failedDirectoriesInSubdirectory;
					else
						failedDirectories.AddRange(failedDirectoriesInSubdirectory);
				}
			}

			try
			{
				if (Directory.GetFiles(directoryPath).Length == 0 && Directory.GetDirectories(directoryPath).Length == 0)
				{
					Directory.Delete(directoryPath, false);
				}
			}
			catch
			{
				if (failedDirectories == null)
					failedDirectories = new List<string>();
				failedDirectories.Add(directoryPath);
			}
			return failedDirectories;
		}

		#endregion

		#region Delete Files

		/// <returns>Returns false if something goes wrong. Check 'failedFiles' list for detailed information.</returns>
		public static bool DeleteFilesWithExtensionInDirectory(string directoryPath, string extension, SearchOption searchOption, ref List<FileInfo> deletedFiles, ref List<FileInfo> failedFiles)
		{
			AssetDatabaseRuntimeTools.ReleaseCachedFileHandles(); // Make Unity release the files to prevent any IO errors.

			if (deletedFiles == null)
				deletedFiles = new List<FileInfo>();
			if (failedFiles == null)
				failedFiles = new List<FileInfo>();
			var error = false;
			var directoryInfo = new DirectoryInfo(directoryPath);
			var fileInfos = directoryInfo.GetFiles("*." + extension, searchOption).Where(fileInfo => fileInfo.Extension == "." + extension).ToList();
			for (int i = 0; i < fileInfos.Count; i++)
			{
				try
				{
					fileInfos[i].DeleteFileEvenIfReadOnly();
					deletedFiles.Add(fileInfos[i]);
				}
				catch
				{
					failedFiles.Add(fileInfos[i]);
					error = true;
				}
			}
			return !error;
		}

		/// <returns>Returns false if something goes wrong. Check 'failedFiles' list for detailed information.</returns>
		public static bool DeleteFilesWithPatternInDirectory(string directoryPath, string searchPattern, SearchOption searchOption, ref List<FileInfo> deletedFiles, ref List<FileInfo> failedFiles)
		{
			AssetDatabaseRuntimeTools.ReleaseCachedFileHandles(); // Make Unity release the files to prevent any IO errors.

			if (deletedFiles == null)
				deletedFiles = new List<FileInfo>();
			if (failedFiles == null)
				failedFiles = new List<FileInfo>();
			var error = false;
			var directoryInfo = new DirectoryInfo(directoryPath);
			var fileInfos = directoryInfo.GetFiles(searchPattern, searchOption).ToList();
			for (int i = 0; i < fileInfos.Count; i++)
			{
				try
				{
					fileInfos[i].DeleteFileEvenIfReadOnly();
					deletedFiles.Add(fileInfos[i]);
				}
				catch
				{
					failedFiles.Add(fileInfos[i]);
					error = true;
				}
			}
			return !error;
		}

		/// <returns>Returns false if something goes wrong. Check 'failedFiles' list for detailed information.</returns>
		public static bool ClearDLLArtifacts(string directoryPath, SearchOption searchOption, ref List<FileInfo> deletedFiles, ref List<FileInfo> failedFiles)
		{
			AssetDatabaseRuntimeTools.ReleaseCachedFileHandles(); // Make Unity release the files to prevent any IO errors.

			if (deletedFiles == null)
				deletedFiles = new List<FileInfo>();
			if (failedFiles == null)
				failedFiles = new List<FileInfo>();
			var error = false;
			var directoryInfo = new DirectoryInfo(directoryPath);
			var fileInfos = directoryInfo.GetFiles("*.dll", searchOption).Where(fileInfo => fileInfo.Extension == ".dll").ToList();
			for (int i = 0; i < fileInfos.Count; i++)
			{
				var dllFullPath = fileInfos[i].FullName;
				var dllDirectoryPath = Path.GetDirectoryName(dllFullPath);
				var dllFullPathWithoutExtension = Path.Combine(
					dllDirectoryPath,
					Path.GetFileNameWithoutExtension(dllFullPath));

				try
				{
					var path = dllFullPathWithoutExtension + ".mdb";
					if (FileTools.DeleteFileEvenIfReadOnly(path, true))
						deletedFiles.Add(new FileInfo(path));
				}
				catch
				{
					failedFiles.Add(fileInfos[i]);
					error = true;
				}

				try
				{
					var path = dllFullPathWithoutExtension + ".pdb";
					if (FileTools.DeleteFileEvenIfReadOnly(path, true))
						deletedFiles.Add(new FileInfo(path));
				}
				catch
				{
					failedFiles.Add(fileInfos[i]);
					error = true;
				}

				try
				{
					var path = dllFullPathWithoutExtension + ".xml";
					if (FileTools.DeleteFileEvenIfReadOnly(path, true))
						deletedFiles.Add(new FileInfo(path));
				}
				catch
				{
					failedFiles.Add(fileInfos[i]);
					error = true;
				}
			}
			return !error;
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
			AssetDatabaseRuntimeTools.ReleaseCachedFileHandles(); // Make Unity release the files to prevent any IO errors.

			const int maxTries = 10;
			const int waitBetweenTries = 1000; // ms
			for (int iTry = 0; iTry < maxTries; iTry++)
			{
				try
				{
					var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()).AddDirectorySeparatorToEnd();
					Directory.CreateDirectory(path);
					return path;
				}
				catch
				{
				}
				Thread.Sleep(waitBetweenTries);
			}
			throw new Exception("Failed to create temporary directory.");
		}

		#endregion

		#region Unity Project Directory

		/// <summary>
		/// Checks if the specified directory is a Unity project directory. It will throw in case Path.Combine or Directory.Exists methods are not happy with the given path.
		/// </summary>
		public static bool IsUnityProjectPath(string directoryPath)
		{
			return
				Directory.Exists(directoryPath) &&
				Directory.Exists(Path.Combine(directoryPath, "Assets")) &&
				Directory.Exists(Path.Combine(directoryPath, "ProjectSettings"));
		}

		#endregion
	}

}
