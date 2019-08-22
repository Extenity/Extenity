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

		[BoxGroup("Links"), PropertyOrder(10)]
		public RectTransform ButtonArea;

		#endregion

		#region Initialization

		// protected override void OnEnable()
		// {
		// 	base.OnEnable();
		//
		// 	Loop.UpdateCallbacks.AddListener(CustomUpdate, -1000);
		// }

		#endregion

		#region Deinitialization

		// protected override void OnDisable()
		// {
		// 	Loop.UpdateCallbacks.RemoveListener(CustomUpdate);
		//
		// 	base.OnDisable();
		// }

		#endregion

		#region Update and Calculations

		// private void CustomUpdate()
		// {
		// }

		#endregion

		#region Scheme

		// Cached parameters of Scheme
		private RectTransform DefaultLocation = default;
		private bool EnableFreeRoam;
		private RectTransform ClickableArea = default;
		private GameObject ClickableAreaGameObject = default; // Cache optimization

		protected override void ApplySchemeElement(ButtonSchemeElement schemeElement)
		{
			if (schemeElement)
			{
				DefaultLocation = schemeElement.DefaultLocation;
				EnableFreeRoam = schemeElement.EnableFreeRoam;
				ClickableArea = schemeElement.ClickableArea;
				ClickableAreaGameObject = ClickableArea ? ClickableArea.gameObject : null;
			}
			else
			{
				DefaultLocation = null;
				EnableFreeRoam = false;
				ClickableArea = null;
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
			if (eventData.pointerPressRaycast.gameObject != ClickableAreaGameObject)
				return;

			IsPressing = true;

			// TODO: Do value animation.
			_Value = 1f;
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (eventData.pointerPressRaycast.gameObject != ClickableAreaGameObject)
				return;

			IsPressing = false;

			// TODO: Do value animation.
			_Value = 0f;
		}

		#endregion
	}

}
