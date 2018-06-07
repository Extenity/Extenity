using DG.Tweening;
using UnityEngine;

namespace Extenity.UIToolbox
{

	public class UISimpleAnimation : MonoBehaviour
	{
		#region Links

		[Header("Links")]
		public RectTransform AnimatedTransform;

		#endregion

		#region Animation

		[Header("Anchors")]
		public Ease EasingToA = Ease.OutCubic;
		public Ease EasingToB = Ease.OutCubic;
		public float Duration = 0.3f;
		public RectTransform AnchorA;
		public RectTransform AnchorB;

		public void AnimateToA(bool immediate = false)
		{
			AnimateTo(AnchorA.anchoredPosition, EasingToA, immediate);
		}

		public void AnimateToB(bool immediate = false)
		{
			AnimateTo(AnchorB.anchoredPosition, EasingToB, immediate);
		}

		public void AnimateTo(Vector2 position, Ease easing, bool immediate = false)
		{
			if (!AnimatedTransform)
				return; // Ignore if not set.

			if (immediate)
			{
				AnimatedTransform.anchoredPosition = position;
			}
			else
			{
				AnimatedTransform.DOAnchorPos(position, Duration, false).SetEase(easing);
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
	}

}
