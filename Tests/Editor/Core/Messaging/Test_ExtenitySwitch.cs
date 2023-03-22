using System;
using Extenity.MessagingToolbox;
using NUnit.Framework;
using UnityEngine;
using Logger = Extenity.Logger;
using Object = UnityEngine.Object;

namespace ExtenityTests.MessagingToolbox
{

	[TestFixture(true)]
	[TestFixture(false)]
	public class Test_ExtenitySwitch : Test_SwitchTestBase
	{
		#region Initialization

		public Test_ExtenitySwitch(bool usingUnsafe) : base(usingUnsafe) { }

		#endregion

		#region Basics

		[Test]
		public void InitiallySwitchedOff()
		{
			var isSwitchedOn = TestSwitch.IsSwitchedOn;
			Assert.That(isSwitchedOn, Is.False);
		}

		[Test]
		public void AlrightToSwitchWithoutListeners()
		{
			SwitchOn();
			SwitchOff();
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
			SwitchOn();
			TestSwitch.AddListener(CallbackOn, CallbackOff);
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));
		}

		[Test]
		public void InvokeNonUnityObjectCallback()
		{
			TestSwitch.AddListener(CallbackOn, null);
			Assert.That(new Action(CallbackOn).Target as Object, Is.Null);

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));
		}

		[Test]
		public void InvokeUnityObjectCallback()
		{
			CreateTestSwitchSubject();
			TestSwitch.AddListener(TestSwitchSubject.CallbackOn, null);
			Assert.That(new Action(TestSwitchSubject.CallbackOn).Target as Object, Is.Not.Null);

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));
		}

		[Test]
		public void InvokingWithMultipleListeners_Orderless()
		{
			TestSwitch.AddListener(CallbackOnA, null);
			TestSwitch.AddListener(CallbackOnB, null);
			TestSwitch.AddListener(CallbackOnC, null);

			SwitchOn();
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

			SwitchOn();
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

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."),
			                (LogType.Log, "Called SwitchOn callback D."),
			                (LogType.Log, "Called SwitchOn callback E."),
			                (LogType.Log, "Called SwitchOn callback F."));
		}

		// Not cool to call Safe or Unsafe exclusively since there are text fixture parameters for that, but whatever.
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

		// Not cool to call Safe or Unsafe exclusively since there are text fixture parameters for that, but whatever.
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
		public void InvokingContinuouslyAsSwitchingOnAndOffAndOnAndOff()
		{
			RegisterCallbacks();

			for (int i = 0; i < 10; i++)
			{
				SwitchOn();
				AssertExpectLog((LogType.Log, "Called SwitchOn callback."));
				SwitchOff();
				AssertExpectLog((LogType.Log, "Called SwitchOff callback."));
			}
		}

		[Test]
		public void SwitchingOffAtFirstWontCallTheOffCallback()
		{
			RegisterCallbacks();

			SwitchOff();
			AssertExpectNoLogs();
		}

		[Test]
		public void ConsecutiveSwitchingWontTriggerCallbacks()
		{
			RegisterCallbacks();

			SwitchOff();
			SwitchOff();
			SwitchOff();
			SwitchOff();
			AssertExpectNoLogs();

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));

			SwitchOn();
			SwitchOn();
			SwitchOn();
			SwitchOn();
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
			SwitchOn();
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
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback A."));

			SwitchOff();
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

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."));

			// The same SwitchOff callback called twice, since they are considered as different callback sets.
			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback A."));
		}

		[Test]
		public void AlrightNotToHaveSwitchOnCallback()
		{
			TestSwitch.AddListener(null, CallbackOffA);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."));

			SwitchOn();
			AssertExpectNoLogs();

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."));

			SwitchOn();
			AssertExpectNoLogs();

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."));
		}

		[Test]
		public void AlrightNotToHaveSwitchOffCallback()
		{
			TestSwitch.AddListener(CallbackOnA, null);
			AssertExpectNoLogs();

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."));

			SwitchOff();
			AssertExpectNoLogs();

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."));

			SwitchOff();
			AssertExpectNoLogs();
		}

		[Test]
		public void AlrightNotToHaveSwitchOnCallback_EvenHavingLifeSpanOfRemovedAtFirstEmit()
		{
			TestSwitch.AddListener(null, CallbackOffA, 0, ListenerLifeSpan.RemovedAtFirstEmit);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."));

			// We don't get any calls but the callback is removed at that point.
			SwitchOn();
			AssertExpectNoLogs();

			// There is no SwitchOff callbacks anymore.
			SwitchOff();
			AssertExpectNoLogs();
		}

		[Test]
		public void AlrightNotToHaveBothCallbacks_WhichIsSilentlyIgnored()
		{
			TestSwitch.AddListener(null, null);
			AssertRegisteredCallbackCount(0);
		}

		#endregion

		#region Nested Operations

		// See 117418312.
		// Nested operations are really hard to implement. So they are not allowed for now. These are some previous
		// thoughts on that topic.
		//    BasicNestedCall
		//    NestedCallsWontCheckForDeadlocks
		//    RemovingListenerInsideNestedCall_FromTheEnd
		//    RemovingListenerInsideNestedCall_FromTheBeginning
		//    RemovingListenerInsideNestedCall_Self
		//    RemovingListenerInsideNestedCall_SelfAndAddSelfAgain
		//    AddingListenerInsideNestedCall_ToTheEnd
		//    AddingListenerInsideNestedCall_ToTheBeginning
		//    AddingListenerInsideNestedCall_SelfIsIgnored

		[Test]
		public void NestedSwitchesAreNotAllowed()
		{
			TestSwitch.AddListener(SwitchOff, null);
			SwitchOn();
			AssertExpectLog((LogType.Exception, "Exception: Invoked switch off while an invocation is ongoing."));
		}

		// Not cool to call Safe or Unsafe exclusively since there are text fixture parameters for that, but whatever.
		[Test]
		public void NestedAddListenerIsNotAllowed_Safe()
		{
			TestSwitch.AddListener(() => TestSwitch.AddListener(CallbackOn, CallbackOff), null);
			TestSwitch.SwitchOnSafe();
			AssertExpectLog((LogType.Exception, "NotSupportedException: Adding listener while invoking is not supported."));
		}

		// Not cool to call Safe or Unsafe exclusively since there are text fixture parameters for that, but whatever.
		[Test]
		public void NestedAddListenerIsNotAllowed_Unsafe()
		{
			TestSwitch.AddListener(() => TestSwitch.AddListener(CallbackOn, CallbackOff), null);
			Assert.Throws<NotSupportedException>(() => TestSwitch.SwitchOnUnsafe());
		}

		#endregion

		// Note that the system does not take the responsibility of handling these edge cases. Tests are here to show
		// how the system should not be used, rather than presenting the very best experience.

		#region Registering Edge Cases

