using System;
using System.Collections;
using Extenity.FlowToolbox;
using Extenity.UnityTestToolbox;
using ExtenityTests.Common;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ExtenityTests.FlowToolbox
{

	public class Test_FastInvokeSubject : MonoBehaviour
	{
		#region Initialization

		private void Awake()
		{
			ResetCallback();
		}

		#endregion

		#region Callback

		public int CallbackCallCount;

		public void ResetCallback()
		{
			CallbackCallCount = 0;
		}

		public void Callback()
		{
			CallbackCallCount++;
		}

		#endregion
	}

	public class Test_FastInvoke
	{
		private const int CheesyRepeats = 1;
		private const int DetailedRepeats = 20;
		private const int OvernightRepeats = 1000;
		private const int CheesyLongRunDuration = 100; // 1 second in realtime
		private const int DetailedLongRunDuration = 20 * 100; // 20 seconds in realtime
		private const int OvernightLongRunDuration = 10 * 60 * 100; // 10 minutes in realtime

		#region Test Definitions

		// ---------- UnityInvoke

		// UnityInvoke_Zero
		[UnityTest, Category(TestCategories.Cheesy), Repeat(CheesyRepeats), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_Zero() { yield return TestUnityInvoke_Zero(); }
		[UnityTest, Category(TestCategories.Detailed), Repeat(DetailedRepeats), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_Zero_Detailed() { yield return TestUnityInvoke_Zero(); }
		[UnityTest, Category(TestCategories.Overnight), Repeat(OvernightRepeats), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_Zero_Overnight() { yield return TestUnityInvoke_Zero(); }

		// UnityInvoke_Various_StartsAtRandomTime
		[UnityTest, Category(TestCategories.Cheesy), Repeat(CheesyRepeats), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_Various_StartsAtRandomTime() { yield return TestUnityInvoke_Various(true); }
		[UnityTest, Category(TestCategories.Detailed), Repeat(DetailedRepeats), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_Various_StartsAtRandomTime_Detailed() { yield return TestUnityInvoke_Various(true); }
		[UnityTest, Category(TestCategories.Overnight), Repeat(OvernightRepeats), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_Various_StartsAtRandomTime_Overnight() { yield return TestUnityInvoke_Various(true); }

		// UnityInvoke_Various_StartsAtFixedUpdate
		[UnityTest, Category(TestCategories.Cheesy), Repeat(CheesyRepeats), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_Various_StartsAtFixedUpdate() { yield return TestUnityInvoke_Various(false); }
		[UnityTest, Category(TestCategories.Detailed), Repeat(DetailedRepeats), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_Various_StartsAtFixedUpdate_Detailed() { yield return TestUnityInvoke_Various(false); }
		[UnityTest, Category(TestCategories.Overnight), Repeat(OvernightRepeats), Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_Various_StartsAtFixedUpdate_Overnight() { yield return TestUnityInvoke_Various(false); }

		// UnityInvoke_LongRun_StartsAtRandomTime
		[UnityTest, Category(TestCategories.Cheesy), /*Repeat(),*/ Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_LongRun_StartsAtRandomTime() { yield return TestUnityInvoke(CheesyLongRunDuration, true); }
		[UnityTest, Category(TestCategories.Detailed), /*Repeat(),*/ Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_LongRun_StartsAtRandomTime_Detailed() { yield return TestUnityInvoke(DetailedLongRunDuration, true); }
		[UnityTest, Category(TestCategories.Overnight), /*Repeat(),*/ Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_LongRun_StartsAtRandomTime_Overnight() { yield return TestUnityInvoke(OvernightLongRunDuration, true); }

		// UnityInvoke_LongRun_StartsAtFixedUpdate
		[UnityTest, Category(TestCategories.Cheesy), /*Repeat(),*/ Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_LongRun_StartsAtFixedUpdate() { yield return TestUnityInvoke(CheesyLongRunDuration, false); }
		[UnityTest, Category(TestCategories.Detailed), /*Repeat(),*/ Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_LongRun_StartsAtFixedUpdate_Detailed() { yield return TestUnityInvoke(DetailedLongRunDuration, false); }
		[UnityTest, Category(TestCategories.Overnight), /*Repeat(),*/ Timeout(int.MaxValue)]
		public IEnumerator UnityInvoke_LongRun_StartsAtFixedUpdate_Overnight() { yield return TestUnityInvoke(OvernightLongRunDuration, false); }

		// ---------- FastInvoke

		// FastInvoke_Zero
		[UnityTest, Category(TestCategories.Cheesy), Repeat(CheesyRepeats), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_Zero() { yield return TestFastInvoke_Zero(); }
		[UnityTest, Category(TestCategories.Detailed), Repeat(DetailedRepeats), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_Zero_Detailed() { yield return TestFastInvoke_Zero(); }
		[UnityTest, Category(TestCategories.Overnight), Repeat(OvernightRepeats), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_Zero_Overnight() { yield return TestFastInvoke_Zero(); }

		// FastInvoke_Various_StartsAtRandomTime
		[UnityTest, Category(TestCategories.Cheesy), Repeat(CheesyRepeats), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_Various_StartsAtRandomTime() { yield return TestFastInvoke_Various(true); }
		[UnityTest, Category(TestCategories.Detailed), Repeat(DetailedRepeats), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_Various_StartsAtRandomTime_Detailed() { yield return TestFastInvoke_Various(true); }
		[UnityTest, Category(TestCategories.Overnight), Repeat(OvernightRepeats), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_Various_StartsAtRandomTime_Overnight() { yield return TestFastInvoke_Various(true); }

		// FastInvoke_Various_StartsAtFixedUpdate
		[UnityTest, Category(TestCategories.Cheesy), Repeat(CheesyRepeats), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_Various_StartsAtFixedUpdate() { yield return TestFastInvoke_Various(false); }
		[UnityTest, Category(TestCategories.Detailed), Repeat(DetailedRepeats), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_Various_StartsAtFixedUpdate_Detailed() { yield return TestFastInvoke_Various(false); }
		[UnityTest, Category(TestCategories.Overnight), Repeat(OvernightRepeats), Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_Various_StartsAtFixedUpdate_Overnight() { yield return TestFastInvoke_Various(false); }

		// FastInvoke_LongRun_StartsAtRandomTime
		[UnityTest, Category(TestCategories.Cheesy), /*Repeat(),*/ Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_LongRun_StartsAtRandomTime() { yield return TestFastInvoke(CheesyLongRunDuration, true); }
		[UnityTest, Category(TestCategories.Detailed), /*Repeat(),*/ Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_LongRun_StartsAtRandomTime_Detailed() { yield return TestFastInvoke(DetailedLongRunDuration, true); }
		[UnityTest, Category(TestCategories.Overnight), /*Repeat(),*/ Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_LongRun_StartsAtRandomTime_Overnight() { yield return TestFastInvoke(OvernightLongRunDuration, true); }

		// FastInvoke_LongRun_StartsAtFixedUpdate
		[UnityTest, Category(TestCategories.Cheesy), /*Repeat(),*/ Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_LongRun_StartsAtFixedUpdate() { yield return TestFastInvoke(CheesyLongRunDuration, false); }
		[UnityTest, Category(TestCategories.Detailed), /*Repeat(),*/ Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_LongRun_StartsAtFixedUpdate_Detailed() { yield return TestFastInvoke(DetailedLongRunDuration, false); }
		[UnityTest, Category(TestCategories.Overnight), /*Repeat(),*/ Timeout(int.MaxValue)]
		public IEnumerator FastInvoke_LongRun_StartsAtFixedUpdate_Overnight() { yield return TestFastInvoke(OvernightLongRunDuration, false); }

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
			yield return TestOutsiderFastInvoke(0, true);
			yield return TestOutsiderFastInvoke(1, true);

			yield return TestOutsiderFastInvoke(0, false);
			yield return TestOutsiderFastInvoke(1, false);
		}

		#endregion

		#region Initialization / Deinitialization

		private IEnumerator InitializeTest(bool startAtRandomTime)
		{
			if (!InitializeBase())
				yield break;

			if (startAtRandomTime)
			{
				// This will make tests start at a random Time.time.
				yield return new WaitForEndOfFrame(); // Ignored by Code Correct
			}
			else
			{
				// This will make tests start right in FixedUpdates where Time.time is consistent.
				yield return new WaitForFixedUpdate(); // Ignored by Code Correct
			}
		}

		#endregion

		private IEnumerator TestUnityInvoke_Zero()
		{
			yield return TestUnityInvoke(0, true);
			yield return TestUnityInvoke(-1, true);
			yield return TestUnityInvoke(-6128, true);

			yield return TestUnityInvoke(0, false);
			yield return TestUnityInvoke(-1, false);
			yield return TestUnityInvoke(-6128, false);
		}

		private IEnumerator TestFastInvoke_Zero()
		{
			yield return TestFastInvoke(0, true);
			yield return TestFastInvoke(-1, true);
			yield return TestFastInvoke(-6128, true);

			yield return TestFastInvoke(0, false);
			yield return TestFastInvoke(-1, false);
			yield return TestFastInvoke(-6128, false);
		}

		private IEnumerator TestUnityInvoke_Various(bool startAtRandomTime)
		{
			yield return TestUnityInvoke(Time.fixedDeltaTime * 0.1, startAtRandomTime);
			yield return TestUnityInvoke(Time.fixedDeltaTime * 0.5, startAtRandomTime);
			yield return TestUnityInvoke(Time.fixedDeltaTime * 0.9, startAtRandomTime);
			yield return TestUnityInvoke(Time.fixedDeltaTime * 0.99, startAtRandomTime);
			yield return TestUnityInvoke(Time.fixedDeltaTime * 1.0, startAtRandomTime);
			yield return TestUnityInvoke(Time.fixedDeltaTime * 1.01, startAtRandomTime);
			yield return TestUnityInvoke(Time.fixedDeltaTime * 1.1, startAtRandomTime);
			yield return TestUnityInvoke(Time.fixedDeltaTime * 1.5, startAtRandomTime);
			yield return TestUnityInvoke(Time.fixedDeltaTime * 1.9, startAtRandomTime);
			yield return TestUnityInvoke(Time.fixedDeltaTime * 1.99, startAtRandomTime);
			yield return TestUnityInvoke(Time.fixedDeltaTime * 2.0, startAtRandomTime);
			yield return TestUnityInvoke(Time.fixedDeltaTime * 2.01, startAtRandomTime);
			yield return TestUnityInvoke(Time.fixedDeltaTime * 2.1, startAtRandomTime);

			yield return TestUnityInvoke(0.05, startAtRandomTime);
			yield return TestUnityInvoke(0.1, startAtRandomTime);
			yield return TestUnityInvoke(1, startAtRandomTime);
			yield return TestUnityInvoke(1.1329587, startAtRandomTime);
			yield return TestUnityInvoke(5.4515328, startAtRandomTime);
			yield return TestUnityInvoke(10, startAtRandomTime);

			for (int i = 0; i < 100; i++)
			{
				yield return TestUnityInvoke(UnityEngine.Random.Range(0.001f, 10.0f), startAtRandomTime);
			}
		}

		private IEnumerator TestFastInvoke_Various(bool startAtRandomTime)
		{
			yield return TestFastInvoke(Time.fixedDeltaTime * 0.1, startAtRandomTime);
			yield return TestFastInvoke(Time.fixedDeltaTime * 0.5, startAtRandomTime);
			yield return TestFastInvoke(Time.fixedDeltaTime * 0.9, startAtRandomTime);
			yield return TestFastInvoke(Time.fixedDeltaTime * 0.99, startAtRandomTime);
			yield return TestFastInvoke(Time.fixedDeltaTime * 1.0, startAtRandomTime);
			yield return TestFastInvoke(Time.fixedDeltaTime * 1.01, startAtRandomTime);
			yield return TestFastInvoke(Time.fixedDeltaTime * 1.1, startAtRandomTime);
			yield return TestFastInvoke(Time.fixedDeltaTime * 1.5, startAtRandomTime);
			yield return TestFastInvoke(Time.fixedDeltaTime * 1.9, startAtRandomTime);
			yield return TestFastInvoke(Time.fixedDeltaTime * 1.99, startAtRandomTime);
			yield return TestFastInvoke(Time.fixedDeltaTime * 2.0, startAtRandomTime);
			yield return TestFastInvoke(Time.fixedDeltaTime * 2.01, startAtRandomTime);
			yield return TestFastInvoke(Time.fixedDeltaTime * 2.1, startAtRandomTime);

			yield return TestFastInvoke(0.05, startAtRandomTime);
			yield return TestFastInvoke(0.1, startAtRandomTime);
			yield return TestFastInvoke(1, startAtRandomTime);
			yield return TestFastInvoke(1.1329587, startAtRandomTime);
			yield return TestFastInvoke(5.4515328, startAtRandomTime);
			yield return TestFastInvoke(10, startAtRandomTime);

			for (int i = 0; i < 100; i++)
			{
				yield return TestFastInvoke(UnityEngine.Random.Range(0.001f, 10.0f), startAtRandomTime);
			}
		}

		private IEnumerator TestUnityInvoke(double invokeTime, bool startAtRandomTime)
		{
			var fixedUpdateCountTolerance = 2; // +2 tolerance because it seems Unity is not good with numbers.

			yield return RunWholeTest("UnityInvoke", invokeTime, startAtRandomTime, fixedUpdateCountTolerance,
				() => Subject.CallbackCallCount,
				() => Subject.Invoke("Callback", (float)invokeTime),
				DoUnityInvokingChecks
			);
		}

		private IEnumerator TestFastInvoke(double invokeTime, bool startAtRandomTime)
		{
			var fixedUpdateCountTolerance = 2; // TODO: Find a way to reduce this to zero tolerance.

			yield return RunWholeTest("FastInvoke", invokeTime, startAtRandomTime, fixedUpdateCountTolerance,
				() => Subject.CallbackCallCount,
				() => Subject.FastInvoke(Subject.Callback, invokeTime),
				DoFastInvokingChecks
			);
		}

		private IEnumerator TestOutsiderFastInvoke(double invokeTime, bool startAtRandomTime)
		{
			var fixedUpdateCountTolerance = 2; // TODO: Find a way to reduce this to zero tolerance.

			yield return RunWholeTest("OutsiderFastInvoke", invokeTime, startAtRandomTime, fixedUpdateCountTolerance,
				() => OutsiderCallbackCallCount,
				() => Subject.FastInvoke(FastInvokeOutsiderCallback, invokeTime),
				DoOutsiderFastInvokingChecks
			);
		}

		private IEnumerator RunWholeTest(string method, double invokeTime, bool startAtRandomTime, int fixedUpdateCountTolerance, Func<int> getCallbackCallCount, Action doInvoke, Action<bool, int> doInvokingChecks)
		{
			yield return InitializeTest(startAtRandomTime);

			double fixedDeltaTime = Time.fixedDeltaTime;
			int fixedUpdateCount = 0;
			double passedTime = 0.0;

			int expectedFixedUpdateCount = invokeTime <= 0
				? 1
				: (int)Math.Floor(invokeTime / fixedDeltaTime);
			var expectedMinFixedUpdateCount = expectedFixedUpdateCount;
			var expectedMaxFixedUpdateCount = expectedFixedUpdateCount + fixedUpdateCountTolerance;
			double expectedPassedTime = invokeTime < 0
				? 0
				: invokeTime;

			doInvokingChecks(false, 0);
			//var startTime = (double)Time.time;
			doInvoke();
			Assert.True(getCallbackCallCount() == 0);
			doInvokingChecks(true, 1);
			while (getCallbackCallCount() == 0)
			{
				doInvokingChecks(true, 1);
				yield return new WaitForFixedUpdate();
				fixedUpdateCount++;
				passedTime += Time.deltaTime;
			}
			//var passedTime = Time.time - startTime;
			Assert.True(getCallbackCallCount() == 1);
			doInvokingChecks(false, 0);

			Check(method, invokeTime, passedTime, expectedPassedTime, fixedUpdateCount, expectedMinFixedUpdateCount, expectedMaxFixedUpdateCount, fixedDeltaTime);
		}

		private static void Check(string method, double invokeTime, double passedTime, double expectedPassedTime, int fixedUpdateCount, int expectedMinFixedUpdateCount, int expectedMaxFixedUpdateCount, double fixedDeltaTime)
		{
			const double tolerance = 0.00001; // In seconds
			var toleranceSteps = Mathf.Max(1, expectedMaxFixedUpdateCount - expectedMinFixedUpdateCount);
			var passedTimeResult =
				passedTime >= expectedPassedTime - tolerance &&
				passedTime <= expectedPassedTime + fixedDeltaTime * toleranceSteps + tolerance;

			var expectedFixedUpdateCountResult =
				fixedUpdateCount >= expectedMinFixedUpdateCount &&
				fixedUpdateCount <= expectedMaxFixedUpdateCount;

			if (!passedTimeResult || !expectedFixedUpdateCountResult)
			{
				Assert.Fail(
					$@"{method} failed.\n
					{(passedTimeResult ? "Passed" : "FAILED")}: Duration was '{passedTime}' where between '{expectedPassedTime}' and '{expectedPassedTime + fixedDeltaTime}' was expected.\n
					{(expectedFixedUpdateCountResult ? "Passed" : "FAILED")}: FixedUpdate count was '{fixedUpdateCount}' where between '{expectedMinFixedUpdateCount}' and '{expectedMaxFixedUpdateCount}' was expected.\n
					Fixed delta time: '{fixedDeltaTime}'.\n
					Invoke time: '{invokeTime}'.\n");
			}
		}

		#region Initialization

		private bool IsInitialized;

		private bool InitializeBase(float timeScale = 100f)
		{
			if (IsInitialized)
			{
				// Deinitialize first
				DeinitializeBase();
			}
			IsInitialized = true;

			Invoker.ResetSystem();
			//if (Invoker.IsFastInvokingAny())
			//{
			//	Assert.Fail("Left a previously set invoke already in action while starting to a new test.");
			//	return false;
			//}

			UnityTestTools.Cleanup();
			ResetSubject();
			//Subject.ResetCallback(); No need to call, but left this line here commented out for convenience.
			ResetOutsiderCallback();

			Time.timeScale = timeScale;
			return true;
		}

		private void DeinitializeBase()
		{
			if (!IsInitialized)
				throw new Exception("Test was not initialized.");
			IsInitialized = false;

			UnityTestTools.Cleanup();
			Invoker.ShutdownSystem();
			Time.timeScale = 1f;
		}

		#endregion

		#region Test Subject

		private Test_FastInvokeSubject _Subject;
		private Test_FastInvokeSubject Subject
		{
			get
			{
				if (!IsInitialized)
				{
					throw new Exception("Test was not initialized.");
				}
				return _Subject;
			}
		}

		private void ResetSubject()
		{
			var go = new GameObject("FastInvoke test object");
			_Subject = go.AddComponent<Test_FastInvokeSubject>();
		}

		#endregion

		#region Callback Outside Of Behaviour

		private int OutsiderCallbackCallCount;

		private void ResetOutsiderCallback()
		{
			OutsiderCallbackCallCount = 0;
		}

		private void FastInvokeOutsiderCallback()
		{
			OutsiderCallbackCallCount++;
		}

		#endregion

		#region Tools

		private void DoUnityInvokingChecks(bool shouldBeInvoking, int invokeCountShouldBe = -1)
		{
			// Ignore 'invokeCountShouldBe' since Unity provides no way to tell us that.

			if (shouldBeInvoking)
			{
				Assert.True(Subject.IsInvoking());
				Assert.True(Subject.IsInvoking("Callback"));
			}
			else
			{
				Assert.True(!Subject.IsInvoking());
				Assert.True(!Subject.IsInvoking("Callback"));
			}
		}

		private void DoFastInvokingChecks(bool shouldBeInvoking, int invokeCountShouldBe = -1)
		{
			if (shouldBeInvoking)
			{
				if (invokeCountShouldBe == 0)
				{
					Assert.Fail("Illogical to expect zero invoke count while also stated that something was expected.");
				}
				else if (invokeCountShouldBe > 0)
				{
					Assert.AreEqual(invokeCountShouldBe, Invoker.TotalFastInvokeCount());
					Assert.AreEqual(invokeCountShouldBe, Subject.FastInvokeCount());
					Assert.AreEqual(invokeCountShouldBe, Subject.FastInvokeCount(Subject.Callback));
				}
				else if (invokeCountShouldBe == -1)
				{
					// Expecting any count.
					Assert.Greater(Invoker.TotalFastInvokeCount(), 0);
					Assert.Greater(Subject.FastInvokeCount(), 0);
					Assert.Greater(Subject.FastInvokeCount(Subject.Callback), 0);
				}
				else
				{
					Assert.Fail(); // Internal error.
				}
				Assert.True(Invoker.IsFastInvokingAny());
				Assert.True(Subject.IsFastInvoking());
				Assert.True(Subject.IsFastInvoking(Subject.Callback));
			}
			else
			{
				if (invokeCountShouldBe > 0)
				{
					Assert.Fail("Illogical to expect '{0}' invoke count while also stated that nothing was expected.", invokeCountShouldBe);
				}
				Assert.False(Invoker.IsFastInvokingAny());
				Assert.False(Subject.IsFastInvoking());
				Assert.False(Subject.IsFastInvoking(Subject.Callback));
			}
		}

		private void DoOutsiderFastInvokingChecks(bool shouldBeInvoking, int invokeCountShouldBe = -1)
		{
			if (shouldBeInvoking)
			{
				if (invokeCountShouldBe == 0)
				{
					Assert.Fail("Illogical to expect zero invoke count while also stated that something was expected.");
				}
				else if (invokeCountShouldBe > 0)
				{
					Assert.AreEqual(invokeCountShouldBe, Invoker.TotalFastInvokeCount());
					Assert.AreEqual(invokeCountShouldBe, Subject.FastInvokeCount());
					Assert.AreEqual(invokeCountShouldBe, Subject.FastInvokeCount(FastInvokeOutsiderCallback));
				}
				else if (invokeCountShouldBe == -1)
				{
					// Expecting any count.
					Assert.Greater(Invoker.TotalFastInvokeCount(), 0);
					Assert.Greater(Subject.FastInvokeCount(), 0);
					Assert.Greater(Subject.FastInvokeCount(FastInvokeOutsiderCallback), 0);
				}
				else
				{
					Assert.Fail(); // Internal error.
				}
				Assert.True(Invoker.IsFastInvokingAny());
				Assert.True(Subject.IsFastInvoking());
				Assert.True(Subject.IsFastInvoking(FastInvokeOutsiderCallback));
			}
			else
			{
				if (invokeCountShouldBe > 0)
				{
					Assert.Fail("Illogical to expect '{0}' invoke count while also stated that nothing was expected.", invokeCountShouldBe);
				}
				Assert.False(Invoker.IsFastInvokingAny());
				Assert.False(Subject.IsFastInvoking());
				Assert.False(Subject.IsFastInvoking(FastInvokeOutsiderCallback));
			}
		}

		#endregion
	}

}
