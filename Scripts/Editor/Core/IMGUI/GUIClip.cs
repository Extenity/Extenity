using System;
using Extenity.ReflectionToolbox;
using UnityEngine;

namespace Extenity.IMGUIToolbox.Editor
{

	public sealed class GUIClip
	{
		static GUIClip()
		{
			// TODO: Update that in new Unity versions.
			// Revealed internals (Unity version 2018.1.1f1)
			var type = typeof(MonoBehaviour).Assembly.GetType("UnityEngine.GUIClip");
			if (type == null)
				throw new InternalException(48672); // See 837379.
			
			type.GetStaticMethodAsFunc("get_topmostRect", out _topmostRect);
			type.GetStaticMethodAsFunc("get_visibleRect", out _visibleRect);
			type.GetStaticMethodAsFunc("Unclip", out _UnclipVector2);
			type.GetStaticMethodAsFunc("Unclip", out _UnclipRect);
			type.GetStaticMethodAsFunc("Clip", out _ClipVector2);
			type.GetStaticMethodAsFunc("Clip", out _ClipRect);
			type.GetStaticMethodAsFunc("UnclipToWindow", out _UnclipToWindowVector2);
			type.GetStaticMethodAsFunc("UnclipToWindow", out _UnclipToWindowRect);
			type.GetStaticMethodAsFunc("ClipToWindow", out _ClipToWindowVector2);
			type.GetStaticMethodAsFunc("ClipToWindow", out _ClipToWindowRect);
			type.GetStaticMethodAsFunc("GetAbsoluteMousePosition", out _GetAbsoluteMousePosition);
		}

		//public static Rect topmostRect
		//public static Rect visibleRect
		//public static Vector2 Unclip(Vector2 pos)
		//public static Rect Unclip(Rect rect)
		//public static Vector2 Clip(Vector2 absolutePos)
		//public static Rect Clip(Rect absoluteRect)
		//public static Vector2 UnclipToWindow(Vector2 pos)
		//public static Rect UnclipToWindow(Rect rect)
		//public static Vector2 ClipToWindow(Vector2 absolutePos)
		//public static Rect ClipToWindow(Rect absoluteRect)
		//public static Vector2 GetAbsoluteMousePosition()

		// Decided not to give this a shot until we really need to reveal.
		//internal static Rect GetTopRect()

		// Note that GUIClip.GetMatrix and GUIClip.SetMatrix are publicly accessible in GUI.matrix getter and setter.
		//internal static Matrix4x4 GetMatrix()
		//internal static void SetMatrix(Matrix4x4 m)

		private static Func<Rect> _topmostRect;
		private static Func<Rect> _visibleRect;
		private static Func<Vector2, Vector2> _UnclipVector2;
		private static Func<Rect, Rect> _UnclipRect;
		private static Func<Vector2, Vector2> _ClipVector2;
		private static Func<Rect, Rect> _ClipRect;
		private static Func<Vector2, Vector2> _UnclipToWindowVector2;
		private static Func<Rect, Rect> _UnclipToWindowRect;
		private static Func<Vector2, Vector2> _ClipToWindowVector2;
		private static Func<Rect, Rect> _ClipToWindowRect;
		private static Func<Vector2> _GetAbsoluteMousePosition;

		public static Rect topmostRect { get { return _topmostRect(); } }
		public static Rect visibleRect { get { return _visibleRect(); } }
		public static Vector2 Unclip(Vector2 pos) { return _UnclipVector2(pos); }
		public static Rect Unclip(Rect rect) { return _UnclipRect(rect); }
		public static Vector2 Clip(Vector2 absolutePos) { return _ClipVector2(absolutePos); }
		public static Rect Clip(Rect absoluteRect) { return _ClipRect(absoluteRect); }
		public static Vector2 UnclipToWindow(Vector2 pos) { return _UnclipToWindowVector2(pos); }
		public static Rect UnclipToWindow(Rect rect) { return _UnclipToWindowRect(rect); }
		public static Vector2 ClipToWindow(Vector2 absolutePos) { return _ClipToWindowVector2(absolutePos); }
		public static Rect ClipToWindow(Rect absoluteRect) { return _ClipToWindowRect(absoluteRect); }
		public static Vector2 GetAbsoluteMousePosition() { return _GetAbsoluteMousePosition(); }
	}

}
