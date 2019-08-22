using System;
using Extenity.MathToolbox;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Extenity.UIToolbox.TouchInput
{

	public enum StickType
	{
		Horizontal,
		Vertical,
		Radial,
		Square,
	}

	public class Stick : ElementBase<StickSchemeElement>,
	                     IPointerUpHandler,
	                     IPointerDownHandler,
	                     IDragHandler
		//IBeginDragHandler,
		//IEndDragHandler
	{
		#region Setup

		[BoxGroup("Configuration")]
		public StickType Type = StickType.Radial;

		[BoxGroup("Configuration")]
		[InfoBox("The Element returns to its default location after releasing the touch in Free Roam mode. This is the duration of that animation.")]
		public float ReturnToDefaultLocationDuration = 0.15f;

		[BoxGroup("Links"), PropertyOrder(10)]
		public RectTransform Handle = default;

		[BoxGroup("Links"), PropertyOrder(11)]
		public RectTransform HandleMovementArea = default;

		#endregion

		#region Initialization

		// protected override void Start()
		// {
		// 	base.Start();
		// }

		#endregion

		#region Deinitialization

		// protected override void OnDestroy()
		// {
		// 	base.OnDestroy();
		// }

		#endregion

		#region Update and Calculations

		protected override void CustomUpdate()
		{
			if (UseDeviceLeaning && !IsDragging)
			{
				CalculateLeaningInput();
			}

			base.CustomUpdate();
		}

		private void CalculateLeaningInput()
		{
			var acceleration = Input.acceleration;

			switch (Type)
			{
				case StickType.Horizontal:
					_StickPosition.x = acceleration.x * DeviceLeaningSensitivity;
					break;

				case StickType.Vertical:
					_StickPosition.y = acceleration.y * DeviceLeaningSensitivity;
					break;

				case StickType.Radial:
					_StickPosition.x = acceleration.x * DeviceLeaningSensitivity;
					_StickPosition.y = acceleration.y * DeviceLeaningSensitivity;
					break;

				case StickType.Square:
					_StickPosition.x = acceleration.x * DeviceLeaningSensitivity;
					_StickPosition.y = acceleration.y * DeviceLeaningSensitivity;
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		#endregion

		#region Scheme

		// Cached parameters of Scheme
		private RectTransform DefaultLocation = default;
		private bool EnableFreeRoam;
		private bool UseDeviceLeaning;
		private float DeviceLeaningSensitivity;
		private RectTransform ClickableArea = default;
		private GameObject ClickableAreaGameObject = default; // Cache optimization

		protected override void ApplySchemeElement(StickSchemeElement schemeElement)
		{
			if (schemeElement)
			{
				DefaultLocation = schemeElement.DefaultLocation;
				EnableFreeRoam = schemeElement.EnableFreeRoam;
				UseDeviceLeaning = schemeElement.UseDeviceLeaning;
				DeviceLeaningSensitivity = schemeElement.DeviceLeaningSensitivity;
				ClickableArea = schemeElement.ClickableArea;
				ClickableAreaGameObject = ClickableArea ? ClickableArea.gameObject : null;
				MoveTo(DefaultLocation, 0f);
			}
			else
			{
				DefaultLocation = null;
				EnableFreeRoam = false;
				UseDeviceLeaning = false;
				DeviceLeaningSensitivity = 0f;
				ClickableArea = null;
				ClickableAreaGameObject = null;
			}
		}

		#endregion

		#region Status

		[BoxGroup("Status")]
		[ShowInInspector, ReadOnly]
		public bool IsDragging { get; private set; }

		private Vector2 _StickPosition;

		[BoxGroup("Status")]
		[ShowInInspector, ReadOnly]
		public Vector2 StickPosition => _StickPosition;

		#endregion

		#region Touch

		public void OnDrag(PointerEventData eventData)
		{
			if (eventData.pointerPressRaycast.gameObject != ClickableAreaGameObject)
				return;

			IsDragging = true;

			switch (Type)
			{
				case StickType.Horizontal:
					var leverPosition = _StickPosition.x;
					eventData.CalculateHorizontalLeverDrag(HandleMovementArea, ClickableArea, Handle, ref leverPosition);
					_StickPosition.x = leverPosition;
					break;

				case StickType.Vertical:
					throw new NotImplementedException();

				case StickType.Radial:
					eventData.Calculate2DLeverDrag(HandleMovementArea, ClickableArea, Handle, true, ref _StickPosition);
					break;

				case StickType.Square:
					eventData.Calculate2DLeverDrag(HandleMovementArea, ClickableArea, Handle, false, ref _StickPosition);
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (eventData.pointerPressRaycast.gameObject != ClickableAreaGameObject)
				return;

			if (EnableFreeRoam)
			{
				MoveTo(eventData.pressPosition, 0f);
			}

			OnDrag(eventData);
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (eventData.pointerPressRaycast.gameObject != ClickableAreaGameObject)
				return;

			_StickPosition.x = 0f;
			_StickPosition.y = 0f;
			Handle.anchoredPosition = Vector3Tools.Zero;
			IsDragging = false;

			if (DefaultLocation)
			{
				MoveTo(DefaultLocation, ReturnToDefaultLocationDuration);
			}
		}

		#endregion
	}

}
