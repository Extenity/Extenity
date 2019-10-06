using System;
using UnityEngine;

namespace ExtenityTests.MessagingToolbox
{

	public class Test_ExtenitySwitchSubject : MonoBehaviour
	{
		// @formatter:on
		public void CallbackOn()   { Log.Info("Called Subject SwitchOn callback.");    }
		public void CallbackOnA()  { Log.Info("Called Subject SwitchOn callback A.");  }
		public void CallbackOnB()  { Log.Info("Called Subject SwitchOn callback B.");  }
		public void CallbackOnC()  { Log.Info("Called Subject SwitchOn callback C.");  }
		public void CallbackOnD()  { Log.Info("Called Subject SwitchOn callback D.");  }
		public void CallbackOnE()  { Log.Info("Called Subject SwitchOn callback E.");  }
		public void CallbackOnF()  { Log.Info("Called Subject SwitchOn callback F.");  }
		public void CallbackOff()  { Log.Info("Called Subject SwitchOff callback.");   }
		public void CallbackOffA() { Log.Info("Called Subject SwitchOff callback A."); }
		public void CallbackOffB() { Log.Info("Called Subject SwitchOff callback B."); }
		public void CallbackOffC() { Log.Info("Called Subject SwitchOff callback C."); }
		public void CallbackOffD() { Log.Info("Called Subject SwitchOff callback D."); }
		public void CallbackOffE() { Log.Info("Called Subject SwitchOff callback E."); }
		public void CallbackOffF() { Log.Info("Called Subject SwitchOff callback F."); }
		public void ThrowingCallback() { throw new Test_ExtenityEventException("Called Subject throwing callback."); }

		public void CallbackOnAAndDestroySubject()  { CallbackOnA();  DestroyTestSwitchSubject(); }
		public void CallbackOnBAndDestroySubject()  { CallbackOnB();  DestroyTestSwitchSubject(); }
		public void CallbackOnCAndDestroySubject()  { CallbackOnC();  DestroyTestSwitchSubject(); }
		public void CallbackOffAAndDestroySubject() { CallbackOffA(); DestroyTestSwitchSubject(); }
		public void CallbackOffBAndDestroySubject() { CallbackOffB(); DestroyTestSwitchSubject(); }
		public void CallbackOffCAndDestroySubject() { CallbackOffC(); DestroyTestSwitchSubject(); }
		// @formatter:off

		public Action DestroyTestSwitchSubject;
	}

}
