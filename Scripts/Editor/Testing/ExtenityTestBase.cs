using System;
using Extenity.DataToolbox;
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
	}

}
