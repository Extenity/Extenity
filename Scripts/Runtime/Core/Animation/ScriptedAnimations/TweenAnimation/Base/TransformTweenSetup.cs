using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.AnimationToolbox
{

	[Serializable]
	public class TransformTweenSetup
	{
		public float Delay = 0f;
		public float Duration = 0.5f;

		[HorizontalGroup("Movement")]
		public Ease MovementEase = Ease.OutCubic;
		[HorizontalGroup("Rotation")]
		public Ease RotationEase = Ease.OutCubic;

		[Required]
		public Transform EndLocation;

		[HorizontalGroup("Movement", MaxWidth = 120), Button(ButtonSizes.Small, Name = "Do Not Animate")]
		public void RemoveMovementAnimation()
		{
			MovementEase = Ease.Unset;
		}

		[HorizontalGroup("Rotation", MaxWidth = 120), Button(ButtonSizes.Small, Name = "Do Not Animate")]
		public void RemoveRotationAnimation()
		{
			RotationEase = Ease.Unset;
		}
	}

}
