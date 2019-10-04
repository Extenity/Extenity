using System;
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
		public void BasicInvoking()
		{
			TestEvent.AddListener(Callback);

			TestEvent.Invoke();
			AssertExpectLog((LogType.Log, "Called callback."));
		}

		[Test]
		public void InvokingDoesNotGetAffectedByExceptions()
		{
			TestEvent.AddListener(Callback, 10);
			TestEvent.AddListener(ThrowingCallback, 20);
			TestEvent.AddListener(Callback, 30);

			TestEvent.InvokeSafe();
			AssertExpectLog((LogType.Log, "Called callback."));
			AssertExpectLog((LogType.Exception, "AAAAAAAAAAA")); // TODO: Copy and paste the exception here
			AssertExpectLog((LogType.Log, "Called callback."));
		}

		[Test]
		public void InvokingUnsafeDoesPreventFutureCallsAfterExceptions()
		{
			TestEvent.AddListener(Callback, 10);
			TestEvent.AddListener(ThrowingCallback, 20);
			TestEvent.AddListener(Callback, 30);

			TestEvent.InvokeSafe();
			AssertExpectLog((LogType.Log, "Called callback."));
			AssertExpectLog((LogType.Exception, "AAAAAAAAAAA")); // TODO: Copy and paste the exception here
			AssertExpectNoLogs(); // The third call will not happen, hence there will be no logs.
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
			AssertExpectLog((LogType.Log, "Called callback with order 60."));

			TestEvent.AddListener(
				() =>
				{
					Log.Info("Called callback with order -40.");
				},
				-40);
			AssertExpectLog((LogType.Log, "Called callback with order -40."));

			TestEvent.AddListener(() =>
			{
				Log.Info("Called callback with default order, added first.");
			});
			AssertExpectLog((LogType.Log, "Called callback with default order, added first."));
			TestEvent.AddListener(
				() =>
				{
					Log.Info("Called callback with default order, added second.");
				});
			AssertExpectLog((LogType.Log, "Called callback with default order, added second."));

			TestEvent.Invoke();
			AssertExpectLog((LogType.Log, "Called callback with order -40."));
			AssertExpectLog((LogType.Log, "Called callback with default order, added first."));
			AssertExpectLog((LogType.Log, "Called callback with default order, added second."));
			AssertExpectLog((LogType.Log, "Called callback with order 60."));
		}

		#endregion

		#region General

		private void Callback()
		{
			Log.Info("Called callback.");
		}

		private void ThrowingCallback()
		{
			throw new Exception("Called throwing callback.");
		}

		#endregion
	}

}
