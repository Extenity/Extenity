using Extenity.ApplicationToolbox;
using Extenity.IMGUIToolbox;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Extenity.UnityEditorToolbox.Editor
{

	[InitializeOnLoad]
	public class SceneSwitcherOnPlay : ExtenityEditorWindowBase
	{
		#region Initialization

		static SceneSwitcherOnPlay()
		{
			if (EnableSceneSwitcherPref)
			{
				var pref = StartScenePathPref;
				if (!string.IsNullOrWhiteSpace(pref))
				{
					SetPlayModeStartScene(pref);
				}
			}
		}

		private void Awake()
		{
			titleContent = new GUIContent("Scene Switcher");

			InitializeAPICallPrevention();
		}

		[MenuItem("Edit/Scene Switcher on Play", priority = 140)] // Priority is just above the Play option.
		private static void OpenWindow()
		{
			GetWindow<SceneSwitcherOnPlay>();
		}

		#endregion

		#region Deinitialization

		private void OnDestroy()
		{
			DeinitializeAPICallPrevention();
		}

		#endregion

		#region GUI

		protected override void OnGUIDerived()
		{
			var isEnabled = EnableSceneSwitcherPref;

			GUILayout.Space(5f);
			GUILayout.Label("Select scene to be started with Play button");
			SceneAsset previousScene;
			SceneAsset scene;
			if (isEnabled)
			{
				previousScene = EditorSceneManager.playModeStartScene;
				scene = (SceneAsset)EditorGUILayout.ObjectField("Start Scene", previousScene, typeof(SceneAsset), false);
			}
			else
			{
				var path = StartScenePathPref;
				if (string.IsNullOrEmpty(path))
				{
					previousScene = null;
				}
				else
				{
					previousScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
				}
				scene = (SceneAsset)EditorGUILayout.ObjectField("Start Scene", previousScene, typeof(SceneAsset), false);
			}
			if (scene != previousScene)
			{
				EditorSceneManager.playModeStartScene = scene;
				if (scene != null)
				{
					StartScenePathPref = AssetDatabase.GetAssetPath(scene);
					EnableSceneSwitcherPref = true;
				}
				else
				{
					StartScenePathPref = "";
					EnableSceneSwitcherPref = false;
				}
			}

			GUILayout.BeginHorizontal();

			if (GUILayoutTools.Button("Enable", !isEnabled && scene != null))
			{
				EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(StartScenePathPref);
				EnableSceneSwitcherPref = true;
			}
			if (GUILayoutTools.Button("Disable", isEnabled))
			{
				EditorSceneManager.playModeStartScene = null;
				EnableSceneSwitcherPref = false;
			}

			GUILayout.EndHorizontal();
		}

		#endregion

		#region Save / Load

		private static string _EnableSceneSwitcherPrefKey;
		private static string EnableSceneSwitcherPrefKey
		{
			get
			{
				if (_EnableSceneSwitcherPrefKey == null)
					_EnableSceneSwitcherPrefKey = "SceneSwitcher.EnableSceneSwitcher." + ApplicationTools.PathHash;
				return _EnableSceneSwitcherPrefKey;
			}
		}

		private static string _StartScenePathPrefKey;
		private static string StartScenePathPrefKey
		{
			get
			{
				if (_StartScenePathPrefKey == null)
					_StartScenePathPrefKey = "SceneSwitcher.StartScenePath." + ApplicationTools.PathHash;
				return _StartScenePathPrefKey;
			}
		}

		private static bool EnableSceneSwitcherPref
		{
			get { return EditorPrefs.GetBool(EnableSceneSwitcherPrefKey, false); }
			set { EditorPrefs.SetBool(EnableSceneSwitcherPrefKey, value); }
		}

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
					Log.Info($"Could not find scene at path '{scenePath}'.");
			}
		}

		#endregion
	}

}
