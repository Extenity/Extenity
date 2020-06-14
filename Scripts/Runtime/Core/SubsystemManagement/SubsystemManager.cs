using UnityEngine;

namespace Extenity.ApplicationToolbox
{

	public class SubsystemManager : MonoBehaviour
	{
		/*
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Initialize()
		{
			Log.Info("RuntimeInitializeOnLoadMethod    BeforeSceneLoad   " + SceneManager.GetActiveScene().name);

			SceneManager.activeSceneChanged += OnActiveSceneChanged;
			SceneManager.sceneLoaded += OnSceneLoaded;
			SceneManager.sceneUnloaded += OnSceneUnloaded;

		}

		private static void OnActiveSceneChanged(Scene scene1, Scene scene2)
		{
			Log.Info("active scene changed : " + scene1.name + "   ->   " + scene2.name);
		}

		private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			Log.Info("scene loaded : " + scene.name);
		}
		private static void OnSceneUnloaded(Scene scene)
		{
			Log.Info("scene unloaded : " + scene.name + "     active scene: " + SceneManager.GetActiveScene().name);
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void RuntimeInitializeOnLoadMethod()
		{
			Log.Info("RuntimeInitializeOnLoadMethod    AfterSceneLoad");
		}
		*/
	}

}
