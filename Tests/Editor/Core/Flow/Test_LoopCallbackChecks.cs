using System;
using Extenity.FlowToolbox;
using Extenity.Testing;
using NUnit.Framework;

namespace ExtenityTests.FlowToolbox
{

	public class Test_LoopCallbackChecks : ExtenityTestBase
	{
		#region Snapshot

		[Test]
		public void Snapshot_PassesWhenCallbacksAreUnchanged()
		{
			Loop.RegisterPreUpdate(PreExistingCallback);
			try
			{
				var snapshot = Loop.CaptureCallbackSnapshot();
				Assert.AreEqual(1, snapshot.TotalCallbackCount);

				Assert.DoesNotThrow(() => Loop.EnsureCallbacksMatchSnapshot(snapshot));
			}
			finally
			{
				Loop.DeregisterPreUpdate(PreExistingCallback);
			}
		}

		[Test]
		public void Snapshot_PassesWhenACallbackIsAddedAndRemovedAgain()
		{
			Loop.RegisterPreUpdate(PreExistingCallback);
			try
			{
				var snapshot = Loop.CaptureCallbackSnapshot();
				Loop.RegisterUpdate(AddedCallback);
				Loop.DeregisterUpdate(AddedCallback);

				Assert.DoesNotThrow(() => Loop.EnsureCallbacksMatchSnapshot(snapshot));
			}
			finally
			{
				Loop.DeregisterPreUpdate(PreExistingCallback);
			}
		}

		[Test]
		public void Snapshot_CatchesAnAddedCallback()
		{
			Loop.RegisterPreUpdate(PreExistingCallback);
			try
			{
				var snapshot = Loop.CaptureCallbackSnapshot();
				Loop.RegisterPreUpdate(AddedCallback);
				try
				{
					var exception = Assert.Throws<Exception>(() => Loop.EnsureCallbacksMatchSnapshot(snapshot));
					StringAssert.Contains($"Added to PreUpdate: {typeof(Test_LoopCallbackChecks).FullName}.{nameof(AddedCallback)}", exception.Message);
					StringAssert.Contains("in 1 place(s)", exception.Message);
				}
				finally
				{
					Loop.DeregisterPreUpdate(AddedCallback);
				}
			}
			finally
			{
				Loop.DeregisterPreUpdate(PreExistingCallback);
			}
		}

		[Test]
		public void Snapshot_CatchesARemovedPreExistingCallback()
		{
			Loop.RegisterPreUpdate(PreExistingCallback);
			Loop.RegisterLateUpdate(AnotherPreExistingCallback);
			try
			{
				var snapshot = Loop.CaptureCallbackSnapshot();
				Loop.DeregisterLateUpdate(AnotherPreExistingCallback);

				var exception = Assert.Throws<Exception>(() => Loop.EnsureCallbacksMatchSnapshot(snapshot));
				StringAssert.Contains($"Removed from LateUpdate: {typeof(Test_LoopCallbackChecks).FullName}.{nameof(AnotherPreExistingCallback)}", exception.Message);
				StringAssert.Contains("in 1 place(s)", exception.Message);
			}
			finally
			{
				Loop.DeregisterPreUpdate(PreExistingCallback);
				Loop.DeregisterLateUpdate(AnotherPreExistingCallback);
			}
		}

		[Test]
		public void Snapshot_CatchesACallbackMovedToAnotherList()
		{
			Loop.RegisterPreUpdate(PreExistingCallback);
			try
			{
				var snapshot = Loop.CaptureCallbackSnapshot();
				Loop.DeregisterPreUpdate(PreExistingCallback);
				Loop.RegisterPostUpdate(PreExistingCallback);

				var exception = Assert.Throws<Exception>(() => Loop.EnsureCallbacksMatchSnapshot(snapshot));
				StringAssert.Contains($"Removed from PreUpdate: {typeof(Test_LoopCallbackChecks).FullName}.{nameof(PreExistingCallback)}", exception.Message);
				StringAssert.Contains($"Added to PostUpdate: {typeof(Test_LoopCallbackChecks).FullName}.{nameof(PreExistingCallback)}", exception.Message);
			}
			finally
			{
				Loop.DeregisterPreUpdate(PreExistingCallback);
				Loop.DeregisterPostUpdate(PreExistingCallback);
			}
		}

		#endregion

		#region No Callbacks Allowed

		[Test]
		public void EnsureAllCallbacksDeregistered_NamesTheLeftoverCallback()
		{
			Loop.RegisterPreUpdate(PreExistingCallback);
			try
			{
				var exception = Assert.Throws<Exception>(Loop.EnsureAllCallbacksDeregistered);
				StringAssert.Contains($"PreUpdate: {typeof(Test_LoopCallbackChecks).FullName}.{nameof(PreExistingCallback)}", exception.Message);
			}
			finally
			{
				Loop.DeregisterPreUpdate(PreExistingCallback);
			}
		}

		/// <summary>
		/// Outside Play Mode, the test base stays strict: SetUp refuses to start while any Loop callback is
		/// registered, even one that was registered before the test. And TearDown after such a failed SetUp does
		/// not throw secondary errors that would hide the real one.
		/// </summary>
		[Test]
		public void TestBase_OutsidePlayMode_RefusesAnyRegisteredCallback()
		{
			Assert.IsFalse(UnityEngine.Application.isPlaying, "This test expects to run in Edit Mode.");

			var otherTest = new EmptyTest();
			Loop.RegisterPreUpdate(PreExistingCallback);
			try
			{
				var exception = Assert.Throws<Exception>(otherTest.Initialize);
				StringAssert.Contains($"PreUpdate: {typeof(Test_LoopCallbackChecks).FullName}.{nameof(PreExistingCallback)}", exception.Message);
				Assert.IsFalse(otherTest.IsOnInitializeCalled);

				Assert.DoesNotThrow(otherTest.Deinitialize);
				Assert.IsFalse(otherTest.IsOnDeinitializeCalled, "TearDown should skip the derived class' cleanup when SetUp failed before its initialization.");
			}
			finally
			{
				Loop.DeregisterPreUpdate(PreExistingCallback);
			}
		}

		private class EmptyTest : ExtenityTestBase
		{
			public bool IsOnInitializeCalled;
			public bool IsOnDeinitializeCalled;

			protected override void OnInitialize()
			{
				IsOnInitializeCalled = true;
			}

			protected override void OnDeinitialize()
			{
				IsOnDeinitializeCalled = true;
			}
		}

		#endregion

		#region Callbacks

		private static void PreExistingCallback()
		{
		}

		private static void AnotherPreExistingCallback()
		{
		}

		private static void AddedCallback()
		{
		}

		#endregion
	}

}
