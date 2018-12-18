#if UNITY_STANDALONE_WIN
using System;
#endif
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using System.Runtime.InteropServices;
#endif

namespace Extenity.ApplicationToolbox
{

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

	public enum AsyncKeyCodes
	{
		R = 82,
		X = 88,
		Z = 90,
		RWin = 91,
		RShift = 160,
		RControl = 162,
	}

#endif

	public static class OperatingSystemTools
	{
		#region GetAsyncKeyState

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

		[DllImport("user32.dll")]
		static extern short GetAsyncKeyState(int vKey);

		public static bool IsAsyncKeyStateDown(AsyncKeyCodes key)
		{
			return (GetAsyncKeyState((int)key) & 0x8000) != 0;
		}

#else

		public static void IsAsyncKeyStateDown(AsyncKeyCodes key)
		{
			throw new System.NotImplementedException();
		}

#endif

		#endregion

		#region ChangeWindowTitle

#if UNITY_EDITOR_WIN

		public static void ChangeWindowTitle(string newTitle)
		{
			// ignored
		}

#elif UNITY_STANDALONE_WIN

		[DllImport("user32.dll", EntryPoint = "GetActiveWindow")]
		public static extern IntPtr Extern_GetActiveWindow();
		[DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "SetWindowText")]
		public static extern bool Extern_SetWindowText(IntPtr hwnd, string lpString);
		[DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "FindWindow")]
		public static extern IntPtr Extern_FindWindow(string className, string windowName);

		public static void ChangeWindowTitle(string newTitle)
		{
			var handle = Extern_GetActiveWindow();
			Extern_SetWindowText(handle, newTitle);
		}

#else

		public static void ChangeWindowTitle(string newTitle)
		{
			throw new System.NotImplementedException();
		}

#endif

		#endregion
	}

}
