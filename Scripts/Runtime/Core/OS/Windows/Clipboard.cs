using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
#if UNITY
using UnityEngine;
#endif

namespace Extenity.OperatingSystemToolbox
{

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

	public class Clipboard : ApplicationToolbox.Clipboard
	{
		#region Initialize

		internal static bool IsInitialized;

#if UNITY_EDITOR
		[UnityEditor.InitializeOnLoadMethod]
#endif
		[RuntimeInitializeOnLoadMethod]
		public static void Initialize()
		{
			if (IsInitialized)
				return;
			IsInitialized = true;
			Instance = new Clipboard();
		}

		#endregion


		enum ClipboardFormats : int
		{
			Text = 1,
			Bitmap = 2,
			SymbolicLink = 4,
			Dif = 5,
			Tiff = 6,
			OemText = 7,
			Dib = 8,
			Palette = 9,
			PenData = 10,
			Riff = 11,
			WaveAudio = 12,
			UnicodeText = 13,
		}

		[DllImport("User32.dll", EntryPoint = "OpenClipboard", SetLastError = true)]
		private static extern bool OpenClipboard(IntPtr hWndNewOwner);

		[DllImport("User32.dll", EntryPoint = "CloseClipboard", SetLastError = true)]
		private static extern bool CloseClipboard();

		[DllImport("User32.dll", EntryPoint = "EmptyClipboard", SetLastError = true)]
		private static extern bool EmptyClipboard();

		[DllImport("User32.dll", EntryPoint = "SetClipboardData", SetLastError = true)]
		private static extern IntPtr SetClipboardData(ClipboardFormats uFormat, IntPtr hMem);

		[DllImport("User32.dll", EntryPoint = "GetClipboardData", SetLastError = true)]
		private static extern IntPtr GetClipboardData(ClipboardFormats uFormat);

		[DllImport("User32.dll", EntryPoint = "IsClipboardFormatAvailable", SetLastError = true)]
		private static extern bool IsClipboardFormatAvailable(ClipboardFormats uFormat);

		//[DllImport("User32.dll", EntryPoint = "CountClipboardFormats", SetLastError = true)]
		//private static extern int CountClipboardFormats();

		//[DllImport("User32.dll", EntryPoint = "EnumClipboardFormats", SetLastError = true)]
		//private static extern ClipboardFormats EnumClipboardFormats(ClipboardFormats uFormat);

		[DllImport("kernel32.dll")]
		static extern IntPtr GlobalLock(IntPtr hMem);
		[DllImport("kernel32.dll")]
		static extern bool GlobalUnlock(IntPtr hMem);


		protected override void DoSetClipboardText(string text, bool log)
		{
			if (!OpenClipboard(IntPtr.Zero))
			{
				throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to open clipboard");
			}

			IntPtr memory = Marshal.StringToHGlobalUni(text);

			if (!EmptyClipboard())
			{
				throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to empty clipboard");
			}

			if (SetClipboardData(ClipboardFormats.UnicodeText, memory) == IntPtr.Zero)
			{
				throw new Win32Exception("Failed to set clipboard data");
			}

			if (!CloseClipboard())
			{
				throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to close clipboard");
			}

			if (log)
			{
				Log.With("Clipboard").Info("Copied to clipboard: " + text);
			}
		}

		protected override string DoGetClipboardText()
		{
			if (!IsClipboardFormatAvailable(ClipboardFormats.UnicodeText))
				return null;
			if (!OpenClipboard(IntPtr.Zero))
				return null;

			string data = null;
			var hGlobal = GetClipboardData(ClipboardFormats.UnicodeText);
			if (hGlobal != IntPtr.Zero)
			{
				var stringPointer = GlobalLock(hGlobal);
				if (stringPointer != IntPtr.Zero)
				{
					data = Marshal.PtrToStringUni(stringPointer);
					GlobalUnlock(stringPointer);
				}
			}
			CloseClipboard();

			return data;
		}
	}

#endif

}
