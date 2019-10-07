using System;
using UnityEngine;

namespace ExtenityTests.DataToolbox
{

	public class Test_NamingToolsTestComponent : MonoBehaviour
	{
		public Action SomeDelegate;

		public void SomeMethod()
		{
			Log.Info("Called SomeMethod.");
		}
	}

}
