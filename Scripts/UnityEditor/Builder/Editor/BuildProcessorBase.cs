using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Extenity.DataToolbox;
using Extenity.GameObjectToolbox;
using Extenity.GameObjectToolbox.Editor;
using Extenity.ParallelToolbox;
using Extenity.ParallelToolbox.Editor;
using Extenity.SceneManagementToolbox.Editor;
using Extenity.UnityEditorToolbox.Editor;
using Extenity.UnityEditorToolbox.ImageMagick;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace Extenity.UnityEditorToolbox
{

	public abstract class BuildProcessorBase<TBuildProcessor> : IPreprocessBuild, IPostprocessBuild, IProcessScene
		where TBuildProcessor : BuildProcessorBase<TBuildProcessor>
	{
		#region Configuration

		public abstract string BuildProcessorName { get; }
		public abstract int callbackOrder { get; }
		public abstract BuildProcessorSceneDefinition[] Scenes { get; }
		public abstract Dictionary<string, BuildProcessConfiguration> Configurations { get; }

		#endregion

		#region Process

		public static bool IsProcessorRunning { get; private set; }
		public static int CurrentStep { get; private set; }
		public static Stopwatch ProcessStopwatch { get; private set; }
		public static TimeSpan PreviousStepStartTime { get; private set; }
		public static string PreviousStepTitle { get; private set; }
		private static CoroutineTask Task;

		protected abstract IEnumerator OnProcessActiveScene(BuildProcessorSceneDefinition definition, BuildProcessConfiguration configuration, bool runAsync);

		public static void ProcessScene(Scene scene, string configurationName, bool askUserForUnsavedChanges)
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode)
			{
				throw new Exception("Tried to start scene processing while in play mode.");
			}
			if (askUserForUnsavedChanges)
			{
				EditorSceneManagerTools.EnforceUserToSaveAllModifiedScenes("First you need to save the scene before processing.");
			}
			var processorInstance = (TBuildProcessor)Activator.CreateInstance(typeof(TBuildProcessor));
			Task = CoroutineTask.Create(processorInstance.DoProcessScene(scene, configurationName, true, true), false);
			Task.StartInEditorUpdate(true, true, null);
		}

		private IEnumerator DoProcessScene(Scene scene, string configurationName, bool isLaunchedByUser, bool runAsync)
		{
			if (IsProcessorRunning)
				throw new Exception("Scene processor was already running.");
			IsProcessorRunning = true;

			try
			{
				// Get scene definition
				var scenePath = scene.path;
				var definition = Scenes.FirstOrDefault(item =>
					item.ProcessedScenePath.Equals(scenePath, StringComparison.InvariantCultureIgnoreCase) ||
					item.MainScenePath.Equals(scenePath, StringComparison.InvariantCultureIgnoreCase) ||
					item.MergedScenePaths.Any(mergedScenePath => mergedScenePath.Equals(scenePath, StringComparison.InvariantCultureIgnoreCase))
				);
				if (definition == null)
				{
					Debug.LogFormat("Skipping scene processing for scene '{0}'.", scene.name);
					yield break;
				}

				// Get process configuration
				BuildProcessConfiguration configuration;
				if (!Configurations.TryGetValue(configurationName, out configuration))
				{
					throw new Exception(string.Format("Configuration '{0}' does not exist.", configurationName));
				}

				// See if we need to process the scene
				if (configuration.NeedsProcessing(isLaunchedByUser))
				{
					Debug.Log("Processing scene at path: " + scenePath);
				}
				else
				{
					Debug.Log("Skipping scene at path: " + scenePath);
					yield break;
				}

				CurrentStep = 1;
				ProcessStopwatch = new Stopwatch();
				ProcessStopwatch.Start();
				PreviousStepStartTime = new TimeSpan();
				PreviousStepTitle = null;

				// Copy main scene to processed scene path
				{
					// Open the main scene, or reopen if already opened. This will head us to a clean start.
					EditorSceneManager.OpenScene(definition.MainScenePath, OpenSceneMode.Single);

					// Copy the main scene to processed scene path and load the processed scene which we will need when merging scenes.
					var activeScene = EditorSceneManager.GetActiveScene();
					var result = EditorSceneManager.SaveScene(activeScene, definition.ProcessedScenePath, false);
					if (!result)
					{
						throw new Exception("Could not copy main scene to processed scene path.");
					}
				}

				// Disable automatic lightmap baking in processed scene
				{
					DisableAutomaticLightmapBakingForActiveScene();
				}

				// Merge scenes into processed scene
				{
					// Processed scene should already be loaded by now.
					var processingScene = EditorSceneManager.GetSceneByPath(definition.ProcessedScenePath);
					if (!processingScene.IsValid())
					{
						throw new Exception(string.Format("Processing scene could not be found at path '{0}'.", definition.ProcessedScenePath));
					}

					// Merge other scenes into processing scene.
					foreach (var mergedScenePath in definition.MergedScenePaths)
					{
						if (!EditorSceneManagerTools.IsSceneExistsAtPath(mergedScenePath))
						{
							throw new Exception(string.Format("Merged scene could not be found at path '{0}'.", mergedScenePath));
						}

						// Load merging scene additively. It will automatically unload when merging is done, which will leave processed scene as the only loaded scene.
						var mergedScene = EditorSceneManager.OpenScene(mergedScenePath, OpenSceneMode.Additive);
						EditorSceneManager.MergeScenes(mergedScene, processingScene);
					}

					// Save processed scene
					EditorSceneManager.SaveOpenScenes();
				}

				yield return Task.StartNested(OnProcessActiveScene(definition, configuration, runAsync));

				var previousStepDuration = ProcessStopwatch.Elapsed - PreviousStepStartTime;
				Debug.LogFormat("Step {0} took {1}.", CurrentStep - 1, previousStepDuration.ToStringHoursMinutesSecondsMilliseconds());

				// Hack: This is needed to save the scene after lightmap settings change.
				// For some reason, we need to wait one more frame or the scene would get
				// marked as unsaved.
				DisplayProgressBar("Finalizing Scene Processor", "Saving scene");
				yield return null;
				AggressivelySaveOpenScenes();

				Debug.LogFormat("{0} | Scene processor finished.", ProcessStopwatch.Elapsed.ToStringHoursMinutesSecondsMilliseconds());

				ClearProgressBar();
			}
			//catch (Exception e)
			//{
			//	Debug.LogException(e);
			//}
			finally
			{
				IsProcessorRunning = false;
				ProcessStopwatch = null;
				PreviousStepStartTime = new TimeSpan();
				PreviousStepTitle = null;
				CurrentStep = 0;
			}
		}

		#endregion

		#region Build Preprocessor / Build Postprocessor / Scene Processor

		public void OnPreprocessBuild(BuildTarget target, string path)
		{
			Debug.LogFormat("Build processor '{0}' is checking up in build preprocess...", BuildProcessorName);

			//EditorSceneManagerTools.EnforceUserToSaveAllModifiedScenes("First you need to save the scene before building."); Disabled because it causes an internal Unity error at build time.
		}

		public void OnPostprocessBuild(BuildTarget target, string path)
		{
			//Debug.Log("Cleaning up in build postprocess...");
		}

		public void OnProcessScene(Scene scene)
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode)
				return;

			// TODO:
			Debug.LogWarning("Launching build processor on build time is not implemented!");
			//DoProcessScene(scene, false).;
		}

		#endregion

		#region Scene File Operations

		/*
		private static string GetBackupScenePath(string scenePath)
		{
			if (string.IsNullOrEmpty(scenePath))
				throw new ArgumentNullException();

			var sceneName = Path.GetFileNameWithoutExtension(scenePath);
			if (!sceneName.EndsWith(BackupScenePostfix))
			{
				var directoryPath = Path.GetDirectoryName(scenePath);
				var backupSceneName = sceneName + BackupScenePostfix;
				return Path.Combine(directoryPath, backupSceneName).FixDirectorySeparatorChars('/') + Path.GetExtension(scenePath);
			}
			return scenePath;
		}

		private static string GetOriginalScenePath(string scenePath)
		{
			if (string.IsNullOrEmpty(scenePath))
				throw new ArgumentNullException();

			var sceneName = Path.GetFileNameWithoutExtension(scenePath);
			if (sceneName.EndsWith(BackupScenePostfix))
			{
				var directoryPath = Path.GetDirectoryName(scenePath);
				var originalSceneName = sceneName.Substring(0, sceneName.Length - BackupScenePostfix.Length);
				return Path.Combine(directoryPath, originalSceneName).FixDirectorySeparatorChars('/') + Path.GetExtension(scenePath);
			}
			return scenePath;
		}

		private static void BackupOriginalScene()
		{
			var scene = EditorSceneManager.GetActiveScene();
			var originalScenePath = GetOriginalScenePath(scene.path);
			var backupScenePath = GetBackupScenePath(scene.path);
			AssetTools.CreateOrReplaceScene(originalScenePath, backupScenePath);
		}

		private static void RevertToBackupScene()
		{
			var scene = EditorSceneManager.GetActiveScene();
			var originalScenePath = GetOriginalScenePath(scene.path);
			var backupScenePath = GetBackupScenePath(scene.path);
			AssetTools.CreateOrReplaceScene(backupScenePath, originalScenePath);
		}

		private static void LoadOriginalScene()
		{
			var scene = EditorSceneManager.GetActiveScene();
			var originalScenePath = GetOriginalScenePath(scene.path);
			EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
		}
		*/

		#endregion

		#region Lightmap

		protected void DisableAutomaticLightmapBakingForActiveScene()
		{
			Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;
		}

		protected void ApplyLightmapConfigurationToActiveScene(LightingBuildConfiguration configuration)
		{
			Lightmapping.bakedGI = configuration.BakedGlobalIlluminationEnabled;
			Lightmapping.realtimeGI = configuration.RealtimeGlobalIlluminationEnabled;
			LightmapEditorSettings.textureCompression = configuration.CompressLightmaps;
			LightmapEditorSettings.enableAmbientOcclusion = configuration.AmbientOcclusion;
			LightmapEditorSettingsTools.SetDirectSamples(configuration.DirectSamples);
			LightmapEditorSettingsTools.SetIndirectSamples(configuration.IndirectSamples);
			LightmapEditorSettingsTools.SetBounces(configuration.Bounces);
		}

		#endregion

		#region Progress Bar

		private static string ProgressBarTitle;
		private static string ProgressBarMessage;

		protected static void DisplayProgressBar(string title, string message, float progress = 0f)
		{
			ProgressBarTitle = title;
			ProgressBarMessage = message;
			UpdateProgressBar(progress);
		}

		protected static void UpdateProgressBar(float progress)
		{
			EditorUtility.DisplayProgressBar(ProgressBarTitle, ProgressBarMessage, progress);
		}

		protected static void ClearProgressBar()
		{
			ProgressBarTitle = null;
			ProgressBarMessage = null;
			EditorUtility.ClearProgressBar();
		}

		#endregion

		#region Tools

		protected static bool StartStep(string title, bool isAllowed = true)
		{
			var now = ProcessStopwatch.Elapsed;
			var isFirstStep = CurrentStep == 1;
			var skippedText = isAllowed ? "" : "SKIPPED ";

			if (isFirstStep)
			{
				Debug.LogFormat("{0} | {1}Scene Processor Step {2} - {3}", now.ToStringHoursMinutesSecondsMilliseconds(), skippedText, CurrentStep, title);
			}
			else
			{
				var previousStepDuration = now - PreviousStepStartTime;
				Debug.LogFormat("Step '{0}' took {1}.", PreviousStepTitle, previousStepDuration.ToStringHoursMinutesSecondsMilliseconds());
				Debug.LogFormat("{0} | {1}Scene Processor Step {2} - {3}", now.ToStringHoursMinutesSecondsMilliseconds(), skippedText, CurrentStep, title);
				DisplayProgressBar("Scene Processor Step " + CurrentStep, title);
			}

			PreviousStepStartTime = now;
			PreviousStepTitle = title;
			CurrentStep++;
			return isAllowed;
		}

		protected static void AggressivelySaveOpenScenes()
		{
			EditorSceneManager.MarkAllScenesDirty();
			EditorSceneManager.SaveOpenScenes();
			AssetDatabase.SaveAssets();
		}

		protected static void DeleteAllGameObjectsContainingComponentInActiveScene<T>() where T : Component
		{
			EditorGameObjectTools.DeleteAllGameObjectsContainingComponentInActiveScene<T>(true, true);
		}

		protected void DeparentAllStaticObjectsContainingComponent<T>() where T : Component
		{
			GameObjectTools.SetParentOfAllObjectsContainingComponent<T>(null, true);
		}

		protected static void DeleteEmptyUnreferencedGameObjectsInActiveScene()
		{
			EditorGameObjectTools.DeleteEmptyUnreferencedGameObjectsInActiveScene(true, true);
		}

		protected static void DeleteAllDisabledStaticMeshRenderersInActiveScene()
		{
			EditorGameObjectTools.DeleteAllDisabledStaticMeshRenderersInActiveScene(true, true);
		}

		public static void BlurReflectionProbesOfActiveScene()
		{
			ImageMagickCommander.BlurReflectionProbesOfActiveScene();
		}

		#endregion
	}

}
