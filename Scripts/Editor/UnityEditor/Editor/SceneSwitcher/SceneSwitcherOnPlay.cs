using Extenity.ApplicationToolbox;
using Extenity.IMGUIToolbox.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Extenity.UnityEditorToolbox.Editor
{

	/// <summary>
	/// Behind the curtains, Scene Switcher tells Unity about which scene should be loaded at starting play mode.
	/// Unity looks for EditorSceneManager.playModeStartScene and loads the scene automatically.
	/// </summary>
	[InitializeOnLoad]
	public class SceneSwitcherOnPlay : ExtenityEditorWindowBase
	{
		#region Configuration

		protected override WindowSpecifications Specifications => new WindowSpecifications
		{
			Title = "Scene Switcher",
		};

		#endregion

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
			// Quick fix:
			// Close the window when play mode changes. Because at the time the user
			// stops playmode, Unity will go crazy about calling ApplicationTools.PathHash
			// from static constructor. This is a duct tape solution, but whatever.
			SetToCloseWindowOnAssemblyReloadOrPlayModeChange();
		}

		[MenuItem(ExtenityMenu.Edit + "Scene Switcher on Play", priority = ExtenityMenu.UnityPlayMenuPriority_AbovePlay)]
		private static void ToggleWindow()
		{
			EditorWindowTools.ToggleWindow<SceneSwitcherOnPlay>();
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
				Enable();
			}
			if (GUILayoutTools.Button("Disable", isEnabled))
			{
				Disable();
			}

			GUILayout.EndHorizontal();
		}

		#endregion

		#region Enable / Disable

		public static void Enable()
		{
			EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(StartScenePathPref);
			EnableSceneSwitcherPref = true;
		}

		public static void Disable()
		{
			EditorSceneManager.playModeStartScene = null;
			EnableSceneSwitcherPref = false;
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

		public static bool EnableSceneSwitcherPref
		{
			get => EditorPrefs.GetBool(EnableSceneSwitcherPrefKey, false);
			set => EditorPrefs.SetBool(EnableSceneSwitcherPrefKey, value);
		}

		public static string StartScenePathPref
		{
			get => EditorPrefs.GetString(StartScenePathPrefKey, "");
			set => EditorPrefs.SetString(StartScenePathPrefKey, value);
		}

		#endregion

		#region Tools

		private static void SetPlayModeStartScene(string scenePath)
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

		#region Log

		private static readonly Logger Log = new(nameof(SceneSwitcherOnPlay));

		#endregion
	}

}
