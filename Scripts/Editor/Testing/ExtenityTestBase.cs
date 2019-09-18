using System;
using System.Collections.Generic;
using System.Linq;
using Extenity.DataToolbox;
using Extenity.ParallelToolbox.Editor;
using Extenity.UnityTestToolbox;
using NUnit.Framework;
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
			StartTime = Time.realtimeSinceStartup;
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
		protected float PassedTimeThreshold = 3f;
		protected float PassedTime => Time.realtimeSinceStartup - StartTime;

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

		protected List<(LogType Type, string Message)> Logs;

		private void InitializeLogCatching()
		{
			Logs = new List<(LogType, string)>();
			Application.logMessageReceived -= RegisterLogMessage; // Just in case.
			Application.logMessageReceived += RegisterLogMessage;
		}

		private void DeinitializeLogCatching()
		{
			AssertExpectNoLogs();

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
			Assert.AreEqual(0, Logs.Count);
		}

		protected void AssertExpectLog(params (LogType Type, string Message)[] expectedLogs)
		{
			Assert.AreEqual(expectedLogs.ToList(), Logs);
			Logs.Clear();
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

		#endregion
	}

}
