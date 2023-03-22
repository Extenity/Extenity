using System;

namespace Extenity.ApplicationToolbox
{

	public struct ApplicationVersion
	{
		public readonly int Major;
		public readonly int Minor;
#if !BuildlessVersioning
		public readonly int Build;
#endif

		#region Configuration

		// Digits are adjusted so that the resulting max value is 4999999 which is below float's integer part of
		// 2^24=16777216. So the version can also be represented in a float value.
		public const int MinorDigits = 100;
		//#if !BuildlessVersioning / Nope! Build digits are still preserved even when using BuildlessVersioning to support compatibility.
		public const int BuildDigits = 1000;
		public const int MinorAndBuildDigits = BuildDigits * MinorDigits;
		//#endif

		// Max major version is here for no good reason. May safely be changed in future. But think wisely about how that would change other systems that uses the version.
		public const int MaxMajorVersion = 49;
		public const int MinMajorVersion = 1;
		public const int MaxMinorVersion = MinorDigits - 1;
		public const int MinMinorVersion = 0;
#if !BuildlessVersioning
		public const int MaxBuildVersion = BuildDigits - 1;
		public const int MinBuildVersion = 0;
#endif

		#endregion

		#region Initialization and Conversions

		public int Combined =>
#if !BuildlessVersioning
			Major * MinorAndBuildDigits +
			Minor * BuildDigits +
			Build;
#else
			Major * MinorAndBuildDigits +
			Minor * BuildDigits;
#endif

#if !BuildlessVersioning
		public ApplicationVersion(int major, int minor, int build)
		{
			if (IsOutOfRange(major, minor, build))
				throw new ArgumentOutOfRangeException();

			Major = major;
			Minor = minor;
			Build = build;
		}
#endif

		public ApplicationVersion(int major, int minor)
		{
			if (IsOutOfRange(major, minor))
				throw new ArgumentOutOfRangeException();

			Major = major;
			Minor = minor;
#if !BuildlessVersioning
			Build = 0;
#endif
		}

		public ApplicationVersion(int combinedVersion)
		{
			if (combinedVersion <= 0)
				throw new ArgumentOutOfRangeException();

			Major = combinedVersion / MinorAndBuildDigits;
			combinedVersion -= Major * MinorAndBuildDigits;
			Minor = combinedVersion / BuildDigits;
#if !BuildlessVersioning
			combinedVersion -= Minor * BuildDigits;
			Build = combinedVersion;
#endif

#if !BuildlessVersioning
			if (IsOutOfRange(Major, Minor, Build))
				throw new ArgumentOutOfRangeException(ToString());
#else
			if (IsOutOfRange(Major, Minor))
				throw new ArgumentOutOfRangeException(ToString());
#endif
		}

		public ApplicationVersion(string versionText)
		{
			try
			{
				var split = versionText.Split('.');

#if !BuildlessVersioning
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
#else
				// Do not allow formats other than *.*
				if (split.Length != 2 ||
				    string.IsNullOrWhiteSpace(split[0]) ||
				    string.IsNullOrWhiteSpace(split[1])
				)
					throw new Exception();

				Major = int.Parse(split[0]);
				Minor = int.Parse(split[1]);

				if (IsOutOfRange(Major, Minor))
					throw new ArgumentOutOfRangeException();
#endif
			}
			catch (Exception exception)
			{
				throw new Exception($"Failed to parse version '{versionText}'.", exception);
			}
		}

#if !BuildlessVersioning
		public void Split(out int major, out int minor, out int build)
		{
			major = Major;
			minor = Minor;
			build = Build;
		}
#endif

