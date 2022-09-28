#if ExtenityScreenManagement

using System;
using Extenity.MessagingToolbox;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Extenity.UIToolbox
{

	[RequireComponent(typeof(Toggle))]
	public class ToggleMessenger : PanelMonoBehaviour
	{
		#region Configuration

		[ValueDropdown(nameof(EventNames))]
		public string ToggleOnEventName;
		[ValueDropdown(nameof(EventNames))]
		public string ToggleOffEventName;
		public EmitTiming EmitTiming = EmitTiming.EmitOnRelease;

		private static string[] EventNames => Messenger.EventNames;

		#endregion

		#region Links

		private Toggle _Toggle;
		public Toggle Toggle
		{
			get
			{
				if (!_Toggle)
				{
					_Toggle = GetComponent<Toggle>();
					if (!_Toggle)
					{
						throw new Exception($"{nameof(ToggleMessenger)} requires a {nameof(Toggle)} to work.");
					}
				}
				return _Toggle;
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

		#region Register To Toggle Click

		protected internal override void OnAfterBecameVisible()
		{
			switch (EmitTiming)
			{
				case EmitTiming.EmitOnRelease:
				{
					Toggle.onValueChanged.AddListener(EmitEvent);
					break;
				}

				case EmitTiming.EmitOnPress:
				{
					Toggle.RegisterToEvent(EventTriggerType.PointerDown, EmitEvent);
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
					Toggle.onValueChanged.RemoveListener(EmitEvent);
					break;
				}

				case EmitTiming.EmitOnPress:
				{
					Toggle.DeregisterFromEvent(EventTriggerType.PointerDown, EmitEvent);
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
			InternalEmitEvent();
		}

		private void EmitEvent(bool _)
		{
			InternalEmitEvent();
		}

		private void EmitEvent()
		{
			InternalEmitEvent();
		}

		private void InternalEmitEvent()
		{
			if (Toggle.isOn)
				Messenger.EmitEvent(ToggleOnEventName);
			else
				Messenger.EmitEvent(ToggleOffEventName);
		}

		#endregion
	}

}

#endif
