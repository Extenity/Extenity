using Extenity.SubsystemManagementToolbox;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Extenity.ApplicationToolbox
{

	public static class SubsystemManager
	{
		// Alternatives:
		// [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		// private static void Initialize_BeforeSceneLoad() { }
		// [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		// private static void Initialize_AfterSceneLoad() { }

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Initialize()
		{
			if (!SubsystemSettings.GetInstance(out var settings, SubsystemConstants.ConfigurationFileNameWithoutExtension))
			{
				return;
			}

			settings.ResetStatus();

			SceneManager.sceneLoaded -= OnSceneLoaded;
			SceneManager.sceneLoaded += OnSceneLoaded;
			// SceneManager.sceneUnloaded -= OnSceneUnloaded;
			// SceneManager.sceneUnloaded += OnSceneUnloaded;
			// SceneManager.activeSceneChanged -= OnActiveSceneChanged;
			// SceneManager.activeSceneChanged += OnActiveSceneChanged;

#if UNITY_EDITOR
			if (UnityEditor.EditorSettings.enterPlayModeOptionsEnabled &&
			    (UnityEditor.EditorSettings.enterPlayModeOptions & UnityEditor.EnterPlayModeOptions.DisableSceneReload) > 0)
			{
				Log.Error("Disabling Scene Reload in Enter Play Mode Options is not supported in Subsystem Manager. Expect scene subsystems to not be initialized when entering Play mode.");
			}

			Application.quitting -= _OnQuit;
			Application.quitting += _OnQuit;
#endif

			settings.InitializeForApplication();
		}

		private static void _OnQuit()
		{
			Log.DebugVerbose("Finalizing subsystem manager.");

			Application.quitting -= _OnQuit;
			SceneManager.sceneLoaded -= OnSceneLoaded;
			// SceneManager.sceneUnloaded -= OnSceneUnloaded;
			// SceneManager.activeSceneChanged -= OnActiveSceneChanged;

			if (SubsystemSettings.GetInstance(out var settings, SubsystemConstants.ConfigurationFileNameWithoutExtension))
			{
				settings.ResetStatus();
			}
		}

		private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			var sceneName = scene.name;
			if (string.IsNullOrWhiteSpace(sceneName))
			{
				Log.DebugVerbose($"Skipping subsystem initialization for scene '{sceneName}'.");
				return;
			}

			if (SubsystemSettings.GetInstance(out var settings, SubsystemConstants.ConfigurationFileNameWithoutExtension))
			{
				settings.InitializeForScene(sceneName);
			}
		}

		// private static void OnSceneUnloaded(Scene scene)
		// {
		// 	Log.Info($"OnSceneUnloaded: {scene.name}");
		// }
		//
		// private static void OnActiveSceneChanged(Scene previous, Scene current)
		// {
		// 	Log.Info($"OnActiveSceneChanged previous: {previous.name} current: {current.name}");
		// }
	}

}
