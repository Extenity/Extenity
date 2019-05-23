using System;
using System.Collections;
using System.IO;
using System.Linq;
using Extenity.FileSystemToolbox;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace Extenity.BuildMachine.Editor
{

	public static class BuildJobRunner
	{
		#region Running Job

		public static BuildJob RunningJob;
		public static bool IsRunning => RunningJob != null;

		#endregion

		#region Start

		internal static void Start(BuildJob job)
		{
			if (IsRunning)
			{
				throw new Exception("Tried to start a build job while there is already a running one.");
			}
			RunningJob = job;

			EditorCoroutineUtility.StartCoroutineOwnerless(Run());
		}

		private static void Continue(BuildJob job)
		{
			if (IsRunning)
			{
				throw new Exception("Tried to continue a build job while there is already a running one.");
			}
			RunningJob = job;

			EditorCoroutineUtility.StartCoroutineOwnerless(Run());
		}

		#endregion

		#region Run

		private static IEnumerator Run()
		{
			if (RunningJob.IsJustCreated)
			{
				Log.Info($"Build '{RunningJob.Plan.Name}' started.");
				Debug.Assert(RunningJob.CurrentPhase == -1);
				Debug.Assert(RunningJob.CurrentBuilder == -1);
				Debug.Assert(string.IsNullOrEmpty(RunningJob.LastProcessedStep));
				RunningJob.CurrentPhase = 0;
				RunningJob.CurrentBuilder = 0;
				RunningJob.LastProcessedStep = "";
			}
			else
			{
				Log.Info($"Build '{RunningJob.Plan.Name}' continuing at phase '{RunningJob.CurrentPhase}', builder '{RunningJob.CurrentBuilder}', previous step '{RunningJob.LastProcessedStep}'.");
			}

			var currentPhase = RunningJob.Plan.BuildPhases[RunningJob.CurrentPhase];
			var currentBuilder = RunningJob.Builders[RunningJob.CurrentBuilder];
			var allStepsOfCurrentPhase = currentBuilder.Info.Steps.Where(step => currentPhase.IncludedSteps.Contains(step.Type)).ToList();



			// TODO:
			yield break;
			/*
			RunningJob.CurrentStep++;
			if (RunningJob.CurrentStep)
			{


			}

			RunningJob.CurrentPhase++;
			if (RunningJob.Plan.BuildPhases.Length <= RunningJob.CurrentPhase)
			{
				Log.Info($"Build '{RunningJob.Plan.Name}' succeeded.");
			}
			else
			{

			}

			//yield return DoBuild();
			CheckBeforeStep()
			Log.Error($"Build '{RunningJob.Plan.Name}' failed.");
			*/
		}

		private static bool CheckBeforeStep()
		{
			var haltExecution = false;

			// At this point, there should be no ongoing compilations.
			// Build system does not allow any code that triggers
			// an assembly reload in Build Step. Otherwise execution
			// gets really messy.
			if (EditorApplication.isCompiling)
			{
				throw new Exception("Compilation is not allowed in the middle of build.");
			}

			// Save the unsaved assets before making any moves.
			AssetDatabase.SaveAssets();

			// Make sure everything is imported.
			{
				AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

				// And wait for scripts to compile.
				if (EditorApplication.isCompiling)
				{
					haltExecution = true;
					SaveRunningJobToFile(RunningJob);
				}
			}

			return haltExecution;
		}

		#endregion

		#region Check for running job existence after assembly reload

		internal static void ContinueFromRunningJobAfterAssemblyReload()
		{
			var job = LoadRunningJobFromFile();
			if (job != null)
			{
				Continue(job);
			}
		}

		#endregion

		#region Assembly reload survival of running job

		private static void SaveRunningJobToFile(BuildJob job)
		{
			Log.Info("Saving running job for assembly reload survival.");

			if (job == null)
				throw new ArgumentNullException(nameof(job));

			if (IsRunningJobFileExists())
			{
				throw new Exception("Running job survival file is expected not to exist, yet it does.");
			}

			var content = job.SerializeToJson();

			AssetDatabase.ReleaseCachedFileHandles(); // Make Unity release the files to prevent any IO errors.

			DirectoryTools.CreateFromFilePath(BuildMachineConstants.RunningJobSurvivalLocation);
			File.WriteAllText(BuildMachineConstants.RunningJobSurvivalLocation, content);
		}

		private static BuildJob LoadRunningJobFromFile()
		{
			if (IsRunningJobFileExists())
			{
				Log.Info("Loading running job after assembly reload.");

				var content = File.ReadAllText(BuildMachineConstants.RunningJobSurvivalLocation);
				DeleteRunningJobFile();
				if (string.IsNullOrWhiteSpace(content))
				{
					throw new Exception("Running job assembly reload survival file was empty.");
				}

				var job = BuildJob.DeserializeFromJson(content);
				if (job == null)
				{
					Log.Error("Deserialization failed. Running job content was:\n" + content);
					throw new Exception("Failed to deserialize running job.");
				}
				return job;
			}
			return null;
		}

		private static bool IsRunningJobFileExists()
		{
			return File.Exists(BuildMachineConstants.RunningJobSurvivalLocation);
		}

		private static void DeleteRunningJobFile()
		{
			AssetDatabase.ReleaseCachedFileHandles(); // Make Unity release the files to prevent any IO errors.

			try
			{
				File.Delete(BuildMachineConstants.RunningJobSurvivalLocation);
			}
			catch (DirectoryNotFoundException)
			{
				// Ignore
			}
		}

		#endregion
	}

}