#if DevelopingNestedAddListenerSupport
		[Test]
		public void AddingTheListenerOnceMoreInsideTheCallbackWillBeIgnored_WithLifeSpanOfPermanent()
		{
			var callCount = 0;

			void CallbackThatAddsSelf()
			{
				callCount++;
				Log.Info("Called " + callCount);

				// We try to add it one more time. The system ignores because the callback already exists.
				TestSwitch.AddListener(CallbackThatAddsSelf, null, 0, ListenerLifeSpan.Permanent);
			}

			TestSwitch.AddListener(CallbackThatAddsSelf, null, 0, ListenerLifeSpan.Permanent);
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called 1"));
		}

		[Test]
		public void AddingTheListenerOnceMoreInsideTheCallbackWillCauseDeadLock_WithLifeSpanOfRemovedAtFirstEmit()
		{
			var callCount = 0;

			void CallbackThatAddsSelf()
			{
				callCount++;
				Log.Info("Called " + callCount);
				if (callCount >= 3)
					throw new Test_ExtenitySwitchException("Hard brakes!"); // Enough

				// The callback is deregistered at this point due to RemovedAtFirstEmit. Then we try to add it one more
				// time. But note that the system calls the callback immediately, which causes dead lock.
				TestSwitch.AddListener(CallbackThatAddsSelf, null, 0, ListenerLifeSpan.RemovedAtFirstEmit);
			}

			TestSwitch.AddListener(CallbackThatAddsSelf, null, 0, ListenerLifeSpan.RemovedAtFirstEmit);
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called 1"),
			                (LogType.Log, "Called 2"),
			                (LogType.Log, "Called 3"),
			                (LogType.Exception, "Test_ExtenitySwitchException: Hard brakes!"));
		}

