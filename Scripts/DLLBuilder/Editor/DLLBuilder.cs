using System;
using System.Collections;
using System.Linq;
using Extenity.ConsistencyToolbox;
using Extenity.ParallelToolbox.Editor;
using UnityEditor;
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
			InternalStartProcess(job, triggerSource).StartCoroutineInEditorUpdate();
		}

		private static IEnumerator InternalStartProcess(BuildJob job, BuildTriggerSource triggerSource)
		{
			if (IsProcessing)
				throw new Exception("A process was already started.");
			IsProcessing = true;

			var jobStatus = job.CurrentlyProcessedProjectStatus;
			if (jobStatus == null)
				throw new Exception("Internal error! Currently processed project status is not set.");

			// Don't continue until we are sure no loading is in progress.
			while (EditorApplication.isUpdating || EditorApplication.isCompiling)
				yield return null;

			DLLBuilderConfiguration builderConfiguration = null;

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
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				InternalFinishProcess(job, jobStatus, false);
				yield break;
			}

			// Load configuration
			{
				builderConfiguration = DLLBuilderConfiguration.Instance;
				UpdateStatus("Waiting to get Builder Configuration.");

				// Don't continue until we are sure no loading is in progress.
				// This may be needed to load builder configuration properly.
				while (EditorApplication.isUpdating || EditorApplication.isCompiling)
					yield return null;
			}

			UpdateStatus("Initializing remote project status list.");
			var remoteProjectPaths = builderConfiguration.EnabledAndIgnoreFilteredRemoteBuilderConfigurations.Select(item => item.ProjectPath).ToArray();
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

										Debug.Log(Constants.DLLBuilderName + " successfully built all DLLs.");
										succeeded = true;
									}
									catch (Exception exception)
									{
										DLLBuilder.LogErrorAndUpdateStatus("Post-build failed. Reason: " + exception.Message);
									}
									InternalFinishProcess(job, jobStatus, succeeded);
								},
								error =>
								{
									DLLBuilder.LogErrorAndUpdateStatus(error);
									InternalFinishProcess(job, jobStatus, false);
								}
							);
						},
						exception =>
						{
							DLLBuilder.LogErrorAndUpdateStatus(exception.Message);
							InternalFinishProcess(job, jobStatus, false);
						}
					);
				},
				error =>
				{
					DLLBuilder.LogErrorAndUpdateStatus(error);
					InternalFinishProcess(job, jobStatus, false);
				}
			);
		}

		private static void InternalFinishProcess(BuildJob job, BuildJobStatus jobStatus, bool succeeded)
		{
			try
			{
				IsProcessing = false;

				UpdateStatus("Finishing process {0}.", succeeded ? "successfully" : "with errors");

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

		#region UI Status

		public enum StatusMessageType
		{
			Normal,
			Warning,
			Error,
		}

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

		public static void UpdateWarningStatus(string format, params object[] args)
		{
			var text = string.Format(format, args);
			UpdateStatus(text, StatusMessageType.Warning);
		}

		public static void UpdateErrorStatus(string format, params object[] args)
		{
			var text = string.Format(format, args);
			UpdateStatus(text, StatusMessageType.Error);
		}

		public static void UpdateStatus(string format, params object[] args)
		{
			var text = string.Format(format, args);
			UpdateStatus(text, StatusMessageType.Normal);
		}

		public static void UpdateWarningStatus(string text)
		{
			UpdateStatus(text, StatusMessageType.Warning);
		}

		public static void UpdateErrorStatus(string text)
		{
			UpdateStatus(text, StatusMessageType.Error);
		}

		public static void UpdateStatus(string text, StatusMessageType type = StatusMessageType.Normal)
		{
			if (CurrentStatus == null)
				CurrentStatus = new StatusMessage();

			CurrentStatus.Type = type;
			CurrentStatus.Message = text;
			OnStatusChanged.Invoke();
		}

		public static void LogWarningAndUpdateStatus(string format, params object[] args)
		{
			var text = string.Format(format, args);
			Debug.LogWarning(text);
			UpdateStatus(text, StatusMessageType.Warning);
		}

		public static void LogErrorAndUpdateStatus(string format, params object[] args)
		{
			var text = string.Format(format, args);
			Debug.LogError(text);
			UpdateStatus(text, StatusMessageType.Error);
		}

		public static void LogAndUpdateStatus(string format, params object[] args)
		{
			var text = string.Format(format, args);
			Debug.Log(text);
			UpdateStatus(text, StatusMessageType.Normal);
		}

		public static void LogWarningAndUpdateStatus(string text)
		{
			LogAndUpdateStatus(text, StatusMessageType.Warning);
		}

		public static void LogErrorAndUpdateStatus(string text)
		{
			LogAndUpdateStatus(text, StatusMessageType.Error);
		}

		public static void LogAndUpdateStatus(string text, StatusMessageType type = StatusMessageType.Normal)
		{
			switch (type)
			{
				case StatusMessageType.Normal: Debug.Log(text); break;
				case StatusMessageType.Warning: Debug.LogWarning(text); break;
				case StatusMessageType.Error: Debug.LogError(text); break;
				default:
					throw new ArgumentOutOfRangeException("type", type, null);
			}
			UpdateStatus(text, type);
		}

		#endregion
	}

}
