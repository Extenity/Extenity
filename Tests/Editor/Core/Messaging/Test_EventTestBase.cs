using System;
using Extenity.MessagingToolbox;
using Extenity.Testing;
using UnityEngine;

namespace ExtenityTests.MessagingToolbox
{

	public abstract class Test_EventTestBase : ExtenityTestBase
	{
		#region Deinitialization

		protected override void OnDeinitialize()
		{
			if (_TestEvent != null)
			{
				_TestEvent = null;
			}

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

		#region Life Span Target

		public GameObject LifeSpanTargetTestObject { get; private set; }

		public GameObject CreateLifeSpanTargetTestObject()
		{
			if (LifeSpanTargetTestObject)
				throw new Exception("The LifeSpanTarget test object was already created."); // There is something wrong with tests.

			LifeSpanTargetTestObject = new GameObject("LifeSpanTargetTestObject");
			return LifeSpanTargetTestObject;
		}

		public void DestroyLifeSpanTargetTestObject()
		{
			if (!LifeSpanTargetTestObject)
				throw new Exception("The LifeSpanTarget test object was not created."); // There is something wrong with tests.

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

		#endregion

		#region Subject

		public Test_ExtenityEventSubject TestEventSubject { get; private set; }

		public Test_ExtenityEventSubject CreateTestEventSubject()
		{
			if (TestEventSubject)
				throw new Exception("The TestEventSubject test object was already created."); // There is something wrong with tests.

			TestEventSubject = new GameObject("TestEventSubject").AddComponent<Test_ExtenityEventSubject>();
			return TestEventSubject;
		}

		public void DestroyTestEventSubject()
		{
			if (!TestEventSubject)
				throw new Exception("The TestEventSubject test object was not created."); // There is something wrong with tests.

			if (Application.isPlaying)
			{
				GameObject.Destroy(TestEventSubject.gameObject);
			}
			else
			{
				GameObject.DestroyImmediate(TestEventSubject.gameObject);
			}

			TestEventSubject = null;
		}

		#endregion
	}

}
