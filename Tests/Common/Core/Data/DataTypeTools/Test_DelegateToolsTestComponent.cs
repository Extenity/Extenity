using System;
using Extenity;
using UnityEngine;

namespace ExtenityTests.DataToolbox
{

	public class Test_DelegateToolsTestComponent : MonoBehaviour
	{
		public Action SomeDelegate;

		public void SomeMethod()
		{
			Log.Info("Called SomeMethod.");
		}
	}

}
