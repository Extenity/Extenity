using System;
using Extenity.BeyondAudio;
using Extenity.FlowToolbox;
using TMPro;
using TMPro.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Extenity.UIToolbox
{

	public class PowerUpButton : Button
	{
		#region UI Elements

		[Header("Power-Up Button")]
		public TextMeshProUGUI CountText;
		public string UsePowerUpSound;
		public UIFader CooldownFader;
		public TextMeshProUGUI CooldownTimerText;
		public float CooldownRefreshInterval = 0.1f;

		private int LastSetCount = Int32.MinValue;

		public void SetCount(int value)
		{
			if (LastSetCount != value)
			{
				LastSetCount = value;
				CountText.SetCharArrayForInt(value);
			}
			RefreshInteractable();
		}

		private void RefreshInteractable()
		{
			var active = LastSetCount > 0 && !IsCooldownActive;
			interactable = active;
		}

		public void InformClick(Transform soundEffectParent, float cooldownDuration)
		{
			if (soundEffectParent)
			{
				AudioManager.PlayAttached(UsePowerUpSound, soundEffectParent, Vector3.zero);
			}
			else
			{
				AudioManager.Play(UsePowerUpSound);
			}

			StartCooldown(cooldownDuration);
		}

		#endregion

		#region Cooldown

		private float CooldownStartTime;
		private float CooldownDuration;
		private bool IsCooldownActive => CooldownStartTime > 0f;

		public void StartCooldown(float duration)
		{
			this.CancelFastInvoke(EndCooldown);
			this.CancelFastInvoke(RefreshCooldown);
			CooldownDuration = duration;
			RefreshInteractable();
			if (duration > 0f)
			{
				CooldownStartTime = Time.time;
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
