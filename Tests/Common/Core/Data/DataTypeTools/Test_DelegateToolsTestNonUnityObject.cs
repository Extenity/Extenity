using System;
using Extenity;

namespace ExtenityTests.DataToolbox
{

	public class Test_DelegateToolsTestNonUnityObject
	{
		public Action SomeDelegate;

		public void SomeMethod()
		{
			Log.Info("Called SomeMethod.");
		}
	}

}
