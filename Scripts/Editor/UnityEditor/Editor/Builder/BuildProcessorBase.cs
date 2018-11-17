using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Extenity.ApplicationToolbox;
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
#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;
#endif
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace Extenity.UnityEditorToolbox
{

	public abstract class BuildProcessorBase<TBuildProcessor> :
#if UNITY_2018_1_OR_NEWER
		IPreprocessBuildWithReport,
		IPostprocessBuildWithReport,
		IProcessSceneWithReport
#else
		IPreprocessBuild, 
		IPostprocessBuild, 
		IProcessScene
#endif
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

		public delegate void ProcessFinishedAction(bool succeeded);
		public static event ProcessFinishedAction OnProcessFinished;

		protected abstract IEnumerator OnBeforeProcess(BuildProcessorSceneDefinition definition, BuildProcessConfiguration configuration, bool runAsync);
		protected abstract IEnumerator OnAfterProcess(BuildProcessorSceneDefinition definition, BuildProcessConfiguration configuration, bool runAsync);

		public static void ProcessScene(Scene scene, string configurationName, bool askUserForUnsavedChanges, ProcessFinishedAction onProcessFinished = null)
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode)
			{
				throw new Exception("Tried to start scene processing while in play mode.");
			}
			if (askUserForUnsavedChanges)
			{
				EditorSceneManagerTools.EnforceUserToSaveAllModifiedScenes("First you need to save the scene before processing.");
			}
			OnProcessFinished += onProcessFinished;
			var processorInstance = (TBuildProcessor)Activator.CreateInstance(typeof(TBuildProcessor));
			Task = CoroutineTask.Create(processorInstance.DoProcessScene(scene, configurationName, true, true), false);
			Task.StartInEditorUpdate(true, true, null);
		}

		private IEnumerator DoProcessScene(Scene scene, string configurationName, bool isLaunchedByUser, bool runAsync)
		{
			if (IsProcessorRunning)
				throw new Exception("Scene processor was already running.");
			IsProcessorRunning = true;

			var succeeded = false;

			try
			{
				// Get scene definition
				var scenePath = scene.path;
				var definition = Scenes.FirstOrDefault(item =>
					item.ProcessedScenePath.Equals(scenePath, StringComparison.InvariantCultureIgnoreCase) ||
					item.MainScenePath.Equals(scenePath, StringComparison.InvariantCultureIgnoreCase) ||
					(item.MergedScenePaths != null && item.MergedScenePaths.Any(mergedScenePath => mergedScenePath.Equals(scenePath, StringComparison.InvariantCultureIgnoreCase)))
				);
				if (definition == null)
				{
					Log.Info($"Skipping scene processing for scene '{scene.name}'.");
					yield break;
				}

				// Get process configuration
				BuildProcessConfiguration configuration;
				if (!Configurations.TryGetValue(configurationName, out configuration))
				{
					throw new Exception($"Configuration '{configurationName}' does not exist.");
				}

				// See if we need to process the scene
				if (configuration.NeedsProcessing(isLaunchedByUser))
				{
					Log.Info($"Processing configuration '{configurationName}' on scene at path: {scenePath}");
				}
				else
				{
					Log.Info($"Skipping configuration '{configurationName}' on scene at path: {scenePath}");
					yield break;
				}

				CurrentStep = 1;
				ProcessStopwatch = new Stopwatch();
				ProcessStopwatch.Start();
				PreviousStepStartTime = new TimeSpan();
				PreviousStepTitle = null;

				if (!configuration.DontLoadAndMergeScenes)
				{
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
							throw new Exception($"Processing scene could not be found at path '{definition.ProcessedScenePath}'.");
						}

						// Merge other scenes into processing scene.
						if (definition.MergedScenePaths != null)
						{
							foreach (var mergedScenePath in definition.MergedScenePaths)
							{
								if (!EditorSceneManagerTools.IsSceneExistsAtPath(mergedScenePath))
								{
									throw new Exception($"Merged scene could not be found at path '{mergedScenePath}'.");
								}

								// Load merging scene additively. It will automatically unload when merging is done, which will leave processed scene as the only loaded scene.
								var mergedScene = EditorSceneManager.OpenScene(mergedScenePath, OpenSceneMode.Additive);
								EditorSceneManager.MergeScenes(mergedScene, processingScene);
							}
						}

						// Save processed scene
						EditorSceneManager.SaveOpenScenes();
					}
				}

				Log.Info("Scene is ready to be processed. Starting the process.");

				// Call initialization process
				yield return Task.StartNested(OnBeforeProcess(definition, configuration, runAsync));

				// Call custom processors
				{
					var category = string.IsNullOrEmpty(configuration.Category)
						? "Default"
						: configuration.Category;
					var methods = CollectProcessorMethods(category);
					foreach (var method in methods)
					{
						var enumerator = (IEnumerator)method.Invoke(this, new object[] { definition, configuration, runAsync });
						yield return Task.StartNested(enumerator);
					}
				}

				// Call finalization process
				yield return Task.StartNested(OnAfterProcess(definition, configuration, runAsync));

				var previousStepDuration = ProcessStopwatch.Elapsed - PreviousStepStartTime;
				if (CurrentStep - 1 > 0)
				{
					Log.Info($"Step {CurrentStep - 1} took {previousStepDuration.ToStringHoursMinutesSecondsMilliseconds()}.");
				}

				// Hack: This is needed to save the scene after lightmap settings change.
				// For some reason, we need to wait one more frame or the scene would get
				// marked as unsaved.
				DisplayProgressBar("Finalizing Scene Processor", "Saving scene");
				yield return null;
				AggressivelySaveOpenScenes();

				succeeded = true;
				Log.Info($"{ProcessStopwatch.Elapsed.ToStringHoursMinutesSecondsMilliseconds()} | Scene processor finished.");

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

			if (OnProcessFinished != null)
			{
				OnProcessFinished(succeeded);
			}
		}

		#endregion

		#region Processor Methods

		public static List<MethodInfo> CollectProcessorMethods(string category)
		{
			if (string.IsNullOrEmpty(category))
				throw new ArgumentNullException(nameof(category));

			var methods = typeof(TBuildProcessor)
				.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(method =>
					{
						if (!method.IsVirtual && method.ReturnType == typeof(IEnumerator))
						{
							var parameters = method.GetParameters();
							if (parameters.Length == 3 &&
								parameters[0].ParameterType == typeof(BuildProcessorSceneDefinition) &&
								parameters[1].ParameterType == typeof(BuildProcessConfiguration) &&
								parameters[2].ParameterType == typeof(bool)
							)
							{
								var attribute = method.GetAttribute<ProcessorAttribute>(true);
								if (attribute != null && attribute.Categories.IsNotNullAndEmpty())
								{
									return attribute.Categories.Contains(category);
								}
							}
						}
						return false;
					}
				)
				.OrderBy(method => method.GetAttribute<ProcessorAttribute>(true).Order)
				.ToList();

			//methods.LogList();

			// Check for duplicate Order values. Note that we already ordered it above.
			if (methods.Count > 1)
			{
				var detected = false;
				var previousMethod = methods[0];
				var previousMethodOrder = previousMethod.GetAttribute<ProcessorAttribute>(true).Order;
				for (int i = 1; i < methods.Count; i++)
				{
					var currentMethod = methods[i];
					var currentMethodOrder = currentMethod.GetAttribute<ProcessorAttribute>(true).Order;

					if (previousMethodOrder == currentMethodOrder)
					{
						detected = true;
						Debug.LogErrorFormat("Methods '{0}' and '{1}' have the same order of '{2}'.", previousMethod.Name, currentMethod.Name, currentMethodOrder);
					}

					previousMethod = currentMethod;
					previousMethodOrder = currentMethodOrder;
				}
				if (detected)
				{
					throw new Exception("Failed to sort processor method list because there were methods with the same order value.");
				}
			}

			return methods;
		}

		#endregion

		#region Build Preprocessor / Build Postprocessor / Scene Processor

