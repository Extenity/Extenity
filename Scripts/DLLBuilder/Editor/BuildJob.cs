using System;
using UnityEngine;
using System.IO;
using UnityEditor;

namespace Extenity.DLLBuilder
{

	/// <summary>
	/// Keeps track of the state of build process. More importantly, the real reason we need DLLBuildJob is  to continue 
	/// building after recompilations. DLLBuildJob is written to disk and ContinueAfterRecompilation reads the latest 
	/// state from disk. Then tells DLLBuilder to continue where it's left off.
	/// </summary>
	[Serializable]
	public class BuildJob
	{
		public BuildRequest BuildRequest;

		public bool IsStarted;
		public bool IsRemoteBuildsCompleted;

		// We won't be needing these. We only need to keep track of what has happened between recompilations and recompilation only happens after remote compilations, which will likely update some DLLs in this project.
		//public bool IsCleanUpCompleted;
		//public bool IsCompilationCompleted;

		#region Save/Load File

		public void SaveToFile()
		{
			try
			{
				//if (Instance == null)
				//	throw new Exception("Build job information was not created.");

				var json = JsonUtility.ToJson(this, true);
				File.WriteAllText(Constants.BuildJob.CurrentBuildJobFilePath, json);
			}
			catch (Exception exception)
			{
				throw new Exception("Failed to save " + Constants.DLLBuilderName + " current job information to continue after recompilation.", exception);
			}
		}

		public static BuildJob LoadFromFile()
		{
			try
			{
				//if (Instance != null)
				//	throw new Exception("Build job information was already created as for request: " + Instance.BuildRequest.ToString());

				if (!File.Exists(Constants.BuildJob.CurrentBuildJobFilePath))
					return null;

				var json = File.ReadAllText(Constants.BuildJob.CurrentBuildJobFilePath);
				return JsonUtility.FromJson<BuildJob>(json);
			}
			catch (Exception exception)
			{
				throw new Exception("Failed to load " + Constants.DLLBuilderName + " current job information to continue after recompilation.", exception);
			}
		}

		#endregion

		#region Continue After Recompilation

		[InitializeOnLoadMethod]
		private static void ContinueAfterRecompilation()
		{
			var buildJob = LoadFromFile();
			if (buildJob != null)
			{
				DLLBuilder.StartProcess(buildJob);
			}
		}

		#endregion
	}

}
