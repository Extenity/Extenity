using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Extenity.UIToolbox.TouchInput
{

	public enum ButtonType
	{
		Push,
		// Toggle,
	}

	public class Button : ElementBase<ButtonSchemeElement>,
	                      IPointerUpHandler,
	                      IPointerDownHandler
	{
		#region Setup

		[BoxGroup("Configuration")]
		public ButtonType Type = ButtonType.Push;

		[BoxGroup("Configuration")]
		[InfoBox("The Element returns to its default location after releasing the touch in Free Roam mode. This is the duration of that animation.")]
		public float ReturnToDefaultLocationDuration = 0.15f;

		// [BoxGroup("Links"), PropertyOrder(10)]
		// public RectTransform ButtonArea;

		#endregion

		#region Scheme

		// Cached parameters of Scheme
		private RectTransform DefaultLocation = default;
		private bool EnableFreeRoam;
		// private RectTransform ClickableArea = default;
		private GameObject ClickableAreaGameObject = default; // Cache optimization

		protected override void ApplySchemeElement(ButtonSchemeElement schemeElement)
		{
			if (schemeElement)
			{
				DefaultLocation = schemeElement.DefaultLocation;
				EnableFreeRoam = schemeElement.EnableFreeRoam;
				var ClickableArea = schemeElement.ClickableArea;
				ClickableAreaGameObject = ClickableArea ? ClickableArea.gameObject : null;
				MoveTo(DefaultLocation, 0f);
			}
			else
			{
				DefaultLocation = null;
				EnableFreeRoam = false;
				// ClickableArea = null;
				ClickableAreaGameObject = null;
			}
		}

		#endregion

		#region Status

		[BoxGroup("Status")]
		[ShowInInspector, ReadOnly]
		public bool IsPressing { get; private set; }

		private float _Value;

		[BoxGroup("Status")]
		[ShowInInspector, ReadOnly]
		public float Value => _Value;

		#endregion

		#region Touch

		public void OnPointerDown(PointerEventData eventData)
		{
			IsPressing = true;

			if (EnableFreeRoam)
			{
				MoveTo(eventData.pressPosition, 0f);
			}

			// TODO: Do value animation.
			_Value = 1f;
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			IsPressing = false;

			if (DefaultLocation)
			{
				MoveTo(DefaultLocation, ReturnToDefaultLocationDuration);
			}

			// TODO: Do value animation.
			_Value = 0f;
		}

		#endregion
	}

}
