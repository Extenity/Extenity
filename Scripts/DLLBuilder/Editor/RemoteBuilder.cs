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

				var request = JsonUtility.FromJson<BuildRequest>(content);
				request.CheckConsistencyAndThrow();

				Debug.Log("Remote DLL build request received. " + request);
				DLLBuilder.StartProcess(request);
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

		public static void CreateBuildRequestFileForProject(BuildRequest request, string targetProjectPath)
		{
			if (string.IsNullOrEmpty(targetProjectPath))
				throw new ArgumentNullException("targetProjectPath");
			request.CheckConsistencyAndThrow();
			// Make sure target project path aims at a Unity project directory.
			if (!DirectoryTools.IsUnityProjectPath(targetProjectPath))
				throw new ArgumentException(string.Format("Target project path '{0}' is not a Unity project path.", targetProjectPath), "targetProjectPath");

			// This is not needed since it's done by DLLBuilder.StartProcess
			//request.AddCurrentProjectToRequesterProjectChain();

			var filePath = Path.Combine(targetProjectPath, Constants.RemoteBuilder.RequestFilePath);
			DirectoryTools.CreateFromFilePath(filePath);

			var json = JsonUtility.ToJson(request, true);
			File.WriteAllText(filePath, json);
		}

		#endregion

		#region Create Build Requests Of Remote Projects

		public static void CreateBuildRequestsOfRemoteProjects(BuildJob job, Action onSucceeded, Action<string> onFailed)
		{
			if (job.IsRemoteBuildsCompleted)
			{
				if (onSucceeded != null)
					onSucceeded();
				return;
			}

			Debug.Log("--------- Building all remote projects");

			InternalCreateBuildRequestsOfRemoteProjects(job, onSucceeded, onFailed).StartCoroutineInEditorUpdate();
		}

		private static IEnumerator InternalCreateBuildRequestsOfRemoteProjects(BuildJob job, Action onSucceeded, Action<string> onFailed)
		{
			var configurations = DLLBuilderConfiguration.Instance.EnabledRemoteBuilderConfigurations;
			if (configurations.IsNullOrEmpty())
			{
				Debug.Log("Skipping remote builder. Nothing to pack.");
				if (onSucceeded != null)
					onSucceeded();
				yield break;
			}

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
				throw new NotImplementedException();
			}

			// Recompile this project. Because we probably got new DLLs coming out of remote builds.
			{
				job.SaveToFile();
				AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

				// It's either we call onSucceeded or we lose control on assembly reload. In the latter case BuildJob.ContinueAfterRecompilation will handle the rest.
				if (onSucceeded != null)
					onSucceeded();
			}
		}

		#endregion
	}

}
