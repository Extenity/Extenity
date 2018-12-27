using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Extenity.UIToolbox
{

	public enum ButtonHitZoneSizing
	{
		Exact,
		AdditivePixels,
		AdditiveProportional,
	}

	public class ButtonHitZone : MonoBehaviour
	{
		#region Initialization

		private void Start()
		{
			CreateHitZone();
		}

		#endregion

		#region Hit Zone

		public ButtonHitZoneSizing Sizing = ButtonHitZoneSizing.Exact;
		public float Width;
		public float Height;

		private void CreateHitZone()
		{
			var hitZoneGameObject = new GameObject("HitZone");
			var hitZoneTransform = hitZoneGameObject.AddComponent<RectTransform>();
			hitZoneTransform.SetParent(transform);
			hitZoneTransform.localPosition = Vector3.zero;
			hitZoneTransform.localScale = Vector3.one;
			switch (Sizing)
			{
				case ButtonHitZoneSizing.Exact:
					hitZoneTransform.sizeDelta = new Vector2(Width, Height);
					break;
				case ButtonHitZoneSizing.AdditivePixels:
					hitZoneTransform.sizeDelta = GetComponent<RectTransform>().sizeDelta + new Vector2(Width, Height);
					break;
				case ButtonHitZoneSizing.AdditiveProportional:
					hitZoneTransform.sizeDelta = Vector2.Scale(GetComponent<RectTransform>().sizeDelta, new Vector2(Width, Height));
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			// create transparent graphic
			hitZoneGameObject.AddComponent<DummyGraphic>();

			// delegate events
			var eventTrigger = hitZoneGameObject.AddComponent<EventTrigger>();
			eventTrigger.AddListener(EventTriggerType.PointerDown,
				data =>
				{
					ExecuteEvents.Execute(gameObject, data, ExecuteEvents.pointerDownHandler);
				});
			eventTrigger.AddListener(EventTriggerType.PointerUp,
				data =>
				{
					ExecuteEvents.Execute(gameObject, data, ExecuteEvents.pointerUpHandler);
				});
			eventTrigger.AddListener(EventTriggerType.PointerClick,
				data =>
				{
					ExecuteEvents.Execute(gameObject, data, ExecuteEvents.pointerClickHandler);
				});
		}

		#endregion

		#region Editor

#if UNITY_EDITOR

		private void OnValidate()
		{
			// Make sure the hit area size is not lesser than the button's actual size.
			// Why anyone would want to do that? If a reason pops up in future, just delete these lines and move on.
			var rectTransform = GetComponent<RectTransform>();
			if (rectTransform != null)
			{
				switch (Sizing)
				{
					case ButtonHitZoneSizing.Exact:
						{
							Width = Mathf.Max(Width, rectTransform.sizeDelta.x);
							Height = Mathf.Max(Height, rectTransform.sizeDelta.y);
						}
						break;
					case ButtonHitZoneSizing.AdditivePixels:
						{
							if (Width < 0f) Width = 0f;
							if (Height < 0f) Height = 0f;
						}
						break;
					case ButtonHitZoneSizing.AdditiveProportional:
						{
							if (Width < 1f) Width = 1f;
							if (Height < 1f) Height = 1f;
						}
						break;
				}
			}
		}

#endif

		#endregion
	}

}
