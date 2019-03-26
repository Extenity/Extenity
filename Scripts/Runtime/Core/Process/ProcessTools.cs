using System.Diagnostics;

namespace Extenity.ProcessToolbox
{

	public static class ProcessTools
	{

		public static void SetProcessPriority(ProcessPriorityClass priority)
		{
			using (var process = Process.GetCurrentProcess())
			{
				process.PriorityClass = priority;
			}
		}

	}

}
