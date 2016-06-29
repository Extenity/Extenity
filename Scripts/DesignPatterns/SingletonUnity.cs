//#define LogSingletonInEditor
//#define LogSingletonInBuilds
#define LogSingletonInDebugBuilds

#if (UNITY_EDITOR && LogSingletonInEditor) || (!UNITY_EDITOR && LogSingletonInBuilds) || (!UNITY_EDITOR && DEBUG && LogSingletonInDebugBuilds)
#define LoggingEnabled
#else
#undef LoggingEnabled
#endif

using UnityEngine;

// Usage:
//   Use the derived class as a Component of a GameObject.
//   InitializeSingleton(this); must be placed on the Awake method of derived class.
public class SingletonUnity<T> : MonoBehaviour where T : Component
{
	private static T instance;
	private string className;

	protected void InitializeSingleton(T obj, bool dontDestroyOnLoad = true)
	{
		className = typeof(T).Name;
#if LoggingEnabled
		Extenity.Logging.Logger.Log("Instantiating singleton: " + className, obj);
#endif
		instance = obj;

		if (dontDestroyOnLoad)
		{
			DontDestroyOnLoad(this);
		}

		DebugOther.SingletonInstantiated(className);
	}

	protected virtual void OnDestroy()
	{
		if (instance == null)  // To prevent errors in ExecuteInEditMode
			return;

#if LoggingEnabled
		Extenity.Logging.Logger.Log("Destroying singleton: " + className);
#endif
		instance = default(T);
		DebugOther.SingletonDestroyed(className);
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
	public static bool IsInstanceAvailable { get { return !(instance == null); } }
}
