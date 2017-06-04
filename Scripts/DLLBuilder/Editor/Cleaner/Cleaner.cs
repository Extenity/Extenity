using System;
using UnityEngine;
using System.IO;
using Extenity.DataToolbox;
using Extenity.DebugToolbox;

namespace Extenity.DLLBuilder
{

	public static class Cleaner
	{
		public static void ClearOutputDLLs(Action onSucceeded = null, Action<Exception> onFailed = null)
		{
			try
			{
				Debug.Log("Clearing output DLLs");
				var failedFiles = DirectoryTools.DeleteFilesWithExtensionInDirectory(Constants.OutputProjectDLLDirectory, "dll", SearchOption.AllDirectories);
				if (failedFiles.IsNotNullAndEmpty())
				{
					failedFiles.LogErrorList("Following DLL files are failed to be deleted. Please check if they are in use by other applications. Restarting Unity may help.");
					throw new Exception("Failed to clear output DLLs. See previous log entry for details");
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
	}

}
