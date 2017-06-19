using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using Extenity.ApplicationToolbox.Editor;
using Extenity.ConsistencyToolbox;
using Extenity.DataToolbox;
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
	[Serializable]
	public class BuildJob : IConsistencyChecker
	{
		#region Initialization

		public BuildJob()
		{
			ProjectChain = new BuildJobStatus[0];
		}

		#endregion

		#region Data - Metadata

		public Guid JobID;

		#endregion

		#region Data - Project Chain

		public BuildJobStatus[] ProjectChain;

		public void AddCurrentProjectToChain(string[] remoteProjectPaths)
		{
			ProjectChain = ProjectChain.Add(new BuildJobStatus(EditorApplicationTools.UnityProjectPath, remoteProjectPaths));
		}

		#endregion

		#region Current Project Status in Project Chain

		private BuildJobStatus _CurrentProjectStatus;
		public BuildJobStatus CurrentProjectStatus
		{
			get
			{
				if (_CurrentProjectStatus == null)
				{
					var currentProjectPath = EditorApplicationTools.UnityProjectPath;
					foreach (var jobStatus in ProjectChain)
					{
						if (jobStatus.ProjectPath.PathCompare(currentProjectPath))
						{
							_CurrentProjectStatus = jobStatus;
							break;
						}
					}
					if (_CurrentProjectStatus == null)
						throw new Exception("Internal error! Current project status does not exist in project chain.");
				}
				return _CurrentProjectStatus;
			}
		}

		#endregion

		#region Save/Load Assembly Reload Survival File

		public void SaveBeforeAssemblyReload()
		{
			try
			{
				var json = JsonUtility.ToJson(this, true);
				File.WriteAllText(Constants.BuildJob.AssemblyReloadSurvivalFilePath, json);
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
