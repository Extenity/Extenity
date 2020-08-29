using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Extenity.ApplicationToolbox.Editor;
using Extenity.BuildToolbox.Editor;
using Extenity.DataToolbox;
using Extenity.FileSystemToolbox;
using Extenity.ParallelToolbox.Editor;
using Extenity.SceneManagementToolbox.Editor;
using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace Extenity.BuildMachine.Editor
{

	public static class BuildJobRunner
	{
		#region Running Job

		private static BuildJob _RunningJob;
		public static BuildJob RunningJob
		{
			get
			{
				if (_RunningJob == null)
					throw new Exception($"Tried to get {nameof(RunningJob)} while it was not set.");
				return _RunningJob;
			}
		}
		public static bool IsRunning => _RunningJob != null;

		private static void SetRunningJob(BuildJob job)
		{
			if (_RunningJob != null)
			{
				throw new Exception($"Tried to set {nameof(RunningJob)} while there was already an existing one.");
			}
			Log.Info($"Setting the {nameof(RunningJob)}");
			_RunningJob = job;
		}

		private static void UnsetRunningJob()
		{
			Log.Info($"Unsetting the {nameof(RunningJob)}. Previously was '{(_RunningJob != null ? "set" : "not set")}'.");
			_RunningJob = null;
		}

		#endregion

		#region Start

		internal static void Start(BuildJob job)
		{
			var jobPlanName = job.NameSafe();
			if (IsRunning)
			{
				throw new Exception($"Tried to start build job '{jobPlanName}' while there is already a running one.");
			}

			Log.Info($"Build '{jobPlanName}' started.");

			ChecksBeforeStartOrContinue("start");

			// Check state consistency
			{
				Debug.Assert(job.OverallState == BuildJobOverallState.JobInitialized);
				//Debug.Assert(job.PhaseState == BuildJobPhaseState.Unknown);
				Debug.Assert(job.StepState == BuildJobStepState.Unknown);
				Debug.Assert(job.Result == BuildJobResult.Incomplete);
				Debug.Assert(job.CurrentPhase == -1);
				Debug.Assert(job.CurrentBuilder == -1);
				Debug.Assert(!job.IsPreviousBuildStepAssigned);
				Debug.Assert(!job.IsCurrentBuildStepAssigned);
				Debug.Assert(!job.IsPreviousFinalizationStepAssigned);
				Debug.Assert(!job.IsCurrentFinalizationStepAssigned);
			}

			job.StartTime = Now;
			job.BuildRunInitialization();
			job.CurrentPhase = 0;
			job.CurrentBuilder = 0;
			job.OverallState = BuildJobOverallState.JobRunning;

			SetRunningJob(job); // Set it just before the Run call so any exceptions above won't leave the reference behind.
			EditorCoroutineUtility.StartCoroutineOwnerless(Run(), CatchRunException);
		}

		private static void Continue(BuildJob job)
		{
			var jobPlanName = job.NameSafe();
			if (IsRunning)
			{
				throw new Exception($"Tried to continue build job '{jobPlanName}' while there is already a running one.");
			}

			Log.Info($"Continuing the build '{job.Name}'...\n" +
					 $"Builder '{job.ToStringCurrentBuilder()}' in Phase '{job.ToStringCurrentPhase()}'\n" +
					 $"Build Step '{job.CurrentBuildStep}' (Previously: {job.PreviousBuildStep})\n" +
					 $"Finalization Step '{job.CurrentFinalizationStep}' (Previously: {job.PreviousFinalizationStep})");

			ChecksBeforeStartOrContinue("continue");

			// Check state consistency
			{
				// See 11917631.
				// The only expected state is StepHalt when continuing after assembly reload.
				if (job.StepState != BuildJobStepState.StepHalt)
				{
					UnsetRunningJob();
					throw new Exception($"Build job '{jobPlanName}' was disrupted in the middle for some reason. It could happen if Editor crashes during build, if not happened for an unexpected reason. (Overall: '{job.OverallState}' Step: '{job.StepState}' Result: '{job.Result}')");
				}
				Debug.Assert(job.OverallState == BuildJobOverallState.JobRunning, $"Unexpected overall state '{job.OverallState}'.");
				//Debug.Assert(job.PhaseState == BuildJobPhaseState.PhaseRunning, $"Unexpected phase state '{job.PhaseState}'.");
			}

			SetRunningJob(job); // Set it just before the Run call so any exceptions above won't leave the reference behind.
			EditorCoroutineUtility.StartCoroutineOwnerless(Run(), CatchRunException);
		}

		private static void ChecksBeforeStartOrContinue(string description)
		{
			if (BuildTools.IsCompiling)
			{
				throw new Exception($"Tried to '{description}' a build job in the middle of an ongoing compilation.");
			}

			// Disable auto-refresh
			{
				EditorPreferencesTools.DisableAutoRefresh();
			}

			// Make console fullscreen
			if (!BuildTools.IsBatchMode)
			{
				EditorApplication.delayCall += () =>
				{
					try
					{
						BuildMachineLayout.LoadConsoleOnlyLayout();
						//EditorApplication.ExecuteMenuItem("Window/Console Pro 3"); // Open console if closed.
						//EditorWindowTools.GetEditorWindowByTitle(" Console Pro").MakeFullscreen(true);
					}
					catch
					{
						// Ignored
					}
				};
			}
		}

		#endregion

		#region Run

		private static IEnumerator Run()
		{
			// Quick access references. These will not ever change during the build run.
			// Do not add variables like 'currentPhase' here.
			var Job = RunningJob;

			RunningJob.LastHaltTime = default;

			// The assets should be saved and refreshed at the very beginning of compilation
			// or continuing the compilation after assembly reload.
			{
				var haltExecution = CheckBeforeStartingOrContinuing();
				if (haltExecution)
					yield break;
			}

			// Deselect any asset or object.
			Selection.activeObject = null;
			yield return null;

			// Close all scenes. Hopefully this will boost the build a bit.
			EditorSceneManagerTools.UnloadAllScenes(true);
			yield return null;

			while (IsRunning)
			{
				// Check before running the Step
				{
					var haltExecution = CheckBeforeStep();
					if (haltExecution)
						yield break;
					Job.StepState = BuildJobStepState.StepRunning;
					SaveRunningJobToFile();
				}

				// Run the Step
				{
					Job.ErrorReceivedInLastStep = "";
					// TODO: See 11422351
					//Application.logMessageReceivedThreaded += 
					yield return EditorCoroutineUtility.StartCoroutineOwnerless(RunStep(), CatchRunStepException);
					if (!string.IsNullOrEmpty(Job.ErrorReceivedInLastStep))
					{
						Job.ErrorReceivedInLastStep = "";
						Job.Finalizing = true;
						Job.SetResult(BuildJobResult.Failed);
						SaveRunningJobToFile();
					}

					// Don't do anything if the Build Run finishes.
					if (Job.OverallState == BuildJobOverallState.JobFinished)
					{
						yield break;
					}
				}

				// Finalize the Step
				{
					// Mark the current Step as previous Step and relieve the current Step.
					// Then immediately save the assembly Survival File so that we will know
					// the process was not cut in the middle of Step execution the next time
					// the survival file is loaded. See 11917631.
					Job.StepState = BuildJobStepState.StepFinished;
					if (Job.IsCurrentBuildStepAssigned)
					{
						Job.PreviousBuildStep = Job.CurrentBuildStep;
						Job.CurrentBuildStep = "";
					}
					if (Job.IsCurrentFinalizationStepAssigned)
					{
						Job.PreviousFinalizationStep = Job.CurrentFinalizationStep;
						Job.CurrentFinalizationStep = "";
					}
					SaveRunningJobToFile();
				}

				// Check after running the Step
				{
					var haltExecution = CheckAfterStep();
					if (haltExecution)
						yield break;
				}

				yield return null; // As a precaution, won't hurt to wait for one frame for all things to settle down.
			}
		}

		private static IEnumerator RunStep()
		{
			// Quick access references. These will not ever change during the build run.
			// Do not add variables like 'currentPhase' here.
			var Job = RunningJob;
			var Builders = RunningJob.Builders;
			var BuildPhases = RunningJob.Plan.BuildPhases;

			EditorApplicationTools.EnsureNotCompiling(false);

			// Figure out which should be next Step to be executed. If there is none left, Finalize the Build Run.
			var completed = false;
			try
			{
				if (!Job.Finalizing)
				{
					if (!Job.IsPreviousBuildStepAssigned)
					{
						// Current Builder is just getting started. Proceed to first Build Step.
						Job.CurrentBuildStep = GetFirstStep(false);
					}
					else
					{
						// Continuing from a Build Step of an ongoing Build Run. See if there is a next Build Step.
						var nextStep = GetNextStep(Job.PreviousBuildStep, false);
						if (!string.IsNullOrEmpty(nextStep))
						{
							// Proceed to next Build Step.
							Job.CurrentBuildStep = nextStep;
						}
						else
						{
							// Finished all Build Steps. Proceed to Finalization.
							Job.CurrentBuildStep = "";
							Job.PreviousBuildStep = "";
							Job.Finalizing = true;
						}
					}
				}

				// Note that 'Finalizing' may have changed above if the execution reaches the last Build Step.
				if (Job.Finalizing)
				{
					Job.CurrentBuildStep = ""; // Make sure Build Steps are reset.
					Job.PreviousBuildStep = "";

					if (!Job.IsPreviousFinalizationStepAssigned)
					{
						// Current Builder is just getting started. Proceed to first Finalization Step.
						Job.CurrentFinalizationStep = GetFirstStep(true);
					}
					else
					{
						// Continuing from a Finalization Step of an ongoing run. See if there is a next Finalization Step.
						var nextStep = GetNextStep(Job.PreviousFinalizationStep, true);
						if (!string.IsNullOrEmpty(nextStep))
						{
							// Proceed to next Finalization Step.
							Job.CurrentFinalizationStep = nextStep;
						}
						else
						{
							// Finished all Finalization Steps of current Builder. See if there is a next Builder.
							// But also check if the Build Run was failed. If so, and with all Finalization Steps
							// are executed, it is time to end the Build Run.
							Job.CurrentFinalizationStep = "";
							Job.PreviousFinalizationStep = "";
							Job.Finalizing = false;
							Debug.Assert(Job.Result != BuildJobResult.Succeeded);
							if (Job.Result == BuildJobResult.Failed)
							{
								DoBuildRunFinalization();
								completed = true;
							}
							else
							{
								if (!Job.IsLastBuilder)
								{
									// Proceed to next Builder. The RunStep will be run again and it will start
									// from the next Builder's first Build Step.
									Job.CurrentBuilder++;
									SaveRunningJobToFile();
									completed = true;
								}
								else
								{
									// All Builders are completed for current Phase. See if there is a next Phase.
									if (!Job.IsLastPhase)
									{
										// Proceed to next Phase. The RunStep will be run again and it will start
										// from the first Build Step of next Phase's first Builder.
										Job.CurrentBuilder = 0;
										Job.CurrentPhase++;
										SaveRunningJobToFile();
										completed = true;
									}
									else
									{
										// All Phases are completed. That means Build Run is completed.
										Job.CurrentBuilder = -2;
										Job.CurrentPhase = -2;
										Job.CurrentFinalizationStep = "";
										Job.PreviousFinalizationStep = "";
										Job.SetResult(BuildJobResult.Succeeded);
										DoBuildRunFinalization();
										completed = true;
									}
								}
							}
						}
					}
				}
			}
			catch (Exception exception)
			{
				// The operation in this try block should be minimal and straightforward.
				// It just selects the next Build Step to be executed.
				// Should anything went wrong, means there is definitely an internal error.
				throw new InternalException(1121821, exception);
			}

			if (completed)
			{
				yield break;
			}
			else
			{
				// Save current state just after determining the current Step. So the next time
				// the Survival File is reloaded, we would have the opportunity to check if there is
				// a CurrentStep specified in it, which is unexpected and means something went wrong
				// in the middle of Step execution. See 11917631.
				SaveRunningJobToFile();
			}

			Debug.Assert(Job.Builders.IsInRange(Job.CurrentBuilder));
			Debug.Assert(Job._CurrentStepInfoCached.Method != null);
			var currentStep = Job._CurrentStepInfoCached.Name;
			Debug.Assert(!string.IsNullOrEmpty(currentStep));
			var currentBuilder = Job.Builders[Job.CurrentBuilder];
			Debug.Assert(currentBuilder != null);

			// Change Unity's active platform if required.
			{
				var buildTarget = currentBuilder.Info.BuildTarget;
				var buildTargetGroup = currentBuilder.Info.BuildTargetGroup;
				if (EditorUserBuildSettings.activeBuildTarget != buildTarget)
				{
					Log.Info($"Changing active build platform from '{EditorUserBuildSettings.activeBuildTarget}' to '{buildTarget}' of group '{buildTargetGroup}'.");
					EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, buildTarget);

					var haltExecution = CheckAfterChangingActivePlatform();
					if (haltExecution)
						yield break;

					// TODO: There we might need an assembly reload, or a Unity restart.
				}
			}

			// Run the Step
			yield return null; // As a precaution, won't hurt to wait for one frame for all things to settle down.
			{
				StartStep(currentStep);

				var currentStepInfo = Job._CurrentStepInfoCached;
				Job._CurrentStepInfoCached = BuildStepInfo.Empty;
				var enumerator = (IEnumerator)currentStepInfo.Method.Invoke(currentBuilder, new object[] { Job, currentStepInfo }); // See 113654126.
				yield return EditorCoroutineUtility.StartCoroutineOwnerless(enumerator);

				EndStep(currentStep);
			}
			yield return null; // As a precaution, won't hurt to wait for one frame for all things to settle down.

			#region Get Step

			string GetFirstStep(bool finalization)
			{
				if (!BuildPhases.IsInRange(Job.CurrentPhase) ||
					!Builders.IsInRange(Job.CurrentBuilder))
					throw new IndexOutOfRangeException($"Phase {Job.CurrentPhase}/{BuildPhases.Length} Builder {Job.CurrentBuilder}/{Builders.Length}");

				var phase = BuildPhases[Job.CurrentPhase];
				var builder = Builders[Job.CurrentBuilder];
				BuildStepInfo firstStepOfCurrentPhase;
				if (finalization)
				{
					firstStepOfCurrentPhase = builder.Info.Steps.FirstOrDefault(entry => phase.IncludedFinalizationSteps.Contains(entry.Type));
				}
				else
				{
					firstStepOfCurrentPhase = builder.Info.Steps.FirstOrDefault(entry => phase.IncludedBuildSteps.Contains(entry.Type));
				}

				if (firstStepOfCurrentPhase.IsEmpty)
				{
					// TODO: Check if the step exists. What to do if it does not exist?
					// TODO: Script reload is a quick fix that prevents going into infinite loop. Delete it when implementing the behaviour.
					EditorUtilityTools.RequestScriptReload();
					throw new NotImplementedException("The behaviour of not finding the first step is not implemented yet!");
				}

				Debug.Assert(Job._CurrentStepInfoCached.Method == null);
				Job._CurrentStepInfoCached = firstStepOfCurrentPhase;
				return firstStepOfCurrentPhase.Name;
			}

			string GetNextStep(string previousStep, bool finalization)
			{
				Debug.Assert(!string.IsNullOrEmpty(previousStep));
				if (!BuildPhases.IsInRange(Job.CurrentPhase) ||
					!Builders.IsInRange(Job.CurrentBuilder))
					throw new IndexOutOfRangeException($"Phase {Job.CurrentPhase}/{BuildPhases.Length} Builder {Job.CurrentBuilder}/{Builders.Length}");

				var phase = BuildPhases[Job.CurrentPhase];
				var builder = Builders[Job.CurrentBuilder];
				List<BuildStepInfo> allStepsOfCurrentPhase;
				if (finalization)
				{
					allStepsOfCurrentPhase = builder.Info.Steps.Where(entry => phase.IncludedFinalizationSteps.Contains(entry.Type)).ToList();
				}
				else
				{
					allStepsOfCurrentPhase = builder.Info.Steps.Where(entry => phase.IncludedBuildSteps.Contains(entry.Type)).ToList();
				}

				if (allStepsOfCurrentPhase.IsNullOrEmpty())
				{
					// TODO: Check if any step exists. What to do if none exist?
					throw new NotImplementedException("The behaviour of not finding any steps is not implemented yet!");
				}

				var foundIndex = -1;
				for (int i = 0; i < allStepsOfCurrentPhase.Count; i++)
				{
					if (allStepsOfCurrentPhase[i].Name.Equals(previousStep, StringComparison.Ordinal))
					{
						foundIndex = i;
						break;
					}
				}

				// Check if the specified Step found.
				if (foundIndex < 0)
				{
					throw new Exception($"Failed to find previous step '{previousStep}'.");
				}

				// Check if this is the last Step.
				if (foundIndex >= allStepsOfCurrentPhase.Count - 1)
				{
					// There is no next Step.
					return "";
				}

				// Get the next Step.
				Debug.Assert(Job._CurrentStepInfoCached.Method == null);
				Job._CurrentStepInfoCached = allStepsOfCurrentPhase[foundIndex + 1];
				return allStepsOfCurrentPhase[foundIndex + 1].Name;
			}

			#endregion
		}

		#endregion

		#region Halt

		private static void HaltStep(string description)
		{
			RunningJob.StepState = BuildJobStepState.StepHalt;
			RunningJob.LastHaltTime = Now;

			if (RunningJob.IsAssemblyReloadScheduled)
			{
				EditorUtilityTools.RequestScriptReload();
				RunningJob.IsAssemblyReloadScheduled = false;
			}

			Log.Info($"Halting the execution until next assembly reload ({description}).");
		}

		#endregion

		#region Exception Handling

		private static bool CatchRunException(Exception exception)
		{
			Log.Error("Exception caught in Build Run. Exception: " + exception);

			if (RunningJob != null)
			{
				RunningJob.Finalizing = true;
				RunningJob.SetResult(BuildJobResult.Failed);
				SaveRunningJobToFile();
			}
			else
			{
				// RunningJob was supposed to be there. Something went terribly wrong. Investigate.
				Log.InternalError(11636112);
			}

			return true;
		}

		private static bool CatchRunStepException(Exception exception)
		{
			Log.Error("Exception caught in Build Step. Exception: " + exception);

			if (RunningJob != null)
			{
				RunningJob.ErrorReceivedInLastStep = exception.Message;
				RunningJob.Finalizing = true;
				RunningJob.SetResult(BuildJobResult.Failed);
				SaveRunningJobToFile();
			}
			else
			{
				// RunningJob was supposed to be there. Something went terribly wrong. Investigate.
				Log.InternalError(11636113);
			}

			return true;
		}

		#endregion

		#region Start/End Step

		private static void StartStep(string stepName)
		{
			var now = Now;
			var totalElapsed = now - RunningJob.StartTime;

			RunningJob.LastStepStartTime = now;

			Log.Info($"{totalElapsed.ToStringHoursMinutesSecondsMilliseconds()} | Started build step '{stepName}'");
			//DisplayProgressBar("Build Step " + CurrentStep, CurrentStepTitle);
		}

		private static void EndStep(string stepName)
		{
			var now = Now;
			var stepDuration = now - RunningJob.LastStepStartTime;

			Log.Info($"Build step '{stepName}' took {stepDuration.ToStringHoursMinutesSecondsMilliseconds()}.");
			RunningJob.LastStepStartTime = default;
		}

		#endregion

		#region Check Before/After Step

		private static bool CheckBeforeStartingOrContinuing()
		{
			var haltExecution = false;

			// At this point, there should be no ongoing compilations. Build system
			// would not be happy if there is a compilation while it starts the process.
			// Otherwise execution gets really messy.
			if (EditorApplication.isCompiling)
			{
				throw new Exception("Compilation is not allowed at the start of a build run or when continuing the build run.");
			}

			// Save the unsaved assets before making any moves.
			AssetDatabase.SaveAssets();

			// Make sure everything is imported. This may trigger an assembly reload
			// if there are script modifications.
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

			// Check if AssetDatabase.Refresh triggered a compilation
			// OR the user requested an assembly reload.
			{
				var isCompiling = EditorApplication.isCompiling;
				if (isCompiling || RunningJob.IsAssemblyReloadScheduled)
				{
					haltExecution = true;
					HaltStep($"Start/continue - Compiling: {isCompiling} Scheduled: {RunningJob.IsAssemblyReloadScheduled}");
					SaveRunningJobToFile();
				}
			}

			return haltExecution;
		}

		private static bool CheckAfterChangingActivePlatform()
		{
			var haltExecution = false;

			// Check if changing the platform triggered a compilation, which obviously
			// is expected.
			{
				var isCompiling = EditorApplication.isCompiling;
				if (isCompiling || RunningJob.IsAssemblyReloadScheduled)
				{
					haltExecution = true;
					HaltStep($"Platform change - Compiling: {isCompiling} Scheduled: {RunningJob.IsAssemblyReloadScheduled}");
					SaveRunningJobToFile();
				}
			}

			return haltExecution;
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
				throw new Exception("Compilation is not allowed before starting the step.");
			}

			// Save the unsaved assets before making any moves.
			AssetDatabase.SaveAssets();

			// Make sure everything is imported. This may trigger an assembly reload
			// if there are script modifications.
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

			// Check if AssetDatabase.Refresh triggered a compilation
			// OR the user requested an assembly reload.
			{
				var isCompiling = EditorApplication.isCompiling;
				if (isCompiling || RunningJob.IsAssemblyReloadScheduled)
				{
					haltExecution = true;
					HaltStep($"Before step - Compiling: {isCompiling} Scheduled: {RunningJob.IsAssemblyReloadScheduled}");
					SaveRunningJobToFile();
				}
			}

			return haltExecution;
		}

		private static bool CheckAfterStep()
		{
			var haltExecution = false;

			// Save the unsaved assets before making any moves.
			AssetDatabase.SaveAssets();

			// Make sure everything is imported. This may trigger an assembly reload
			// if there are script modifications.
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

			// Check if AssetDatabase.Refresh triggered a compilation
			// OR the user requested an assembly reload.
			{
				var isCompiling = EditorApplication.isCompiling;
				if (isCompiling || RunningJob.IsAssemblyReloadScheduled)
				{
					haltExecution = true;
					HaltStep($"After step - Compiling: {isCompiling} Scheduled: {RunningJob.IsAssemblyReloadScheduled}");
					SaveRunningJobToFile();
				}
			}

			return haltExecution;
		}

		#endregion

		#region Build Run Finalization

		private static void DoBuildRunFinalization()
		{
			Log.Info($"Finalizing the '{RunningJob.Result}' build job.");
			EditorApplicationTools.EnsureNotCompiling(false);

			RunningJob.OverallState = BuildJobOverallState.JobFinished;

			// Execute finalization on RunningJob
			try
			{
				RunningJob.BuildRunFinalization();
			}
			catch (Exception exception)
			{
				Log.Error("Failed to execute build run finalization on job. Exception: " + exception);
			}

			if (RunningJob.Result == BuildJobResult.Succeeded)
			{
				Log.Info($"Build '{RunningJob.NameSafe()}' succeeded.");
			}
			else
			{
				Log.Error($"Build '{RunningJob.NameSafe()}' failed. See the log for details.");
			}

			UnsetRunningJob();
			DeleteRunningJobFile();

			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
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

		private static void SaveRunningJobToFile()
		{
			Log.Info("Saving running job for assembly reload survival.");

			var job = RunningJob;
			if (job == null)
				throw new ArgumentNullException(nameof(job));

			//if (IsRunningJobFileExists())
			//{
			//	throw new Exception("Running job survival file is expected not to exist, yet it does.");
			//}

			var content = job.SerializeToJson();

			AssetDatabase.ReleaseCachedFileHandles(); // Make Unity release the files to prevent any IO errors.

			DirectoryTools.CreateFromFilePath(BuildMachineConstants.RunningJobSurvivalFilePath);
			File.WriteAllText(BuildMachineConstants.RunningJobSurvivalFilePath, content);

			// Save a copy for logging purposes.
			{
				var directoryPath = string.Format(BuildMachineConstants.RunningJobSurvivalFileLogDirectory, job.ID);
				DirectoryTools.Create(directoryPath);
				var filePath = Path.Combine(directoryPath, BuildMachineConstants.RunningJobSurvivalFileName);
				var availableFilePath = filePath.GenerateUniqueFilePath();
				File.WriteAllText(availableFilePath, content);
			}
		}

		private static BuildJob LoadRunningJobFromFile()
		{
			if (IsRunningJobFileExists())
			{
				Log.Info("Loading running job after assembly reload.");

				var content = File.ReadAllText(BuildMachineConstants.RunningJobSurvivalFilePath);
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
			return File.Exists(BuildMachineConstants.RunningJobSurvivalFilePath);
		}

		private static void DeleteRunningJobFile()
		{
			AssetDatabase.ReleaseCachedFileHandles(); // Make Unity release the files to prevent any IO errors.

			try
			{
				File.Delete(BuildMachineConstants.RunningJobSurvivalFilePath);
			}
			catch (DirectoryNotFoundException)
			{
				// Ignore
			}
		}

		#endregion

		#region Tools

		private static DateTime Now => DateTime.Now;

		#endregion
	}

}
