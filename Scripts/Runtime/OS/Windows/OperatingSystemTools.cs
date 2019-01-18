#if UNITY_STANDALONE_WIN
using System;
#endif
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using System.Runtime.InteropServices;
#endif

namespace Extenity.ApplicationToolbox
{

	#region Virtual KeyCodes

	public enum VirtualKeyCode : int
	{
		LButton = 0x01,
		RButton = 0x02,
		Cancel = 0x03,
		MButton = 0x04,    // NOT contiguous with L & RButton

		XButton1 = 0x05,    // NOT contiguous with L & RButton
		XButton2 = 0x06,    // NOT contiguous with L & RButton

		// 0x07 : unassigned

		Back = 0x08,
		Tab = 0x09,

		// 0x0A - 0x0B : reserved

		Clear = 0x0C,
		Return = 0x0D,

		Shift = 0x10,
		Control = 0x11,
		Menu = 0x12,
		Pause = 0x13,
		Capital = 0x14,

		/*
		KANA = 0x15,
		HANGEUL = 0x15,  // old name - should be here for compatibility
		HANGUL = 0x15,
		JUNJA = 0x17,
		FINAL = 0x18,
		HANJA = 0x19,
		KANJI = 0x19,
		*/

		Escape = 0x1B,

		/*
		CONVERT = 0x1C,
		NONCONVERT = 0x1D,
		ACCEPT = 0x1E,
		MODECHANGE = 0x1F,
		*/

		Space = 0x20,
		Prior = 0x21,
		Next = 0x22,
		End = 0x23,
		Home = 0x24,
		Left = 0x25,
		Up = 0x26,
		Right = 0x27,
		Down = 0x28,
		Select = 0x29,
		Print = 0x2A,
		Execute = 0x2B,
		Snapshot = 0x2C,
		Insert = 0x2D,
		Delete = 0x2E,
		Help = 0x2F,

		Alpha0 = 0x30,
		Alpha1 = 0x31,
		Alpha2 = 0x32,
		Alpha3 = 0x33,
		Alpha4 = 0x34,
		Alpha5 = 0x35,
		Alpha6 = 0x36,
		Alpha7 = 0x37,
		Alpha8 = 0x38,
		Alpha9 = 0x39,
		// 0x40 : unassigned
		A = 0x41,
		B = 0x42,
		C = 0x43,
		D = 0x44,
		E = 0x45,
		F = 0x46,
		G = 0x47,
		H = 0x48,
		I = 0x49,
		J = 0x4A,
		K = 0x4B,
		L = 0x4C,
		M = 0x4D,
		N = 0x4E,
		O = 0x4F,
		P = 0x50,
		Q = 0x51,
		R = 0x52,
		S = 0x53,
		T = 0x54,
		U = 0x55,
		V = 0x56,
		W = 0x57,
		X = 0x58,
		Y = 0x59,
		Z = 0x5A,

		LWin = 0x5B,
		RWin = 0x5C,
		//Apps = 0x5D,

		// 0x5E : reserved

		Sleep = 0x5F,

		Numpad0 = 0x60,
		Numpad1 = 0x61,
		Numpad2 = 0x62,
		Numpad3 = 0x63,
		Numpad4 = 0x64,
		Numpad5 = 0x65,
		Numpad6 = 0x66,
		Numpad7 = 0x67,
		Numpad8 = 0x68,
		Numpad9 = 0x69,
		Multiply = 0x6A,
		Add = 0x6B,
		Separator = 0x6C,
		Subtract = 0x6D,
		Decimal = 0x6E,
		Divide = 0x6F,
		F1 = 0x70,
		F2 = 0x71,
		F3 = 0x72,
		F4 = 0x73,
		F5 = 0x74,
		F6 = 0x75,
		F7 = 0x76,
		F8 = 0x77,
		F9 = 0x78,
		F10 = 0x79,
		F11 = 0x7A,
		F12 = 0x7B,
		F13 = 0x7C,
		F14 = 0x7D,
		F15 = 0x7E,
		F16 = 0x7F,
		F17 = 0x80,
		F18 = 0x81,
		F19 = 0x82,
		F20 = 0x83,
		F21 = 0x84,
		F22 = 0x85,
		F23 = 0x86,
		F24 = 0x87,

		// 0x88 - 0x8F : unassigned

		Numlock = 0x90,
		Scroll = 0x91,

