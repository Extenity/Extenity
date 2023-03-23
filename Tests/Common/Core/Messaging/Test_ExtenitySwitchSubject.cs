using System;
using UnityEngine;
using Logger = Extenity.Logger;

namespace ExtenityTests.MessagingToolbox
{

	public class Test_ExtenitySwitchSubject : MonoBehaviour
	{
		// @formatter:off
		public void CallbackOn()   { Log.Info("Called SwitchOn callback.");    }
		public void CallbackOnA()  { Log.Info("Called SwitchOn callback A.");  }
		public void CallbackOnB()  { Log.Info("Called SwitchOn callback B.");  }
		public void CallbackOnC()  { Log.Info("Called SwitchOn callback C.");  }
		public void CallbackOnD()  { Log.Info("Called SwitchOn callback D.");  }
		public void CallbackOnE()  { Log.Info("Called SwitchOn callback E.");  }
		public void CallbackOnF()  { Log.Info("Called SwitchOn callback F.");  }
		public void CallbackOff()  { Log.Info("Called SwitchOff callback.");   }
		public void CallbackOffA() { Log.Info("Called SwitchOff callback A."); }
		public void CallbackOffB() { Log.Info("Called SwitchOff callback B."); }
		public void CallbackOffC() { Log.Info("Called SwitchOff callback C."); }
		public void CallbackOffD() { Log.Info("Called SwitchOff callback D."); }
		public void CallbackOffE() { Log.Info("Called SwitchOff callback E."); }
		public void CallbackOffF() { Log.Info("Called SwitchOff callback F."); }
		public void ThrowingCallback() { throw new Test_ExtenityEventException("Called throwing callback."); }

		public void CallbackOnAAndDestroySubject()  { CallbackOnA();  DestroyTestSwitchSubject(); }
		public void CallbackOnBAndDestroySubject()  { CallbackOnB();  DestroyTestSwitchSubject(); }
		public void CallbackOnCAndDestroySubject()  { CallbackOnC();  DestroyTestSwitchSubject(); }
		public void CallbackOffAAndDestroySubject() { CallbackOffA(); DestroyTestSwitchSubject(); }
		public void CallbackOffBAndDestroySubject() { CallbackOffB(); DestroyTestSwitchSubject(); }
		public void CallbackOffCAndDestroySubject() { CallbackOffC(); DestroyTestSwitchSubject(); }
		// @formatter:on

		public Action DestroyTestSwitchSubject;

		#region Log

		private static readonly Logger Log = new("Test");

		#endregion
	}

}
