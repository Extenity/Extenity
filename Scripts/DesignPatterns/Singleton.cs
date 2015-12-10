using UnityEngine;
using Extenity.Logging;
using Logger = Extenity.Logging.Logger;

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
		Logger.Log("Instantiating singleton: " + className);

		instance = obj;
		DebugOther.SingletonInstantiated(className);
	}

	public void DestroySingleton()
	{
		Logger.Log("Destroying singleton: " + className);

		OnDestroySingleton();

		instance = default(T);

		DebugOther.SingletonDestroyed(className);
	}

	public static T Instance { get { return instance; } }
	public static bool IsInstanceAvailable { get { return !(instance == null); } }
}
