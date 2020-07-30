using System;
using Extenity.MessagingToolbox;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ExtenityTests.MessagingToolbox
{

	[TestFixture(true)]
	[TestFixture(false)]
	public class Test_ExtenityEvent : Test_EventTestBase
	{
		#region Initialization

		public Test_ExtenityEvent(bool usingUnsafe) : base(usingUnsafe) { }

		#endregion

		#region Basics

		[Test]
		public void AlrightToInvokeWithoutListeners()
		{
			Invoke();
		}

		#endregion

		#region Invoking

		[Test]
		public void CallbackIsNotInstantlyInvoked()
		{
			TestEvent.AddListener(Callback);
			AssertExpectNoLogs();
		}

		[Test]
		public void InvokeNonUnityObjectCallback()
		{
			TestEvent.AddListener(Callback);
			Assert.That(new Action(Callback).Target as Object, Is.Null);

			Invoke();
			AssertExpectLog((LogType.Log, "Called callback."));
		}

		[Test]
		public void InvokeUnityObjectCallback()
		{
			CreateTestEventSubject();
			TestEvent.AddListener(TestEventSubject.Callback);
			Assert.That(new Action(TestEventSubject.Callback).Target as Object, Is.Not.Null);

			Invoke();
			AssertExpectLog((LogType.Log, "Called callback."));
		}

		[Test]
		public void InvokingWithMultipleListeners_Orderless()
		{
			TestEvent.AddListener(CallbackA);
			TestEvent.AddListener(CallbackB);
			TestEvent.AddListener(CallbackC);

			Invoke();
			AssertExpectLog((LogType.Log, "Called callback A."),
			                (LogType.Log, "Called callback B."),
			                (LogType.Log, "Called callback C."));
		}

		[Test]
		public void InvokingWithMultipleListeners_NegativeOrder()
		{
			TestEvent.AddListener(CallbackA, -10);
			TestEvent.AddListener(CallbackB, -20);
			TestEvent.AddListener(CallbackC, -30);

			Invoke();
			AssertExpectLog((LogType.Log, "Called callback C."),
			                (LogType.Log, "Called callback B."),
			                (LogType.Log, "Called callback A."));
		}


		[Test]
		public void InvokingWithMultipleListeners_MixedOrders()
		{
			TestEvent.AddListener(CallbackA, -10);
			TestEvent.AddListener(CallbackB, -10);
			TestEvent.AddListener(CallbackC, 0);
			TestEvent.AddListener(CallbackD, 0);
			TestEvent.AddListener(CallbackE, 30);
			TestEvent.AddListener(CallbackF, 30);

			Invoke();
			AssertExpectLog((LogType.Log, "Called callback A."),
			                (LogType.Log, "Called callback B."),
			                (LogType.Log, "Called callback C."),
			                (LogType.Log, "Called callback D."),
			                (LogType.Log, "Called callback E."),
			                (LogType.Log, "Called callback F."));
		}

		// Not cool to call Safe or Unsafe exclusively since there are text fixture parameters for that, but whatever.
		[Test]
		public void InvokingSafeIsNotAffectedByExceptions()
		{
			TestEvent.AddListener(CallbackA, 10);
			TestEvent.AddListener(ThrowingCallback, 20);
			TestEvent.AddListener(CallbackC, 30);

			TestEvent.InvokeSafe();
			AssertExpectLog((LogType.Log, "Called callback A."),
			                (LogType.Exception, "Test_ExtenityEventException: Called throwing callback."),
			                (LogType.Log, "Called callback C."));
		}

		// Not cool to call Safe or Unsafe exclusively since there are text fixture parameters for that, but whatever.
		[Test]
		public void InvokingUnsafeDoesNotCatchExceptions()
		{
			TestEvent.AddListener(CallbackA, 10);
			TestEvent.AddListener(ThrowingCallback, 20);
			TestEvent.AddListener(CallbackC, 30);

			Assert.Throws<Test_ExtenityEventException>(() => TestEvent.InvokeUnsafe());
			AssertExpectLog((LogType.Log, "Called callback A."));
		}

		[Test]
		public void InvokingContinuously()
		{
			RegisterCallbacks();

			for (int i = 0; i < 10; i++)
			{
				Invoke();
				AssertExpectLog((LogType.Log, "Called callback."));
			}
		}

		[Test]
		public void AddingListenersMoreThanOnceIsIgnored()
		{
			TestEvent.AddListener(Callback);
			TestEvent.AddListener(Callback);
			TestEvent.AddListener(Callback);
			AssertExpectNoLogs();

			// Note that the callback is called only once.
			Invoke();
			AssertExpectLog((LogType.Log, "Called callback."));

			TestEvent.AddListener(Callback);
			TestEvent.AddListener(Callback);
			TestEvent.AddListener(Callback);
			AssertExpectNoLogs();
		}

		[Test]
		public void AlrightNotToHaveCallback_WhichIsSilentlyIgnored()
		{
			TestEvent.AddListener(null);
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
		public void NestedEventsAreNotAllowed()
		{
			TestEvent.AddListener(Invoke);
			Invoke();
			AssertExpectLog((LogType.Exception, "Exception: Invoked event while an invocation is ongoing."));
		}

		// Not cool to call Safe or Unsafe exclusively since there are text fixture parameters for that, but whatever.
		[Test]
		public void NestedAddListenerIsNotAllowed_Safe()
		{
			TestEvent.AddListener(() => TestEvent.AddListener(Callback));
			TestEvent.InvokeSafe();
			AssertExpectLog((LogType.Exception, "NotSupportedException: Adding listener while invoking is not supported."));
		}

		// Not cool to call Safe or Unsafe exclusively since there are text fixture parameters for that, but whatever.
		[Test]
		public void NestedAddListenerIsNotAllowed_Unsafe()
		{
			TestEvent.AddListener(() => TestEvent.AddListener(Callback));
			Assert.Throws<NotSupportedException>(() => TestEvent.InvokeUnsafe());
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
				TestEvent.AddListener(CallbackThatAddsSelf, 0, ListenerLifeSpan.Permanent);
			}

			TestEvent.AddListener(CallbackThatAddsSelf, 0, ListenerLifeSpan.Permanent);
			Invoke();
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
					throw new Test_ExtenityEventException("Hard brakes!"); // Enough

				// The callback is deregistered at this point due to RemovedAtFirstEmit. Then we try to add it one more
				// time. But note that the system calls the callback immediately, which causes dead lock.
				TestEvent.AddListener(CallbackThatAddsSelf, 0, ListenerLifeSpan.RemovedAtFirstEmit);
			}

			TestEvent.AddListener(CallbackThatAddsSelf, 0, ListenerLifeSpan.RemovedAtFirstEmit);
			Invoke();
			AssertExpectLog((LogType.Log, "Called 1"),
			                (LogType.Log, "Called 2"),
			                (LogType.Log, "Called 3"),
			                (LogType.Exception, "Test_ExtenityEventException: Hard brakes!"));
		}

#endif

		#endregion

		#region Callback Life Span

		[Test]
		public void EndsAfterRemovingListener()
		{
			TestEvent.AddListener(Callback);
			TestEvent.RemoveListener(Callback);

			Invoke();
			AssertExpectNoLogs();
		}

		[Test]
		public void AlrightToRemoveListenerInsideCallback_ManualRemove()
		{
			TestEvent.AddListener(CallbackAndRemove);

			// Callback removes itself.
			Invoke();
			AssertExpectLog((LogType.Log, "Called callback."));

			// No more calls.
			Invoke();
			AssertExpectNoLogs();
		}

		[Test]
		public void AlrightToRemoveListenerInsideCallback_UsingRemoveCurrentListener()
		{
			TestEvent.AddListener(CallbackAndRemoveSelf);

			// Callback removes itself.
			Invoke();
			AssertExpectLog((LogType.Log, "Called callback."));

			// No more calls.
			Invoke();
			AssertExpectNoLogs();
		}

		[Test]
		public void NotAlrightToCallRemoveCurrentListenerOutsideOfCallback()
		{
			TestEvent.AddListener(Callback);

			Assert.Throws<Exception>(() => TestEvent.RemoveCurrentListener());
		}

		#region RemovingListener_DoesNotAffectOtherListeners

		[Test]
		public void RemovingListener_DoesNotAffectOtherListeners_Take1()
		{
			TestEvent.AddListener(CallbackA);
			TestEvent.AddListener(CallbackB);
			TestEvent.AddListener(CallbackC);

			TestEvent.RemoveListener(CallbackA);

			Invoke();
			AssertExpectLog((LogType.Log, "Called callback B."),
			                (LogType.Log, "Called callback C."));
			Invoke();
			AssertExpectLog((LogType.Log, "Called callback B."),
			                (LogType.Log, "Called callback C."));
		}

		[Test]
		public void RemovingListener_DoesNotAffectOtherListeners_Take2()
		{
			TestEvent.AddListener(CallbackA);
			TestEvent.AddListener(CallbackB);
			TestEvent.AddListener(CallbackC);

			TestEvent.RemoveListener(CallbackB);

			Invoke();
			AssertExpectLog((LogType.Log, "Called callback A."),
			                (LogType.Log, "Called callback C."));
			Invoke();
			AssertExpectLog((LogType.Log, "Called callback A."),
			                (LogType.Log, "Called callback C."));
		}

		[Test]
		public void RemovingListener_DoesNotAffectOtherListeners_Take3()
		{
			TestEvent.AddListener(CallbackA);
			TestEvent.AddListener(CallbackB);
			TestEvent.AddListener(CallbackC);

			TestEvent.RemoveListener(CallbackC);

			Invoke();
			AssertExpectLog((LogType.Log, "Called callback A."),
			                (LogType.Log, "Called callback B."));
			Invoke();
			AssertExpectLog((LogType.Log, "Called callback A."),
			                (LogType.Log, "Called callback B."));
		}

		[Test]
		public void RemovingListener_DoesNotAffectOtherListeners_Take4()
		{
			TestEvent.AddListener(CallbackA);
			TestEvent.AddListener(CallbackB);
			TestEvent.AddListener(CallbackC);

			TestEvent.RemoveListener(CallbackB);

			Invoke();
			AssertExpectLog((LogType.Log, "Called callback A."),
			                (LogType.Log, "Called callback C."));

			TestEvent.RemoveListener(CallbackA);

			Invoke();
			AssertExpectLog((LogType.Log, "Called callback C."));

			TestEvent.RemoveListener(CallbackC);

			Invoke();
			AssertExpectNoLogs();
		}

		#endregion

		#region RemovingListener_InsideListener_DoesNotAffectOtherListeners_ManualRemove

		[Test]
		public void RemovingListener_InsideListener_DoesNotAffectOtherListeners_ManualRemove_ARemovesA()
		{
			TestEvent.AddListener(CallbackAAndRemoveA);
			TestEvent.AddListener(CallbackB);
			TestEvent.AddListener(CallbackC);

			// This is where it's removed.
			Invoke();
			AssertExpectLog((LogType.Log, "Called callback A."),
			                (LogType.Log, "Called callback B."),
			                (LogType.Log, "Called callback C."));

			Invoke();
			AssertExpectLog((LogType.Log, "Called callback B."),
			                (LogType.Log, "Called callback C."));
		}

		[Test]
		public void RemovingListener_InsideListener_DoesNotAffectOtherListeners_ManualRemove_BRemovesB()
		{
			TestEvent.AddListener(CallbackA);
			TestEvent.AddListener(CallbackBAndRemoveB);
			TestEvent.AddListener(CallbackC);

			// This is where it's removed.
			Invoke();
			AssertExpectLog((LogType.Log, "Called callback A."),
			                (LogType.Log, "Called callback B."),
			                (LogType.Log, "Called callback C."));

			Invoke();
			AssertExpectLog((LogType.Log, "Called callback A."),
			                (LogType.Log, "Called callback C."));
		}

		[Test]
		public void RemovingListener_InsideListener_DoesNotAffectOtherListeners_ManualRemove_CRemovesC()
		{
			TestEvent.AddListener(CallbackA);
			TestEvent.AddListener(CallbackB);
			TestEvent.AddListener(CallbackCAndRemoveC);

			// This is where it's removed.
			Invoke();
			AssertExpectLog((LogType.Log, "Called callback A."),
			                (LogType.Log, "Called callback B."),
			                (LogType.Log, "Called callback C."));

			Invoke();
			AssertExpectLog((LogType.Log, "Called callback A."),
			                (LogType.Log, "Called callback B."));
		}

		#endregion

		#region RemovingListener_InsideListener_DoesNotAffectOtherListeners_UsingRemoveCurrentListener

		[Test]
		public void RemovingListener_InsideListener_DoesNotAffectOtherListeners_UsingRemoveCurrentListener_ARemovesSelf()
		{
			TestEvent.AddListener(CallbackAAndRemoveSelf);
			TestEvent.AddListener(CallbackB);
			TestEvent.AddListener(CallbackC);

			// This is where it's removed.
			Invoke();
			AssertExpectLog((LogType.Log, "Called callback A."),
			                (LogType.Log, "Called callback B."),
			                (LogType.Log, "Called callback C."));

			Invoke();
			AssertExpectLog((LogType.Log, "Called callback B."),
			                (LogType.Log, "Called callback C."));
		}

		[Test]
		public void RemovingListener_InsideListener_DoesNotAffectOtherListeners_UsingRemoveCurrentListener_BRemovesSelf()
		{
			TestEvent.AddListener(CallbackA);
			TestEvent.AddListener(CallbackBAndRemoveSelf);
			TestEvent.AddListener(CallbackC);

			// This is where it's removed.
			Invoke();
			AssertExpectLog((LogType.Log, "Called callback A."),
			                (LogType.Log, "Called callback B."),
			                (LogType.Log, "Called callback C."));

			Invoke();
			AssertExpectLog((LogType.Log, "Called callback A."),
			                (LogType.Log, "Called callback C."));
		}

		[Test]
		public void RemovingListener_InsideListener_DoesNotAffectOtherListeners_UsingRemoveCurrentListener_CRemovesSelf()
		{
			TestEvent.AddListener(CallbackA);
			TestEvent.AddListener(CallbackB);
			TestEvent.AddListener(CallbackCAndRemoveSelf);

			// This is where it's removed.
			Invoke();
			AssertExpectLog((LogType.Log, "Called callback A."),
			                (LogType.Log, "Called callback B."),
			                (LogType.Log, "Called callback C."));

			Invoke();
			AssertExpectLog((LogType.Log, "Called callback A."),
			                (LogType.Log, "Called callback B."));
		}

		#endregion

		#region RemovingAnotherListener_InsideListener_DoesNotAffectOtherListeners

		[Test]
		public void RemovingAnotherListener_InsideListener_DoesNotAffectOtherListeners_ARemovesB()
		{
			TestEvent.AddListener(CallbackAAndRemoveB);
			TestEvent.AddListener(CallbackB);
			TestEvent.AddListener(CallbackC);

			// This is where it's removed.
			Invoke();
			AssertExpectLog((LogType.Log, "Called callback A."),
			//              (LogType.Log, "Called callback B."), Removed when OnA is called.
			                (LogType.Log, "Called callback C."));

			Invoke();
			AssertExpectLog((LogType.Log, "Called callback A."),
			                (LogType.Log, "Called callback C."));
		}

		[Test]
		public void RemovingAnotherListener_InsideListener_DoesNotAffectOtherListeners_ARemovesC()
		{
			TestEvent.AddListener(CallbackAAndRemoveC);
			TestEvent.AddListener(CallbackB);
			TestEvent.AddListener(CallbackC);

			// This is where it's removed.
			Invoke();
			AssertExpectLog((LogType.Log, "Called callback A."),
			                (LogType.Log, "Called callback B."));
			//              (LogType.Log, "Called callback C.")); Removed when OnA is called.

			Invoke();
			AssertExpectLog((LogType.Log, "Called callback A."),
			                (LogType.Log, "Called callback B."));
		}

		[Test]
		public void RemovingAnotherListener_InsideListener_DoesNotAffectOtherListeners_BRemovesA()
		{
			TestEvent.AddListener(CallbackA);
			TestEvent.AddListener(CallbackBAndRemoveA);
			TestEvent.AddListener(CallbackC);

			// This is where it's removed.
			Invoke();
			AssertExpectLog((LogType.Log, "Called callback A."), // Not removed right now because OnA is called before OnB, then removed inside OnB.
			                (LogType.Log, "Called callback B."),
			                (LogType.Log, "Called callback C."));

			Invoke();
			AssertExpectLog((LogType.Log, "Called callback B."),
			                (LogType.Log, "Called callback C."));
		}

		[Test]
		public void RemovingAnotherListener_InsideListener_DoesNotAffectOtherListeners_BRemovesC()
		{
			TestEvent.AddListener(CallbackA);
			TestEvent.AddListener(CallbackBAndRemoveC);
			TestEvent.AddListener(CallbackC);

			// This is where it's removed.
			Invoke();
			AssertExpectLog((LogType.Log, "Called callback A."),
			                (LogType.Log, "Called callback B."));
			//              (LogType.Log, "Called callback C.")); Removed when OnB is called.

			Invoke();
			AssertExpectLog((LogType.Log, "Called callback A."),
			                (LogType.Log, "Called callback B."));
		}

		[Test]
		public void RemovingAnotherListener_InsideListener_DoesNotAffectOtherListeners_CRemovesA()
		{
			TestEvent.AddListener(CallbackA);
			TestEvent.AddListener(CallbackB);
			TestEvent.AddListener(CallbackCAndRemoveA);

			// This is where it's removed.
			Invoke();
			AssertExpectLog((LogType.Log, "Called callback A."), // Not removed right now because OnA is called before OnC, then removed inside OnC.
			                (LogType.Log, "Called callback B."),
			                (LogType.Log, "Called callback C."));

			Invoke();
			AssertExpectLog((LogType.Log, "Called callback B."),
			                (LogType.Log, "Called callback C."));
		}

		[Test]
		public void RemovingAnotherListener_InsideListener_DoesNotAffectOtherListeners_CRemovesB()
		{
			TestEvent.AddListener(CallbackA);
			TestEvent.AddListener(CallbackB);
			TestEvent.AddListener(CallbackCAndRemoveB);

			// This is where it's removed.
			Invoke();
			AssertExpectLog((LogType.Log, "Called callback A."),
			                (LogType.Log, "Called callback B."), // Not removed right now because OnB is called before OnC, then removed inside OnC.
			                (LogType.Log, "Called callback C."));

			Invoke();
			AssertExpectLog((LogType.Log, "Called callback A."),
			                (LogType.Log, "Called callback C."));
		}

		#endregion

		[Test]
		public void LifeSpan_Permanent()
		{
			RegisterCallbacks(0, ListenerLifeSpan.Permanent);

			Invoke();
			AssertExpectLog((LogType.Log, "Called callback."));
			Invoke();
			AssertExpectLog((LogType.Log, "Called callback."));
			Invoke();
			AssertExpectLog((LogType.Log, "Called callback."));

			// Manually removing is the only way. (or there is that LifeSpanTarget feature too)
			TestEvent.RemoveListener(Callback);
			AssertRegisteredCallbackCount(0);

			Invoke();
			Invoke();
			AssertExpectNoLogs();
		}

		[Test]
		public void LifeSpan_Permanent_WithLifeSpanTargetDestroyedLater()
		{
			CreateLifeSpanTargetTestObject();
			RegisterCallbacks(0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);

			Invoke();
			AssertExpectLog((LogType.Log, "Called callback."));
			Invoke();
			AssertExpectLog((LogType.Log, "Called callback."));
			Invoke();
			AssertExpectLog((LogType.Log, "Called callback."));

			// Destroy the LifeSpanTarget and the registered listener will not be called anymore.
			DestroyLifeSpanTargetTestObject();
			AssertRegisteredCallbackCount(0);

			Invoke();
			Invoke();
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

			Invoke();
			Invoke();
			AssertExpectNoLogs();
		}

		[Test]
		public void LifeSpan_Permanent_WithDelegateTargetDestroyedLater()
		{
			RegisterSubjectCallbacks(0, ListenerLifeSpan.Permanent);

			Invoke();
			AssertExpectLog((LogType.Log, "Called callback."));
			Invoke();
			AssertExpectLog((LogType.Log, "Called callback."));
			Invoke();
			AssertExpectLog((LogType.Log, "Called callback."));

			// Destroy the Subject and the registered listener will not be called anymore.
			DestroyTestEventSubject();
			AssertRegisteredCallbackCount(0);

			Invoke();
			Invoke();
			AssertExpectNoLogs();
		}

		[Test]
		public void LifeSpan_Permanent_WithDelegateTargetDestroyedFirst()
		{
			RegisterSubjectCallbacks(0, ListenerLifeSpan.Permanent);

			// Destroy the Subject and the registered listener will not be called anymore.
			DestroyTestEventSubject();
			AssertRegisteredCallbackCount(0);

			Invoke();
			Invoke();
			AssertExpectNoLogs();
		}

		[Test]
		public void LifeSpan_RemovedAtFirstEmit()
		{
			TestEvent.AddListener(Callback, 0, ListenerLifeSpan.RemovedAtFirstEmit);

			// The callback will be deregistered after this.
			Invoke();
			AssertExpectLog((LogType.Log, "Called callback."));
			AssertRegisteredCallbackCount(0);

			Invoke();
			Invoke();
			AssertExpectNoLogs();
		}

		[Test]
		public void LifeSpan_RemovedAtFirstEmit_WithLifeSpanTargetDestroyedLater()
		{
			CreateLifeSpanTargetTestObject();
			RegisterCallbacks(0, ListenerLifeSpan.RemovedAtFirstEmit, LifeSpanTargetTestObject);

			// The callback will be deregistered after this.
			Invoke();
			AssertExpectLog((LogType.Log, "Called callback."));
			AssertRegisteredCallbackCount(0);

			Invoke();
			AssertExpectNoLogs();

			// Destroying the LifeSpanTarget does nothing after that. The listener was already deregistered, thanks to RemovedAtFirstEmit.
			DestroyLifeSpanTargetTestObject();
			Invoke();
			Invoke();
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

			Invoke();
			Invoke();
			AssertExpectNoLogs();
		}

		[Test]
		public void LifeSpan_RemovedAtFirstEmit_WithDelegateTargetDestroyedLater()
		{
			RegisterSubjectCallbacks(0, ListenerLifeSpan.RemovedAtFirstEmit);

			// The callback will be deregistered after this.
			Invoke();
			AssertExpectLog((LogType.Log, "Called callback."));
			AssertRegisteredCallbackCount(0);

			Invoke();
			AssertExpectNoLogs();

			// Destroying the Subject does nothing after that. The listener was already deregistered, thanks to RemovedAtFirstEmit.
			DestroyTestEventSubject();
			Invoke();
			Invoke();
			AssertExpectNoLogs();
		}

		[Test]
		public void LifeSpan_RemovedAtFirstEmit_WithDelegateTargetDestroyedFirst()
		{
			RegisterSubjectCallbacks(0, ListenerLifeSpan.RemovedAtFirstEmit);

			// The callback will be deregistered after this.
			DestroyTestEventSubject();
			AssertRegisteredCallbackCount(0);

			Invoke();
			Invoke();
			AssertExpectNoLogs();
		}

		#endregion
		/*

		#region LifeSpan_RemovedAtFirstEmit_DoesNotAffectOtherListeners

		[Test]
		public void LifeSpan_RemovedAtFirstEmit_DoesNotAffectOtherListeners_OnA()
		{
			TestEvent.AddListener(CallbackOnA, CallbackOffA, 0, ListenerLifeSpan.RemovedAtFirstEmit);
			TestEvent.AddListener(CallbackOnB, CallbackOffB, 0, ListenerLifeSpan.Permanent);
			TestEvent.AddListener(CallbackOnC, CallbackOffC, 0, ListenerLifeSpan.Permanent);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			Invoke();
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
			TestEvent.AddListener(CallbackOnA, CallbackOffA, 0, ListenerLifeSpan.Permanent);
			TestEvent.AddListener(CallbackOnB, CallbackOffB, 0, ListenerLifeSpan.RemovedAtFirstEmit);
			TestEvent.AddListener(CallbackOnC, CallbackOffC, 0, ListenerLifeSpan.Permanent);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			Invoke();
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
			TestEvent.AddListener(CallbackOnA, CallbackOffA, 0, ListenerLifeSpan.Permanent);
			TestEvent.AddListener(CallbackOnB, CallbackOffB, 0, ListenerLifeSpan.Permanent);
			TestEvent.AddListener(CallbackOnC, CallbackOffC, 0, ListenerLifeSpan.RemovedAtFirstEmit);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			Invoke();
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
			TestEvent.AddListener(CallbackOnA, CallbackOffA, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);
			TestEvent.AddListener(CallbackOnB, CallbackOffB);
			TestEvent.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			DestroyLifeSpanTargetTestObject();

			Invoke();
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
			TestEvent.AddListener(CallbackOnA, CallbackOffA);
			TestEvent.AddListener(CallbackOnB, CallbackOffB, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);
			TestEvent.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			DestroyLifeSpanTargetTestObject();

			Invoke();
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
			TestEvent.AddListener(CallbackOnA, CallbackOffA);
			TestEvent.AddListener(CallbackOnB, CallbackOffB);
			TestEvent.AddListener(CallbackOnC, CallbackOffC, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			DestroyLifeSpanTargetTestObject();

			Invoke();
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
			TestEvent.AddListener(CallbackOnAAndDestroyLifeSpanTarget, CallbackOffA, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);
			TestEvent.AddListener(CallbackOnB, CallbackOffB);
			TestEvent.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			Invoke();
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
			TestEvent.AddListener(CallbackOnA, CallbackOffA);
			TestEvent.AddListener(CallbackOnBAndDestroyLifeSpanTarget, CallbackOffB, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);
			TestEvent.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			Invoke();
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
			TestEvent.AddListener(CallbackOnA, CallbackOffA);
			TestEvent.AddListener(CallbackOnB, CallbackOffB);
			TestEvent.AddListener(CallbackOnCAndDestroyLifeSpanTarget, CallbackOffC, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			Invoke();
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
			TestEvent.AddListener(CallbackOnA, CallbackOffAAndDestroyLifeSpanTarget, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);
			TestEvent.AddListener(CallbackOnB, CallbackOffB);
			TestEvent.AddListener(CallbackOnC, CallbackOffC);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			Invoke();
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
			TestEvent.AddListener(CallbackOnA, CallbackOffA);
			TestEvent.AddListener(CallbackOnB, CallbackOffBAndDestroyLifeSpanTarget, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);
			TestEvent.AddListener(CallbackOnC, CallbackOffC);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			Invoke();
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
			TestEvent.AddListener(CallbackOnA, CallbackOffA);
			TestEvent.AddListener(CallbackOnB, CallbackOffB);
			TestEvent.AddListener(CallbackOnC, CallbackOffCAndDestroyLifeSpanTarget, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			Invoke();
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
			TestEvent.AddListener(CallbackOnAAndDestroyLifeSpanTarget, CallbackOffA);
			TestEvent.AddListener(CallbackOnB, CallbackOffB, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);
			TestEvent.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			Invoke();
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
			TestEvent.AddListener(CallbackOnAAndDestroyLifeSpanTarget, CallbackOffA);
			TestEvent.AddListener(CallbackOnB, CallbackOffB);
			TestEvent.AddListener(CallbackOnC, CallbackOffC, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			Invoke();
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
			TestEvent.AddListener(CallbackOnA, CallbackOffA, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);
			TestEvent.AddListener(CallbackOnBAndDestroyLifeSpanTarget, CallbackOffB);
			TestEvent.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			Invoke();
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
			TestEvent.AddListener(CallbackOnA, CallbackOffA);
			TestEvent.AddListener(CallbackOnBAndDestroyLifeSpanTarget, CallbackOffB);
			TestEvent.AddListener(CallbackOnC, CallbackOffC, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			Invoke();
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
			TestEvent.AddListener(CallbackOnA, CallbackOffA, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);
			TestEvent.AddListener(CallbackOnB, CallbackOffB);
			TestEvent.AddListener(CallbackOnCAndDestroyLifeSpanTarget, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			Invoke();
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
			TestEvent.AddListener(CallbackOnA, CallbackOffA);
			TestEvent.AddListener(CallbackOnB, CallbackOffB, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);
			TestEvent.AddListener(CallbackOnCAndDestroyLifeSpanTarget, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			Invoke();
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
			TestEvent.AddListener(CallbackOnA, CallbackOffAAndDestroyLifeSpanTarget);
			Assert.Throws<Exception>(() => TestEvent.AddListener(CallbackOnB, CallbackOffB, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject)); // Because LifeSpanTarget was destroyed above.
			TestEvent.AddListener(CallbackOnC, CallbackOffC);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                //(LogType.Log, "Called SwitchOff callback B."), Removed when OffA is called.
			                (LogType.Log, "Called SwitchOff callback C."));

			Invoke();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback C."));
		}

		[Test]
		public void DestroyingLifeSpanTarget_InsideListener_DoesNotAffectOtherListeners_OffADestroysC()
		{
			CreateLifeSpanTargetTestObject();
			TestEvent.AddListener(CallbackOnA, CallbackOffAAndDestroyLifeSpanTarget);
			TestEvent.AddListener(CallbackOnB, CallbackOffB);
			Assert.Throws<Exception>(() => TestEvent.AddListener(CallbackOnC, CallbackOffC, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject)); // Because LifeSpanTarget was destroyed above.
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
			//              (LogType.Log, "Called SwitchOff callback C.")); Removed when OffA is called.

			Invoke();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."));
		}

		[Test]
		public void DestroyingLifeSpanTarget_InsideListener_DoesNotAffectOtherListeners_OffBDestroysA()
		{
			CreateLifeSpanTargetTestObject();
			TestEvent.AddListener(CallbackOnA, CallbackOffA, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);
			TestEvent.AddListener(CallbackOnB, CallbackOffBAndDestroyLifeSpanTarget);
			TestEvent.AddListener(CallbackOnC, CallbackOffC);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."), // Not removed right now because OffA is called before OffB, then removed inside OffB.
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			Invoke();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));
		}

		[Test]
		public void DestroyingLifeSpanTarget_InsideListener_DoesNotAffectOtherListeners_OffBDestroysC()
		{
			CreateLifeSpanTargetTestObject();
			TestEvent.AddListener(CallbackOnA, CallbackOffA);
			TestEvent.AddListener(CallbackOnB, CallbackOffBAndDestroyLifeSpanTarget);
			Assert.Throws<Exception>(() => TestEvent.AddListener(CallbackOnC, CallbackOffC, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject)); // Because LifeSpanTarget was destroyed above.
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
			//              (LogType.Log, "Called SwitchOff callback C.")); Removed when OffB is called.

			Invoke();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."));
		}

		[Test]
		public void DestroyingLifeSpanTarget_InsideListener_DoesNotAffectOtherListeners_OffCDestroysA()
		{
			CreateLifeSpanTargetTestObject();
			TestEvent.AddListener(CallbackOnA, CallbackOffA, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);
			TestEvent.AddListener(CallbackOnB, CallbackOffB);
			TestEvent.AddListener(CallbackOnC, CallbackOffCAndDestroyLifeSpanTarget);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."), // Not removed right now because OffA is called before OffC, then removed inside OffC.
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			Invoke();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));
		}

		[Test]
		public void DestroyingLifeSpanTarget_InsideListener_DoesNotAffectOtherListeners_OffCDestroysB()
		{
			CreateLifeSpanTargetTestObject();
			TestEvent.AddListener(CallbackOnA, CallbackOffA);
			TestEvent.AddListener(CallbackOnB, CallbackOffB, 0, ListenerLifeSpan.Permanent, LifeSpanTargetTestObject);
			TestEvent.AddListener(CallbackOnC, CallbackOffCAndDestroyLifeSpanTarget);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."), // Not removed right now because OffB is called before OffC, then removed inside OffC.
			                (LogType.Log, "Called SwitchOff callback C."));

			Invoke();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback C."));
		}

		#endregion

		#region DestroyingDelegateTarget_DoesNotAffectOtherListeners

		[Test]
		public void DestroyingDelegateTarget_DoesNotAffectOtherListeners_Take1()
		{
			CreateTestSwitchSubject();
			TestEvent.AddListener(TestSwitchSubject.CallbackOnA, TestSwitchSubject.CallbackOffA);
			TestEvent.AddListener(CallbackOnB, CallbackOffB);
			TestEvent.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			DestroyTestSwitchSubject();

			Invoke();
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
			TestEvent.AddListener(CallbackOnA, CallbackOffA);
			TestEvent.AddListener(TestSwitchSubject.CallbackOnB, TestSwitchSubject.CallbackOffB);
			TestEvent.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			DestroyTestSwitchSubject();

			Invoke();
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
			TestEvent.AddListener(CallbackOnA, CallbackOffA);
			TestEvent.AddListener(CallbackOnB, CallbackOffB);
			TestEvent.AddListener(TestSwitchSubject.CallbackOnC, TestSwitchSubject.CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			DestroyTestSwitchSubject();

			Invoke();
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
			TestEvent.AddListener(TestSwitchSubject.CallbackOnAAndDestroySubject, TestSwitchSubject.CallbackOffA);
			TestEvent.AddListener(CallbackOnB, CallbackOffB);
			TestEvent.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			Invoke();
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
			TestEvent.AddListener(CallbackOnA, CallbackOffA);
			TestEvent.AddListener(TestSwitchSubject.CallbackOnBAndDestroySubject, TestSwitchSubject.CallbackOffB);
			TestEvent.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			Invoke();
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
			TestEvent.AddListener(CallbackOnA, CallbackOffA);
			TestEvent.AddListener(CallbackOnB, CallbackOffB);
			TestEvent.AddListener(TestSwitchSubject.CallbackOnCAndDestroySubject, TestSwitchSubject.CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			Invoke();
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
			TestEvent.AddListener(TestSwitchSubject.CallbackOnA, TestSwitchSubject.CallbackOffAAndDestroySubject);
			TestEvent.AddListener(CallbackOnB, CallbackOffB);
			TestEvent.AddListener(CallbackOnC, CallbackOffC);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			Invoke();
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
			TestEvent.AddListener(CallbackOnA, CallbackOffA);
			TestEvent.AddListener(TestSwitchSubject.CallbackOnB, TestSwitchSubject.CallbackOffBAndDestroySubject);
			TestEvent.AddListener(CallbackOnC, CallbackOffC);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			Invoke();
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
			TestEvent.AddListener(CallbackOnA, CallbackOffA);
			TestEvent.AddListener(CallbackOnB, CallbackOffB);
			TestEvent.AddListener(TestSwitchSubject.CallbackOnC, TestSwitchSubject.CallbackOffCAndDestroySubject);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			Invoke();
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
			TestEvent.AddListener(CallbackOnAAndDestroySubject, CallbackOffA);
			TestEvent.AddListener(TestSwitchSubject.CallbackOnB, TestSwitchSubject.CallbackOffB);
			TestEvent.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			Invoke();
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
			TestEvent.AddListener(CallbackOnAAndDestroySubject, CallbackOffA);
			TestEvent.AddListener(CallbackOnB, CallbackOffB);
			TestEvent.AddListener(TestSwitchSubject.CallbackOnC, TestSwitchSubject.CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			Invoke();
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
			TestEvent.AddListener(TestSwitchSubject.CallbackOnA, TestSwitchSubject.CallbackOffA);
			TestEvent.AddListener(CallbackOnBAndDestroySubject, CallbackOffB);
			TestEvent.AddListener(CallbackOnC, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			Invoke();
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
			TestEvent.AddListener(CallbackOnA, CallbackOffA);
			TestEvent.AddListener(CallbackOnBAndDestroySubject, CallbackOffB);
			TestEvent.AddListener(TestSwitchSubject.CallbackOnC, TestSwitchSubject.CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			Invoke();
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
			TestEvent.AddListener(TestSwitchSubject.CallbackOnA, TestSwitchSubject.CallbackOffA);
			TestEvent.AddListener(CallbackOnB, CallbackOffB);
			TestEvent.AddListener(CallbackOnCAndDestroySubject, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			Invoke();
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
			TestEvent.AddListener(CallbackOnA, CallbackOffA);
			TestEvent.AddListener(TestSwitchSubject.CallbackOnB, TestSwitchSubject.CallbackOffB);
			TestEvent.AddListener(CallbackOnCAndDestroySubject, CallbackOffC);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			// This is where it's removed.
			Invoke();
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
			TestEvent.AddListener(CallbackOnA, CallbackOffAAndDestroySubject);
			Assert.Throws<Exception>(() => TestEvent.AddListener(TestSwitchSubject.CallbackOnB, TestSwitchSubject.CallbackOffB)); // Because Subject was destroyed above.
			TestEvent.AddListener(CallbackOnC, CallbackOffC);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                //(LogType.Log, "Called SwitchOff callback B."), Removed when OffA is called.
			                (LogType.Log, "Called SwitchOff callback C."));

			Invoke();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback C."));
		}

		[Test]
		public void DestroyingDelegateTarget_InsideListener_DoesNotAffectOtherListeners_OffADestroysC()
		{
			CreateTestSwitchSubject();
			TestEvent.AddListener(CallbackOnA, CallbackOffAAndDestroySubject);
			TestEvent.AddListener(CallbackOnB, CallbackOffB);
			Assert.Throws<Exception>(() => TestEvent.AddListener(TestSwitchSubject.CallbackOnC, TestSwitchSubject.CallbackOffC)); // Because Subject was destroyed above.
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
			//              (LogType.Log, "Called SwitchOff callback C.")); Removed when OffA is called.

			Invoke();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."));
		}

		[Test]
		public void DestroyingDelegateTarget_InsideListener_DoesNotAffectOtherListeners_OffBDestroysA()
		{
			CreateTestSwitchSubject();
			TestEvent.AddListener(TestSwitchSubject.CallbackOnA, TestSwitchSubject.CallbackOffA);
			TestEvent.AddListener(CallbackOnB, CallbackOffBAndDestroySubject);
			TestEvent.AddListener(CallbackOnC, CallbackOffC);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."), // Not removed right now because OffA is called before OffB, then removed inside OffB.
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			Invoke();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));
		}

		[Test]
		public void DestroyingDelegateTarget_InsideListener_DoesNotAffectOtherListeners_OffBDestroysC()
		{
			CreateTestSwitchSubject();
			TestEvent.AddListener(CallbackOnA, CallbackOffA);
			TestEvent.AddListener(CallbackOnB, CallbackOffBAndDestroySubject);
			Assert.Throws<Exception>(() => TestEvent.AddListener(TestSwitchSubject.CallbackOnC, TestSwitchSubject.CallbackOffC)); // Because Subject was destroyed above.
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."));
			//              (LogType.Log, "Called SwitchOff callback C.")); Removed when OffB is called.

			Invoke();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback A."),
			                (LogType.Log, "Called SwitchOn callback B."));
		}

		[Test]
		public void DestroyingDelegateTarget_InsideListener_DoesNotAffectOtherListeners_OffCDestroysA()
		{
			CreateTestSwitchSubject();
			TestEvent.AddListener(TestSwitchSubject.CallbackOnA, TestSwitchSubject.CallbackOffA);
			TestEvent.AddListener(CallbackOnB, CallbackOffB);
			TestEvent.AddListener(CallbackOnC, CallbackOffCAndDestroySubject);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."), // Not removed right now because OffA is called before OffC, then removed inside OffC.
			                (LogType.Log, "Called SwitchOff callback B."),
			                (LogType.Log, "Called SwitchOff callback C."));

			Invoke();
			AssertExpectLog((LogType.Log, "Called SwitchOn callback B."),
			                (LogType.Log, "Called SwitchOn callback C."));
		}

		[Test]
		public void DestroyingDelegateTarget_InsideListener_DoesNotAffectOtherListeners_OffCDestroysB()
		{
			CreateTestSwitchSubject();
			TestEvent.AddListener(CallbackOnA, CallbackOffA);
			TestEvent.AddListener(TestSwitchSubject.CallbackOnB, TestSwitchSubject.CallbackOffB);
			TestEvent.AddListener(CallbackOnC, CallbackOffCAndDestroySubject);
			// This is where it's removed.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback A."),
			                (LogType.Log, "Called SwitchOff callback B."), // Not removed right now because OffB is called before OffC, then removed inside OffC.
			                (LogType.Log, "Called SwitchOff callback C."));

			Invoke();
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
			Assert.Catch<ArgumentOutOfRangeException>(() => TestEvent.AddListener(Callback, int.MinValue));
			Assert.Catch<ArgumentOutOfRangeException>(() => TestEvent.AddListener(Callback, int.MaxValue));
		}

		[Test]
		public void CallbackOrder()
		{
			TestEvent.AddListener(
				() =>
				{
					Log.Info("Called callback with order 60.");
				},
				60);

			TestEvent.AddListener(
				() =>
				{
					Log.Info("Called callback with order -40.");
				},
				-40);

			TestEvent.AddListener(
				() =>
				{
					Log.Info("Called callback with default order, added first.");
				}
			);

			TestEvent.AddListener(
				() =>
				{
					Log.Info("Called callback with default order, added second.");
				}
			);

			Invoke();
			AssertExpectLog((LogType.Log, "Called callback with order -40."),
			                (LogType.Log, "Called callback with default order, added first."),
			                (LogType.Log, "Called callback with default order, added second."),
			                (LogType.Log, "Called callback with order 60."));
		}

		#endregion
		/**/

		#region General

		// @formatter:off
		private void Callback()   { Log.Info("Called callback.");    }
		private void CallbackA()  { Log.Info("Called callback A.");  }
		private void CallbackB()  { Log.Info("Called callback B.");  }
		private void CallbackC()  { Log.Info("Called callback C.");  }
		private void CallbackD()  { Log.Info("Called callback D.");  }
		private void CallbackE()  { Log.Info("Called callback E.");  }
		private void CallbackF()  { Log.Info("Called callback F.");  }
		private void ThrowingCallback() { throw new Test_ExtenityEventException("Called throwing callback."); }

		private void CallbackAndRemove()     { Callback(); TestEvent.RemoveListener(CallbackAndRemove); }
		private void CallbackAndRemoveSelf() { Callback(); TestEvent.RemoveCurrentListener(); }

		private void CallbackAAndRemoveSelf()  { CallbackA();  TestEvent.RemoveCurrentListener(); }
		private void CallbackBAndRemoveSelf()  { CallbackB();  TestEvent.RemoveCurrentListener(); }
		private void CallbackCAndRemoveSelf()  { CallbackC();  TestEvent.RemoveCurrentListener(); }

		private void CallbackAAndRemoveA()  { CallbackA();  TestEvent.RemoveListener(CallbackAAndRemoveA); }
		private void CallbackBAndRemoveB()  { CallbackB();  TestEvent.RemoveListener(CallbackBAndRemoveB); }
		private void CallbackCAndRemoveC()  { CallbackC();  TestEvent.RemoveListener(CallbackCAndRemoveC); }

		private void CallbackAAndRemoveB()  { CallbackA();  TestEvent.RemoveListener(CallbackB); }
		private void CallbackAAndRemoveC()  { CallbackA();  TestEvent.RemoveListener(CallbackC); }
		private void CallbackBAndRemoveA()  { CallbackB();  TestEvent.RemoveListener(CallbackA); }
		private void CallbackBAndRemoveC()  { CallbackB();  TestEvent.RemoveListener(CallbackC); }
		private void CallbackCAndRemoveA()  { CallbackC();  TestEvent.RemoveListener(CallbackA); }
		private void CallbackCAndRemoveB()  { CallbackC();  TestEvent.RemoveListener(CallbackB); }

		private void CallbackAAndDestroyLifeSpanTarget()  { CallbackA();  DestroyLifeSpanTargetTestObject(); }
		private void CallbackBAndDestroyLifeSpanTarget()  { CallbackB();  DestroyLifeSpanTargetTestObject(); }
		private void CallbackCAndDestroyLifeSpanTarget()  { CallbackC();  DestroyLifeSpanTargetTestObject(); }

		private void CallbackAAndDestroySubject()  { CallbackA();  DestroyTestEventSubject(); }
		private void CallbackBAndDestroySubject()  { CallbackB();  DestroyTestEventSubject(); }
		private void CallbackCAndDestroySubject()  { CallbackC();  DestroyTestEventSubject(); }
		// @formatter:on

		private void RegisterCallbacks(int order = 0, ListenerLifeSpan lifeSpan = ListenerLifeSpan.Permanent, Object lifeSpanTarget = null)
		{
			TestEvent.AddListener(Callback, order, lifeSpan, lifeSpanTarget);
			AssertRegisteredCallbackCount(1);
		}

		private void RegisterSubjectCallbacks(int order = 0, ListenerLifeSpan lifeSpan = ListenerLifeSpan.Permanent, Object lifeSpanTarget = null)
		{
			CreateTestEventSubject();
			TestEvent.AddListener(TestEventSubject.Callback, order, lifeSpan, lifeSpanTarget);
			AssertRegisteredCallbackCount(1);
		}

		private void AssertRegisteredCallbackCount(int expectedCount)
		{
			Assert.AreEqual(expectedCount, TestEvent.ListenersAliveCount, "Unexpected registered callback count.");
		}

		#endregion
	}

}
