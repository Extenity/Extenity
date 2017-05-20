using UnityEngine;
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
