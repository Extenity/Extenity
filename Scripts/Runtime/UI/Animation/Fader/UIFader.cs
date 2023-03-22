using System;
using DG.Tweening;
using Extenity.DataToolbox;
using UnityEngine;
using UnityEngine.Events;

namespace Extenity.UIToolbox
{

	public enum FadeState
	{
		Untouched,
		FadedIn,
		FadedOut,
	}

	public class UIFader : MonoBehaviour
	{
		#region Configuration

		public CanvasGroup CanvasGroup;
		[Tooltip("Optional Canvas reference that will be enabled or disabled.")]
		public Canvas Canvas;
		public FadeState InitialState = FadeState.Untouched;
		public bool Interactable = true;
		public bool BlocksRaycasts = true;
		[Tooltip("Optional orchestrator that is triggered with fading.")]
		public UISimpleAnimationOrchestrator TriggeredAnimationOrchestrator;

		[Header("Transparency")]
		public bool GetFadeInConfigurationFromInitialValue = false;
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

		protected void Start()
		{
			Logger.SetContext(ref Log, this);

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
				case FadeState.Untouched:
					break;
				case FadeState.FadedIn:
					AlphaFadeIn(0f, 0f);
					break;
				case FadeState.FadedOut:
					AlphaFadeOut(0f, 0f);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(InitialState), (int)InitialState, "");
			}
		}

		#endregion

		#region Deinitialization

		protected void OnDestroy()
		{
			Stop();
		}

		#endregion

		#region State

		public FadeState State { get; private set; }

		public bool IsVisible
		{
			get
			{
				switch (State)
				{
					case FadeState.Untouched:
						return CanvasGroup.alpha > 0f;
					case FadeState.FadedIn:
						return true;
					case FadeState.FadedOut:
						return false;
					default:
						throw new ArgumentOutOfRangeException();
				}
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

		public float Fade(bool visible, bool immediate)
		{
			if (visible)
				return FadeIn(immediate);
			else
				return FadeOut(immediate);
		}

		public float Fade(bool visible)
		{
			if (visible)
				return FadeIn();
			else
				return FadeOut();
		}

		public float FadeImmediate(bool visible)
		{
			if (visible)
				return FadeInImmediate();
			else
				return FadeOutImmediate();
		}

		public float FadeIn(bool immediate)
		{
			if (immediate)
				return AlphaFadeIn(0f, 0f);
			else
				return AlphaFadeIn(FadeInDelay, FadeInDuration);
		}

		public float FadeIn()
		{
			return AlphaFadeIn(FadeInDelay, FadeInDuration);
		}

		public float FadeInImmediate()
		{
			return AlphaFadeIn(0f, 0f);
		}

		public float FadeOut(bool immediate)
		{
			if (immediate)
				return AlphaFadeOut(0f, 0f);
			else
				return AlphaFadeOut(FadeOutDelay, FadeOutDuration);
		}

		public float FadeOut()
		{
			return AlphaFadeOut(FadeOutDelay, FadeOutDuration);
		}

		public float FadeOutImmediate()
		{
			return AlphaFadeOut(0f, 0f);
		}

		#endregion

		#region Fade - Alpha

		protected float AlphaFadeIn(float delay, float duration)
		{
			if (!CanvasGroup)
			{
				// Loosing CanvasGroup reference means we have probably lost the UIFader too.
				// Meaning 'this' reference would be null too. Check the callstack to see what's going wrong.
				Log.Fatal($"Trying to fade a null CanvasGroup for fader '{this.GameObjectNameSafe()}'.");
				return duration + delay; // Even though we are not in a desired situation, just pretend everything is working fine.
			}

			Log.Verbose($"Fading in '{CanvasGroup.FullGameObjectName()}'");

			State = FadeState.FadedIn;

			if (delay < 0f)
				delay = 0f;
			if (duration < 0f)
				duration = 0f;
			var immediate = duration < 0.001f;

			Stop();
			if (TriggeredAnimationOrchestrator)
				TriggeredAnimationOrchestrator.AnimateToB(immediate);

			OnFadeIn.Invoke(this);

			if (CanvasGroup != null)
			{
				if (immediate)
				{
					CanvasGroup.alpha = FadeInAlpha;
					if (BlocksRaycasts)
						CanvasGroup.blocksRaycasts = true;
					if (Interactable)
						CanvasGroup.interactable = true;
					if (Canvas != null)
						Canvas.enabled = true;
					OnFinishedFadeIn.Invoke(this);
				}
				else
				{
					// Always block raycasts while the panel is visible whether it's visible slightly or fully.
					if (BlocksRaycasts)
						CanvasGroup.blocksRaycasts = true;
					// Panel won't be interactable until fully visible.
					//CanvasGroup.interactable = false;
					// Panel is going to be instantly interactable before getting fully visible.
					if (Interactable)
						CanvasGroup.interactable = true;
					if (Canvas != null)
						Canvas.enabled = true;
					CanvasGroupTweener = CanvasGroup.DOFade(FadeInAlpha, duration).SetUpdate(true).SetDelay(delay).OnComplete(() =>
					{
						Log.Verbose($"Fade in completed for '{CanvasGroup.FullGameObjectName()}'");
						CanvasGroupTweener = null;
						if (BlocksRaycasts)
							CanvasGroup.blocksRaycasts = true;
						if (Interactable)
							CanvasGroup.interactable = true;
						if (Canvas != null)
							Canvas.enabled = true;
						OnFinishedFadeIn.Invoke(this);
					});
				}
			}
			return duration + delay;
		}

		protected float AlphaFadeOut(float delay, float duration)
		{
			if (!CanvasGroup)
			{
				// Loosing CanvasGroup reference means we have probably lost the UIFader too.
				// Meaning 'this' reference would be null too. Check the callstack to see what's going wrong.
				Log.Fatal($"Trying to fade a null CanvasGroup for fader '{this.GameObjectNameSafe()}'.");
				return duration + delay; // Even though we are not in a desired situation, just pretend everything is working fine.
			}

			Log.Verbose($"Fading out '{CanvasGroup.FullGameObjectName()}'");

			State = FadeState.FadedOut;

			if (delay < 0f)
				delay = 0f;
			if (duration < 0f)
				duration = 0f;
			var immediate = duration < 0.001f;

			Stop();

			OnFadeOut.Invoke(this);
			if (TriggeredAnimationOrchestrator)
				TriggeredAnimationOrchestrator.AnimateToA(immediate);

			if (CanvasGroup != null)
			{
				if (immediate)
				{
					if (Canvas != null)
						Canvas.enabled = false;
					CanvasGroup.alpha = FadeOutAlpha;
					CanvasGroup.blocksRaycasts = false;
					CanvasGroup.interactable = false;
					OnFinishedFadeOut.Invoke(this);
				}
				else
				{
					// Always block raycasts while the panel is visible whether it's visible slightly or fully.
					if (BlocksRaycasts)
						CanvasGroup.blocksRaycasts = true;
					// Break interaction right away so user won't be able to click anything during fade out animation.
					CanvasGroup.interactable = false;
					CanvasGroupTweener = CanvasGroup.DOFade(FadeOutAlpha, duration).SetUpdate(true).SetDelay(delay).OnComplete(() =>
					{
						Log.Verbose($"Fade out completed for '{CanvasGroup.FullGameObjectName()}'");
						CanvasGroupTweener = null;
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

		#region Log

		private Logger Log = new(nameof(UIFader));

		#endregion
	}

}
