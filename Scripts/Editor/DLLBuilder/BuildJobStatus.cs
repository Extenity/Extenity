using System;
using System.Collections.Generic;
using System.IO;
using Extenity.ConsistencyToolbox;
using Extenity.DataToolbox;
using Newtonsoft.Json;

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
	[JsonObject(MemberSerialization.OptIn)]
	public class BuildJobStatus : IConsistencyChecker
	{
		#region Initialization

		public BuildJobStatus(string projectPath)
		{
			ProjectPath = projectPath;
			RemoteProjects = null; // Null means we don't know remote projects yet. See IsRemoteProjectDataAvailable.
		}

		#endregion

		#region Data - Metadata

		/// <summary>
		/// Unity project path.
		/// </summary>
		[JsonProperty]
		public string ProjectPath;

		/// <summary>
		/// Tells about why this build process started.
		/// </summary>
		[JsonProperty]
		public BuildTriggerSource BuildTriggerSource;

		#endregion

		#region Data - Build Status

		[JsonProperty]
		public bool IsStarted;
		[JsonProperty]
		public bool IsSucceeded;
		[JsonProperty]
		public bool IsFailed;
		//[JsonProperty]
		//public bool IsCancelled;
		public bool IsFinished { get { return IsSucceeded || IsFailed; } }

		[JsonProperty]
		public bool IsRemoteBuildsCompleted;
		[JsonProperty]
		public bool IsCollectorCompleted;

		// We won't be needing these. We only need to keep track of what has happened between recompilations and recompilation only happens after remote compilations, which will likely update some DLLs in this project.
		//public bool IsCleanUpCompleted;
		//public bool IsCompilationCompleted;

		//public void RequestCancel()
		//{
		//	if (IsCancelled)
		//		return;
		//	DLLBuilder.LogAndUpdateStatus("Cancel requested");
		//	IsCancelled = true;
		//}

		#endregion

		#region Data - Currently Processed Project

		[JsonProperty]
		public bool IsCurrentlyProcessedProject;

		#endregion

		#region Data - Remote Projects

		[JsonProperty]
		public BuildJobStatus[] RemoteProjects;
		public bool IsRemoteProjectDataAvailable { get { return RemoteProjects != null; } }

		public BuildJobStatus GetRemoteProject(string projectPath)
		{
			if (!IsRemoteProjectDataAvailable)
				return null;
			for (var i = 0; i < RemoteProjects.Length; i++)
			{
				var status = RemoteProjects[i];
				if (status.ProjectPath.PathCompare(projectPath))
					return status;
			}
			return null;
		}

		public void SetRemoteProjectStatusList(string[] remoteProjectPaths)
		{
			if (remoteProjectPaths != null)
			{
				Array.Resize(ref RemoteProjects, remoteProjectPaths.Length);
				for (var i = 0; i < remoteProjectPaths.Length; i++)
				{
					var remoteProjectPath = remoteProjectPaths[i];
					// We don't know remote projects of the remote project. That information 
					// will be filled inside remote project and we will fetch the information
					// via remote build results.
					RemoteProjects[i] = new BuildJobStatus(remoteProjectPath);
				}
			}
			else
			{
				//RemoteProjects = new BuildJobStatus[0]; Not that line! We want to keep it as unknown, which is stated with null value.
				RemoteProjects = null;
			}
		}

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
				errors.Add(new ConsistencyError(this, $"Project Path directory '{ProjectPath}' does not exist."));
			}

			//if (BuildTriggerSource == BuildTriggerSource.Unspecified)
			//{
			//	errors.Add(new ConsistencyError(this, "Build Trigger Source must be specified."));
			//}

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
