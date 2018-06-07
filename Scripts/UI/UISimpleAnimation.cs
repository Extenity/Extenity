using System;
using DG.Tweening;
using Extenity.GameObjectToolbox;
using UnityEngine;
using UnityEngine.Events;

namespace Extenity.UIToolbox
{

	public class UISimpleAnimation : MonoBehaviour
	{
		#region Initialization

		public enum InitialAnimationState
		{
			Untouched,
			PlaceToA,
			PlaceToB,
		}

		protected void Start()
		{
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

		#region Animation

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
				if (DEBUG_ShowAnimationMessages)
				{
					Debug.LogWarning($"Ignored animation request because no '{nameof(AnchorA)}' specified in animation of '{gameObject.FullName()}'.", gameObject);
				}
				return 0f;
			}
			return AnimateTo(AnchorA.anchoredPosition, EasingToA, 0f, 0f);
		}

		public float AnimateToA()
		{
			if (!AnchorA)
			{
				if (DEBUG_ShowAnimationMessages)
				{
					Debug.LogWarning($"Ignored animation request because no '{nameof(AnchorA)}' specified in animation of '{gameObject.FullName()}'.", gameObject);
				}
				return 0f;
			}
			return AnimateTo(AnchorA.anchoredPosition, EasingToA, DelayToA, DurationToA);
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
				if (DEBUG_ShowAnimationMessages)
				{
					Debug.LogWarning($"Ignored animation request because no '{nameof(AnchorB)}' specified in animation of '{gameObject.FullName()}'.", gameObject);
				}
				return 0f;
			}
			return AnimateTo(AnchorB.anchoredPosition, EasingToB, 0f, 0f);
		}

		public float AnimateToB()
		{
			if (!AnchorB)
			{
				if (DEBUG_ShowAnimationMessages)
				{
					Debug.LogWarning($"Ignored animation request because no '{nameof(AnchorB)}' specified in animation of '{gameObject.FullName()}'.", gameObject);
				}
				return 0f;
			}
			return AnimateTo(AnchorB.anchoredPosition, EasingToB, DelayToB, DurationToB);
		}

		/// ----------------------------------------------------------------------------------------

		public float AnimateTo(Vector2 position, Ease easing, float delay, float duration)
		{
			if (!AnimatedTransform) // Ignore if not set.
			{
				if (DEBUG_ShowAnimationMessages)
				{
					Debug.LogWarning($"Ignored animation request because no '{nameof(AnimatedTransform)}' specified in animation of '{gameObject.FullName()}'.", gameObject);
				}
				return 0f;
			}

			if (DEBUG_ShowAnimationMessages)
			{
				Debug.Log($"Animating '{AnimatedTransform.gameObject.FullName()}' to '{position}' with '{easing}' in '{duration}' seconds and delayed '{delay}' seconds.", gameObject);
			}

			if (delay < 0f)
				delay = 0f;
			if (duration < 0f)
				duration = 0f;

			Stop();

			OnAnimationStarted.Invoke(this);

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
			return duration + delay;
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

		protected void OnValidate()
		{
			if (!AnimatedTransform)
			{
				AnimatedTransform = gameObject.GetComponent<RectTransform>();
			}
		}

		#endregion

		#region Debug

		[Header("Debug")]
		public bool DEBUG_ShowAnimationMessages = false;

		#endregion
	}

}
