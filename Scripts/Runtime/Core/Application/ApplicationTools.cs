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
					case RuntimePlatform.OSXEditor:
					case RuntimePlatform.WindowsPlayer:
					case RuntimePlatform.WindowsEditor:
						return Application.dataPath.RemoveLastDirectoryFromPath().AddDirectorySeparatorToEnd().FixDirectorySeparatorChars();

					default:
						throw new NotImplementedException();
				}
			}
		}

		#endregion

		#region Path Hash

		private static string _PathHash;
		public static string PathHash
		{
			get
			{
				if (string.IsNullOrEmpty(_PathHash))
				{
					// CAUTION! DO NOT CHANGE HASH ALGORITHM OR PATH!
					// We need consistency, rather than better hash algorithms.
					// That's why GetHashCodeGuaranteed is used because it will
					// stay here, guaranteed to be never modified forever.
					var path = Application.dataPath;
					var hash = path.GetHashCodeGuaranteed();
					_PathHash = hash.ToHexString(false);
				}
				return _PathHash;
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
