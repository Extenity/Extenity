using System;
using Extenity.MessagingToolbox;
using Extenity.Testing;
using UnityEngine;

namespace ExtenityTests.MessagingToolbox
{

	public abstract class Test_SwitchTestBase : ExtenityTestBase
	{
		#region Deinitialization

		protected override void OnDeinitialize()
		{
			if (_TestSwitch != null)
			{
				_TestSwitch = null;
			}

			DestroyLifeSpanTargetTestObject(false);
			DestroyTestSwitchSubject(false);

			base.OnDeinitialize();
		}

		#endregion

		#region Test Switch

		private ExtenitySwitch _TestSwitch;
		public ExtenitySwitch TestSwitch
		{
			get
			{
				if (_TestSwitch == null)
				{
					_TestSwitch = new ExtenitySwitch();
				}
				return _TestSwitch;
			}
		}

		#endregion

		#region Life Span Target

		public GameObject LifeSpanTargetTestObject { get; private set; }

		public GameObject CreateLifeSpanTargetTestObject()
		{
			if (LifeSpanTargetTestObject)
				throw new Exception("The LifeSpanTarget test object was already created."); // There is something wrong with tests.

			LifeSpanTargetTestObject = new GameObject("LifeSpanTargetTestObject");
			return LifeSpanTargetTestObject;
		}

		public void DestroyLifeSpanTargetTestObject(bool ensureExists = true)
		{
			if (ensureExists && !LifeSpanTargetTestObject)
				throw new Exception("The LifeSpanTarget test object was not created."); // There is something wrong with tests.

			if (LifeSpanTargetTestObject)
			{
				if (Application.isPlaying)
				{
					GameObject.Destroy(LifeSpanTargetTestObject);
				}
				else
				{
					GameObject.DestroyImmediate(LifeSpanTargetTestObject);
				}

				LifeSpanTargetTestObject = null;
			}
		}

		#endregion

		#region Subject

		public Test_ExtenitySwitchSubject TestSwitchSubject { get; private set; }

		public Test_ExtenitySwitchSubject CreateTestSwitchSubject()
		{
			if (TestSwitchSubject)
				throw new Exception("The TestSwitchSubject test object was already created."); // There is something wrong with tests.

			TestSwitchSubject = new GameObject("TestSwitchSubject").AddComponent<Test_ExtenitySwitchSubject>();
			return TestSwitchSubject;
		}

		public void DestroyTestSwitchSubject(bool ensureExists = true)
		{
			if (ensureExists && !TestSwitchSubject)
				throw new Exception("The TestSwitchSubject test object was not created."); // There is something wrong with tests.

			if (TestSwitchSubject)
			{
				if (Application.isPlaying)
				{
					GameObject.Destroy(TestSwitchSubject.gameObject);
				}
				else
				{
					GameObject.DestroyImmediate(TestSwitchSubject.gameObject);
				}

				TestSwitchSubject = null;
			}
		}

		#endregion
	}

}
