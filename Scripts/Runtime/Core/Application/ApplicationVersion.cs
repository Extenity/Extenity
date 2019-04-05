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

		public const int MinorDigits = 1000;
		public const int BuildDigits = 10000;
		public const int MinorAndBuildDigits = BuildDigits * MinorDigits;

		// Max major version is here for no good reason. May safely be changed in future. But think wisely about how that would change other systems that uses the version.
		public const int MaxMajorVersion = 49;
		public const int MinMajorVersion = 1;
		public const int MaxMinorVersion = MinorDigits - 1;
		public const int MinMinorVersion = 0;
		public const int MaxBuildVersion = BuildDigits - 1;
		public const int MinBuildVersion = 0;

		#endregion

		#region Initialization and Conversions

		public int Combined =>
			Major * MinorAndBuildDigits +
			Minor * BuildDigits +
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

			Major = combinedVersion / MinorAndBuildDigits;
			combinedVersion -= Major * MinorAndBuildDigits;
			Minor = combinedVersion / BuildDigits;
			combinedVersion -= Minor * BuildDigits;
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

		#region Comparison

		public static bool operator >(ApplicationVersion lhs, ApplicationVersion rhs)
		{
			return lhs.Combined > rhs.Combined;
		}

		public static bool operator <(ApplicationVersion lhs, ApplicationVersion rhs)
		{
			return lhs.Combined < rhs.Combined;
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

			if (IsOutOfRange(major, minor, build))
			{
				throw new Exception($"Version change makes the version go out of range. Current version is: {ToString()}. New version is: {ToString(major, minor, build)}");
			}

			return new ApplicationVersion(major, minor, build);
		}

		#endregion

		#region Get From Unity and Project Configuration

		public static ApplicationVersion GetUnityApplicationVersion()
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

		public static void SetAllPlatformVersions(ApplicationVersion version, bool saveAssets)
		{
			UnityEditor.PlayerSettings.bundleVersion = version.ToString();
			UnityEditor.PlayerSettings.Android.bundleVersionCode = version.Combined;
			UnityEditor.PlayerSettings.iOS.buildNumber = version.ToString();

			if (saveAssets)
			{
				UnityEditor.AssetDatabase.SaveAssets();
			}
		}

		public static void AddToUnityVersionConfiguration(int addMajor, int addMinor, int addBuild, bool saveAssets)
		{
			CheckVersionConfigurationConsistency();

			var version = GetUnityApplicationVersion();

			if (addMajor != 0 || addMinor != 0 || addBuild != 0)
			{
				version = version.AddVersion(addMajor, addMinor, addBuild);
				Log.Info($"New version: {version}  (increment by {addMajor}.{addMinor}.{addBuild})");
			}
			else
			{
				// Even though we don't need to change the version, we still
				//   - check for consistency,
				//   - get the Unity version,
				//   - apply it to all platforms,
				//   - then save the configuration if asked.
				Log.Info($"Keeping current version: {version}");
			}

			// Set versions for all platforms
			SetAllPlatformVersions(version, saveAssets);
		}

		/// <summary>
		/// Makes sure all platform configurations have the same version set.
		/// </summary>
		public static void CheckVersionConfigurationConsistency()
		{
			ApplicationVersion AndroidVersion;
			ApplicationVersion iOSVersion;
			ApplicationVersion ApplicationVersion;

			try
			{
				AndroidVersion = GetAndroidVersion();
			}
			catch (Exception exception)
			{
				throw new Exception("Failed to get version configuration.", exception);
			}
			try
			{
				iOSVersion = GetIOSVersion();
			}
			catch (Exception exception)
			{
				throw new Exception("Failed to get version configuration.", exception);
			}
			try
			{
				ApplicationVersion = GetUnityApplicationVersion();
			}
			catch (Exception exception)
			{
				throw new Exception("Failed to get version configuration.", exception);
			}

			if (!Equals(AndroidVersion, iOSVersion))
			{
				throw new Exception($"Android version '{AndroidVersion}' and iOS version '{iOSVersion}' does not match. This must be manually resolved. Correct it from project configuration then try again.");
			}
			if (!Equals(AndroidVersion, ApplicationVersion))
			{
				throw new Exception($"Android version '{AndroidVersion}' and Bundle version '{ApplicationVersion}' does not match. This must be manually resolved. Correct it from project configuration then try again.");
			}
		}

		public static void FixVersionConfigurationByChoosingTheHighestVersion()
		{
			try
			{
				CheckVersionConfigurationConsistency();
			}
			catch
			{
				ApplicationVersion AndroidVersion;
				ApplicationVersion iOSVersion;
				ApplicationVersion UnityVersion;

				try
				{
					AndroidVersion = GetAndroidVersion();
				}
				catch
				{
					AndroidVersion = new ApplicationVersion(1, 0, 0);
				}
				try
				{
					iOSVersion = GetIOSVersion();
				}
				catch
				{
					iOSVersion = new ApplicationVersion(1, 0, 0);
				}
				try
				{
					UnityVersion = GetUnityApplicationVersion();
				}
				catch
				{
					UnityVersion = new ApplicationVersion(1, 0, 0);
				}

				var maxVersion = AndroidVersion > iOSVersion ? AndroidVersion : iOSVersion;
				maxVersion = UnityVersion > maxVersion ? UnityVersion : maxVersion;

				Log.Warning($"Fixing platform versions to the detected maximum version '{maxVersion}'.");
				SetAllPlatformVersions(maxVersion, true);
			}
		}

#endif

		#endregion

		#region Consistency

		private static bool IsOutOfRange(int major, int minor, int build)
		{
			return
				major < MinMajorVersion || major > MaxMajorVersion ||
				minor < MinMinorVersion || minor > MaxMinorVersion ||
				build < MinBuildVersion || build > MaxBuildVersion;
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

		public string ToMajorMinorString()
		{
			return Major + "." + Minor;
		}

		#endregion
	}

}
