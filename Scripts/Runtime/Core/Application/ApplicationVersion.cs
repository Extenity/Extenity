using System;
using UnityEngine;

namespace Extenity.ApplicationToolbox
{

	public struct ApplicationVersion
	{
		private const int MajorDigits = 10000;
		private const int MinorDigits = 100;

		public readonly int Major;
		public readonly int Minor;
		public readonly int Build;

		public int Combined =>
			Major * MajorDigits +
			Minor * MinorDigits +
			Build;

		public ApplicationVersion(int major, int minor, int build)
		{
			if (major < 1 || major > 49 ||
				minor < 0 || minor > 99 ||
				build < 0 || build > 99)
			{
				throw new ArgumentOutOfRangeException();
			}

			Major = major;
			Minor = minor;
			Build = build;
		}

		public static ApplicationVersion FromCombined(int combined)
		{
			if (combined <= 0)
				throw new ArgumentOutOfRangeException();

			var major = combined / MajorDigits;
			combined -= major * MajorDigits;
			var minor = combined / MinorDigits;
			combined -= minor * MinorDigits;
			return new ApplicationVersion(major, minor, combined);
		}

		public static ApplicationVersion Parse(string versionText)
		{
			try
			{
				var split = versionText.Split('.');
				return new ApplicationVersion(
					int.Parse(split[0]),
					int.Parse(split[1]),
					int.Parse(split[2]));
			}
			catch
			{
				throw new Exception($"Failed to parse version '{versionText}'.");
			}
		}

		public static ApplicationVersion GetFromUnity()
		{
			return Parse(Application.version);
		}
	}

}
