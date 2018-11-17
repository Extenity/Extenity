using System;
using System.Collections.Generic;
using System.IO;
using Extenity.ConsistencyToolbox;
using Extenity.DataToolbox;

namespace Extenity.DLLBuilder
{

	public static class Cleaner
	{
		public static void ClearAllOutputDLLs(DLLBuilderConfiguration configuration, Action onSucceeded = null, Action<Exception> onFailed = null)
		{
			DLLBuilder.LogAndUpdateStatus("Clearing all existing DLLs");

			try
			{
				foreach (var compilerConfiguration in configuration.CompilerConfigurations)
				{
					ClearOutputDLLs(compilerConfiguration, true, true);
				}

				if (onSucceeded != null)
					onSucceeded();
			}
			catch (Exception exception)
			{
				if (onFailed != null)
					onFailed(exception);
			}
		}

		public static void ClearOutputDLLs(CompilerConfiguration configuration, bool clearRutimeDLL, bool clearEditorDLL)
		{
			// Do not check whether compiler configuration is enabled or not.

			// Check consistency first
			{
				var errors = new List<ConsistencyError>();
				configuration.CheckConsistencyOfPaths(ref errors); // Note that this does not check whether configuration is enabled or not.
				if (errors.Count > 0)
				{
					throw new Exception("Failed to clear output because of consistency errors:\n" + errors.Serialize('\n'));
				}
			}

			var result = new List<string>();
			var filesToDelete = new List<string>();
			var failed = false;

			if (clearRutimeDLL)
			{
				filesToDelete.Add(configuration.DLLPath);
				filesToDelete.Add(configuration.DLLDocumentationPath);
				filesToDelete.Add(configuration.DLLDebugDatabasePath);
			}
			if (clearEditorDLL)
			{
				filesToDelete.Add(configuration.EditorDLLPath);
				filesToDelete.Add(configuration.EditorDLLDocumentationPath);
				filesToDelete.Add(configuration.EditorDLLDebugDatabasePath);
			}

			foreach (var fileToDelete in filesToDelete)
			{
				try
				{
					if (File.Exists(fileToDelete))
					{
						FileTools.DeleteFileEvenIfReadOnly(fileToDelete);
						result.Add("Deleted : " + fileToDelete);
					}
					else
					{
						result.Add("Not Found : " + fileToDelete);
					}
				}
				catch (DirectoryNotFoundException)
				{
					result.Add("Not Found : " + fileToDelete);
				}
				//catch (FileNotFoundException) // Well, this does not happen since File.Delete won't throw if file not found.
				//{
				//	result.Add("Not Found : " + fileToDelete);
				//}
				catch (Exception exception)
				{
					failed = true;
					result.Add("FAILED : " + fileToDelete + " (reason: " + exception.Message + ")");
				}
			}

			result.Sort(); // This sorting will make FAILED appear on top.

			if (failed)
			{
				throw new Exception($"Failed to clear previous output files: (runtime: {clearRutimeDLL}, editor: {clearEditorDLL})\n{result.Serialize('\n')}");
			}
			else
			{
				DLLBuilder.LogAndUpdateStatus($"Cleared previous output files: (runtime: {clearRutimeDLL}, editor: {clearEditorDLL})\n{result.Serialize('\n')}");
			}
		}
	}

}
