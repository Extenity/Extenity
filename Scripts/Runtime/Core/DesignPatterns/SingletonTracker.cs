using System.Collections.Generic;

namespace Extenity.DesignPatternsToolbox
{

	internal static class SingletonTracker
	{
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
