#if ExtenityMessenger && !UseLegacyMessenger

using UnityEngine;
using Extenity.MessagingToolbox;

namespace Extenity.Messaging
{

	public class Example_MessengerGarbageCollection_Listener : MonoBehaviour
	{
		protected void Awake()
		{
			Log.Info("LISTENER :: Awake", this);
			Global.AddMessageListener("1001", MyMessageHandler);
		}

		protected void OnDestroy()
		{
			Log.Info("LISTENER :: OnDestroy", this);

			// We may call RemoveListener here but whole point of this messaging system is not requiring to do so.
			// MessageHandler should not be called anymore when this MonoBehaviour gets destroyed.
			//Messenger.Global.RemoveListener(1001, MyMessageHandler);
		}

		private void MyMessageHandler(string text)
		{
			Log.Info("LISTENER :: MessageHandler    text: " + text, this);

			if (text.Contains("should NOT be delivered to MyMessageHandler"))
			{
				Log.Warning("Well, we have an unexpected message.", this);
			}
		}

		private Messenger _Global;
		public Messenger Global
		{
			get
			{
				if (!_Global)
				{
					var go = new GameObject("_GlobalMessenger", typeof(Messenger));
					go.hideFlags = HideFlags.HideAndDontSave;
					_Global = go.GetComponent<Messenger>();
				}
				return _Global;
			}
		}
	}

}

#endif
