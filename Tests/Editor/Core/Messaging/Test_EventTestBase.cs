using System;
using Extenity.MessagingToolbox;
using Extenity.Testing;
using UnityEngine;

namespace ExtenityTests.MessagingToolbox
{

	public abstract class Test_EventTestBase : ExtenityTestBase
	{
		#region Initialization

		public Test_EventTestBase(bool usingUnsafe)
		{
			UsingUnsafe = usingUnsafe;
		}

		#endregion

		#region Deinitialization

		protected override void OnDeinitialize()
		{
			if (_TestEvent != null)
			{
				_TestEvent = null;
			}

			DestroyLifeSpanTargetTestObject(false);
			DestroyTestEventSubject(false);

			base.OnDeinitialize();
		}

		#endregion

		#region Test Event

		private ExtenityEvent _TestEvent;
		public ExtenityEvent TestEvent
		{
			get
			{
				if (_TestEvent == null)
				{
					_TestEvent = new ExtenityEvent();
				}
				return _TestEvent;
			}
		}

		#endregion

		#region Test Event Wrapper

		protected bool UsingUnsafe = false;

		protected void Invoke()
		{
			if (UsingUnsafe)
			{
				TestEvent.InvokeUnsafe();
			}
			else
			{
				TestEvent.InvokeSafe();
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

		private Test_ExtenityEventSubject _TestEventSubject;
		public Test_ExtenityEventSubject TestEventSubject
		{
			get
			{
				if (!_TestEventSubject)
				{
					throw new Exception("The TestEventSubject test object is missing.");
				}
				return _TestEventSubject;
			}
		}

		public Test_ExtenityEventSubject CreateTestEventSubject()
		{
			if (_TestEventSubject)
				throw new Exception("The TestEventSubject test object was already created."); // There is something wrong with tests.

			_TestEventSubject = new GameObject("TestEventSubject").AddComponent<Test_ExtenityEventSubject>();
			_TestEventSubject.DestroyTestEventSubject = DestroyTestEventSubject;
			return _TestEventSubject;
		}

		public void DestroyTestEventSubject()
		{
			DestroyTestEventSubject(true);
		}

		public void DestroyTestEventSubject(bool ensureExists)
		{
			if (ensureExists && !_TestEventSubject)
				throw new Exception("The TestEventSubject test object was not created."); // There is something wrong with tests.

			if (_TestEventSubject)
			{
				if (Application.isPlaying)
				{
					GameObject.Destroy(_TestEventSubject.gameObject);
				}
				else
				{
					GameObject.DestroyImmediate(_TestEventSubject.gameObject);
				}

				_TestEventSubject = null;
			}
		}

		#endregion
	}

}
