﻿using System;
using System.Collections.Generic;
using System.Linq;
using Extenity.DataToolbox;
using Extenity.ParallelToolbox;
using Extenity.ParallelToolbox.Editor;
using Extenity.UnityTestToolbox;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using UnityEngine;

namespace Extenity.Testing
{

	public abstract class ExtenityTestBase : AssertionHelper
	{
		#region Initialization

		protected virtual void OnInitialize()
		{
		}

		[SetUp]
		public void Initialize()
		{
			// Note that the previous test values are persistent. So everything should be reset to its defaults here.

			// This should be the very first line of the test.
			StartTime = Time.realtimeSinceStartup;

			InitializeTiming();
			EditorCoroutine.EnsureNoRunningEditorCoroutines();
			InitializeLogCatching();

			OnInitialize();
		}

		#endregion

		#region Deinitialization

		protected virtual void OnDeinitialize()
		{
		}

		[TearDown]
		public void Deinitialize()
		{
			OnDeinitialize();

			DeinitializeLogCatching();
			EditorCoroutine.EnsureNoRunningEditorCoroutines();
			UnityTestTools.Cleanup();
		}

		#endregion

		#region Timing

		protected float StartTime;
		private const float DefaultPassedTimeThreshold = 3f;
		protected float PassedTimeThreshold;
		protected float PassedTime => Time.realtimeSinceStartup - StartTime;

		private WaitUntilResult _WaitUntilResult;
		protected WaitUntilResult WaitUntilResult => _WaitUntilResult ?? (_WaitUntilResult = new WaitUntilResult());

		private void InitializeTiming()
		{
			PassedTimeThreshold = DefaultPassedTimeThreshold;
			_WaitUntilResult = null;
		}

		protected void CheckPassedTestTimeThreshold()
		{
			if (PassedTime > PassedTimeThreshold)
				throw new Exception("Test taking too long.");
		}

		protected void LogPassedTime()
		{
			Log.Info("Passed time: " + TimeSpan.FromSeconds(PassedTime).ToStringMinutesSecondsMilliseconds());
		}

		#endregion

		#region Log Catching

		private const bool DefaultDoesNotCareAboutCleanLogs = false;
		private bool DoesNotCareAboutCleanLogs;
		protected List<(LogType Type, string Message)> Logs;

		private void InitializeLogCatching()
		{
			DoesNotCareAboutCleanLogs = DefaultDoesNotCareAboutCleanLogs;

			if (Logs == null)
				Logs = new List<(LogType, string)>(100);
			else
				Logs.Clear();

			Application.logMessageReceived -= RegisterLogMessage; // Just in case.
			Application.logMessageReceived += RegisterLogMessage;
		}

		private void DeinitializeLogCatching()
		{
			if (!DoesNotCareAboutCleanLogs && TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Passed)
			{
				AssertExpectNoLogs();
			}

			Application.logMessageReceived -= RegisterLogMessage;
		}

		private void RegisterLogMessage(string condition, string stacktrace, LogType type)
		{
			if (condition.StartsWith("#")) // Ignore lines that starts with comment '#' character
				return;
			Logs.Add((type, condition));
		}

		protected void AssertExpectNoLogs()
		{
			Assert.AreEqual(0, Logs.Count, "There were unexpected log entries emitted in test.");
		}

		protected void AssertExpectLog(params (LogType Type, string Message)[] expectedLogs)
		{
			Assert.AreEqual(expectedLogs.ToList(), Logs);
			Logs.Clear();
		}

		/// <summary>
		/// <para>There is an assertion check at the end of all tests that looks into console log history to see if
		/// an unexpected log has been written throughout the test.</para>
		///
		/// <para>This method should be called inside a test to explicitly tell the coder who takes a look at that unit
		/// test to understand that the test does not expect a clean console log history.</para>
		/// </summary>
		protected void MarkThatThisTestDoesNotCareAboutCleanLogs()
		{
			DoesNotCareAboutCleanLogs = true;
		}

		#endregion

		#region Exception

		/// <summary>
		/// Without including any yield in the coroutine, it won't be generated properly. It can easily be overlooked
		/// by a programmer. Use this method in simple coroutines instead of throwing directly, so that the compiler
		/// will always show an error if there is no yield in the coroutine.
		/// </summary>
		protected void Throw(string message)
		{
			throw new Exception(message);
		}

		/// <summary>
		/// Without including any yield in the coroutine, it won't be generated properly. It can easily be overlooked
		/// by a programmer. Use this method in simple coroutines instead of throwing directly, so that the compiler
		/// will always show an error if there is no yield in the coroutine.
		/// </summary>
		protected void ThrowNotImplemented()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Without including any yield in the coroutine, it won't be generated properly. It can easily be overlooked
		/// by a programmer. Use this method in simple coroutines instead of throwing directly, so that the compiler
		/// will always show an error if there is no yield in the coroutine.
		/// </summary>
		protected void ThrowTimedOut()
		{
			throw new Exception("The operation did not complete in allowed duration.");
		}

		protected void ThrowIfWaitUntilTimedOut()
		{
			if (WaitUntilResult.IsTimedOut)
			{
				ThrowTimedOut();
			}
		}

		#endregion
	}

}