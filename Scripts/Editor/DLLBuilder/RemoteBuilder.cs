using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Timers;
using UnityEditor;
using System.IO;
using Extenity.ConsistencyToolbox;
using Extenity.DataToolbox;
using Extenity.ParallelToolbox.Editor;
using Newtonsoft.Json;

namespace Extenity.DLLBuilder
{

	public static class RemoteBuilder
	{
		#region Periodic Compile Request Checker

		[InitializeOnLoadMethod]
		private static void InitializePeriodicBuildRequestChecker()
		{
			if (EditorApplication.isCompiling)
				return; // Let it pass. We will start the operation after Unity finishes recompiling assemblies. This may be an unnecessary safety belt. But let's keep it here anyway.

			var timer = new Timer(Constants.RemoteBuilder.RequestCheckerInterval * 1000);
			timer.Elapsed += OnTimeToCheckRequests;
			timer.AutoReset = true;
			timer.Enabled = true;
		}

		private static void OnTimeToCheckRequests(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			CheckBuildRequestFromFile();
		}

		#endregion

		#region Receive Build Request

		private static void CheckBuildRequestFromFile()
		{
			try
			{
				var content = File.ReadAllText(Constants.RemoteBuilder.RequestFilePath);
				DeleteBuildRequestFile();
				Log.Info("## file content: " + content);

				var job = JsonConvert.DeserializeObject<BuildJob>(content);
				//job.CheckConsistencyAndThrow(); This will be done in DLLBuilder.StartProcess. Otherwise we can't be able to produce response file.

				DLLBuilder.LogAndUpdateStatus("Remote DLL build request received. " + job);
				DLLBuilder.StartProcess(BuildTriggerSource.RemoteBuildRequest, job);

				//// Continue in main thread
				//ContinuationManager.Run(
				//	() =>
				//	{
				//		try
				//		{
				//			Log.Info("Remote DLL build request received. " + job);
				//			DLLBuilder.StartProcess(job, BuildTriggerSource.RemoteBuildRequest);
				//		}
				//		catch (Exception exception)
				//		{
				//			Debug.LogException(exception);
				//		}
				//	}
				//);
			}
			catch (DirectoryNotFoundException)
			{
				// ignored
			}
			catch (FileNotFoundException)
			{
				// ignored
			}
			catch (Exception exception)
			{
				DLLBuilder.LogAndUpdateStatus($"Failed to process {Constants.DLLBuilderName} remote request file. Reason: " + exception, StatusMessageType.Error);
				// Delete request file so that it won't bother console logs again.
				DeleteBuildRequestFile();
			}
		}

		private static void DeleteBuildRequestFile()
		{
			try
			{
				FileTools.DeleteFileEvenIfReadOnly(Constants.RemoteBuilder.RequestFilePath);
			}
			catch
			{
				// ignored
			}
		}

		#endregion

		#region Create Build Request

		public static void CreateBuildRequestFileForProject(BuildJob job, string targetProjectPath)
		{
			DLLBuilder.UpdateStatus($"Creating build request file for project '{targetProjectPath}'.");

			if (string.IsNullOrEmpty(targetProjectPath))
				throw new ArgumentNullException(nameof(targetProjectPath));
			job.CheckConsistencyAndThrow();
			// Make sure target project path aims at a Unity project directory.
			if (!DirectoryTools.IsUnityProjectPath(targetProjectPath))
				throw new ArgumentException($"Target project path '{targetProjectPath}' is not a Unity project path.", nameof(targetProjectPath));

			// Remove already existing response file with the same job ID if exists.
			var remoteProjectResponseFilePath = Path.Combine(targetProjectPath, string.Format(Constants.RemoteBuilder.ResponseFilePath, job.JobID));
			DeleteRemoteProjectBuildResponseFile(remoteProjectResponseFilePath);

			var filePath = Path.Combine(targetProjectPath, Constants.RemoteBuilder.RequestFilePath);
			DirectoryTools.CreateFromFilePath(filePath);

			var json = JsonConvert.SerializeObject(job, Formatting.Indented);
			File.WriteAllText(filePath, json);
		}

		#endregion

		#region Create Build Requests Of Remote Projects

		private static double LastResponseCheckTime;
		private static readonly double ResponseCheckInterval = 1000;

		public static void CreateBuildRequestsOfRemoteProjects(DLLBuilderConfiguration builderConfiguration, BuildJob job, BuildJobStatus thisProjectStatus, Action onSucceeded, Action<string> onFailed)
		{
			if (!thisProjectStatus.IsCurrentlyProcessedProject)
			{
				if (onFailed != null)
					onFailed("Internal error! Trying to build remote projects for a project that was not set as current.");
			}

			if (thisProjectStatus.IsRemoteBuildsCompleted)
			{
				DLLBuilder.LogAndUpdateStatus("Skipping remote builder. Already completed.");
				//thisProjectStatus.IsRemoteBuildsCompleted = true; Already true. But we will keep it here to remind that we need to set this to true when successfully finishing this process.
				if (onSucceeded != null)
					onSucceeded();
				return;
			}

			var configurations = builderConfiguration.EnabledRemoteBuilderConfigurations;
			if (configurations.IsNullOrEmpty())
			{
				DLLBuilder.LogAndUpdateStatus("Skipping remote builder. Nothing to pack.");
				thisProjectStatus.IsRemoteBuildsCompleted = true;
				if (onSucceeded != null)
					onSucceeded();
				return;
			}

			DLLBuilder.LogAndUpdateStatus("Building all remote projects");

			InternalCreateBuildRequestsOfRemoteProjects(configurations, job, thisProjectStatus, onSucceeded, onFailed).StartCoroutineInEditorUpdate();
		}

