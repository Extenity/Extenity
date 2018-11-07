using Extenity.ApplicationToolbox;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Extenity.UnityEditorToolbox.Editor
{

	[InitializeOnLoad]
	public class SceneSwitcherOnPlay : EditorWindow
	{
		#region Initialization

		static SceneSwitcherOnPlay()
		{
			var pref = StartScenePathPref;
			if (!string.IsNullOrWhiteSpace(pref))
			{
				SetPlayModeStartScene(pref);
			}
		}

		private void Awake()
		{
			titleContent = new GUIContent("Scene Switcher");
		}

		[MenuItem("File/Scene Switcher on Play")]
		static void Open()
		{
			GetWindow<SceneSwitcherOnPlay>();
		}

		#endregion

		#region GUI

		private void OnGUI()
		{
			GUILayout.Space(5f);
			GUILayout.Label("Select scene to be started with Play button");
			var previousScene = EditorSceneManager.playModeStartScene;
			var scene = (SceneAsset)EditorGUILayout.ObjectField("Start Scene", previousScene, typeof(SceneAsset), false);
			if (scene != previousScene)
			{
				EditorSceneManager.playModeStartScene = scene;
				StartScenePathPref = AssetDatabase.GetAssetPath(scene);
			}
		}

		#endregion

		#region Save / Load

		private static readonly string StartScenePathPrefKey = "SceneSwitcher.StartScenePath." + ApplicationTools.PathHash;

		private static string StartScenePathPref
		{
			get { return EditorPrefs.GetString(StartScenePathPrefKey, ""); }
			set { EditorPrefs.SetString(StartScenePathPrefKey, value); }
		}

		#endregion

		#region Tools

		public static void SetPlayModeStartScene(string scenePath)
		{
			if (string.IsNullOrWhiteSpace(scenePath))
			{
				EditorSceneManager.playModeStartScene = null;
			}
			else
			{
				var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
				if (scene != null)
					EditorSceneManager.playModeStartScene = scene;
				else
					Debug.Log($"Could not find scene at path '{scenePath}'.");
			}
		}

		#endregion
	}

}
