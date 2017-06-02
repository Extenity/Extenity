using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

namespace Extenity.OperatingSystemToolbox
{

#if UNITY_STANDALONE_WIN

	public static class PrecisionTiming
	{
		private static double HighPerformanceCounterFrequency;

		[DllImport("Kernel32.dll")]
		[System.Security.SuppressUnmanagedCodeSecurity()]
		private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);
		[DllImport("Kernel32.dll")]
		[System.Security.SuppressUnmanagedCodeSecurity()]
		private static extern bool QueryPerformanceFrequency(out long lpFrequency);

		[DllImport("winmm.dll")]
		[System.Security.SuppressUnmanagedCodeSecurity()]
		public static extern int timeBeginPeriod(int uPeriod);
		[DllImport("winmm.dll")]
		[System.Security.SuppressUnmanagedCodeSecurity()]
		public static extern int timeEndPeriod(int uPeriod);

		public static void PrecisionSleep(int msec)
		{
			timeBeginPeriod(msec);
			Thread.Sleep(msec);
			timeEndPeriod(msec);
		}

		public static double PreciseTime
		{
			get
			{
				if (HighPerformanceCounterFrequency == 0)
				{
					long freq;
					if (QueryPerformanceFrequency(out freq) == false)
					{
						// high-performance counter not supported
						throw new Win32Exception();
					}
					HighPerformanceCounterFrequency = freq;
				}

				long counter;
				QueryPerformanceCounter(out counter);

				return counter / HighPerformanceCounterFrequency;
			}
		}
	}

#endif

}
