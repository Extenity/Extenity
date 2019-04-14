using System;
using System.Collections;
using Extenity.FlowToolbox;
using Extenity.UnityTestToolbox;
using UnityEngine;

namespace ExtenityTests.FlowToolbox
{

	public class Test_FastInvokeSubject : MonoBehaviour
	{
		#region Initialization

		private void Awake()
		{
			ResetCallback();
		}

		#endregion

		#region Callback

		public int CallbackCallCount;

		public void ResetCallback()
		{
			CallbackCallCount = 0;
		}

		public void Callback()
		{
			CallbackCallCount++;
		}

		#endregion
	}

	public abstract class TestBase_FastInvoke
	{
		#region Test Initialization / Deinitialization

		protected bool IsInitialized;

		protected IEnumerator InitializeTest(bool startAtRandomTime)
		{
			if (!InitializeBase())
				yield break;

			if (startAtRandomTime)
			{
				// This will make tests start at a random Time.time.
				yield return new WaitForEndOfFrame(); // Ignored by Code Correct
			}
			else
			{
				// This will make tests start right in FixedUpdates where Time.time is consistent.
				yield return new WaitForFixedUpdate(); // Ignored by Code Correct
			}
		}

		private bool InitializeBase(float timeScale = 100f)
		{
			if (IsInitialized)
			{
				// Deinitialize first
				DeinitializeBase();
			}
			IsInitialized = true;

			Invoker.ResetSystem();
			//if (Invoker.IsFastInvokingAny())
			//{
			//	Assert.Fail("Left a previously set invoke already in action while starting to a new test.");
			//	return false;
			//}

			UnityTestTools.Cleanup();
			CreateSubject();
			//Subject.ResetCallback(); No need to call, but left this line here commented out for convenience.
			ResetOutsiderCallback();

			Time.timeScale = timeScale;
			return true;
		}

		private void DeinitializeBase()
		{
			if (!IsInitialized)
				throw new Exception("Test was not initialized.");
			IsInitialized = false;

			UnityTestTools.Cleanup();
			Invoker.ShutdownSystem();
			Time.timeScale = 1f;
		}

		#endregion

		#region Test Subject

		private Test_FastInvokeSubject _Subject;
		protected Test_FastInvokeSubject Subject
		{
			get
			{
				if (!IsInitialized)
				{
					throw new Exception("Test was not initialized.");
				}
				return _Subject;
			}
		}

		private void CreateSubject()
		{
			if (_Subject)
			{
				throw new Exception(); // Subject should already be destroyed by now.
			}

			var go = new GameObject("FastInvoke test object");
			_Subject = go.AddComponent<Test_FastInvokeSubject>();
		}

		#endregion

		#region Callback Outside Of Behaviour

		protected int OutsiderCallbackCallCount;

		private void ResetOutsiderCallback()
		{
			OutsiderCallbackCallCount = 0;
		}

		protected void FastInvokeOutsiderCallback()
		{
			OutsiderCallbackCallCount++;
		}

		#endregion
	}

}
