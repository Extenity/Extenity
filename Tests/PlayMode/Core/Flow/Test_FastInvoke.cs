using System;
using System.Collections;
using Extenity.FlowToolbox;
using Extenity.ParallelToolbox;
using Extenity.Testing;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ExtenityTests.FlowToolbox
{

	public class Test_FastInvoke : TestBase_FastInvoke
	{
		#region Simple

		[UnityTest, Category(TestCategories.Cheesy), Timeout(TimeRequired_Simple * 3), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator Simple(bool startAtRandomTime) { yield return TestInvoke_Simple(DoFastInvoke, 3, startAtRandomTime, 1); }

		#endregion

		#region Timing

		// Zero
		[UnityTest, Category(TestCategories.Cheesy), Timeout(TimeRequired_Zero * CheesyRepeats), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator Timing_1_Zero(bool startAtRandomTime) { yield return TestInvoke_Zero(DoFastInvoke, startAtRandomTime, CheesyRepeats); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(TimeRequired_Zero * DetailedRepeats), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator Timing_1_Zero_Detailed(bool startAtRandomTime) { yield return TestInvoke_Zero(DoFastInvoke, startAtRandomTime, DetailedRepeats); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(TimeRequired_Zero * OvernightRepeats), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator Timing_1_Zero_Overnight(bool startAtRandomTime) { yield return TestInvoke_Zero(DoFastInvoke, startAtRandomTime, OvernightRepeats); }

		// Various
		[UnityTest, Category(TestCategories.Cheesy), Timeout(TimeRequired_Various * CheesyRepeats), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator Timing_2_Various(bool startAtRandomTime) { yield return TestInvoke_Various(DoFastInvoke, startAtRandomTime, CheesyRepeats); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(TimeRequired_Various * DetailedRepeats), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator Timing_2_Various_Detailed(bool startAtRandomTime) { yield return TestInvoke_Various(DoFastInvoke, startAtRandomTime, DetailedRepeats); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(TimeRequired_Various * OvernightRepeats), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator Timing_2_Various_Overnight(bool startAtRandomTime) { yield return TestInvoke_Various(DoFastInvoke, startAtRandomTime, OvernightRepeats); }

		// LongRun
		[UnityTest, Category(TestCategories.Cheesy), Timeout(TimeRequired_Simple * CheesyLongRunDuration * LongRunRepeats), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator Timing_3_LongRun(bool startAtRandomTime) { yield return TestInvoke_Simple(DoFastInvoke, CheesyLongRunDuration, startAtRandomTime, LongRunRepeats); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(TimeRequired_Simple * DetailedLongRunDuration * LongRunRepeats), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator Timing_3_LongRun_Detailed(bool startAtRandomTime) { yield return TestInvoke_Simple(DoFastInvoke, DetailedLongRunDuration, startAtRandomTime, LongRunRepeats); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(TimeRequired_Simple * OvernightLongRunDuration * LongRunRepeats), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator Timing_3_LongRun_Overnight(bool startAtRandomTime) { yield return TestInvoke_Simple(DoFastInvoke, OvernightLongRunDuration, startAtRandomTime, LongRunRepeats); }

		#endregion

		#region Call Order

		[UnityTest, Category(TestCategories.Cheesy), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator CallOrder_1_CallbackNotCalledUntilNextFixedUpdate(bool startAtRandomTime)
		{
			yield return InitializeTest(startAtRandomTime);

			// No invoke or fixed update processed yet. They are all zeros.
			Assert.AreEqual(0, Subject.CallbackCallCount);
			Assert.AreEqual(0, Subject.FixedUpdateCallCount);
			Assert.IsFalse(Invoker.IsFastInvokingAny());
			Assert.AreEqual(0, Invoker.TotalActiveFastInvokeCount());

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
			Assert.AreEqual(callCount, Invoker.TotalActiveFastInvokeCount());
		}

		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator CallOrder_2_1_CalledAtFirstFixedUpdateAsLongAsTheDelayIsBelowDeltaTime()
		{
			yield return CallOrder_2_CalledAtFirstFixedUpdateAsLongAsTheDelayIsBelowDeltaTime(0.0);
		}

		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator CallOrder_2_2_CalledAtFirstFixedUpdateAsLongAsTheDelayIsBelowDeltaTime()
		{
			yield return CallOrder_2_CalledAtFirstFixedUpdateAsLongAsTheDelayIsBelowDeltaTime(0.001);
		}

		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator CallOrder_2_3_CalledAtFirstFixedUpdateAsLongAsTheDelayIsBelowDeltaTime()
		{
			yield return CallOrder_2_CalledAtFirstFixedUpdateAsLongAsTheDelayIsBelowDeltaTime(Time.fixedDeltaTime);
		}

		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator CallOrder_2_4_CalledAtFirstFixedUpdateAsLongAsTheDelayIsBelowDeltaTime()
		{
			yield return CallOrder_2_CalledAtFirstFixedUpdateAsLongAsTheDelayIsBelowDeltaTime(Time.fixedDeltaTime * 0.5);
		}

		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator CallOrder_2_5_CalledAtFirstFixedUpdateAsLongAsTheDelayIsBelowDeltaTime()
		{
			yield return CallOrder_2_CalledAtFirstFixedUpdateAsLongAsTheDelayIsBelowDeltaTime(Time.fixedDeltaTime * 0.99);
		}

		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator CallOrder_2_6_CalledAtFirstFixedUpdateAsLongAsTheDelayIsBelowDeltaTime()
		{
			yield return CallOrder_2_CalledAtFirstFixedUpdateAsLongAsTheDelayIsBelowDeltaTime(-1.0);
		}

		private IEnumerator CallOrder_2_CalledAtFirstFixedUpdateAsLongAsTheDelayIsBelowDeltaTime(double delay)
		{
			for (int i = 0; i < 30; i++)
			{
				yield return InitializeTest(false);

				Subject.FastInvoke(Subject.Callback, delay);

				// Invoke should be set to be called.
				Assert.AreEqual(0, Subject.CallbackCallCount);
				Assert.AreEqual(0, Subject.FixedUpdateCallCount);
				Assert.IsTrue(Invoker.IsFastInvokingAny());
				Assert.AreEqual(1, Invoker.TotalActiveFastInvokeCount());

				// Wait for the next fixed update...
				yield return Yields.WaitForFixedUpdate;

				// ... and the callback should be called by now. Because its delays is below or equal to fixedDeltaTime.
				Assert.AreEqual(1, Subject.CallbackCallCount);
				Assert.AreEqual(1, Subject.FixedUpdateCallCount);
				Assert.IsFalse(Invoker.IsFastInvokingAny());
				Assert.AreEqual(0, Invoker.TotalActiveFastInvokeCount());
			}
		}

		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator CallOrder_3_1_AnyDelayAboveTheDeltaTimeShouldBeCalledInNextFixedUpdate()
		{
			yield return CallOrder_3_AnyDelayAboveTheDeltaTimeShouldBeCalledInNextFixedUpdate(Time.fixedDeltaTime * 1.01);
		}

		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator CallOrder_3_2_AnyDelayAboveTheDeltaTimeShouldBeCalledInNextFixedUpdate()
		{
			yield return CallOrder_3_AnyDelayAboveTheDeltaTimeShouldBeCalledInNextFixedUpdate(Time.fixedDeltaTime * 1.5);
		}

		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator CallOrder_3_3_AnyDelayAboveTheDeltaTimeShouldBeCalledInNextFixedUpdate()
		{
			yield return CallOrder_3_AnyDelayAboveTheDeltaTimeShouldBeCalledInNextFixedUpdate(Time.fixedDeltaTime * 1.9999);
		}

		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator CallOrder_3_4_AnyDelayAboveTheDeltaTimeShouldBeCalledInNextFixedUpdate()
		{
			yield return CallOrder_3_AnyDelayAboveTheDeltaTimeShouldBeCalledInNextFixedUpdate(Time.fixedDeltaTime * 2);
		}

		private IEnumerator CallOrder_3_AnyDelayAboveTheDeltaTimeShouldBeCalledInNextFixedUpdate(double delay)
		{
			for (int i = 0; i < 30; i++)
			{
				yield return InitializeTest(false);

				// Calling invoke with various delays
				Subject.FastInvoke(Subject.Callback, delay);

				// Invoke should be set to be called.
				Assert.AreEqual(0, Subject.CallbackCallCount);
				Assert.AreEqual(0, Subject.FixedUpdateCallCount);
				Assert.IsTrue(Invoker.IsFastInvokingAny());
				Assert.AreEqual(1, Invoker.TotalActiveFastInvokeCount());

				// Wait for the first fixed update...
				yield return Yields.WaitForFixedUpdate;

				// ... which won't cover the invoke.
				Assert.AreEqual(0, Subject.CallbackCallCount);
				Assert.AreEqual(1, Subject.FixedUpdateCallCount);
				Assert.IsTrue(Invoker.IsFastInvokingAny());
				Assert.AreEqual(1, Invoker.TotalActiveFastInvokeCount());

				// Wait for the second fixed update...
				yield return Yields.WaitForFixedUpdate;

				// ... and the callback should be called by now.
				Assert.AreEqual(1, Subject.CallbackCallCount);
				Assert.AreEqual(2, Subject.FixedUpdateCallCount);
				Assert.IsFalse(Invoker.IsFastInvokingAny());
				Assert.AreEqual(0, Invoker.TotalActiveFastInvokeCount());
			}
		}

		[UnityTest, Category(TestCategories.Cheesy), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator CallOrder_4_CalledBeforeAllFixedUpdates(bool startAtRandomTime)
		{
			yield return InitializeTest(startAtRandomTime);
			var delay = 0.0;
			//FastInvokeHandler.VerboseLoggingInEachFixedUpdate = true;
			//Subject.EnableLogging();

			Subject.FastInvoke(Subject.Callback, delay);
			Subject.ExpectedInvokeCallbackCallCountInFixedUpdate = 1; // Invoke callback should be called before FixedUpdate
			Subject.ExpectedFixedUpdateCallCountInInvokeCallback = 0; // FixedUpdate should be called after Invoke callback
			yield return Yields.WaitForFixedUpdate;

			// WaitForFixedUpdate yield us to the end of current fixed update loop. Invoke callback will be called
			// at the start of the loop. Then the FixedUpdate will be called.
			Assert.AreEqual(1, Subject.CallbackCallCount);
			Assert.AreEqual(1, Subject.FixedUpdateCallCount);

			Subject.FastInvoke(Subject.Callback, delay); // Call once more
			Subject.ExpectedInvokeCallbackCallCountInFixedUpdate = 2; // Invoke callback should be called before FixedUpdate
			Subject.ExpectedFixedUpdateCallCountInInvokeCallback = 1; // FixedUpdate should be called after Invoke callback
			yield return Yields.WaitForFixedUpdate;

			// We have completed a full cycle of fixed update loop. Both invoke callback and FixedUpdate will be
			// called once more.
			Assert.AreEqual(2, Subject.CallbackCallCount);
			Assert.AreEqual(2, Subject.FixedUpdateCallCount);

			Subject.FastInvoke(Subject.Callback, delay); // Call once more
			Subject.ExpectedInvokeCallbackCallCountInFixedUpdate = 3; // Invoke callback should be called before FixedUpdate
			Subject.ExpectedFixedUpdateCallCountInInvokeCallback = 2; // FixedUpdate should be called after Invoke callback
			yield return Yields.WaitForFixedUpdate;

			// And it goes on...
			Assert.AreEqual(3, Subject.CallbackCallCount);
			Assert.AreEqual(3, Subject.FixedUpdateCallCount);

			//Subject.FastInvoke(Subject.Callback, delay); Do not call Invoke anymore
			yield return Yields.WaitForFixedUpdate;

			// Invoke callback should stop since we are not calling the invoke anymore.
			Assert.AreEqual(3, Subject.CallbackCallCount);
			Assert.AreEqual(4, Subject.FixedUpdateCallCount);

			yield return Yields.WaitForFixedUpdate;

			// And once more...
			Assert.AreEqual(3, Subject.CallbackCallCount);
			Assert.AreEqual(5, Subject.FixedUpdateCallCount);
		}

		#endregion

		#region Halt - Cancellation

		[UnityTest, Category(TestCategories.Cheesy), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator Halt_1_Cancel(bool startAtRandomTime)
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
				yield return Yields.WaitForFixedUpdate;
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
				yield return Yields.WaitForFixedUpdate;
			DoFastInvokingChecks(true);
			Assert.AreEqual(0.1, Subject.RemainingTimeUntilNextFastInvoke(), FloatingTolerance);
			Subject.CancelFastInvoke(Subject.Callback);
			DoFastInvokingChecks(false);
		}

		#endregion

		#region Halt - Stop When Destroyed

		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator Halt_2_StopsWhenObjectDestroyed()
		{
			yield return InitializeTest(false);
			var fixedDeltaTime = Time.fixedDeltaTime;
			var cachedSubject = Subject;

			Subject.FastInvoke(Subject.Callback, 1.0);

			// Wait half way. Then destroy.
			do yield return Yields.WaitForFixedUpdate; while (Subject.RemainingTimeUntilNextFastInvoke() > 0.5);
			var lastCallbackCallCount = Subject.CallbackCallCount;
			GameObject.Destroy(Subject);

			// Check if the callback is still called.
			var remainingTime = 1.0;
			while ((remainingTime -= fixedDeltaTime) > 0.0)
			{
				yield return Yields.WaitForFixedUpdate;
				Assert.AreEqual(lastCallbackCallCount, cachedSubject.CallbackCallCount);
			}
		}

		#endregion

		#region Halt - Throws Over Null

		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator Halt_3_ThrowsWhenCalledOverADestroyedObject()
		{
			yield return InitializeTest(false);
			var cachedSubject = Subject;
			GameObject.DestroyImmediate(Subject);
			Assert.Throws<Exception>(() => cachedSubject.FastInvoke(cachedSubject.Callback, 1.0));
		}

		[UnityTest, Category(TestCategories.Cheesy)]
		public IEnumerator Halt_4_ThrowsWhenCalledOverANullObject()
		{
			yield return InitializeTest(false);
			Test_FastInvokeSubject nullRef = null;
			Assert.Throws<Exception>(() => nullRef.FastInvoke(FastInvokeOutsiderCallback, 1.0));
		}

		#endregion

		#region Query - Remaining Time

		[UnityTest, Category(TestCategories.Cheesy), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator Query_1_RemainingTime(bool startAtRandomTime)
		{
			yield return InitializeTest(startAtRandomTime);
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
			yield return Yields.WaitForFixedUpdate;
			Assert.AreEqual(0, Subject.CallbackCallCount);
			Assert.AreEqual(2.0 - fixedDeltaTime, Subject.RemainingTimeUntilNextFastInvoke(), FloatingTolerance);
			yield return Yields.WaitForFixedUpdate;
			Assert.AreEqual(0, Subject.CallbackCallCount);
			Assert.AreEqual(2.0 - 2 * fixedDeltaTime, Subject.RemainingTimeUntilNextFastInvoke(), FloatingTolerance);


			do yield return Yields.WaitForFixedUpdate; while (Time.time - startTime < 0.5);
			Assert.AreEqual(1.5, Subject.RemainingTimeUntilNextFastInvoke(), FloatingTolerance);
			do yield return Yields.WaitForFixedUpdate; while (Time.time - startTime < 0.9);
			Assert.AreEqual(1.1, Subject.RemainingTimeUntilNextFastInvoke(), FloatingTolerance);
			do yield return Yields.WaitForFixedUpdate; while (Time.time - startTime < 1.9);
			DoFastInvokingChecks(true, 4); // Just before invoking the first one, that is set to 2 seconds
			Assert.AreEqual(0.1, Subject.RemainingTimeUntilNextFastInvoke(), FloatingTolerance);
			do yield return Yields.WaitForFixedUpdate; while (Time.time - startTime < 2.0);
			DoFastInvokingChecks(true, 3);
			Assert.AreEqual(1.0, Subject.RemainingTimeUntilNextFastInvoke(), FloatingTolerance);
			do yield return Yields.WaitForFixedUpdate; while (Time.time - startTime < 2.9);
			DoFastInvokingChecks(true, 3);
			Assert.AreEqual(0.1, Subject.RemainingTimeUntilNextFastInvoke(), FloatingTolerance);
			do yield return Yields.WaitForFixedUpdate; while (Time.time - startTime < 3.0);
			DoFastInvokingChecks(true, 2);
			Assert.AreEqual(1.0, Subject.RemainingTimeUntilNextFastInvoke(), FloatingTolerance);
			do yield return Yields.WaitForFixedUpdate; while (Time.time - startTime < 4.0);
			DoFastInvokingChecks(true, 1);
			Assert.AreEqual(1.0, Subject.RemainingTimeUntilNextFastInvoke(), FloatingTolerance);
			do yield return Yields.WaitForFixedUpdate; while (Time.time - startTime < 4.4);
			DoFastInvokingChecks(true, 1);
			Assert.AreEqual(0.6, Subject.RemainingTimeUntilNextFastInvoke(), FloatingTolerance);
			do yield return Yields.WaitForFixedUpdate; while (Time.time - startTime < 5.0);
			DoFastInvokingChecks(false, 0);
			Assert.IsTrue(double.IsNaN(Subject.RemainingTimeUntilNextFastInvoke()));
		}

		#endregion

		#region AllowsRegisteringMethodsOutsideOfBehaviour

		[UnityTest, Category(TestCategories.Cheesy), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator AllowsRegisteringMethodsOutsideOfBehaviour(bool startAtRandomTime)
		{
			yield return TestInvoke_Simple(DoOutsiderFastInvoke, 0.0, startAtRandomTime, 1);
			yield return TestInvoke_Simple(DoOutsiderFastInvoke, 1.0, startAtRandomTime, 1);
		}

		#endregion
	}

}
