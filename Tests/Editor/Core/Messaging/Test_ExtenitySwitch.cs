using System;
using Extenity.MessagingToolbox;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ExtenityTests.MessagingToolbox
{

	// TODO: See region "Switching Inside Callback"

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

		#region Switching Inside Callback

		// TODO: BasicNestedCall
		// TODO: NestedCallsWontCheckForDeadlocks
		// TODO: RemovingListenerInsideNestedCall_FromTheEnd
		// TODO: RemovingListenerInsideNestedCall_FromTheBeginning
		// TODO: RemovingListenerInsideNestedCall_Self
		// TODO: RemovingListenerInsideNestedCall_SelfAndAddSelfAgain
		// TODO: AddingListenerInsideNestedCall_ToTheEnd
		// TODO: AddingListenerInsideNestedCall_ToTheBeginning
		// TODO: AddingListenerInsideNestedCall_SelfIsIgnored

		#endregion

		#region Callback Life Span

		[Test]
		public void EndsAfterRemovingListener()
		{
			TestSwitch.AddListener(CallbackOn, null);
			TestSwitch.RemoveListener(CallbackOn, null);

			TestSwitch.SwitchOnSafe();
			TestSwitch.SwitchOffSafe();
			TestSwitch.SwitchOnSafe();
			TestSwitch.SwitchOffSafe();
			AssertExpectNoLogs();
		}

		[Test]
		public void AlrightToRemoveListenerInsideCallback_ManualRemove()
		{
			TestSwitch.AddListener(CallbackOnAndRemove, CallbackOff);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback."));

			// Callback removes itself.
			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));

			// No more calls.
			TestSwitch.SwitchOnSafe();
			TestSwitch.SwitchOnSafe();
			AssertExpectNoLogs();
		}

		[Test]
		public void AlrightToRemoveListenerInsideCallback_UsingRemoveCurrentListener()
		{
			TestSwitch.AddListener(CallbackOnAndRemoveCurrent, CallbackOff);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback."));

			// Callback removes itself.
			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));

			// No more calls.
			TestSwitch.SwitchOnSafe();
			TestSwitch.SwitchOnSafe();
			AssertExpectNoLogs();
		}

		[Test]
		public void NotAlrightToCallRemoveCurrentListenerOutsideOfCallback()
		{
			TestSwitch.AddListener(CallbackOn, null);

			Assert.Throws<Exception>(() => TestSwitch.RemoveCurrentListener());
		}

		#region RemovingListenerDoesNotAffectOtherListeners

		[Test]
		public void RemovingListenerDoesNotAffectOtherListeners_Take1()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			TestSwitch.RemoveListener(CallbackOnA, CallbackOffA);

			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));
			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));
		}

		[Test]
		public void RemovingListenerDoesNotAffectOtherListeners_Take2()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			TestSwitch.RemoveListener(CallbackOnB, CallbackOffB);

			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback C."));
			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback C."));
		}

		[Test]
		public void RemovingListenerDoesNotAffectOtherListeners_Take3()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			TestSwitch.RemoveListener(CallbackOnC, CallbackOffC);

			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."));
			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."));
		}

		[Test]
		public void RemovingListenerDoesNotAffectOtherListeners_Take4()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			TestSwitch.RemoveListener(CallbackOnB, CallbackOffB);

			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback C."));
			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback C."));

			TestSwitch.RemoveListener(CallbackOnA, CallbackOffA);

			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback C."));
			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback C."));

			TestSwitch.RemoveListener(CallbackOnC, CallbackOffC);

			TestSwitch.SwitchOnSafe();
			TestSwitch.SwitchOffSafe();
			AssertExpectNoLogs();
		}

		#endregion

		#region RemovingInsideListenerDoesNotAffectOtherListeners_ManualRemove

		[Test]
		public void RemovingInsideListenerDoesNotAffectOtherListeners_ManualRemove_Take1On()
		{
			TestSwitch.AddListener(CallbackOnAAndRemoveCurrent, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));

			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void RemovingInsideListenerDoesNotAffectOtherListeners_ManualRemove_Take2On()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnBAndRemoveCurrent, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));

			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void RemovingInsideListenerDoesNotAffectOtherListeners_ManualRemove_Take3On()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnCAndRemoveCurrent, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));

			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
		}

		[Test]
		public void RemovingInsideListenerDoesNotAffectOtherListeners_ManualRemove_Take1Off()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffAAndRemoveCurrent);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));

			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void RemovingInsideListenerDoesNotAffectOtherListeners_ManualRemove_Take2Off()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffBAndRemoveCurrent);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback C."));

			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void RemovingInsideListenerDoesNotAffectOtherListeners_ManualRemove_Take3Off()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffCAndRemoveCurrent);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."));

			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
		}

		#endregion

		#region RemovingInsideListenerDoesNotAffectOtherListeners_UsingRemoveCurrentListener

		[Test]
		public void RemovingInsideListenerDoesNotAffectOtherListeners_UsingRemoveCurrentListener_Take1On()
		{
			TestSwitch.AddListener(CallbackOnAAndRemove, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));

			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void RemovingInsideListenerDoesNotAffectOtherListeners_UsingRemoveCurrentListener_Take2On()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnBAndRemove, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));

			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void RemovingInsideListenerDoesNotAffectOtherListeners_UsingRemoveCurrentListener_Take3On()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnCAndRemove, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));

			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
		}

		[Test]
		public void RemovingInsideListenerDoesNotAffectOtherListeners_UsingRemoveCurrentListener_Take1Off()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffAAndRemove);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));

			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void RemovingInsideListenerDoesNotAffectOtherListeners_UsingRemoveCurrentListener_Take2Off()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffBAndRemove);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback C."));

			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void RemovingInsideListenerDoesNotAffectOtherListeners_UsingRemoveCurrentListener_Take3Off()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffCAndRemove);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."));

			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
		}

		#endregion

		#region RemovingAnotherListenerInsideListenerDoesNotAffectOtherListeners - On

		[Test]
		public void RemovingAnotherListenerInsideListenerDoesNotAffectOtherListeners_OnARemoveB()
		{
			TestSwitch.AddListener(CallbackOnAAndRemoveB, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                //(LogType.Log, "Called SwitchOn callback B."), Removed when OnA is called.
			                (LogType.Log, "Called SwitchOn callback C."));

			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void RemovingAnotherListenerInsideListenerDoesNotAffectOtherListeners_OnARemoveC()
		{
			TestSwitch.AddListener(CallbackOnAAndRemoveC, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."));
			//(LogType.Log, "Called SwitchOn callback C.")); Removed when OnA is called.

			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
		}

		[Test]
		public void RemovingAnotherListenerInsideListenerDoesNotAffectOtherListeners_OnBRemoveA()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnBAndRemoveA, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."), // Not removed right now because OnA is called before OnB, then removed inside OnB.
			                (LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));

			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void RemovingAnotherListenerInsideListenerDoesNotAffectOtherListeners_OnBRemoveC()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnBAndRemoveC, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."));
			//(LogType.Log, "Called SwitchOn callback C.")); Removed when OnB is called.

			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
		}

		[Test]
		public void RemovingAnotherListenerInsideListenerDoesNotAffectOtherListeners_OnCRemoveA()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnCAndRemoveA, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."), // Not removed right now because OnA is called before OnC, then removed inside OnC.
			                (LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));

			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void RemovingAnotherListenerInsideListenerDoesNotAffectOtherListeners_OnCRemoveB()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnCAndRemoveB, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."), // Not removed right now because OnB is called before OnC, then removed inside OnC.
			                (LogType.Log, "Called SwitchOn callback C."));

			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		#endregion

		#region RemovingAnotherListenerInsideListenerDoesNotAffectOtherListeners - Off

		[Test]
		public void RemovingAnotherListenerInsideListenerDoesNotAffectOtherListeners_OffARemoveB()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffAAndRemoveB);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                //(LogType.Log, "Called SwitchOff callback B."), Removed when OffA is called.
			                (LogType.Log, "Called SwitchOff callback C."));

			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback C."));

			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void RemovingAnotherListenerInsideListenerDoesNotAffectOtherListeners_OffARemoveC()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffAAndRemoveC);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
			                //(LogType.Log, "Called SwitchOff callback C.")); Removed when OffA is called.

			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."));

			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
		}

		[Test]
		public void RemovingAnotherListenerInsideListenerDoesNotAffectOtherListeners_OffBRemoveA()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffBAndRemoveA);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."), // Not removed right now because OffA is called before OffB, then removed inside OffB.
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));

			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void RemovingAnotherListenerInsideListenerDoesNotAffectOtherListeners_OffBRemoveC()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffBAndRemoveC);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
			                //(LogType.Log, "Called SwitchOff callback C.")); Removed when OffB is called.

			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."));

			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
		}

		[Test]
		public void RemovingAnotherListenerInsideListenerDoesNotAffectOtherListeners_OffCRemoveA()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffCAndRemoveA);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."), // Not removed right now because OffA is called before OffC, then removed inside OffC.
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));

			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void RemovingAnotherListenerInsideListenerDoesNotAffectOtherListeners_OffCRemoveB()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffCAndRemoveB);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."), // Not removed right now because OffB is called before OffC, then removed inside OffC.
			                (LogType.Log, "Called SwitchOff callback C."));

			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback C."));

			TestSwitch.SwitchOffSafe();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		#endregion

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

		// @formatter:off
		private void CallbackOnAndRemove() { CallbackOn(); TestSwitch.RemoveListener(CallbackOn, CallbackOff); }
		private void CallbackOnAndRemoveCurrent(){ CallbackOn(); TestSwitch.RemoveCurrentListener(); }

		private void CallbackOnAAndRemoveCurrent(){ CallbackOnA(); TestSwitch.RemoveCurrentListener(); }
		private void CallbackOnBAndRemoveCurrent(){ CallbackOnB(); TestSwitch.RemoveCurrentListener(); }
		private void CallbackOnCAndRemoveCurrent(){ CallbackOnC(); TestSwitch.RemoveCurrentListener(); }
		private void CallbackOffAAndRemoveCurrent(){ CallbackOffA(); TestSwitch.RemoveCurrentListener(); }
		private void CallbackOffBAndRemoveCurrent(){ CallbackOffB(); TestSwitch.RemoveCurrentListener(); }
		private void CallbackOffCAndRemoveCurrent(){ CallbackOffC(); TestSwitch.RemoveCurrentListener(); }

		private void CallbackOnAAndRemove(){ CallbackOnA(); TestSwitch.RemoveListener(CallbackOnA, CallbackOffA); }
		private void CallbackOnBAndRemove(){ CallbackOnB(); TestSwitch.RemoveListener(CallbackOnB, CallbackOffB); }
		private void CallbackOnCAndRemove(){ CallbackOnC(); TestSwitch.RemoveListener(CallbackOnC, CallbackOffC); }
		private void CallbackOffAAndRemove(){ CallbackOffA(); TestSwitch.RemoveListener(CallbackOnA, CallbackOffA); }
		private void CallbackOffBAndRemove(){ CallbackOffB(); TestSwitch.RemoveListener(CallbackOnB, CallbackOffB); }
		private void CallbackOffCAndRemove(){ CallbackOffC(); TestSwitch.RemoveListener(CallbackOnC, CallbackOffC); }

		private void CallbackOnAAndRemoveB(){ CallbackOnA(); TestSwitch.RemoveListener(CallbackOnB, CallbackOffB); }
		private void CallbackOnAAndRemoveC(){ CallbackOnA(); TestSwitch.RemoveListener(CallbackOnC, CallbackOffC); }
		private void CallbackOnBAndRemoveA(){ CallbackOnB(); TestSwitch.RemoveListener(CallbackOnA, CallbackOffA); }
		private void CallbackOnBAndRemoveC(){ CallbackOnB(); TestSwitch.RemoveListener(CallbackOnC, CallbackOffC); }
		private void CallbackOnCAndRemoveA(){ CallbackOnC(); TestSwitch.RemoveListener(CallbackOnA, CallbackOffA); }
		private void CallbackOnCAndRemoveB(){ CallbackOnC(); TestSwitch.RemoveListener(CallbackOnB, CallbackOffB); }
		private void CallbackOffAAndRemoveB(){ CallbackOffA(); TestSwitch.RemoveListener(CallbackOnB, CallbackOffB); }
		private void CallbackOffAAndRemoveC(){ CallbackOffA(); TestSwitch.RemoveListener(CallbackOnC, CallbackOffC); }
		private void CallbackOffBAndRemoveA(){ CallbackOffB(); TestSwitch.RemoveListener(CallbackOnA, CallbackOffA); }
		private void CallbackOffBAndRemoveC(){ CallbackOffB(); TestSwitch.RemoveListener(CallbackOnC, CallbackOffC); }
		private void CallbackOffCAndRemoveA(){ CallbackOffC(); TestSwitch.RemoveListener(CallbackOnA, CallbackOffA); }
		private void CallbackOffCAndRemoveB(){ CallbackOffC(); TestSwitch.RemoveListener(CallbackOnB, CallbackOffB); }
		// @formatter:on


		private void RegisterOnlyOnCallback(int order = 0, ListenerLifeSpan lifeSpan = ListenerLifeSpan.Permanent, Object lifeSpanTarget = null)
		{
			TestSwitch.AddListener(CallbackOn, null, order, lifeSpan, lifeSpanTarget);
			AssertRegisteredCallbackCount(1);
		}

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
