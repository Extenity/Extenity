using System;
using UnityEngine;
using UnityEditor;

public abstract class ExtenityEditorWindowBase : EditorWindow
{
	#region OnGUI

	protected abstract void OnGUIDerived();

	protected void OnGUI()
	{
		if (RefreshTitleContentInNextRender)
		{
			titleContent = new GUIContent(_Title, _IconRetriever != null ? _IconRetriever() : null);
			_Title = null;
			_IconRetriever = null;
			RefreshTitleContentInNextRender = false;
		}

		CalculateRightMouseButtonScrolling();
		OnGUIDerived();
	}

	#endregion

	#region Title And Icon

	private string _Title;
	private Func<Texture2D> _IconRetriever;
	private bool RefreshTitleContentInNextRender;

	/// <summary>
	/// Call this inside OnEnable.
	/// </summary>
	public void SetTitleAndIcon(string title, Func<Texture2D> iconRetriever)
	{
		_Title = title;
		_IconRetriever = iconRetriever;
		RefreshTitleContentInNextRender = true; // This will prevent any possibilities of losing window icon. This could happen on relaunching the editor while window was left open.
	}

	#endregion

	#region Scroll

	protected Vector2 ScrollPosition = Vector2.zero;

	#endregion

	#region Scroll Window With Right Mouse Button

	public bool IsRightMouseButtonScrollingEnabled = false;
	private bool WasScrollingWithRightMouseButton;

	private void CalculateRightMouseButtonScrolling()
	{
		if (Event.current.isMouse)
		{
			if (IsRightMouseButtonScrollingEnabled && Event.current.button == 1 && Event.current.type == EventType.MouseDrag)
			{
				ScrollPosition -= Event.current.delta;

				Event.current.Use();
				Repaint();

				WasScrollingWithRightMouseButton = true;
			}

			// Prevent any right click events if right click is used for scrolling.
			if (WasScrollingWithRightMouseButton && Event.current.type == EventType.MouseUp)
			{
				WasScrollingWithRightMouseButton = false;
				Event.current.Use();
				Repaint();
			}
		}
	}

	#endregion
}
