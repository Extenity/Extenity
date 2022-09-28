#if ExtenityScreenManagement

using System;
using Extenity.MessagingToolbox;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Extenity.UIToolbox
{

	[RequireComponent(typeof(Button))]
	public class ButtonMessenger : PanelMonoBehaviour
	{
		#region Configuration

		[ValueDropdown(nameof(EventNames))]
		public string EventName;
		public EmitTiming EmitTiming = EmitTiming.EmitOnRelease;

		private static string[] EventNames => Messenger.EventNames;

		#endregion

		#region Links

		private Button _Button;
		public Button Button
		{
			get
			{
				if (!_Button)
				{
					_Button = GetComponent<Button>();
					if (!_Button)
					{
						throw new Exception($"{nameof(ButtonMessenger)} requires a {nameof(Button)} to work.");
					}
				}
				return _Button;
			}
		}

		private EventTrigger _EventTrigger;
		public EventTrigger EventTrigger
		{
			get
			{
				if (!_EventTrigger)
				{
					_EventTrigger = GetComponent<EventTrigger>();
					if (!_EventTrigger)
					{
						_EventTrigger = gameObject.AddComponent<EventTrigger>();
					}
				}
				return _EventTrigger;
			}
		}

		#endregion

		#region Register To Button Click

		protected internal override void OnAfterBecameVisible()
		{
			switch (EmitTiming)
			{
				case EmitTiming.EmitOnRelease:
				{
					Button.onClick.AddListener(EmitEvent);
					break;
				}

				case EmitTiming.EmitOnPress:
				{
					Button.RegisterToEvent(EventTriggerType.PointerDown, EmitEvent);
					break;
				}

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		protected internal override void OnBeforeBecomingInvisible()
		{
			switch (EmitTiming)
			{
				case EmitTiming.EmitOnRelease:
				{
					Button.onClick.RemoveListener(EmitEvent);
					break;
				}

				case EmitTiming.EmitOnPress:
				{
					Button.DeregisterFromEvent(EventTriggerType.PointerDown, EmitEvent);
					break;
				}

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		#endregion

		#region Emit

		private void EmitEvent(BaseEventData _)
		{
			Messenger.EmitEvent(EventName);
		}

		private void EmitEvent()
		{
			Messenger.EmitEvent(EventName);
		}

		#endregion
	}

}

#endif
