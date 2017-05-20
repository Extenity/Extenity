using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Extenity.UIToolbox
{

	public static class UINavigationTools
	{
		public static bool NavigateToNextSelectableOnUp()
		{
			if (EventSystem.current == null)
				return false;
			var currentSelectable = EventSystem.current.currentSelectedGameObject;
			if (currentSelectable)
			{
				var next = currentSelectable.GetComponent<Selectable>().FindSelectableOnUp();
				return NavigateToSelectable(next);
			}
			return false;
		}

		public static bool NavigateToNextSelectableOnDown()
		{
			if (EventSystem.current == null)
				return false;
			var currentSelectable = EventSystem.current.currentSelectedGameObject;
			if (currentSelectable)
			{
				var next = currentSelectable.GetComponent<Selectable>().FindSelectableOnDown();
				return NavigateToSelectable(next);
			}
			return false;
		}

		public static bool NavigateToNextSelectableOnLeft()
		{
			if (EventSystem.current == null)
				return false;
			var currentSelectable = EventSystem.current.currentSelectedGameObject;
			if (currentSelectable)
			{
				var next = currentSelectable.GetComponent<Selectable>().FindSelectableOnLeft();
				return NavigateToSelectable(next);
			}
			return false;
		}

		public static bool NavigateToNextSelectableOnRight()
		{
			if (EventSystem.current == null)
				return false;
			var currentSelectable = EventSystem.current.currentSelectedGameObject;
			if (currentSelectable)
			{
				var next = currentSelectable.GetComponent<Selectable>().FindSelectableOnRight();
				return NavigateToSelectable(next);
			}
			return false;
		}

		public static bool NavigateToSelectable(Selectable selectable)
		{
			if (EventSystem.current == null || selectable == null)
				return false;

			if (EventSystem.current.alreadySelecting)
				return false;
			if (!EventSystem.current.sendNavigationEvents)
				return false;

			// If it's an input field, also set the text caret.
			var inputfield = selectable.GetComponent<InputField>();
			if (inputfield != null)
				inputfield.OnPointerClick(new PointerEventData(EventSystem.current));

			EventSystem.current.SetSelectedGameObject(selectable.gameObject, new BaseEventData(EventSystem.current));
			return true;
		}
	}

}
