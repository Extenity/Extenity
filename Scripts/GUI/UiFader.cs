using System;
using DG.Tweening;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UiFader : MonoBehaviour
{
	#region Configuration

	public FadeType Type;

	public bool GetFadeInConfigurationFromInitialValue = true;
	public Color FadeInColor = Color.white;
	public float FadeInAlpha = 1f;
	public bool GetFadeOutConfigurationFromInitialValue = false;
	public Color FadeOutColor = Color.black;
	public float FadeOutAlpha = 0f;

	public float FadeInDuration = 1f;
	public float FadeOutDuration = 1f;
	public float FadeInDelay = 0f;
	public float FadeOutDelay = 0f;

	#endregion

	#region Initialization

	private void Awake()
	{
		InitializeTarget();
	}

	#endregion

	#region Target

	private Image Image;
	private CanvasGroup CanvasGroup;

	private void InitializeTarget()
	{
		Image = GetComponent<Image>();
		CanvasGroup = GetComponent<CanvasGroup>();

		if (GetFadeInConfigurationFromInitialValue)
		{
			if (Image != null)
				FadeInColor = Image.color;
			if (CanvasGroup != null)
				FadeInAlpha = CanvasGroup.alpha;
		}
		if (GetFadeOutConfigurationFromInitialValue)
		{
			if (Image != null)
				FadeOutColor = Image.color;
			if (CanvasGroup != null)
				FadeOutAlpha = CanvasGroup.alpha;
		}
	}

	#endregion

	#region Fade Type

	public enum FadeType
	{
		Undefined,
		Color,
		Alpha,
	}

	#endregion

	#region Fade Commands

	public float FadeIn()
	{
		switch (Type)
		{
			case FadeType.Undefined:
				return 0f;
			case FadeType.Color:
				return ColorFadeIn();
			case FadeType.Alpha:
				return AlphaFadeIn();
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	public float FadeOut()
	{
		switch (Type)
		{
			case FadeType.Undefined:
				return 0f;
			case FadeType.Color:
				return ColorFadeOut();
			case FadeType.Alpha:
				return AlphaFadeOut();
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	#endregion

	#region Fade - Alpha

	protected float AlphaFadeIn()
	{
		return AlphaFadeIn(FadeInDelay, FadeInDuration);
	}

	protected float AlphaFadeIn(float delay, float duration)
	{
		Stop();
		if (Image != null)
			ImageTweener = Image.DOFade(FadeInAlpha, duration).SetDelay(delay);
		if (CanvasGroup != null)
			CanvasGroupTweener = CanvasGroup.DOFade(FadeInAlpha, duration).SetDelay(delay);
		return duration + delay;
	}

	protected float AlphaFadeOut()
	{
		return AlphaFadeOut(FadeOutDelay, FadeOutDuration);
	}

	protected float AlphaFadeOut(float delay, float duration)
	{
		Stop();
		if (Image != null)
			ImageTweener = Image.DOFade(FadeOutAlpha, duration).SetDelay(delay);
		if (CanvasGroup != null)
			CanvasGroupTweener = CanvasGroup.DOFade(FadeOutAlpha, duration).SetDelay(delay);
		return duration + delay;
	}

	#endregion

	#region Fade - Color

	protected float ColorFadeIn()
	{
		return ColorFadeIn(FadeInDelay, FadeInDuration);
	}

	protected float ColorFadeIn(float delay, float duration)
	{
		Stop();
		if (Image != null)
			ImageTweener = Image.DOColor(FadeInColor, duration).SetDelay(delay);
		return duration + delay;
	}

	protected float ColorFadeOut()
	{
		return ColorFadeOut(FadeOutDelay, FadeOutDuration);
	}

	protected float ColorFadeOut(float delay, float duration)
	{
		Stop();
		if (Image != null)
			ImageTweener = Image.DOColor(FadeOutColor, duration).SetDelay(delay);
		return duration + delay;
	}

	#endregion

	#region Tweener

	private Tweener ImageTweener;
	private Tweener CanvasGroupTweener;

	public void Stop()
	{
		if (ImageTweener != null && ImageTweener.IsPlaying())
		{
			ImageTweener.Kill();
			ImageTweener = null;
		}
		if (CanvasGroupTweener != null && CanvasGroupTweener.IsPlaying())
		{
			CanvasGroupTweener.Kill();
			CanvasGroupTweener = null;
		}
	}

	#endregion
}
