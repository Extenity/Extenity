using UnityEngine;
using System.Collections;
using System.Diagnostics;

public static class ProcessTools
{
#if !UNITY_WEBPLAYER

	public static void SetProcessPriority(ProcessPriorityClass priority)
	{
		using (Process process = Process.GetCurrentProcess())
		{
			process.PriorityClass = priority;
		}
	}

#endif
}
