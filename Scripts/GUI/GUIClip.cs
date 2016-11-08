using System;
using UnityEngine;

namespace Extenity.UIToolbox
{

	public sealed class GUIClip
	{
		static GUIClip()
		{
			var guiClipType = typeof(MonoBehaviour).Assembly.GetType("UnityEngine.GUIClip");

			guiClipType.GetStaticMethodAsFunc("get_topmostRect", out _topmostRect);
			guiClipType.GetStaticMethodAsFunc("get_visibleRect", out _visibleRect);
			guiClipType.GetStaticMethodAsFunc("Unclip", out _UnclipVector2);
			guiClipType.GetStaticMethodAsFunc("Unclip", out _UnclipRect);
			guiClipType.GetStaticMethodAsFunc("Clip", out _ClipVector2);
			guiClipType.GetStaticMethodAsFunc("Clip", out _ClipRect);
			guiClipType.GetStaticMethodAsFunc("GetAbsoluteMousePosition", out _GetAbsoluteMousePosition);
		}

		//Rect topmostRect
		//Rect visibleRect
		//Vector2 Unclip(Vector2 pos)
		//Rect Unclip(Rect rect)
		//Vector2 Clip(Vector2 absolutePos)
		//Rect Clip(Rect absoluteRect)
		//Vector2 GetAbsoluteMousePosition()

		private static Func<Rect> _topmostRect;
		private static Func<Rect> _visibleRect;
		private static Func<Vector2, Vector2> _UnclipVector2;
		private static Func<Rect, Rect> _UnclipRect;
		private static Func<Vector2, Vector2> _ClipVector2;
		private static Func<Rect, Rect> _ClipRect;
		private static Func<Vector2> _GetAbsoluteMousePosition;

		public static Rect topmostRect { get { return _topmostRect(); } }
		public static Rect visibleRect { get { return _visibleRect(); } }
		public static Vector2 Unclip(Vector2 pos) { return _UnclipVector2(pos); }
		public static Rect Unclip(Rect rect) { return _UnclipRect(rect); }
		public static Vector2 Clip(Vector2 absolutePos) { return _ClipVector2(absolutePos); }
		public static Rect Clip(Rect absoluteRect) { return _ClipRect(absoluteRect); }
		public static Vector2 GetAbsoluteMousePosition() { return _GetAbsoluteMousePosition(); }
	}

}
