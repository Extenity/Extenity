using Extenity.MessagingToolbox;
using NUnit.Framework;
using UnityEngine;

namespace ExtenityTests.MessagingToolbox
{

	public class Test_ExtenityEvent : Test_EventTestBase
	{
		#region Basics

		[Test]
		public void AlrightToInvokeInBlank()
		{
			TestEvent.Invoke();
			TestEvent.InvokeSafe();
			AssertExpectNoLogs();
		}

		#endregion

		#region Emitting (Invoke)

		[Test]
		public void BasicInvoking_NonUnityObject()
		{
			TestEvent.AddListener(Callback);

			TestEvent.Invoke();
			AssertExpectLog((LogType.Log, "Called callback."));
		}

		[Test]
		public void BasicInvoking_UnityObject()
		{
			TestEvent.AddListener(CreateTestEventSubject().Callback);

			TestEvent.Invoke();
			AssertExpectLog((LogType.Log, "Called Subject callback."));
		}

		[Test]
		public void InvokingWithMultipleListeners_Orderless()
		{
			TestEvent.AddListener(CallbackA);
			TestEvent.AddListener(CallbackB);
			TestEvent.AddListener(CallbackC);

			TestEvent.InvokeSafe();
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

			TestEvent.InvokeSafe();
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

			TestEvent.InvokeSafe();
			AssertExpectLog((LogType.Log, "Called callback A."),
			                (LogType.Log, "Called callback B."),
			                (LogType.Log, "Called callback C."),
			                (LogType.Log, "Called callback D."),
			                (LogType.Log, "Called callback E."),
			                (LogType.Log, "Called callback F."));
		}

		[Test]
		public void InvokingDoesNotGetAffectedByExceptions()
		{
			TestEvent.AddListener(CallbackA, 10);
			TestEvent.AddListener(ThrowingCallback, 20);
			TestEvent.AddListener(CallbackC, 30);

			TestEvent.InvokeSafe();
			AssertExpectLog((LogType.Log, "Called callback A."),
			                (LogType.Exception, "Test_ExtenityEventException: Called throwing callback."),
			                (LogType.Log, "Called callback C."));
		}

		[Test]
		public void InvokingUnsafeDoesNotCatchExceptions()
		{
			TestEvent.AddListener(CallbackA, 10);
			TestEvent.AddListener(ThrowingCallback, 20);
			TestEvent.AddListener(CallbackC, 30);

			Assert.Throws<Test_ExtenityEventException>(() => TestEvent.Invoke());
			AssertExpectLog((LogType.Log, "Called callback A."));
		}

		#endregion

		#region Callback Life Span

		[Test]
		public void LifeSpan_Permanent()
		{
			TestEvent.AddListener(Callback, 0, ListenerLifeSpan.Permanent);

			TestEvent.Invoke();
			AssertExpectLog((LogType.Log, "Called callback."));
			TestEvent.Invoke();
			AssertExpectLog((LogType.Log, "Called callback."));
			TestEvent.Invoke();
			AssertExpectLog((LogType.Log, "Called callback."));

			// Manually removing is the only way. (or there is that LifeSpanTarget feature too)
			TestEvent.RemoveListener(Callback);

			TestEvent.Invoke();
			AssertExpectNoLogs();
		}

		[Test]
		public void LifeSpan_Permanent_WithLifeSpanTargetDestroyedLater()
		{
			TestEvent.AddListener(Callback, 0, ListenerLifeSpan.Permanent, CreateLifeSpanTargetTestObject());

			TestEvent.Invoke();
			AssertExpectLog((LogType.Log, "Called callback."));
			TestEvent.Invoke();
			AssertExpectLog((LogType.Log, "Called callback."));
			TestEvent.Invoke();
			AssertExpectLog((LogType.Log, "Called callback."));

			// Destroy the LifeSpanTarget and the registered listener will not be called anymore.
			DestroyLifeSpanTargetTestObject();

			TestEvent.Invoke();
			AssertExpectNoLogs();
		}

		[Test]
		public void LifeSpan_Permanent_WithLifeSpanTargetDestroyedFirst()
		{
			TestEvent.AddListener(Callback, 0, ListenerLifeSpan.Permanent, CreateLifeSpanTargetTestObject());

			// Destroy the LifeSpanTarget and the registered listener will not be called anymore.
			DestroyLifeSpanTargetTestObject();

			TestEvent.Invoke();
			AssertExpectNoLogs();
		}

