/*
using UnityEngine;
using Extenity.MessagingToolbox;

namespace Extenity.Messaging
{

	public class TEST_MessengerGarbageCollection_Listener : MonoBehaviour
	{
		protected void Awake()
		{
			Debug.Log("LISTENER :: Awake", this);
			Messenger.Global.AddListener(1001, MyMessageHandler);
		}

		protected void OnDestroy()
		{
			Debug.Log("LISTENER :: OnDestroy", this);

			// We may call RemoveListener here but whole point of this messaging system is not requiring to do so.
			// MessageHandler should not be called anymore when this MonoBehaviour gets destroyed.
			//Messenger.Global.RemoveListener(1001, MyMessageHandler);
		}

		private void MyMessageHandler(string text)
		{
			Debug.Log("LISTENER :: MessageHandler    text: " + text, this);

			if (text.Contains("should NOT be delivered to MyMessageHandler"))
			{
				Debug.LogWarning("Well, we have an unexpected message.", this);
			}
		}
	}

}
*/
