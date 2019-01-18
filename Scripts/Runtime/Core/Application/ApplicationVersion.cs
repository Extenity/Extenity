using System;
using UnityEngine;

namespace Extenity.ApplicationToolbox
{

	public struct ApplicationVersion
	{
		public readonly int Major;
		public readonly int Minor;
		public readonly int Build;

		#region Configuration

		public const int MajorDigits = 10000;
		public const int MinorDigits = 100;

		#endregion

		#region Initialization and Conversions

		public int Combined =>
			Major * MajorDigits +
			Minor * MinorDigits +
			Build;

		public ApplicationVersion(int major, int minor, int build)
		{
			if (IsOutOfRange(major, minor, build))
				throw new ArgumentOutOfRangeException();

			Major = major;
			Minor = minor;
			Build = build;
		}

		public ApplicationVersion(int combinedVersion)
		{
			if (combinedVersion <= 0)
				throw new ArgumentOutOfRangeException();

			Major = combinedVersion / MajorDigits;
			combinedVersion -= Major * MajorDigits;
			Minor = combinedVersion / MinorDigits;
			combinedVersion -= Minor * MinorDigits;
			Build = combinedVersion;

			if (IsOutOfRange(Major, Minor, Build))
				throw new ArgumentOutOfRangeException();
		}

		public ApplicationVersion(string versionText)
		{
			try
			{
				var split = versionText.Split('.');

				// Do not allow formats other than *.*.*
				if (split.Length != 3 ||
					string.IsNullOrWhiteSpace(split[0]) ||
					string.IsNullOrWhiteSpace(split[1]) ||
					string.IsNullOrWhiteSpace(split[2])
				)
					throw new Exception();

				Major = int.Parse(split[0]);
				Minor = int.Parse(split[1]);
				Build = int.Parse(split[2]);

				if (IsOutOfRange(Major, Minor, Build))
					throw new ArgumentOutOfRangeException();
			}
			catch (Exception exception)
			{
				throw new Exception($"Failed to parse version '{versionText}'.", exception);
			}
		}

		public void Split(out int major, out int minor, out int build)
		{
			major = Major;
			minor = Minor;
			build = Build;
		}

		#endregion

		#region Change Version

		public ApplicationVersion IncrementedMajor => AddVersion(1, 0, 0);
		public ApplicationVersion DecrementedMajor => AddVersion(-1, 0, 0);
		public ApplicationVersion IncrementedMinor => AddVersion(0, 1, 0);
		public ApplicationVersion DecrementedMinor => AddVersion(0, -1, 0);
		public ApplicationVersion IncrementedBuild => AddVersion(0, 0, 1);
		public ApplicationVersion DecrementedBuild => AddVersion(0, 0, -1);

		public ApplicationVersion AddVersion(int addMajor, int addMinor, int addBuild)
		{
			Split(out var major, out var minor, out var build);

			major += addMajor;
			minor += addMinor;
			build += addBuild;

			if (IsOutOfRange(major,minor,build))
			{
				throw new Exception($"Version change makes the version go out of range. Current version is: {ToString()}. New version is: {ToString(major, minor, build)}");
			}

			return new ApplicationVersion(major, minor, build);
		}

		#endregion

		#region Get From Unity and Project Configuration

		public static ApplicationVersion GetUnityVersion()
		{
			return new ApplicationVersion(Application.version);
		}

#if UNITY_EDITOR

		public static ApplicationVersion GetAndroidVersion()
		{
			return new ApplicationVersion(UnityEditor.PlayerSettings.Android.bundleVersionCode);
		}

		public static ApplicationVersion GetIOSVersion()
		{
			return new ApplicationVersion(UnityEditor.PlayerSettings.iOS.buildNumber);
		}

		/// <summary>
		/// Makes sure all platform configurations have the same version set.
		/// </summary>
		public static void CheckVersionConfigurationConsistency()
		{
			if (!Equals(GetAndroidVersion(), GetIOSVersion()))
			{
				throw new Exception($"Android version '{GetAndroidVersion()}' and iOS version '{GetIOSVersion()}' does not match. This must be manually resolved. Correct it from project configuration then try again.");
			}
			if (!Equals(GetUnityVersion(), GetAndroidVersion()))
			{
				throw new Exception($"Android version '{GetAndroidVersion()}' and Bundle version '{GetUnityVersion()}' does not match. This must be manually resolved. Correct it from project configuration then try again.");
			}
		}

#endif

		#endregion

		#region Consistency

		private static bool IsOutOfRange(int major, int minor, int build)
		{
			return
				major < 1 || major >= 50 ||
				minor < 0 || minor >= (MajorDigits / MinorDigits) ||
				build < 0 || build >= MinorDigits;
		}

		#endregion

		#region ToString

		public static string ToString(int major, int minor, int build)
		{
			return major + "." + minor + "." + build;
		}

		public override string ToString()
		{
			return Major + "." + Minor + "." + Build;
		}

		#endregion
	}

}
