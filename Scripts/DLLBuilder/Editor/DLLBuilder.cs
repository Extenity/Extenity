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
			newJob.AddCurrentProjectToChain(DLLBuilderConfiguration.Instance.EnabledAndIgnoreFilteredRemoteBuilderConfigurations.Select(item => item.ProjectPath).ToArray());
			StartProcess(newJob, triggerSource);
		}

		public static void StartProcess(BuildJob job, BuildTriggerSource triggerSource)
		{
			if (IsProcessing)
				throw new Exception("A process was already started.");
			IsProcessing = true;

			try
			{
				if (!job.CurrentProjectStatus.IsStarted)
				{
					Debug.Log(Constants.DLLBuilderName + " started to build all DLLs. Job ID: " + job.JobID);
					job.CurrentProjectStatus.IsStarted = true;

					if (job.CurrentProjectStatus.BuildTriggerSource != BuildTriggerSource.Unspecified)
						throw new Exception(string.Format("Build trigger source was already specified as '{0}' where it was going to be set as '{1}'.", job.CurrentProjectStatus.BuildTriggerSource, triggerSource));
					job.CurrentProjectStatus.BuildTriggerSource = triggerSource;
				}
				else
				{
					// Means we are in the middle of build process. That is we are continuing after a recompilation.
					Debug.Log(Constants.DLLBuilderName + " continuing to build all DLLs. Job ID: " + job.JobID);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				InternalFinishProcess(job, false);
				return;
			}

			if (job.CheckConsistencyAndLog().Count > 0)
			{
				InternalFinishProcess(job, false);
				return;
			}

			Repaint();

			Cleaner.ClearAllOutputDLLs(DLLBuilderConfiguration.Instance,
				() =>
				{
					Repaint();

					Compiler.CompileAllDLLs(
						() =>
						{
							var succeeded = false;
							try
							{
								Repaint();
								Packer.PackAll();
								Repaint();
								Distributer.DistributeToAll();

								Debug.Log(Constants.DLLBuilderName + " successfully built all DLLs.");
								succeeded = true;
							}
							catch (Exception exception)
							{
								Debug.LogException(exception);
							}
							InternalFinishProcess(job, succeeded);
						},
						error =>
						{
							Debug.LogError(error);
							InternalFinishProcess(job, false);
						}
					);
				},
				exception =>
				{
					Debug.LogException(exception);
					InternalFinishProcess(job, false);
				}
			);
		}

		private static void InternalFinishProcess(BuildJob job, bool succeeded)
		{
			try
			{
				IsProcessing = false;

				if (succeeded)
					job.CurrentProjectStatus.IsSucceeded = true;
				else
					job.CurrentProjectStatus.IsFailed = true;

				RemoteBuilder.SaveBuildResponseFile(job);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);

				// Well, it won't count as succeeded if we can't finalize the process.
				job.CurrentProjectStatus.IsSucceeded = false;
				job.CurrentProjectStatus.IsFailed = true;
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
