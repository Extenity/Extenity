using UnityEngine;

namespace ExtenityTests.MessagingToolbox
{

	public class Test_ExtenitySwitchSubject : MonoBehaviour
	{
		public void Callback()
		{
			Log.Info("Called Subject callback.");
		}
	}

}
