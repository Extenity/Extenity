using System.Collections;
using Extenity.GameObjectToolbox;
using Extenity.MathToolbox;
using Extenity.ParallelToolbox;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Extenity.UIToolbox
{

	public static class UITools
	{
		public static void SetNormalColorAlpha(this Button me, float value)
		{
			var colors = me.colors;
			colors.normalColor = new Color(colors.normalColor.r, colors.normalColor.g, colors.normalColor.b, value);
			me.colors = colors;
		}

		#region Simulate Button Click

		public static void SimulateButtonClick(this Selectable selectable)
		{
			CoroutineTask.Create(DoSimulateButtonClick(selectable));
		}

		private static IEnumerator DoSimulateButtonClick(Selectable selectable)
		{
			if (!selectable)
				yield break;
			var go = selectable.gameObject;
			var pointer = new PointerEventData(EventSystem.current);
			ExecuteEvents.Execute(go, pointer, ExecuteEvents.pointerEnterHandler);
			if (!selectable || !go)
				yield break;
			yield return null;
			ExecuteEvents.Execute(go, pointer, ExecuteEvents.pointerDownHandler);
			if (!selectable || !go)
				yield break;
			yield return null;
			ExecuteEvents.Execute(go, pointer, ExecuteEvents.submitHandler);
			if (!selectable || !go)
				yield break;
			yield return null;
			ExecuteEvents.Execute(go, pointer, ExecuteEvents.pointerUpHandler);
			if (!selectable || !go)
				yield break;
			yield return null;
			ExecuteEvents.Execute(go, pointer, ExecuteEvents.pointerExitHandler);
		}

		#endregion

		#region Register To Events

		public static void RegisterToEvent(this Selectable me, EventTriggerType eventTriggerType, UnityAction<BaseEventData> callback)
		{
			var trigger = me.gameObject.GetSingleOrAddComponent<EventTrigger>();
			var entry = new EventTrigger.Entry();
			entry.eventID = eventTriggerType;
			entry.callback.AddListener(callback);
			trigger.triggers.Add(entry);
		}

		public static void DeregisterFromEvent(this Selectable me, EventTriggerType eventTriggerType, UnityAction<BaseEventData> callback)
		{
			var trigger = me.gameObject.GetComponent<EventTrigger>();
			if (!trigger)
				return;
			foreach (var entry in trigger.triggers)
			{
				if (entry.eventID == eventTriggerType)
				{
					entry.callback.RemoveListener(callback);
				}
			}
		}

		#endregion

		#region Input

		// See 11637281.
		public static bool IsPointerOverUIWithPrimaryTouchesInCurrentEventSystem
		{
			get
			{
				return EventSystem.current.IsPointerOverUIWithPrimaryTouches();
			}
		}

		/// <summary>
		/// Source: https://forum.unity.com/threads/ispointerovereventsystemobject-always-returns-false-on-mobile.265372/
		/// </summary>
		public static bool IsPointerOverUIWithPrimaryTouches(this EventSystem eventSystem)
		{
			if (!eventSystem)
				return false;

			// Check for mouse. Calling without parameters means it's called with PointerInputModule.kMouseLeftId
			// parameter that checks if the mouse is hovering over a UI object. Don't mind the MouseLeftId, since we are
			// not interested in any buttons. Note that it does not cover touch input. See below.
			if (eventSystem.IsPointerOverGameObject())
			{
				return true;
			}

			// Check for touches (actually only the first touch which is considered the primary input).
			if (Input.touchCount > 0)
			{
				if (eventSystem.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsEnterHit
		{
			get { return Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Return); }
		}

		public static void DisableTabTravel()
		{
			if (Event.current.keyCode == KeyCode.Tab || Event.current.character == '\t')
				Event.current.Use();
		}

		#endregion

		#region Drag

		/// <summary>
		/// Note that this method is not tested in depth so there is a possibility that the method may give wrong results under some edge conditions.
		/// </summary>
		public static void Calculate2DLeverDrag(this PointerEventData eventData, RectTransform dragAreaTransform, RectTransform hitAreaTransform, RectTransform handleTransform, bool radial, ref Vector2 normalizedLeverPosition)
		{
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(hitAreaTransform, eventData.position, eventData.pressEventCamera, out _))
			{
				RectTransformUtility.ScreenPointToLocalPointInRectangle(dragAreaTransform, eventData.position, eventData.pressEventCamera, out var positionInDragArea);

				var dragAreaSize = dragAreaTransform.sizeDelta;

				normalizedLeverPosition = new Vector2(
					(positionInDragArea.x / dragAreaSize.x) * 2f,
					(positionInDragArea.y / dragAreaSize.y) * 2f);

				if (radial)
				{
					// Radial input
					normalizedLeverPosition = normalizedLeverPosition.ClampLength01();
				}
				else
				{
					// Rectangular input
					normalizedLeverPosition.x = Mathf.Clamp(normalizedLeverPosition.x, -1f, 1f);
					normalizedLeverPosition.y = Mathf.Clamp(normalizedLeverPosition.y, -1f, 1f);
				}

				handleTransform.anchoredPosition = new Vector2(
					normalizedLeverPosition.x * (dragAreaSize.x / 2f),
					normalizedLeverPosition.y * (dragAreaSize.y / 2f));
			}
		}

		/// <summary>
		/// Note that this method is not tested in depth so there is a possibility that the method may give wrong results under some edge conditions.
		/// </summary>
		public static void CalculateHorizontalLeverDrag(this PointerEventData eventData, RectTransform dragAreaTransform, RectTransform hitAreaTransform, RectTransform handleTransform, ref float normalizedLeverPosition)
		{
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(hitAreaTransform, eventData.position, eventData.pressEventCamera, out _))
			{
				RectTransformUtility.ScreenPointToLocalPointInRectangle(dragAreaTransform, eventData.position, eventData.pressEventCamera, out var positionInDragArea);

				var dragAreaSize = dragAreaTransform.sizeDelta.x;

				normalizedLeverPosition = Mathf.Clamp((positionInDragArea.x / dragAreaSize) * 2f, -1f, 1f);

				handleTransform.anchoredPosition = new Vector2(
					normalizedLeverPosition * (dragAreaSize / 2f),
					0);
			}
		}

		#endregion

		#region RectTransform

		/// <summary>
		/// Converts RectTransform.rect to screen space.
		/// Note that this method is not tested in depth so there is a possibility that the method may give wrong results in some unthought conditions.
		/// Source: https://answers.unity.com/questions/1013011/convert-recttransform-rect-to-screen-space.html
		/// </summary>
		public static Rect RectInScreenSpace(this RectTransform transform)
		{
			var size = Vector2.Scale(transform.rect.size, transform.lossyScale);
			var rect = new Rect(transform.position.x, Screen.height - transform.position.y, size.x, size.y);
			rect.x -= (transform.pivot.x * size.x);
			rect.y -= ((1.0f - transform.pivot.y) * size.y);
			return rect;
		}

		public static void MoveTo(this RectTransform transform, RectTransform target, bool setAnchor, bool setPivot)
		{
			if (setPivot)
			{
				transform.pivot = target.pivot;
			}
			if (setAnchor)
			{
				transform.anchorMin = target.anchorMin;
				transform.anchorMax = target.anchorMax;
			}
			transform.anchoredPosition = target.anchoredPosition;
		}

		#endregion

		#region Layout Rebuild

		public static void RebuildLayoutFromGroundUp(this RectTransform parentTransform)
		{
			Canvas.ForceUpdateCanvases();

			var layoutGroups = parentTransform.GetComponentsInChildren<LayoutGroup>();
			foreach (var layoutGroup in layoutGroups)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
			}

			var contentSizeFitters = parentTransform.GetComponentsInChildren<ContentSizeFitter>();
			foreach (var contentSizeFitter in contentSizeFitters)
			{
				contentSizeFitter.SetLayoutVertical();
			}
		}

		#endregion
	}

}
