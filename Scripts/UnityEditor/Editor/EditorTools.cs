using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;

namespace Extenity.EditorUtilities
{

	public static class EditorTools
	{
		#region File/Directory Delete

		public static void DeleteMetaFileAndItem(string path)
		{
			if (Directory.Exists(path))
			{
				Directory.Delete(path, true);
				DeleteMetaFileOfItem(path);
			}
			else if (File.Exists(path))
			{
				File.Delete(path);
				DeleteMetaFileOfItem(path);
			}
			else
			{
				Debug.LogError("Tried to delete file or directory at path '" + path + "' but item cannot be found.");
			}
		}

		private static void DeleteMetaFileOfItem(string path)
		{
			var metaFile = path + ".meta";
			if (File.Exists(metaFile))
				File.Delete(metaFile);
		}

		#endregion

		#region Load Scene

		public static void LoadSceneInEditorByPath(string scenePath)
		{
			if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
				EditorSceneManager.OpenScene(scenePath);
		}

		public static void LoadSceneInEditorByName(string sceneName)
		{
			if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
				EditorSceneManager.OpenScene(GetScenePathFromBuildSettings(sceneName, false));
		}

		#endregion

		#region Get Scene Names From Build Settings

		public static string[] GetSceneNamesFromBuildSettings(List<string> excludingNames = null)
		{
			var list = new List<string>();

			for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
			{
				var scene = EditorBuildSettings.scenes[i];

				if (scene.enabled)
				{
					string name = scene.path.Substring(scene.path.LastIndexOf('/') + 1);
					name = name.Substring(0, name.Length - 6);

					if (excludingNames != null)
					{
						if (excludingNames.Contains(name))
							continue;
					}

					list.Add(name);
				}
			}

			return list.ToArray();
		}

		public static string[] GetScenePathsFromBuildSettings(List<string> excludingPaths = null)
		{
			var list = new List<string>();

			for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
			{
				var scene = EditorBuildSettings.scenes[i];

				if (scene.enabled)
				{
					if (excludingPaths != null)
					{
						if (excludingPaths.Contains(scene.path))
							continue;
					}

					list.Add(scene.path);
				}
			}

			return list.ToArray();
		}

		public static string GetScenePathFromBuildSettings(string sceneName, bool onlyIfEnabled)
		{
			if (string.IsNullOrEmpty(sceneName))
				return null;

			for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
			{
				var scene = EditorBuildSettings.scenes[i];

				if (!onlyIfEnabled || scene.enabled)
				{
					string name = scene.path.Substring(scene.path.LastIndexOf('/') + 1);
					name = name.Substring(0, name.Length - 6);

					if (name == sceneName)
					{
						return scene.path;
					}
				}
			}
			return null;
		}

		#endregion

		#region Enable/Disable Auto Refresh

		public static bool IsAutoRefreshEnabled
		{
			get
			{
				return EditorPrefs.GetBool("kAutoRefresh");
			}
		}

		public static void EnableAutoRefresh(bool enabled)
		{
			EditorPrefs.SetBool("kAutoRefresh", enabled);
		}

		public static void EnableAutoRefresh()
		{
			EditorPrefs.SetBool("kAutoRefresh", true);
		}

		public static void DisableAutoRefresh()
		{
			EditorPrefs.SetBool("kAutoRefresh", false);
		}

		public static void ToggleAutoRefresh()
		{
			if (IsAutoRefreshEnabled)
			{
				DisableAutoRefresh();
			}
			else
			{
				EnableAutoRefresh();
			}
		}

		#endregion
	}

}
