using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extenity.DataToolbox;
using Extenity.MathToolbox;
using Extenity.ParallelToolbox;
using Extenity.UnityTestToolbox;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using UnityEngine;
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

			InitializeTiming();
#if UNITY_EDITOR
			EditorCoroutine.EnsureNoRunningEditorCoroutines();
#endif
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
			EnsureAllCheckpointsReached();
#if UNITY_EDITOR
			EditorCoroutine.EnsureNoRunningEditorCoroutines();
#endif
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
				Logs = New.List<(LogType, string)>(100);
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

			Release.List(ref Logs);
		}

		private void RegisterLogMessage(string condition, string stacktrace, LogType type)
		{
			if (condition.StartsWith("#")) // Ignore lines that starts with comment '#' character
				return;
			Logs.Add((type, condition));
		}

		protected void AssertExpectNoLogs()
		{
			if (Logs.Count > 0)
			{
				Assert.Fail($"There were '{Logs.Count}' unexpected log entries emitted in test.");
			}
		}

		protected void AssertExpectLog(params (LogType Type, string Message)[] expectedLogs)
		{
			foreach (var expectedExceptionLog in expectedLogs.Where(entry => entry.Type == LogType.Exception))
			{
				// Tell the Unity we are expecting the exception. Unity checks the logs if an exception was logged in
				// test. We are handling the exception log in out own way, so there is no need for Unity to jump into
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
		protected void MarkThatThisTestDoesNotCareAboutCleanLogs()
		{
			DoesNotCareAboutCleanLogs = true;
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

		protected void ExpectCheckpoints(string checkpoint1)
		{
			ClearCheckpoints();
			ExpectedCheckpoints.Add(checkpoint1);
		}

		protected void ExpectCheckpoints(string checkpoint1, string checkpoint2)
		{
			ClearCheckpoints();
			ExpectedCheckpoints.Add(checkpoint1);
			ExpectedCheckpoints.Add(checkpoint2);
		}

		protected void ExpectCheckpoints(string checkpoint1, string checkpoint2, string checkpoint3)
		{
			ClearCheckpoints();
			ExpectedCheckpoints.Add(checkpoint1);
			ExpectedCheckpoints.Add(checkpoint2);
			ExpectedCheckpoints.Add(checkpoint3);
		}

		protected void ExpectCheckpoints(string checkpoint1, string checkpoint2, string checkpoint3, string checkpoint4)
		{
			ClearCheckpoints();
			ExpectedCheckpoints.Add(checkpoint1);
			ExpectedCheckpoints.Add(checkpoint2);
			ExpectedCheckpoints.Add(checkpoint3);
			ExpectedCheckpoints.Add(checkpoint4);
		}

		protected void InformCheckpoint(string checkpoint)
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

		#region Monkey Testing

		protected IEnumerator PlayLikeMonkey(float testDuration, Func<int> playSingleSession)
		{
			MarkThatThisTestDoesNotCareAboutCleanLogs();

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

		private static readonly Logger Log = new(nameof(ExtenityTestBase));

		#endregion
	}

}
