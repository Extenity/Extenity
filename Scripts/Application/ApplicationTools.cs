using System;
using System.Runtime.InteropServices;
using UnityEngine;

public static class ApplicationTools
{
	public static string ApplicationPath
	{
		get
		{
			switch (UnityEngine.Application.platform)
			{
				case RuntimePlatform.OSXEditor: throw new NotImplementedException();
				case RuntimePlatform.OSXPlayer: throw new NotImplementedException();
				case RuntimePlatform.OSXDashboardPlayer: throw new NotImplementedException();
				case RuntimePlatform.IPhonePlayer: throw new NotImplementedException();
				case RuntimePlatform.XBOX360: throw new NotImplementedException();
				case RuntimePlatform.PS3: throw new NotImplementedException();
				case RuntimePlatform.Android: throw new NotImplementedException();
				case RuntimePlatform.LinuxPlayer: throw new NotImplementedException();
				
				case RuntimePlatform.WindowsPlayer:
				case RuntimePlatform.WindowsEditor:
					return UnityEngine.Application.dataPath.RemoveLastDirectoryFromPath();

				case RuntimePlatform.OSXWebPlayer:
				case RuntimePlatform.WindowsWebPlayer:
					throw new Exception("Application path is not meaningful in '" + UnityEngine.Application.platform.ToString() + "' platform");
				
				default:
					throw new ArgumentOutOfRangeException("platform");
			}
		}
	}

	#region Company And Product Name

	private static string _AsciiCompanyName;
	public static string AsciiCompanyName
	{
		get
		{
			if (_AsciiCompanyName == null)
				_AsciiCompanyName = Application.companyName.ConvertToAscii();
			return _AsciiCompanyName;
		}
	}

	private static string _AsciiProductName;
	public static string AsciiProductName
	{
		get
		{
			if (_AsciiProductName == null)
				_AsciiProductName = Application.productName.ConvertToAscii();
			return _AsciiProductName;
		}
	}

	#endregion

	#region Architecture

	public static bool Is32Bit()
	{
		return IntPtrSize == 4;
	}

	public static bool Is64Bit()
	{
		return IntPtrSize == 8;
	}

	public static int IntPtrSize
	{
		get { return Marshal.SizeOf(typeof(IntPtr)); }
	}

	#endregion
}
