using System.Diagnostics;

namespace Extenity.DesignPatternsToolbox
{

	// Usage:
	//   Use "new" to create singleton.
	//   InitializeSingleton(...); must be placed on the constructor of derived class.
	public class Singleton<T> where T : Singleton<T>
	{
		protected virtual void OnDestroySingleton() { }

		private static T _Instance;
#pragma warning disable 414
		private string ClassName;
#pragma warning restore

		protected void InitializeSingleton()
		{
			ClassName = typeof(T).Name;
			Log.With("Singleton").Verbose("Instantiating singleton: " + ClassName);
			_Instance = this as T;

			SingletonTracker.SingletonInstantiated(ClassName);
		}

		public void DestroySingleton()
		{
			Log.With("Singleton").Verbose("Destroying singleton: " + ClassName);

			OnDestroySingleton();

			_Instance = default(T);
			SingletonTracker.SingletonDestroyed(ClassName);
		}

		public static T Instance
		{
			[DebuggerStepThrough]
			get => _Instance;
		}
		public static bool IsInstanceAvailable
		{
			[DebuggerStepThrough]
			get => _Instance != null;
		}
	}

}
