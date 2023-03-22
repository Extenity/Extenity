using System;
using DG.Tweening;
using Extenity.DataToolbox;
using UnityEngine;
using UnityEngine.Events;

namespace Extenity.UIToolbox
{

	public class UISimpleAnimation : MonoBehaviour
	{
		#region Initialization

		protected void Start()
		{
			Logger.SetContext(ref Log, this);

			switch (InitialState)
			{
				case InitialAnimationState.Untouched:
					break;
				case InitialAnimationState.PlaceToA:
					AnimateImmediateToA();
					break;
				case InitialAnimationState.PlaceToB:
					AnimateImmediateToB();
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(InitialState), (int)InitialState, "");
			}
		}

		#endregion

		#region Links

		[Header("Links")]
		public RectTransform AnimatedTransform;

		#endregion

		#region Triggered Faders

		[Serializable]
		public class TriggeredFaderEntry
		{
			public UIFader Fader;
			public bool Inverted = false;

			public Func<float> GetFaderMethod(AnimationState targetState)
			{
				if (!Fader)
					return null;
				switch (targetState)
				{
					case AnimationState.PlaceToA:
						if (Inverted)
							return Fader.FadeIn;
						else
							return Fader.FadeOut;
					case AnimationState.PlaceToB:
						if (Inverted)
							return Fader.FadeOut;
						else
							return Fader.FadeIn;
				}
				return null;
			}
		}

		[Tooltip("Optional faders that is triggered with animation.")]
		public TriggeredFaderEntry[] TriggeredFaders;

		public float TriggerFaders(AnimationState targetState)
		{
			if (TriggeredFaders == null)
				return 0f;
			var maxDelayAndDuration = float.MaxValue;
			for (var i = 0; i < TriggeredFaders.Length; i++)
			{
				var action = TriggeredFaders[i].GetFaderMethod(targetState);
				if (action != null)
				{
					var delayAndDuration = action();
					if (maxDelayAndDuration > delayAndDuration)
						maxDelayAndDuration = delayAndDuration;
				}
			}
			return maxDelayAndDuration;
		}

		#endregion

		#region Animation

		public enum AnimationState
		{
			Untouched,
			PlaceToA,
			PlaceToB,
		}

		public enum InitialAnimationState
		{
			Untouched,
			PlaceToA,
			PlaceToB,
		}

		[Header("Animation")]
		public InitialAnimationState InitialState = InitialAnimationState.Untouched;
		public Ease EasingToA = Ease.OutCubic;
		public Ease EasingToB = Ease.OutCubic;
		public float DurationToA = 0.3f;
		public float DurationToB = 0.3f;
		public float DelayToA = 0f;
		public float DelayToB = 0f;
		public RectTransform AnchorA;
		public RectTransform AnchorB;

		public class AnimationEvent : UnityEvent<UISimpleAnimation> { }

		[NonSerialized]
		public AnimationEvent OnAnimationStarted = new AnimationEvent();
		[NonSerialized]
		public AnimationEvent OnAnimationFinished = new AnimationEvent();

		/// ----------------------------------------------------------------------------------------

		public float AnimateToA(bool immediate)
		{
			if (immediate)
				return AnimateImmediateToA();
			else
				return AnimateToA();
		}

		public float AnimateImmediateToA()
		{
			if (!AnchorA)
			{
				Log.Warning($"Ignored animation request because no '{nameof(AnchorA)}' specified in animation of '{this.FullGameObjectName()}'.");
				return 0f;
			}
			return AnimateTo(AnchorA.anchoredPosition, EasingToA, 0f, 0f, AnimationState.PlaceToA);
		}

		public float AnimateToA()
		{
			if (!AnchorA)
			{
				Log.Warning($"Ignored animation request because no '{nameof(AnchorA)}' specified in animation of '{this.FullGameObjectName()}'.");
				return 0f;
			}
			return AnimateTo(AnchorA.anchoredPosition, EasingToA, DelayToA, DurationToA, AnimationState.PlaceToA);
		}

		/// ----------------------------------------------------------------------------------------

		public float AnimateToB(bool immediate)
		{
			if (immediate)
				return AnimateImmediateToB();
			else
				return AnimateToB();
		}

		public float AnimateImmediateToB()
		{
			if (!AnchorB)
			{
				Log.Warning($"Ignored animation request because no '{nameof(AnchorB)}' specified in animation of '{this.FullGameObjectName()}'.");
				return 0f;
			}
			return AnimateTo(AnchorB.anchoredPosition, EasingToB, 0f, 0f, AnimationState.PlaceToB);
		}

		public float AnimateToB()
		{
			if (!AnchorB)
			{
				Log.Warning($"Ignored animation request because no '{nameof(AnchorB)}' specified in animation of '{this.FullGameObjectName()}'.");
				return 0f;
			}
			return AnimateTo(AnchorB.anchoredPosition, EasingToB, DelayToB, DurationToB, AnimationState.PlaceToB);
		}

		/// ----------------------------------------------------------------------------------------

		public float AnimateTo(Vector2 position, Ease easing, float delay, float duration, AnimationState targetState)
		{
			if (!AnimatedTransform) // Ignore if not set.
			{
				Log.Warning($"Ignored animation request because no '{nameof(AnimatedTransform)}' specified in animation of '{this.FullGameObjectName()}'.");
				return 0f;
			}

			Log.Verbose($"Animating '{AnimatedTransform.FullGameObjectName()}' to '{position}' with '{easing}' in '{duration}' seconds and delayed '{delay}' seconds.");

			if (delay < 0f)
				delay = 0f;
			if (duration < 0f)
				duration = 0f;

			Stop();

			OnAnimationStarted.Invoke(this);
			var maxFaderDelayAndDuration = TriggerFaders(targetState);

			if (duration < 0.001f)
			{
				AnimatedTransform.anchoredPosition = position;
				OnAnimationFinished.Invoke(this);
			}
			else
			{
				Tweener = AnimatedTransform.DOAnchorPos(position, duration, false).SetEase(easing).SetUpdate(true).SetDelay(delay).OnComplete(() =>
				{
					Tweener = null;
					OnAnimationFinished.Invoke(this);
				});
			}
			return Mathf.Max(duration + delay, maxFaderDelayAndDuration);
		}

		#endregion

		#region Tweener

		private Tweener Tweener;

		public void Stop()
		{
			if (Tweener != null)
			{
				Tweener.Kill();
				Tweener = null;
			}
		}

		#endregion

		#region Editor

#if UNITY_EDITOR

		protected void OnValidate()
		{
			if (!AnimatedTransform)
			{
				AnimatedTransform = gameObject.GetComponent<RectTransform>();
			}
		}

#endif

		#endregion

		#region Log

		private Logger Log = new(nameof(UISimpleAnimation));

		#endregion
	}

}
