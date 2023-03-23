using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Extenity.DataToolbox;
using Extenity.FileSystemToolbox;
using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Extenity.SceneManagementToolbox.Editor
{

	public class EditorSceneManagerTools : SceneManagerTools
	{
		public static void EnforceUserToSaveAllModifiedScenes(string failMessage = "Currently loaded scene has unsaved changes. You must save or discard them first.",
		                                                      bool showMessageBox = true,
		                                                      string messageBoxTitle = "Attention Required",
		                                                      string okayButtonText = "Okay")
		{
			var isAnySceneDirty = IsAnyLoadedSceneDirty();
			if (isAnySceneDirty)
			{
				var isNotCancelled = EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
				if (!isNotCancelled)
				{
					if (showMessageBox)
					{
						EditorUtility.DisplayDialog(messageBoxTitle, failMessage, okayButtonText);
					}
					throw new Exception(failMessage);
				}
			}
		}

		public static List<SceneSetup> GetLoadedSceneSetups(bool includeActiveScene)
		{
			var list = new List<SceneSetup>();
			var sceneSetups = EditorSceneManager.GetSceneManagerSetup();
			for (int i = 0; i < sceneSetups.Length; i++)
			{
				var sceneSetup = sceneSetups[i];
				if (sceneSetup.isLoaded)
				{
					if (!sceneSetup.isActive || includeActiveScene)
					{
						list.Add(sceneSetup);
					}
				}
			}
			return list;
		}

		public static bool IsAnyLoadedSceneDirty(bool includeActiveScene = true)
		{
			var sceneListFilter = includeActiveScene
				? SceneListFilter.LoadedScenes
				: SceneListFilter.LoadedInactiveScenes;
			return GetScenes(sceneListFilter).Any(scene => scene.isDirty);
		}

		#region Load Scene

		public static void LoadSceneInEditorByPath(string scenePath)
		{
			if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
				EditorSceneManager.OpenScene(scenePath);
		}

		public static void LoadSceneInEditorByName(string sceneName)
		{
			if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
				EditorSceneManager.OpenScene(EditorBuildSettingsTools.GetScenePathFromBuildSettings(sceneName, false));
		}

		#endregion

		public static void LoadMultipleScenes(IList<string> loadedScenesWithActiveAtFirst)
		{
			if (loadedScenesWithActiveAtFirst.IsNotNullAndEmpty())
			{
				var i = 0;
				var scene = EditorSceneManager.OpenScene(loadedScenesWithActiveAtFirst[i++], OpenSceneMode.Single);
				EditorSceneManager.SetActiveScene(scene);
				for (; i < loadedScenesWithActiveAtFirst.Count; i++)
				{
					EditorSceneManager.OpenScene(loadedScenesWithActiveAtFirst[i], OpenSceneMode.Additive);
				}
			}
		}

		public static void LoadMultipleScenes(string activeScene, IList<string> loadedScenes)
		{
			var scene = EditorSceneManager.OpenScene(activeScene, OpenSceneMode.Single);
			EditorSceneManager.SetActiveScene(scene);
			if (loadedScenes != null)
			{
				foreach (var loadedScene in loadedScenes)
				{
					EditorSceneManager.OpenScene(loadedScene, OpenSceneMode.Additive);
				}
			}
		}

		public static void UnloadAllScenes(bool unloadUnusedAssets)
		{
			EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
			if (unloadUnusedAssets)
			{
				EditorUtility.UnloadUnusedAssetsImmediate(true);
			}
		}

		public new static void ReloadAllLoadedScenes()
		{
			var loadedInactiveScenes = GetScenes(SceneListFilter.LoadedInactiveScenes);
			var loadedActiveScene = EditorSceneManager.GetActiveScene();
			if (!loadedActiveScene.IsValid())
				return;

			EditorSceneManager.OpenScene(loadedActiveScene.path, OpenSceneMode.Single);
			if (loadedInactiveScenes != null)
			{
				foreach (var loadedScene in loadedInactiveScenes)
				{
					EditorSceneManager.OpenScene(loadedScene.path, OpenSceneMode.Additive);
				}
			}
		}

		public static bool IsSceneExistsAtPath(string path)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException(nameof(path));
			var fullPath = path.AddFileExtension(".unity");
			return File.Exists(fullPath);
		}
	}

	public static class EditorSceneManagerToolsExtensions
	{
		public static List<string> GetPaths(this IEnumerable<SceneSetup> sceneSetups)
		{
			var list = New.List<string>();
			foreach (var sceneSetup in sceneSetups)
			{
				if (sceneSetup != null)
				{
					list.Add(sceneSetup.path);
				}
			}
			return list;
		}

		public static List<string> GetPaths(this IEnumerable<Scene> scenes)
		{
			var list = New.List<string>();
			foreach (var scene in scenes)
			{
				//if (scene != null)
				{
					list.Add(scene.path);
				}
			}
			return list;
		}
	}

}
