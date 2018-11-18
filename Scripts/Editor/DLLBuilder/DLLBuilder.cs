using System;
using System.Collections;
using System.Linq;
using Extenity.ConsistencyToolbox;
using Extenity.ParallelToolbox.Editor;
using UnityEditor;
using UnityEngine.Events;

namespace Extenity.DLLBuilder
{

	public enum StatusMessageType
	{
		Normal,
		Warning,
		Error,
	}

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

		public static void StartProcess(BuildTriggerSource triggerSource, BuildJob job = null, bool buildOnlyRemote = false)
		{
			InternalStartProcess(triggerSource, job, buildOnlyRemote).StartCoroutineInEditorUpdate();
		}

		private static IEnumerator InternalStartProcess(BuildTriggerSource triggerSource, BuildJob job, bool buildOnlyRemote)
		{
			if (IsProcessing)
				throw new Exception("A process was already started.");
			IsProcessing = true;

			// Create new job if not specified an existing one.
			if (job == null)
			{
				var newJob = new BuildJob
				{
					JobID = Guid.NewGuid()
				};
				var newJobStatus = newJob.AddCurrentProjectToChain();
				newJobStatus.IsCurrentlyProcessedProject = true;
				job = newJob;
			}

			var jobStatus = job.CurrentlyProcessedProjectStatus;
			if (jobStatus == null)
				throw new Exception("Internal error! Currently processed project status is not set.");

			// Don't continue if loading is in progress.
			if (EditorApplication.isUpdating || EditorApplication.isCompiling)
			{
				LogAndUpdateStatus("Unity was processing assets at the time the builder started.", StatusMessageType.Error);
				InternalFinishProcess(job, jobStatus, false);
				yield break;
			}

			DLLBuilderConfiguration builderConfiguration = null;

			try
			{
				if (!jobStatus.IsStarted)
				{
					LogAndUpdateStatus(Constants.DLLBuilderName + " started to build all DLLs. Job ID: " + job.JobID);
					jobStatus.IsStarted = true;

					if (jobStatus.BuildTriggerSource != BuildTriggerSource.Unspecified)
						throw new Exception($"Build trigger source was already specified as '{jobStatus.BuildTriggerSource}' where it was going to be set as '{triggerSource}'.");
					jobStatus.BuildTriggerSource = triggerSource;
				}
				else
				{
					// Means we are in the middle of build process. That is we are continuing after a recompilation.
					LogAndUpdateStatus(Constants.DLLBuilderName + " continuing to build all DLLs. Job ID: " + job.JobID);
				}
			}
			catch (Exception exception)
			{
				LogAndUpdateStatus(exception.Message, StatusMessageType.Error);
				InternalFinishProcess(job, jobStatus, false);
				yield break;
			}

			// Load configuration
			{
				if (EditorApplication.isCompiling)
				{
					LogAndUpdateStatus("Unity was compiling just before the builder needed configuration asset.", StatusMessageType.Error);
					InternalFinishProcess(job, jobStatus, false);
					yield break;
				}

				builderConfiguration = DLLBuilderConfiguration.Instance;
				UpdateStatus("Waiting to get Builder Configuration.");

				// Give it a minute to process internal things to lower the chance of getting isCompiling or isUpdating. Better safe than sorry.
				yield return null;
				yield return null;
				yield return null;

				//// Don't continue until we are sure no loading is in progress.
				//// This may be needed to load builder configuration properly.
				//if (EditorApplication.isCompiling) Can't figure out how not to get it not compiling. That's probably a Unity glitch.
				//{
				//	LogErrorAndUpdateStatus("Unity was compiling at the time the builder needed configuration asset.");
				//	InternalFinishProcess(job, jobStatus, false);
				//	yield break;
				//}
				while (EditorApplication.isUpdating)
				{
					LogAndUpdateStatus("Waiting for asset refresh");
					yield return null;
				}
			}

			UpdateStatus("Initializing remote project status list.");
			var remoteProjectPaths = builderConfiguration.EnabledAndIgnoreFilteredRemoteBuilderConfigurations.Select(item => DLLBuilderConfiguration.InsertEnvironmentVariables(item.ProjectPath)).ToArray();
			jobStatus.SetRemoteProjectStatusList(remoteProjectPaths);

			UpdateStatus("Checking job consistency.");
			if (job.CheckConsistencyAndLog().Count > 0)
			{
				InternalFinishProcess(job, jobStatus, false);
				yield break;
			}

			Repaint();

			RemoteBuilder.CreateBuildRequestsOfRemoteProjects(builderConfiguration, job, jobStatus,
				() =>
				{
					Collector.GatherDependenciesFromAll(builderConfiguration, job,
						() =>
						{
							if (buildOnlyRemote)
							{
								InternalFinishProcess(job, jobStatus, true);
								return;
							}

							Cleaner.ClearAllOutputDLLs(builderConfiguration,
								() =>
								{
									Repaint();

									Compiler.CompileAllDLLs(builderConfiguration,
										() =>
										{
											var succeeded = false;
											try
											{
												Repaint();
												Packer.PackAll();
												Repaint();
												Distributer.DistributeToAll(builderConfiguration);

												LogAndUpdateStatus(Constants.DLLBuilderName + " successfully built all DLLs.");
												succeeded = true;
											}
											catch (Exception exception)
											{
												LogAndUpdateStatus("Post-build failed. Reason: " + exception.Message, StatusMessageType.Error);
											}
											InternalFinishProcess(job, jobStatus, succeeded);
										},
										error =>
										{
											LogAndUpdateStatus(error, StatusMessageType.Error);
											InternalFinishProcess(job, jobStatus, false);
										}
									);
								},
								exception =>
								{
									LogAndUpdateStatus(exception.Message, StatusMessageType.Error);
									InternalFinishProcess(job, jobStatus, false);
								}
							);
						},
						error =>
						{
							LogAndUpdateStatus(error, StatusMessageType.Error);
							InternalFinishProcess(job, jobStatus, false);
						}
					);
				},
				error =>
				{
					LogAndUpdateStatus(error, StatusMessageType.Error);
					InternalFinishProcess(job, jobStatus, false);
				}
			);
		}

