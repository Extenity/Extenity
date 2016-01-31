using System;
using DG.Tweening;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UiFader : MonoBehaviour
{
	#region Configuration

	public bool GetFadeInConfigurationFromInitialValue = true;
	public float FadeInAlpha = 1f;
	public bool GetFadeOutConfigurationFromInitialValue = false;
	public float FadeOutAlpha = 0f;

	public float FadeInDuration = 1f;
	public float FadeOutDuration = 1f;
	public float FadeInDelay = 0f;
	public float FadeOutDelay = 0f;

	public InitialFadeState InitialState = InitialFadeState.Untouched;

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
		InitializeTarget();

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

	#region Target

	public CanvasGroup CanvasGroup;

	private void InitializeTarget()
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
	}

	#endregion

	#region Fade Commands

	public float Fade(bool visible)
	{
		if (visible)
			return FadeIn();
		else
			return FadeOut();
	}

	public float FadeIn()
	{
		return AlphaFadeIn();
	}

	public float FadeOut()
	{
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

		Stop();
		if (CanvasGroup != null)
		{
			CanvasGroup.blocksRaycasts = true; // Always block raycasts while the panel is visible whether it's visible slightly or fully.
			CanvasGroup.interactable = false; // Panel won't be interactable until fully visible.
			CanvasGroupTweener = CanvasGroup.DOFade(FadeInAlpha, duration).SetDelay(delay).OnComplete(() =>
			{
				CanvasGroup.blocksRaycasts = true;
				CanvasGroup.interactable = true;
			});
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

		Stop();
		if (CanvasGroup != null)
		{
			CanvasGroup.blocksRaycasts = true;  // Always block raycasts while the panel is visible whether it's visible slightly or fully.
			CanvasGroup.interactable = false; // Break interaction right away so user won't be able to click anything during fade out animation.
			CanvasGroupTweener = CanvasGroup.DOFade(FadeOutAlpha, duration).SetDelay(delay).OnComplete(() =>
			{
				CanvasGroup.blocksRaycasts = false;
				CanvasGroup.interactable = false;
			});
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