		public void Split(out int major, out int minor)
		{
			major = Major;
			minor = Minor;
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

		public ApplicationVersion IncrementedMajor => AddVersion(1, 0);
		public ApplicationVersion DecrementedMajor => AddVersion(-1, 0);
		public ApplicationVersion IncrementedMinor => AddVersion(0, 1);
		public ApplicationVersion DecrementedMinor => AddVersion(0, -1);
#if !BuildlessVersioning
		public ApplicationVersion IncrementedBuild => AddVersion(0, 0, 1);
		public ApplicationVersion DecrementedBuild => AddVersion(0, 0, -1);
#endif
#if !BuildlessVersioning
		public ApplicationVersion IncrementedLower => IncrementedBuild;
		public ApplicationVersion DecrementedLower => DecrementedBuild;
#else
		public ApplicationVersion IncrementedLower => IncrementedMinor;
		public ApplicationVersion DecrementedLower => DecrementedMinor;
#endif

#if !BuildlessVersioning
		/// <summary>
		/// Increments or decrements Major, Minor or Build versions and checks for out of range errors.
		/// Specifying int.MinValue for a version resets that version to 1. Useful when increasing
		/// Minor version which also requires Build version to be set to 1.
		/// </summary>
		public ApplicationVersion AddVersion(int addMajor, int addMinor, int addBuild)
		{
			Split(out var major, out var minor, out var build);

			major = addMajor == int.MinValue ? 1 : major + addMajor;
			minor = addMinor == int.MinValue ? 1 : minor + addMinor;
			build = addBuild == int.MinValue ? 1 : build + addBuild;

			if (IsOutOfRange(major, minor, build))
			{
				throw new Exception($"Version change makes the version go out of range. Current version is: {ToString()}. New version is: {ToString(major, minor, build)}");
			}

			return new ApplicationVersion(major, minor, build);
		}
#endif

		/// <summary>
		/// Increments or decrements Major, Minor versions and checks for out of range errors.
		/// Specifying int.MinValue for a version resets that version to 1. Useful when increasing
		/// Minor version which also requires Build version to be set to 1.
		///
		/// Note that this overload won't change Build version.
		/// </summary>
		public ApplicationVersion AddVersion(int addMajor, int addMinor)
		{
			Split(out var major, out var minor);

			major = addMajor == int.MinValue ? 1 : major + addMajor;
			minor = addMinor == int.MinValue ? 1 : minor + addMinor;

			if (IsOutOfRange(major, minor))
			{
#if !BuildlessVersioning
				var newVersion = ToString(major, minor, Build);
#else
				var newVersion = ToString(major, minor);
#endif
				throw new Exception($"Version change makes the version go out of range. Current version is: {ToString()}. New version is: {newVersion}");
			}

#if !BuildlessVersioning
			return new ApplicationVersion(major, minor, Build);
#else
			return new ApplicationVersion(major, minor);
#endif
		}

		#endregion

		#region Get From Unity and Project Configuration

#if UNITY

		public static ApplicationVersion GetUnityApplicationVersion()
		{
			return new ApplicationVersion(UnityEngine.Application.version);
		}

#endif

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

			OnProjectVersionChanged?.Invoke();
		}

#if !BuildlessVersioning
		public static void AddToUnityVersionConfiguration(int addMajor, int addMinor, int addBuild, bool saveAssets)
#else
		public static void AddToUnityVersionConfiguration(int addMajor, int addMinor, bool saveAssets)
#endif
		{
			CheckVersionConfigurationConsistency();

			var version = GetUnityApplicationVersion();

#if !BuildlessVersioning
			if (addMajor != 0 || addMinor != 0 || addBuild != 0)
			{
				version = version.AddVersion(addMajor, addMinor, addBuild);
				Log.Info($"New version: {version}  (increment by {ToIncrementString(addMajor)}.{ToIncrementString(addMinor)}.{ToIncrementString(addBuild)})");
			}
#else
			if (addMajor != 0 || addMinor != 0)
			{
				version = version.AddVersion(addMajor, addMinor);
				Log.Info($"New version: {version}  (increment by {ToIncrementString(addMajor)}.{ToIncrementString(addMinor)})");
			}
#endif
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

				try { AndroidVersion = GetAndroidVersion(); }
				catch { AndroidVersion = new ApplicationVersion(1, 0); }
				try { iOSVersion = GetIOSVersion(); }
				catch { iOSVersion = new ApplicationVersion(1, 0); }
				try { UnityVersion = GetUnityApplicationVersion(); }
				catch { UnityVersion = new ApplicationVersion(1, 0); }

				var maxVersion = AndroidVersion > iOSVersion ? AndroidVersion : iOSVersion;
				maxVersion = UnityVersion > maxVersion ? UnityVersion : maxVersion;

				Log.Warning($"Fixing platform versions to the detected maximum version '{maxVersion}'.");
				SetAllPlatformVersions(maxVersion, true);
			}
		}

#endif

		#endregion

		#region Version Change Emitter

#if UNITY_EDITOR

#pragma warning disable 67
		public static event Action OnProjectVersionChanged;
#pragma warning restore 67

#endif

		#endregion

		#region Consistency

#if !BuildlessVersioning
		private static bool IsOutOfRange(int major, int minor, int build)
		{
			return
				major < MinMajorVersion || major > MaxMajorVersion ||
				minor < MinMinorVersion || minor > MaxMinorVersion ||
				build < MinBuildVersion || build > MaxBuildVersion;
		}
#endif

		private static bool IsOutOfRange(int major, int minor)
		{
			return
				major < MinMajorVersion || major > MaxMajorVersion ||
				minor < MinMinorVersion || minor > MaxMinorVersion;
		}

		#endregion

		#region ToString

#if !BuildlessVersioning
		public static string ToString(int major, int minor, int build)
		{
			return major + "." + minor + "." + build;
		}
#endif

		public static string ToString(int major, int minor)
		{
			return major + "." + minor;
		}

#if !BuildlessVersioning
		public override string ToString()
		{
			return Major + "." + Minor + "." + Build;
		}
#else
		public override string ToString()
		{
			return Major + "." + Minor;
		}
#endif

		public string ToMajorMinorString()
		{
			return Major + "." + Minor;
		}

		private static string ToIncrementString(int increment)
		{
			return increment == int.MinValue
				? "reset"
				: increment.ToString();
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(ApplicationVersion));

		#endregion
	}

}