		private static void InternalFinishProcess(BuildJob job, BuildJobStatus jobStatus, bool succeeded)
		{
			try
			{
				IsProcessing = false;

				UpdateStatus($"Finishing process {(succeeded ? "successfully" : "with errors")}.");

				BuildJob.DeleteAssemblyReloadSurvivalFile();

				if (succeeded)
					jobStatus.IsSucceeded = true;
				else
					jobStatus.IsFailed = true;

				RemoteBuilder.SaveBuildResponseFile(job);

				if (succeeded)
					LogAndUpdateStatus("Process finished successfully.");
				else
					LogAndUpdateStatus("Process finished with errors. Check previous console logs for more information.", StatusMessageType.Error);
			}
			catch (Exception exception)
			{
				Log.Exception(exception);

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

		#region UI Status

		public class StatusMessage
		{
			public StatusMessageType Type;
			public string Message;

			public StatusMessage Clone()
			{
				return new StatusMessage
				{
					Type = Type,
					Message = Message
				};
			}
		}

		public static StatusMessage CurrentStatus { get; private set; }
		public static readonly UnityEvent OnStatusChanged = new UnityEvent();

		public static void UpdateStatus(string text, StatusMessageType type = StatusMessageType.Normal)
		{
			if (CurrentStatus == null)
				CurrentStatus = new StatusMessage();

			CurrentStatus.Type = type;
			CurrentStatus.Message = text;
			OnStatusChanged.Invoke();
		}

		public static void LogAndUpdateStatus(string text, StatusMessageType type = StatusMessageType.Normal)
		{
			switch (type)
			{
				case StatusMessageType.Normal: Log.Info(text); break;
				case StatusMessageType.Warning: Log.Warning(text); break;
				case StatusMessageType.Error: Log.Error(text); break;
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
			UpdateStatus(text, type);
		}

		#endregion

		#region Reload Assemblies

		public static IEnumerator ReloadAssemblies(BuildJob job, Action onSucceeded)
		{
			LogAndUpdateStatus("Refreshing asset database");

			job.SaveBeforeAssemblyReload();

			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
			while (EditorApplication.isUpdating || EditorApplication.isCompiling)
				yield return null;

			LogAndUpdateStatus("Continuing after asset database refresh");

			// It's either we call onSucceeded or we lose control on assembly reload. In the latter case BuildJob.ContinueAfterRecompilation will handle the rest.
			if (onSucceeded != null)
				onSucceeded();
			//EditorApplication.delayCall += () => onSucceeded();
			//EditorApplicationTools.GuaranteeNextUpdateCall();
		}

		#endregion
	}

}
