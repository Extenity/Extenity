using System;
using UnityEngine;
using Logger = Extenity.Logger;

namespace ExtenityTests.DataToolbox
{

	public class Test_NamingToolsTestComponent : MonoBehaviour
	{
		public Action SomeDelegate;

		public void SomeMethod()
		{
			Log.Info("Called SomeMethod.");
		}

		#region Log

		private static readonly Logger Log = new("Test");

		#endregion
	}

}
