using System;
using Extenity.BeyondAudio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Extenity.UIToolbox
{

	public enum BlinkerColor
	{
		Nothing,
		OnColor,
		OffColor,
	}

	public class Blinker : MonoBehaviour
	{
		public BlinkerColor SetToStateAtStart = BlinkerColor.OffColor;
		[Header("Timing")]
		public float BlinkInterval = 0.4f;
		public float BlinkOnPercentage = 0.8f;
		public float Duration = 0f;
		[Header("Audio")]
		public string BlinkAudio = "";
		public string HideAudio = "";
		[Header("Color")]
		public Color OnColor = Color.white;
		public Color OffColor = Color.grey;
		[Header("Controlled Objects")]
		public MonoBehaviour ActivatedObject;
		public CanvasGroup ActivatedCanvasGroup;
		public SpriteRenderer ColoredSprite;
		public Image ColoredImage;
		public TextMeshProUGUI ColoredTextMeshProUGUI;

		public float BlinkOnDuration { get { return BlinkInterval * BlinkOnPercentage; } }
		public float BlinkOffDuration { get { return BlinkInterval * (1f - BlinkOnPercentage); } }

		[NonSerialized]
		public bool IsBlinking = false;
		private bool BlinkState = false;
		private float StartTime = -1f;
		private float NextActionTime = -1f;

		private void Start()
		{
			switch (SetToStateAtStart)
			{
				case BlinkerColor.Nothing:
					break;
				case BlinkerColor.OnColor:
					SwitchState(true, false);
					break;
				case BlinkerColor.OffColor:
					SwitchState(false, false);
					break;
			}
		}

		private void FixedUpdate()
		{
			if (!IsBlinking)
				return;

			var now = Time.time;
			if (NextActionTime > now)
				return;

			if (Duration > 0f && now - StartTime > Duration)
			{
				StopBlinking();
				return;
			}

			BlinkState = !BlinkState;
			SwitchState(BlinkState, true);
			NextActionTime += (BlinkState ? BlinkOnDuration : BlinkOffDuration);
		}

		public void StartBlinking()
		{
			IsBlinking = true;
			BlinkState = true;
			SwitchState(BlinkState, true);
			StartTime = Time.time;
			NextActionTime = StartTime + BlinkOnDuration;
		}

		public void StopBlinking()
		{
			IsBlinking = false;
			BlinkState = false;
			SwitchState(BlinkState, false);
			StartTime = -1f;
			NextActionTime = -1f;
		}

		private void SwitchState(bool blinkState, bool playAudio)
		{
			if (ActivatedObject)
				ActivatedObject.enabled = blinkState;
			if (ActivatedCanvasGroup)
			{
				if (blinkState)
				{
					ActivatedCanvasGroup.alpha = 1f;
				}
				else
				{
					ActivatedCanvasGroup.alpha = 0f;
				}
			}
			if (ColoredSprite)
				ColoredSprite.color = blinkState ? OnColor : OffColor;
			if (ColoredImage)
				ColoredImage.color = blinkState ? OnColor : OffColor;
			if (ColoredTextMeshProUGUI)
				ColoredTextMeshProUGUI.color = blinkState ? OnColor : OffColor;

			if (playAudio)
			{
				if (blinkState)
				{
					AudioManager.Play(HideAudio);
				}
				else
				{
					AudioManager.Play(BlinkAudio);
				}
			}
		}
	}

}
