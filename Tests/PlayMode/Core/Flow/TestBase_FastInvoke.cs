using System;
using System.Collections;
using Extenity.FlowToolbox;
using Extenity.UnityTestToolbox;
using NUnit.Framework;
using UnityEngine;

namespace ExtenityTests.FlowToolbox
{

	public class Test_FastInvokeSubject : MonoBehaviour
	{
		#region FixedUpdate Calls

		[NonSerialized]
		public int FixedUpdateCallCount;

		private void FixedUpdate()
		{
			FixedUpdateCallCount++;
			if (IsLoggingEnabled)
				Log.Info($"FixedUpdate calls: {FixedUpdateCallCount}  (Invoke callback calls: {CallbackCallCount})");
			if (ExpectedInvokeCallbackCallCountInFixedUpdate >= 0)
			{
				Assert.AreEqual(ExpectedInvokeCallbackCallCountInFixedUpdate, CallbackCallCount);
			}
		}

		#endregion

		#region Invoke Callback Calls

		[NonSerialized]
		public int CallbackCallCount;

		public void Callback()
		{
			CallbackCallCount++;
			if (IsLoggingEnabled)
				Log.Info($"Invoke callback calls: {CallbackCallCount}  (FixedUpdate calls: {FixedUpdateCallCount})");
			if (ExpectedFixedUpdateCallCountInInvokeCallback >= 0)
			{
				Assert.AreEqual(ExpectedFixedUpdateCallCountInInvokeCallback, FixedUpdateCallCount);
			}
		}

		#endregion

		#region Expected Call Counts

		[NonSerialized]
		public int ExpectedInvokeCallbackCallCountInFixedUpdate = -1;
		[NonSerialized]
		public int ExpectedFixedUpdateCallCountInInvokeCallback = -1;

		#endregion

		#region Logging

		[NonSerialized]
		public bool IsLoggingEnabled;

		public void EnableLogging()
		{
			IsLoggingEnabled = true;
		}

		#endregion
	}

	public abstract class TestBase_FastInvoke
	{
		#region Configuration

		protected const double FloatingTolerance = 0.00001;
		protected const float TimeScale = 100f;

		#endregion

		#region Test Initialization / Deinitialization

		protected bool IsInitialized;

		protected IEnumerator InitializeTest(bool startAtRandomTime)
		{
			if (IsInitialized)
			{
				// Deinitialize first
				DeinitializeBase();
			}

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

			InitializeBase();
		}

		private void InitializeBase()
		{
			if (IsInitialized)
				throw new Exception("Test was already initialized.");
			IsInitialized = true;

			Invoker.ResetSystem();
			UnityTestTools.Cleanup();
			Time.timeScale = TimeScale;

			CreateSubject();
			ResetOutsiderCallback();
		}

		private void DeinitializeBase()
		{
			if (!IsInitialized)
				throw new Exception("Test was not initialized.");
			IsInitialized = false;

			Invoker.ShutdownSystem();
			UnityTestTools.Cleanup();
			Time.timeScale = 1f;
		}

		[TearDown]
		public void TearDown()
		{
			DeinitializeBase();
		}

		#endregion

		#region Test Subject

		private Test_FastInvokeSubject _Subject;
		protected Test_FastInvokeSubject Subject
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

		private void CreateSubject()
		{
			if (_Subject)
			{
				throw new Exception(); // Subject should already be destroyed by now.
			}

			var go = new GameObject("FastInvoke test object");
			_Subject = go.AddComponent<Test_FastInvokeSubject>();
		}

		#endregion

		#region Invoke Callers

		protected IEnumerator TestInvoke_Zero(DoInvokeTemplate doInvoke, bool startAtRandomTime, int repeats)
		{
			Assert.Greater(repeats, 0);
			for (int i = 0; i < repeats; i++)
			{
				yield return doInvoke(0, startAtRandomTime);
				yield return doInvoke(-1, startAtRandomTime);
				yield return doInvoke(-6128, startAtRandomTime);
			}
		}

