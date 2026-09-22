using System;
using Extenity.MathToolbox;
#if UNITY_5_3_OR_NEWER
using Extenity.SceneManagementToolbox;
using UnityEngine;
#endif

namespace Extenity.UnityTestToolbox
{

	public static class UnityTestTools
	{
		#region Cleanup

		public static void Cleanup()
		{
#if UNITY_5_3_OR_NEWER
			SceneManagerTools.GetScenes(SceneListFilter.LoadedScenes).ForEach(scene =>
			{
				foreach (var rootObject in scene.GetRootGameObjects())
				{
					if (!rootObject.GetComponent("UnityEngine.TestTools.TestRunner.PlaymodeTestsController"))
					{
						GameObject.DestroyImmediate(rootObject);
					}
				}
			});
#endif
		}

		#endregion

		#region Memory Checker

		private static bool MemoryCheckStarted;

#if UNITY_5_3_OR_NEWER
		// GC.GetAllocatedBytesForCurrentThread does not move in Unity's Mono runtime, not even around allocations that
		// certainly happen, so it cannot tell whether anything was allocated. Unity's profiler records a GC.Alloc sample
		// for every managed allocation instead, which is what Unity Test Framework's Is.Not.AllocatingGCMemory()
		// constraint counts. It counts allocations, not bytes.
		private static UnityEngine.Profiling.Recorder AllocationRecorder;
#else
		private static Int64 AllocatedBytesAtMemoryCheckStart;
#endif

		public static void BeginMemoryCheck()
		{
			if (MemoryCheckStarted)
			{
				MemoryCheckStarted = false; // Reset it for further use.
				throw new Exception("Memory check was already started.");
			}

#if UNITY_5_3_OR_NEWER
			if (AllocationRecorder == null)
			{
				AllocationRecorder = UnityEngine.Profiling.Recorder.Get("GC.Alloc");
			}
			if (!AllocationRecorder.isValid)
			{
				throw new InvalidOperationException("The GC.Alloc profiler recorder is not available, so allocations cannot be checked. Memory checks need the Unity Editor or a development build.");
			}

			// Disabling flushes what the recorder captured so far, so the count read at the end covers only the code
			// between the two calls.
			AllocationRecorder.enabled = false;
#if !UNITY_WEBGL
			AllocationRecorder.FilterToCurrentThread();
#endif
			MemoryCheckStarted = true;
			AllocationRecorder.enabled = true;
#else
			MemoryCheckStarted = true;
			AllocatedBytesAtMemoryCheckStart = GC.GetAllocatedBytesForCurrentThread();
#endif
		}

		public static bool EndMemoryCheck()
		{
			if (!MemoryCheckStarted)
			{
				ThrowMemoryCheckNotStarted();
			}

#if UNITY_5_3_OR_NEWER
			AllocationRecorder.enabled = false;
#if !UNITY_WEBGL
			AllocationRecorder.CollectFromAllThreads();
#endif
			MemoryCheckStarted = false;
			var allocationCount = AllocationRecorder.sampleBlockCount;
			if (allocationCount != 0)
			{
				LogDetectedAllocations(allocationCount);
			}
			return allocationCount != 0;
#else
			var change = GC.GetAllocatedBytesForCurrentThread() - AllocatedBytesAtMemoryCheckStart;
			MemoryCheckStarted = false;
			if (change != 0)
			{
				Log.With("MemoryCheck").Warning($"Detected a memory change of '{change:N0}' bytes.");
			}
			return change != 0;
#endif
		}

		// EndMemoryCheck is compiled on its first call, while the recorder is still recording. Mono creates a method's
		// string literals when it compiles the method, so a message written inside EndMemoryCheck showed up as extra
		// allocations of the code under test on the first check of a session. The messages live in these methods, which
		// are only compiled when they run: on a failure, or after recording has stopped.
		private static void ThrowMemoryCheckNotStarted()
		{
			throw new Exception("Memory check was not started.");
		}

#if UNITY_5_3_OR_NEWER
		private static void LogDetectedAllocations(int allocationCount)
		{
			Log.With("MemoryCheck").Warning($"Detected '{allocationCount:N0}' GC allocation(s).");
		}
#endif

