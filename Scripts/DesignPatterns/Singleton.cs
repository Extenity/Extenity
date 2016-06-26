//#define LogSingletonInEditor
#define LogSingletonInBuilds

#if (UNITY_EDITOR && LogSingletonInEditor) || (!UNITY_EDITOR && LogSingletonInBuilds)
#define LoggingEnabled
#endif

// Usage:
//   Use "new" to create singleton.
//   InitializeSingleton(this); must be placed on the constructor of derived class.
public class Singleton<T>
{
	virtual protected void OnDestroySingleton() { }

	private static T instance;
	private string className;

	protected void InitializeSingleton(T obj)
	{
		className = typeof(T).Name;
#if LoggingEnabled
		Extenity.Logging.Logger.Log("Instantiating singleton: " + className);
#endif

		instance = obj;
		DebugOther.SingletonInstantiated(className);
	}

	public void DestroySingleton()
	{
#if LoggingEnabled
		Extenity.Logging.Logger.Log("Destroying singleton: " + className);
#endif

		OnDestroySingleton();

		instance = default(T);

		DebugOther.SingletonDestroyed(className);
	}

	public static T Instance { get { return instance; } }
	public static bool IsInstanceAvailable { get { return !(instance == null); } }
}
