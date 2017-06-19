using System;
using System.Collections.Generic;
using System.IO;
using Extenity.ConsistencyToolbox;

namespace Extenity.DLLBuilder
{

	public enum BuildTriggerSource
	{
		Unspecified = 0,
		UI = 5,
		RemoteBuildRequest = 10,
		//CommandLine,

		ContinueAfterAssemblyReload = 40,
	}

	/// <summary>
	/// Represents the build status of one project. Also keeps links 
	/// to the project's remote projects, like a tree structure.
	/// </summary>
	[Serializable]
	public class BuildJobStatus : IConsistencyChecker
	{
		#region Initialization

		public BuildJobStatus(string projectPath, string[] remoteProjectPaths)
		{
			ProjectPath = projectPath;
			if (remoteProjectPaths != null)
			{
				Array.Resize(ref RemoteProjects, remoteProjectPaths.Length);
				for (var i = 0; i < remoteProjectPaths.Length; i++)
				{
					var remoteProjectPath = remoteProjectPaths[i];
					// As passing null, we are telling we don't know remote projects of the remote project.
					// That information will be filled inside remote project and we will fetch the information
					// via build results.
					RemoteProjects[i] = new BuildJobStatus(remoteProjectPath, null);
				}
			}
			else
			{
				//RemoteProjects = new BuildJobStatus[0]; Not that line! We want to keep it as unknown.
				RemoteProjects = null;
			}
		}

		#endregion

		#region Data - Metadata

		/// <summary>
		/// Unity project path.
		/// </summary>
		public string ProjectPath;

		/// <summary>
		/// Tells about why this build process started.
		/// </summary>
		public BuildTriggerSource BuildTriggerSource;

		#endregion

		#region Data - Build Status

		public bool IsStarted;
		public bool IsSucceeded;
		public bool IsFailed;
		public bool IsFinished { get { return IsSucceeded || IsFailed; } }

		public bool IsRemoteBuildsCompleted;

		// We won't be needing these. We only need to keep track of what has happened between recompilations and recompilation only happens after remote compilations, which will likely update some DLLs in this project.
		//public bool IsCleanUpCompleted;
		//public bool IsCompilationCompleted;

		#endregion

		#region Data - Remote Projects

		public BuildJobStatus[] RemoteProjects;
		public bool IsRemoteProjectDataAvailable { get { return RemoteProjects != null; } }

		#endregion

		#region Consistency

		public void CheckConsistency(ref List<ConsistencyError> errors)
		{
			if (string.IsNullOrEmpty(ProjectPath))
			{
				errors.Add(new ConsistencyError(this, "Project Path must be specified."));
			}
			if (!Directory.Exists(ProjectPath))
			{
				errors.Add(new ConsistencyError(this, string.Format("Project Path directory '{0}' does not exist.", ProjectPath)));
			}

			if (BuildTriggerSource == BuildTriggerSource.Unspecified)
			{
				errors.Add(new ConsistencyError(this, "Build Trigger Source must be specified."));
			}

			if (IsRemoteProjectDataAvailable)
			{
				for (var i = 0; i < RemoteProjects.Length; i++)
				{
					var remoteProject = RemoteProjects[i];
					remoteProject.CheckConsistency(ref errors);
				}
			}
		}

		#endregion
	}

}