		[Test]
		public void LifeSpan_Permanent_WithDelegateTargetDestroyedLater()
		{
			TestEvent.AddListener(CreateTestEventSubject().Callback, 0, ListenerLifeSpan.Permanent);

			TestEvent.Invoke();
			AssertExpectLog((LogType.Log, "Called Subject callback."));
			TestEvent.Invoke();
			AssertExpectLog((LogType.Log, "Called Subject callback."));
			TestEvent.Invoke();
			AssertExpectLog((LogType.Log, "Called Subject callback."));

			// Destroy the Subject and the registered listener will not be called anymore.
			DestroyTestEventSubject();

			TestEvent.Invoke();
			AssertExpectNoLogs();
		}

		[Test]
		public void LifeSpan_Permanent_WithDelegateTargetDestroyedFirst()
		{
			TestEvent.AddListener(CreateTestEventSubject().Callback, 0, ListenerLifeSpan.Permanent);

			// Destroy the Subject and the registered listener will not be called anymore.
			DestroyTestEventSubject();

			TestEvent.Invoke();
			AssertExpectNoLogs();
		}

		[Test]
		public void LifeSpan_RemovedAtFirstEmit()
		{
			TestEvent.AddListener(Callback, 0, ListenerLifeSpan.RemovedAtFirstEmit);

			// The callback will be deregistered after this.
			TestEvent.Invoke();
			AssertExpectLog((LogType.Log, "Called callback."));

			TestEvent.Invoke();
			AssertExpectNoLogs();
		}

		[Test]
		public void LifeSpan_RemovedAtFirstEmit_WithLifeSpanTargetDestroyedLater()
		{
			TestEvent.AddListener(Callback, 0, ListenerLifeSpan.RemovedAtFirstEmit, CreateLifeSpanTargetTestObject());

			// The callback will be deregistered after this.
			TestEvent.Invoke();
			AssertExpectLog((LogType.Log, "Called callback."));

			TestEvent.Invoke();
			AssertExpectNoLogs();

			// Destroying the LifeSpanTarget does nothing after that. The listener was already deregistered, thanks to RemovedAtFirstEmit.
			DestroyLifeSpanTargetTestObject();
			TestEvent.Invoke();
			AssertExpectNoLogs();
		}

		[Test]
		public void LifeSpan_RemovedAtFirstEmit_WithLifeSpanTargetDestroyedFirst()
		{
			TestEvent.AddListener(Callback, 0, ListenerLifeSpan.RemovedAtFirstEmit, CreateLifeSpanTargetTestObject());

			// The callback will be deregistered after this.
			DestroyLifeSpanTargetTestObject();

			TestEvent.Invoke();
			AssertExpectNoLogs();
		}

		[Test]
		public void LifeSpan_RemovedAtFirstEmit_WithDelegateTargetDestroyedLater()
		{
			TestEvent.AddListener(CreateTestEventSubject().Callback, 0, ListenerLifeSpan.RemovedAtFirstEmit);

			// The callback will be deregistered after this.
			TestEvent.Invoke();
			AssertExpectLog((LogType.Log, "Called Subject callback."));

			TestEvent.Invoke();
			AssertExpectNoLogs();

			// Destroying the Subject does nothing after that. The listener was already deregistered, thanks to RemovedAtFirstEmit.
			DestroyTestEventSubject();
			TestEvent.Invoke();
			AssertExpectNoLogs();
		}

		[Test]
		public void LifeSpan_RemovedAtFirstEmit_WithDelegateTargetDestroyedFirst()
		{
			TestEvent.AddListener(CreateTestEventSubject().Callback, 0, ListenerLifeSpan.RemovedAtFirstEmit);

			// The callback will be deregistered after this.
			DestroyTestEventSubject();

			TestEvent.Invoke();
			AssertExpectNoLogs();
		}

		#endregion

		#region Callback Order

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

			TestEvent.AddListener(() =>
			{
				Log.Info("Called callback with default order, added first.");
			});

			TestEvent.AddListener(
				() =>
				{
					Log.Info("Called callback with default order, added second.");
				});

			TestEvent.Invoke();
			AssertExpectLog((LogType.Log, "Called callback with order -40."),
			                (LogType.Log, "Called callback with default order, added first."),
			                (LogType.Log, "Called callback with default order, added second."),
			                (LogType.Log, "Called callback with order 60."));
		}

		#endregion

		#region General

		private void Callback() { Log.Info("Called callback."); }
		private void CallbackA() { Log.Info("Called callback A."); }
		private void CallbackB() { Log.Info("Called callback B."); }
		private void CallbackC() { Log.Info("Called callback C."); }
		private void CallbackD() { Log.Info("Called callback D."); }
		private void CallbackE() { Log.Info("Called callback E."); }
		private void CallbackF() { Log.Info("Called callback F."); }
		private void ThrowingCallback() { throw new Test_ExtenityEventException("Called throwing callback."); }

		#endregion
	}

}