#endif

		#endregion

		#region Callback Life Span

		[Test]
		public void EndsAfterRemovingListener()
		{
			TestSwitch.AddListener(CallbackOn, null);
			TestSwitch.RemoveListener(CallbackOn, null);

			SwitchOn();
			SwitchOff();
			SwitchOn();
			SwitchOff();
			AssertExpectNoLogs();
		}

		[Test]
		public void AlrightToRemoveListenerInsideCallback_ManualRemove()
		{
			TestSwitch.AddListener(CallbackOnAndRemove, CallbackOff);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback."));

			// Callback removes itself.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));

			// No more calls.
			SwitchOff();
			SwitchOn();
			AssertExpectNoLogs();
		}

		[Test]
		public void AlrightToRemoveListenerInsideCallback_UsingRemoveCurrentListener()
		{
			TestSwitch.AddListener(CallbackOnAndRemoveSelf, CallbackOff);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback."));

			// Callback removes itself.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));

			// No more calls.
			SwitchOff();
			SwitchOn();
			AssertExpectNoLogs();
		}

		[Test]
		public void NotAlrightToCallRemoveCurrentListenerOutsideOfCallback()
		{
			TestSwitch.AddListener(CallbackOn, null);

			Assert.Throws<Exception>(() => TestSwitch.RemoveCurrentListener());
		}

		#region RemovingListener_DoesNotAffectOtherListeners

		[Test]
		public void RemovingListener_DoesNotAffectOtherListeners_Take1()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			TestSwitch.RemoveListener(CallbackOnA, CallbackOffA);

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));
			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void RemovingListener_DoesNotAffectOtherListeners_Take2()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			TestSwitch.RemoveListener(CallbackOnB, CallbackOffB);

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback C."));
			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void RemovingListener_DoesNotAffectOtherListeners_Take3()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			TestSwitch.RemoveListener(CallbackOnC, CallbackOffC);

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."));
			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
		}

		[Test]
		public void RemovingListener_DoesNotAffectOtherListeners_Take4()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			TestSwitch.RemoveListener(CallbackOnB, CallbackOffB);

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback C."));
			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback C."));

			TestSwitch.RemoveListener(CallbackOnA, CallbackOffA);

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback C."));
			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback C."));

			TestSwitch.RemoveListener(CallbackOnC, CallbackOffC);

			SwitchOn();
			SwitchOff();
			AssertExpectNoLogs();
		}

		#endregion

		#region RemovingListener_InsideListener_DoesNotAffectOtherListeners_ManualRemove

		[Test]
		public void RemovingListener_InsideListener_DoesNotAffectOtherListeners_ManualRemove_OnARemovesA()
		{
			TestSwitch.AddListener(CallbackOnAAndRemoveA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void RemovingListener_InsideListener_DoesNotAffectOtherListeners_ManualRemove_OnBRemovesB()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnBAndRemoveB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void RemovingListener_InsideListener_DoesNotAffectOtherListeners_ManualRemove_OnCRemovesC()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnCAndRemoveC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
		}

		[Test]
		public void RemovingListener_InsideListener_DoesNotAffectOtherListeners_ManualRemove_OffARemovesA()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffAAndRemoveA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void RemovingListener_InsideListener_DoesNotAffectOtherListeners_ManualRemove_OffBRemovesB()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffBAndRemoveB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback C."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void RemovingListener_InsideListener_DoesNotAffectOtherListeners_ManualRemove_OffCRemovesC()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffCAndRemoveC);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
		}

		#endregion

		#region RemovingListener_InsideListener_DoesNotAffectOtherListeners_UsingRemoveCurrentListener

		[Test]
		public void RemovingListener_InsideListener_DoesNotAffectOtherListeners_UsingRemoveCurrentListener_OnARemovesSelf()
		{
			TestSwitch.AddListener(CallbackOnAAndRemoveSelf, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void RemovingListener_InsideListener_DoesNotAffectOtherListeners_UsingRemoveCurrentListener_OnBRemovesSelf()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnBAndRemoveSelf, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void RemovingListener_InsideListener_DoesNotAffectOtherListeners_UsingRemoveCurrentListener_OnCRemovesSelf()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnCAndRemoveSelf, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
		}

		[Test]
		public void RemovingListener_InsideListener_DoesNotAffectOtherListeners_UsingRemoveCurrentListener_OffARemovesSelf()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffAAndRemoveSelf);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void RemovingListener_InsideListener_DoesNotAffectOtherListeners_UsingRemoveCurrentListener_OffBRemovesSelf()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffBAndRemoveSelf);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback C."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void RemovingListener_InsideListener_DoesNotAffectOtherListeners_UsingRemoveCurrentListener_OffCRemovesSelf()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffCAndRemoveSelf);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
		}

		#endregion

		#region RemovingAnotherListener_InsideListener_DoesNotAffectOtherListeners - On

		[Test]
		public void RemovingAnotherListener_InsideListener_DoesNotAffectOtherListeners_OnARemovesB()
		{
			TestSwitch.AddListener(CallbackOnAAndRemoveB, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                //(LogType.Log, "Called SwitchOn callback B."), Removed when OnA is called.
			                (LogType.Log, "Called SwitchOn callback C."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void RemovingAnotherListener_InsideListener_DoesNotAffectOtherListeners_OnARemovesC()
		{
			TestSwitch.AddListener(CallbackOnAAndRemoveC, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."));
			//              (LogType.Log, "Called SwitchOn callback C.")); Removed when OnA is called.

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
		}

		[Test]
		public void RemovingAnotherListener_InsideListener_DoesNotAffectOtherListeners_OnBRemovesA()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnBAndRemoveA, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."), // Not removed right now because OnA is called before OnB, then removed inside OnB.
			                (LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void RemovingAnotherListener_InsideListener_DoesNotAffectOtherListeners_OnBRemovesC()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnBAndRemoveC, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."));
			//              (LogType.Log, "Called SwitchOn callback C.")); Removed when OnB is called.

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
		}

		[Test]
		public void RemovingAnotherListener_InsideListener_DoesNotAffectOtherListeners_OnCRemovesA()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnCAndRemoveA, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."), // Not removed right now because OnA is called before OnC, then removed inside OnC.
			                (LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void RemovingAnotherListener_InsideListener_DoesNotAffectOtherListeners_OnCRemovesB()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnCAndRemoveB, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."), // Not removed right now because OnB is called before OnC, then removed inside OnC.
			                (LogType.Log, "Called SwitchOn callback C."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		#endregion

		#region RemovingAnotherListener_InsideListener_DoesNotAffectOtherListeners - Off

		[Test]
		public void RemovingAnotherListener_InsideListener_DoesNotAffectOtherListeners_OffARemovesB()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffAAndRemoveB);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."), // OffA fails to remove B because it was not registered at the time it was tried to be removed.
			                (LogType.Log, "Called SwitchOff callback C."));

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));
		}

		[Test]
		public void RemovingAnotherListener_InsideListener_DoesNotAffectOtherListeners_OffARemovesC()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffAAndRemoveC);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C.")); // OffA fails to remove C because it was not registered at the time it was tried to be removed.

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));
		}

		[Test]
		public void RemovingAnotherListener_InsideListener_DoesNotAffectOtherListeners_OffBRemovesA()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffBAndRemoveA);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."), // Not removed right now because OffA is called before OffB, then removed inside OffB.
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));
		}

		[Test]
		public void RemovingAnotherListener_InsideListener_DoesNotAffectOtherListeners_OffBRemovesC()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffBAndRemoveC);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C.")); // OffB fails to remove C because it was not registered at the time it was tried to be removed.

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));
		}

		[Test]
		public void RemovingAnotherListener_InsideListener_DoesNotAffectOtherListeners_OffCRemovesA()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffCAndRemoveA);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."), // Not removed right now because OffA is called before OffC, then removed inside OffC.
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));
		}

		[Test]
		public void RemovingAnotherListener_InsideListener_DoesNotAffectOtherListeners_OffCRemovesB()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffCAndRemoveB);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."), // Not removed right now because OffB is called before OffC, then removed inside OffC.
			                (LogType.Log, "Called SwitchOff callback C."));

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback C."));
		}

		#endregion

		[Test]
		public void HavingLifeSpanOfRemovedAtFirstEmitConsideredFastTrackAndWontBeRegisteredIntoCallbacksList()
		{
			// Switch-on before adding the listener. That makes the switch-on callback to be immediately called.
			// In that case, there is no need to register the callback, since it should be instantly removed afterwards.
			SwitchOn();
			TestSwitch.AddListener(CallbackOn, null, 0, ListenerLifeSpan.RemovedAtFirstEmit);
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));
			AssertRegisteredCallbackCount(0); // The callback is instantly called and not registered into the list.
		}

		[Test]
		public void LifeSpan_Permanent()
		{
			RegisterCallbacks(0, ListenerLifeSpan.Permanent);

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));
			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback."));
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));

			// Manually removing is the only way. (or there is that LifeSpanTarget feature too)
			TestSwitch.RemoveListener(CallbackOn, CallbackOff);
			AssertRegisteredCallbackCount(0);

			SwitchOff();
			SwitchOn();
			AssertExpectNoLogs();
		}

		[Test]
		public void LifeSpan_Permanent_WithLifeSpanTargetDestroyedLater()
		{
			CreateLifeSpanTargetTestObject();
			RegisterCallbacks(0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));
			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback."));
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));

			// Destroy the LifeSpanTarget and the registered listener will not be called anymore.
			DestroyLifeSpanTargetTestObject();
			AssertRegisteredCallbackCount(0);

			SwitchOff();
			SwitchOn();
			AssertExpectNoLogs();
		}

		[Test]
		public void LifeSpan_Permanent_WithLifeSpanTargetDestroyedFirst()
		{
			CreateLifeSpanTargetTestObject();
			RegisterCallbacks(0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);

			// Destroy the LifeSpanTarget and the registered listener will not be called anymore.
			DestroyLifeSpanTargetTestObject();
			AssertRegisteredCallbackCount(0);

			SwitchOn();
			SwitchOff();
			AssertExpectNoLogs();
		}

		[Test]
		public void LifeSpan_Permanent_WithDelegateTargetDestroyedLater()
		{
			RegisterSubjectCallbacks(0, ListenerLifeSpan.Permanent);

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));
			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback."));
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));

			// Destroy the Subject and the registered listener will not be called anymore.
			DestroyTestSwitchSubject();
			AssertRegisteredCallbackCount(0);

			SwitchOff();
			SwitchOn();
			AssertExpectNoLogs();
		}

		[Test]
		public void LifeSpan_Permanent_WithDelegateTargetDestroyedFirst()
		{
			RegisterSubjectCallbacks(0, ListenerLifeSpan.Permanent);

			// Destroy the Subject and the registered listener will not be called anymore.
			DestroyTestSwitchSubject();
			AssertRegisteredCallbackCount(0);

			SwitchOn();
			SwitchOff();
			AssertExpectNoLogs();
		}

		[Test]
		public void LifeSpan_RemovedAtFirstEmit()
		{
			TestSwitch.AddListener(CallbackOn, null, 0, ListenerLifeSpan.RemovedAtFirstEmit);

			// The callback will be deregistered after this.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));
			AssertRegisteredCallbackCount(0);

			SwitchOff();
			SwitchOn();
			AssertExpectNoLogs();
		}

		[Test]
		public void LifeSpan_RemovedAtFirstEmit_WithLifeSpanTargetDestroyedLater()
		{
			CreateLifeSpanTargetTestObject();
			RegisterCallbacks(0, ListenerLifeSpan.RemovedAtFirstEmit, LifeSpanTargetTestObject);

			// The callback will be deregistered after this.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));
			AssertRegisteredCallbackCount(0);

			SwitchOff();
			AssertExpectNoLogs();

			// Destroying the LifeSpanTarget does nothing after that. The listener was already deregistered, thanks to RemovedAtFirstEmit.
			DestroyLifeSpanTargetTestObject();
			SwitchOn();
			SwitchOff();
			AssertExpectNoLogs();
		}

		[Test]
		public void LifeSpan_RemovedAtFirstEmit_WithLifeSpanTargetDestroyedFirst()
		{
			CreateLifeSpanTargetTestObject();
			RegisterCallbacks(0, ListenerLifeSpan.RemovedAtFirstEmit, LifeSpanTargetTestObject);

			// The callback will be deregistered after this.
			DestroyLifeSpanTargetTestObject();
			AssertRegisteredCallbackCount(0);

			SwitchOn();
			SwitchOff();
			AssertExpectNoLogs();
		}

		[Test]
		public void LifeSpan_RemovedAtFirstEmit_WithDelegateTargetDestroyedLater()
		{
			RegisterSubjectCallbacks(0, ListenerLifeSpan.RemovedAtFirstEmit);

			// The callback will be deregistered after this.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));
			AssertRegisteredCallbackCount(0);

			SwitchOff();
			AssertExpectNoLogs();

			// Destroying the Subject does nothing after that. The listener was already deregistered, thanks to RemovedAtFirstEmit.
			DestroyTestSwitchSubject();
			SwitchOn();
			SwitchOff();
			AssertExpectNoLogs();
		}

		[Test]
		public void LifeSpan_RemovedAtFirstEmit_WithDelegateTargetDestroyedFirst()
		{
			RegisterSubjectCallbacks(0, ListenerLifeSpan.RemovedAtFirstEmit);

			// The callback will be deregistered after this.
			DestroyTestSwitchSubject();
			AssertRegisteredCallbackCount(0);

			SwitchOn();
			SwitchOff();
			AssertExpectNoLogs();
		}

		#region LifeSpan_RemovedAtFirstEmit_DoesNotAffectOtherListeners

		[Test]
		public void LifeSpan_RemovedAtFirstEmit_DoesNotAffectOtherListeners_OnA()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA, 0, ListenerLifeSpan.RemovedAtFirstEmit);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB, 0, ListenerLifeSpan.Permanent);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC, 0, ListenerLifeSpan.Permanent);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));
			AssertRegisteredCallbackCount(2);

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void LifeSpan_RemovedAtFirstEmit_DoesNotAffectOtherListeners_OnB()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA, 0, ListenerLifeSpan.Permanent);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB, 0, ListenerLifeSpan.RemovedAtFirstEmit);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC, 0, ListenerLifeSpan.Permanent);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));
			AssertRegisteredCallbackCount(2);

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void LifeSpan_RemovedAtFirstEmit_DoesNotAffectOtherListeners_OnC()
		{
			TestSwitch.AddListener(CallbackOnA, CallbackOffA, 0, ListenerLifeSpan.Permanent);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB, 0, ListenerLifeSpan.Permanent);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC, 0, ListenerLifeSpan.RemovedAtFirstEmit);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));
			AssertRegisteredCallbackCount(2);

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
		}

		#endregion

		#region DestroyingLifeSpanTarget_DoesNotAffectOtherListeners

		[Test]
		public void DestroyingLifeSpanTarget_DoesNotAffectOtherListeners_Take1()
		{
			CreateLifeSpanTargetTestObject();
			TestSwitch.AddListener(CallbackOnA, CallbackOffA, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			DestroyLifeSpanTargetTestObject();

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));
			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void DestroyingLifeSpanTarget_DoesNotAffectOtherListeners_Take2()
		{
			CreateLifeSpanTargetTestObject();
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			DestroyLifeSpanTargetTestObject();

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback C."));
			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void DestroyingLifeSpanTarget_DoesNotAffectOtherListeners_Take3()
		{
			CreateLifeSpanTargetTestObject();
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			DestroyLifeSpanTargetTestObject();

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."));
			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
		}

		#endregion

		#region DestroyingLifeSpanTarget_InsideListener_DoesNotAffectOtherListeners

		[Test]
		public void DestroyingLifeSpanTarget_InsideListener_DoesNotAffectOtherListeners_OnADestroysA()
		{
			CreateLifeSpanTargetTestObject();
			TestSwitch.AddListener(CallbackOnAAndDestroyLifeSpanTarget, CallbackOffA, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void DestroyingLifeSpanTarget_InsideListener_DoesNotAffectOtherListeners_OnBDestroysB()
		{
			CreateLifeSpanTargetTestObject();
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnBAndDestroyLifeSpanTarget, CallbackOffB, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void DestroyingLifeSpanTarget_InsideListener_DoesNotAffectOtherListeners_OnCDestroysC()
		{
			CreateLifeSpanTargetTestObject();
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnCAndDestroyLifeSpanTarget, CallbackOffC, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
		}

		[Test]
		public void DestroyingLifeSpanTarget_InsideListener_DoesNotAffectOtherListeners_OffADestroysA()
		{
			CreateLifeSpanTargetTestObject();
			TestSwitch.AddListener(CallbackOnA, CallbackOffAAndDestroyLifeSpanTarget, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void DestroyingLifeSpanTarget_InsideListener_DoesNotAffectOtherListeners_OffBDestroysB()
		{
			CreateLifeSpanTargetTestObject();
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffBAndDestroyLifeSpanTarget, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback C."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void DestroyingLifeSpanTarget_InsideListener_DoesNotAffectOtherListeners_OffCDestroysC()
		{
			CreateLifeSpanTargetTestObject();
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffCAndDestroyLifeSpanTarget, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
		}

		#endregion

		#region DestroyingLifeSpanTarget_InsideListener_DoesNotAffectOtherListeners - On

		[Test]
		public void DestroyingLifeSpanTarget_InsideListener_DoesNotAffectOtherListeners_OnADestroysB()
		{
			CreateLifeSpanTargetTestObject();
			TestSwitch.AddListener(CallbackOnAAndDestroyLifeSpanTarget, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                //(LogType.Log, "Called SwitchOn callback B."), Removed when OnA is called.
			                (LogType.Log, "Called SwitchOn callback C."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void DestroyingLifeSpanTarget_InsideListener_DoesNotAffectOtherListeners_OnADestroysC()
		{
			CreateLifeSpanTargetTestObject();
			TestSwitch.AddListener(CallbackOnAAndDestroyLifeSpanTarget, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."));
			//              (LogType.Log, "Called SwitchOn callback C.")); Removed when OnA is called.

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
		}

		[Test]
		public void DestroyingLifeSpanTarget_InsideListener_DoesNotAffectOtherListeners_OnBDestroysA()
		{
			CreateLifeSpanTargetTestObject();
			TestSwitch.AddListener(CallbackOnA, CallbackOffA, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);
			TestSwitch.AddListener(CallbackOnBAndDestroyLifeSpanTarget, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."), // Not removed right now because OnA is called before OnB, then removed inside OnB.
			                (LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void DestroyingLifeSpanTarget_InsideListener_DoesNotAffectOtherListeners_OnBDestroysC()
		{
			CreateLifeSpanTargetTestObject();
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnBAndDestroyLifeSpanTarget, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."));
			//              (LogType.Log, "Called SwitchOn callback C.")); Removed when OnB is called.

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
		}

		[Test]
		public void DestroyingLifeSpanTarget_InsideListener_DoesNotAffectOtherListeners_OnCDestroysA()
		{
			CreateLifeSpanTargetTestObject();
			TestSwitch.AddListener(CallbackOnA, CallbackOffA, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnCAndDestroyLifeSpanTarget, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."), // Not removed right now because OnA is called before OnC, then removed inside OnC.
			                (LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void DestroyingLifeSpanTarget_InsideListener_DoesNotAffectOtherListeners_OnCDestroysB()
		{
			CreateLifeSpanTargetTestObject();
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);
			TestSwitch.AddListener(CallbackOnCAndDestroyLifeSpanTarget, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."), // Not removed right now because OnB is called before OnC, then removed inside OnC.
			                (LogType.Log, "Called SwitchOn callback C."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		#endregion

		#region DestroyingLifeSpanTarget_InsideListener_DoesNotAffectOtherListeners - Off

		[Test]
		public void DestroyingLifeSpanTarget_InsideListener_DoesNotAffectOtherListeners_OffADestroysB()
		{
			CreateLifeSpanTargetTestObject();
			TestSwitch.AddListener(CallbackOnA, CallbackOffAAndDestroyLifeSpanTarget);
			Assert.Throws<Exception>(() => TestSwitch.AddListener(CallbackOnB, CallbackOffB, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject)); // Because LifeSpanTarget was destroyed above.
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                //(LogType.Log, "Called SwitchOff callback B."), Removed when OffA is called.
			                (LogType.Log, "Called SwitchOff callback C."));

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback C."));
		}

		[Test]
		public void DestroyingLifeSpanTarget_InsideListener_DoesNotAffectOtherListeners_OffADestroysC()
		{
			CreateLifeSpanTargetTestObject();
			TestSwitch.AddListener(CallbackOnA, CallbackOffAAndDestroyLifeSpanTarget);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			Assert.Throws<Exception>(() => TestSwitch.AddListener(CallbackOnC, CallbackOffC, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject)); // Because LifeSpanTarget was destroyed above.
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
			//              (LogType.Log, "Called SwitchOff callback C.")); Removed when OffA is called.

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."));
		}

		[Test]
		public void DestroyingLifeSpanTarget_InsideListener_DoesNotAffectOtherListeners_OffBDestroysA()
		{
			CreateLifeSpanTargetTestObject();
			TestSwitch.AddListener(CallbackOnA, CallbackOffA, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);
			TestSwitch.AddListener(CallbackOnB, CallbackOffBAndDestroyLifeSpanTarget);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."), // Not removed right now because OffA is called before OffB, then removed inside OffB.
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));
		}

		[Test]
		public void DestroyingLifeSpanTarget_InsideListener_DoesNotAffectOtherListeners_OffBDestroysC()
		{
			CreateLifeSpanTargetTestObject();
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffBAndDestroyLifeSpanTarget);
			Assert.Throws<Exception>(() => TestSwitch.AddListener(CallbackOnC, CallbackOffC, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject)); // Because LifeSpanTarget was destroyed above.
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
			//              (LogType.Log, "Called SwitchOff callback C.")); Removed when OffB is called.

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."));
		}

		[Test]
		public void DestroyingLifeSpanTarget_InsideListener_DoesNotAffectOtherListeners_OffCDestroysA()
		{
			CreateLifeSpanTargetTestObject();
			TestSwitch.AddListener(CallbackOnA, CallbackOffA, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffCAndDestroyLifeSpanTarget);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."), // Not removed right now because OffA is called before OffC, then removed inside OffC.
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));
		}

		[Test]
		public void DestroyingLifeSpanTarget_InsideListener_DoesNotAffectOtherListeners_OffCDestroysB()
		{
			CreateLifeSpanTargetTestObject();
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);
			TestSwitch.AddListener(CallbackOnC, CallbackOffCAndDestroyLifeSpanTarget);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."), // Not removed right now because OffB is called before OffC, then removed inside OffC.
			                (LogType.Log, "Called SwitchOff callback C."));

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback C."));
		}

		#endregion

		#region DestroyingDelegateTarget_DoesNotAffectOtherListeners

		[Test]
		public void DestroyingDelegateTarget_DoesNotAffectOtherListeners_Take1()
		{
			CreateTestSwitchSubject();
			TestSwitch.AddListener(TestSwitchSubject.CallbackOnA, TestSwitchSubject.CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			DestroyTestSwitchSubject();

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));
			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void DestroyingDelegateTarget_DoesNotAffectOtherListeners_Take2()
		{
			CreateTestSwitchSubject();
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(TestSwitchSubject.CallbackOnB, TestSwitchSubject.CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			DestroyTestSwitchSubject();

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback C."));
			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void DestroyingDelegateTarget_DoesNotAffectOtherListeners_Take3()
		{
			CreateTestSwitchSubject();
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(TestSwitchSubject.CallbackOnC, TestSwitchSubject.CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			DestroyTestSwitchSubject();

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."));
			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
		}

		#endregion

		#region DestroyingDelegateTarget_InsideListener_DoesNotAffectOtherListeners

		[Test]
		public void DestroyingDelegateTarget_InsideListener_DoesNotAffectOtherListeners_OnADestroysA()
		{
			CreateTestSwitchSubject();
			TestSwitch.AddListener(TestSwitchSubject.CallbackOnAAndDestroySubject, TestSwitchSubject.CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void DestroyingDelegateTarget_InsideListener_DoesNotAffectOtherListeners_OnBDestroysB()
		{
			CreateTestSwitchSubject();
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(TestSwitchSubject.CallbackOnBAndDestroySubject, TestSwitchSubject.CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void DestroyingDelegateTarget_InsideListener_DoesNotAffectOtherListeners_OnCDestroysC()
		{
			CreateTestSwitchSubject();
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(TestSwitchSubject.CallbackOnCAndDestroySubject, TestSwitchSubject.CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
		}

		[Test]
		public void DestroyingDelegateTarget_InsideListener_DoesNotAffectOtherListeners_OffADestroysA()
		{
			CreateTestSwitchSubject();
			TestSwitch.AddListener(TestSwitchSubject.CallbackOnA, TestSwitchSubject.CallbackOffAAndDestroySubject);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void DestroyingDelegateTarget_InsideListener_DoesNotAffectOtherListeners_OffBDestroysB()
		{
			CreateTestSwitchSubject();
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(TestSwitchSubject.CallbackOnB, TestSwitchSubject.CallbackOffBAndDestroySubject);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback C."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void DestroyingDelegateTarget_InsideListener_DoesNotAffectOtherListeners_OffCDestroysC()
		{
			CreateTestSwitchSubject();
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(TestSwitchSubject.CallbackOnC, TestSwitchSubject.CallbackOffCAndDestroySubject);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
		}

		#endregion

		#region DestroyingDelegateTarget_InsideListener_DoesNotAffectOtherListeners - On

		[Test]
		public void DestroyingDelegateTarget_InsideListener_DoesNotAffectOtherListeners_OnADestroysB()
		{
			CreateTestSwitchSubject();
			TestSwitch.AddListener(CallbackOnAAndDestroySubject, CallbackOffA);
			TestSwitch.AddListener(TestSwitchSubject.CallbackOnB, TestSwitchSubject.CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                //(LogType.Log, "Called SwitchOn callback B."), Removed when OnA is called.
			                (LogType.Log, "Called SwitchOn callback C."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void DestroyingDelegateTarget_InsideListener_DoesNotAffectOtherListeners_OnADestroysC()
		{
			CreateTestSwitchSubject();
			TestSwitch.AddListener(CallbackOnAAndDestroySubject, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(TestSwitchSubject.CallbackOnC, TestSwitchSubject.CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."));
			//              (LogType.Log, "Called SwitchOn callback C.")); Removed when OnA is called.

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
		}

		[Test]
		public void DestroyingDelegateTarget_InsideListener_DoesNotAffectOtherListeners_OnBDestroysA()
		{
			CreateTestSwitchSubject();
			TestSwitch.AddListener(TestSwitchSubject.CallbackOnA, TestSwitchSubject.CallbackOffA);
			TestSwitch.AddListener(CallbackOnBAndDestroySubject, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."), // Not removed right now because OnA is called before OnB, then removed inside OnB.
			                (LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void DestroyingDelegateTarget_InsideListener_DoesNotAffectOtherListeners_OnBDestroysC()
		{
			CreateTestSwitchSubject();
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnBAndDestroySubject, CallbackOffB);
			TestSwitch.AddListener(TestSwitchSubject.CallbackOnC, TestSwitchSubject.CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."));
			//              (LogType.Log, "Called SwitchOn callback C.")); Removed when OnB is called.

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
		}

		[Test]
		public void DestroyingDelegateTarget_InsideListener_DoesNotAffectOtherListeners_OnCDestroysA()
		{
			CreateTestSwitchSubject();
			TestSwitch.AddListener(TestSwitchSubject.CallbackOnA, TestSwitchSubject.CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnCAndDestroySubject, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."), // Not removed right now because OnA is called before OnC, then removed inside OnC.
			                (LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		[Test]
		public void DestroyingDelegateTarget_InsideListener_DoesNotAffectOtherListeners_OnCDestroysB()
		{
			CreateTestSwitchSubject();
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(TestSwitchSubject.CallbackOnB, TestSwitchSubject.CallbackOffB);
			TestSwitch.AddListener(CallbackOnCAndDestroySubject, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."), // Not removed right now because OnB is called before OnC, then removed inside OnC.
			                (LogType.Log, "Called SwitchOn callback C."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback C."));
		}

		#endregion

		#region DestroyingDelegateTarget_InsideListener_DoesNotAffectOtherListeners - Off

		[Test]
		public void DestroyingDelegateTarget_InsideListener_DoesNotAffectOtherListeners_OffADestroysB()
		{
			CreateTestSwitchSubject();
			TestSwitch.AddListener(CallbackOnA, CallbackOffAAndDestroySubject);
			Assert.Throws<Exception>(() => TestSwitch.AddListener(TestSwitchSubject.CallbackOnB, TestSwitchSubject.CallbackOffB)); // Because Subject was destroyed above.
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                //(LogType.Log, "Called SwitchOff callback B."), Removed when OffA is called.
			                (LogType.Log, "Called SwitchOff callback C."));

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback C."));
		}

		[Test]
		public void DestroyingDelegateTarget_InsideListener_DoesNotAffectOtherListeners_OffADestroysC()
		{
			CreateTestSwitchSubject();
			TestSwitch.AddListener(CallbackOnA, CallbackOffAAndDestroySubject);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			Assert.Throws<Exception>(() => TestSwitch.AddListener(TestSwitchSubject.CallbackOnC, TestSwitchSubject.CallbackOffC)); // Because Subject was destroyed above.
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
			//              (LogType.Log, "Called SwitchOff callback C.")); Removed when OffA is called.

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."));
		}

		[Test]
		public void DestroyingDelegateTarget_InsideListener_DoesNotAffectOtherListeners_OffBDestroysA()
		{
			CreateTestSwitchSubject();
			TestSwitch.AddListener(TestSwitchSubject.CallbackOnA, TestSwitchSubject.CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffBAndDestroySubject);
			TestSwitch.AddListener(CallbackOnC, CallbackOffC);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."), // Not removed right now because OffA is called before OffB, then removed inside OffB.
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));
		}

		[Test]
		public void DestroyingDelegateTarget_InsideListener_DoesNotAffectOtherListeners_OffBDestroysC()
		{
			CreateTestSwitchSubject();
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffBAndDestroySubject);
			Assert.Throws<Exception>(() => TestSwitch.AddListener(TestSwitchSubject.CallbackOnC, TestSwitchSubject.CallbackOffC)); // Because Subject was destroyed above.
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
			//              (LogType.Log, "Called SwitchOff callback C.")); Removed when OffB is called.

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."));
		}

		[Test]
		public void DestroyingDelegateTarget_InsideListener_DoesNotAffectOtherListeners_OffCDestroysA()
		{
			CreateTestSwitchSubject();
			TestSwitch.AddListener(TestSwitchSubject.CallbackOnA, TestSwitchSubject.CallbackOffA);
			TestSwitch.AddListener(CallbackOnB, CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffCAndDestroySubject);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."), // Not removed right now because OffA is called before OffC, then removed inside OffC.
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));
		}

		[Test]
		public void DestroyingDelegateTarget_InsideListener_DoesNotAffectOtherListeners_OffCDestroysB()
		{
			CreateTestSwitchSubject();
			TestSwitch.AddListener(CallbackOnA, CallbackOffA);
			TestSwitch.AddListener(TestSwitchSubject.CallbackOnB, TestSwitchSubject.CallbackOffB);
			TestSwitch.AddListener(CallbackOnC, CallbackOffCAndDestroySubject);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."), // Not removed right now because OffB is called before OffC, then removed inside OffC.
			                (LogType.Log, "Called SwitchOff callback C."));

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback C."));
		}

		#endregion

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

			SwitchOn();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback with order -40."),
			                (LogType.Log, "Called SwitchOn callback with default order, added first."),
			                (LogType.Log, "Called SwitchOn callback with default order, added second."),
			                (LogType.Log, "Called SwitchOn callback with order 60."));

			SwitchOff();
			AssertExpectLog((LogType.Log, "Called SwitchOff callback with order -40."),
			                (LogType.Log, "Called SwitchOff callback with default order, added first."),
			                (LogType.Log, "Called SwitchOff callback with default order, added second."),
			                (LogType.Log, "Called SwitchOff callback with order 60."));
		}

		#endregion

		#region General

		// @formatter:off
		private void CallbackOn()   { Log.Info("Called SwitchOn callback.");    }
		private void CallbackOnA()  { Log.Info("Called SwitchOn callback A.");  }
		private void CallbackOnB()  { Log.Info("Called SwitchOn callback B.");  }
		private void CallbackOnC()  { Log.Info("Called SwitchOn callback C.");  }
		private void CallbackOnD()  { Log.Info("Called SwitchOn callback D.");  }
		private void CallbackOnE()  { Log.Info("Called SwitchOn callback E.");  }
		private void CallbackOnF()  { Log.Info("Called SwitchOn callback F.");  }
		private void CallbackOff()  { Log.Info("Called SwitchOff callback.");   }
		private void CallbackOffA() { Log.Info("Called SwitchOff callback A."); }
		private void CallbackOffB() { Log.Info("Called SwitchOff callback B."); }
		private void CallbackOffC() { Log.Info("Called SwitchOff callback C."); }
		private void CallbackOffD() { Log.Info("Called SwitchOff callback D."); }
		private void CallbackOffE() { Log.Info("Called SwitchOff callback E."); }
		private void CallbackOffF() { Log.Info("Called SwitchOff callback F."); }
		private void ThrowingCallback() { throw new Test_ExtenityEventException("Called throwing callback."); }

		private void CallbackOnAndRemove()     { CallbackOn(); TestSwitch.RemoveListener(CallbackOnAndRemove, CallbackOff); }
		private void CallbackOnAndRemoveSelf() { CallbackOn(); TestSwitch.RemoveCurrentListener(); }

		private void CallbackOnAAndRemoveSelf()  { CallbackOnA();  TestSwitch.RemoveCurrentListener(); }
		private void CallbackOnBAndRemoveSelf()  { CallbackOnB();  TestSwitch.RemoveCurrentListener(); }
		private void CallbackOnCAndRemoveSelf()  { CallbackOnC();  TestSwitch.RemoveCurrentListener(); }
		private void CallbackOffAAndRemoveSelf() { CallbackOffA(); TestSwitch.RemoveCurrentListener(); }
		private void CallbackOffBAndRemoveSelf() { CallbackOffB(); TestSwitch.RemoveCurrentListener(); }
		private void CallbackOffCAndRemoveSelf() { CallbackOffC(); TestSwitch.RemoveCurrentListener(); }

		private void CallbackOnAAndRemoveA()  { CallbackOnA();  TestSwitch.RemoveListener(CallbackOnAAndRemoveA, CallbackOffA); }
		private void CallbackOnBAndRemoveB()  { CallbackOnB();  TestSwitch.RemoveListener(CallbackOnBAndRemoveB, CallbackOffB); }
		private void CallbackOnCAndRemoveC()  { CallbackOnC();  TestSwitch.RemoveListener(CallbackOnCAndRemoveC, CallbackOffC); }
		private void CallbackOffAAndRemoveA() { CallbackOffA(); TestSwitch.RemoveListener(CallbackOnA, CallbackOffAAndRemoveA); }
		private void CallbackOffBAndRemoveB() { CallbackOffB(); TestSwitch.RemoveListener(CallbackOnB, CallbackOffBAndRemoveB); }
		private void CallbackOffCAndRemoveC() { CallbackOffC(); TestSwitch.RemoveListener(CallbackOnC, CallbackOffCAndRemoveC); }

		private void CallbackOnAAndRemoveB()  { CallbackOnA();  TestSwitch.RemoveListener(CallbackOnB, CallbackOffB); }
		private void CallbackOnAAndRemoveC()  { CallbackOnA();  TestSwitch.RemoveListener(CallbackOnC, CallbackOffC); }
		private void CallbackOnBAndRemoveA()  { CallbackOnB();  TestSwitch.RemoveListener(CallbackOnA, CallbackOffA); }
		private void CallbackOnBAndRemoveC()  { CallbackOnB();  TestSwitch.RemoveListener(CallbackOnC, CallbackOffC); }
		private void CallbackOnCAndRemoveA()  { CallbackOnC();  TestSwitch.RemoveListener(CallbackOnA, CallbackOffA); }
		private void CallbackOnCAndRemoveB()  { CallbackOnC();  TestSwitch.RemoveListener(CallbackOnB, CallbackOffB); }
		private void CallbackOffAAndRemoveB() { CallbackOffA(); TestSwitch.RemoveListener(CallbackOnB, CallbackOffB); }
		private void CallbackOffAAndRemoveC() { CallbackOffA(); TestSwitch.RemoveListener(CallbackOnC, CallbackOffC); }
		private void CallbackOffBAndRemoveA() { CallbackOffB(); TestSwitch.RemoveListener(CallbackOnA, CallbackOffA); }
		private void CallbackOffBAndRemoveC() { CallbackOffB(); TestSwitch.RemoveListener(CallbackOnC, CallbackOffC); }
		private void CallbackOffCAndRemoveA() { CallbackOffC(); TestSwitch.RemoveListener(CallbackOnA, CallbackOffA); }
		private void CallbackOffCAndRemoveB() { CallbackOffC(); TestSwitch.RemoveListener(CallbackOnB, CallbackOffB); }

		private void CallbackOnAAndDestroyLifeSpanTarget()  { CallbackOnA();  DestroyLifeSpanTargetTestObject(); }
		private void CallbackOnBAndDestroyLifeSpanTarget()  { CallbackOnB();  DestroyLifeSpanTargetTestObject(); }
		private void CallbackOnCAndDestroyLifeSpanTarget()  { CallbackOnC();  DestroyLifeSpanTargetTestObject(); }
		private void CallbackOffAAndDestroyLifeSpanTarget() { CallbackOffA(); DestroyLifeSpanTargetTestObject(); }
		private void CallbackOffBAndDestroyLifeSpanTarget() { CallbackOffB(); DestroyLifeSpanTargetTestObject(); }
		private void CallbackOffCAndDestroyLifeSpanTarget() { CallbackOffC(); DestroyLifeSpanTargetTestObject(); }

		private void CallbackOnAAndDestroySubject()  { CallbackOnA();  DestroyTestSwitchSubject(); }
		private void CallbackOnBAndDestroySubject()  { CallbackOnB();  DestroyTestSwitchSubject(); }
		private void CallbackOnCAndDestroySubject()  { CallbackOnC();  DestroyTestSwitchSubject(); }
		private void CallbackOffAAndDestroySubject() { CallbackOffA(); DestroyTestSwitchSubject(); }
		private void CallbackOffBAndDestroySubject() { CallbackOffB(); DestroyTestSwitchSubject(); }
		private void CallbackOffCAndDestroySubject() { CallbackOffC(); DestroyTestSwitchSubject(); }
		// @formatter:on

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
			AssertExpectLog((LogType.Log, "Called SwitchOff callback."));
			AssertRegisteredCallbackCount(1);
		}

		private void AssertRegisteredCallbackCount(int expectedCount)
		{
			Assert.AreEqual(expectedCount, TestSwitch.ListenersAliveCount, "Unexpected registered callback count.");
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(Test_ExtenitySwitch));

		#endregion
	}

}
