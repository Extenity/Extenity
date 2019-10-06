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

		private GameObject _LifeSpanTargetTestObject;
		public GameObject LifeSpanTargetTestObject
		{
			get
			{
				if (!_LifeSpanTargetTestObject)
				{
					throw new Exception("The LifeSpanTarget test object is missing.");
				}
				return _LifeSpanTargetTestObject;
			}
		}

		public GameObject CreateLifeSpanTargetTestObject()
		{
			if (_LifeSpanTargetTestObject)
				throw new Exception("The LifeSpanTarget test object was already created."); // There is something wrong with tests.

			_LifeSpanTargetTestObject = new GameObject("LifeSpanTargetTestObject");
			return _LifeSpanTargetTestObject;
		}

		public void DestroyLifeSpanTargetTestObject(bool ensureExists = true)
		{
			if (ensureExists && !_LifeSpanTargetTestObject)
				throw new Exception("The LifeSpanTarget test object was not created."); // There is something wrong with tests.

			if (_LifeSpanTargetTestObject)
			{
				if (Application.isPlaying)
				{
					GameObject.Destroy(_LifeSpanTargetTestObject);
				}
				else
				{
					GameObject.DestroyImmediate(_LifeSpanTargetTestObject);
				}

				_LifeSpanTargetTestObject = null;
			}
		}

		#endregion

		#region Subject

		private Test_ExtenitySwitchSubject _TestSwitchSubject;
		public Test_ExtenitySwitchSubject TestSwitchSubject
		{
			get
			{
				if (!_TestSwitchSubject)
				{
					throw new Exception("The TestSwitchSubject test object is missing.");
				}
				return _TestSwitchSubject;
			}
		}

		public Test_ExtenitySwitchSubject CreateTestSwitchSubject()
		{
			if (_TestSwitchSubject)
				throw new Exception("The TestSwitchSubject test object was already created."); // There is something wrong with tests.

			_TestSwitchSubject = new GameObject("TestSwitchSubject").AddComponent<Test_ExtenitySwitchSubject>();
			_TestSwitchSubject.DestroyTestSwitchSubject = DestroyTestSwitchSubject;
			return _TestSwitchSubject;
		}

		public void DestroyTestSwitchSubject()
		{
			DestroyTestSwitchSubject(true);
		}

		public void DestroyTestSwitchSubject(bool ensureExists)
		{
			if (ensureExists && !_TestSwitchSubject)
				throw new Exception("The TestSwitchSubject test object was not created."); // There is something wrong with tests.

			if (_TestSwitchSubject)
			{
				if (Application.isPlaying)
				{
					GameObject.Destroy(_TestSwitchSubject.gameObject);
				}
				else
				{
					GameObject.DestroyImmediate(_TestSwitchSubject.gameObject);
				}

				_TestSwitchSubject = null;
			}
		}

		#endregion
	}

}
