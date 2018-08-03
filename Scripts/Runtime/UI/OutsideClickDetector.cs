using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Extenity.UIToolbox
{

	public class OutsideClickDetector : MonoBehaviour, IPointerClickHandler
	{
		public UnityEvent OnClickedOutside = new UnityEvent();

		public void OnPointerClick(PointerEventData eventData)
		{
			OnClickedOutside.Invoke();
			eventData.Use();
		}
	}

}
