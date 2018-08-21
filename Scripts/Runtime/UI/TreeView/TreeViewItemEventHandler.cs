using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Extenity.UIToolbox
{

	public class TreeViewItemEventHandler : MonoBehaviour,
		IBeginDragHandler, IDragHandler, IEndDragHandler,
		IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
	{
		public Action<PointerEventData> onBeginDrag;
		public Action<PointerEventData> onDrag;
		public Action<PointerEventData> onEndDrag;
		public Action<PointerEventData> onPointerClick;
		public Action<PointerEventData> onPointerEnter;
		public Action<PointerEventData> onPointerExit;

		public void OnBeginDrag(PointerEventData eventData)
		{
			if (onBeginDrag != null)
				onBeginDrag(eventData);
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (onDrag != null)
				onDrag(eventData);
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			if (onEndDrag != null)
				onEndDrag(eventData);
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (onPointerClick != null)
				onPointerClick(eventData);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (onPointerEnter != null)
				onPointerEnter(eventData);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (onPointerExit != null)
				onPointerExit(eventData);
		}
	}
}