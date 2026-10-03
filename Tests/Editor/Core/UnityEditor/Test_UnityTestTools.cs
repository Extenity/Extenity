using System;
using System.Threading;
using Extenity.DataToolbox;
using Extenity.Testing;
using Extenity.UnityTestToolbox;
using NUnit.Framework;
using UnityEngine;

namespace ExtenityTests.UnityTestToolbox
{

	public class Test_UnityTestTools : ExtenityTestBase
	{
		#region Memory Checker

		// Keeps the test allocations reachable, so they are not optimized away.
		private static object KeptAlive;

		[Test]
		public void MemoryCheck_ReportsEveryAllocation()
		{
			var value = Environment.TickCount;

			UnityTestTools.BeginMemoryCheck();
			KeptAlive = new object();
			KeptAlive = new byte[16];
			KeptAlive = value; // Boxing
			var allocated = UnityTestTools.EndMemoryCheck();

			Assert.IsTrue(allocated, "The memory check missed allocations that certainly happened.");
			AssertExpectLog((LogType.Warning, StringFilterEntry.CreateContains("Detected '3' GC allocation(s).")));
		}

		[Test]
		public void MemoryCheck_ReportsNothingWhenNothingIsAllocated()
		{
			var value = Environment.TickCount;

			UnityTestTools.BeginMemoryCheck();
			var sum = 0;
			for (int i = 0; i < 100; i++)
			{
				sum += value ^ i;
			}
			var allocated = UnityTestTools.EndMemoryCheck();

			Assert.IsFalse(allocated, $"The memory check reported allocations in code that makes none (sum {sum}).");
		}

		private static volatile bool StopBackgroundAllocations;
		private static object BackgroundKeptAlive;

		[Test]
		public void MemoryCheck_IgnoresAllocationsOfOtherThreads()
		{
			const int CheckCount = 200000;

			StopBackgroundAllocations = false;
			var allocatorThread = new Thread(() =>
			{
				while (!StopBackgroundAllocations)
				{
					BackgroundKeptAlive = new byte[32];
				}
			});
			allocatorThread.IsBackground = true;
			allocatorThread.Start();

			var falseDetectionAt = -1;
			try
			{
				for (int i = 0; i < CheckCount; i++)
				{
					UnityTestTools.BeginMemoryCheck();
					if (UnityTestTools.EndMemoryCheck())
					{
						falseDetectionAt = i;
						break;
					}
				}
			}
			finally
			{
				StopBackgroundAllocations = true;
				allocatorThread.Join();
			}

			if (falseDetectionAt >= 0)
			{
				Assert.Fail($"The memory check reported another thread's allocation as its own at check '{falseDetectionAt}' of '{CheckCount}'.");
			}
		}

		#endregion
	}

}
