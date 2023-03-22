#if UNITY
#if ExtenitySubsystems

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Extenity.SubsystemManagementToolbox
{

	public static class SubsystemManager
	{
		#region Settings Asset

		public static SubsystemSettings Settings
		{
			get
			{
				SubsystemSettings.GetInstance(out var settings, SubsystemConstants.ConfigurationFileNameWithoutExtension);
				return settings;
			}
		}

		#endregion

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Initialize_SubsystemRegistration()
		{
#if !UNITY_EDITOR
			Log.Info("Subsystem Manager checking in at SubsystemRegistration.");
#endif
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
		private static void Initialize_AfterAssembliesLoaded()
		{
#if !UNITY_EDITOR
			Log.Info("Subsystem Manager checking in at AfterAssembliesLoaded.");
#endif
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void Initialize_AfterSceneLoad()
		{
#if !UNITY_EDITOR
			Log.Info("Subsystem Manager checking in at AfterSceneLoad.");
#endif
		}

		// Instantiating game objects in SubsystemRegistration and AfterAssembliesLoaded is a bad idea.
		// It works in Editor but observed not working in Windows and Android builds and probably other
		// platforms too. Game objects are destroyed just before BeforeSceneLoad for some reason.
		// So decided to initialize our subsystems at BeforeSceneLoad stage. See 119392241.
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Initialize()
		{
#if !UNITY_EDITOR
			Log.Info("Subsystem Manager checking in at BeforeSceneLoad.");
#endif

			var settings = Settings;
			if (!settings)
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
			Log.Verbose("Finalizing subsystem manager.");

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

			// Not doing this anymore. Empty scenes should be initialized too, if empty name matches any of the name filters.
			// if (string.IsNullOrWhiteSpace(sceneName))
			// {
			// 	Log.DebugVerbose($"Skipping subsystem initialization for scene '{sceneName}'.");
			// 	return;
			// }

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

		#region Log

		private static readonly Logger Log = new(nameof(SubsystemManager));

		#endregion
	}

}

#endif
#endif
