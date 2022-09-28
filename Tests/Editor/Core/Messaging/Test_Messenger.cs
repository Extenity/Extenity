#if ExtenityMessenger && !UseLegacyMessenger

using Extenity.MessagingToolbox;
using NUnit.Framework;
using UnityEngine;

namespace ExtenityTests.MessagingToolbox
{

	// TODO:
	//TestMessenger.RegisterToEvent("")
	//TestMessenger.RegisterToState("")

	public class Test_Messenger : Test_MessengerTestBase
	{
		// -----------------------------------------------------------------------------------
		//    Switch
		// Please note that the tests below are only the tip of the iceberg. ExtenitySwitch
		// is used as the underlying platform and it has its own tests. The tests here only
		// covers the ExtenitySwitch integration of Messenger.
		// -----------------------------------------------------------------------------------

		#region Switch Basics

		[Test]
		public void Switch_InitiallySwitchedOff()
		{
			var isSwitchedOn = TestMessenger.GetSwitch("LevelLoaded");
			Assert.False(isSwitchedOn);
		}

		[Test]
		public void Switch_AlrightToSwitchWithoutCallbacks()
		{
			TestMessenger.SwitchOn("LevelLoaded");
			TestMessenger.SwitchOff("LevelLoaded");
		}

		[Test]
		public void Switch_AlrightToRegisterToNotYetEmittedSwitch()
		{
			TestMessenger.AddSwitchListener("LevelLoaded", null, null);
		}

		#endregion

		#region Switch On/Off

		[Test]
		public void Switch_CallbackInstantlyInvoked_InitiallySwitchedOff()
		{
			TestMessenger.AddSwitchListener("LevelLoaded", SwitchedOnCallback, SwitchedOffCallback);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback."));
		}

		[Test]
		public void Switch_CallbackInstantlyInvoked_InitiallySwitchedOn()
		{
			TestMessenger.SwitchOn("LevelLoaded");
			TestMessenger.AddSwitchListener("LevelLoaded", SwitchedOnCallback, SwitchedOffCallback);
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));
		}

		[Test]
		public void Switch_EmittingSwitchedOffAtFirstWontCallTheOffCallback()
		{
			TestMessenger.AddSwitchListener("LevelLoaded", SwitchedOnCallback, SwitchedOffCallback);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback."));

			TestMessenger.SwitchOff("LevelLoaded");
			AssertExpectNoLogs();
		}

		[Test]
		public void Switch_ConsecutiveSwitchingWontTriggerCallbacks()
		{
			RegisterSwitchCallbacks();

			TestMessenger.SwitchOff("LevelLoaded");
			TestMessenger.SwitchOff("LevelLoaded");
			TestMessenger.SwitchOff("LevelLoaded");
			TestMessenger.SwitchOff("LevelLoaded");
			AssertExpectNoLogs();

			TestMessenger.SwitchOn("LevelLoaded");
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));

			TestMessenger.SwitchOn("LevelLoaded");
			TestMessenger.SwitchOn("LevelLoaded");
			TestMessenger.SwitchOn("LevelLoaded");
			TestMessenger.SwitchOn("LevelLoaded");
			AssertExpectNoLogs();
		}

		#endregion

		#region General

		private void RegisterSwitchCallbacks(int order = 0, ListenerLifeSpan lifeSpan = ListenerLifeSpan.Permanent, Object lifeSpanTarget = null)
		{
			TestMessenger.AddSwitchListener("LevelLoaded", SwitchedOnCallback, SwitchedOffCallback, order, lifeSpan, lifeSpanTarget);

			if (TestMessenger.GetSwitch("LevelLoaded"))
			{
				AssertExpectLog((LogType.Log, "Called SwitchOn callback."));
			}
			else
			{
				AssertExpectLog((LogType.Log, "Called SwitchOff callback."));
			}
		}

		private void SwitchedOnCallback()
		{
			Log.Info("Called SwitchOn callback.");
		}

		private void SwitchedOffCallback()
		{
			Log.Info("Called SwitchOff callback.");
		}

		#endregion
	}

}

#endif
