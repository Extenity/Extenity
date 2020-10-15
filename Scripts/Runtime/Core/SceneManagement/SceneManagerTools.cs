using System;
using System.Collections.Generic;
using System.IO;
using Extenity.DataToolbox;
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

		public static List<Scene> GetLoadedScenes(bool includeActiveScene, bool includeDontDestroyOnLoadScene)
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
			if (includeDontDestroyOnLoadScene
#if UNITY_EDITOR
				&& Application.isPlaying
#endif
			)
			{
				list.Add(GetDontDestroyOnLoadScene());
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
			var loadedScenes = GetLoadedScenes(false, false);
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

		public static List<GameObject> GetRootGameObjectsOfLoadedScenes(bool includeActiveScene, bool includeDontDestroyOnLoadScene)
		{
			var scenes = GetLoadedScenes(includeActiveScene, includeDontDestroyOnLoadScene);
			var result = new List<GameObject>();
			foreach (var scene in scenes)
			{
				scene.GetRootGameObjects(result);
			}
			return result;
		}

		#region Collect Scene List From Build Settings

		public static string[] CollectSceneListFromBuildSettings()
		{
			return CollectSceneListFromBuildSettings(new StringFilter(new StringFilterEntry(StringFilterType.Any, "")));
		}

		public static string[] CollectSceneListFromBuildSettings(StringFilter sceneNameFilter)
		{
			Log.Verbose("Collecting scene list");
			var duplicateNameChecker = new HashSet<string>();
			var result = New.List<string>();
			var sceneCount = SceneManager.sceneCountInBuildSettings;
			for (int i = 0; i < sceneCount; i++)
			{
				var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
				var sceneName = Path.GetFileNameWithoutExtension(scenePath);
#if UNITY_EDITOR
				// SceneUtility.GetScenePathByBuildIndex also counts disabled scenes in Build Settings.
				// So extra work required to ignore them in Editor.
				// TODO IMMEDIATE: Ensure GetScenePathByBuildIndex skips disabled scenes in builds. Otherwise find another method of detecting disabled scenes.
				if (!UnityEditor.EditorBuildSettings.scenes[i].enabled)
				{
					Log.Verbose($"   {i}. {sceneName} (Disabled)");
					continue;
				}
#endif
				Log.Verbose($"   {i}. {sceneName}");
				if (!string.IsNullOrWhiteSpace(sceneName))
				{
					if (!duplicateNameChecker.Add(sceneName))
					{
						Log.CriticalError($"Duplicate scene names are not allowed. Rename the scene '{sceneName}'.");
					}

					if (sceneNameFilter.IsMatching(sceneName))
					{
						result.Add(sceneName);
					}
				}
			}
			var resultArray = result.ToArray();
			Release.List(ref result);
			return resultArray;
		}

		#endregion

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
#if UNITY_EDITOR
				if (!Application.isPlaying)
				{
					throw new Exception("There is no DontDestroyOnLoad scene in edit mode.");
				}
#endif
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
