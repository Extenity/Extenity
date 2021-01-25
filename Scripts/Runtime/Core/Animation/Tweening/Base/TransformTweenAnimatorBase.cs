using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.AnimationToolbox
{

	public abstract class TransformTweenAnimatorBase : MonoBehaviour
	{
		#region Transformed Object

		public Transform Transform;

		#endregion

		#region Animate

		[NonSerialized]
		public Sequence CurrentAnimation;

		public void Play(TransformTweenSetup sequence)
		{
			Stop();
			CurrentAnimation = DOTween.Sequence();
			if (sequence.MovementEase != Ease.Unset)
			{
				var tween = Transform.DOMove(sequence.Location.position, sequence.Duration)
				                     .SetDelay(sequence.Delay)
				                     .SetEase(sequence.MovementEase);
				CurrentAnimation.Insert(0f, tween);
			}
			if (sequence.RotationEase != Ease.Unset)
			{
				var tween = Transform.DORotateQuaternion(sequence.Location.rotation, sequence.Duration)
				                     .SetDelay(sequence.Delay)
				                     .SetEase(sequence.RotationEase);
				CurrentAnimation.Insert(0f, tween);
			}
			if (sequence.ScaleEase != Ease.Unset)
			{
				var tween = Transform.DOScale(sequence.Location.localScale, sequence.Duration)
				                     .SetDelay(sequence.Delay)
				                     .SetEase(sequence.ScaleEase);
				CurrentAnimation.Insert(0f, tween);
			}
			CurrentAnimation.Play();
		}

		public void Jump(TransformTweenSetup sequence)
		{
			Stop();
			if (sequence.MovementEase != Ease.Unset)
			{
				Transform.position = sequence.Location.position;
			}
			if (sequence.RotationEase != Ease.Unset)
			{
				Transform.rotation = sequence.Location.rotation;
			}
			if (sequence.ScaleEase != Ease.Unset)
			{
				Transform.localScale = sequence.Location.localScale;
			}
		}

		[TitleGroup("Playback")]
		[HorizontalGroup("Playback/Buttons"), Button(ButtonSizes.Large), PropertyOrder(100)]
		public void Stop()
		{
			if (CurrentAnimation != null)
			{
				CurrentAnimation.Kill(false);
				CurrentAnimation = null;
			}
		}

		#endregion

		#region Editor

		private void OnValidate()
		{
			if (!Transform)
			{
				Transform = transform;
			}
		}

		#endregion
	}

}
