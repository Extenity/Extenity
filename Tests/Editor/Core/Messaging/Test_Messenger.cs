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
		#region Switch Basics

		[Test]
		public void Switch_AlrightToRegisterToNotYetEmittedSwitch()
		{
			// It's alright to register to an unknown Switch.
			TestMessenger.AddSwitchListener("LevelLoaded", null, null);
		}

		[Test]
		public void Switch_AlrightToEmitOnInBlank()
		{
			TestMessenger.EmitSwitchOn("LevelLoaded");
		}

		[Test]
		public void Switch_AlrightToEmitOffInBlank()
		{
			TestMessenger.EmitSwitchOff("LevelLoaded");
		}

		[Test]
		public void Switch_InitiallyConsideredSwitchedOff()
		{
			var isSwitchedOn = TestMessenger.GetSwitch("LevelLoaded");
			Assert.That(isSwitchedOn, Is.False);
		}

		#endregion

		#region Switch Emitting

		[Test]
		public void Switch_All_InitiallySwitchedOff()
		{
			TestMessenger.AddSwitchListener("LevelLoaded", SwitchedOnCallback, SwitchedOffCallback, SwitchCallbackExpectation.All);

			// Callback is immediately called whether the switch is on or off.
			AssertExpectLog((LogType.Log, "Called SwitchOff callback."));

			TestMessenger.EmitSwitchOn("LevelLoaded");
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));

			TestMessenger.EmitSwitchOff("LevelLoaded");
			AssertExpectLog((LogType.Log, "Called SwitchOff callback."));

			TestMessenger.EmitSwitchOn("LevelLoaded");
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));
		}

		[Test]
		public void Switch_All_InitiallySwitchedOn()
		{
			TestMessenger.EmitSwitchOn("LevelLoaded");
			TestMessenger.AddSwitchListener("LevelLoaded", SwitchedOnCallback, SwitchedOffCallback, SwitchCallbackExpectation.All);

			// Callback is immediately called whether the switch is on or off.
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));

			TestMessenger.EmitSwitchOff("LevelLoaded");
			AssertExpectLog((LogType.Log, "Called SwitchOff callback."));

			TestMessenger.EmitSwitchOn("LevelLoaded");
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));

			TestMessenger.EmitSwitchOff("LevelLoaded");
			AssertExpectLog((LogType.Log, "Called SwitchOff callback."));
		}

		[Test]
		public void Switch_EndsAfterRemovingListener()
		{
			TestMessenger.AddSwitchListener("LevelLoaded", SwitchedOnCallback, SwitchedOffCallback, SwitchCallbackExpectation.All);
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));
			TestMessenger.RemoveSwitchListener("LevelLoaded", SwitchedOnCallback, SwitchedOffCallback);

			TestMessenger.EmitSwitchOn("LevelLoaded");
			TestMessenger.EmitSwitchOff("LevelLoaded");
			TestMessenger.EmitSwitchOn("LevelLoaded");
			TestMessenger.EmitSwitchOff("LevelLoaded");
			AssertExpectNoLogs();
		}

		#endregion

		#region Switch Emitting - Edge Cases

		[Test]
		public void Switch_EmittingSwitchedOffAtFirstWontCallTheOffCallback()
		{
			TestMessenger.AddSwitchListener("LevelLoaded", SwitchedOnCallback, SwitchedOffCallback, SwitchCallbackExpectation.All);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback."));

			TestMessenger.EmitSwitchOff("LevelLoaded");
			AssertExpectNoLogs();
		}

		[Test]
		public void Switch_ConsecutiveSwitchingWontTriggerCallbacks()
		{
			RegisterSwitchCallbacks();

			TestMessenger.EmitSwitchOff("LevelLoaded");
			TestMessenger.EmitSwitchOff("LevelLoaded");
			TestMessenger.EmitSwitchOff("LevelLoaded");
			TestMessenger.EmitSwitchOff("LevelLoaded");
			AssertExpectNoLogs();

			TestMessenger.EmitSwitchOn("LevelLoaded");
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));

			TestMessenger.EmitSwitchOn("LevelLoaded");
			TestMessenger.EmitSwitchOn("LevelLoaded");
			TestMessenger.EmitSwitchOn("LevelLoaded");
			TestMessenger.EmitSwitchOn("LevelLoaded");
			AssertExpectNoLogs();
		}

		#endregion

		#region Switch Callback Expectation Modes

		[Test]
		public void Switch_CallbackExpectation_ForTheFirstOnCall_InitiallySwitchedOff()
		{
			TestMessenger.AddSwitchListener("LevelLoaded", SwitchedOnCallback, SwitchedOffCallback, SwitchCallbackExpectation.ForTheFirstOnCall);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback."));

			// The callback will be deregistered after this.
			TestMessenger.EmitSwitchOn("LevelLoaded");
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));

			TestMessenger.EmitSwitchOff("LevelLoaded");
			AssertExpectNoLogs();
		}

		[Test]
		public void Switch_CallbackExpectation_ForTheFirstOnCall_InitiallySwitchedOn()
		{
			TestMessenger.EmitSwitchOn("LevelLoaded");

			// The callback won't be registered at all. It will immediately be called and that's all.
			TestMessenger.AddSwitchListener("LevelLoaded", SwitchedOnCallback, SwitchedOffCallback, SwitchCallbackExpectation.ForTheFirstOnCall);
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));

			TestMessenger.EmitSwitchOff("LevelLoaded");
			AssertExpectNoLogs();
		}

		[Test]
		public void Switch_CallbackExpectation_ForTheFirstOffCall_InitiallySwitchedOn()
		{
			TestMessenger.EmitSwitchOn("LevelLoaded");
			TestMessenger.AddSwitchListener("LevelLoaded", SwitchedOnCallback, SwitchedOffCallback, SwitchCallbackExpectation.ForTheFirstOffCall);
			AssertExpectLog((LogType.Log, "Called SwitchOn callback."));

			// The callback will be deregistered after this.
			TestMessenger.EmitSwitchOff("LevelLoaded");
			AssertExpectLog((LogType.Log, "Called SwitchOff callback."));

			TestMessenger.EmitSwitchOn("LevelLoaded");
			AssertExpectNoLogs();
		}

		[Test]
		public void Switch_CallbackExpectation_ForTheFirstOffCall_InitiallySwitchedOff()
		{
			// The callback won't be registered at all. It will immediately be called and that's all.
			TestMessenger.AddSwitchListener("LevelLoaded", SwitchedOnCallback, SwitchedOffCallback, SwitchCallbackExpectation.ForTheFirstOffCall);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback."));

			TestMessenger.EmitSwitchOn("LevelLoaded");
			AssertExpectNoLogs();
		}

		#endregion

		#region Switch Callback Order

		[Test]
		public void Switch_CallbackOrder()
		{
			TestMessenger.AddSwitchListener("LevelLoaded",
			                               () =>
			                               {
				                               Log.Info("Called SwitchOn callback with order 60.");
			                               },
			                               () =>
			                               {
				                               Log.Info("Called SwitchOff callback with order 60.");
			                               },
			                               SwitchCallbackExpectation.All, 60);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback with order 60."));

			TestMessenger.AddSwitchListener("LevelLoaded",
			                               () =>
			                               {
				                               Log.Info("Called SwitchOn callback with order -40.");
			                               },
			                               () =>
			                               {
				                               Log.Info("Called SwitchOff callback with order -40.");
			                               },
			                               SwitchCallbackExpectation.All, -40);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback with order -40."));

			TestMessenger.AddSwitchListener("LevelLoaded",
			                               () =>
			                               {
				                               Log.Info("Called SwitchOn callback with default order, added first.");
			                               },
			                               () =>
			                               {
				                               Log.Info("Called SwitchOff callback with default order, added first.");
			                               },
			                               SwitchCallbackExpectation.All);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback with default order, added first."));

			TestMessenger.AddSwitchListener("LevelLoaded",
			                                () =>
			                                {
				                                Log.Info("Called SwitchOn callback with default order, added second.");
			                                },
			                                () =>
			                                {
				                                Log.Info("Called SwitchOff callback with default order, added second.");
			                                },
			                                SwitchCallbackExpectation.All);
			AssertExpectLog((LogType.Log, "Called SwitchOff callback with default order, added second."));

			TestMessenger.EmitSwitchOn("LevelLoaded");
			AssertExpectLog((LogType.Log, "Called SwitchOn callback with order -40."));
			AssertExpectLog((LogType.Log, "Called SwitchOn callback with default order, added first."));
			AssertExpectLog((LogType.Log, "Called SwitchOn callback with default order, added second."));
			AssertExpectLog((LogType.Log, "Called SwitchOn callback with order 60."));

			TestMessenger.EmitSwitchOff("LevelLoaded");
			AssertExpectLog((LogType.Log, "Called SwitchOff callback with order -40."));
			AssertExpectLog((LogType.Log, "Called SwitchOn callback with default order, added first."));
			AssertExpectLog((LogType.Log, "Called SwitchOn callback with default order, added second."));
			AssertExpectLog((LogType.Log, "Called SwitchOff callback with order 60."));
		}

		#endregion

		#region General

		private void RegisterSwitchCallbacks(SwitchCallbackExpectation switchCallbackExpectation = SwitchCallbackExpectation.All)
		{
			TestMessenger.AddSwitchListener("LevelLoaded", SwitchedOnCallback, SwitchedOffCallback, switchCallbackExpectation);

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