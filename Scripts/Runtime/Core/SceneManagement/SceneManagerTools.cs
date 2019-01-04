using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Extenity.SceneManagementToolbox
{

	public class SceneManagerTools
	{
		public static List<Scene> GetScenes()
		{
			var sceneCount = SceneManager.sceneCount;
			var list = new List<Scene>(sceneCount);
			for (int i = 0; i < sceneCount; i++)
			{
				var scene = SceneManager.GetSceneAt(i);
				list.Add(scene);
			}
			return list;
		}

		public static List<Scene> GetLoadedScenes()
		{
			var sceneCount = SceneManager.sceneCount;
			var list = new List<Scene>(sceneCount);
			for (int i = 0; i < sceneCount; i++)
			{
				var scene = SceneManager.GetSceneAt(i);
				if (scene.isLoaded)
				{
					list.Add(scene);
				}
			}
			return list;
		}

		public static List<Scene> GetLoadedScenes(bool includeActiveScene)
		{
			var sceneCount = SceneManager.sceneCount;
			var list = new List<Scene>(sceneCount);
			for (int i = 0; i < sceneCount; i++)
			{
				var scene = SceneManager.GetSceneAt(i);
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

		public static void UnloadAllScenesAsyncExcept(string sceneName)
		{
			var sceneCount = SceneManager.sceneCount;
			for (int i = sceneCount - 1; i >= 0; i--)
			{
				var scene = SceneManager.GetSceneAt(i);
				if (scene.name != sceneName)
				{
					SceneManager.UnloadSceneAsync(scene);
				}
			}
		}

		public static void UnloadAllScenesAsync()
		{
			var sceneCount = SceneManager.sceneCount;
			for (int i = sceneCount - 1; i >= 0; i--)
			{
				var scene = SceneManager.GetSceneAt(i);
				SceneManager.UnloadSceneAsync(scene);
			}
		}

		public static void ReloadAllLoadedScenes()
		{
			var loadedScenes = GetLoadedScenes(false);
			var loadedActiveScene = SceneManager.GetActiveScene();
			if (!loadedActiveScene.IsValid())
				return;

			SceneManager.LoadScene(loadedActiveScene.name, LoadSceneMode.Single);
			if (loadedScenes != null)
			{
				foreach (var loadedScene in loadedScenes)
				{
					SceneManager.LoadScene(loadedScene.name, LoadSceneMode.Additive);
				}
			}
		}

		public static List<GameObject> GetRootGameObjectsOfLoadedScenes()
		{
			var scenes = GetLoadedScenes();
			var result = new List<GameObject>();
			foreach (var scene in scenes)
			{
				scene.GetRootGameObjects(result);
			}
			return result;
		}

		#region DontDestroyOnLoad Scene

		/// <summary>
		/// Source: https://forum.unity.com/threads/editor-script-how-to-access-objects-under-dontdestroyonload-while-in-play-mode.442014/
		/// </summary>
		public static GameObject[] GetRootGameObjectsOfDontDestroyOnLoadScene()
		{
			GameObject temp = null;
			try
			{
				temp = new GameObject();
				Object.DontDestroyOnLoad(temp);
				var dontDestroyOnLoadScene = temp.scene;
				Object.DestroyImmediate(temp);
				temp = null;

				return dontDestroyOnLoadScene.GetRootGameObjects();
			}
			finally
			{
				if (temp != null)
					Object.DestroyImmediate(temp);
			}
		}

		/// <summary>
		/// Source: https://forum.unity.com/threads/editor-script-how-to-access-objects-under-dontdestroyonload-while-in-play-mode.442014/
		/// </summary>
		public static Scene GetDontDestroyOnLoadScene()
		{
			GameObject temp = null;
			try
			{
				temp = new GameObject();
				Object.DontDestroyOnLoad(temp);
				var dontDestroyOnLoadScene = temp.scene;
				Object.DestroyImmediate(temp);
				temp = null;
				return dontDestroyOnLoadScene;
			}
			finally
			{
				if (temp != null)
					Object.DestroyImmediate(temp);
			}
		}

		#endregion
	}

	public static class SceneManagerToolsExtensions
	{
		public static bool IsActive(this Scene scene)
		{
			if (!scene.IsValid())
				throw new Exception("Scene is not valid.");

			var activeScene = SceneManager.GetActiveScene();
			if (!activeScene.IsValid())
				throw new Exception("Active scene is not valid.");

			return activeScene == scene;
		}

		public static string GetSceneArtifactDirectoryPath(this Scene scene)
		{
			if (!scene.IsValid())
				throw new Exception("Scene is not valid.");

			var path = scene.path;
			if (!path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
				throw new Exception("Scene is not saved to a file.");
			return path.Substring(0, path.Length - ".unity".Length);
		}
	}

}
