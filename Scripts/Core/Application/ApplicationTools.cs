using System;
using System.Runtime.InteropServices;
using Extenity.DataToolbox;
using UnityEngine;
using UnityEngine.Rendering;

namespace Extenity.ApplicationToolbox
{

	public static class ApplicationTools
	{
		#region Paths

		public static string ApplicationPath
		{
			get
			{
				switch (Application.platform)
				{
					//case RuntimePlatform.OSXEditor: throw new NotImplementedException();
					case RuntimePlatform.OSXPlayer:
						throw new NotImplementedException();
					case RuntimePlatform.OSXDashboardPlayer:
						throw new NotImplementedException();
					case RuntimePlatform.IPhonePlayer:
						throw new NotImplementedException();
					case RuntimePlatform.Android:
						throw new NotImplementedException();
					case RuntimePlatform.LinuxPlayer:
						throw new NotImplementedException();

					case RuntimePlatform.OSXEditor:
					case RuntimePlatform.WindowsPlayer:
					case RuntimePlatform.WindowsEditor:
						return Application.dataPath.RemoveLastDirectoryFromPath().AddDirectorySeparatorToEnd().FixDirectorySeparatorChars();

					default:
						throw new ArgumentOutOfRangeException("platform");
				}
			}
		}

		#endregion

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

		#region Headless

		public static bool IsHeadless()
		{
			return SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;
		}

		#endregion
	}

}
