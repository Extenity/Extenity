using System;
using System.IO;
using System.Runtime.InteropServices;
using Extenity.DataToolbox;
using Extenity.FileSystemToolbox;
using UnityEngine;
using UnityEngine.Rendering;

namespace Extenity.ApplicationToolbox
{

	public static class ApplicationTools
	{
		#region Deinitialization

		public static bool IsShuttingDown = false;

		#endregion

		#region Paths

		public static string ApplicationPath
		{
			get
			{
				switch (Application.platform)
				{
					case RuntimePlatform.WindowsEditor:
						// This does not work in threaded environment. So we use working directory instead.
						// return Application.dataPath.AddDirectorySeparatorToEnd().RemoveLastDirectoryFromPath().AddDirectorySeparatorToEnd().FixDirectorySeparatorChars();
						return Directory.GetCurrentDirectory().AddDirectorySeparatorToEnd().FixDirectorySeparatorChars();

					case RuntimePlatform.WindowsPlayer:
						throw new NotImplementedException(); // TODO:

					default:
						throw new NotImplementedException();
				}
			}
		}

		public static string PersistentDataPath
		{
			get
			{
#if UNITY_ANDROID && !UNITY_EDITOR
				using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
				using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
				using (var applicationContext = currentActivity.Call<AndroidJavaObject>("getApplicationContext"))
				{
					var filesDir = applicationContext.Call<AndroidJavaObject>("getFilesDir");
					return filesDir.Call<string>("getCanonicalPath");
				}
#else
				return Application.persistentDataPath;
#endif
			}
		}

		public static string TemporaryCachePath
		{
			get
			{
#if UNITY_ANDROID && !UNITY_EDITOR
				using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
				using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
				using (var applicationContext = currentActivity.Call<AndroidJavaObject>("getApplicationContext"))
				{
					var cacheDir = applicationContext.Call<AndroidJavaObject>("getCacheDir");
					return cacheDir.Call<string>("getCanonicalPath");
				}
#else
				return Application.temporaryCachePath;
#endif
			}
		}

		#endregion

		#region Paths - Unity Project

		public static class UnityProjectPaths
		{
			public static readonly string AssetsDirectory = "Assets";
			public static readonly string ProjectSettingsDirectory = "ProjectSettings";
			public static readonly string UserSettingsDirectory = "UserSettings";
			public static readonly string LibraryDirectory = "Library";
			public static readonly string PackagesDirectory = "Packages";
			public static readonly string TempDirectory = "Temp";

			public static string AssetsRelativePath => AssetsDirectory.AddDirectorySeparatorToEnd();
			public static string ProjectSettingsRelativePath => ProjectSettingsDirectory.AddDirectorySeparatorToEnd();
			public static string UserSettingsRelativePath => UserSettingsDirectory.AddDirectorySeparatorToEnd();
			public static string LibraryRelativePath => LibraryDirectory.AddDirectorySeparatorToEnd();
			public static string PackagesRelativePath => PackagesDirectory.AddDirectorySeparatorToEnd();
			public static string TempRelativePath => TempDirectory.AddDirectorySeparatorToEnd();

			public static string AssetsFullPath => ApplicationPath.AppendDirectoryToPath(AssetsDirectory, true);
			public static string ProjectSettingsFullPath => ApplicationPath.AppendDirectoryToPath(ProjectSettingsDirectory, true);
			public static string UserSettingsFullPath => ApplicationPath.AppendDirectoryToPath(UserSettingsDirectory, true);
			public static string LibraryFullPath => ApplicationPath.AppendDirectoryToPath(LibraryDirectory, true);
			public static string PackagesFullPath => ApplicationPath.AppendDirectoryToPath(PackagesDirectory, true);
			public static string TempFullPath => ApplicationPath.AppendDirectoryToPath(TempDirectory, true);
		}

		/// <summary>
		/// Checks if the specified directory is a Unity project directory. It will throw in case Path.Combine or
		/// Directory.Exists methods are not happy with the given path.
		/// </summary>
		public static bool IsUnityProjectPath(string directoryPath)
		{
			return
				Directory.Exists(directoryPath) &&
				Directory.Exists(Path.Combine(directoryPath, UnityProjectPaths.AssetsDirectory)) &&
				Directory.Exists(Path.Combine(directoryPath, UnityProjectPaths.ProjectSettingsDirectory));
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

		#region Launch Mobile Market Page

		public static void LaunchMarketPageAndQuit()
		{
			LaunchMarketPage();
			Quit();
		}

		/// <summary>
		/// Source: https://forum.unity.com/threads/link-to-app-on-appstore-from-in-an-other-app.31070/
		/// </summary>
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

		public static string AppStoreMarketID;

		private static void LaunchMarketPage_AppStore()
		{
			var address = "itms-apps://itunes.apple.com/app/id" + AppStoreMarketID;
			Log.Info("Launching market page: " + address);
			Application.OpenURL(address);
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

		#region Restart / Quit

		public static void Quit()
		{
#if UNITY_EDITOR
			if (UnityEditor.EditorApplication.isPlaying)
			{
				UnityEditor.EditorApplication.isPlaying = false;
			}
#else
			Application.Quit();
#endif
		}

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

#elif UNITY_ANDROID || UNITY_IOS
			// Android and iOS does not provide a way to restart the application. Seems like
			// they are not happy about the idea of applications launching themselves or other
			// applications.
			// 
			// The best way to restart the application is not restarting but destroying all
			// objects and reloading the splash menu item. Though static objects and singletons
			// should be handled carefully.
			Application.Quit();

#else
			throw new NotImplementedException();
#endif
		}

		#endregion
	}

}
