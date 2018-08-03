using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Extenity.UIToolbox
{

	public static class EventTools
	{
		public static void AddListener(this EventTrigger trigger, EventTriggerType eventType, UnityAction<BaseEventData> method)
		{
			var entry = new EventTrigger.Entry
			{
				eventID = eventType,
				callback = new EventTrigger.TriggerEvent()
			};
			entry.callback.AddListener(method);
			trigger.triggers.Add(entry);
		}
	}

}
