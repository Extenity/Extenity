using System;
using Extenity.BeyondAudio;
using Extenity.FlowToolbox;
using TMPro;
using TMPro.Extensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Extenity.UIToolbox
{

	public class PowerUpButton : Button
	{
		#region UI Elements

		[Header("Power-Up Button")]
		public Image IconImage;
		public TextMeshProUGUI CountText;
		public string PowerUpUsedSound;
		public string PowerUpFailedSound;
		public UIFader CooldownFader;
		public TextMeshProUGUI CooldownTimerText;
		public float CooldownRefreshInterval = 0.1f;
		[NonSerialized]
		public object Tag;

		private int LastSetCount = Int32.MinValue;

		public Sprite Icon
		{
			get { return IconImage.sprite; }
			set
			{
				IconImage.sprite = value;
				IconImage.enabled = value;
			}
		}

		public Color IconColor
		{
			get { return IconImage.color; }
			set { IconImage.color = value; }
		}

		public void SetCount(int count)
		{
			if (LastSetCount != count)
			{
				LastSetCount = count;
				if (CountText)
				{
					if (count >= 0)
					{
						CountText.SetCharArrayForInt(count);
					}
					else
					{
						CountText.text = "";
					}
				}
			}
			RefreshInteractable();
		}

		private void RefreshInteractable()
		{
			var active = LastSetCount > 0 && !IsCooldownActive;
			interactable = active;
		}

		#endregion

		#region Click

		public class ClickEvent : UnityEvent<PowerUpButton> { }
		public readonly ClickEvent OnClicked = new ClickEvent();

		public override void OnPointerDown(PointerEventData eventData)
		{
			base.OnPointerDown(eventData);
			OnClicked.Invoke(this);
		}

		public void InformClickSuccessful(Transform soundEffectParent, float cooldownDuration, Action<PowerUpButton> onButtonCooldownEnd = null, string powerUpUsedSoundOverride = null)
		{
			var sound = string.IsNullOrEmpty(powerUpUsedSoundOverride) ? PowerUpUsedSound : powerUpUsedSoundOverride;

			if (soundEffectParent)
			{
				AudioManager.PlayAttached(sound, soundEffectParent, Vector3.zero);
			}
			else
			{
				AudioManager.Play(sound);
			}

			StartCooldown(cooldownDuration, onButtonCooldownEnd);
		}

		public void InformClickFailed(Transform soundEffectParent, string powerUpFailedSoundOverride = null)
		{
			var sound = string.IsNullOrEmpty(powerUpFailedSoundOverride) ? PowerUpFailedSound : powerUpFailedSoundOverride;

			if (soundEffectParent)
			{
				AudioManager.PlayAttached(sound, soundEffectParent, Vector3.zero);
			}
			else
			{
				AudioManager.Play(sound);
			}
		}

		#endregion

		#region Cooldown

		private float CooldownStartTime;
		private float CooldownDuration;
		public bool IsCooldownActive => CooldownStartTime > 0f;

		private Action<PowerUpButton> CallbackOnButtonCooldownEnd;

		public void ResetCooldown()
		{
			StartCooldown(0f, null);
		}

		public void StartCooldown(float duration, Action<PowerUpButton> onButtonCooldownEnd)
		{
			this.CancelFastInvoke(EndCooldown);
			this.CancelFastInvoke(RefreshCooldown);
			CooldownDuration = duration;
			RefreshInteractable();
			if (duration > 0f)
			{
				CooldownStartTime = Time.time;
				CallbackOnButtonCooldownEnd = onButtonCooldownEnd;
				this.FastInvoke(EndCooldown, duration);
				this.FastInvokeRepeating(RefreshCooldown, CooldownRefreshInterval, CooldownRefreshInterval, false);
				RefreshCooldown();
				CooldownFader.FadeIn();
			}
			else
			{
				CooldownFader.FadeOut();
			}
		}

		private void EndCooldown()
		{
			CooldownDuration = 0f;
			CooldownStartTime = 0f;
			this.CancelFastInvoke(RefreshCooldown);
			RefreshInteractable();
			CooldownFader.FadeOut();

			if (CallbackOnButtonCooldownEnd != null)
			{
				CallbackOnButtonCooldownEnd(this);
				CallbackOnButtonCooldownEnd = null;
			}
		}

		private void RefreshCooldown()
		{
			var passedTime = Time.time - CooldownStartTime;
			var remainingTime = CooldownDuration - passedTime;
			if (remainingTime < 0f)
				remainingTime = 0f;
			CooldownTimerText.SetCharArrayForValue("N1", remainingTime);
		}

		#endregion
	}

}
