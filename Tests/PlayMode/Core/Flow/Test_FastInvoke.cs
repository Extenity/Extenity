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
		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator UnityInvoke_Zero(bool startAtRandomTime) { yield return TestInvoke_Zero(DoUnityInvoke, startAtRandomTime, CheesyRepeats); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(int.MaxValue), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator UnityInvoke_Zero_Detailed(bool startAtRandomTime) { yield return TestInvoke_Zero(DoUnityInvoke, startAtRandomTime, DetailedRepeats); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(int.MaxValue), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator UnityInvoke_Zero_Overnight(bool startAtRandomTime) { yield return TestInvoke_Zero(DoUnityInvoke, startAtRandomTime, OvernightRepeats); }

		// UnityInvoke_Various
		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator UnityInvoke_Various(bool startAtRandomTime) { yield return TestInvoke_Various(DoUnityInvoke, startAtRandomTime, CheesyRepeats); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(int.MaxValue), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator UnityInvoke_Various_Detailed(bool startAtRandomTime) { yield return TestInvoke_Various(DoUnityInvoke, startAtRandomTime, DetailedRepeats); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(int.MaxValue), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator UnityInvoke_Various_Overnight(bool startAtRandomTime) { yield return TestInvoke_Various(DoUnityInvoke, startAtRandomTime, OvernightRepeats); }

		// UnityInvoke_LongRun
		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator UnityInvoke_LongRun(bool startAtRandomTime) { yield return TestInvoke(DoUnityInvoke, CheesyLongRunDuration, startAtRandomTime, LongRunRepeats); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(int.MaxValue), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator UnityInvoke_LongRun_Detailed(bool startAtRandomTime) { yield return TestInvoke(DoUnityInvoke, DetailedLongRunDuration, startAtRandomTime, LongRunRepeats); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(int.MaxValue), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator UnityInvoke_LongRun_Overnight(bool startAtRandomTime) { yield return TestInvoke(DoUnityInvoke, OvernightLongRunDuration, startAtRandomTime, LongRunRepeats); }

		#endregion

		#region Timing - Extenity FastInvoke

		// FastInvoke_Zero
		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator FastInvoke_Zero(bool startAtRandomTime) { yield return TestInvoke_Zero(DoFastInvoke, startAtRandomTime, CheesyRepeats); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(int.MaxValue), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator FastInvoke_Zero_Detailed(bool startAtRandomTime) { yield return TestInvoke_Zero(DoFastInvoke, startAtRandomTime, DetailedRepeats); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(int.MaxValue), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator FastInvoke_Zero_Overnight(bool startAtRandomTime) { yield return TestInvoke_Zero(DoFastInvoke, startAtRandomTime, OvernightRepeats); }

		// FastInvoke_Various
		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator FastInvoke_Various(bool startAtRandomTime) { yield return TestInvoke_Various(DoFastInvoke, startAtRandomTime, CheesyRepeats); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(int.MaxValue), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator FastInvoke_Various_Detailed(bool startAtRandomTime) { yield return TestInvoke_Various(DoFastInvoke, startAtRandomTime, DetailedRepeats); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(int.MaxValue), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator FastInvoke_Various_Overnight(bool startAtRandomTime) { yield return TestInvoke_Various(DoFastInvoke, startAtRandomTime, OvernightRepeats); }

		// FastInvoke_LongRun
		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator FastInvoke_LongRun(bool startAtRandomTime) { yield return TestInvoke(DoFastInvoke, CheesyLongRunDuration, startAtRandomTime, LongRunRepeats); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(int.MaxValue), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator FastInvoke_LongRun_Detailed(bool startAtRandomTime) { yield return TestInvoke(DoFastInvoke, DetailedLongRunDuration, startAtRandomTime, LongRunRepeats); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(int.MaxValue), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator FastInvoke_LongRun_Overnight(bool startAtRandomTime) { yield return TestInvoke(DoFastInvoke, OvernightLongRunDuration, startAtRandomTime, LongRunRepeats); }

		#endregion

		#region Call Order

		[UnityTest, Category(TestCategories.Cheesy), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator FastInvoke_1_CallbackNotCalledUntilNextFixedUpdate(bool startAtRandomTime)
		{
			yield return InitializeTest(startAtRandomTime);

			// No invoke or fixed update processed yet. They are all zeros.
			Assert.AreEqual(0, Subject.CallbackCallCount);
			Assert.AreEqual(0, Subject.FixedUpdateCallCount);
			Assert.IsFalse(Invoker.IsFastInvokingAny());
			Assert.AreEqual(0, Invoker.TotalFastInvokeCount());

			// Calling invoke with various delays
			var callCount = 0;
			Subject.FastInvoke(Subject.Callback, 0.0); callCount++;
			Subject.FastInvoke(Subject.Callback, 0.001); callCount++;
			Subject.FastInvoke(Subject.Callback, 0.1); callCount++;
			Subject.FastInvoke(Subject.Callback, 1.0); callCount++;
			Subject.FastInvoke(Subject.Callback, Time.fixedDeltaTime); callCount++;
			Subject.FastInvoke(Subject.Callback, Time.fixedDeltaTime * 0.5); callCount++;
			Subject.FastInvoke(Subject.Callback, Time.fixedDeltaTime * 1.5); callCount++;
			Subject.FastInvoke(Subject.Callback, 100.0); callCount++;
			Subject.FastInvoke(Subject.Callback, -1.0); callCount++;

			// No invoke or fixed update after just calling the FastInvoke. They are still all zeros.
			// The calls are just queued and waiting until next FixedUpdate.
			Assert.AreEqual(0, Subject.CallbackCallCount);
			Assert.AreEqual(0, Subject.FixedUpdateCallCount);
			Assert.IsTrue(Invoker.IsFastInvokingAny());
			Assert.AreEqual(callCount, Invoker.TotalFastInvokeCount());
		}

		[UnityTest, Category(TestCategories.Cheesy), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator FastInvoke_2_CalledAtFirstFixedUpdateAsLongAsTheDelayIsBelowDeltaTime(bool startAtRandomTime)
		{
			yield return InitializeTest(startAtRandomTime);

			// Calling invoke with various delays
			var callCount = 0;
			Subject.FastInvoke(Subject.Callback, 0.0); callCount++;
			Subject.FastInvoke(Subject.Callback, 0.001); callCount++;
			Subject.FastInvoke(Subject.Callback, Time.fixedDeltaTime); callCount++;
			Subject.FastInvoke(Subject.Callback, Time.fixedDeltaTime * 0.5); callCount++;
			Subject.FastInvoke(Subject.Callback, -1.0); callCount++;

			// Wait for the next fixed update...
			yield return new WaitForFixedUpdate();

			// ... and all callbacks should be called by now. Because their delays are all below or equal to fixedDeltaTime.
			Assert.AreEqual(callCount, Subject.CallbackCallCount);
			Assert.AreEqual(callCount, Subject.FixedUpdateCallCount);
			Assert.IsFalse(Invoker.IsFastInvokingAny());
			Assert.AreEqual(0, Invoker.TotalFastInvokeCount());
		}

		[UnityTest, Category(TestCategories.Cheesy), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator FastInvoke_3_AnyDelayAboveTheDeltaTimeShouldBeCalledInNextFixedUpdate(bool startAtRandomTime)
		{
			yield return InitializeTest(startAtRandomTime);

			// Calling invoke with various delays
			var callCount = 0;
			Subject.FastInvoke(Subject.Callback, Time.fixedDeltaTime + FastInvokeHandler.Tolerance * 2); callCount++;
			Subject.FastInvoke(Subject.Callback, Time.fixedDeltaTime * 1.5); callCount++;
			Subject.FastInvoke(Subject.Callback, Time.fixedDeltaTime * 1.9999); callCount++;
			Subject.FastInvoke(Subject.Callback, Time.fixedDeltaTime * 2); callCount++;

			// Wait for the first fixed update...
			yield return new WaitForFixedUpdate();

			// ... which won't cover any invokes.
			Assert.AreEqual(0, Subject.CallbackCallCount);
			Assert.AreEqual(0, Subject.FixedUpdateCallCount);
			Assert.IsTrue(Invoker.IsFastInvokingAny());
			Assert.AreEqual(callCount, Invoker.TotalFastInvokeCount());

			// Wait for the second fixed update...
			yield return new WaitForFixedUpdate();

			// ... and all callbacks should be called by now.
			Assert.AreEqual(callCount, Subject.CallbackCallCount);
			Assert.AreEqual(callCount, Subject.FixedUpdateCallCount);
			Assert.IsFalse(Invoker.IsFastInvokingAny());
			Assert.AreEqual(0, Invoker.TotalFastInvokeCount());
		}

		[UnityTest, Category(TestCategories.Cheesy), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator FastInvoke_4_CalledBeforeAllFixedUpdates(bool startAtRandomTime)
		{
			yield return InitializeTest(startAtRandomTime);
			var WaitForFixedUpdate = new WaitForFixedUpdate();
			var delay = 0.0;
			//FastInvokeHandler.VerboseLoggingInEachFixedUpdate = true;
			//Subject.EnableLogging();

			Subject.FastInvoke(Subject.Callback, delay);
			Subject.ExpectedInvokeCallbackCallCountInFixedUpdate = 1; // Invoke callback should be called before FixedUpdate
			Subject.ExpectedFixedUpdateCallCountInInvokeCallback = 0; // FixedUpdate should be called after Invoke callback
			yield return WaitForFixedUpdate;

			// WaitForFixedUpdate yield us to the end of current fixed update loop. Invoke callback will be called
			// at the start of the loop. Then the FixedUpdate will be called.
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

			//Subject.FastInvoke(Subject.Callback, delay); Do not call Invoke anymore
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

		[UnityTest, Category(TestCategories.Cheesy), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator FastInvoke_Cancel(bool startAtRandomTime)
		{
			yield return InitializeTest(startAtRandomTime);
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

		[UnityTest, Category(TestCategories.Cheesy), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator FastInvoke_RemainingTime(bool startAtRandomTime)
		{
			yield return InitializeTest(startAtRandomTime);
			var WaitForFixedUpdate = new WaitForFixedUpdate();
			var fixedDeltaTime = Time.fixedDeltaTime;
			var startTime = Time.time;

			// Initially there should be no fast invoke and no remaining time.
			Assert.IsTrue(double.IsNaN(Subject.RemainingTimeUntilNextFastInvoke()));

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

		[UnityTest, Category(TestCategories.Cheesy), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator FastInvoke_AllowsRegisteringMethodsOutsideOfBehaviour(bool startAtRandomTime)
		{
			yield return TestInvoke(DoOutsiderFastInvoke, 0.0, startAtRandomTime, 1);
			yield return TestInvoke(DoOutsiderFastInvoke, 1.0, startAtRandomTime, 1);
		}

		#endregion

		#region Test System Validation
		
		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator TestSystemValidation_StartAtRandomTimeWorks()
		{
			yield return InitializeTest(true);

			// No invoke or fixed update processed yet. They are all zeros.
			Assert.AreEqual(0, Subject.CallbackCallCount);
			Assert.AreEqual(0, Subject.FixedUpdateCallCount);
			Assert.IsFalse(Invoker.IsFastInvokingAny());
			Assert.AreEqual(0, Invoker.TotalFastInvokeCount());
		}

		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator TestSystemValidation_StartAtFixedUpdateWorks()
		{
			yield return InitializeTest(false);

			// No invoke or fixed update processed yet. They are all zeros.
			Assert.AreEqual(0, Subject.CallbackCallCount);
			Assert.AreEqual(0, Subject.FixedUpdateCallCount);
			Assert.IsFalse(Invoker.IsFastInvokingAny());
			Assert.AreEqual(0, Invoker.TotalFastInvokeCount());
		}

		#endregion
	}

}
