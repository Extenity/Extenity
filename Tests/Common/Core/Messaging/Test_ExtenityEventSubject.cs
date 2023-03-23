using System;
using UnityEngine;
using Logger = Extenity.Logger;

namespace ExtenityTests.MessagingToolbox
{

	public class Test_ExtenityEventSubject : MonoBehaviour
	{
		// @formatter:off
		public void Callback()   { Log.Info("Called callback.");    }
		public void CallbackA()  { Log.Info("Called callback A.");  }
		public void CallbackB()  { Log.Info("Called callback B.");  }
		public void CallbackC()  { Log.Info("Called callback C.");  }
		public void CallbackD()  { Log.Info("Called callback D.");  }
		public void CallbackE()  { Log.Info("Called callback E.");  }
		public void CallbackF()  { Log.Info("Called callback F.");  }
		public void ThrowingCallback() { throw new Test_ExtenityEventException("Called throwing callback."); }

		public void CallbackAAndDestroySubject()  { CallbackA();  DestroyTestEventSubject(); }
		public void CallbackBAndDestroySubject()  { CallbackB();  DestroyTestEventSubject(); }
		public void CallbackCAndDestroySubject()  { CallbackC();  DestroyTestEventSubject(); }
		// @formatter:on

		public Action DestroyTestEventSubject;

		#region Log

		private static readonly Logger Log = new("Test");

		#endregion
	}

}