		private static IEnumerator InternalCreateBuildRequestsOfRemoteProjects(List<RemoteBuilderConfiguration> configurations, BuildJob job, BuildJobStatus thisProjectStatus, Action onSucceeded, Action<string> onFailed)
		{
			for (var i = 0; i < configurations.Count; i++)
			{
				var configuration = configurations[i];
				DLLBuilder.LogAndUpdateStatus($"Building remote project at path '{configuration.ProjectPath}'");

				// Check consistency first.
				{
					var errors = new List<ConsistencyError>();
					configuration.CheckConsistency(ref errors);
					if (errors.Count > 0)
					{
						if (onFailed != null)
							onFailed("Failed to pack because of consistency errors:\n" + errors.Serialize('\n'));
						yield break;
					}
				}

				// Trigger a compilation on remote project and wait for it to finish
				var projectPathProcessed = DLLBuilderConfiguration.InsertEnvironmentVariables(configuration.ProjectPath);
				job.SetCurrentlyProcessedProject(thisProjectStatus.GetRemoteProject(projectPathProcessed));
				CreateBuildRequestFileForProject(job, projectPathProcessed);

				var checkCount = 0;
				var remoteProjectResponseFilePath = Path.Combine(projectPathProcessed, string.Format(Constants.RemoteBuilder.ResponseFilePath, job.JobID));
				while (true)
				{
					var now = DateTime.UtcNow.TotalMilliseconds();
					if (LastResponseCheckTime + ResponseCheckInterval > now)
					{
						yield return null;
						continue;
					}
					LastResponseCheckTime = now;

					checkCount++;

					DLLBuilder.UpdateStatus($"Waiting for remote project response ({checkCount})");
					var responseJob = LoadRemoteProjectBuildResponseFile(remoteProjectResponseFilePath);
					if (responseJob != null)
					{
						DLLBuilder.LogAndUpdateStatus("Processing remote project response");
						var responseStatus = responseJob.CurrentlyProcessedProjectStatus;
						var result = job.UpdateCurrentlyProcessedProjectStatus(responseStatus);
						if (!result)
						{
							if (onFailed != null)
								onFailed("Internal error! Currently processed remote project status could not be updated.");
							yield break;
						}
						job.UnsetCurrentlyProcessedProject();
						if (responseStatus.IsFailed)
						{
							if (onFailed != null)
								onFailed("Remote project build failed. Check console logs for more information.");
							yield break;
						}
						break;
					}
					yield return null;
				}
			}

			DLLBuilder.LogAndUpdateStatus("Marking remote build completion in project status");
			job.SetCurrentlyProcessedProject(thisProjectStatus);
			thisProjectStatus.IsRemoteBuildsCompleted = true;

			// Recompile this project. Because we probably got new DLLs coming out of remote builds.
			DLLBuilder.ReloadAssemblies(job, onSucceeded).StartCoroutineInEditorUpdate();
		}

		#endregion

		#region Check Remote Project Build Response

		public static BuildJob LoadRemoteProjectBuildResponseFile(string remoteProjectResponseFilePath)
		{
			try
			{
				var json = File.ReadAllText(remoteProjectResponseFilePath);
				Log.Info("## Remote project response: " + json);
				return JsonConvert.DeserializeObject<BuildJob>(json);
			}
			catch (FileNotFoundException)
			{
				// ignore
			}
			catch (DirectoryNotFoundException)
			{
				// ignore
			}
			catch
			{
				throw;
			}
			return null;
		}

		public static void DeleteRemoteProjectBuildResponseFile(string remoteProjectResponseFilePath)
		{
			try
			{
				if (File.Exists(remoteProjectResponseFilePath))
				{
					FileTools.DeleteFileEvenIfReadOnly(remoteProjectResponseFilePath);
					DLLBuilder.LogAndUpdateStatus($"Deleted remote project build response file '{remoteProjectResponseFilePath}'.");
				}
			}
			catch (FileNotFoundException)
			{
				// ignore
			}
			catch (DirectoryNotFoundException)
			{
				// ignore
			}
			catch
			{
				throw;
			}
		}

		#endregion

		#region Save Build Response

		public static void SaveBuildResponseFile(BuildJob job)
		{
			try
			{
				DLLBuilder.UpdateStatus("Saving build response file");

				var json = JsonConvert.SerializeObject(job, Formatting.Indented);
				var filePath = string.Format(Constants.RemoteBuilder.ResponseFilePath, job.JobID.ToString());
				DirectoryTools.CreateFromFilePath(filePath);
				File.WriteAllText(filePath, json);
			}
			catch (Exception exception)
			{
				throw new Exception("Failed to save " + Constants.DLLBuilderName + " remote build response file.", exception);
			}
		}

		#endregion
	}

}
