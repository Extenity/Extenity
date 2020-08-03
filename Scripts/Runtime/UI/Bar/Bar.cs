using DG.Tweening;
using Extenity.MathToolbox;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.UIToolbox
{

	[ExecuteAlways]
	public class Bar : MonoBehaviour
#if UNITY_EDITOR
	                   , ISerializationCallbackReceiver
#endif
	{
		[BoxGroup("Setup")] public RectTransform BarMask;
		[BoxGroup("Setup")] public RectTransform BarIncreaseMask;
		[BoxGroup("Setup")] public RectTransform BarDecreaseMask;
		[BoxGroup("Setup")] public CanvasGroup BarIncreaseMaskCanvasGroup;
		[BoxGroup("Setup")] public CanvasGroup BarDecreaseMaskCanvasGroup;
		[BoxGroup("Setup")] public RectTransform BarMaskVisual;
		[BoxGroup("Setup")] public RectTransform BarIncreaseMaskVisual;
		[BoxGroup("Setup")] public RectTransform BarDecreaseMaskVisual;

		[BoxGroup("Setup")]
		public float BarSize = 100;
		[BoxGroup("Setup")]
		[Tooltip("An initial value below zero means the bar won't be initialized.")]
		public float InitialValue = -1f;

		[BoxGroup("Animation")]
		public Ease AnimationEasing = Ease.OutCubic;
		[Tooltip("Duration of 0 means no animation.")]
		[Range(0f, 10f)]
		[HorizontalGroup("Animation/AnimationDurationGroup")]
		public float AnimationDuration = 0f;
#if UNITY_EDITOR
		[Button("Disable Animation")]
		[DisableIf(nameof(AnimationDuration), 0f)]
		[HorizontalGroup("Animation/AnimationDurationGroup")]
		private void _DisableAnimation()
		{
			AnimationDuration = 0f;
		}
#endif
		[Range(0f, 10f)]
		[BoxGroup("Animation")]
		public float AnimationDelay = 0f;

		private Tweener Animation;
		private float AnimationEndValue = float.NaN;

		protected void Awake()
		{
			if (InitialValue >= 0)
			{
				SetValue(InitialValue, true);
			}
			HideIncreaseDecreaseMasksAndEndAnimation();
		}

		protected void OnDestroy()
		{
			if (Animation != null)
			{
				Animation.Kill(false);
				Animation = null;
			}
		}

		public void SetValue(float percentage)
		{
			SetValueUnclamped(Mathf.Clamp01(percentage), false);
		}

		public void SetValue(float percentage, bool skipAnimation)
		{
			SetValueUnclamped(Mathf.Clamp01(percentage), skipAnimation);
		}

		public void SetValueUnclamped(float percentage)
		{
			SetValueUnclamped(percentage, false);
		}

		public void SetValueUnclamped(float percentage, bool skipAnimation)
		{
			var currentSize = BarMask.sizeDelta;
			var newSize = currentSize;
			newSize.x = BarSize * percentage;

			// Check if the new value is the same as previously set new value
			if ((!float.IsNaN(AnimationEndValue) && newSize.x.IsAlmostEqual(AnimationEndValue)) || // This line checks for the target value of ongoing animation.
			    newSize.x.IsAlmostEqual(currentSize.x)) // This line checks for the current size of bar mask. Doing this covers the cases for animationless bars.
			{
				return;
			}

			if (AnimationDuration > 0f && !skipAnimation
#if UNITY_EDITOR
			                           && Application.isPlaying
#endif
			)
			{
				AnimationEndValue = newSize.x;

				if (Animation == null)
				{
					Animation = DOTween.To(TweenGetterX, TweenSetterX, newSize.x, AnimationDuration)
					                   .SetDelay(AnimationDelay)
					                   .SetEase(AnimationEasing)
					                   .SetAutoKill(false)
					                   .SetUpdate(UpdateType.Late, true)
					                   .SetTarget(this)
					                   .OnComplete(HideIncreaseDecreaseMasksAndEndAnimation);
				}
				else
				{
#if UNITY_EDITOR
					// Apply modified parameters in inspector.
					Animation.SetDelay(AnimationDelay); // Seems like changing delay is not working.
					Animation.SetEase(AnimationEasing);
#endif
					Animation.ChangeValues(currentSize.x, newSize.x, AnimationDuration);
					Animation.Restart(true);
				}

				{
					if (BarIncreaseMaskCanvasGroup && newSize.x > currentSize.x)
					{
						if (BarDecreaseMaskCanvasGroup) // Disable the other mask first
							BarDecreaseMaskCanvasGroup.alpha = 0f;
						var sizeX = newSize.x - currentSize.x;
						SetIncreaseMaskPositionAndSize(sizeX);
					}
					else if (BarDecreaseMaskCanvasGroup && newSize.x < currentSize.x)
					{
						if (BarIncreaseMaskCanvasGroup) // Disable the other mask first
							BarIncreaseMaskCanvasGroup.alpha = 0f;
						var sizeX = currentSize.x - newSize.x;
						SetDecreaseMaskPositionAndSize(sizeX);
					}
				}
			}
			else
			{
				BarMask.sizeDelta = newSize;
				HideIncreaseDecreaseMasksAndEndAnimation();
			}
		}

		private void SetIncreaseMaskPositionAndSize(float sizeX)
		{
			if (BarIncreaseMask)
			{
				BarIncreaseMask.anchoredPosition = new Vector2(BarMask.anchoredPosition.x + BarMask.sizeDelta.x, BarIncreaseMask.anchoredPosition.y);
				BarIncreaseMask.sizeDelta = new Vector2(sizeX, BarIncreaseMask.sizeDelta.y);
				BarIncreaseMaskCanvasGroup.alpha = 1f;
			}
		}

		private void SetDecreaseMaskPositionAndSize(float sizeX)
		{
			if (BarDecreaseMask)
			{
				BarDecreaseMask.anchoredPosition = new Vector2(BarMask.anchoredPosition.x + BarMask.sizeDelta.x - sizeX, BarDecreaseMask.anchoredPosition.y);
				BarDecreaseMask.sizeDelta = new Vector2(sizeX, BarDecreaseMask.sizeDelta.y);
				BarDecreaseMaskCanvasGroup.alpha = 1f;
			}
		}

		private void HideIncreaseDecreaseMasksAndEndAnimation()
		{
			if (BarIncreaseMaskCanvasGroup)
			{
				BarIncreaseMaskCanvasGroup.alpha = 0f;
			}
			if (BarDecreaseMaskCanvasGroup)
			{
				BarDecreaseMaskCanvasGroup.alpha = 0f;
			}

			AnimationEndValue = float.NaN;
		}

		private void TweenSetterX(float newValue)
		{
			if (BarDecreaseMaskCanvasGroup && BarDecreaseMaskCanvasGroup.alpha > 0f)
			{
				var sizeX = BarMask.sizeDelta.x - AnimationEndValue;
				BarDecreaseMask.anchoredPosition = new Vector2(BarMask.anchoredPosition.x + AnimationEndValue, BarDecreaseMask.anchoredPosition.y);
				BarDecreaseMask.sizeDelta = new Vector2(sizeX, BarDecreaseMask.sizeDelta.y);
			}

			BarMask.sizeDelta = new Vector2(newValue, BarMask.sizeDelta.y);
		}

		private float TweenGetterX()
		{
			return BarMask.sizeDelta.x;
		}

		#region Editor

#if UNITY_EDITOR

		[BoxGroup("Test")]
		[Button(ButtonSizes.Large, ButtonStyle.Box, Name = "%0"), HorizontalGroup("Test/TestButtons")]
		private void SetValue_0() { SetValue(0.0f); }

		[Button(ButtonSizes.Large, ButtonStyle.Box, Name = "%10"), HorizontalGroup("Test/TestButtons")]
		private void SetValue_10() { SetValue(0.1f); }

		[Button(ButtonSizes.Large, ButtonStyle.Box, Name = "%30"), HorizontalGroup("Test/TestButtons")]
		private void SetValue_30() { SetValue(0.3f); }

		[Button(ButtonSizes.Large, ButtonStyle.Box, Name = "%50"), HorizontalGroup("Test/TestButtons")]
		private void SetValue_50() { SetValue(0.5f); }

		[Button(ButtonSizes.Large, ButtonStyle.Box, Name = "%80"), HorizontalGroup("Test/TestButtons")]
		private void SetValue_80() { SetValue(0.8f); }

		[Button(ButtonSizes.Large, ButtonStyle.Box, Name = "%100"), HorizontalGroup("Test/TestButtons")]
		private void SetValue_100() { SetValue(1.0f); }

		protected void Update()
		{
			if (!Application.IsPlaying(gameObject))
			{
				var rect = GetComponent<RectTransform>();
				rect.sizeDelta = new Vector2(BarSize, rect.sizeDelta.y);
				if (BarMask)
					BarMask.sizeDelta = new Vector2(BarSize, BarMask.sizeDelta.y);
				if (BarMaskVisual)
					BarMaskVisual.sizeDelta = new Vector2(BarSize, BarMaskVisual.sizeDelta.y);
				if (BarIncreaseMaskVisual)
					BarIncreaseMaskVisual.sizeDelta = new Vector2(BarSize, BarIncreaseMaskVisual.sizeDelta.y);
				if (BarDecreaseMaskVisual)
					BarDecreaseMaskVisual.sizeDelta = new Vector2(BarSize, BarDecreaseMaskVisual.sizeDelta.y);
			}
		}

		public void OnBeforeSerialize()
		{
			if (!Application.isPlaying)
			{
				SetValue(0f, true);
				SetIncreaseMaskPositionAndSize(0f);
				SetDecreaseMaskPositionAndSize(0f);
				HideIncreaseDecreaseMasksAndEndAnimation();
			}
		}

		public void OnAfterDeserialize()
		{
		}

#endif

		#endregion

		#region Test

		/* Currently does not work after implementing the reset mechanism in OnBeforeSerialize. Find a way to have them both coexist.
#if UNITY_EDITOR
#pragma warning disable 414

		[NonSerialized, ShowInInspector]
		[BoxGroup("Test")]
		[HorizontalGroup("Test/Hor")]
		[Range(0f, 1f)]
		[EnableIf(nameof(BarMask))]
		[OnValueChanged(nameof(SetValue))]
		private float _InspectorBar;

		[ShowInInspector]
		[Button("Min"), ButtonGroup("Test/Hor/ValueGroup")]
		[EnableIf(nameof(BarMask))]
		private void _SetToMin()
		{
			SetValue(_InspectorBar = 0f);
		}

		[ShowInInspector]
		[Button("Max"), ButtonGroup("Test/Hor/ValueGroup")]
		[EnableIf(nameof(BarMask))]
		private void _SetToMax()
		{
			SetValue(_InspectorBar = 1f);
		}

#pragma warning restore 414
#endif
		*/

		#endregion
	}

}
