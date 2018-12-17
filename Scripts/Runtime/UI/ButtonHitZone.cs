using UnityEngine;
using UnityEngine.EventSystems;

namespace Extenity.UIToolbox
{

	public class ButtonHitZone : MonoBehaviour
	{
		#region Initialization

		private void Start()
		{
			CreateHitZone();
		}

		#endregion

		#region Hit Zone

		public float Width;
		public float Height;

		private void CreateHitZone()
		{
			var hitZoneGameObject = new GameObject("HitZone");
			var hitZoneTransform = hitZoneGameObject.AddComponent<RectTransform>();
			hitZoneTransform.SetParent(transform);
			hitZoneTransform.localPosition = Vector3.zero;
			hitZoneTransform.localScale = Vector3.one;
			hitZoneTransform.sizeDelta = new Vector2(Width, Height);

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
			var rectTransform = GetComponent<RectTransform>();
			if (rectTransform != null)
			{
				Width = Mathf.Max(Width, rectTransform.sizeDelta.x);
				Height = Mathf.Max(Height, rectTransform.sizeDelta.y);
			}
		}

#endif

		#endregion
	}

}