		protected IEnumerator TestInvoke_Various(DoInvokeTemplate doInvoke, bool startAtRandomTime, int repeats)
		{
			Assert.Greater(repeats, 0);
			for (int i = 0; i < repeats; i++)
			{
				yield return doInvoke(Time.fixedDeltaTime * 0.1, startAtRandomTime);
				yield return doInvoke(Time.fixedDeltaTime * 0.49, startAtRandomTime);
				yield return doInvoke(Time.fixedDeltaTime * 0.499, startAtRandomTime);
				yield return doInvoke(Time.fixedDeltaTime * 0.4999, startAtRandomTime);
				yield return doInvoke(Time.fixedDeltaTime * 0.5, startAtRandomTime);
				yield return doInvoke(Time.fixedDeltaTime * 0.51, startAtRandomTime);
				yield return doInvoke(Time.fixedDeltaTime * 0.501, startAtRandomTime);
				yield return doInvoke(Time.fixedDeltaTime * 0.5001, startAtRandomTime);
				yield return doInvoke(Time.fixedDeltaTime * 0.9, startAtRandomTime);
				yield return doInvoke(Time.fixedDeltaTime * 0.99, startAtRandomTime);
				yield return doInvoke(Time.fixedDeltaTime * 0.999, startAtRandomTime);
				yield return doInvoke(Time.fixedDeltaTime * 1.0, startAtRandomTime);
				yield return doInvoke(Time.fixedDeltaTime * 1.01, startAtRandomTime);
				yield return doInvoke(Time.fixedDeltaTime * 1.001, startAtRandomTime);
				yield return doInvoke(Time.fixedDeltaTime * 1.1, startAtRandomTime);
				yield return doInvoke(Time.fixedDeltaTime * 1.5, startAtRandomTime);
				yield return doInvoke(Time.fixedDeltaTime * 1.9, startAtRandomTime);
				yield return doInvoke(Time.fixedDeltaTime * 1.99, startAtRandomTime);
				yield return doInvoke(Time.fixedDeltaTime * 1.999, startAtRandomTime);
				yield return doInvoke(Time.fixedDeltaTime * 2.0, startAtRandomTime);
				yield return doInvoke(Time.fixedDeltaTime * 2.01, startAtRandomTime);
				yield return doInvoke(Time.fixedDeltaTime * 2.001, startAtRandomTime);
				yield return doInvoke(Time.fixedDeltaTime * 2.1, startAtRandomTime);

				yield return doInvoke(0.05, startAtRandomTime);
				yield return doInvoke(0.1, startAtRandomTime);
				yield return doInvoke(1, startAtRandomTime);
				yield return doInvoke(1.1329587, startAtRandomTime);
				yield return doInvoke(5.4515328, startAtRandomTime);
				yield return doInvoke(10, startAtRandomTime);

				for (int iRandom = 0; iRandom < 100; iRandom++)
				{
					yield return doInvoke(UnityEngine.Random.Range(0.001f, 10.0f), startAtRandomTime);
				}
			}
		}

		protected IEnumerator TestInvoke(DoInvokeTemplate doInvoke, double invokeTime, bool startAtRandomTime, int repeats)
		{
			Assert.Greater(repeats, 0);
			for (int i = 0; i < repeats; i++)
			{
				yield return doInvoke(invokeTime, startAtRandomTime);
			}
		}

		#endregion

		#region Do Invoke

		protected delegate IEnumerator DoInvokeTemplate(double invokeTime, bool startAtRandomTime);

		protected IEnumerator DoUnityInvoke(double invokeTime, bool startAtRandomTime)
		{
			// +2 tolerance because it seems Unity is not good with numbers.
			var fixedUpdateCountTolerance = 2;

			yield return RunWholeTest("Unity Invoke", invokeTime, startAtRandomTime, fixedUpdateCountTolerance,
				() => Subject.CallbackCallCount,
				() => Subject.Invoke(nameof(Test_FastInvokeSubject.Callback), (float)invokeTime),
				DoUnityInvokingChecks
			);
		}