		#endregion

		#region Apply A Method Over Value Sets

		public static void ApplyOverValueSet_Int32(Action<Int32> tester)
		{
			tester(0);
			tester(1);
			tester(-1);
#if UNITY_5_3_OR_NEWER
			for (Int32 value = -10000; value < 10000; value += RandomTools.Range(1, 500))
			{
				tester(value);
			}
#endif
			tester(-99999);
			tester(-999999);
			tester(-9999999);
			tester(-99999999);
			tester(-999999999);
			tester(-10000);
			tester(-100000);
			tester(-1000000);
			tester(-10000000);
			tester(-100000000);
			tester(-20000);
			tester(-200000);
			tester(-2000000);
			tester(-20000000);
			tester(-200000000);
			tester(-2000000000);
			tester(10000);
			tester(100000);
			tester(1000000);
			tester(10000000);
			tester(100000000);
			tester(1000000000);
			tester(20000);
			tester(200000);
			tester(2000000);
			tester(20000000);
			tester(200000000);
			tester(2000000000);
			tester(99999);
			tester(999999);
			tester(9999999);
			tester(99999999);
			tester(999999999);
			tester(123456789);
			tester(987654321);
			tester(Int32.MinValue);
			tester(Int32.MinValue + 1);
			tester(Int32.MinValue + 2);
			tester(Int32.MaxValue);
			tester(Int32.MaxValue - 1);
			tester(Int32.MaxValue - 2);
		}

		public static void ApplyOverValueSet_Int64(Action<Int64> tester)
		{
			tester(0);
			tester(1);
			tester(-1);
#if UNITY_5_3_OR_NEWER
			for (Int64 value = -10000; value < 10000; value += RandomTools.Range(1, 500))
			{
				tester(value);
			}
#endif
			tester(-99999);
			tester(-999999);
			tester(-9999999);
			tester(-99999999);
			tester(-999999999);
			tester(-9999999999);
			tester(-99999999999);
			tester(-999999999999);
			tester(-9999999999999);
			tester(-99999999999999);
			tester(-999999999999999);
			tester(-9999999999999999);
			tester(-99999999999999999);
			tester(-999999999999999999);
			tester(-10000);
			tester(-100000);
			tester(-1000000);
			tester(-10000000);
			tester(-100000000);
			tester(-1000000000);
			tester(-10000000000);
			tester(-100000000000);
			tester(-1000000000000);
			tester(-10000000000000);
			tester(-100000000000000);
			tester(-1000000000000000);
			tester(-10000000000000000);
			tester(-100000000000000000);
			tester(-1000000000000000000);
			tester(-20000);
			tester(-200000);
			tester(-2000000);
			tester(-200000000);
			tester(-2000000000);
			tester(-20000000000);
			tester(-200000000000);
			tester(-2000000000000);
			tester(-20000000000000);
			tester(-200000000000000);
			tester(-2000000000000000);
			tester(-20000000000000000);
			tester(-200000000000000000);
			tester(-2000000000000000000);
			tester(10000);
			tester(100000);
			tester(1000000);
			tester(10000000);
			tester(100000000);
			tester(1000000000);
			tester(10000000000);
			tester(100000000000);
			tester(1000000000000);
			tester(10000000000000);
			tester(100000000000000);
			tester(1000000000000000);
			tester(10000000000000000);
			tester(100000000000000000);
			tester(1000000000000000000);
			tester(20000);
			tester(200000);
			tester(2000000);
			tester(200000000);
			tester(2000000000);
			tester(20000000000);
			tester(200000000000);
			tester(2000000000000);
			tester(20000000000000);
			tester(200000000000000);
			tester(2000000000000000);
			tester(20000000000000000);
			tester(200000000000000000);
			tester(2000000000000000000);
			tester(99999);
			tester(999999);
			tester(9999999);
			tester(99999999);
			tester(999999999);
			tester(9999999999);
			tester(99999999999);
			tester(999999999999);
			tester(9999999999999);
			tester(99999999999999);
			tester(999999999999999);
			tester(9999999999999999);
			tester(99999999999999999);
			tester(999999999999999999);
			tester(123456789);
			tester(987654321);
			tester(Int32.MinValue);
			tester(Int32.MinValue + 1);
			tester(Int32.MinValue + 2);
			tester(Int32.MaxValue);
			tester(Int32.MaxValue - 1);
			tester(Int32.MaxValue - 2);
			tester(Int64.MinValue);
			tester(Int64.MinValue + 1);
			tester(Int64.MinValue + 2);
			tester(Int64.MaxValue);
			tester(Int64.MaxValue - 1);
			tester(Int64.MaxValue - 2);
		}

