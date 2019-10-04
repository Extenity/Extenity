using UnityEngine;

namespace ExtenityTests.MessagingToolbox
{

	public class Test_ExtenityEventSubject : MonoBehaviour
	{
		public void Callback()
		{
			Log.Info("Called Subject callback.");
		}
	}

}