		/*
		// NEC PC-9800 kbd definitions
		OEM_NEC_EQUAL = 0x92,   // '=' key on numpad

		// Fujitsu/OASYS kbd definitions
		OEM_FJ_JISHO = 0x92,   // 'Dictionary' key
		OEM_FJ_MASSHOU = 0x93,   // 'Unregister word' key
		OEM_FJ_TOUROKU = 0x94,   // 'Register word' key
		OEM_FJ_LOYA = 0x95,   // 'Left OYAYUBI' key
		OEM_FJ_ROYA = 0x96,   // 'Right OYAYUBI' key
		*/

		// 0x97 - 0x9F : unassigned

		// L* & R* - left and right Alt, Ctrl and Shift virtual keys.
		// Used only as parameters to GetAsyncKeyState() and GetKeyState().
		// No other API or message will distinguish left and right keys in this way.
		LShift = 0xA0,
		RShift = 0xA1,
		LControl = 0xA2,
		RControl = 0xA3,
		LMenu = 0xA4,
		RMenu = 0xA5,

		Browser_Back = 0xA6,
		Browser_Forward = 0xA7,
		Browser_Refresh = 0xA8,
		Browser_Stop = 0xA9,
		Browser_Search = 0xAA,
		Browser_Favorites = 0xAB,
		Browser_Home = 0xAC,

		Volume_Mute = 0xAD,
		Volume_Down = 0xAE,
		Volume_Up = 0xAF,
		Media_Next_Track = 0xB0,
		Media_Prev_Track = 0xB1,
		Media_Stop = 0xB2,
		Media_Play_Pause = 0xB3,
		Launch_Mail = 0xB4,
		Launch_Media_Select = 0xB5,
		Launch_App1 = 0xB6,
		Launch_App2 = 0xB7,

		// 0xB8 - 0xB9 : reserved

		Oem_1 = 0xBA,   // ';:' for US
		Oem_Plus = 0xBB,   // '+' any country
		Oem_Comma = 0xBC,   // ',' any country
		Oem_Minus = 0xBD,   // '-' any country
		Oem_Period = 0xBE,   // '.' any country
		Oem_2 = 0xBF,   // '/?' for US
		Oem_3 = 0xC0,   // '`~' for US

		// 0xC1 - 0xD7 : reserved
		// 0xD8 - 0xDA : unassigned

		Oem_4 = 0xDB,  //  '[{' for US
		Oem_5 = 0xDC,  //  '\|' for US
		Oem_6 = 0xDD,  //  ']}' for US
		Oem_7 = 0xDE,  //  ''"' for US
		Oem_8 = 0xDF,

		// 0xE0 : reserved

		// Various extended or enhanced keyboards
		Oem_Ax = 0xE1,  //  'AX' key on Japanese AX kbd
		Oem_102 = 0xE2,  //  "<>" or "\|" on RT 102-key kbd.
		Ico_Help = 0xE3,  //  Help key on ICO
		Ico_00 = 0xE4,  //  00 key on ICO

		ProcessKey = 0xE5,

		Ico_Clear = 0xE6,

		Packet = 0xE7,

		// 0xE8 : unassigned

		/*
		// Nokia/Ericsson definitions
		OEM_RESET = 0xE9,
		OEM_JUMP = 0xEA,
		OEM_PA1 = 0xEB,
		OEM_PA2 = 0xEC,
		OEM_PA3 = 0xED,
		OEM_WSCTRL = 0xEE,
		OEM_CUSEL = 0xEF,
		OEM_ATTN = 0xF0,
		OEM_FINISH = 0xF1,
		OEM_COPY = 0xF2,
		OEM_AUTO = 0xF3,
		OEM_ENLW = 0xF4,
		OEM_BACKTAB = 0xF5,

		ATTN = 0xF6,
		CRSEL = 0xF7,
		EXSEL = 0xF8,
		EREOF = 0xF9,
		PLAY = 0xFA,
		ZOOM = 0xFB,
		NONAME = 0xFC,
		PA1 = 0xFD,
		OEM_CLEAR = 0xFE,
		*/
	}

	#endregion

	public static class OperatingSystemTools
	{
		#region GetAsyncKeyState

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

		[DllImport("user32.dll")]
		static extern short GetAsyncKeyState(int vKey);

		public static bool IsAsyncKeyStateDown(VirtualKeyCode key)
		{
			return (GetAsyncKeyState((int)key) & 0x8000) != 0;
		}

#else

		public static bool IsAsyncKeyStateDown(VirtualKeyCode key)
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
