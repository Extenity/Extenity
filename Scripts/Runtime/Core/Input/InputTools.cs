using System.Collections.Generic;
using UnityEngine;

namespace Extenity.InputToolbox
{

	public static class InputTools
	{
		public static Vector2 MousePositionNormalizedInScreenHeightCoordinates
		{
			get
			{
				var position = Input.mousePosition;
				var oneOverHeight = 1f / Screen.height;
				return new Vector2(
					position.x * oneOverHeight,
					position.y * oneOverHeight);
			}
		}

		public static Vector2 MousePositionNormalizedInScreenWidthCoordinates
		{
			get
			{
				var position = Input.mousePosition;
				var oneOverWidth = 1f / Screen.width;
				return new Vector2(
					position.x * oneOverWidth,
					position.y * oneOverWidth);
			}
		}

		public static Vector3 MousePositionYInverted
		{
			get
			{
				return YInverted(Input.mousePosition);
			}
		}

		public static bool IsMouseInsideScreen
		{
			get
			{
				return (Input.mousePosition.x > 0 && Input.mousePosition.y > 0 && Input.mousePosition.x < Screen.width && Input.mousePosition.y < Screen.height);
			}
		}

		public static Vector3 YInverted(Vector3 position)
		{
			position.y = Screen.height - position.y;
			return position;
		}

		public static Vector2 YInverted(Vector2 position)
		{
			position.y = Screen.height - position.y;
			return position;
		}

		#region Touch

		public static readonly Touch InvalidTouch = new Touch();

		public static bool GetByFingerID(this IEnumerable<Touch> touches, int fingerID, out Touch result)
		{
			foreach (var touch in touches)
			{
				if (touch.fingerId == fingerID)
				{
					result = touch;
					return true;
				}
			}
			result = InvalidTouch;
			return false;
		}

		public static bool GetByTapCount(this IEnumerable<Touch> touches, int tapCount, out Touch result)
		{
			foreach (var touch in touches)
			{
				if (touch.tapCount == tapCount)
				{
					result = touch;
					return true;
				}
			}
			result = InvalidTouch;
			return false;
		}

		public static bool HasFingerID(this IEnumerable<Touch> touches, int fingerID)
		{
			foreach (var touch in touches)
			{
				if (touch.fingerId == fingerID)
				{
					return true;
				}
			}
			return false;
		}

		#endregion

		// Calculates position of the mouse cursor where cursor ray intersects with the y=0 plane
		public static Vector3 GetMousePositionOnGroundPlane()
		{
			return RaycastToGroundPlane(Input.mousePosition);
		}

		public static Vector3 RaycastToGroundPlane(Vector3 screenPoint)
		{
			Ray mouseRay = Camera.main.ScreenPointToRay(screenPoint);
			Plane groundPlane = new Plane(Vector3.up, 0f);

			groundPlane.Raycast(mouseRay, out var distance);

			return mouseRay.origin + mouseRay.direction * distance;
		}

		public static GameObject PickObjectWithMouse()
		{
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hitObject, Mathf.Infinity))
			{
				return hitObject.collider.gameObject;
			}
			return null;
		}

		public static bool GetKeyDown_TabOrESC { get { return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab); } }

		public static bool GetKey_Shift { get { return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift); } }
		public static bool GetKey_Alt { get { return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt); } }
		public static bool GetKey_Control { get { return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl); } }

		public static bool GetKeyDown_Shift { get { return Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift); } }
		public static bool GetKeyDown_Alt { get { return Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt); } }
		public static bool GetKeyDown_Control { get { return Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl); } }

		public static bool GetKeyUp_Shift { get { return Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift); } }
		public static bool GetKeyUp_Alt { get { return Input.GetKeyUp(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.RightAlt); } }
		public static bool GetKeyUp_Control { get { return Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl); } }

		public static bool GetKey_OnlyControl { get { return GetKey_Control && !GetKey_Shift && !GetKey_Alt; } }
		public static bool GetKey_OnlyShift { get { return !GetKey_Control && GetKey_Shift && !GetKey_Alt; } }
		public static bool GetKey_OnlyAlt { get { return !GetKey_Control && !GetKey_Shift && GetKey_Alt; } }

		public static bool GetKeyDown_OnlyControl { get { return GetKeyDown_Control && !GetKeyDown_Shift && !GetKeyDown_Alt; } }
		public static bool GetKeyDown_OnlyShift { get { return !GetKeyDown_Control && GetKeyDown_Shift && !GetKeyDown_Alt; } }
		public static bool GetKeyDown_OnlyAlt { get { return !GetKeyDown_Control && !GetKeyDown_Shift && GetKeyDown_Alt; } }

		public static bool GetKeyUp_OnlyControl { get { return GetKeyUp_Control && !GetKeyUp_Shift && !GetKeyUp_Alt; } }
		public static bool GetKeyUp_OnlyShift { get { return !GetKeyUp_Control && GetKeyUp_Shift && !GetKeyUp_Alt; } }
		public static bool GetKeyUp_OnlyAlt { get { return !GetKeyUp_Control && !GetKeyUp_Shift && GetKeyUp_Alt; } }

		public static bool GetKey_NoModifiers { get { return !GetKey_Control && !GetKey_Shift && !GetKey_Alt; } }
		public static bool GetKeyDown_NoModifiers { get { return !GetKeyDown_Control && !GetKeyDown_Shift && !GetKeyDown_Alt; } }
		public static bool GetKeyUp_NoModifiers { get { return !GetKeyUp_Control && !GetKeyUp_Shift && !GetKeyUp_Alt; } }
	}

}
