using UnityEngine;
using UnityEngine.EventSystems;

namespace Extenity.UIToolbox
{

	public class StandaloneImprovedInputModule : StandaloneInputModule
	{
		#region CurrentInput

		private static StandaloneImprovedInputModule _CurrentInput;
		public static StandaloneImprovedInputModule CurrentInput
		{
			get
			{
				if (_CurrentInput == null)
				{
					var current = EventSystem.current;
					if (current != null)
					{
						_CurrentInput = current.currentInputModule as StandaloneImprovedInputModule;
					}
					if (_CurrentInput == null)
					{
						Log.Error($"{nameof(StandaloneImprovedInputModule)} is expected.");
					}
				}
				return _CurrentInput;
			}
		}

		#endregion

		#region IsPointerOverGameObject

		public bool IsPointerOverGameObject()
		{
			return IsPointerOverGameObject(kMouseLeftId);
		}

		#endregion

		#region GameObjectUnderPointer

		/// <summary>
		/// Source: https://stackoverflow.com/questions/39150165/how-do-i-find-which-object-is-eventsystem-current-ispointerovergameobject-detect
		/// </summary>
		public GameObject GameObjectUnderPointer(int pointerId)
		{
			var lastPointer = GetLastPointerEventData(pointerId);
			if (lastPointer != null)
				return lastPointer.pointerCurrentRaycast.gameObject;
			return null;
		}

		/// <summary>
		/// Source: https://stackoverflow.com/questions/39150165/how-do-i-find-which-object-is-eventsystem-current-ispointerovergameobject-detect
		/// </summary>
		public GameObject GameObjectUnderPointer()
		{
			return GameObjectUnderPointer(kMouseLeftId);
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(StandaloneImprovedInputModule));

		#endregion
	}

}
