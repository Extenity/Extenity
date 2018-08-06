using UnityEditor;
using UnityEngine;

namespace Extenity.UnityEditorToolbox.Editor
{

	public enum DragHandleResult
	{
		None = 0,

		LeftPress,
		LeftClick,
		LeftDoubleClick,
		LeftDrag,
		LeftRelease,

		RightPress,
		RightClick,
		RightDoubleClick,
		RightDrag,
		RightRelease,
	};

	public static class HandleTools
	{
		#region Drag Handle

		private static int _DragHandleHash = "DragHandleHash".GetHashCode();
		private static Vector2 _DragHandleMouseStart;
		private static Vector2 _DragHandleMouseCurrent;
		private static Vector3 _DragHandleWorldStart;
		private static float _DragHandleClickTime = 0;
		private static int _DragHandleClickID;
		private static float _DragHandleDoubleClickInterval = 0.5f;
		private static bool _DragHandleHasMoved;

		// Externally accessible to get the ID of the most recently processed DragHandle
		public static int LastDragHandleID;

		/// <summary>
		/// Based on: https://answers.unity.com/questions/463207/how-do-you-make-a-custom-handle-respond-to-the-mou.html
		/// </summary>
		public static Vector3 DragHandle(Vector3 position, Quaternion rotation, float handleSize, Handles.CapFunction capFunc, Color color, Color selectedColor, out DragHandleResult result)
		{
			var currentEvent = Event.current;
			var button = currentEvent.button;

			var id = GUIUtility.GetControlID(_DragHandleHash, FocusType.Passive);
			LastDragHandleID = id;

			var screenPosition = Handles.matrix.MultiplyPoint(position);
			var cachedMatrix = Handles.matrix;

			result = DragHandleResult.None;

			switch (currentEvent.GetTypeForControl(id))
			{
				case EventType.MouseDown:
					if (HandleUtility.nearestControl == id && (button == 0 || button == 1))
					{
						GUIUtility.hotControl = id;
						_DragHandleMouseCurrent = _DragHandleMouseStart = currentEvent.mousePosition;
						_DragHandleWorldStart = position;
						_DragHandleHasMoved = false;

						currentEvent.Use();
						EditorGUIUtility.SetWantsMouseJumping(1);

						if (button == 0)
							result = DragHandleResult.LeftPress;
						else if (button == 1)
							result = DragHandleResult.RightPress;
					}
					break;

				case EventType.MouseUp:
					if (GUIUtility.hotControl == id && (button == 0 || button == 1))
					{
						GUIUtility.hotControl = 0;
						currentEvent.Use();
						EditorGUIUtility.SetWantsMouseJumping(0);

						if (button == 0)
							result = DragHandleResult.LeftRelease;
						else if (button == 1)
							result = DragHandleResult.RightRelease;

						if (currentEvent.mousePosition == _DragHandleMouseStart)
						{
							var doubleClick = (_DragHandleClickID == id) && (Time.realtimeSinceStartup - _DragHandleClickTime < _DragHandleDoubleClickInterval);

							_DragHandleClickID = id;
							_DragHandleClickTime = Time.realtimeSinceStartup;

							if (button == 0)
								result = doubleClick ? DragHandleResult.LeftDoubleClick : DragHandleResult.LeftClick;
							else if (button == 1)
								result = doubleClick ? DragHandleResult.RightDoubleClick : DragHandleResult.RightClick;
						}
					}
					break;

				case EventType.MouseDrag:
					if (GUIUtility.hotControl == id)
					{
						_DragHandleMouseCurrent += new Vector2(currentEvent.delta.x, -currentEvent.delta.y);
						var position2 = Camera.current.WorldToScreenPoint(Handles.matrix.MultiplyPoint(_DragHandleWorldStart))
							+ (Vector3)(_DragHandleMouseCurrent - _DragHandleMouseStart);
						position = Handles.matrix.inverse.MultiplyPoint(Camera.current.ScreenToWorldPoint(position2));

						if (Camera.current.transform.forward == Vector3.forward || Camera.current.transform.forward == -Vector3.forward)
							position.z = _DragHandleWorldStart.z;
						if (Camera.current.transform.forward == Vector3.up || Camera.current.transform.forward == -Vector3.up)
							position.y = _DragHandleWorldStart.y;
						if (Camera.current.transform.forward == Vector3.right || Camera.current.transform.forward == -Vector3.right)
							position.x = _DragHandleWorldStart.x;

						if (button == 0)
							result = DragHandleResult.LeftDrag;
						else if (button == 1)
							result = DragHandleResult.RightDrag;

						_DragHandleHasMoved = true;

						GUI.changed = true;
						currentEvent.Use();
					}
					break;

				case EventType.Repaint:
					{
						var currentColour = Handles.color;
						Handles.color = id == GUIUtility.hotControl && _DragHandleHasMoved
							? selectedColor
							: color;

						Handles.matrix = Matrix4x4.identity;
						capFunc(id, screenPosition, rotation, handleSize, currentEvent.type);
						Handles.matrix = cachedMatrix;

						Handles.color = currentColour;
					}
					break;

				case EventType.Layout:
					{
						Handles.matrix = Matrix4x4.identity;
						HandleUtility.AddControl(id, HandleUtility.DistanceToCircle(screenPosition, handleSize));
						Handles.matrix = cachedMatrix;
					}
					break;
			}

			return position;
		}

		#endregion

		#region Lable Clipped

		public static void LabelClipped(Vector3 position, string text)
		{
			var camera = Camera.current;
			var viewportPoint = camera.WorldToViewportPoint(position);
			if (viewportPoint.z > 0f && // Checking for Z prevents drawing labels behind the camera.
				viewportPoint.x > 0f && viewportPoint.x < 1f &&
				viewportPoint.y > 0f && viewportPoint.y < 1f)
			{
				Handles.Label(position, text);
			}
		}

		#endregion
	}

}
