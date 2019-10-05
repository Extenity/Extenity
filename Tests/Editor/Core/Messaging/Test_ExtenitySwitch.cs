using System;
using Extenity.MessagingToolbox;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ExtenityTests.MessagingToolbox
{

	public class Test_ExtenitySwitch : Test_SwitchTestBase
	{
		#region Basics

		[Test]
		public void InitiallySwitchedOff()
		{
			var isSwitchedOn = TestSwitch.IsSwitchedOn;
			Assert.That(isSwitchedOn, Is.False);
		}

		[Test]
		public void AlrightToSwitchWithoutCallbacks()
		{
			TestSwitch.SwitchOnSafe();
			TestSwitch.SwitchOffSafe();
		}

		#endregion

		#region Switching

		[Test]
		public void CallbackInstantlyInvoked_InitiallySwitchedOff()
		{
			TestSwitch.AddListener(CallbackOn, CallbackOff);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback."));
		}

		[Test]
		public void CallbackInstantlyInvoked_InitiallySwitchedOn()
		{
			TestSwitch.SwitchOnSafe();
			TestSwitch.AddListener(CallbackOn, CallbackOff);
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));
		}

		[Test]
		public void InvokeNonUnityObjectCallback()
		{
			TestSwitch.AddListener(CallbackOn, null);
			Assert.That(new Action(CallbackOn).Target as Object, Is.Null);

			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));
		}

		[Test]
		public void InvokeUnityObjectCallback()
		{
			TestSwitch.AddListener(CreateTestSwitchSubject().CallbackOn, null);
			Assert.That(new Action(TestSwitchSubject.CallbackOn).Target as Object, Is.Not.Null);

			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called Subject SwitchOn callback."));
		}

		[Test]
		public void InvokingWithMultipleListeners_Orderless()
		{
			TestSwitch.AddListener(CallbackOnA, null);
			TestSwitch.AddListener(CallbackOnB, null);
			TestSwitch.AddListener(CallbackOnC, null);

			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));
		}

		[Test]
		public void InvokingWithMultipleListeners_NegativeOrder()
		{
			TestSwitch.AddListener(CallbackOnA, null, -10);
			TestSwitch.AddListener(CallbackOnB, null, -20);
			TestSwitch.AddListener(CallbackOnC, null, -30);

			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback C."),
			                (LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback A."));
		}


		[Test]
		public void InvokingWithMultipleListeners_MixedOrders()
		{
			TestSwitch.AddListener(CallbackOnA, null, -10);
			TestSwitch.AddListener(CallbackOnB, null, -10);
			TestSwitch.AddListener(CallbackOnC, null, 0);
			TestSwitch.AddListener(CallbackOnD, null, 0);
			TestSwitch.AddListener(CallbackOnE, null, 30);
			TestSwitch.AddListener(CallbackOnF, null, 30);

			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."),
			                (LogType.Log, "Called SwitchOn callback D."),
			                (LogType.Log, "Called SwitchOn callback E."),
			                (LogType.Log, "Called SwitchOn callback F."));
		}

		[Test]
		public void InvokingSafeIsNotAffectedByExceptions()
		{
			TestSwitch.AddListener(CallbackOnA, null, 10);
			TestSwitch.AddListener(ThrowingCallback, null, 20);
			TestSwitch.AddListener(CallbackOnC, null, 30);

			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Exception, "Test_ExtenityEventException: Called throwing callback."),
			                (LogType.Log, "Called SwitchOn callback C."));
		}

		[Test]
		public void InvokingUnsafeDoesNotCatchExceptions()
		{
			TestSwitch.AddListener(CallbackOnA, null, 10);
			TestSwitch.AddListener(ThrowingCallback, null, 20);
			TestSwitch.AddListener(CallbackOnC, null, 30);

			Assert.Throws<Test_ExtenityEventException>(() => TestSwitch.SwitchOnUnsafe());
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."));
		}

		[Test]
		public void InvokingWhenSwitchingOnAndOffAndOnAndOff()
		{
			RegisterCallbacks();

			for (int i = 0; i < 10; i++)
			{
				TestSwitch.SwitchOnSafe();
				AssertExpectLog((LogType.Log, "Called SwitchOn callback."));
				TestSwitch.SwitchOffSafe();
				AssertExpectLog((LogType.Log, "Called SwitchOff callback."));
			}
		}

		[Test]
		public void SwitchingOffAtFirstWontCallTheOffCallback()
		{
			RegisterCallbacks();

			TestSwitch.SwitchOffSafe();
			AssertExpectNoLogs();
		}

		[Test]
		public void ConsecutiveSwitchingWontTriggerCallbacks()
		{
			RegisterCallbacks();

			TestSwitch.SwitchOffSafe();
			TestSwitch.SwitchOffSafe();
			TestSwitch.SwitchOffSafe();
			TestSwitch.SwitchOffSafe();
			AssertExpectNoLogs();

			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));

			TestSwitch.SwitchOnSafe();
			TestSwitch.SwitchOnSafe();
			TestSwitch.SwitchOnSafe();
			TestSwitch.SwitchOnSafe();
			AssertExpectNoLogs();
		}

		[Test]
		public void AddingListenersMoreThanOnceIsIgnored()
		{
			TestSwitch.AddListener(CallbackOn, CallbackOff);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback."));

			TestSwitch.AddListener(CallbackOn, CallbackOff);
			TestSwitch.AddListener(CallbackOn, CallbackOff);
			TestSwitch.AddListener(CallbackOn, CallbackOff);
			AssertExpectNoLogs();

			// Note that the callback is called only once.
			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));

			TestSwitch.AddListener(CallbackOn, CallbackOff);
			TestSwitch.AddListener(CallbackOn, CallbackOff);
			TestSwitch.AddListener(CallbackOn, CallbackOff);
			AssertExpectNoLogs();
		}

		[Test]
		public void AddingDifferentSetsOfListenersConsideredNotTheSame_Take1()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."));

			// SwitchOn callback is the same but SwitchOff callback is different.
			TestSwitch.AddListener(CallbackOnA, CallbackOffB);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback B."));

			// The same SwitchOn callback called twice, since they are considered as different callback sets.
			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback A."));

			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
		}

		[Test]
		public void AddingDifferentSetsOfListenersConsideredNotTheSame_Take2()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."));

			// SwitchOff callback is the same but SwitchOn callback is different.
			TestSwitch.AddListener(CallbackOnB, CallbackOffA);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."));

			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."));

			// The same SwitchOff callback called twice, since they are considered as different callback sets.
			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback A."));
		}

		[Test]
		public void AlrightNotToHaveSwitchOnCallback()
		{
			TestSwitch.AddListener(null, CallbackOffA);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."));

			TestSwitch.SwitchOnSafe();
			AssertExpectNoLogs();

			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."));

			TestSwitch.SwitchOnSafe();
			AssertExpectNoLogs();

			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."));
		}

		[Test]
		public void AlrightNotToHaveSwitchOffCallback()
		{
			TestSwitch.AddListener(CallbackOnA, null);
			AssertExpectNoLogs();

			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."));

			TestSwitch.SwitchOffSafe();
			AssertExpectNoLogs();

			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."));

			TestSwitch.SwitchOffSafe();
			AssertExpectNoLogs();
		}

		[Test]
		public void AlrightNotToHaveSwitchOnCallback_EvenHavingLifeSpanOfRemovedAtFirstEmit()
		{
			TestSwitch.AddListener(null, CallbackOffA, 0, ListenerLifeSpan.RemovedAtFirstEmit);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."));

			// We don't get any calls but the callback is removed at that point.
			TestSwitch.SwitchOnSafe();
			AssertExpectNoLogs();

			// There is no SwitchOff callbacks anymore.
			TestSwitch.SwitchOffSafe();
			AssertExpectNoLogs();
		}


		[Test]
		public void AlrightNotToHaveBothCallbacks()
		{
			TestSwitch.AddListener(null, null);
			AssertRegisteredCallbackCount(0);
		}

		#endregion

		#region Callback Life Span

		[Test]
		public void HavingLifeSpanOfRemovedAtFirstEmitConsideredFastTrackAndWontBeRegisteredIntoCallbacksList()
		{
			TestSwitch.SwitchOnSafe();
			TestSwitch.AddListener(CallbackOn, null, 0, ListenerLifeSpan.RemovedAtFirstEmit);
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));
			Assert.Zero(TestSwitch.Callbacks.Count); // The callback is instantly called and not registered into the list.
		}

		[Test]
		public void LifeSpan_Permanent()
		{
			RegisterCallbacks(0, ListenerLifeSpan.Permanent);

			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));
			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback."));
			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));

			// Manually removing is the only way. (or there is that LifeSpanTarget feature too)
			TestSwitch.RemoveListener(CallbackOn, CallbackOff);
			AssertRegisteredCallbackCount(0);

			TestSwitch.SwitchOffSafe();
			TestSwitch.SwitchOnSafe();
			AssertExpectNoLogs();
		}

		[Test]
		public void LifeSpan_Permanent_WithLifeSpanTargetDestroyedLater()
		{
			RegisterCallbacks(0, ListenerLifeSpan.Permanent, CreateLifeSpanTargetTestObject());

			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));
			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback."));
			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));

			// Destroy the LifeSpanTarget and the registered listener will not be called anymore.
			DestroyLifeSpanTargetTestObject();
			AssertRegisteredCallbackCount(0);

			TestSwitch.SwitchOffSafe();
			TestSwitch.SwitchOnSafe();
			AssertExpectNoLogs();
		}

		[Test]
		public void LifeSpan_Permanent_WithLifeSpanTargetDestroyedFirst()
		{
			RegisterCallbacks(0, ListenerLifeSpan.Permanent, CreateLifeSpanTargetTestObject());

			// Destroy the LifeSpanTarget and the registered listener will not be called anymore.
			DestroyLifeSpanTargetTestObject();
			AssertRegisteredCallbackCount(0);

			TestSwitch.SwitchOnSafe();
			TestSwitch.SwitchOffSafe();
			AssertExpectNoLogs();
		}

		[Test]
		public void LifeSpan_Permanent_WithDelegateTargetDestroyedLater()
		{
			RegisterSubjectCallbacks(0, ListenerLifeSpan.Permanent);

			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called Subject SwitchOn callback."));
			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called Subject SwitchOff callback."));
			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called Subject SwitchOn callback."));

			// Destroy the Subject and the registered listener will not be called anymore.
			DestroyTestSwitchSubject();
			AssertRegisteredCallbackCount(0);

			TestSwitch.SwitchOffSafe();
			TestSwitch.SwitchOnSafe();
			AssertExpectNoLogs();
		}

		[Test]
		public void LifeSpan_Permanent_WithDelegateTargetDestroyedFirst()
		{
			RegisterSubjectCallbacks(0, ListenerLifeSpan.Permanent);

			// Destroy the Subject and the registered listener will not be called anymore.
			DestroyTestSwitchSubject();
			AssertRegisteredCallbackCount(0);

			TestSwitch.SwitchOnSafe();
			TestSwitch.SwitchOffSafe();
			AssertExpectNoLogs();
		}

		[Test]
		public void LifeSpan_RemovedAtFirstEmit()
		{
			TestSwitch.AddListener(CallbackOn, null, 0, ListenerLifeSpan.RemovedAtFirstEmit);

			// The callback will be deregistered after this.
			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));
			AssertRegisteredCallbackCount(0);

			TestSwitch.SwitchOffSafe();
			TestSwitch.SwitchOnSafe();
			AssertExpectNoLogs();
		}

		[Test]
		public void LifeSpan_RemovedAtFirstEmit_WithLifeSpanTargetDestroyedLater()
		{
			RegisterCallbacks(0, ListenerLifeSpan.RemovedAtFirstEmit, CreateLifeSpanTargetTestObject());

			// The callback will be deregistered after this.
			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));
			AssertRegisteredCallbackCount(0);

			TestSwitch.SwitchOffSafe();
			AssertExpectNoLogs();

			// Destroying the LifeSpanTarget does nothing after that. The listener was already deregistered, thanks to RemovedAtFirstEmit.
			DestroyLifeSpanTargetTestObject();
			TestSwitch.SwitchOnSafe();
			TestSwitch.SwitchOffSafe();
			AssertExpectNoLogs();
		}

		[Test]
		public void LifeSpan_RemovedAtFirstEmit_WithLifeSpanTargetDestroyedFirst()
		{
			RegisterCallbacks(0, ListenerLifeSpan.RemovedAtFirstEmit, CreateLifeSpanTargetTestObject());

			// The callback will be deregistered after this.
			DestroyLifeSpanTargetTestObject();
			AssertRegisteredCallbackCount(0);

			TestSwitch.SwitchOnSafe();
			TestSwitch.SwitchOffSafe();
			AssertExpectNoLogs();
		}

		[Test]
		public void LifeSpan_RemovedAtFirstEmit_WithDelegateTargetDestroyedLater()
		{
			RegisterSubjectCallbacks(0, ListenerLifeSpan.RemovedAtFirstEmit);

			// The callback will be deregistered after this.
			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called Subject SwitchOn callback."));
			AssertRegisteredCallbackCount(0);

			TestSwitch.SwitchOffSafe();
			AssertExpectNoLogs();

			// Destroying the Subject does nothing after that. The listener was already deregistered, thanks to RemovedAtFirstEmit.
			DestroyTestSwitchSubject();
			TestSwitch.SwitchOnSafe();
			TestSwitch.SwitchOffSafe();
			AssertExpectNoLogs();
		}

		[Test]
		public void LifeSpan_RemovedAtFirstEmit_WithDelegateTargetDestroyedFirst()
		{
			RegisterSubjectCallbacks(0, ListenerLifeSpan.RemovedAtFirstEmit);

			// The callback will be deregistered after this.
			DestroyTestSwitchSubject();
			AssertRegisteredCallbackCount(0);

			TestSwitch.SwitchOnSafe();
			TestSwitch.SwitchOffSafe();
			AssertExpectNoLogs();
		}

		#endregion

		#region Callback Order

		[Test]
		public void CallbackOrderLimits()
		{
			// Min and Max are reserved for internal use.
			Assert.Catch<ArgumentOutOfRangeException>(() => TestSwitch.AddListener(CallbackOn, CallbackOff, int.MinValue));
			Assert.Catch<ArgumentOutOfRangeException>(() => TestSwitch.AddListener(CallbackOn, CallbackOff, int.MaxValue));
		}

		[Test]
		public void CallbackOrder()
		{
			TestSwitch.AddListener(
				() =>
				{
					Log.Info("Called SwitchOn callback with order 60.");
				},
				() =>
				{
					Log.Info("Called SwitchOff callback with order 60.");
				},
				60);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback with order 60."));

			TestSwitch.AddListener(
				() =>
				{
					Log.Info("Called SwitchOn callback with order -40.");
				},
				() =>
				{
					Log.Info("Called SwitchOff callback with order -40.");
				},
				-40);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback with order -40."));

			TestSwitch.AddListener(
				() =>
				{
					Log.Info("Called SwitchOn callback with default order, added first.");
				},
				() =>
				{
					Log.Info("Called SwitchOff callback with default order, added first.");
				}
			);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback with default order, added first."));

			TestSwitch.AddListener(
				() =>
				{
					Log.Info("Called SwitchOn callback with default order, added second.");
				},
				() =>
				{
					Log.Info("Called SwitchOff callback with default order, added second.");
				}
			);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback with default order, added second."));

			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback with order -40."),
			                (LogType.Log, "Called SwitchOn callback with default order, added first."),
			                (LogType.Log, "Called SwitchOn callback with default order, added second."),
			                (LogType.Log, "Called SwitchOn callback with order 60."));

			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback with order -40."),
			                (LogType.Log, "Called SwitchOff callback with default order, added first."),
			                (LogType.Log, "Called SwitchOff callback with default order, added second."),
			                (LogType.Log, "Called SwitchOff callback with order 60."));
		}

		#endregion

		#region General

		private void CallbackOn() { Log.Info("Called SwitchOn callback."); }
		private void CallbackOnA() { Log.Info("Called SwitchOn callback A."); }
		private void CallbackOnB() { Log.Info("Called SwitchOn callback B."); }
		private void CallbackOnC() { Log.Info("Called SwitchOn callback C."); }
		private void CallbackOnD() { Log.Info("Called SwitchOn callback D."); }
		private void CallbackOnE() { Log.Info("Called SwitchOn callback E."); }
		private void CallbackOnF() { Log.Info("Called SwitchOn callback F."); }
		private void CallbackOff() { Log.Info("Called SwitchOff callback."); }
		private void CallbackOffA() { Log.Info("Called SwitchOff callback A."); }
		private void CallbackOffB() { Log.Info("Called SwitchOff callback B."); }
		private void CallbackOffC() { Log.Info("Called SwitchOff callback C."); }
		private void CallbackOffD() { Log.Info("Called SwitchOff callback D."); }
		private void CallbackOffE() { Log.Info("Called SwitchOff callback E."); }
		private void CallbackOffF() { Log.Info("Called SwitchOff callback F."); }
		private void ThrowingCallback() { throw new Test_ExtenityEventException("Called throwing callback."); }

		private void RegisterCallbacks(int order = 0, ListenerLifeSpan lifeSpan = ListenerLifeSpan.Permanent, Object lifeSpanTarget = null)
		{
			TestSwitch.AddListener(CallbackOn, CallbackOff, order, lifeSpan, lifeSpanTarget);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback."));
			AssertRegisteredCallbackCount(1);
		}

		private void RegisterSubjectCallbacks(int order = 0, ListenerLifeSpan lifeSpan = ListenerLifeSpan.Permanent, Object lifeSpanTarget = null)
		{
			CreateTestSwitchSubject();
			TestSwitch.AddListener(TestSwitchSubject.CallbackOn, TestSwitchSubject.CallbackOff, order, lifeSpan, lifeSpanTarget);
			AssertExpectLog((LogType.Log, "Called Subject SwitchOff callback."));
			AssertRegisteredCallbackCount(1);
		}

		private void AssertRegisteredCallbackCount(int expectedCount)
		{
			Assert.AreEqual(expectedCount, TestSwitch.CallbacksAliveAndWellCount, "Unexpected registered callback count.");
		}

		#endregion
	}

}
