using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Extenity.ApplicationToolbox.Editor;
using Extenity.ConsistencyToolbox;
using Extenity.DataToolbox;
using Newtonsoft.Json;
using UnityEditor;
using Guid = System.Guid;

namespace Extenity.DLLBuilder
{

	/// <summary>
	/// Keeps track of the state of build process. 
	/// 
	/// BuildJob is used for continuing build process after recompilations. 
	/// Current state of BuildJob is written to disk before recompilation 
	/// and ContinueAfterRecompilation reads the latest state from disk 
	/// after assemblies loaded. 
	/// 
	/// Another area we use BuildJob is informing remote projects about 
	/// the whole build process. 
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class BuildJob : IConsistencyChecker
	{
		#region Initialization

		public BuildJob()
		{
			ProjectChain = new BuildJobStatus[0];
		}

		#endregion

		#region Data - Metadata

		[JsonProperty]
		public Guid JobID;

		#endregion

		#region Data - Project Chain

		[JsonProperty]
		public BuildJobStatus[] ProjectChain;

		public BuildJobStatus AddCurrentProjectToChain()
		{
			var jobStatus = new BuildJobStatus(EditorApplicationTools.UnityProjectPath);
			ProjectChain = ProjectChain.Add(jobStatus);
			return jobStatus;
		}

		#endregion

		#region Currently Processed Project

		[NonSerialized]
		private BuildJobStatus _CurrentlyProcessedProjectStatus;
		public BuildJobStatus CurrentlyProcessedProjectStatus
		{
			get
			{
				if (_CurrentlyProcessedProjectStatus == null || _CurrentlyProcessedProjectStatus.IsCurrentlyProcessedProject == false)
				{
					_CurrentlyProcessedProjectStatus = InternalFindCurrentlyProcessedProjectStatusRecursively();
				}
				return _CurrentlyProcessedProjectStatus;
			}
		}

		private BuildJobStatus InternalFindCurrentlyProcessedProjectStatusRecursively()
		{
			for (var i = 0; i < ProjectChain.Length; i++)
			{
				var found = InternalFindCurrentlyProcessedProjectStatusRecursively(ProjectChain[i]);
				if (found != null)
					return found;
			}
			return null;
		}

		private BuildJobStatus InternalFindCurrentlyProcessedProjectStatusRecursively(BuildJobStatus status)
		{
			if (status == null)
				return null;
			if (status.IsCurrentlyProcessedProject)
				return status;
			if (status.IsRemoteProjectDataAvailable)
			{
				for (var i = 0; i < status.RemoteProjects.Length; i++)
				{
					var found = InternalFindCurrentlyProcessedProjectStatusRecursively(status.RemoteProjects[i]);
					if (found != null)
						return found;
				}
			}
			return null;
		}

		public void SetCurrentlyProcessedProject(BuildJobStatus remoteProjectStatus)
		{
			UnsetCurrentlyProcessedProject();

			remoteProjectStatus.IsCurrentlyProcessedProject = true;
		}

		public void UnsetCurrentlyProcessedProject()
		{
			foreach (var status in ProjectChain)
			{
				UnsetCurrentlyProcessedProject(status);
			}
		}

		private void UnsetCurrentlyProcessedProject(BuildJobStatus status)
		{
			status.IsCurrentlyProcessedProject = false;
			if (status.IsRemoteProjectDataAvailable)
			{
				foreach (var remoteProject in status.RemoteProjects)
					UnsetCurrentlyProcessedProject(remoteProject);
			}
		}

		public bool UpdateCurrentlyProcessedProjectStatus(BuildJobStatus newStatus)
		{
			var oldStatus = CurrentlyProcessedProjectStatus;
			_CurrentlyProcessedProjectStatus = null; // Because we will change this object with newStatus. So we must get rid of this cached reference.

			for (int i = 0; i < ProjectChain.Length; i++)
			{
				if (ProjectChain[i] == oldStatus)
				{
					ProjectChain[i] = newStatus;
					return true;
				}
				var result = InternalChangeCurrentlyProcessedProjectStatusReference(ProjectChain[i], oldStatus, newStatus);
				if (result)
					return true;
			}
			return false;
		}

		private bool InternalChangeCurrentlyProcessedProjectStatusReference(BuildJobStatus iteratedStatus, BuildJobStatus oldStatus, BuildJobStatus newStatus)
		{
			if (iteratedStatus.IsRemoteProjectDataAvailable)
			{
				for (int i = 0; i < iteratedStatus.RemoteProjects.Length; i++)
				{
					if (iteratedStatus.RemoteProjects[i] == oldStatus)
					{
						iteratedStatus.RemoteProjects[i] = newStatus;
						return true;
					}
					InternalChangeCurrentlyProcessedProjectStatusReference(iteratedStatus.RemoteProjects[i], oldStatus, newStatus);
				}
			}
			return false;
		}

		#endregion

		#region Save/Load Assembly Reload Survival File

		public void SaveBeforeAssemblyReload()
		{
			try
			{
				var json = JsonConvert.SerializeObject(this, Formatting.Indented);
				var filePath = Constants.BuildJob.AssemblyReloadSurvivalFilePath;
				DirectoryTools.CreateFromFilePath(filePath);
				File.WriteAllText(filePath, json);
			}
			catch (Exception exception)
			{
				throw new Exception("Failed to save " + Constants.DLLBuilderName + " current job information to continue after recompilation.", exception);
			}
		}

		public static BuildJob LoadAfterAssemblyReload()
		{
			try
			{
				if (!File.Exists(Constants.BuildJob.AssemblyReloadSurvivalFilePath))
					return null;

				var json = File.ReadAllText(Constants.BuildJob.AssemblyReloadSurvivalFilePath);
				DeleteAssemblyReloadSurvivalFile();
				return JsonConvert.DeserializeObject<BuildJob>(json);
			}
			catch (Exception exception)
			{
				throw new Exception("Failed to load " + Constants.DLLBuilderName + " current job information to continue after recompilation.", exception);
			}
		}

		public static void DeleteAssemblyReloadSurvivalFile()
		{
			try
			{
				File.Delete(Constants.BuildJob.AssemblyReloadSurvivalFilePath);
			}
			catch
			{
				// ignored
			}
		}

		#endregion

		#region Continue After Recompilation

		[InitializeOnLoadMethod]
		private static void ContinueAfterRecompilation()
		{
			var buildJob = LoadAfterAssemblyReload();
			if (buildJob != null)
			{
				DLLBuilder.StartProcess(buildJob, BuildTriggerSource.ContinueAfterAssemblyReload);
			}
		}

		#endregion

		#region Consistency

		public void CheckConsistency(ref List<ConsistencyError> errors)
		{
			if (JobID == Guid.Empty)
			{
				errors.Add(new ConsistencyError(this, "Job ID is not specified."));
			}

			if (ProjectChain == null || ProjectChain.Length == 0)
			{
				errors.Add(new ConsistencyError(this, "Project chain is empty."));
			}
			else
			{
				for (var i = 0; i < ProjectChain.Length; i++)
				{
					var jobStatus = ProjectChain[i];
					jobStatus.CheckConsistency(ref errors);
				}
			}
		}

		#endregion

		#region ToString

		public override string ToString()
		{
			return "Build Job '" + JobID + "' as requested by following projects:\n" + StringTools.Serialize(ProjectChain.Select(item => item.ProjectPath).ToList(), '\n');
		}

		#endregion
	}

}
