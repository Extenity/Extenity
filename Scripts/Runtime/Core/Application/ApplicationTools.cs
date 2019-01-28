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
					case RuntimePlatform.WindowsEditor:
						return Application.dataPath.AddDirectorySeparatorToEnd().RemoveLastDirectoryFromPath().AddDirectorySeparatorToEnd().FixDirectorySeparatorChars();
					case RuntimePlatform.WindowsPlayer:
						throw new NotImplementedException(); // TODO:

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

		#region Launch App Store Page

		public static void LaunchMarketPage()
		{
#if UNITY_EDITOR
			Log.Info("Ambiguous to launch the market page in Editor.");
			//LaunchMarketPage_GooglePlay();
			//LaunchMarketPage_AppStore();
#elif UNITY_ANDROID
			LaunchMarketPage_GooglePlay();
#elif UNITY_IOS
			LaunchMarketPage_AppStore();
#else
			throw new System.NotImplementedException();
#endif
		}

#if UNITY_EDITOR || UNITY_IOS

		private static void LaunchMarketPage_AppStore()
		{
			throw new NotImplementedException();
		}

#endif

#if UNITY_EDITOR || UNITY_ANDROID

		private static void LaunchMarketPage_GooglePlay()
		{
			var address = "market://details?id=" + Application.identifier;
			Log.Info("Launching market page: " + address);
			Application.OpenURL(address);
		}

#endif

		#endregion

		#region Restart

		public static void Restart()
		{
#if UNITY_EDITOR
			if (UnityEditor.EditorApplication.isPlaying)
			{
				// Quit playing.
				UnityEditor.EditorApplication.ExecuteMenuItem("Edit/Play");
				// Start playing again.
				UnityEditor.EditorApplication.delayCall += () =>
				{
					UnityEditor.EditorApplication.ExecuteMenuItem("Edit/Play");
				};
			}
#else
			throw new NotImplementedException();
#endif
		}

		#endregion
	}

}
