using System;
using System.Collections;
using System.IO;
using System.Linq;
using Extenity.DataToolbox;
using Extenity.FileSystemToolbox;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using Debug = UnityEngine.Debug;

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

			Log.Info($"Build '{RunningJob.Plan.Name}' started.");
			Debug.Assert(RunningJob.IsJustCreated);
			Debug.Assert(RunningJob.CurrentPhase == -1);
			Debug.Assert(RunningJob.CurrentBuilder == -1);
			Debug.Assert(!RunningJob.IsPreviousStepAssigned);
			Debug.Assert(!RunningJob.IsCurrentStepAssigned);
			RunningJob.CurrentPhase = 0;
			RunningJob.CurrentBuilder = 0;
			RunningJob.StartTime = Now;

			EditorCoroutineUtility.StartCoroutineOwnerless(Run());
		}

		private static void Continue(BuildJob job)
		{
			if (IsRunning)
			{
				throw new Exception("Tried to continue a build job while there is already a running one.");
			}
			RunningJob = job;

			if (RunningJob.IsCurrentStepAssigned)
			{
				// See 11917631.
				RunningJob = null;
				throw new Exception($"Build job '{job.Plan.Name}' was disrupted in the middle for some reason. It could happen if Editor crashes during build, if not happened for an unexpected reason.");
			}

			Log.Info($"Continuing the build '{RunningJob.Plan.Name}' at phase '{RunningJob.ToStringCurrentPhase()}' and builder '{RunningJob.ToStringCurrentBuilder()}' with previously processed step '{RunningJob.PreviousStep}'.");

			EditorCoroutineUtility.StartCoroutineOwnerless(Run());
		}

		#endregion

		#region Run

		private static IEnumerator Run()
		{
			// Quick access references. These will not ever change during the build run.
			// Do not add variables like 'currentPhase' here.
			var Job = RunningJob;


			while (IsRunning)
			{
				// Check before running the step
				{
					var haltExecution = CheckBeforeStep();
					if (haltExecution)
					{
						RunningJob.LastHaltTime = Now;
						Log.Info("Halting the execution until next assembly reload.");
						yield break;
					}
					else
					{
						RunningJob.LastHaltTime = default;
					}
				}

				// Run the step
				yield return RunStep();

				// Check after running the step
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

			// Find the next step to be processed. If there is none left, finalize the build run.
			try
			{
				if (!Job.IsPreviousStepAssigned)
				{
					// Current builder is just getting started. Proceed to first step.
					Job.CurrentStep = GetFirstStep();
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
						Job.PreviousStep = "";
						if (!Job.IsLastBuilder)
						{
							// Proceed to next builder and start from its first step.
							Job.CurrentBuilder++;
							Job.CurrentStep = GetFirstStep();
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
							}
							else
							{
								// All phases are complete.
								Job.CurrentBuilder = -2;
								Job.CurrentPhase = -2;
								Job.CurrentStep = "";
								Job._CurrentStepCached = BuildStepInfo.Empty;

								DoBuildRunFinalization();
								yield break;
							}
						}
					}
				}
			}
			catch (Exception exception)
			{
				throw new InternalException(1121821, exception);
			}
			// Save current state just after determining the current step. So the next time
			// the survival file is reloaded, we would have the opportunity to check if there is
			// a CurrentStep specified in it, which is unexpected and means something went wrong
			// in the middle of step execution. See 11917631.
			SaveRunningJobToFile();

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

				var enumerator = (IEnumerator)Job._CurrentStepCached.Method.Invoke(currentBuilder, new object[] { Job._CurrentStepCached });
				yield return EditorCoroutineUtility.StartCoroutineOwnerless(enumerator);

				EndStep(currentStep);
			}
			yield return null; // As a precaution, won't hurt to wait for one frame for all things to settle down.

			#region Get Step

			string GetFirstStep()
			{
				var currentPhase = BuildPhases[Job.CurrentPhase];
				var currentBuilder = Builders[Job.CurrentBuilder];
				var firstStepOfCurrentPhase = currentBuilder.Info.Steps.FirstOrDefault(entry => currentPhase.IncludedSteps.Contains(entry.Type));

				// TODO: Check if the step exists. What to do if it does not exist?

				Job._CurrentStepCached = firstStepOfCurrentPhase;
				return firstStepOfCurrentPhase.Name;
			}

			string GetNextStep(string previousStep)
			{
				Debug.Assert(!string.IsNullOrEmpty(previousStep));

				var currentPhase = BuildPhases[Job.CurrentPhase];
				var currentBuilder = Builders[Job.CurrentBuilder];
				var allStepsOfCurrentPhase = currentBuilder.Info.Steps.Where(entry => currentPhase.IncludedSteps.Contains(entry.Type)).ToList();

				// TODO: Check if any step exists. What to do if none exist?

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

		#region Check Before Step

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
					SaveRunningJobToFile();
				}
			}

			return haltExecution;
		}

		#endregion

		#region Build Run Finalization

		private static void DoBuildRunFinalization()
		{
			Log.Info("Finalizing the build job.");

			// TODO: Finalization. Do version increment, call the virtual finalization method of job, etc.

			Log.Info($"Build '{RunningJob.Plan.Name}' succeeded.");
			RunningJob = null;
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