		public static void ApplyOverValueSet_Double(Action<double> tester)
		{
			tester(0);
			tester(1);
			tester(-1);
#if UNITY_5_3_OR_NEWER
			for (double value = -10000d; value < 10000d; value += RandomTools.Range(0.1f, 500.0f))
			{
				tester(value);
			}
#endif
			tester(-99999);
			tester(-999999);
			tester(-9999999);
			tester(-99999999);
			tester(-999999999);
			tester(-9999999999);
			tester(-99999999999);
			tester(-999999999999);
			tester(-9999999999999);
			tester(-99999999999999);
			tester(-999999999999999);
			tester(-9999999999999999);
			tester(-99999999999999999);
			tester(-999999999999999999);
			tester(-10000);
			tester(-100000);
			tester(-1000000);
			tester(-10000000);
			tester(-100000000);
			tester(-1000000000);
			tester(-10000000000);
			tester(-100000000000);
			tester(-1000000000000);
			tester(-10000000000000);
			tester(-100000000000000);
			tester(-1000000000000000);
			tester(-10000000000000000);
			tester(-100000000000000000);
			tester(-1000000000000000000);
			tester(-20000);
			tester(-200000);
			tester(-2000000);
			tester(-200000000);
			tester(-2000000000);
			tester(-20000000000);
			tester(-200000000000);
			tester(-2000000000000);
			tester(-20000000000000);
			tester(-200000000000000);
			tester(-2000000000000000);
			tester(-20000000000000000);
			tester(-200000000000000000);
			tester(-2000000000000000000);
			tester(10000);
			tester(100000);
			tester(1000000);
			tester(10000000);
			tester(100000000);
			tester(1000000000);
			tester(10000000000);
			tester(100000000000);
			tester(1000000000000);
			tester(10000000000000);
			tester(100000000000000);
			tester(1000000000000000);
			tester(10000000000000000);
			tester(100000000000000000);
			tester(1000000000000000000);
			tester(20000);
			tester(200000);
			tester(2000000);
			tester(200000000);
			tester(2000000000);
			tester(20000000000);
			tester(200000000000);
			tester(2000000000000);
			tester(20000000000000);
			tester(200000000000000);
			tester(2000000000000000);
			tester(20000000000000000);
			tester(200000000000000000);
			tester(2000000000000000000);
			tester(99999);
			tester(999999);
			tester(9999999);
			tester(99999999);
			tester(999999999);
			tester(9999999999);
			tester(99999999999);
			tester(999999999999);
			tester(9999999999999);
			tester(99999999999999);
			tester(999999999999999);
			tester(9999999999999999);
			tester(99999999999999999);
			tester(999999999999999999);
			tester(123456789);
			tester(987654321);
			tester(float.MinValue);
			tester(float.MinValue + 1);
			tester(float.MinValue + 2);
			tester(float.MaxValue);
			tester(float.MaxValue - 1);
			tester(float.MaxValue - 2);
			tester(double.MinValue);
			tester(double.MinValue + 1);
			tester(double.MinValue + 2);
			tester(double.MaxValue);
			tester(double.MaxValue - 1);
			tester(double.MaxValue - 2);
		}

		#endregion
	}

}
