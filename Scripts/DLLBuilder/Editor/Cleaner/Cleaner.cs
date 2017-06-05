using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Extenity.ConsistencyToolbox;
using Extenity.DataToolbox;

namespace Extenity.DLLBuilder
{

	public static class Cleaner
	{
		public static void ClearAllOutputDLLs(DLLBuilderConfiguration configuration, Action onSucceeded = null, Action<Exception> onFailed = null)
		{
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

		public static bool ClearOutputDLLs(DLLBuilderConfiguration.CompilerConfiguration configuration, bool clearRutimeDLL, bool clearEditorDLL)
		{
			// Check consistency first
			{
				var errors = new List<ConsistencyError>();
				configuration.CheckConsistencyOfPaths(ref errors);
				if (errors.Count > 0)
				{
					Debug.LogError("Failed to clear output because of consistency errors:\n" + errors.Serialize('\n'));
					return false;
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
						File.Delete(fileToDelete);
						result.Add("OK : " + fileToDelete);
					}
					else
					{
						result.Add("OK (not found) : " + fileToDelete);
					}
				}
				catch (DirectoryNotFoundException)
				{
					result.Add("OK (not found) : " + fileToDelete);
				}
				//catch (FileNotFoundException) // Well, this does not happen since File.Delete won't throw if file not found.
				//{
				//	result.Add("OK (not found) : " + fileToDelete);
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
				Debug.LogErrorFormat("Failed to clear previous output files: (runtime: {0}, editor: {1})\n{2}", clearRutimeDLL, clearEditorDLL, result.Serialize('\n'));
			}
			else
			{
				Debug.LogFormat("Cleared previous output files: (runtime: {0}, editor: {1})\n{2}", clearRutimeDLL, clearEditorDLL, result.Serialize('\n'));
			}
			return !failed;
		}
	}

}
