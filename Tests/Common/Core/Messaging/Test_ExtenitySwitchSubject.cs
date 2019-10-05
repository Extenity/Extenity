using UnityEngine;

namespace ExtenityTests.MessagingToolbox
{

	public class Test_ExtenitySwitchSubject : MonoBehaviour
	{
		public void CallbackOn()
		{
			Log.Info("Called Subject SwitchOn callback.");
		}

		public void CallbackOff()
		{
			Log.Info("Called Subject SwitchOff callback.");
		}
	}

}
