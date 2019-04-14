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
		#region Configuration - Timing

		private const int CheesyRepeats = 1;
		private const int DetailedRepeats = 20;
		private const int OvernightRepeats = 1000;

		private const int LongRunRepeats = 1; // Long runs are already taking too long. So don't repeat them.
		private const double CheesyLongRunDuration = 1 * TimeScale; // 1 second in realtime
		private const double DetailedLongRunDuration = 20 * TimeScale; // 20 seconds in realtime
		private const double OvernightLongRunDuration = 10 * 60 * TimeScale; // 10 minutes in realtime

		#endregion

		#region Timing - Unity Invoke

		// UnityInvoke_Zero
		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_Zero() { yield return TestInvoke_Zero(DoUnityInvoke, CheesyRepeats); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_Zero_Detailed() { yield return TestInvoke_Zero(DoUnityInvoke, DetailedRepeats); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_Zero_Overnight() { yield return TestInvoke_Zero(DoUnityInvoke, OvernightRepeats); }

		// UnityInvoke_Various_StartsAtRandomTime
		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_Various_StartsAtRandomTime() { yield return TestInvoke_Various(DoUnityInvoke, true, CheesyRepeats); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_Various_StartsAtRandomTime_Detailed() { yield return TestInvoke_Various(DoUnityInvoke, true, DetailedRepeats); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_Various_StartsAtRandomTime_Overnight() { yield return TestInvoke_Various(DoUnityInvoke, true, OvernightRepeats); }

		// UnityInvoke_Various_StartsAtFixedUpdate
		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_Various_StartsAtFixedUpdate() { yield return TestInvoke_Various(DoUnityInvoke, false, CheesyRepeats); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_Various_StartsAtFixedUpdate_Detailed() { yield return TestInvoke_Various(DoUnityInvoke, false, DetailedRepeats); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_Various_StartsAtFixedUpdate_Overnight() { yield return TestInvoke_Various(DoUnityInvoke, false, OvernightRepeats); }

		// UnityInvoke_LongRun_StartsAtRandomTime
		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_LongRun_StartsAtRandomTime() { yield return TestInvoke(DoUnityInvoke, CheesyLongRunDuration, true, LongRunRepeats); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_LongRun_StartsAtRandomTime_Detailed() { yield return TestInvoke(DoUnityInvoke, DetailedLongRunDuration, true, LongRunRepeats); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_LongRun_StartsAtRandomTime_Overnight() { yield return TestInvoke(DoUnityInvoke, OvernightLongRunDuration, true, LongRunRepeats); }

		// UnityInvoke_LongRun_StartsAtFixedUpdate
		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_LongRun_StartsAtFixedUpdate() { yield return TestInvoke(DoUnityInvoke, CheesyLongRunDuration, false, LongRunRepeats); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_LongRun_StartsAtFixedUpdate_Detailed() { yield return TestInvoke(DoUnityInvoke, DetailedLongRunDuration, false, LongRunRepeats); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_LongRun_StartsAtFixedUpdate_Overnight() { yield return TestInvoke(DoUnityInvoke, OvernightLongRunDuration, false, LongRunRepeats); }

		#endregion

		#region Timing - Extenity FastInvoke

		// FastInvoke_Zero
		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_Zero() { yield return TestInvoke_Zero(DoFastInvoke, CheesyRepeats); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_Zero_Detailed() { yield return TestInvoke_Zero(DoFastInvoke, DetailedRepeats); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_Zero_Overnight() { yield return TestInvoke_Zero(DoFastInvoke, OvernightRepeats); }

		// FastInvoke_Various_StartsAtRandomTime
		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_Various_StartsAtRandomTime() { yield return TestInvoke_Various(DoFastInvoke, true, CheesyRepeats); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_Various_StartsAtRandomTime_Detailed() { yield return TestInvoke_Various(DoFastInvoke, true, DetailedRepeats); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_Various_StartsAtRandomTime_Overnight() { yield return TestInvoke_Various(DoFastInvoke, true, OvernightRepeats); }

		// FastInvoke_Various_StartsAtFixedUpdate
		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_Various_StartsAtFixedUpdate() { yield return TestInvoke_Various(DoFastInvoke, false, CheesyRepeats); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_Various_StartsAtFixedUpdate_Detailed() { yield return TestInvoke_Various(DoFastInvoke, false, DetailedRepeats); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_Various_StartsAtFixedUpdate_Overnight() { yield return TestInvoke_Various(DoFastInvoke, false, OvernightRepeats); }

		// FastInvoke_LongRun_StartsAtRandomTime
		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_LongRun_StartsAtRandomTime() { yield return TestInvoke(DoFastInvoke, CheesyLongRunDuration, true, LongRunRepeats); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_LongRun_StartsAtRandomTime_Detailed() { yield return TestInvoke(DoFastInvoke, DetailedLongRunDuration, true, LongRunRepeats); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_LongRun_StartsAtRandomTime_Overnight() { yield return TestInvoke(DoFastInvoke, OvernightLongRunDuration, true, LongRunRepeats); }

		// FastInvoke_LongRun_StartsAtFixedUpdate
		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_LongRun_StartsAtFixedUpdate() { yield return TestInvoke(DoFastInvoke, CheesyLongRunDuration, false, LongRunRepeats); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_LongRun_StartsAtFixedUpdate_Detailed() { yield return TestInvoke(DoFastInvoke, DetailedLongRunDuration, false, LongRunRepeats); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_LongRun_StartsAtFixedUpdate_Overnight() { yield return TestInvoke(DoFastInvoke, OvernightLongRunDuration, false, LongRunRepeats); }

		#endregion

		#region Call Order

		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_CalledBeforeAllFixedUpdates()
		{
			yield return InitializeTest(true);
			var WaitForFixedUpdate = new WaitForFixedUpdate();
			var delay = 0.0;
			//FastInvokeHandler.VerboseLoggingInEachFixedUpdate = true;
			//Subject.EnableLogging();

			// No invoke or fixed update processed yet. They are all zeros.
			Assert.AreEqual(0, Subject.CallbackCallCount);
			Assert.AreEqual(0, Subject.FixedUpdateCallCount);

			Subject.FastInvoke(Subject.Callback, delay);
			// No invoke or fixed update after just calling the FastInvoke. They are still all zeros.
			Assert.AreEqual(0, Subject.CallbackCallCount);
			Assert.AreEqual(0, Subject.FixedUpdateCallCount);

			Subject.ExpectedInvokeCallbackCallCountInFixedUpdate = 1; // Invoke callback should be called before FixedUpdate
			Subject.ExpectedFixedUpdateCallCountInInvokeCallback = 0; // FixedUpdate should be called after Invoke callback
			yield return WaitForFixedUpdate;

			// WaitForFixedUpdate yield us to the end of current fixed update loop. Invoke callback will be called
			// at the start of the loop. Then FixedUpdate will be called once.
			Assert.AreEqual(1, Subject.CallbackCallCount);
			Assert.AreEqual(1, Subject.FixedUpdateCallCount);

			Subject.FastInvoke(Subject.Callback, delay); // Call once more
			Subject.ExpectedInvokeCallbackCallCountInFixedUpdate = 2; // Invoke callback should be called before FixedUpdate
			Subject.ExpectedFixedUpdateCallCountInInvokeCallback = 1; // FixedUpdate should be called after Invoke callback
			yield return WaitForFixedUpdate;

			// We have completed a full cycle of fixed update loop. Both invoke callback and FixedUpdate will be
			// called once more.
			Assert.AreEqual(2, Subject.CallbackCallCount);
			Assert.AreEqual(2, Subject.FixedUpdateCallCount);

			Subject.FastInvoke(Subject.Callback, delay); // Call once more
			Subject.ExpectedInvokeCallbackCallCountInFixedUpdate = 3; // Invoke callback should be called before FixedUpdate
			Subject.ExpectedFixedUpdateCallCountInInvokeCallback = 2; // FixedUpdate should be called after Invoke callback
			yield return WaitForFixedUpdate;

			// And it goes on...
			Assert.AreEqual(3, Subject.CallbackCallCount);
			Assert.AreEqual(3, Subject.FixedUpdateCallCount);

			//Subject.FastInvoke(Subject.Callback, delay); Do not call anymore
			yield return WaitForFixedUpdate;

			// Invoke callback should stop since we are not calling the invoke anymore.
			Assert.AreEqual(3, Subject.CallbackCallCount);
			Assert.AreEqual(4, Subject.FixedUpdateCallCount);

			yield return WaitForFixedUpdate;

			// And once more...
			Assert.AreEqual(3, Subject.CallbackCallCount);
			Assert.AreEqual(5, Subject.FixedUpdateCallCount);
		}

		#endregion

		#region Cancellation

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
			Assert.AreEqual(0.1, Subject.RemainingTimeUntilNextFastInvoke(), FloatingTolerance);
			Subject.CancelFastInvoke(Subject.Callback);
			DoFastInvokingChecks(false);
		}

		#endregion

		#region Remaining Time

		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_RemainingTime()
		{
			yield return InitializeTest(true);
			var WaitForFixedUpdate = new WaitForFixedUpdate();
			var fixedDeltaTime = Time.fixedDeltaTime;
			var startTime = Time.time;

			// Initially there should be no fast invoke and no remaining time.
			Assert.IsTrue(double.IsNaN(Subject.RemainingTimeUntilNextFastInvoke()));
			Assert.AreEqual(0, Subject.CallbackCallCount);

			// Try different orders
			Subject.FastInvoke(Subject.Callback, 5);
			Assert.AreEqual(5, Subject.RemainingTimeUntilNextFastInvoke(), FloatingTolerance);
			Subject.FastInvoke(Subject.Callback, 4);
			Assert.AreEqual(4, Subject.RemainingTimeUntilNextFastInvoke(), FloatingTolerance);
			Subject.FastInvoke(Subject.Callback, 3);
			Assert.AreEqual(3, Subject.RemainingTimeUntilNextFastInvoke(), FloatingTolerance);
			Subject.FastInvoke(Subject.Callback, 2);
			Assert.AreEqual(2, Subject.RemainingTimeUntilNextFastInvoke(), FloatingTolerance);
			Subject.CancelAllFastInvokes();
			Assert.IsTrue(double.IsNaN(Subject.RemainingTimeUntilNextFastInvoke()));

			// Try different orders
			Subject.FastInvoke(Subject.Callback, 3);
			Assert.AreEqual(3, Subject.RemainingTimeUntilNextFastInvoke(), FloatingTolerance);
			Subject.FastInvoke(Subject.Callback, 4);
			Assert.AreEqual(3, Subject.RemainingTimeUntilNextFastInvoke(), FloatingTolerance);
			Subject.FastInvoke(Subject.Callback, 2);
			Assert.AreEqual(2, Subject.RemainingTimeUntilNextFastInvoke(), FloatingTolerance);
			Subject.FastInvoke(Subject.Callback, 5);
			Assert.AreEqual(2, Subject.RemainingTimeUntilNextFastInvoke(), FloatingTolerance);

			Assert.AreEqual(0, Subject.CallbackCallCount);

			// Wait for just one fixed update and see if remaining time acts accordingly.
			yield return WaitForFixedUpdate;
			Assert.AreEqual(1, Subject.CallbackCallCount);
			Assert.AreEqual(2.0 - fixedDeltaTime, Subject.RemainingTimeUntilNextFastInvoke(), FloatingTolerance);
			yield return WaitForFixedUpdate;
			Assert.AreEqual(2, Subject.CallbackCallCount);
			Assert.AreEqual(2.0 - 2 * fixedDeltaTime, Subject.RemainingTimeUntilNextFastInvoke(), FloatingTolerance);


			do yield return WaitForFixedUpdate; while (Time.time - startTime < 0.5 + FloatingTolerance);
			Assert.AreEqual(1.5, Subject.RemainingTimeUntilNextFastInvoke(), FloatingTolerance);
			do yield return WaitForFixedUpdate; while (Time.time - startTime < 0.9 + FloatingTolerance);
			Assert.AreEqual(1.1, Subject.RemainingTimeUntilNextFastInvoke(), FloatingTolerance);
			do yield return WaitForFixedUpdate; while (Time.time - startTime < 1.9 + FloatingTolerance);
			DoFastInvokingChecks(true, 4); // Just before invoking the first one, that is set to 2 seconds
			Assert.AreEqual(0.1, Subject.RemainingTimeUntilNextFastInvoke(), FloatingTolerance);
			do yield return WaitForFixedUpdate; while (Time.time - startTime < 2.0 + FloatingTolerance);
			DoFastInvokingChecks(true, 3);
			Assert.AreEqual(1.0, Subject.RemainingTimeUntilNextFastInvoke(), FloatingTolerance);
			do yield return WaitForFixedUpdate; while (Time.time - startTime < 2.9 + FloatingTolerance);
			DoFastInvokingChecks(true, 3);
			Assert.AreEqual(0.1, Subject.RemainingTimeUntilNextFastInvoke(), FloatingTolerance);
			do yield return WaitForFixedUpdate; while (Time.time - startTime < 3.0 + FloatingTolerance);
			DoFastInvokingChecks(true, 2);
			Assert.AreEqual(1.0, Subject.RemainingTimeUntilNextFastInvoke(), FloatingTolerance);
			do yield return WaitForFixedUpdate; while (Time.time - startTime < 4.0 + FloatingTolerance);
			DoFastInvokingChecks(true, 1);
			Assert.AreEqual(1.0, Subject.RemainingTimeUntilNextFastInvoke(), FloatingTolerance);
			do yield return WaitForFixedUpdate; while (Time.time - startTime < 4.4 + FloatingTolerance);
			DoFastInvokingChecks(true, 1);
			Assert.AreEqual(0.6, Subject.RemainingTimeUntilNextFastInvoke(), FloatingTolerance);
			do yield return WaitForFixedUpdate; while (Time.time - startTime < 5.0 + FloatingTolerance);
			DoFastInvokingChecks(false, 0);
			Assert.IsTrue(double.IsNaN(Subject.RemainingTimeUntilNextFastInvoke()));
		}

		#endregion

		#region AllowsRegisteringMethodsOutsideOfBehaviour

		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_AllowsRegisteringMethodsOutsideOfBehaviour()
		{
			yield return TestInvoke(DoOutsiderFastInvoke, 0.0, true, 1);
			yield return TestInvoke(DoOutsiderFastInvoke, 1.0, true, 1);

			yield return TestInvoke(DoOutsiderFastInvoke, 0.0, false, 1);
			yield return TestInvoke(DoOutsiderFastInvoke, 1.0, false, 1);
		}

		#endregion
	}

}
