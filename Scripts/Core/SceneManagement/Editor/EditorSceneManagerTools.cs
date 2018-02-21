using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Extenity.DataToolbox;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Extenity.SceneManagementToolbox.Editor
{

	public static class EditorSceneManagerTools
	{
		public static void EnforceUserToSaveAllModifiedScenes(string failMessage)
		{
			var isAnySceneDirty = IsAnyLoadedSceneDirty();
			if (isAnySceneDirty)
			{
				EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
				var isSceneStillDirty = IsAnyLoadedSceneDirty();
				if (isSceneStillDirty)
				{
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

		public static List<Scene> GetLoadedScenes(bool includeActiveScene)
		{
			var list = new List<Scene>();
			for (int i = 0; i < EditorSceneManager.sceneCount; i++)
			{
				var scene = EditorSceneManager.GetSceneAt(i);
				if (scene.isLoaded)
				{
					if (!scene.IsActive() || includeActiveScene)
					{
						list.Add(scene);
					}
				}
			}
			return list;
		}

		public static bool IsActive(this Scene scene)
		{
			if (scene.IsValid())
				throw new Exception("Scene is not valid.");
			if (string.IsNullOrEmpty(scene.path))
				throw new Exception("Scene path is not available.");
			var activeScene = EditorSceneManager.GetActiveScene();
			if (activeScene.IsValid())
				throw new Exception("Active scene is not valid.");
			if (string.IsNullOrEmpty(activeScene.path))
				throw new Exception("Active scene path is not available.");
			return activeScene.path.PathCompare(scene.path);
		}

		public static bool IsAnyLoadedSceneDirty(bool includeActiveScene = true)
		{
			return GetLoadedScenes(includeActiveScene).Any(scene => scene.isDirty);
		}

		public static void LoadMultipleScenes(string activeScene, List<string> loadedScenes)
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

		public static List<string> GetPaths(this IEnumerable<SceneSetup> sceneSetups)
		{
			var list = new List<string>();
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
			var list = new List<string>();
			foreach (var scene in scenes)
			{
				//if (scene != null)
				{
					list.Add(scene.path);
				}
			}
			return list;
		}

		public static bool IsSceneExistsAtPath(string path)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException("path");
			var fullPath = path.AddFileExtension(".unity");
			return File.Exists(fullPath);
		}
	}

}
