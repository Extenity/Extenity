using System;
using UnityEngine;
using System.Collections;
using DG.Tweening;
using Extenity.GameObjectToolbox;

[RequireComponent(typeof(CanvasGroup))]
public class UiVisibilityController : MonoBehaviour
{
	#region Initialization

	public enum VisibilityState
	{
		Unspecified,
		Shown,
		Hidden,
	}

	public VisibilityState InitialVisibility = VisibilityState.Unspecified;

	protected void Start()
	{
		switch (InitialVisibility)
		{
			case VisibilityState.Unspecified:
				break;
			case VisibilityState.Shown:
				{
					CanvasGroup.alpha = VisibleAlpha;
					CanvasGroup.interactable = true;
				}
				break;
			case VisibilityState.Hidden:
				{
					CanvasGroup.alpha = HiddenAlpha;
					CanvasGroup.interactable = true;
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	#endregion

	#region Configuration

	public float VisibleAlpha = 1f;
	public float HiddenAlpha = 0f;
	public float FadeInDuration = 0.2f;
	public float FadeOutDuration = 0.2f;
	public float ShowDelay = 1f;
	public float HideDelay = 1f;
	public bool PostponeDelayOnConsecutiveCalls = false;

	#endregion

	#region Canvas Group

	private CanvasGroup _CanvasGroup;
	public CanvasGroup CanvasGroup
	{
		get
		{
			if (_CanvasGroup == null)
			{
				_CanvasGroup = transform.GetSingleComponentEnsured<CanvasGroup>();
			}
			return _CanvasGroup;
		}
	}

	#endregion

	#region Show / Hide

	private void CancelShowHideInvokes()
	{
		CancelInvoke("Show");
		CancelInvoke("Hide");
	}

	public void Show()
	{
		CancelShowHideInvokes();
		CanvasGroup.DOFade(VisibleAlpha, FadeInDuration);
		CanvasGroup.interactable = true;
	}

	public void InstantShow()
	{
		CancelShowHideInvokes();
		CanvasGroup.alpha = VisibleAlpha;
		CanvasGroup.interactable = false;
	}

	public void DelayedShow()
	{
		if (!PostponeDelayOnConsecutiveCalls && IsInvoking("Show"))
		{
			return;
		}

		CancelShowHideInvokes();
		Invoke("Show", ShowDelay);
	}

	public void Hide()
	{
		CancelShowHideInvokes();
		CanvasGroup.DOFade(HiddenAlpha, FadeOutDuration);
		CanvasGroup.interactable = false;
	}

	public void InstantHide()
	{
		CancelShowHideInvokes();
		CanvasGroup.alpha = HiddenAlpha;
		CanvasGroup.interactable = false;
	}

	public void DelayedHide()
	{
		if (!PostponeDelayOnConsecutiveCalls && IsInvoking("Hide"))
		{
			return;
		}

		CancelShowHideInvokes();
		Invoke("Hide", HideDelay);
	}

	#endregion
}
