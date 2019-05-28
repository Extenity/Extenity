using System;
using System.Collections;
using System.IO;
using System.Linq;
using Extenity.ApplicationToolbox.Editor;
using Extenity.BuildToolbox.Editor;
using Extenity.DataToolbox;
using Extenity.FileSystemToolbox;
using Extenity.ParallelToolbox;
using Extenity.UnityEditorToolbox.Editor;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace Extenity.BuildMachine.Editor
{

	public static class BuildJobRunner
	{
		#region Running Job

		private static BuildJob _RunningJob;
		public static BuildJob RunningJob => _RunningJob;
		public static bool IsRunning => RunningJob != null;

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

		internal static void Start(BuildJob _job)
		{
			var jobPlanName = _job.NameSafe();
			if (IsRunning)
			{
				throw new Exception($"Tried to start build job '{jobPlanName}' while there is already a running one.");
			}

			Log.Info($"Build '{jobPlanName}' started.");
			SetRunningJob(_job);
			Debug.Assert(RunningJob.CurrentState == BuildJobState.JobInitialized, RunningJob.CurrentState.ToString());
			Debug.Assert(RunningJob.CurrentPhase == -1);
			Debug.Assert(RunningJob.CurrentBuilder == -1);
			Debug.Assert(!RunningJob.IsPreviousStepAssigned);
			Debug.Assert(!RunningJob.IsCurrentStepAssigned);
			RunningJob.StartTime = Now;

			ChecksBeforeStartOrContinue("start");

			RunningJob.BuildRunInitialization();

			RunningJob.CurrentPhase = 0;
			RunningJob.CurrentBuilder = 0;
			EditorCoroutineUtility.StartCoroutineOwnerless(Run(), OnException);
		}

		private static void Continue(BuildJob _job)
		{
			var jobPlanName = _job.NameSafe();
			if (IsRunning)
			{
				throw new Exception($"Tried to continue build job '{jobPlanName}' while there is already a running one.");
			}

			SetRunningJob(_job);
			Log.Info($"Continuing the build '{RunningJob.Name}' at phase '{RunningJob.ToStringCurrentPhase()}' and builder '{RunningJob.ToStringCurrentBuilder()}' with previously processed step '{RunningJob.PreviousStep}'.");

			// See 11917631.
			switch (RunningJob.CurrentState)
			{
				// Expected state(s) that would be encountered when continuing after assembly reload.
				case BuildJobState.StepHalt:
					break;

				// Unexpected states. Encountering these would mean something went wrong in build run.
				case BuildJobState.JobInitialized:
				case BuildJobState.StepRunning:
				case BuildJobState.StepFinalization:
				case BuildJobState.StepFailed:
				case BuildJobState.StepSucceeded:
				case BuildJobState.JobFinalization:
				case BuildJobState.JobFailed:
				case BuildJobState.JobSucceeded:
					{
						UnsetRunningJob();
						throw new Exception($"Build job '{jobPlanName}' was disrupted in the middle for some reason. It could happen if Editor crashes during build, if not happened for an unexpected reason.");
					}

				case BuildJobState.Unknown: // That should be impossible
				default:
					throw new ArgumentOutOfRangeException(nameof(RunningJob.CurrentState), RunningJob.CurrentState, "Unexpected state.");
			}

			ChecksBeforeStartOrContinue("continue");

			EditorCoroutineUtility.StartCoroutineOwnerless(Run(), OnException);
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
						BuildMachineTools.LoadBuildMachineLayout();
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

		private static bool OnException(Exception exception)
		{
			Log.Error("Exception caught in Build Step. Exception: " + exception);

			// NOTE: Keep this line at the TOP, just after the log line above.
			// Get rid of the survival file immediately, without making any moves.
			// The build was failed. So even a slight possibility of reloading
			// the survival file on next assembly reload is a deal breaker.
			DeleteRunningJobFile();

			if (RunningJob != null && RunningJob.IsCurrentBuilderAssigned)
			{
				RunningJob.Builders[RunningJob.CurrentBuilder].DoBuilderFinalizationForCurrentPhase(RunningJob);
			}

			DoBuildRunFinalization(false);

			// NOTE: Keep this line at the BOTTOM.
			// There is a possibility that a survival file could be created in the
			// operations above. So make sure that file will be gone too.
			DeleteRunningJobFile();

			return false;
		}

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

			while (IsRunning)
			{
				// Check before running the step
				{
					var haltExecution = CheckBeforeStep();
					if (haltExecution)
						yield break;
				}

				// Run the step
				yield return RunStep();

				// Check after running the step
				{
					CheckAfterStepBeforePostStep();
				}

				// Run the post-step operations
				{
					if (Job.DelayedAssemblyReloadingOperations != null)
					{
						// Lose the registered events before we start processing them.
						// Any exceptions thrown in those events would cause these event
						// references to be kept indefinitely.
						var operations = Job.DelayedAssemblyReloadingOperations;
						Job.DelayedAssemblyReloadingOperations = null;

						foreach (var operation in operations)
						{
							operation.Invoke();
						}
					}
				}

				// Finalize the step
				{
					// Mark the current step as previous step and relieve the current step.
					// Then immediately save the assembly survival file so that we will know
					// the process was not cut in the middle of step execution the next time
					// the survival file is loaded. See 11917631.
					Job.PreviousStep = Job.CurrentStep;
					Job.CurrentStep = "";
					Job._CurrentStepCached = BuildStepInfo.Empty;
					SaveRunningJobToFile();
				}

				// Check after running post-step
				{
					var haltExecution = CheckAfterPostStep();
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

			// Yes making it local and instantiating it in each step is not wise for performance.
			// But better for consistency and we are not fighting for milliseconds in Editor.
			var delayedCaller = new DelayedCaller<Action<BuildJob>>();

			// Find the next step to be processed. If there is none left, finalize the build run.
			var completed = false;
			try
			{
				if (!Job.IsPreviousStepAssigned)
				{
					// Current builder is just getting started. Proceed to first step.
					Job.CurrentStep = GetFirstStep();
					delayedCaller.AddDelayedCall(Job.Builders[Job.CurrentBuilder].DoBuilderInitializationForCurrentPhase);
				}
				else
				{
					// Continuing from a step of an ongoing run. See if there is a next step.
					var nextStep = GetNextStep(Job.PreviousStep);
					if (!string.IsNullOrEmpty(nextStep))
					{
						// Proceed to next step.
						Job.CurrentStep = nextStep;
					}
					else
					{
						// Finished all steps of current builder. See if there is a next builder.
						Job.CurrentStep = "";
						Job.PreviousStep = "";
						delayedCaller.AddDelayedCall(Job.Builders[Job.CurrentBuilder].DoBuilderFinalizationForCurrentPhase);

						if (!Job.IsLastBuilder)
						{
							// Proceed to next builder and start from its first step.
							Job.CurrentBuilder++;
							Job.CurrentStep = GetFirstStep();
							delayedCaller.AddDelayedCall(Job.Builders[Job.CurrentBuilder].DoBuilderInitializationForCurrentPhase);
						}
						else
						{
							// All builders are completed for current phase. See if there is a next phase.
							if (!Job.IsLastPhase)
							{
								// Proceed to next phase and start from its first step of its first builder.
								Job.CurrentBuilder = 0;
								Job.CurrentPhase++;
								Job.CurrentStep = GetFirstStep();
								delayedCaller.AddDelayedCall(Job.Builders[Job.CurrentBuilder].DoBuilderInitializationForCurrentPhase);
							}
							else
							{
								// All phases are complete.
								Job.CurrentBuilder = -2;
								Job.CurrentPhase = -2;
								Job.CurrentStep = "";
								Job._CurrentStepCached = BuildStepInfo.Empty;
								delayedCaller.AddDelayedCall((job) => DoBuildRunFinalization(true));
								completed = true;
							}
						}
					}
				}
			}
			catch (Exception exception)
			{
				// The operation in this try block should be minimal and straightforward.
				// It just selects the next build step to be executed and while doing that,
				// determines which callback methods should be called and delays those calls.
				// Should anything went wrong, means there is definitely an internal error.
				throw new InternalException(1121821, exception);
			}
			// Save current state just after determining the current step. So the next time
			// the survival file is reloaded, we would have the opportunity to check if there is
			// a CurrentStep specified in it, which is unexpected and means something went wrong
			// in the middle of step execution. See 11917631.
			SaveRunningJobToFile();

			// After saving the survival file, we can call the callbacks delayed above.
			{
				EditorApplicationTools.EnsureNotCompiling(false);
				delayedCaller.CallAllDelayedCalls(action => action(Job));
				delayedCaller = null;
				EditorApplicationTools.EnsureNotCompiling(false);
			}

			if (completed)
			{
				yield break;
			}

			// Run the step
			yield return null; // As a precaution, won't hurt to wait for one frame for all things to settle down.
			{
				Debug.Assert(Job.Builders.IsInRange(Job.CurrentBuilder));
				var currentStep = Job.CurrentStep;
				var currentBuilder = Job.Builders[Job.CurrentBuilder];
				Debug.Assert(!string.IsNullOrEmpty(currentStep));
				Debug.Assert(currentBuilder != null);
				Debug.Assert(Job._CurrentStepCached.Method != null);

				StartStep(currentStep);

				var enumerator = (IEnumerator)Job._CurrentStepCached.Method.Invoke(currentBuilder, new object[] { Job, Job._CurrentStepCached }); // See 113654126.
				yield return EditorCoroutineUtility.StartCoroutineOwnerless(enumerator);

				EndStep(currentStep);
			}
			yield return null; // As a precaution, won't hurt to wait for one frame for all things to settle down.

			#region Get Step

			string GetFirstStep()
			{
				if (!BuildPhases.IsInRange(Job.CurrentPhase) ||
					!Builders.IsInRange(Job.CurrentBuilder))
					throw new IndexOutOfRangeException($"Phase {Job.CurrentPhase}/{BuildPhases.Length} Builder {Job.CurrentBuilder}/{Builders.Length}");

				var currentPhase = BuildPhases[Job.CurrentPhase];
				var currentBuilder = Builders[Job.CurrentBuilder];
				var firstStepOfCurrentPhase = currentBuilder.Info.Steps.FirstOrDefault(entry => currentPhase.IncludedSteps.Contains(entry.Type));

				if (firstStepOfCurrentPhase.IsEmpty)
				{
					// TODO: Check if the step exists. What to do if it does not exist?
					throw new NotImplementedException("The behaviour of not finding the first step is not implemented yet!");
				}

				Job._CurrentStepCached = firstStepOfCurrentPhase;
				return firstStepOfCurrentPhase.Name;
			}

			string GetNextStep(string previousStep)
			{
				Debug.Assert(!string.IsNullOrEmpty(previousStep));
				if (!BuildPhases.IsInRange(Job.CurrentPhase) ||
					!Builders.IsInRange(Job.CurrentBuilder))
					throw new IndexOutOfRangeException($"Phase {Job.CurrentPhase}/{BuildPhases.Length} Builder {Job.CurrentBuilder}/{Builders.Length}");

				var currentPhase = BuildPhases[Job.CurrentPhase];
				var currentBuilder = Builders[Job.CurrentBuilder];
				var allStepsOfCurrentPhase = currentBuilder.Info.Steps.Where(entry => currentPhase.IncludedSteps.Contains(entry.Type)).ToList();

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

				// Check if the specified step found.
				if (foundIndex < 0)
				{
					throw new Exception($"Failed to find previous step '{previousStep}'.");
				}

				// Check if this is the last step.
				if (foundIndex >= allStepsOfCurrentPhase.Count - 1)
				{
					// There is no next step.
					return "";
				}

				// Get the next step.
				Job._CurrentStepCached = allStepsOfCurrentPhase[foundIndex + 1];
				return allStepsOfCurrentPhase[foundIndex + 1].Name;
			}

			#endregion
		}

		private static void HaltStep(string description)
		{
			RunningJob.CurrentState = BuildJobState.StepHalt;
			RunningJob.LastHaltTime = Now;
			Log.Info($"Halting the execution until next assembly reload ({description}).");
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

			// Make sure everything is imported.
			{
				AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

				// And wait for scripts to compile.
				if (EditorApplication.isCompiling)
				{
					haltExecution = true;
					HaltStep("Start/Continue");
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

			// Make sure everything is imported.
			{
				AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

				// And wait for scripts to compile.
				if (EditorApplication.isCompiling)
				{
					haltExecution = true;
					HaltStep("Before step");
					SaveRunningJobToFile();
				}
			}

			return haltExecution;
		}

		private static void CheckAfterStepBeforePostStep()
		{
			// At this point, there should be no ongoing compilations.
			// Build system does not allow any code that triggers
			// an assembly reload in Build Step. Otherwise execution
			// gets really messy.
			if (EditorApplication.isCompiling)
			{
				throw new Exception("Compilation is not allowed in the middle of build step.");
			}
		}

		private static bool CheckAfterPostStep()
		{
			var haltExecution = false;

			// Save the unsaved assets before making any moves.
			AssetDatabase.SaveAssets();

			// Make sure everything is imported.
			{
				AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

				// And wait for scripts to compile.
				if (EditorApplication.isCompiling)
				{
					haltExecution = true;
					HaltStep("After step");
					SaveRunningJobToFile();
				}
			}

			return haltExecution;
		}

		#endregion

		#region Build Run Finalization

		private static void DoBuildRunFinalization(bool succeeded)
		{
			Log.Info($"Finalizing the '{(succeeded ? "succeeded" : "failed")}' build job.");
			EditorApplicationTools.EnsureNotCompiling(false);

			// Execute finalization on RunningJob
			try
			{
				if (RunningJob == null)
					throw new Exception($"{nameof(RunningJob)} was not set.");

				RunningJob.BuildRunFinalization(succeeded);
			}
			catch (Exception exception)
			{
				Log.Error("Failed to execute finalization on job. Exception: " + exception);
			}

			if (succeeded)
			{
				Log.Info($"Build '{RunningJob.NameSafe()}' succeeded.");
			}
			else
			{
				Log.Error($"Build '{RunningJob.NameSafe()}' failed. See the log for details.");
			}

			UnsetRunningJob();

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

		#region Tools

		private static DateTime Now => DateTime.Now;

		#endregion
	}

}
