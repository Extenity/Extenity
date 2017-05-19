using UnityEngine;
using System.Collections.Generic;
using Conditional = System.Diagnostics.ConditionalAttribute;

public class DebugOther
{
	#region Singleton Tracker

	private static Dictionary<string, int> singletonCalls = new Dictionary<string, int>();

	public static Dictionary<string, int> SingletonCalls
	{
		get { return singletonCalls; }
	}

	[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
	public static void SingletonInstantiated()
	{
		SingletonInstantiated(DebugReflection.PreviousMethodType);
	}

	[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
	public static void SingletonInstantiated(string className)
	{
		if (singletonCalls.ContainsKey(className))
		{
			singletonCalls[className]++;
			if (singletonCalls[className] > 1)
			{
				Debug.LogError("Singleton '" + className + "' instantiated " + singletonCalls[className] + " times");
			}
		}
		else
			singletonCalls.Add(className, 1);
	}

	[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
	public static void SingletonDestroyed()
	{
		SingletonDestroyed(DebugReflection.PreviousMethodType);
	}

	[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
	public static void SingletonDestroyed(string className)
	{
		if (singletonCalls.ContainsKey(className))
			singletonCalls[className]--;
		else
			Debug.LogError("Unregistered singleton: " + className);
	}

	[Conditional("UNITY_EDITOR"), Conditional("DEBUG")]
	public static void DrawSingletonTrackerDebugGUI()
	{
		int lineHeight = 14;

		Rect rect1 = new Rect(100, 20, 210, 20);
		Rect rect2 = new Rect(320, 20, 100, 20);

		GUI.Label(new Rect(130, 5, 160, 20), "Singleton Tracker");

		foreach (KeyValuePair<string, int> pair in singletonCalls)
		{
			GUI.Label(rect1, pair.Key);
			GUI.Label(rect2, pair.Value.ToString());
			rect1.y += lineHeight;
			rect2.y += lineHeight;
		}
	}

	#endregion
}
