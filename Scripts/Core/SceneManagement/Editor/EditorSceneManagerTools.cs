using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Extenity.DataToolbox;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Extenity.SceneManagementToolbox.Editor
{

	public class EditorSceneManagerTools : SceneManagerTools
	{
		public static void EnforceUserToSaveAllModifiedScenes(string failMessage, bool showMessageBox = false, string messageBoxTitle = "Attention Required", string okayButtonText = "Okay")
		{
			var isAnySceneDirty = IsAnyLoadedSceneDirty();
			if (isAnySceneDirty)
			{
				EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
				var isSceneStillDirty = IsAnyLoadedSceneDirty();
				if (isSceneStillDirty)
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

		public static bool IsSceneExistsAtPath(string path)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException("path");
			var fullPath = path.AddFileExtension(".unity");
			return File.Exists(fullPath);
		}
	}

	public static class EditorSceneManagerToolsExtensions
	{
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
	}

}
