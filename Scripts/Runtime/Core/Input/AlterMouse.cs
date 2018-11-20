#if UNITY_STANDALONE_WIN
using System.Runtime.InteropServices;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#else
using System;
#endif
#endif

namespace Extenity.InputToolbox
{

	public static class AlterMouse
	{
#if UNITY_STANDALONE_WIN

		[DllImport("user32.dll")]
		private static extern int SetCursorPos(int x, int y);

#if UNITY_EDITOR
		public static void MoveMouseCursor(Vector2Int position)
		{
			var rect = EditorWindow.focusedWindow.position;

			position.x += (int)rect.x;
			position.y += (int)rect.y;
			position.y += 16;

			SetCursorPos(position.x, position.y);
		}
#else
		[DllImport("user32.dll")]
		private static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);
	
		[StructLayout(LayoutKind.Sequential)]
		public struct POINT
		{
			public int x;
			public int y;
		}

		public static void MoveMouseCursor(Vector2Int position)
		{
			POINT clientAreaScreenPosition = new POINT();
			ClientToScreen(GetForegroundWindow(), ref clientAreaScreenPosition);

			position.x += clientAreaScreenPosition.x;
			position.y += clientAreaScreenPosition.y;

			SetCursorPos(position.x, position.y);
		}
#endif
#endif
	}

}
