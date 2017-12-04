using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Extenity.UIToolbox
{

	public class UIFader : MonoBehaviour
	{
		#region Configuration

		[Header("Setup")]
		public CanvasGroup CanvasGroup;
		[Tooltip("Optional Canvas reference that will be enabled or disabled.")]
		public Canvas Canvas;
		public InitialFadeState InitialState = InitialFadeState.Untouched;

		[Header("Transparency")]
		public bool GetFadeInConfigurationFromInitialValue = true;
		[Range(0f, 1f)]
		public float FadeInAlpha = 1f;
		public bool GetFadeOutConfigurationFromInitialValue = false;
		[Range(0f, 1f)]
		public float FadeOutAlpha = 0f;

		[Header("Timing")]
		public float FadeInDuration = 0.5f;
		public float FadeOutDuration = 0.5f;
		public float FadeInDelay = 0f;
		public float FadeOutDelay = 0f;

		#endregion

		#region Initialization

		public enum InitialFadeState
		{
			Untouched,
			FadedIn,
			FadedOut,
		}

		private void Start()
		{
			if (GetFadeInConfigurationFromInitialValue)
			{
				if (CanvasGroup != null)
					FadeInAlpha = CanvasGroup.alpha;
			}
			if (GetFadeOutConfigurationFromInitialValue)
			{
				if (CanvasGroup != null)
					FadeOutAlpha = CanvasGroup.alpha;
			}

			switch (InitialState)
			{
				case InitialFadeState.Untouched:
					break;
				case InitialFadeState.FadedIn:
					AlphaFadeIn(0f, 0f);
					break;
				case InitialFadeState.FadedOut:
					AlphaFadeOut(0f, 0f);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		#endregion

		#region Fade Commands

		public class FaderEvent : UnityEvent<UIFader> { }

		[NonSerialized]
		public FaderEvent OnFadeIn = new FaderEvent();
		[NonSerialized]
		public FaderEvent OnFadeOut = new FaderEvent();
		[NonSerialized]
		public FaderEvent OnFinishedFadeIn = new FaderEvent();
		[NonSerialized]
		public FaderEvent OnFinishedFadeOut = new FaderEvent();

		public float Fade(bool visible)
		{
			if (visible)
				return FadeIn();
			else
				return FadeOut();
		}

		public float FadeIn()
		{
			OnFadeIn.Invoke(this);
			return AlphaFadeIn();
		}

		public float FadeOut()
		{
			OnFadeOut.Invoke(this);
			return AlphaFadeOut();
		}

		#endregion

		#region Fade - Alpha

		protected float AlphaFadeIn()
		{
			return AlphaFadeIn(FadeInDelay, FadeInDuration);
		}

		protected float AlphaFadeIn(float delay, float duration)
		{
			if (DEBUG_ShowFadeMessages)
			{
				Debug.LogFormat("Fading in '{0}'", CanvasGroup.gameObject.name);
			}

			if (delay < 0f)
				delay = 0f;
			if (duration < 0f)
				duration = 0f;

			Stop();
			if (CanvasGroup != null)
			{
				if (duration < 0.001f)
				{
					CanvasGroup.alpha = FadeInAlpha;
					CanvasGroup.blocksRaycasts = true;
					CanvasGroup.interactable = true;
					if (Canvas != null)
						Canvas.enabled = true;
				}
				else
				{
					// Always block raycasts while the panel is visible whether it's visible slightly or fully.
					CanvasGroup.blocksRaycasts = true;
					// Panel won't be interactable until fully visible.
					//CanvasGroup.interactable = false;
					// Panel is going to be instantly interactable before getting fully visible.
					CanvasGroup.interactable = true;
					if (Canvas != null)
						Canvas.enabled = true;
					CanvasGroupTweener = CanvasGroup.DOFade(FadeInAlpha, duration).SetUpdate(true).SetDelay(delay).OnComplete(() =>
					{
						CanvasGroup.blocksRaycasts = true;
						CanvasGroup.interactable = true;
						if (Canvas != null)
							Canvas.enabled = true;
						OnFinishedFadeIn.Invoke(this);
					});
				}
			}
			return duration + delay;
		}

		protected float AlphaFadeOut()
		{
			return AlphaFadeOut(FadeOutDelay, FadeOutDuration);
		}

		protected float AlphaFadeOut(float delay, float duration)
		{
			if (DEBUG_ShowFadeMessages)
			{
				Debug.LogFormat("Fading out '{0}'", CanvasGroup.gameObject.name);
			}

			if (delay < 0f)
				delay = 0f;
			if (duration < 0f)
				duration = 0f;

			Stop();
			if (CanvasGroup != null)
			{
				if (duration < 0.001f)
				{
					if (Canvas != null)
						Canvas.enabled = false;
					CanvasGroup.alpha = FadeOutAlpha;
					CanvasGroup.blocksRaycasts = false;
					CanvasGroup.interactable = false;
				}
				else
				{
					// Always block raycasts while the panel is visible whether it's visible slightly or fully.
					CanvasGroup.blocksRaycasts = true;
					// Break interaction right away so user won't be able to click anything during fade out animation.
					CanvasGroup.interactable = false;
					CanvasGroupTweener = CanvasGroup.DOFade(FadeOutAlpha, duration).SetUpdate(true).SetDelay(delay).OnComplete(() =>
					{
						if (Canvas != null)
							Canvas.enabled = false;
						CanvasGroup.blocksRaycasts = false;
						CanvasGroup.interactable = false;
						OnFinishedFadeOut.Invoke(this);
					});
				}
			}
			return duration + delay;
		}

		#endregion

		#region Tweener

		private Tweener CanvasGroupTweener;

		public void Stop()
		{
			if (CanvasGroupTweener != null)
			{
				CanvasGroupTweener.Kill();
				CanvasGroupTweener = null;
			}
		}

		#endregion

		#region Debug

		[Header("Debug")]
		public bool DEBUG_ShowFadeMessages = false;

		#endregion
	}

}
