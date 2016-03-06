using System;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Extenity.Logging;
using Logger = Extenity.Logging.Logger;

namespace Extenity
{
	public static class EditorTools
	{
		#region Empty directories

		private static List<string> GetEmptyDirectories()
		{
			return GetEmptyDirectories(Application.dataPath);
		}

		private static List<string> GetEmptyDirectories(string path)
		{
			var list = new List<string>();
			var subDirectories = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);

			for (int iSubDirectory = 0; iSubDirectory < subDirectories.Length; iSubDirectory++)
			{
				var subDirectory = subDirectories[iSubDirectory];
				if (DirectoryTools.IsDirectoryEmpty(subDirectory))
				{
					list.Add(subDirectory);
				}
			}

			return list;
		}

		#endregion

		#region '*.orig' Files

		public static string[] GetOrigFiles()
		{
			return GetOrigFiles(Application.dataPath);
		}

		public static string[] GetOrigFiles(string path)
		{
			return Directory.GetFiles(path, "*.orig", SearchOption.AllDirectories);
		}

		public static string[] GetThumbsDbFiles()
		{
			return GetThumbsDbFiles(Application.dataPath);
		}

		public static string[] GetThumbsDbFiles(string path)
		{
			return Directory.GetFiles(path, "thumbs.db", SearchOption.AllDirectories);
		}

		#endregion

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
				Logger.LogError("Tried to delete file or directory at path '" + path + "' but item cannot be found.");
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
			EditorApplication.SaveCurrentSceneIfUserWantsTo();
			EditorApplication.OpenScene(scenePath);
		}

		public static void LoadSceneInEditorByName(string sceneName)
		{
			EditorApplication.SaveCurrentSceneIfUserWantsTo();
			EditorApplication.OpenScene(GetScenePathFromBuildSettings(sceneName, false));
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

		#region Menu Items - Cleaning

		[MenuItem("Tools/Clean Up/Clear all")]
		public static void ClearAll()
		{
			ClearOrigFiles();
			ClearEmptyDirectories();
		}

		[MenuItem("Tools/Clean Up/Clear .orig files")]
		public static void ClearOrigFiles()
		{
			var items = GetOrigFiles();
			for (int i = 0; i < items.Length; i++)
			{
				DeleteMetaFileAndItem(items[i]);
			}

			Logger.Log("Cleared '.orig' files: " + items.Length);
			if (items.Length > 0)
				items.LogList();

			AssetDatabase.Refresh();
		}

		[MenuItem("Tools/Clean Up/Clear thumbs.db files")]
		public static void ClearThumbsDbFiles()
		{
			var items = GetThumbsDbFiles();
			for (int i = 0; i < items.Length; i++)
			{
				DeleteMetaFileAndItem(items[i]);
			}

			Logger.Log("Cleared 'thumbs.db' files: " + items.Length);
			if (items.Length > 0)
				items.LogList();

			AssetDatabase.Refresh();
		}

		[MenuItem("Tools/Clean Up/Clear empty directories")]
		public static void ClearEmptyDirectories()
		{
			var tryAgain = true;
			var clearedItems = new List<string>();

			while (tryAgain)
			{
				var items = GetEmptyDirectories();
				for (int i = 0; i < items.Count; i++)
				{
					DeleteMetaFileAndItem(items[i]);
				}

				clearedItems.Combine(items);
				tryAgain = items.Count > 0;
			}

			Logger.Log("Cleared empty directories: " + clearedItems.Count);
			if (clearedItems.Count > 0)
				clearedItems.LogList();

			AssetDatabase.Refresh();
		}

		#endregion
	}
}
