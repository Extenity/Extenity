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
				Debug.Log("## file content: " + content);

				var job = JsonConvert.DeserializeObject<BuildJob>(content);
				//job.CheckConsistencyAndThrow(); This will be done in DLLBuilder.StartProcess. Otherwise we can't be able to produce response file.

				Debug.Log("Remote DLL build request received. " + job);
				DLLBuilder.StartProcess(job, BuildTriggerSource.RemoteBuildRequest);

				//// Continue in main thread
				//ContinuationManager.Run(
				//	() =>
				//	{
				//		try
				//		{
				//			Debug.Log("Remote DLL build request received. " + job);
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
				Debug.LogError("Failed to process " + Constants.DLLBuilderName + " remote request file. Reason: " + exception);
				// Delete request file so that it won't bother console logs again.
				DeleteBuildRequestFile();
			}
		}

		private static void DeleteBuildRequestFile()
		{
			try
			{
				File.Delete(Constants.RemoteBuilder.RequestFilePath);
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
			if (string.IsNullOrEmpty(targetProjectPath))
				throw new ArgumentNullException("targetProjectPath");
			job.CheckConsistencyAndThrow();
			// Make sure target project path aims at a Unity project directory.
			if (!DirectoryTools.IsUnityProjectPath(targetProjectPath))
				throw new ArgumentException(string.Format("Target project path '{0}' is not a Unity project path.", targetProjectPath), "targetProjectPath");

			// This is not needed since it's done by DLLBuilder.StartProcess
			//request.AddCurrentProjectToRequesterProjectChain();

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
				Debug.LogError("Internal error! Trying to build remote projects for a project that was not set as current.");
				if (onFailed != null)
					onFailed("Internal error! Trying to build remote projects for a project that was not set as current.");
			}

			if (thisProjectStatus.IsRemoteBuildsCompleted)
			{
				if (onSucceeded != null)
					onSucceeded();
				return;
			}

			var configurations = builderConfiguration.EnabledRemoteBuilderConfigurations;
			if (configurations.IsNullOrEmpty())
			{
				Debug.Log("Skipping remote builder. Nothing to pack.");
				if (onSucceeded != null)
					onSucceeded();
				return;
			}

			Debug.Log("--------- Building all remote projects");

			InternalCreateBuildRequestsOfRemoteProjects(configurations, job, thisProjectStatus, onSucceeded, onFailed).StartCoroutineInTimer();
		}

		private static IEnumerator InternalCreateBuildRequestsOfRemoteProjects(List<RemoteBuilderConfiguration> configurations, BuildJob job, BuildJobStatus thisProjectStatus, Action onSucceeded, Action<string> onFailed)
		{
			for (var i = 0; i < configurations.Count; i++)
			{
				var configuration = configurations[i];
				Debug.LogFormat("Building remote project at path '{0}'", configuration.ProjectPath);

				// Check consistency first.
				{
					var errors = new List<ConsistencyError>();
					configuration.CheckConsistency(ref errors);
					if (errors.Count > 0)
					{
						Debug.LogError("Failed to pack because of consistency errors:\n" + errors.Serialize('\n'));
						if (onFailed != null)
							onFailed("There were consistency errors. Check console for more information.");
						yield break;
					}
				}

				// Trigger a compilation on remote project and wait for it to finish
				job.SetCurrentlyProcessedProject(thisProjectStatus.GetRemoteProject(configuration.ProjectPath));
				CreateBuildRequestFileForProject(job, configuration.ProjectPath);

				var remoteProjectResponseFilePath = Path.Combine(configuration.ProjectPath, string.Format(Constants.RemoteBuilder.ResponseFilePath, job.JobID));
				while (true)
				{
					var now = DateTime.UtcNow.TotalMilliseconds();
					if (LastResponseCheckTime + ResponseCheckInterval > now)
					{
						yield return null;
						continue;
					}
					LastResponseCheckTime = now;

					Debug.Log("### checking remote project: " + remoteProjectResponseFilePath);
					var responseJob = CheckRemoteProjectBuildResponseFile(remoteProjectResponseFilePath);
					if (responseJob != null)
					{
						var responseStatus = responseJob.CurrentlyProcessedProjectStatus;
						var result = job.UpdateCurrentlyProcessedProjectStatus(responseStatus);
						if (!result)
						{
							Debug.LogError("Internal error! Currently processed remote project status could not be updated.");
							if (onFailed != null)
								onFailed("Internal error! Currently processed remote project status could not be updated.");
							yield break;
						}
						job.UnsetCurrentlyProcessedProject();
						if (responseStatus.IsFailed)
						{
							Debug.LogError("Remote project build failed. Check console logs for more information.");
							if (onFailed != null)
								onFailed("Remote project build failed. Check console logs for more information.");
							yield break;
						}
						break;
					}
					yield return null;
				}
			}

			job.SetCurrentlyProcessedProject(thisProjectStatus);
			thisProjectStatus.IsRemoteBuildsCompleted = true;

			// Recompile this project. Because we probably got new DLLs coming out of remote builds.
			{
				job.SaveBeforeAssemblyReload();
				AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

				// It's either we call onSucceeded or we lose control on assembly reload. In the latter case BuildJob.ContinueAfterRecompilation will handle the rest.
				if (onSucceeded != null)
					onSucceeded();
			}
		}

		#endregion

		#region Check Remote Project Build Response

		public static BuildJob CheckRemoteProjectBuildResponseFile(string remoteProjectResponseFilePath)
		{
			try
			{
				var json = File.ReadAllText(remoteProjectResponseFilePath);
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

		#endregion

		#region Save Build Response

		public static void SaveBuildResponseFile(BuildJob job)
		{
			try
			{
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
