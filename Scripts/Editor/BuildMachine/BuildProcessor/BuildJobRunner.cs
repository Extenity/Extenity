using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Extenity.ApplicationToolbox;
using Extenity.ApplicationToolbox.Editor;
using Extenity.BuildToolbox.Editor;
using Extenity.DataToolbox;
using Extenity.FileSystemToolbox;
using Extenity.ParallelToolbox.Editor;
using Extenity.SceneManagementToolbox.Editor;
using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.Experimental;
using Debug = UnityEngine.Debug;

namespace Extenity.BuildMachine.Editor
{

	public static class BuildJobRunner
	{
		#region IsRunning State

		public static bool IsRunning { get; private set; }

		private static void SetRunningJobState()
		{
			if (IsRunning)
			{
				throw new BuildMachineException($"Tried to Set {nameof(BuildJobRunner)}.{nameof(IsRunning)} state but it was already set.");
			}
			Log.Info($"Setting the {nameof(BuildJobRunner)}.{nameof(IsRunning)} state");
			IsRunning = true;
		}

		private static void UnsetRunningJobState()
		{
			var previousState = IsRunning;
			Log.Info($"Unsetting the {nameof(BuildJobRunner)}.{nameof(IsRunning)} state. Previously was '{(previousState ? "set" : "not set")}'.");
			IsRunning = false;
		}

		#endregion

		#region Start

		internal static void Start(BuildJob job)
		{
			if (IsRunning)
			{
				throw new BuildMachineException($"Tried to 'Start' build job '{job.NameSafe()}' while there is already a running one.");
			}
			if (BuildTools.IsCompiling)
			{
				throw new BuildMachineException($"Tried to 'Start' build job '{job.NameSafe()}' in the middle of an ongoing compilation.");
			}

			Log.Info($"Starting the build '{job.NameSafe()}'...\n" +
			         "Here is the current state of the job for debugging purposes.\n" +
			         "Note that they might look suspicious because we are at the very beginning of build process " +
			         "where these states are not initialized yet.\n" +
			         $"Builder '{job.ToStringBuilderName()}' in Phase '{job.ToStringCurrentPhase()}'\n" +
			         $"Build Step '{job.CurrentBuildStep}' (Previously: {job.PreviousBuildStep})\n" +
			         $"Finalization Step '{job.CurrentFinalizationStep}' (Previously: {job.PreviousFinalizationStep})\n" +
			         $"Job ID: {job.ID}");

			// Check state consistency
			{
				Debug.Assert(job.OverallState == BuildJobOverallState.JobInitialized);
				Debug.Assert(job.StepState == BuildJobStepState.Unknown);
				Debug.Assert(job.Result == BuildJobResult.Incomplete);
				Debug.Assert(job.CurrentPhase == -1);
				Debug.Assert(!job.IsPreviousBuildStepAssigned);
				Debug.Assert(!job.IsCurrentBuildStepAssigned);
				Debug.Assert(!job.IsPreviousFinalizationStepAssigned);
				Debug.Assert(!job.IsCurrentFinalizationStepAssigned);
			}

			job.StartTime = Now;
			job.CurrentPhase = 0;
			job.OverallState = BuildJobOverallState.JobRunning;

			SetRunningJobState(); // Set it just before the Run call. Otherwise an exception in the codes above would cause the value of static field to be left behind.
			EditorCoroutineUtility.StartCoroutineOwnerless(Run(job), job.CatchRunException);
		}

