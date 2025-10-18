using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Extenity.DataToolbox;
using Extenity.MathToolbox;
using Extenity.ParallelToolbox;
using Extenity.UnityTestToolbox;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using Extenity.ParallelToolbox.Editor;
#endif

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

		private const LogExpectation ExpectedLogsDefault = LogExpectation.NoLogsAllowed;
		private LogExpectation ExpectedLogs;
		public List<(LogType Type, string Message)> Logs;

		private void InitializeLogCatching()
		{
			ExpectedLogs = ExpectedLogsDefault;

			if (Logs == null)
				Logs = New.List<(LogType, string)>(100);
			else
				Logs.Clear();

			Application.logMessageReceived -= RegisterLogMessage; // Just in case.
			Application.logMessageReceived += RegisterLogMessage;
		}

		private void DeinitializeLogCatching()
		{
			if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Passed)
			{
				AssertExpectLogs(ExpectedLogs);
			}

			Application.logMessageReceived -= RegisterLogMessage;

			Release.List(ref Logs);
		}

		private void RegisterLogMessage(string condition, string stacktrace, LogType type)
		{
			if (condition.StartsWith("#")) // Ignore lines that starts with comment '#' character
				return;

			// Ignore unimportant Unity logs
			switch (type)
			{
				case LogType.Warning:
				{
					if (condition.StartsWith("Diagnostic switches are active and may impact performance or degrade your user experience", StringComparison.Ordinal))
					{
						return;
					}
					break;
				}
				default:
					break; // Ignore others
			}

			Logs.Add((type, condition));
		}

		public void AssertExpectNoLogs()
		{
			AssertExpectLogs(LogExpectation.NoLogsAllowed);
		}

		public void AssertExpectLogs(LogExpectation expectedLogs)
		{
			if (Logs.Count > 0)
			{
				switch (expectedLogs)
				{
					case LogExpectation.NoLogsAllowed:
					{
						// All logs are unexpected
						Assert.Fail($"There were '{Logs.Count}' unexpected log entries emitted in test.");
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

		/// <summary>
		/// <para>There is an assertion check at the end of all tests that looks into console log history to see if
		/// an unexpected log has been written throughout the test.</para>
		///
		/// <para>This method should be called inside a test to explicitly tell the coder who takes a look at that unit
		/// test to understand that the test does not expect a clean console log history.</para>
		/// </summary>
		public void SetExpectedLogs(LogExpectation expectedLogs)
		{
			ExpectedLogs = expectedLogs;
		}

		/// <remarks>
		/// Note that this method only marks already logged messages, not future messages.
		/// It allows omitting log message before the point of calling this method,
		/// and allows denying the same log message at later stages of the test.
		/// So, call this method after the log message is emitted, if you want to mark it as expected.
		/// </remarks>
		/// <returns>The number of log messages that were marked as expected.</returns>
		public int MarkLogsAsExpectedThatIncludes(string includedText)
		{
			var foundCount = 0;
			for (var i = 0; i < Logs.Count; i++)
			{
				if (Logs[i].Message.Contains(includedText, StringComparison.Ordinal))
				{
					// Mark this log as expected by removing it from the log list.
					Logs.RemoveAt(i);
					i--;
					foundCount++;
				}
			}

			return foundCount;
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
			SetExpectedLogs(LogExpectation.AllowInfoAndBelow); // Because there might be gameplay logs

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
