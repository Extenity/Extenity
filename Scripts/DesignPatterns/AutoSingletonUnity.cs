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
	//   Use the derived class as a Component of a GameObject.
	//   InitializeSingleton(this); must be placed on the Awake method of derived class.
	public class AutoSingletonUnity<T> : MonoBehaviour where T : Component
	{
		private static T instance;
#pragma warning disable 414
		private string className;
#pragma warning restore

		protected void InitializeSingleton(T obj, bool dontDestroyOnLoad = true)
		{
			className = typeof(T).Name;
#if LoggingEnabled
			Debug.Log("Instantiating singleton: " + className, obj);
#endif
			instance = obj;

			if (dontDestroyOnLoad)
			{
				DontDestroyOnLoad(this);
			}

			SingletonTracker.SingletonInstantiated(className);
		}

		protected virtual void OnDestroy()
		{
			if (instance == null)  // To prevent errors in ExecuteInEditMode
				return;

#if LoggingEnabled
			Debug.Log("Destroying singleton: " + className);
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

		public static T Instance
		{
			get
			{
				if (instance == null)
				{
					instance = CreateSingleton("_" + typeof(T).Name);
				}
				return instance;
			}
		}
		public static bool IsInstanceAvailable { get { return !(instance == null); } }
	}

}
