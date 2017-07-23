using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

namespace Extenity.ApplicationToolbox
{

	public static class PrecisionTiming
	{
		#region Precise Sleep

#if UNITY_STANDALONE_WIN

		[DllImport("winmm.dll")]
		[SuppressUnmanagedCodeSecurity]
		public static extern int timeBeginPeriod(int uPeriod);
		[DllImport("winmm.dll")]
		[SuppressUnmanagedCodeSecurity]
		public static extern int timeEndPeriod(int uPeriod);

		public static void PreciseSleep(int msec)
		{
			timeBeginPeriod(msec);
			Thread.Sleep(msec);
			timeEndPeriod(msec);
		}

#endif

		#endregion

		#region Precise Time

		public static double PreciseTime
		{
			get { return (double)Stopwatch.GetTimestamp() / Stopwatch.Frequency; }
		}

		#endregion

		#region Precise Time - Old Implementation

		// Decided to use Stopwatch's wrapper to access performance counter since it's platform independent and works if system does not support high resolution timer.

		//private static double HighPerformanceCounterFrequency;

		//[DllImport("Kernel32.dll")]
		//[SuppressUnmanagedCodeSecurity]
		//private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);
		//[DllImport("Kernel32.dll")]
		//[SuppressUnmanagedCodeSecurity]
		//private static extern bool QueryPerformanceFrequency(out long lpFrequency);

		//public static double PreciseTime
		//{
		//	get
		//	{
		//		if (HighPerformanceCounterFrequency == 0)
		//		{
		//			long freq;
		//			if (QueryPerformanceFrequency(out freq) == false)
		//			{
		//				// high-performance counter not supported
		//				throw new Win32Exception();
		//			}
		//			HighPerformanceCounterFrequency = freq;
		//		}

		//		long counter;
		//		QueryPerformanceCounter(out counter);

		//		return counter / HighPerformanceCounterFrequency;
		//	}
		//}

		#endregion
	}

}
