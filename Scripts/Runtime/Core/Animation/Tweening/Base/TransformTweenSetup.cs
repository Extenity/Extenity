#if UNITY_5_3_OR_NEWER

using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace Extenity.AnimationToolbox
{

	[Serializable]
	public class TransformTweenSetup
	{
		[FormerlySerializedAs("EndLocation")]
		public Transform Location;

		public float Delay = 0f;
		public float Duration = 0.5f;

		public Ease MovementEase = Ease.OutCubic;
		public Ease RotationEase = Ease.OutCubic;
		public Ease ScaleEase = Ease.OutCubic;

		public bool UseLocalPosition;
		public bool UseLocalRotation;
		public bool MoveInLocal;
		public bool RotateInLocal;

		public void RemoveMovementAnimation()
		{
			MovementEase = Ease.Unset;
		}

		public void RemoveRotationAnimation()
		{
			RotationEase = Ease.Unset;
		}

		public void RemoveScaleAnimation()
		{
			ScaleEase = Ease.Unset;
		}
	}

}

#endif