#if UNITY_2018_1_OR_NEWER
		public void OnPreprocessBuild(BuildReport report)
#else
		public void OnPreprocessBuild(BuildTarget target, string path)
#endif
		{
			Log.Info($"Build processor '{BuildProcessorName}' is checking up in build preprocess...");

			//EditorSceneManagerTools.EnforceUserToSaveAllModifiedScenes("First you need to save the scene before building."); Disabled because it causes an internal Unity error at build time.
		}

#if UNITY_2018_1_OR_NEWER
		public void OnPostprocessBuild(BuildReport report)
#else
		public void OnPostprocessBuild(BuildTarget target, string path)
#endif
		{
			//Log.Info("Cleaning up in build postprocess...");
		}

#if UNITY_2018_1_OR_NEWER
		public void OnProcessScene(Scene scene, BuildReport report)
#else
		public void OnProcessScene(Scene scene)
#endif
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode)
				return;

			// TODO:
			Log.Warning("Launching build processor on build time is not implemented!");
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
		private static double LastProgressBarUpdateTime;
		private const float ProgressBarUpdateInterval = 0.2f;

		protected static void DisplayProgressBar(string title, string message, float progress = 0f)
		{
			ProgressBarTitle = title;
			ProgressBarMessage = message;
			UpdateProgressBar(progress);
		}

		protected static void UpdateProgressBar(float progress)
		{
			var now = PrecisionTiming.PreciseTime;
			if (now > ProgressBarUpdateInterval + LastProgressBarUpdateTime)
			{
				LastProgressBarUpdateTime = now;
				EditorUtility.DisplayProgressBar(ProgressBarTitle, ProgressBarMessage, progress);
			}
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
				Log.Info($"{now.ToStringHoursMinutesSecondsMilliseconds()} | {skippedText}Scene Processor Step {CurrentStep} - {title}");
			}
			else
			{
				var previousStepDuration = now - PreviousStepStartTime;
				Log.Info($"Step '{PreviousStepTitle}' took {previousStepDuration.ToStringHoursMinutesSecondsMilliseconds()}.");
				Log.Info($"{now.ToStringHoursMinutesSecondsMilliseconds()} | {skippedText}Scene Processor Step {CurrentStep} - {title}");
				if (isAllowed)
				{
					DisplayProgressBar("Scene Processor Step " + CurrentStep, title);
				}
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

		// -------------------------------------------------------------------------

		protected void DeparentAllStaticObjectsContainingComponentInLoadedScenes<T>(bool worldPositionStays, StaticEditorFlags leastExpectedFlags, ActiveCheck activeCheck) where T : Component
		{
			EditorGameObjectTools.SetParentOfAllStaticObjectsContainingComponentInLoadedScenes<T>(null, worldPositionStays, leastExpectedFlags, activeCheck);
		}

		protected void DeparentAllStaticObjectsContainingComponentInActiveScene<T>(bool worldPositionStays, StaticEditorFlags leastExpectedFlags, ActiveCheck activeCheck) where T : Component
		{
			EditorGameObjectTools.SetParentOfAllStaticObjectsContainingComponentInActiveScene<T>(null, worldPositionStays, leastExpectedFlags, activeCheck);
		}

		protected void DeparentAllObjectsContainingComponentInLoadedScenes<T>(bool worldPositionStays, ActiveCheck activeCheck) where T : Component
		{
			GameObjectTools.SetParentOfAllObjectsContainingComponentInLoadedScenes<T>(null, worldPositionStays, activeCheck);
		}

		protected void DeparentAllObjectsContainingComponentInActiveScene<T>(bool worldPositionStays, ActiveCheck activeCheck) where T : Component
		{
			GameObjectTools.SetParentOfAllObjectsContainingComponentInActiveScene<T>(null, worldPositionStays, activeCheck);
		}

		// -------------------------------------------------------------------------

		protected void MakeSureNoStaticObjectsContainingComponentExistInLoadedScenes<T>(StaticEditorFlags leastExpectedFlags, ActiveCheck activeCheck) where T : Component
		{
			EditorGameObjectTools.MakeSureNoStaticObjectsContainingComponentExistInLoadedScenes<T>(leastExpectedFlags, activeCheck);
		}

		protected void MakeSureNoStaticObjectsContainingComponentExistInActiveScene<T>(StaticEditorFlags leastExpectedFlags, ActiveCheck activeCheck) where T : Component
		{
			EditorGameObjectTools.MakeSureNoStaticObjectsContainingComponentExistInActiveScene<T>(leastExpectedFlags, activeCheck);
		}

		// -------------------------------------------------------------------------

		protected static void DestroyAllGameObjectsContainingComponentInLoadedScenes<T>(ActiveCheck activeCheck) where T : Component
		{
			EditorGameObjectTools.DestroyAllGameObjectsContainingComponentInLoadedScenes<T>(activeCheck, true, true);
		}

		protected static void DestroyAllGameObjectsContainingComponentInActiveScene<T>(ActiveCheck activeCheck) where T : Component
		{
			EditorGameObjectTools.DestroyAllGameObjectsContainingComponentInActiveScene<T>(activeCheck, true, true);
		}

		protected static void DestroyAllStaticGameObjectsContainingComponentInLoadedScenes<T>(StaticEditorFlags leastExpectedFlags, ActiveCheck activeCheck) where T : Component
		{
			EditorGameObjectTools.DestroyAllStaticGameObjectsContainingComponentInLoadedScenes<T>(leastExpectedFlags, activeCheck, true, true);
		}

		protected static void DestroyAllStaticGameObjectsContainingComponentInActiveScene<T>(StaticEditorFlags leastExpectedFlags, ActiveCheck activeCheck) where T : Component
		{
			EditorGameObjectTools.DestroyAllStaticGameObjectsContainingComponentInActiveScene<T>(leastExpectedFlags, activeCheck, true, true);
		}

		protected static void DestroyEmptyUnreferencedGameObjectsInLoadedScenes(Type[] excludedTypes = null)
		{
			EditorGameObjectTools.DestroyEmptyUnreferencedGameObjectsInLoadedScenes(excludedTypes, true, true);
		}

		protected static void DestroyEmptyUnreferencedGameObjectsInActiveScene(Type[] excludedTypes = null)
		{
			EditorGameObjectTools.DestroyEmptyUnreferencedGameObjectsInActiveScene(excludedTypes, true, true);
		}

		// -------------------------------------------------------------------------

		protected static void DestroyAllComponentsInLoadedScenes<T>(ActiveCheck activeCheck) where T : Component
		{
			EditorGameObjectTools.DestroyAllComponentsInLoadedScenes<T>(activeCheck, true, true);
		}

		protected static void DestroyAllComponentsInActiveScene<T>(ActiveCheck activeCheck) where T : Component
		{
			EditorGameObjectTools.DestroyAllComponentsInActiveScene<T>(activeCheck, true, true);
		}

		protected static void DestroyAllStaticComponentsInLoadedScenes<T>(StaticEditorFlags leastExpectedFlags, ActiveCheck activeCheck) where T : Component
		{
			EditorGameObjectTools.DestroyAllStaticComponentsInLoadedScenes<T>(leastExpectedFlags, activeCheck, true, true);
		}

		protected static void DestroyAllStaticComponentsInActiveScene<T>(StaticEditorFlags leastExpectedFlags, ActiveCheck activeCheck) where T : Component
		{
			EditorGameObjectTools.DestroyAllStaticComponentsInActiveScene<T>(leastExpectedFlags, activeCheck, true, true);
		}

		protected static void DestroyAllStaticMeshRenderersAndMeshFiltersInLoadedScenes(ActiveCheck activeCheck)
		{
			EditorGameObjectTools.DestroyAllStaticMeshRenderersAndMeshFiltersInLoadedScenes(activeCheck, true, true);
		}

		protected static void DestroyAllStaticMeshRenderersAndMeshFiltersInActiveScene(ActiveCheck activeCheck)
		{
			EditorGameObjectTools.DestroyAllStaticMeshRenderersAndMeshFiltersInActiveScene(activeCheck, true, true);
		}

		// -------------------------------------------------------------------------

		protected void DeleteComponentsOfEditorOnlyToolsInLoadedScenes()
		{
			DestroyAllComponentsInLoadedScenes<SnapToGroundInEditor>(ActiveCheck.IncludingInactive);
			DestroyAllComponentsInLoadedScenes<DontShowEditorHandler>(ActiveCheck.IncludingInactive);
			DestroyAllComponentsInLoadedScenes<Devnote>(ActiveCheck.IncludingInactive);
		}

		// -------------------------------------------------------------------------

		protected static void BlurReflectionProbesInLoadedScenes(ActiveCheck activeCheck)
		{
			ImageMagickCommander.BlurReflectionProbesInLoadedScenes(activeCheck);
		}

		protected static void BlurReflectionProbesInActiveScene(ActiveCheck activeCheck)
		{
			ImageMagickCommander.BlurReflectionProbesInActiveScene(activeCheck);
		}

		#endregion
	}

}