		protected IEnumerator DoFastInvoke(double invokeTime, bool startAtRandomTime)
		{
			// Unlike Unity's implementation, there is no tolerance for FastInvoke. Because it's coded with love.
			var fixedUpdateCountTolerance = 2; // TODO: Find a way to reduce this to zero tolerance.
			//var fixedUpdateCountTolerance = 0;

			yield return RunWholeTest("Extenity FastInvoke", invokeTime, startAtRandomTime, fixedUpdateCountTolerance,
				() => Subject.CallbackCallCount,
				() => Subject.FastInvoke(Subject.Callback, invokeTime),
				DoFastInvokingChecks
			);
		}

		protected IEnumerator DoOutsiderFastInvoke(double invokeTime, bool startAtRandomTime)
		{
			// Unlike Unity's implementation, there is no tolerance for FastInvoke. Because it's coded with love.
			var fixedUpdateCountTolerance = 2; // TODO: Find a way to reduce this to zero tolerance.
			//var fixedUpdateCountTolerance = 0;

			yield return RunWholeTest("Extenity Outsider FastInvoke", invokeTime, startAtRandomTime, fixedUpdateCountTolerance,
				() => OutsiderCallbackCallCount,
				() => Subject.FastInvoke(FastInvokeOutsiderCallback, invokeTime),
				DoOutsiderFastInvokingChecks
			);
		}

		#endregion

		#region Hearth Of The Caller

		private IEnumerator RunWholeTest(string invokeMethodName, double invokeTime, bool startAtRandomTime, int fixedUpdateCountTolerance, Func<int> getCallbackCallCount, Action doInvoke, Action<bool, int> doInvokingChecks)
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

			var toleranceSteps = Mathf.Max(1, expectedMaxFixedUpdateCount - expectedMinFixedUpdateCount);
			var passedTimeResult =
				passedTime >= expectedPassedTime - FloatingTolerance &&
				passedTime <= expectedPassedTime + fixedDeltaTime * toleranceSteps + FloatingTolerance;

			var expectedFixedUpdateCountResult =
				fixedUpdateCount >= expectedMinFixedUpdateCount &&
				fixedUpdateCount <= expectedMaxFixedUpdateCount;

			if (!passedTimeResult || !expectedFixedUpdateCountResult)
			{
				Assert.Fail(
					$@"{invokeMethodName} failed.\n
					{(passedTimeResult ? "Passed" : "FAILED")}: Duration was '{passedTime}' where between '{expectedPassedTime}' and '{expectedPassedTime + fixedDeltaTime}' was expected.\n
					{(expectedFixedUpdateCountResult ? "Passed" : "FAILED")}: FixedUpdate count was '{fixedUpdateCount}' where between '{expectedMinFixedUpdateCount}' and '{expectedMaxFixedUpdateCount}' was expected.\n
					Fixed delta time: '{fixedDeltaTime}'.\n
					Invoke time: '{invokeTime}'.\n");
			}
		}

		protected void DoUnityInvokingChecks(bool shouldBeInvoking, int invokeCountShouldBe = -1)
		{
			// Ignore 'invokeCountShouldBe' since Unity provides no way to tell us that.

			if (shouldBeInvoking)
			{
				Assert.True(Subject.IsInvoking());
				Assert.True(Subject.IsInvoking(nameof(Test_FastInvokeSubject.Callback)));
			}
			else
			{
				Assert.True(!Subject.IsInvoking());
				Assert.True(!Subject.IsInvoking(nameof(Test_FastInvokeSubject.Callback)));
			}
		}

		protected void DoFastInvokingChecks(bool shouldBeInvoking, int invokeCountShouldBe = -1)
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

		protected void DoOutsiderFastInvokingChecks(bool shouldBeInvoking, int invokeCountShouldBe = -1)
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

		#region Callback Outside Of Behaviour

		protected int OutsiderCallbackCallCount;

		private void ResetOutsiderCallback()
		{
			OutsiderCallbackCallCount = 0;
		}

		protected void FastInvokeOutsiderCallback()
		{
			OutsiderCallbackCallCount++;
		}

		#endregion
	}

}
