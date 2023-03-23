#if ExtenityAudio

using System;
using Extenity.Audio;
using Extenity.DataToolbox;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Extenity.UIToolbox.Audio
{

	public enum ButtonClickSoundAction
	{
		Up,
		Down,
	}

	public class ButtonClickSound : MonoBehaviour
	{
		public Button Button;
		public Toggle Toggle;
		[FormerlySerializedAs("EventName")]
		public string SoundEvent;
		public ButtonClickSoundAction Action = ButtonClickSoundAction.Up;

		public static readonly Type[] SupportedTypes = new[]
		{
			typeof(Button),
			typeof(Toggle)
		};

		protected void OnEnable()
		{
			// Wow! RegisterEvents does not get called on some buttons for some reason. Don't know why, and don't care for now. It's not about FastInvoke because Invoke does not work too.
			//// Allow everything to be initialized first. The user would not likely press the button in 100 ms.
			//this.FastInvoke("RegisterEvents", 0.1f);

			RegisterEvents();
		}

		protected void OnDisable()
		{
			DeregisterEvents();
		}

		private void RegisterEvents()
		{
			if (string.IsNullOrWhiteSpace(SoundEvent))
				return; // Do not even bother registering to events.

			if (Button)
			{
				Log.VerboseWithContext(Button, $"Registering '{nameof(Button)}' click sound for '{this.FullGameObjectName()}'.");
				switch (Action)
				{
					case ButtonClickSoundAction.Up:
						Button.onClick.RemoveListener(OnClick);
						Button.onClick.AddListener(OnClick);
						break;
					case ButtonClickSoundAction.Down:
						Button.DeregisterFromEvent(EventTriggerType.PointerDown, OnCustom);
						Button.RegisterToEvent(EventTriggerType.PointerDown, OnCustom);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			else if (Toggle)
			{
				Log.VerboseWithContext(Toggle, $"Registering '{nameof(Toggle)}' click sound for '{this.FullGameObjectName()}'.");
				switch (Action)
				{
					case ButtonClickSoundAction.Up:
						Toggle.onValueChanged.RemoveListener(OnValueChanged);
						Toggle.onValueChanged.AddListener(OnValueChanged);
						break;
					case ButtonClickSoundAction.Down:
						Toggle.DeregisterFromEvent(EventTriggerType.PointerDown, OnCustom);
						Toggle.RegisterToEvent(EventTriggerType.PointerDown, OnCustom);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		private void DeregisterEvents()
		{
			if (Button)
			{
				Log.VerboseWithContext(Button, $"Deregistering '{nameof(Button)}' click sound for '{this.FullGameObjectName()}'.");
				switch (Action)
				{
					case ButtonClickSoundAction.Up:
						Button.onClick.RemoveListener(OnClick);
						break;
					case ButtonClickSoundAction.Down:
						Button.DeregisterFromEvent(EventTriggerType.PointerDown, OnCustom);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			else if (Toggle)
			{
				Log.VerboseWithContext(Toggle, $"Deregistering '{nameof(Toggle)}' click sound for '{this.FullGameObjectName()}'.");
				switch (Action)
				{
					case ButtonClickSoundAction.Up:
						Toggle.onValueChanged.RemoveListener(OnValueChanged);
						break;
					case ButtonClickSoundAction.Down:
						Toggle.DeregisterFromEvent(EventTriggerType.PointerDown, OnCustom);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		private void OnValueChanged(bool dummy)
		{
			Play();
		}

		private void OnClick()
		{
			Play();
		}

		private void OnCustom(BaseEventData dummy)
		{
			Play();
		}

		public void Play()
		{
			Log.VerboseWithContext(this, $"Playing click sound of '{this.FullGameObjectName()}'.");
			AudioManager.Play(SoundEvent);
		}

#if UNITY_EDITOR

		protected void OnValidate()
		{
			// Need to check for both at the same time. We are interested in triggering
			// a heavy GetComponent check only if all of the references are missing.
			if (!Button && !Toggle)
			{
				Button = GetComponent<Button>();
				Toggle = GetComponent<Toggle>();
			}
		}

#endif

		#region Log

		private static readonly Logger Log = new(nameof(ButtonClickSound));

		#endregion
	}

}

#endif