		private static void Continue(BuildJob job)
		{
			if (IsRunning)
			{
				throw new BuildMachineException($"Tried to 'Continue' build job '{job.NameSafe()}' while there is already a running one.");
			}
			if (BuildTools.IsCompiling)
			{
				throw new BuildMachineException($"Tried to 'Continue' build job '{job.NameSafe()}' in the middle of an ongoing compilation.");
			}

			Log.Info($"Continuing the build '{job.NameSafe()}'...\n" +
			         "Here is the current state of the job for debugging purposes.\n" +
			         $"Builder '{job.ToStringBuilderName()}' in Phase '{job.ToStringCurrentPhase()}'\n" +
			         $"Build Step '{job.CurrentBuildStep}' (Previously: {job.PreviousBuildStep})\n" +
			         $"Finalization Step '{job.CurrentFinalizationStep}' (Previously: {job.PreviousFinalizationStep})\n" +
			         $"Job ID: {job.ID}");

			// Check state consistency
			{
				// See 11917631.
				// The only expected state is StepHalt when continuing after assembly reload.
				if (job.StepState != BuildJobStepState.StepHalt)
				{
					UnsetRunningJobState();
					throw new BuildMachineException($"Build job '{job.NameSafe()}' was disrupted in the middle for some reason. " +
					                               $"It could happen if Editor crashes during build, if not happened " +
					                               $"for an unexpected reason." +
					                               $"(Overall: '{job.OverallState}' Step: '{job.StepState}' Result: '{job.Result}')");
				}
				Debug.Assert(job.OverallState == BuildJobOverallState.JobRunning, $"Unexpected overall state '{job.OverallState}'.");
			}

			SetRunningJobState(); // Set it just before the Run call. Otherwise an exception in the codes above would cause the value of static field to be left behind.
			EditorCoroutineUtility.StartCoroutineOwnerless(Run(job), job.CatchRunException);
		}

		#endregion

		#region Run

		private static IEnumerator Run(BuildJob job)
		{
			job.LastHaltTime = default;

			// At this point, there should be no ongoing compilations. Build system
			// would not be happy if there is a compilation while it processes the step.
			// Otherwise execution gets really messy. See 11685123.
			if (EditorApplication.isCompiling)
			{
				ThrowScriptCompilationDetectedBeforeStartingTheBuildRun();
			}

			ChecksBeforeRun(job);

			// Make console full-screen
			BuildMachineLayout.LoadConsoleOnlyLayout();

			// The assets should be saved and refreshed at the very beginning of compilation
			// or continuing the compilation after assembly reload.
			{
				// Save the unsaved assets before making any moves. Make sure everything is imported.
				// This may trigger an assembly reload if there are script modifications. See 11658912.
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

				// Check if AssetDatabase.Refresh triggered a compilation
				// OR the user requested an assembly reload.
				{
					var isCompiling = EditorApplication.isCompiling;
					if (isCompiling || job.IsAssemblyReloadScheduled)
					{
						HaltStep(job, $"Start/continue - Compiling: {isCompiling} Scheduled: {job.IsAssemblyReloadScheduled}");
						SaveRunningJobToFile(job);
						yield break; // Halt execution
					}
				}
			}

			// Deselect any asset or object.
			Selection.activeObject = null;
			yield return null;

			// Close all scenes. Hopefully this will boost the build a bit.
			EditorSceneManagerTools.UnloadAllScenes(true);
			yield return null;

			// Run build steps
			while (IsRunning)
			{
				// Ensure there is no compilation going on before running the build step.
				{
					// At this point, there should be no ongoing compilations. Build system
					// would not be happy if there is a compilation while it processes the step.
					// Otherwise execution gets really messy. See 11685123.
					if (EditorApplication.isCompiling)
					{
						ThrowScriptCompilationDetectedBeforeProcessingBuildStep();
					}
				}

				// AssetDatabase Save and Refresh before the Step
				{
					// Save the unsaved assets before making any moves. Make sure everything is imported.
					// This may trigger an assembly reload if there are script modifications. See 11658912.
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

					// Check if AssetDatabase.Refresh triggered a compilation
					// OR the user requested an assembly reload.
					{
						var isCompiling = EditorApplication.isCompiling;
						if (isCompiling || job.IsAssemblyReloadScheduled)
						{
							HaltStep(job, $"Before step - Compiling: {isCompiling} Scheduled: {job.IsAssemblyReloadScheduled}");
							SaveRunningJobToFile(job);
							yield break;
						}
					}
				}

				// Run the Step
				{
					job.ErrorReceivedInLastStep = "";
					job.StepState = BuildJobStepState.StepRunning;
					SaveRunningJobToFile(job);

					// We already catch exceptions and fail the build. We may also catch error logs.
					// But then, there will be some errors that Unity would write out of nowhere and
					// failing the build for these logs which we don't have any control over, 
					// would be a bit harsh. So any code that wants to fail the build should throw
					// an exception, instead of logging an error.
					// Application.logMessageReceivedThreaded += 
					//
					// or...
					//
					// On second thought, it might be a good idea to fail the build for all errors.
					// See if Unity would throw these cryptic errors again and try to come up with a
					// solution for them. Filtering some error logs as required might work.
					job.RegisterForErrorLogCatching();

					job.RegisterForCompilationCatching();

					EditorApplication.LockReloadAssemblies();
					Log.Info("Build step coroutine started");
					yield return EditorCoroutineUtility.StartCoroutineOwnerless(RunStep(job), job.CatchRunStepException);
					Log.Info("Build step coroutine finished");
					EditorApplication.UnlockReloadAssemblies();

					job.DeregisterFromCompilationCatching();

					job.DeregisterFromErrorLogCatching();

					if (!string.IsNullOrEmpty(job.ErrorReceivedInLastStep))
					{
						job.ErrorReceivedInLastStep = "";
						job.Finalizing = true;
						job.SetResult(BuildJobResult.Failed);
						SaveRunningJobToFile(job);
					}

					// Don't do anything if the Build Run finishes.
					if (job.OverallState == BuildJobOverallState.JobFinished)
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
					job.StepState = BuildJobStepState.StepFinished;
					if (job.IsCurrentBuildStepAssigned)
					{
						job.PreviousBuildStep = job.CurrentBuildStep;
						job.CurrentBuildStep = "";
					}
					if (job.IsCurrentFinalizationStepAssigned)
					{
						job.PreviousFinalizationStep = job.CurrentFinalizationStep;
						job.CurrentFinalizationStep = "";
					}
					SaveRunningJobToFile(job);
				}

				// Ensure there is no compilation going on after running the build step.
				{
					// At this point, there should be no ongoing compilations. Build system
					// would not be happy if there is a compilation while it processes the step.
					// Otherwise execution gets really messy. See 11685123.
					if (EditorApplication.isCompiling)
					{
						ThrowScriptCompilationDetectedAfterProcessingBuildStep();
					}
				}

				// AssetDatabase Save and Refresh after the Step
				{
					// Save the unsaved assets before making any moves. Make sure everything is imported.
					// This may trigger an assembly reload if there are script modifications. See 11658912.
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

					// Check if AssetDatabase.Refresh triggered a compilation
					// OR the user requested an assembly reload.
					{
						var isCompiling = EditorApplication.isCompiling;
						if (isCompiling || job.IsAssemblyReloadScheduled)
						{
							HaltStep(job, $"After step - Compiling: {isCompiling} Scheduled: {job.IsAssemblyReloadScheduled}");
							SaveRunningJobToFile(job);
							yield break;
						}
					}
				}

				yield return null; // As a precaution, won't hurt to wait for one frame for all things to settle down.
			}
		}

