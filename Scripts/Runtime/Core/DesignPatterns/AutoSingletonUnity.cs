#if UNITY

using System;
using System.Diagnostics;
using UnityEngine;

namespace Extenity.DesignPatternsToolbox
{

	// Usage:
	//   Use the derived class as a Component of a GameObject.
	//   InitializeSingleton(...); must be placed on the Awake method of derived class.
	public class AutoSingletonUnity<T> : MonoBehaviour where T : AutoSingletonUnity<T>
	{
		private static T _Instance;
#pragma warning disable 414
		private string ClassName;
#pragma warning restore

#if !ManuallyInitializeSingletons
		protected virtual void AwakeDerived() { }
		protected void Awake()
		{
			InitializeSingleton();

			AwakeDerived();
		}
#endif

		protected T InitializeSingleton(bool dontDestroyOnLoad = false)
		{
			ClassName = typeof(T).Name;
			Log.With("Singleton").VerboseWithContext(this, "Instantiating singleton: " + ClassName);
			_Instance = this as T;

			if (dontDestroyOnLoad)
			{
				DontDestroyOnLoad(this);
			}

#if DEBUG
			if (!typeof(T).FullName.Equals(GetType().FullName, StringComparison.Ordinal))
			{
				Log.With("Singleton").Fatal($"Singleton '{typeof(T).Name}' is derived from a different generic class '{GetType().Name}'.");
			}
#endif

			SingletonTracker.SingletonInstantiated(ClassName);

			// Returning the instance for ease of use. When there are double derived singleton classes,
			// they need to keep their own static instance fields. Returning the instance here allows
			// these fields to be set directly in one-liner code where InitializeSingleton is called.
			//
			// Note that with strict initialization of singletons, InitializeSingleton is called by
			// singleton itself. So you need to define ManuallyInitializeSingletons directive and
			// initialize singletons by yourself.
			return _Instance;
		}

		protected virtual void OnDestroyDerived() { }

		/// <summary>
		/// Derived classes should implement OnDestroyDerived.
		/// </summary>
		protected void OnDestroy()
		{
			if (_Instance == null)  // To prevent errors in ExecuteInEditMode
				return;

			Log.With("Singleton").Verbose("Destroying singleton: " + ClassName);

			_Instance = default(T);
			SingletonTracker.SingletonDestroyed(ClassName);

			OnDestroyDerived();
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
			if (_Instance.gameObject.GetComponents<Component>().Length == 2) // 1 for 'Transform' component and 1 for 'T' component
			{
				// If this component is the only one left in gameobject, destroy the gameobject as well
				Destroy(_Instance.gameObject);
			}
			else
			{
				// Destroy only this component
				Destroy(_Instance);
			}
		}

		public static T Instance
		{
			[DebuggerStepThrough]
			get
			{
				if (_Instance == null)
				{
					_Instance = CreateSingleton("_" + typeof(T).Name);
				}
				return _Instance;
			}
		}
		public static bool IsInstanceAvailable
		{
			[DebuggerStepThrough]
			get => _Instance;
		}
		public static bool IsInstanceEnabled
		{
			[DebuggerStepThrough]
			get => _Instance && _Instance.isActiveAndEnabled;
		}
	}

}

#endif
