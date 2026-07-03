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

			// Ensure Loop is initialized for edit mode tests
			if (Loop.Instance == null)
			{
				Loop.InitializeSystem();
			}
			else
			{
				Loop.EnsureAllCallbacksDeregistered();
			}

			InitializeCancellationToken();
			InitializeTiming();
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
			DeinitializeCancellationToken();

			OnDeinitialize();

			DeinitializeLogCatching();
			EnsureAllCheckpointsReached();
			Loop.EnsureAllCallbacksDeregistered();
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
			CancellationTokenSource.Cancel();
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
			if (Logs.Count > 0)
			{
				switch (expectedLogSeverity)
				{
					case LogExpectation.NoLogsAllowed:
					{
						// All logs are unexpected
						Assert.Fail($"There were '{Logs.Count}' unexpected log entries emitted in test:\n" + string.Join('\n', Logs.Select(log => string.Concat(log.Type.ToString(), ": ", log.Message))));
						break;
					}
					case LogExpectation.AllowInfoAndBelow:
					{
						// Only info and below are expected
						Logs.RemoveAll(entry => entry.Type == LogType.Log);
						break;
					}
					case LogExpectation.AllowAllLogs:
					{
						// Nothing is unexpected
						break;
					}
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		public void AssertExpectLog(params (LogType Type, string Message)[] expectedLogs)
		{
			foreach (var expectedExceptionLog in expectedLogs.Where(entry => entry.Type == LogType.Exception))
			{
				// Tell Unity we are expecting the exception. Unity checks the logs if an exception was logged in
				// test. We are handling the exception log in our own way, so there is no need for Unity to jump into
				// conclusions.
				LogAssert.Expect(LogType.Exception, expectedExceptionLog.Message);
			}

			Assert.AreEqual(expectedLogs.ToList(), Logs);
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
			if (ExpectedCheckpoints.IsNotNullAndEmpty())
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
