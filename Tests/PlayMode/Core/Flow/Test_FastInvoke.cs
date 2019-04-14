using System.Collections;
using Extenity.FlowToolbox;
using ExtenityTests.Common;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ExtenityTests.FlowToolbox
{

	public class Test_FastInvoke : TestBase_FastInvoke
	{
		private const int CheesyRepeats = 1;
		private const int DetailedRepeats = 20;
		private const int OvernightRepeats = 1000;
		private const int LongRunRepeats = 1; // Long runs are already taking too long. So don't repeat them.
		private const int CheesyLongRunDuration = 100; // 1 second in realtime
		private const int DetailedLongRunDuration = 20 * 100; // 20 seconds in realtime
		private const int OvernightLongRunDuration = 10 * 60 * 100; // 10 minutes in realtime

		#region Test Definitions

		// ---------- UnityInvoke

		// UnityInvoke_Zero
		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_Zero() { yield return TestInvoke_Zero(CheesyRepeats, DoUnityInvoke); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_Zero_Detailed() { yield return TestInvoke_Zero(DetailedRepeats, DoUnityInvoke); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_Zero_Overnight() { yield return TestInvoke_Zero(OvernightRepeats, DoUnityInvoke); }

		// UnityInvoke_Various_StartsAtRandomTime
		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_Various_StartsAtRandomTime() { yield return TestInvoke_Various(true, CheesyRepeats, DoUnityInvoke); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_Various_StartsAtRandomTime_Detailed() { yield return TestInvoke_Various(true, DetailedRepeats, DoUnityInvoke); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_Various_StartsAtRandomTime_Overnight() { yield return TestInvoke_Various(true, OvernightRepeats, DoUnityInvoke); }

		// UnityInvoke_Various_StartsAtFixedUpdate
		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_Various_StartsAtFixedUpdate() { yield return TestInvoke_Various(false, CheesyRepeats, DoUnityInvoke); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_Various_StartsAtFixedUpdate_Detailed() { yield return TestInvoke_Various(false, DetailedRepeats, DoUnityInvoke); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_Various_StartsAtFixedUpdate_Overnight() { yield return TestInvoke_Various(false, OvernightRepeats, DoUnityInvoke); }

		// UnityInvoke_LongRun_StartsAtRandomTime
		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_LongRun_StartsAtRandomTime() { yield return TestInvoke(LongRunRepeats, CheesyLongRunDuration, true, DoUnityInvoke); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_LongRun_StartsAtRandomTime_Detailed() { yield return TestInvoke(LongRunRepeats, DetailedLongRunDuration, true, DoUnityInvoke); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_LongRun_StartsAtRandomTime_Overnight() { yield return TestInvoke(LongRunRepeats, OvernightLongRunDuration, true, DoUnityInvoke); }

		// UnityInvoke_LongRun_StartsAtFixedUpdate
		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_LongRun_StartsAtFixedUpdate() { yield return TestInvoke(LongRunRepeats, CheesyLongRunDuration, false, DoUnityInvoke); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_LongRun_StartsAtFixedUpdate_Detailed() { yield return TestInvoke(LongRunRepeats, DetailedLongRunDuration, false, DoUnityInvoke); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_LongRun_StartsAtFixedUpdate_Overnight() { yield return TestInvoke(LongRunRepeats, OvernightLongRunDuration, false, DoUnityInvoke); }

		// ---------- FastInvoke

		// FastInvoke_Zero
		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_Zero() { yield return TestInvoke_Zero(CheesyRepeats, DoFastInvoke); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_Zero_Detailed() { yield return TestInvoke_Zero(DetailedRepeats, DoFastInvoke); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_Zero_Overnight() { yield return TestInvoke_Zero(OvernightRepeats, DoFastInvoke); }

		// FastInvoke_Various_StartsAtRandomTime
		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_Various_StartsAtRandomTime() { yield return TestInvoke_Various(true, CheesyRepeats, DoFastInvoke); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_Various_StartsAtRandomTime_Detailed() { yield return TestInvoke_Various(true, DetailedRepeats, DoFastInvoke); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_Various_StartsAtRandomTime_Overnight() { yield return TestInvoke_Various(true, OvernightRepeats, DoFastInvoke); }

		// FastInvoke_Various_StartsAtFixedUpdate
		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_Various_StartsAtFixedUpdate() { yield return TestInvoke_Various(false, CheesyRepeats, DoFastInvoke); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_Various_StartsAtFixedUpdate_Detailed() { yield return TestInvoke_Various(false, DetailedRepeats, DoFastInvoke); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_Various_StartsAtFixedUpdate_Overnight() { yield return TestInvoke_Various(false, OvernightRepeats, DoFastInvoke); }

		// FastInvoke_LongRun_StartsAtRandomTime
		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_LongRun_StartsAtRandomTime() { yield return TestInvoke(LongRunRepeats, CheesyLongRunDuration, true, DoFastInvoke); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_LongRun_StartsAtRandomTime_Detailed() { yield return TestInvoke(LongRunRepeats, DetailedLongRunDuration, true, DoFastInvoke); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_LongRun_StartsAtRandomTime_Overnight() { yield return TestInvoke(LongRunRepeats, OvernightLongRunDuration, true, DoFastInvoke); }

		// FastInvoke_LongRun_StartsAtFixedUpdate
		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_LongRun_StartsAtFixedUpdate() { yield return TestInvoke(LongRunRepeats, CheesyLongRunDuration, false, DoFastInvoke); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_LongRun_StartsAtFixedUpdate_Detailed() { yield return TestInvoke(LongRunRepeats, DetailedLongRunDuration, false, DoFastInvoke); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_LongRun_StartsAtFixedUpdate_Overnight() { yield return TestInvoke(LongRunRepeats, OvernightLongRunDuration, false, DoFastInvoke); }

		// ---------- FastInvoke Consistency

		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_Cancel()
		{
			yield return InitializeTest(true);
			var fixedDeltaTime = Time.fixedDeltaTime;

			// Invoke and cancel immediately
			Subject.FastInvoke(Subject.Callback, 1);
			DoFastInvokingChecks(true);
			Subject.CancelFastInvoke(Subject.Callback);
			DoFastInvokingChecks(false);

			// Invoke and wait for half of it, then cancel
			Subject.FastInvoke(Subject.Callback, 1);
			while (Subject.RemainingTimeUntilNextFastInvoke() > 0.5)
				yield return new WaitForFixedUpdate(); // Ignored by Code Correct
			DoFastInvokingChecks(true);
			Subject.CancelFastInvoke(Subject.Callback);
			DoFastInvokingChecks(false);

			// Invoke more than one and cancel immediately
			Subject.FastInvoke(Subject.Callback, 1);
			Subject.FastInvoke(Subject.Callback, 0.6);
			Subject.FastInvoke(Subject.Callback, 0.4);
			Subject.FastInvoke(Subject.Callback, 0.2);
			DoFastInvokingChecks(true);
			Subject.CancelFastInvoke(Subject.Callback);
			DoFastInvokingChecks(false);

			// Invoke more than one, wait for half of it, then cancel
			Subject.FastInvoke(Subject.Callback, 1);
			DoFastInvokingChecks(true);
			Subject.FastInvoke(Subject.Callback, 0.6);
			DoFastInvokingChecks(true);
			Subject.FastInvoke(Subject.Callback, 0.4);
			DoFastInvokingChecks(true);
			Subject.FastInvoke(Subject.Callback, 0.2);
			DoFastInvokingChecks(true);
			for (int i = 0; i < 0.5f / fixedDeltaTime; i++)
				yield return new WaitForFixedUpdate(); // Ignored by Code Correct
			DoFastInvokingChecks(true);
			Assert.AreEqual(0.1, Subject.RemainingTimeUntilNextFastInvoke(), fixedDeltaTime);
			Subject.CancelFastInvoke(Subject.Callback);
			DoFastInvokingChecks(false);
		}

		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_RemainingTime()
		{
			yield return InitializeTest(true);
			Assert.IsTrue(double.IsNaN(Subject.RemainingTimeUntilNextFastInvoke()));
			var fixedDeltaTime = Time.fixedDeltaTime;

			// Try different orders
			Subject.FastInvoke(Subject.Callback, 5);
			Assert.AreEqual(5, Subject.RemainingTimeUntilNextFastInvoke(), fixedDeltaTime);
			Subject.FastInvoke(Subject.Callback, 4);
			Assert.AreEqual(4, Subject.RemainingTimeUntilNextFastInvoke(), fixedDeltaTime);
			Subject.FastInvoke(Subject.Callback, 3);
			Assert.AreEqual(3, Subject.RemainingTimeUntilNextFastInvoke(), fixedDeltaTime);
			Subject.FastInvoke(Subject.Callback, 2);
			Assert.AreEqual(2, Subject.RemainingTimeUntilNextFastInvoke(), fixedDeltaTime);
			Subject.CancelAllFastInvokes();
			Assert.IsTrue(double.IsNaN(Subject.RemainingTimeUntilNextFastInvoke()));

			// Try different orders
			Subject.FastInvoke(Subject.Callback, 3);
			Assert.AreEqual(3, Subject.RemainingTimeUntilNextFastInvoke(), fixedDeltaTime);
			Subject.FastInvoke(Subject.Callback, 4);
			Assert.AreEqual(3, Subject.RemainingTimeUntilNextFastInvoke(), fixedDeltaTime);
			Subject.FastInvoke(Subject.Callback, 2);
			Assert.AreEqual(2, Subject.RemainingTimeUntilNextFastInvoke(), fixedDeltaTime);
			Subject.FastInvoke(Subject.Callback, 5);
			Assert.AreEqual(2, Subject.RemainingTimeUntilNextFastInvoke(), fixedDeltaTime);
			double time = 0;
			var WaitForFixedUpdate = new WaitForFixedUpdate();
			do yield return WaitForFixedUpdate; while ((time += fixedDeltaTime) < 0.5);
			Assert.AreEqual(1.5, Subject.RemainingTimeUntilNextFastInvoke(), fixedDeltaTime);
			do yield return WaitForFixedUpdate; while ((time += fixedDeltaTime) < 0.9);
			Assert.AreEqual(1.1, Subject.RemainingTimeUntilNextFastInvoke(), fixedDeltaTime);
			do yield return WaitForFixedUpdate; while ((time += fixedDeltaTime) < 1.9);
			DoFastInvokingChecks(true, 4); // Just before invoking the first one, that is set to 2 seconds
			Assert.AreEqual(0.1, Subject.RemainingTimeUntilNextFastInvoke(), fixedDeltaTime);
			do yield return WaitForFixedUpdate; while ((time += fixedDeltaTime) < 2.0);
			DoFastInvokingChecks(true, 3);
			Assert.AreEqual(1.0, Subject.RemainingTimeUntilNextFastInvoke(), fixedDeltaTime);
			do yield return WaitForFixedUpdate; while ((time += fixedDeltaTime) < 2.9);
			DoFastInvokingChecks(true, 3);
			Assert.AreEqual(0.1, Subject.RemainingTimeUntilNextFastInvoke(), fixedDeltaTime);
			do yield return WaitForFixedUpdate; while ((time += fixedDeltaTime) < 3.0);
			DoFastInvokingChecks(true, 2);
			Assert.AreEqual(1.0, Subject.RemainingTimeUntilNextFastInvoke(), fixedDeltaTime);
			do yield return WaitForFixedUpdate; while ((time += fixedDeltaTime) < 4.0);
			DoFastInvokingChecks(true, 1);
			Assert.AreEqual(1.0, Subject.RemainingTimeUntilNextFastInvoke(), fixedDeltaTime);
			do yield return WaitForFixedUpdate; while ((time += fixedDeltaTime) < 4.4);
			DoFastInvokingChecks(true, 1);
			Assert.AreEqual(0.6, Subject.RemainingTimeUntilNextFastInvoke(), fixedDeltaTime);
			do yield return WaitForFixedUpdate; while ((time += fixedDeltaTime) < 5.0);
			DoFastInvokingChecks(false, 0);
			Assert.IsTrue(double.IsNaN(Subject.RemainingTimeUntilNextFastInvoke()));
		}

		// ---------- FastInvoke Other Specialities

		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_AllowsRegisteringMethodsOutsideOfBehaviour()
		{
			yield return TestInvoke(0, true, DoOutsiderFastInvoke);
			yield return TestInvoke(1, true, DoOutsiderFastInvoke);

			yield return TestInvoke(0, false, DoOutsiderFastInvoke);
			yield return TestInvoke(1, false, DoOutsiderFastInvoke);
		}

		#endregion
	}

}
