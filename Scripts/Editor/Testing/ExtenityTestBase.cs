using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Extenity.DataToolbox;
using Extenity.FlowToolbox;
using Extenity.MathToolbox;
using Extenity.ParallelToolbox;
using Extenity.UnityTestToolbox;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.TestTools;

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

			// Mark everything as not initialized first, so TearDown only cleans up what this SetUp managed to
			// initialize if it fails midway, instead of throwing secondary errors that hide the real one.
			IsOnInitializeCalled = false;
			LoopCallbackCheck = LoopCallbackCheckMode.None;
			LoopCallbackSnapshot = null;

			InitializeLoopCallbackCheck();
			InitializeCancellationToken();
			InitializeTiming();
			InitializeLogCatching();

			IsOnInitializeCalled = true;
			OnInitialize();
		}

		private bool IsOnInitializeCalled;

		#endregion

		#region Deinitialization

		protected virtual void OnDeinitialize()
		{
		}

		[TearDown]
		public void Deinitialize()
		{
			DeinitializeCancellationToken();

			// Skip the derived class' cleanup if SetUp failed before calling its initialization.
			if (IsOnInitializeCalled)
			{
				OnDeinitialize();
			}

			DeinitializeLogCatching();
			EnsureAllCheckpointsReached();
			DeinitializeLoopCallbackCheck();
			UnityTestTools.Cleanup();

			// Disable the profiler if it was enabled during the test.
			// This helps a lot when trying to profile a test, as Unity Editor will
			// bog down with many Editor frames if the profiler is not immediately
			// disabled after the test.
			// Note that Record button will still show as if it's recording, but it won't
			// actually record anything when Profiler.enabled is false. Could not find
			// a way to turn off the Record button.
			if (AutoDisableProfilerAtTheEndOfTest && Profiler.enabled)
			{
				Profiler.enabled = false;
			}
		}

		#endregion

		#region Cancellation Token

		private CancellationTokenSource CancellationTokenSource;
		public CancellationToken CancellationToken;

		private void InitializeCancellationToken()
		{
			CancellationTokenSource = new CancellationTokenSource();
			CancellationToken = CancellationTokenSource.Token;
		}

		private void DeinitializeCancellationToken()
		{
			// Not created if SetUp failed before reaching it.
			if (CancellationTokenSource != null)
			{
				CancellationTokenSource.Cancel();
				CancellationTokenSource = null;
			}
		}

		#endregion

		#region Loop Callback Check

		private enum LoopCallbackCheckMode
		{
			/// <summary>SetUp failed before the check could start. TearDown skips the check.</summary>
			None,
			/// <summary>No Loop callbacks are allowed at SetUp and TearDown.</summary>
			NoCallbacksAllowed,
			/// <summary>The callbacks registered at SetUp must be exactly the same at TearDown.</summary>
			MatchSnapshot,
		}

		private LoopCallbackCheckMode LoopCallbackCheck;
		private Loop.CallbackSnapshot LoopCallbackSnapshot;

		private void InitializeLoopCallbackCheck()
		{
			// Ensure Loop is initialized for edit mode tests
			if (Loop.Instance == null)
			{
				Loop.InitializeSystem();
			}

			if (Application.isPlaying)
			{
				// In Play Mode, the application may keep callbacks registered for its whole lifetime (e.g. registered
				// from a [RuntimeInitializeOnLoadMethod]). These are expected, so only require the test to leave the
				// Loop callbacks exactly as it found them: nothing added, nothing removed. Note that with domain
				// reload disabled for entering Play Mode, the callbacks also survive from previous Play Mode sessions,
				// which the snapshot covers as well.
				LoopCallbackSnapshot = Loop.CaptureCallbackSnapshot();
				LoopCallbackCheck = LoopCallbackCheckMode.MatchSnapshot;
			}
			else
			{
				Loop.EnsureAllCallbacksDeregistered();
				LoopCallbackCheck = LoopCallbackCheckMode.NoCallbacksAllowed;
			}
		}

		private void DeinitializeLoopCallbackCheck()
		{
			var mode = LoopCallbackCheck;
			var snapshot = LoopCallbackSnapshot;
			LoopCallbackCheck = LoopCallbackCheckMode.None;
			LoopCallbackSnapshot = null;

			switch (mode)
			{
				case LoopCallbackCheckMode.None:
					// SetUp failed before the check started. Its error is already reported.
					break;
				case LoopCallbackCheckMode.NoCallbacksAllowed:
					Loop.EnsureAllCallbacksDeregistered();
					break;
				case LoopCallbackCheckMode.MatchSnapshot:
					Loop.EnsureCallbacksMatchSnapshot(snapshot);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
			}
		}

		#endregion

		#region Timing

		public float StartTime;
		private const float DefaultPassedTimeThreshold = 3f;
		public float PassedTimeThreshold;
		public float PassedTime => Time.realtimeSinceStartup - StartTime;

		private WaitUntilResult _WaitUntilResult;
		public WaitUntilResult WaitUntilResult => _WaitUntilResult ?? (_WaitUntilResult = new WaitUntilResult());

		private void InitializeTiming()
		{
			PassedTimeThreshold = DefaultPassedTimeThreshold;
			_WaitUntilResult = null;
		}

		public void CheckPassedTestTimeThreshold()
		{
			if (PassedTime > PassedTimeThreshold)
				throw new Exception("Test taking too long. Exceeded the threshold of " + PassedTimeThreshold + " second(s).");
		}

		public void LogPassedTime()
		{
			Log.Info("Passed time: " + TimeSpan.FromSeconds(PassedTime).ToStringMinutesSecondsMilliseconds());
		}

		public async Task<bool> WaitUntilWithTimeout(Func<bool> condition, float timeoutSeconds)
		{
			if (condition())
				return true;

			while (true)
			{
				CancellationToken.ThrowIfCancellationRequested();

				if (condition())
					return true;

				if (PassedTime > timeoutSeconds)
					return false;

				await Task.Yield();
			}
		}

		#endregion

		#region Log Catching

		public enum LogExpectation
		{
			NoLogsAllowed,
			AllowInfoAndBelow,
			AllowAllLogs,
		}

		private const LogExpectation ExpectedLogSeverityDefault = LogExpectation.NoLogsAllowed;
		private LogExpectation ExpectedLogSeverity;
		public List<(LogType Type, string Message)> Logs;

		private LogCaptureScope LogCaptureScope;

		private void InitializeLogCatching()
		{
			ExpectedLogSeverity = ExpectedLogSeverityDefault;
			LogCaptureScope = new LogCaptureScope(ShouldRecordLog);
			Logs = LogCaptureScope.Logs;
		}

		private void DeinitializeLogCatching()
		{
			// Not created if SetUp failed before reaching it.
			if (LogCaptureScope == null)
			{
				Logs = null;
				return;
			}

			try
			{
				if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Passed)
				{
					AssertExpectNoLogsForLogSeverity(ExpectedLogSeverity);
				}
			}
			finally
			{
				// Always restore the original log handler, even if the log assertion above fails.
				Logs = null;
				LogCaptureScope.Dispose();
				LogCaptureScope = null;
			}
		}

		private static bool ShouldRecordLog(LogType type, string message)
		{
			// Special treatment for # logs, that are used for development purposes and have nothing to do with any other system.
			if (message.StartsWith("#", StringComparison.Ordinal))
			{
				return false;
			}

			if (type == LogType.Warning)
			{
				// Discard Unity's diagnostic log that pops up here and there.
				if (message.StartsWith("Diagnostic switches are active and may impact performance or degrade your user experience", StringComparison.Ordinal))
				{
					return false;
				}
			}

			return true;
		}

		public void AssertExpectNoLogs()
		{
			AssertExpectNoLogsForLogSeverity(LogExpectation.NoLogsAllowed);
		}

		public void AssertExpectNoLogsForLogSeverity(LogExpectation expectedLogSeverity)
		{
			switch (expectedLogSeverity)
			{
				case LogExpectation.NoLogsAllowed:
				{
					// All logs are unexpected
					break;
				}
				case LogExpectation.AllowInfoAndBelow:
				{
					// Only info and below are expected. Clear all such logs.
					Logs.RemoveAll(entry => entry.Type == LogType.Log);
					break;
				}
				case LogExpectation.AllowAllLogs:
				{
					// Nothing is unexpected
					return;
				}
				default:
					throw new ArgumentOutOfRangeException();
			}

			if (Logs.Count > 0)
			{
				Assert.Fail($"There were '{Logs.Count}' unexpected log entries emitted in test:\n" + string.Join('\n', Logs.Select(log => string.Concat(log.Type.ToString(), ": ", log.Message))));
			}
		}

		public void AssertExpectLog(params (LogType Type, StringFilterEntry Message)[] expectedLogs)
		{
			/*
			// This became outdated with the fact that ExtenityTestBase is overriding Unity debug logs.
			// Kept here commented out if we ever decide to remove that log override.
			foreach (var expectedExceptionLog in expectedLogs.Where(entry => entry.Type == LogType.Exception))
			{
				// Tell Unity we are expecting the exception. Unity checks the logs if an exception was logged in
				// test. We are handling the exception log in our own way, so there is no need for Unity to jump into
				// conclusions.
				if (expectedExceptionLog.Message.FilterType == StringFilterType.Exactly &&
				    expectedExceptionLog.Message.ComparisonType == StringComparison.InvariantCulture)
				{
					LogAssert.Expect(LogType.Exception, expectedExceptionLog.Message.Filter);
				}
				else
				{
					throw new NotSupportedException("You should modify your test codes for expected Exception logs. These are expected to be stated with Exactly and InvariantCulture string filter configuration, because Unity only accepts full messages in LogAssert.Expect API.");
				}
			}
			*/

			if (expectedLogs.Length != Logs.Count)
			{
				Assert.Fail($"Expected '{expectedLogs.Length}' log entries but got '{Logs.Count}':\n" +
				            "Expected:\n" + string.Join('\n', expectedLogs.Select(log => string.Concat(log.Type.ToString(), ": ", log.Message.ToHumanReadableString()))) + "\n" +
				            "Actual:\n" + string.Join('\n', Logs.Select(log => string.Concat(log.Type.ToString(), ": ", log.Message))));
			}

			for (var i = 0; i < expectedLogs.Length; i++)
			{
				var expectedLog = expectedLogs[i];
				var actualLog = Logs[i];
				if (expectedLog.Type != actualLog.Type || !expectedLog.Message.IsMatching(actualLog.Message))
				{
					Assert.Fail($"Log entry at index '{i}' did not match.\n" +
					            $"Expected: {expectedLog.Type}: {expectedLog.Message.ToHumanReadableString()}\n" +
					            $"Actual: {actualLog.Type}: {actualLog.Message}");
				}
			}

			Logs.Clear();
		}

		public void FailIfAnyErrorLogged()
		{
			foreach (var log in Logs)
			{
				if (log.Type == LogType.Error || log.Type == LogType.Exception)
				{
					Assert.Fail("Stopped the test because there was an unexpected error/exception log. Check previous errors.");
				}
			}
		}

		/// <summary>
		/// <para>There is an assertion check at the end of all tests that looks into console log history to see if
		/// an unexpected log has been written throughout the test.</para>
		///
		/// <para>This method should be called inside a test to explicitly tell the coder who takes a look at that unit
		/// test to understand that the test does not expect a clean console log history.</para>
		/// </summary>
		public void SetExpectedLogSeverity(LogExpectation expectedLogSeverity)
		{
			ExpectedLogSeverity = expectedLogSeverity;
		}

		/// <summary>
		/// Removes every captured log whose message contains <paramref name="includedText"/>
		/// and returns how many were removed. Call this method after a log message is emitted,
		/// if you want to mark it as expected.
		/// </summary>
		/// <remarks>
		/// Note that this method only marks already logged messages, not future messages.
		/// It allows omitting log message before the point of calling this method,
		/// and allows denying the same log message at later stages of the test.
		/// </remarks>
		/// <returns>The number of log messages that were marked as expected.</returns>
		public int MarkLogsAsExpectedThatIncludes(string includedText)
		{
			return LogCaptureScope.MarkLogsAsExpectedThatIncludes(includedText);
		}

		#endregion

		#region Checkpoints

		private HashSet<string> ExpectedCheckpoints;
		private HashSet<string> ReachedCheckpoints;

		private void EnsureAllCheckpointsReached()
		{
			if (ExpectedCheckpoints.IsNotNullAndNotEmpty())
			{
				foreach (var reachedCheckpoint in ReachedCheckpoints)
				{
					ExpectedCheckpoints.Remove(reachedCheckpoint);
				}

				if (ExpectedCheckpoints.Count > 0)
				{
					Assert.Fail($"There were were '{ExpectedCheckpoints.Count}' unreached test checkpoints: " + string.Join(", ", ExpectedCheckpoints));
				}
			}

			ClearCheckpoints();
		}

		private void ClearCheckpoints()
		{
			ExpectedCheckpoints = new HashSet<string>();
			ReachedCheckpoints = new HashSet<string>();
		}

		public void ExpectCheckpoints(string checkpoint1)
		{
			ClearCheckpoints();
			ExpectedCheckpoints.Add(checkpoint1);
		}

		public void ExpectCheckpoints(string checkpoint1, string checkpoint2)
		{
			ClearCheckpoints();
			ExpectedCheckpoints.Add(checkpoint1);
			ExpectedCheckpoints.Add(checkpoint2);
		}

		public void ExpectCheckpoints(string checkpoint1, string checkpoint2, string checkpoint3)
		{
			ClearCheckpoints();
			ExpectedCheckpoints.Add(checkpoint1);
			ExpectedCheckpoints.Add(checkpoint2);
			ExpectedCheckpoints.Add(checkpoint3);
		}

		public void ExpectCheckpoints(string checkpoint1, string checkpoint2, string checkpoint3, string checkpoint4)
		{
			ClearCheckpoints();
			ExpectedCheckpoints.Add(checkpoint1);
			ExpectedCheckpoints.Add(checkpoint2);
			ExpectedCheckpoints.Add(checkpoint3);
			ExpectedCheckpoints.Add(checkpoint4);
		}

		public void InformCheckpoint(string checkpoint)
		{
			ReachedCheckpoints.Add(checkpoint);
		}

		#endregion

		#region Exception

		/// <summary>
		/// Without including any yield in the coroutine, it won't be generated properly. It can easily be overlooked
		/// by a programmer. Use this method in simple coroutines instead of throwing directly, so that the compiler
		/// will always show an error if there is no yield in the coroutine.
		/// </summary>
		public void Throw(string message)
		{
			throw new Exception(message);
		}

		/// <summary>
		/// Without including any yield in the coroutine, it won't be generated properly. It can easily be overlooked
		/// by a programmer. Use this method in simple coroutines instead of throwing directly, so that the compiler
		/// will always show an error if there is no yield in the coroutine.
		/// </summary>
		public void ThrowNotImplemented()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Without including any yield in the coroutine, it won't be generated properly. It can easily be overlooked
		/// by a programmer. Use this method in simple coroutines instead of throwing directly, so that the compiler
		/// will always show an error if there is no yield in the coroutine.
		/// </summary>
		public void ThrowTimedOut()
		{
			throw new Exception("The operation did not complete in allowed duration.");
		}

		public void ThrowIfWaitUntilTimedOut()
		{
			if (WaitUntilResult.IsTimedOut)
			{
				ThrowTimedOut();
			}
		}

		#endregion

		#region Profiling

		public bool AutoDisableProfilerAtTheEndOfTest = true;

		#endregion

		#region Monkey Testing

		public IEnumerator PlayLikeMonkey(float testDuration, Func<int> playSingleSession)
		{
			SetExpectedLogSeverity(LogExpectation.AllowInfoAndBelow); // Because there might be gameplay logs

			const float EditorRefreshIntervals = 1.5f;
			var editorRefreshCountdown = EditorRefreshIntervals;
			var testStartTime = Time.realtimeSinceStartup;
			var meanSessionDuration = new RunningMean();
			var meanMoveCount = new RunningMean();

			while (Time.realtimeSinceStartup < testStartTime + testDuration)
			{
				if (editorRefreshCountdown < 0) // Allow editor to work without locking it down.
				{
					editorRefreshCountdown = EditorRefreshIntervals;
					yield return null;
				}

				var startTime = Time.realtimeSinceStartup;
				try
				{
					int moveCount = playSingleSession();
					meanMoveCount.Push(moveCount);
				}
				catch (Exception exception)
				{
					Log.Error(exception);
				}
				var endTime = Time.realtimeSinceStartup;

				meanSessionDuration.Push(endTime - startTime);
				editorRefreshCountdown -= endTime - startTime;
			}

			Log.Info($"Monkey testing finished with total of {meanSessionDuration.ValueCount} sessions, an average of {meanMoveCount.Mean} moves and an average session duration of {meanSessionDuration.Mean.ToStringMinutesSecondsMillisecondsFromSeconds()}.");
		}

		#endregion

		#region Log

		private static readonly Logger Log = new();

		#endregion
	}

}
