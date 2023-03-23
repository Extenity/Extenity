using System.Collections.Generic;
using Extenity.ApplicationToolbox;
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
			if (change == PlayModeStateChange.EnteredEditMode)
			{
				// Clear all static data to support Enable Play Mode Options.
				_SingletonCalls.Clear();
			}
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
				if (_SingletonCalls[className] < 0)
				{
					Log.With("Singleton").Error($"Singleton '{className}' tracker was below zero");
					_SingletonCalls[className] = 0;
				}
				_SingletonCalls[className]++;
				if (_SingletonCalls[className] > 1)
				{
					Log.With("Singleton").Error($"Singleton '{className}' instantiated {_SingletonCalls[className]} times");
				}
			}
			else
			{
				_SingletonCalls.Add(className, 1);
			}
		}

		public static void SingletonDestroyed(string className)
		{
#if UNITY
			// Enter Playmode Options is a funny tool. It will leave static variables
			// in all kinds of unpleasant states. So this was the easiest way. Just stop
			// tracking singletons when quitting play mode.
			//
			// This behaviour needs a bit more inspection to be fair, but SingletonTracker
			// is just a helper tool to track singleton instantiation at runtime and it's
			// okay not to track singletons when closing the application.
			if (ApplicationTools.IsShuttingDown)
			{
				// Clearing is required, because calling order between playModeStateChanged
				// and OnDestroy will vary wildly depending on the singleton script
				// having ExecuteAlways attribute. So just clear _SingletonCalls in both
				// occasions to make sure everything works smooth.
				_SingletonCalls.Clear();
				return;
			}
#endif

			if (_SingletonCalls.ContainsKey(className))
			{
				_SingletonCalls[className]--;
			}
			else
			{
				Log.With("Singleton").Error("Unregistered singleton: " + className);
			}
		}

		#endregion
	}

}
