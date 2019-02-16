using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Extenity.UnityEditorToolbox.Editor
{

	public static class EditorBuildSettingsTools
	{
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
	}

}
