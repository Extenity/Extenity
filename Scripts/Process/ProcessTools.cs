using System.Diagnostics;

namespace Extenity.ProcessToolbox
{

	public static class ProcessTools
	{
#if !UNITY_WEBPLAYER

		public static void SetProcessPriority(ProcessPriorityClass priority)
		{
			using (var process = Process.GetCurrentProcess())
			{
				process.PriorityClass = priority;
			}
		}

#endif
	}

}
