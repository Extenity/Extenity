using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Extenity.DesignPatternsToolbox
{

	internal static class SingletonTracker
	{
		#region Initialization / Deinitialization

#if UNITY_EDITOR

		[InitializeOnLoadMethod]
		private static void Initialize()
		{
			EditorApplication.playModeStateChanged -= Deinitialize;
			EditorApplication.playModeStateChanged += Deinitialize;
		}

		private static void Deinitialize(PlayModeStateChange change)
		{
			EditorApplication.playModeStateChanged -= Deinitialize;

			// Clear all static data to support Enable Play Mode Options.
			_SingletonCalls.Clear();
		}

#endif

		#endregion

		#region Singleton Tracker

		private static Dictionary<string, int> _SingletonCalls = new Dictionary<string, int>();
		public static Dictionary<string, int> SingletonCalls => _SingletonCalls;

		public static void SingletonInstantiated(string className)
		{
			if (_SingletonCalls.ContainsKey(className))
			{
				_SingletonCalls[className]++;
				if (_SingletonCalls[className] > 1)
				{
					Log.Error("Singleton '" + className + "' instantiated " + _SingletonCalls[className] + " times");
				}
			}
			else
			{
				_SingletonCalls.Add(className, 1);
			}
		}

		public static void SingletonDestroyed(string className)
		{
			if (_SingletonCalls.ContainsKey(className))
			{
				_SingletonCalls[className]--;
			}
			else
			{
				Log.Error("Unregistered singleton: " + className);
			}
		}

		#endregion
	}

}
