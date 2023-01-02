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
			var list = New.List<string>(slicedFolders.Count);
			foreach (var folder in slicedFolders)
			{
				list.Add(folder);
			}
			list.Sort(Comparer<string>.Default);
			return list;
		}

		#endregion

		#region Files In Directory

		public static bool IsDirectoryEmpty(string path)
		{
			// The old implementation. It was not performance friendly.
			// return Directory.GetFileSystemEntries(path).Length == 0;

			try
			{
				var items = Directory.EnumerateFileSystemEntries(path);
				using (var enumerator = items.GetEnumerator())
				{
					return !enumerator.MoveNext();
				}
			}
			catch (DirectoryNotFoundException)
			{
				return true;
			}
		}

		public static bool IsDirectoryEmptyOrOnlyContainsEmptySubdirectories(string path)
		{
			// Note that EnumerateFiles is performance friendly. It will stop looking for files
			// the moment it finds a file, instead of traversing the full directory tree.
			return !Directory.EnumerateFiles(path).Any();
		}

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

		#region Create Directory

		public static bool Create(string directoryPath)
		{
			AssetDatabaseRuntimeTools.ReleaseCachedFileHandles(); // Make Unity release the files to prevent any IO errors.

			if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
				return true;
			}
			return false;
		}

		public static bool CreateFromFilePath(string filePath)
		{
			AssetDatabaseRuntimeTools.ReleaseCachedFileHandles(); // Make Unity release the files to prevent any IO errors.

			var directoryPath = Path.GetDirectoryName(filePath);
			if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
				return true;
			}
			return false;
		}

		public static string CreateSubDirectory(string parentDirectory, string subDirectory)
		{
			// AssetDatabaseRuntimeTools.ReleaseCachedFileHandles(); Already done in Create() below

			var path = Path.Combine(parentDirectory, subDirectory);
			Create(path);
			return path;
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

			private List<FailReason> _FailedFiles = New.List<FailReason>();
			public List<FailReason> FailedFiles { get { return _FailedFiles; } }
			public int FailedFileCount { get { return _FailedFiles == null ? 0 : _FailedFiles.Count; } }
			private List<CopiedFile> _CopiedFiles = New.List<CopiedFile>();
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
				Release.List(ref _CopiedFiles);
				Release.List(ref _FailedFiles);
			}

			public void AddCopiedFile(string sourceFilePath, string targetFilePath)
			{
				CopiedFileCount++;
				if (CreateCopiedFileList)
				{
					_CopiedFiles.Add(new CopiedFile(sourceFilePath, targetFilePath));
				}
			}

			public void AddFailedFile(FailReason failReason)
			{
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
							result.FailedFiles = New.List<CopyResult.FailReason>();
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

		private static void _DeleteNonRecursive(string path)
		{
			try
			{
				Directory.Delete(path, false);
			}
			catch (IOException) 
			{
				Thread.Sleep(1); // Allow system to release file handles by waiting and then try once more
				Directory.Delete(path, false);
			}
			catch (UnauthorizedAccessException)
			{
				Thread.Sleep(1); // Allow system to release file handles by waiting and then try once more
				Directory.Delete(path, false);
			}
		}

		public static bool DeleteIfEmpty(string path)
		{
			AssetDatabaseRuntimeTools.ReleaseCachedFileHandles(); // Make Unity release the files to prevent any IO errors.

			if (Directory.Exists(path) &&
				Directory.GetFiles(path).Length == 0 &&
			    Directory.GetDirectories(path).Length == 0)
			{
				_DeleteNonRecursive(path);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Source: https://stackoverflow.com/questions/329355/cannot-delete-directory-with-directory-deletepath-true
		/// </summary>
		public static bool DeleteWithContent(string path)
		{
			AssetDatabaseRuntimeTools.ReleaseCachedFileHandles(); // Make Unity release the files to prevent any IO errors.

			if (Directory.Exists(path))
			{
				var files = Directory.GetFiles(path);
				var subDirectories = Directory.GetDirectories(path);

				foreach (var file in files)
				{
					FileTools.Delete(file);
				}

				foreach (var subDirectory in subDirectories)
				{
					DeleteWithContent(subDirectory);
				}

				_DeleteNonRecursive(path);
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
				DeleteIfEmpty(directoryPath);
			}
			catch
			{
				if (failedDirectories == null)
					failedDirectories = New.List<string>();
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
				deletedFiles = New.List<FileInfo>();
			if (failedFiles == null)
				failedFiles = New.List<FileInfo>();
			var error = false;
			var directoryInfo = new DirectoryInfo(directoryPath);
			var fileInfos = directoryInfo.GetFiles("*." + extension, searchOption).Where(fileInfo => fileInfo.Extension == "." + extension).ToList();
			for (int i = 0; i < fileInfos.Count; i++)
			{
				try
				{
					FileTools.Delete(fileInfos[i]);
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
				deletedFiles = New.List<FileInfo>();
			if (failedFiles == null)
				failedFiles = New.List<FileInfo>();
			var error = false;
			var directoryInfo = new DirectoryInfo(directoryPath);
			var fileInfos = directoryInfo.GetFiles(searchPattern, searchOption).ToList();
			for (int i = 0; i < fileInfos.Count; i++)
			{
				try
				{
					FileTools.Delete(fileInfos[i]);
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
		public static bool ClearDLLArtifacts(string directoryPath, SearchOption searchOption, List<FileInfo> deletedFiles, List<FileInfo> failedFiles)
		{
			AssetDatabaseRuntimeTools.ReleaseCachedFileHandles(); // Make Unity release the files to prevent any IO errors.

			var initialErrors = failedFiles.Count;
			var directoryInfo = new DirectoryInfo(directoryPath);

			FindAndDeleteIncludingXMLCounterparts(".dll");
			FindAndDeleteIncludingXMLCounterparts(".mdb");
			FindAndDeleteIncludingXMLCounterparts(".pdb");

			return initialErrors != failedFiles.Count;

			void FindAndDeleteIncludingXMLCounterparts(string extension)
			{
				var fileInfos = directoryInfo.GetFiles("*" + extension, searchOption)
				                             .Where(fileInfo => fileInfo.Extension == extension)
				                             .ToList();
				for (int i = 0; i < fileInfos.Count; i++)
				{
					try
					{
						var fullPath = fileInfos[i].FullName;
						var fullPathWithoutExtension = Path.Combine(
							Path.GetDirectoryName(fullPath),
							Path.GetFileNameWithoutExtension(fullPath));

						var path = fullPathWithoutExtension + ".xml";
						if (FileTools.Delete(path, true))
						{
							deletedFiles.Add(new FileInfo(path));
						}
					}
					catch
					{
						failedFiles.Add(fileInfos[i]);
					}
				}
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
	}

}
