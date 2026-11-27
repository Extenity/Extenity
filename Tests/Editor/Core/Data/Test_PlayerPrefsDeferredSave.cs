using System;
using System.Linq;
using System.Reflection;
using Extenity.DataToolbox;
using Extenity.FlowToolbox;
using Extenity.Testing;
using NUnit.Framework;
using UnityEngine;

namespace ExtenityTests.DataToolbox
{

	/// <summary>
	/// The deferred save of <see cref="PlayerPrefsTools"/>. The writes are counted through
	/// <see cref="PlayerPrefsTools.SaveToDisk"/> instead of touching the disk, and the clock is passed in, so no test
	/// waits for real time to pass.
	/// </summary>
	public class Test_PlayerPrefsDeferredSave : ExtenityTestBase
	{
		#region Initialization and Deinitialization

		private int SaveCount;
		private Action OriginalSaveToDisk;

		protected override void OnInitialize()
		{
			Assert.IsFalse(PlayerPrefsTools.IsDeferredSavePending, "A deferred save was already pending before the test.");
			SaveCount = 0;
			OriginalSaveToDisk = PlayerPrefsTools.SaveToDisk;
			PlayerPrefsTools.SaveToDisk = () => SaveCount++;
		}

		protected override void OnDeinitialize()
		{
			// Takes the pending save's callbacks out of Loop and Application.quitting, without writing to disk.
			PlayerPrefsTools.FlushDeferredSave();
			PlayerPrefsTools.SaveToDisk = OriginalSaveToDisk;
		}

		#endregion

		#region Delay

		[Test]
		public void BurstOfRequests_DoesNotSaveAtOnce_AndSavesOnceWhenTheFirstDelayEnds()
		{
			// One request per frame for one second, like a slider dragged across.
			for (var i = 0; i < 60; i++)
			{
				PlayerPrefsTools.DeferredSave(1f, 10.0 + i / 60.0);
			}
			Assert.AreEqual(0, SaveCount);
			Assert.IsTrue(PlayerPrefsTools.IsDeferredSavePending);

			PlayerPrefsTools.CheckDeferredSave(10.999);
			Assert.AreEqual(0, SaveCount);

			PlayerPrefsTools.CheckDeferredSave(11.0);
			Assert.AreEqual(1, SaveCount);
			Assert.IsFalse(PlayerPrefsTools.IsDeferredSavePending);

			PlayerPrefsTools.CheckDeferredSave(30.0);
			Assert.AreEqual(1, SaveCount, "Saved again without a new request.");
		}

		[Test]
		public void ShorterDelay_BringsTheSaveForward()
		{
			PlayerPrefsTools.DeferredSave(5f, 10.0);
			PlayerPrefsTools.DeferredSave(1f, 11.0);

			PlayerPrefsTools.CheckDeferredSave(11.999);
			Assert.AreEqual(0, SaveCount);
			PlayerPrefsTools.CheckDeferredSave(12.0);
			Assert.AreEqual(1, SaveCount);
		}

		[Test]
		public void LongerDelay_DoesNotPutTheSaveOff()
		{
			PlayerPrefsTools.DeferredSave(1f, 10.0);
			PlayerPrefsTools.DeferredSave(5f, 10.5);

			PlayerPrefsTools.CheckDeferredSave(11.0);
			Assert.AreEqual(1, SaveCount);
		}

		[Test]
		public void RequestAfterASave_StartsANewDelay()
		{
			PlayerPrefsTools.DeferredSave(1f, 10.0);
			PlayerPrefsTools.CheckDeferredSave(11.0);
			Assert.AreEqual(1, SaveCount);

			PlayerPrefsTools.DeferredSave(1f, 20.0);
			Assert.IsTrue(PlayerPrefsTools.IsDeferredSavePending);
			PlayerPrefsTools.CheckDeferredSave(20.5);
			Assert.AreEqual(1, SaveCount);
			PlayerPrefsTools.CheckDeferredSave(21.0);
			Assert.AreEqual(2, SaveCount);
		}

		[Test]
		public void PendingSave_IsCheckedByLoop_OnlyWhilePending()
		{
			var callbacks = Loop.Instance.UpdateEvery100MillisecondsUnscaledCallbacks;
			Assert.AreEqual(0, callbacks.ListenersCount);

			PlayerPrefsTools.DeferredSave(1f, 10.0);
			PlayerPrefsTools.DeferredSave(1f, 10.5);
			Assert.AreEqual(1, callbacks.ListenersCount);

			PlayerPrefsTools.CheckDeferredSave(11.0);
			Assert.AreEqual(0, callbacks.ListenersCount);
		}

		#endregion

		#region Flush

		[Test]
		public void Flush_WritesThePendingSaveAtOnce_AndOnlyOnce()
		{
			PlayerPrefsTools.DeferredSave(1f, 10.0);

			PlayerPrefsTools.FlushDeferredSave();
			Assert.AreEqual(1, SaveCount);
			Assert.IsFalse(PlayerPrefsTools.IsDeferredSavePending);

			PlayerPrefsTools.CheckDeferredSave(11.0);
			PlayerPrefsTools.FlushDeferredSave();
			Assert.AreEqual(1, SaveCount);
		}

		[Test]
		public void Flush_WithoutAPendingSave_DoesNothing()
		{
			PlayerPrefsTools.FlushDeferredSave();
			Assert.AreEqual(0, SaveCount);
		}

		#endregion

		#region Application Quit

		/// <summary>
		/// Application.quitting is raised when the application quits and, in the Editor, when Play Mode ends. The test
		/// can't raise it without also running every other subscriber, so it looks for the handler among the
		/// subscribers and calls only that one.
		/// </summary>
		[Test]
		public void PendingSave_IsWrittenWhenTheApplicationQuits()
		{
			Assert.IsNull(GetQuittingHandler(), "The handler was subscribed before a save was requested.");

			PlayerPrefsTools.DeferredSave(1f, 10.0);
			PlayerPrefsTools.DeferredSave(1f, 10.5);
			var handler = GetQuittingHandler();
			Assert.IsNotNull(handler, "No handler of Application.quitting writes the pending save.");
			Assert.AreEqual(1, GetQuittingHandlers().Length, "Subscribed more than once.");

			handler();
			Assert.AreEqual(1, SaveCount);
			Assert.IsFalse(PlayerPrefsTools.IsDeferredSavePending);
			Assert.IsNull(GetQuittingHandler(), "Still subscribed after the save.");
		}

		private static Action GetQuittingHandler()
		{
			return GetQuittingHandlers().SingleOrDefault();
		}

		private static Action[] GetQuittingHandlers()
		{
			var field = typeof(Application).GetField("quitting", BindingFlags.Static | BindingFlags.NonPublic);
			if (field == null)
				throw new Exception("Could not find the field behind Application.quitting. Unity may have changed it.");
			var quitting = (Action)field.GetValue(null);
			if (quitting == null)
				return Array.Empty<Action>();
			return quitting.GetInvocationList()
			               .Cast<Action>()
			               .Where(handler => handler.Method.DeclaringType == typeof(PlayerPrefsTools))
			               .ToArray();
		}

		#endregion
	}

}
