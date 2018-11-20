#if UNITY_EDITOR_WIN
#else
using System;
#if UNITY_STANDALONE_WIN
using System.Runtime.InteropServices;
#endif
#endif

namespace Extenity.ApplicationToolbox
{

	public static class OperatingSystemTools
	{
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
			throw new NotImplementedException();
		}

#endif
	}

}
