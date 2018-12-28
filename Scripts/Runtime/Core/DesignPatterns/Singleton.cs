//#define LogSingletonInEditor
//#define LogSingletonInBuilds
#define LogSingletonInDebugBuilds

#if (UNITY_EDITOR && LogSingletonInEditor) || (!UNITY_EDITOR && LogSingletonInBuilds) || (!UNITY_EDITOR && DEBUG && LogSingletonInDebugBuilds)
#define LoggingEnabled
#else
#undef LoggingEnabled
#endif

namespace Extenity.DesignPatternsToolbox
{

	// Usage:
	//   Use "new" to create singleton.
	//   InitializeSingleton(...); must be placed on the constructor of derived class.
	public class Singleton<T> where T : Singleton<T>
	{
		protected virtual void OnDestroySingleton() { }

		private static T instance;
#pragma warning disable 414
		private string className;
#pragma warning restore

		protected void InitializeSingleton()
		{
			className = typeof(T).Name;
#if LoggingEnabled
			Log.Info("Instantiating singleton: " + className);
#endif

			instance = this as T;
			SingletonTracker.SingletonInstantiated(className);
		}

		public void DestroySingleton()
		{
#if LoggingEnabled
			Log.Info("Destroying singleton: " + className);
#endif

			OnDestroySingleton();

			instance = default(T);

			SingletonTracker.SingletonDestroyed(className);
		}

		public static T Instance { get { return instance; } }
		public static bool IsInstanceAvailable { get { return instance != null; } }
	}

}
