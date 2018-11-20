//#define LogSingletonInEditor
//#define LogSingletonInBuilds
#define LogSingletonInDebugBuilds

#if (UNITY_EDITOR && LogSingletonInEditor) || (!UNITY_EDITOR && LogSingletonInBuilds) || (!UNITY_EDITOR && DEBUG && LogSingletonInDebugBuilds)
#define LoggingEnabled
#else
#undef LoggingEnabled
#endif

using UnityEngine;

namespace Extenity.DesignPatternsToolbox
{

	// Usage:
	//   Use the derived class as a MonoBehaviour of a GameObject.
	//   InitializeSingleton(this); must be placed on the Awake method of derived class.
	public class SingletonUnity<T> : MonoBehaviour where T : MonoBehaviour
	{
		private static T instance;
#pragma warning disable 414
		private string className;
#pragma warning restore

		protected T InitializeSingleton(T obj, bool dontDestroyOnLoad = true)
		//protected void InitializeSingleton(bool dontDestroyOnLoad = true) // TODO: Refactor: Do this on extra time. See 1759175
		{
			className = typeof(T).Name;
#if LoggingEnabled
			Log.Info("Instantiating singleton: " + className, obj);
#endif
			instance = obj;
			//instance = this as T; // TODO: Refactor: Do this on extra time. See 1759175

			if (dontDestroyOnLoad)
			{
				DontDestroyOnLoad(this);
			}

			SingletonTracker.SingletonInstantiated(className);

			// Returning the instance for ease of use. When there are double derived singleton classes,
			// they need to keep their own static instance fields. Returning the instance here allows
			// these fields to be set directly in one-liner code where InitializeSingleton is called.
			return instance;
		}

		protected virtual void OnDestroy()
		{
			if (instance == null)  // To prevent errors in ExecuteInEditMode
				return;

#if LoggingEnabled
			Log.Info("Destroying singleton: " + className);
#endif
			instance = default(T);
			SingletonTracker.SingletonDestroyed(className);
		}

		public static T CreateSingleton(string addedGameObjectName = "_")
		{
			var go = GameObject.Find(addedGameObjectName);
			if (go == null)
				go = new GameObject(addedGameObjectName);
			return go.AddComponent<T>();
		}

		public static void DestroySingleton()
		{
			if (instance.gameObject.GetComponents<Component>().Length == 2) // 1 for 'Transform' component and 1 for 'T' component
			{
				// If this component is the only one left in gameobject, destroy the gameobject as well
				Destroy(instance.gameObject);
			}
			else
			{
				// Destroy only this component
				Destroy(instance);
			}
		}

		public static T Instance { get { return instance; } }
		public static bool IsInstanceAvailable { get { return instance; } }
		public static bool IsInstanceEnabled { get { return instance && instance.isActiveAndEnabled; } }

		private static T _EditorInstance;
		public static T EditorInstance
		{
			get
			{
				if (Application.isPlaying)
				{
					Log.Error($"Tried to get editor instance of singleton '{typeof(T).Name}' in play time.");
					return null;
				}
				if (!_EditorInstance)
				{
					_EditorInstance = FindObjectOfType<T>();
					if (!_EditorInstance)
					{
						Log.Error($"Could not find an instance of singleton '{typeof(T).Name}' in scene.");
					}
				}
				return _EditorInstance;
			}
		}
		public static bool IsEditorInstanceAvailable
		{
			get
			{
				if (!_EditorInstance)
				{
					_EditorInstance = FindObjectOfType<T>();
				}
				return _EditorInstance;
			}
		}
	}

}
