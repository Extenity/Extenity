using UnityEngine;
using UnityEngine.EventSystems;

namespace Extenity.UIToolbox
{

	public static class UITools
	{
		#region Input

		public static bool IsGUIActiveInCurrentEventSystem
		{
			get
			{
				var eventSystem = EventSystem.current;
				if (eventSystem)
				{
					if (eventSystem.currentSelectedGameObject != null)
					{
						return true;
					}
				}
				return false;
			}
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
	}

}
