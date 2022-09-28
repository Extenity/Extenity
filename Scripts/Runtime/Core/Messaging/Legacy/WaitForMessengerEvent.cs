#if ExtenityMessenger && UseLegacyMessenger

using UnityEngine;

namespace Extenity.MessagingToolbox
{

	public class WaitForMessengerEvent : CustomYieldInstruction
	{
		private string EventName;

		public WaitForMessengerEvent(string eventName)
		{
			EventName = eventName;
			_KeepWaiting = true;
			Messenger.RegisterEvent(eventName, OnEventEmitted);
		}

		private void OnEventEmitted()
		{
			Messenger.DeregisterEvent(EventName, OnEventEmitted);
			_KeepWaiting = false;
		}

		private bool _KeepWaiting;
		public override bool keepWaiting
		{
			get => _KeepWaiting;
		}
	}

}

#endif
