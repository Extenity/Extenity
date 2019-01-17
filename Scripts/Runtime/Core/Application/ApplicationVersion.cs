using System;
using UnityEngine;

namespace Extenity.ApplicationToolbox
{

	public struct ApplicationVersion
	{
		public const int MajorDigits = 10000;
		public const int MinorDigits = 100;

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

		public ApplicationVersion IncrementedMajor => new ApplicationVersion(Major + 1, Minor, Build);
		public ApplicationVersion DecrementedMajor => new ApplicationVersion(Major - 1, Minor, Build);
		public ApplicationVersion IncrementedMinor => new ApplicationVersion(Major, Minor + 1, Build);
		public ApplicationVersion DecrementedMinor => new ApplicationVersion(Major, Minor - 1, Build);
		public ApplicationVersion IncrementedBuild => new ApplicationVersion(Major, Minor, Build + 1);
		public ApplicationVersion DecrementedBuild => new ApplicationVersion(Major, Minor, Build - 1);

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
			int major;
			int minor;
			int build;

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

				major = int.Parse(split[0]);
				minor = int.Parse(split[1]);
				build = int.Parse(split[2]);
			}
			catch (Exception exception)
			{
				throw new Exception($"Failed to parse version '{versionText}'.", exception);
			}

			return new ApplicationVersion(major, minor, build);
		}

		public static ApplicationVersion GetFromUnity()
		{
			return Parse(Application.version);
		}
	}

}
