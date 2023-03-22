#if UNITY

using System;
using System.Collections.Generic;
using System.IO;
using Extenity.DataToolbox;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Extenity.SceneManagementToolbox
{

	[ExecuteAlways]
	public class LevelSceneManager : MonoBehaviour
	{
		#region Configuration

		[ListDrawerSettings(Expanded = true)]
		public string LevelSceneNamePrefix = "Level-";

		#endregion

		#region Initialization

		protected void Awake()
		{
			InitializeLevelSceneList();
#if UNITY_EDITOR
			RegisterForBuildSettingsSceneListChanges();
#endif
		}

		#endregion

		#region Update

#if UNITY_EDITOR
		protected void Update()
		{
			if (!Application.isPlaying)
			{
				InitializeLevelSceneList();
				RegisterForBuildSettingsSceneListChanges();
			}
		}

		protected void OnValidate()
		{
			if (!Application.isPlaying)
			{
				InitializeLevelSceneList();
				RegisterForBuildSettingsSceneListChanges();
			}
		}

		private void RegisterForBuildSettingsSceneListChanges()
		{
			UnityEditor.EditorBuildSettings.sceneListChanged -= InitializeLevelSceneList; // Deregister first, for every condition.
			if (!Application.isPlaying)
			{
				UnityEditor.EditorBuildSettings.sceneListChanged += InitializeLevelSceneList;
			}
		}

		// It's not cool to keep the inspector redraw itself every frame. But failed to find a working way to repaint
		// only when the data changes.
		[OnInspectorGUI]
		private void RedrawConstantly()
		{
			Sirenix.Utilities.Editor.GUIHelper.RequestRepaint();
		}
#endif

		#endregion

		#region Level Scene List

		[NonSerialized, ShowInInspector, ReadOnly]
		public string[] LevelScenes;

		private void InitializeLevelSceneList()
		{
			LevelScenes = GenerateLevelSceneList(LevelSceneNamePrefix);
		}

		public static string[] GenerateLevelSceneList(string levelSceneNamePrefix)
		{
			Log.Verbose("Generating level scene list");
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
				// TODO: Ensure GetScenePathByBuildIndex skips disabled scenes in builds. Otherwise find another method of detecting disabled scenes.
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
						Log.Fatal($"Duplicate scene names are not allowed. Rename the scene '{sceneName}'.");
					}

					if (sceneName.StartsWith(levelSceneNamePrefix, StringComparison.Ordinal))
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

		#region Log

		private static readonly Logger Log = new(nameof(LevelSceneManager));

		#endregion
	}

}

#endif
