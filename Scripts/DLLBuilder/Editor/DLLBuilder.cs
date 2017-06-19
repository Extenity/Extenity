using System;
using System.Linq;
using Extenity.ConsistencyToolbox;
using UnityEngine;
using UnityEngine.Events;

namespace Extenity.DLLBuilder
{

	public static class DLLBuilder
	{
		#region Configuration

		//public static readonly string ExtenitySourcesBasePath = "Assets/Extenity/";

		//public static readonly string[] References =
		//{
		//	@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v3.5\Profile\Unity Full v3.5\mscorlib.dll",
		//	@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v3.5\Profile\Unity Full v3.5\System.Core.dll",
		//	@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v3.5\Profile\Unity Full v3.5\System.Data.dll",
		//	@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v3.5\Profile\Unity Full v3.5\System.dll",
		//	@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v3.5\Profile\Unity Full v3.5\System.Xml.dll",
		//	@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v3.5\Profile\Unity Full v3.5\System.Xml.Linq.dll",
		//	@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v3.5\Profile\Unity Full v3.5\Boo.Lang.dll",
		//	@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v3.5\Profile\Unity Full v3.5\UnityScript.Lang.dll",
		//};

		#endregion

		#region Process

		public static bool IsProcessing { get; private set; }

		public static void StartProcess(BuildTriggerSource triggerSource)
		{
			var newJob = new BuildJob
			{
				JobID = Guid.NewGuid()
			};
			var newJobStatus = newJob.AddCurrentProjectToChain();
			newJobStatus.IsCurrentlyProcessedProject = true;
			StartProcess(newJob, triggerSource);
		}

		public static void StartProcess(BuildJob job, BuildTriggerSource triggerSource)
		{
			if (IsProcessing)
				throw new Exception("A process was already started.");
			IsProcessing = true;

			var jobStatus = job.CurrentlyProcessedProjectStatus;
			if (jobStatus == null)
				throw new Exception("Internal error! Currently processed project status is not set.");

			try
			{
				if (!jobStatus.IsStarted)
				{
					Debug.Log(Constants.DLLBuilderName + " started to build all DLLs. Job ID: " + job.JobID);
					jobStatus.IsStarted = true;

					if (jobStatus.BuildTriggerSource != BuildTriggerSource.Unspecified)
						throw new Exception(string.Format("Build trigger source was already specified as '{0}' where it was going to be set as '{1}'.", jobStatus.BuildTriggerSource, triggerSource));
					jobStatus.BuildTriggerSource = triggerSource;
				}
				else
				{
					// Means we are in the middle of build process. That is we are continuing after a recompilation.
					Debug.Log(Constants.DLLBuilderName + " continuing to build all DLLs. Job ID: " + job.JobID);
				}

				var remoteProjectPaths = DLLBuilderConfiguration.Instance.EnabledAndIgnoreFilteredRemoteBuilderConfigurations.Select(item => item.ProjectPath).ToArray();
				jobStatus.SetRemoteProjectStatusList(remoteProjectPaths);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				InternalFinishProcess(job, jobStatus, false);
				return;
			}

			if (job.CheckConsistencyAndLog().Count > 0)
			{
				InternalFinishProcess(job, jobStatus, false);
				return;
			}

			Repaint();

			RemoteBuilder.CreateBuildRequestsOfRemoteProjects(DLLBuilderConfiguration.Instance, job, jobStatus,
				() =>
				{
					Cleaner.ClearAllOutputDLLs(DLLBuilderConfiguration.Instance,
						() =>
						{
							Repaint();

							Compiler.CompileAllDLLs(DLLBuilderConfiguration.Instance,
								() =>
								{
									var succeeded = false;
									try
									{
										Repaint();
										Packer.PackAll();
										Repaint();
										Distributer.DistributeToAll(DLLBuilderConfiguration.Instance);

										Debug.Log(Constants.DLLBuilderName + " successfully built all DLLs.");
										succeeded = true;
									}
									catch (Exception exception)
									{
										Debug.LogException(exception);
									}
									InternalFinishProcess(job, jobStatus, succeeded);
								},
								error =>
								{
									Debug.LogError(error);
									InternalFinishProcess(job, jobStatus, false);
								}
							);
						},
						exception =>
						{
							Debug.LogException(exception);
							InternalFinishProcess(job, jobStatus, false);
						}
					);
				},
				error =>
				{
					Debug.LogError(error);
					InternalFinishProcess(job, jobStatus, false);
				}
			);
		}

		private static void InternalFinishProcess(BuildJob job, BuildJobStatus jobStatus, bool succeeded)
		{
			try
			{
				IsProcessing = false;

				BuildJob.DeleteAssemblyReloadSurvivalFile();

				if (succeeded)
					jobStatus.IsSucceeded = true;
				else
					jobStatus.IsFailed = true;

				RemoteBuilder.SaveBuildResponseFile(job);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);

				// Well, it won't count as succeeded if we can't finalize the process.
				jobStatus.IsSucceeded = false;
				jobStatus.IsFailed = true;
			}

			Repaint();
		}

		#endregion

		#region UI Repaint

		public static readonly UnityEvent OnRepaintRequested = new UnityEvent();

		public static void Repaint()
		{
			OnRepaintRequested.Invoke();
		}

		#endregion
	}

}
