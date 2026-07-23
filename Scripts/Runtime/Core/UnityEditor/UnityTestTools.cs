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
		private static Int64 AllocatedBytesAtMemoryCheckStart;

		public static void BeginMemoryCheck()
		{
			if (MemoryCheckStarted)
			{
				MemoryCheckStarted = false; // Reset it for further use.
				throw new Exception("Memory check was already started.");
			}

			MemoryCheckStarted = true;
			AllocatedBytesAtMemoryCheckStart = GC.GetAllocatedBytesForCurrentThread();
		}

		public static bool EndMemoryCheck()
		{
			if (!MemoryCheckStarted)
			{
				throw new Exception("Memory check was not started.");
			}

			var change = GC.GetAllocatedBytesForCurrentThread() - AllocatedBytesAtMemoryCheckStart;
			MemoryCheckStarted = false;
			if (change != 0)
			{
				Log.With("MemoryCheck").Warning($"Detected a memory change of '{change:N0}' bytes.");
			}
			return change != 0;
		}

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
