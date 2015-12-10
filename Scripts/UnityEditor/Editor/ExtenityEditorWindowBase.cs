using UnityEngine;
using UnityEditor;

public abstract class ExtenityEditorWindowBase : EditorWindow
{
	#region OnGUI

	protected abstract void OnGUIDerived();

	protected void OnGUI()
	{
		CalculateRightMouseButtonScrolling();
		OnGUIDerived();
	}

	#endregion

	#region Scroll

	protected Vector2 ScrollPosition = Vector2.zero;

	#endregion

	#region Scroll Window With Right Mouse Button

	private bool WasScrollingWithRightMouseButton;

	private void CalculateRightMouseButtonScrolling()
	{
		if (Event.current.isMouse)
		{
			if (Event.current.button == 1 && Event.current.type == EventType.mouseDrag)
			{
				ScrollPosition -= Event.current.delta;

				Event.current.Use();
				Repaint();

				WasScrollingWithRightMouseButton = true;
			}

			// Prevent any right click events if right click is used for scrolling.
			if (WasScrollingWithRightMouseButton && Event.current.type == EventType.mouseUp)
			{
				WasScrollingWithRightMouseButton = false;
				Event.current.Use();
				Repaint();
			}
		}
	}

	#endregion
}