		private static IEnumerator RunStep(BuildJob job)
		{
			// Quick access references. These will not ever change during the build run.
			// Do not add variables like 'currentPhase' here.
			var Builder = job.Builder;
			var BuildPhases = job.Plan.BuildPhases;

			EditorApplicationTools.EnsureNotCompiling(false);

			// Figure out which should be next Step to be executed. If there is none left, Finalize the Build Run.
			var completed = false;
			try
			{
				if (!job.Finalizing)
				{
					if (!job.IsPreviousBuildStepAssigned)
					{
						// Current Builder is just getting started. Proceed to first Build Step.
						job.CurrentBuildStep = GetFirstStep(false);
					}
					else
					{
						// Continuing from a Build Step of an ongoing Build Run. See if there is a next Build Step.
						var nextStep = GetNextStep(job.PreviousBuildStep, false);
						if (!string.IsNullOrEmpty(nextStep))
						{
							// Proceed to next Build Step.
							job.CurrentBuildStep = nextStep;
						}
						else
						{
							// Finished all Build Steps. Proceed to Finalization.
							job.CurrentBuildStep = "";
							job.PreviousBuildStep = "";
							job.Finalizing = true;
						}
					}
				}

				// Note that 'Finalizing' may have changed above if the execution reaches the last Build Step.
				if (job.Finalizing)
				{
					job.CurrentBuildStep = ""; // Make sure Build Steps are reset.
					job.PreviousBuildStep = "";

					if (!job.IsPreviousFinalizationStepAssigned)
					{
						// Current Builder is just getting started. Proceed to first Finalization Step.
						job.CurrentFinalizationStep = GetFirstStep(true);
					}
					else
					{
						// Continuing from a Finalization Step of an ongoing run. See if there is a next Finalization Step.
						var nextStep = GetNextStep(job.PreviousFinalizationStep, true);
						if (!string.IsNullOrEmpty(nextStep))
						{
							// Proceed to next Finalization Step.
							job.CurrentFinalizationStep = nextStep;
						}
						else
						{
							// Finished all Finalization Steps of Builder.
							// Check if the Build Run was failed. If so, and with all Finalization Steps
							// are executed, it is time to end the Build Run.
							job.CurrentFinalizationStep = "";
							job.PreviousFinalizationStep = "";
							job.Finalizing = false;
							Debug.Assert(job.Result != BuildJobResult.Succeeded);
							if (job.Result == BuildJobResult.Failed)
							{
								DoBuildRunFinalization(job);
								completed = true;
							}
							else
							{
								// See if there is a next Phase.
								if (!job.IsLastPhase)
								{
									// Proceed to next Phase. The RunStep will be run again and it will start
									// from the first Build Step of next Phase's first Builder.
									job.CurrentPhase++;
									SaveRunningJobToFile(job);
									completed = true;
								}
								else
								{
									// All Phases are completed. That means Build Run is completed.
									job.CurrentPhase = -2;
									job.CurrentFinalizationStep = "";
									job.PreviousFinalizationStep = "";
									job.SetResult(BuildJobResult.Succeeded);
									DoBuildRunFinalization(job);
									completed = true;
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
				SaveRunningJobToFile(job);
			}

			Debug.Assert(job._CurrentStepInfoCached.Method != null);
			var currentStep = job._CurrentStepInfoCached.Name;
			Debug.Assert(!string.IsNullOrEmpty(currentStep));

			// Run the Step
			yield return null; // As a precaution, won't hurt to wait for one frame for all things to settle down.
			{
				{
					var now = Now;
					var totalElapsed = now - job.StartTime;
					job.LastStepStartTime = now;
					Log.Info($"{totalElapsed.ToStringHoursMinutesSecondsMilliseconds()} | Started build step '{currentStep}'");
				}

				var currentStepInfo = job._CurrentStepInfoCached;
				job._CurrentStepInfoCached = BuildStepInfo.Empty;
				var enumerator = (IEnumerator)currentStepInfo.Method.Invoke(Builder, new object[] { job, currentStepInfo }); // See 113654126.
				yield return EditorCoroutineUtility.StartCoroutineOwnerless(enumerator);

				{
					var now = Now;
					var stepDuration = now - job.LastStepStartTime;
					Log.Info($"Build step '{currentStep}' took {stepDuration.ToStringHoursMinutesSecondsMilliseconds()}.");
					job.LastStepStartTime = default;
				}
			}
			yield return null; // As a precaution, won't hurt to wait for one frame for all things to settle down.

			#region Get Step

			string GetFirstStep(bool finalization)
			{
				if (!BuildPhases.IsInRange(job.CurrentPhase))
					throw new BuildMachineException($"Index out of range. Phase {job.CurrentPhase}/{BuildPhases.Length}");

				var phase = BuildPhases[job.CurrentPhase];
				var builder = Builder;
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
					throw new BuildMachineException("The behaviour of not finding the first step is not implemented yet!");
				}

				Debug.Assert(job._CurrentStepInfoCached.Method == null);
				job._CurrentStepInfoCached = firstStepOfCurrentPhase;
				return firstStepOfCurrentPhase.Name;
			}

			string GetNextStep(string previousStep, bool finalization)
			{
				Debug.Assert(!string.IsNullOrEmpty(previousStep));
				if (!BuildPhases.IsInRange(job.CurrentPhase))
					throw new BuildMachineException($"Index out of range. Phase {job.CurrentPhase}/{BuildPhases.Length}");

				var phase = BuildPhases[job.CurrentPhase];
				var builder = Builder;
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
					throw new BuildMachineException("The behaviour of not finding any steps is not implemented yet!");
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
					throw new BuildMachineException($"Failed to find previous step '{previousStep}'.");
				}

				// Check if this is the last Step.
				if (foundIndex >= allStepsOfCurrentPhase.Count - 1)
				{
					// There is no next Step.
					return "";
				}

				// Get the next Step.
				Debug.Assert(job._CurrentStepInfoCached.Method == null);
				job._CurrentStepInfoCached = allStepsOfCurrentPhase[foundIndex + 1];
				return allStepsOfCurrentPhase[foundIndex + 1].Name;
			}

			#endregion
		}

		#endregion

		#region Halt

		private static void HaltStep(BuildJob job, string description)
		{
			job.StepState = BuildJobStepState.StepHalt;
			job.LastHaltTime = Now;

			if (job.IsAssemblyReloadScheduled)
			{
				EditorUtilityTools.RequestScriptReload();
				job.IsAssemblyReloadScheduled = false;
			}

			Log.Info($"Halting the execution until next assembly reload ({description}).");
		}

		#endregion

		#region Exception Handling

		private static void ThrowScriptCompilationDetectedBeforeStartingTheBuildRun()
		{
			throw new BuildMachineException("Compilation is not allowed at the start of a build run or when continuing the build run.");
		}

		private static void ThrowScriptCompilationDetectedBeforeProcessingBuildStep()
		{
			throw new BuildMachineException("Compilation is not allowed before starting the build step.");
		}

		/*
		This is not used anymore but kept for documentation purposes. The EditorApplication.LockReloadAssemblies()
		method was not working properly in previous versions of Unity. So the workaround was to throw an exception when
		a script compilation is detected. Lock mechanism is now working properly so this workaround is not needed anymore.
		See 11685123.
 
		private static void ThrowScriptCompilationDetectedWhileProcessingBuildStep()
		{
			// This message is expected to be shown to the coder that tries to write a Build Step but accidentally
			// triggers recompilation. So the description is a bit more detailed than other exceptions, where other
			// exceptions are more like internal errors in Build Machine.
			throw new BuildFailedException("Triggering a script compilation is not allowed while processing the build step. " +
			                               $"Current state is {RunningJob.ToStringCurrentPhaseBuilderAndStep()}. " +
			                               "Make sure the codes in the step won't trigger a compilation like calling " +
			                               "AssetDatabase.Refresh() or switching the active platform. " +
			                               "Any changes that requires a compilation like script modifications, " +
			                               "project settings modifications etc. will be automatically handled by " +
			                               $"{nameof(BuildMachine)} when proceeding to next build step.");
		}
		*/

		private static void ThrowScriptCompilationDetectedAfterProcessingBuildStep()
		{
			throw new BuildMachineException("Compilation is not allowed after finishing the build step.");
		}

		#endregion

		#region Checks Before Run

		private static void ChecksBeforeRun(BuildJob job)
		{
			//-------- Note to the developers ---------------------------------- 
			// We don't want to alter the machine's preferences automatically.
			// Otherwise it would be a bit confusing for the user. So we just
			// warn the user about the situation and let them decide what to do.
			// -----------------------------------------------------------------

			// Check for AssetDatabase ActiveOnDemandMode
			{
#if !DisableExtenityBuilderActiveOnDemandModeCheck
				if (AssetDatabaseExperimental.ActiveOnDemandMode != AssetDatabaseExperimental.OnDemandMode.Off)
				{
					throw new BuildMachineException("AssetDatabase ActiveOnDemandMode was set to " +
					                                $"'{AssetDatabaseExperimental.ActiveOnDemandMode}' which makes " +
					                                "the build process undeterministic. Please set it to " +
					                                $"'{AssetDatabaseExperimental.OnDemandMode.Off}' before starting " +
					                                "the build.");
				}
#endif
			}

			// Check if Unity's Auto Refresh option is disabled.
			{
				if (EditorPreferences.AutoRefresh.GetValueIfSavedBefore(out var autoRefresh))
				{
					if (autoRefresh != AssetPipelineAutoRefreshMode.Disabled)
					{
						throw new BuildMachineException("Detected that Unity's Auto Refresh option is not disabled. " +
						                                "Please go into 'Edit>Preferences>Asset Pipeline>Auto Refresh' " +
						                                "and set its value to 'Disabled' to prevent Unity from starting " +
						                                "asset refresh operation in the middle of build steps.");
					}
				}
				else
				{
					throw new BuildMachineException("Failed to detect Unity's Auto Refresh option because it wasn't " +
					                                "manually configured by hand yet. Please go into " +
					                                "'Edit>Preferences>Asset Pipeline>Auto Refresh' and just change its " +
					                                "value to something else. That will force Unity to save its value " +
					                                "which then Build Machine can read it. Then set its value to " +
					                                "'Disabled' to prevent Unity from starting asset refresh operation " +
					                                "in the middle of build steps.");
				}
			}

			// Check if Unity's active platform is the same as the one we are building for.
			{
#if !DisableExtenityBuilderActivePlatformCheck
				var buildTarget = job.Builder.Info.BuildTarget;
				if (EditorUserBuildSettings.activeBuildTarget != buildTarget)
				{
					throw new BuildMachineException("Detected that Unity's active platform is not the same as the one " +
					                                "we are building for. Please go into 'File>Build Settings' and " +
					                                $"change the active platform to '{buildTarget}'.");
				}
#endif
			}

			// Check if script compilation code optimization mode is Release.
			{
#if !DisableExtenityBuilderCodeOptimizationCheck
				if (CompilationPipeline.codeOptimization != CodeOptimization.Release)
				{
					throw new BuildMachineException("Detected that script compilation code optimization mode is not " +
					                                "'Release'. Please go into 'Edit>Preferences>General>" +
					                                "Code Optimization On Startup' and change it to 'Release'.");
				}
#endif
			}
		}

		#endregion

		#region Build Run Finalization

		private static void DoBuildRunFinalization(BuildJob job)
		{
			Log.Info($"Finalizing the '{job.Result}' build job.");
			EditorApplicationTools.EnsureNotCompiling(false);

			job.OverallState = BuildJobOverallState.JobFinished;

			if (job.Result == BuildJobResult.Succeeded)
			{
				Log.Info($"Build '{job.NameSafe()}' succeeded.");
			}
			else
			{
				Log.Error($"Build '{job.NameSafe()}' failed. See the log for details.");
			}

			BuildJobResult result = job.Result;
			bool isSetToQuitInBatchMode = job.IsSetToQuitInBatchMode;

			UnsetRunningJobState();
			DeleteRunningJobFile();

			// Close the editor in batch mode OR let the Editor live.
			// Note that asset database refresh is a heavy operation
			// and will only be done if Editor will continue to run.
			if (ApplicationTools.IsBatchMode && isSetToQuitInBatchMode)
			{
				var exitCode = result == BuildJobResult.Succeeded ? 0 : -1;
				EditorApplication.Exit(exitCode);
			}
			else
			{
				AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
			}
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

		internal static void SaveRunningJobToFile(BuildJob job)
		{
			Log.Info("Saving running job for assembly reload survival.");

			//if (IsRunningJobFileExists())
			//{
			//	throw new BuildFailedException("Running job survival file is expected not to exist, yet it does.");
			//}

			var content = job.SerializeToJson();

			AssetDatabase.ReleaseCachedFileHandles(); // Make Unity release the files to prevent any IO errors.

			DirectoryTools.CreateFromFilePath(BuildMachineConstants.RunningJobSurvivalFilePath);
			File.WriteAllText(BuildMachineConstants.RunningJobSurvivalFilePath, content);

			// Save a copy for logging purposes.
			{
				var directoryPath = string.Format(BuildMachineConstants.RunningJobSurvivalFileLogDirectoryFormat, job.ID);
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
					throw new BuildMachineException("Running job assembly reload survival file was empty.");
				}

				var job = BuildJob.DeserializeFromJson(content);
				if (job == null)
				{
					Log.Error("Deserialization failed. Running job content was:\n" + content);
					throw new BuildMachineException("Failed to deserialize running job.");
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
			try
			{
				FileTools.Delete(BuildMachineConstants.RunningJobSurvivalFilePath);
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

		#region Log

		private static readonly Logger Log = new("Builder");

		#endregion
	}

}
