﻿#if UNITY

using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Extenity.AnimationToolbox
{

	[Serializable]
	public class TransformTweenSetup
	{
		[FormerlySerializedAs("EndLocation")]
		[Required]
		public Transform Location;

		public float Delay = 0f;
		public float Duration = 0.5f;

		[HorizontalGroup("Movement")]
		public Ease MovementEase = Ease.OutCubic;
		[HorizontalGroup("Rotation")]
		public Ease RotationEase = Ease.OutCubic;
		[HorizontalGroup("Scale")]
		public Ease ScaleEase = Ease.OutCubic;

		[HorizontalGroup("LocalPosition", LabelWidth = 110, MaxWidth = 10), ToggleLeft]
		public bool UseLocalPosition;
		[HorizontalGroup("LocalRotation", LabelWidth = 110, MaxWidth = 10), ToggleLeft]
		public bool UseLocalRotation;
		[HorizontalGroup("LocalPosition", LabelWidth = 110, MaxWidth = 10), ToggleLeft]
		public bool MoveInLocal;
		[HorizontalGroup("LocalRotation", LabelWidth = 110, MaxWidth = 10), ToggleLeft]
		public bool RotateInLocal;

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

		[HorizontalGroup("Scale", MaxWidth = 120), Button(ButtonSizes.Small, Name = "Do Not Animate")]
		public void RemoveScaleAnimation()
		{
			ScaleEase = Ease.Unset;
		}
	}

}

#endif
