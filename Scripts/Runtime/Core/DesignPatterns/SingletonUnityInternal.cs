using UnityEngine;

namespace Extenity.DesignPatternsToolbox
{

	// Usage:
	//   Use the derived class as a Component of a GameObject.
	//   InitializeSingleton(this); must be placed on the Awake method of derived class.
	public class SingletonUnityInternal<T> : MonoBehaviour where T : Component
	{
		private static T instance;
		private string className;

		protected void InitializeSingleton(T obj, bool dontDestroyOnLoad = true)
		{
			className = typeof(T).Name;
			Debug.Log("Instantiating internal singleton: " + className, obj);
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

			Debug.Log("Destroying internal singleton: " + className);
			instance = default(T);
			SingletonTracker.SingletonDestroyed(className);
		}

		internal static T CreateSingleton(string addedGameObjectName = "_")
		{
			var go = GameObject.Find(addedGameObjectName);
			if (go == null)
				go = new GameObject(addedGameObjectName);
			return go.AddComponent<T>();
		}

		internal static void DestroySingleton()
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

		internal static T Instance { get { return instance; } }
		internal static bool IsInstanceAvailable { get { return !(instance == null); } }
	}

}
