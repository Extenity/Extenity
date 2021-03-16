using System;
using Extenity.ApplicationToolbox;
using Extenity.Testing;
using NUnit.Framework;

namespace ExtenityTests.ApplicationToolbox
{

	public class Test_ApplicationVersion : ExtenityTestBase
	{
		[Test]
		public static void ConstantsAreCorrect()
		{
			Assert.AreEqual(1, ApplicationVersion.MinMajorVersion);
			Assert.AreEqual(0, ApplicationVersion.MinMinorVersion);
#if !BuildlessVersioning
			Assert.AreEqual(0, ApplicationVersion.MinBuildVersion);
#endif

			Assert.AreEqual(49, ApplicationVersion.MaxMajorVersion);
			Assert.AreEqual(999, ApplicationVersion.MaxMinorVersion);
#if !BuildlessVersioning
			Assert.AreEqual(9999, ApplicationVersion.MaxBuildVersion);
#endif

			Assert.AreEqual(1000, ApplicationVersion.MinorDigits);
			Assert.AreEqual(10000, ApplicationVersion.BuildDigits);
		}

		[Test]
		public static void VersionRangeLimits()
		{
			// Major version
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(ApplicationVersion.MinMajorVersion - 1, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(ApplicationVersion.MaxMajorVersion + 1, 0));
			Assert.DoesNotThrow(() => new ApplicationVersion(ApplicationVersion.MinMajorVersion, 0));
			Assert.DoesNotThrow(() => new ApplicationVersion(ApplicationVersion.MaxMajorVersion, 0));

			// Minor version
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(1, ApplicationVersion.MinMinorVersion - 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(1, ApplicationVersion.MaxMinorVersion + 1));
			Assert.DoesNotThrow(() => new ApplicationVersion(1, ApplicationVersion.MinMinorVersion));
			Assert.DoesNotThrow(() => new ApplicationVersion(1, ApplicationVersion.MinMinorVersion));

			// Build version
#if !BuildlessVersioning
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(1, 0, ApplicationVersion.MinBuildVersion - 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(1, 0, ApplicationVersion.MaxBuildVersion + 1));
			Assert.DoesNotThrow(() => new ApplicationVersion(1, 0, ApplicationVersion.MinBuildVersion));
			Assert.DoesNotThrow(() => new ApplicationVersion(1, 0, ApplicationVersion.MinBuildVersion));
#endif
		}

		[Test]
		public static void Combined()
		{
#if !BuildlessVersioning
			Assert.AreEqual(10000000, new ApplicationVersion(1, 0, 0).Combined);
			Assert.AreEqual(10020003, new ApplicationVersion(1, 2, 3).Combined);
			Assert.AreEqual(10210043, new ApplicationVersion(1, 21, 43).Combined);
			Assert.AreEqual(499999999, new ApplicationVersion(49, 999, 9999).Combined);
#else
			Assert.AreEqual(10000000, new ApplicationVersion(1, 0).Combined);
			Assert.AreEqual(10020000, new ApplicationVersion(1, 2).Combined);
			Assert.AreEqual(10210000, new ApplicationVersion(1, 21).Combined);
			Assert.AreEqual(499990000, new ApplicationVersion(49, 999).Combined);
#endif
		}

		[Test]
		public static void IncrementsAndDecrements()
		{
			Assert.AreEqual(new ApplicationVersion(2, 0), new ApplicationVersion(1, 0).IncrementedMajor);
			Assert.AreEqual(new ApplicationVersion(1, 0), new ApplicationVersion(2, 0).DecrementedMajor);
			Assert.AreEqual(new ApplicationVersion(1, 1), new ApplicationVersion(1, 0).IncrementedMinor);
			Assert.AreEqual(new ApplicationVersion(1, 0), new ApplicationVersion(1, 1).DecrementedMinor);
#if !BuildlessVersioning
			Assert.AreEqual(new ApplicationVersion(1, 0, 1), new ApplicationVersion(1, 0, 0).IncrementedBuild);
			Assert.AreEqual(new ApplicationVersion(1, 0, 0), new ApplicationVersion(1, 0, 1).DecrementedBuild);
#endif
#if !BuildlessVersioning
			Assert.AreEqual(new ApplicationVersion(1, 0, 1), new ApplicationVersion(1, 0, 0).IncrementedLower);
			Assert.AreEqual(new ApplicationVersion(1, 0, 0), new ApplicationVersion(1, 0, 1).DecrementedLower);
#else
			Assert.AreEqual(new ApplicationVersion(1, 1), new ApplicationVersion(1, 0).IncrementedLower);
			Assert.AreEqual(new ApplicationVersion(1, 0), new ApplicationVersion(1, 1).DecrementedLower);
#endif

			// Throws if tried to increment/decrement to out of range.
			Assert.Throws<Exception>(() => { var dummy = new ApplicationVersion(ApplicationVersion.MaxMajorVersion, 0).IncrementedMajor; });
			Assert.Throws<Exception>(() => { var dummy = new ApplicationVersion(ApplicationVersion.MinMajorVersion, 0).DecrementedMajor; });
			Assert.Throws<Exception>(() => { var dummy = new ApplicationVersion(1, ApplicationVersion.MaxMinorVersion).IncrementedMinor; });
			Assert.Throws<Exception>(() => { var dummy = new ApplicationVersion(1, ApplicationVersion.MinMinorVersion).DecrementedMinor; });
#if !BuildlessVersioning
			Assert.Throws<Exception>(() => { var dummy = new ApplicationVersion(1, 0, ApplicationVersion.MaxBuildVersion).IncrementedBuild; });
			Assert.Throws<Exception>(() => { var dummy = new ApplicationVersion(1, 0, ApplicationVersion.MinBuildVersion).DecrementedBuild; });
#endif
#if !BuildlessVersioning
			Assert.Throws<Exception>(() => { var dummy = new ApplicationVersion(1, 0, ApplicationVersion.MaxBuildVersion).IncrementedLower; });
			Assert.Throws<Exception>(() => { var dummy = new ApplicationVersion(1, 0, ApplicationVersion.MinBuildVersion).DecrementedLower; });
#else
			Assert.Throws<Exception>(() => { var dummy = new ApplicationVersion(1, ApplicationVersion.MaxMinorVersion).IncrementedLower; });
			Assert.Throws<Exception>(() => { var dummy = new ApplicationVersion(1, ApplicationVersion.MinMinorVersion).IncrementedLower; });
#endif
		}

		[Test]
		public static void FromCombined()
		{
			// Throws if zero or negative
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(0));
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(-1));
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(-1000));
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(int.MinValue));

			// Throws if Major is below 1
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(ApplicationVersion.MinorAndBuildDigits - 1));
			Assert.AreEqual(new ApplicationVersion(1, 0), new ApplicationVersion(ApplicationVersion.MinorAndBuildDigits)); // This line is just here to make sure we use "MinorAndBuildDigits-1" above properly.
			Assert.AreEqual(new ApplicationVersion(1, 0), new ApplicationVersion(1 * 1000 * 10000));

			// Throws if Major is above 49
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(500000000));
#if !BuildlessVersioning
			Assert.AreEqual(new ApplicationVersion(49, 999, 9999), new ApplicationVersion(499999999));
#else
			Assert.AreEqual(new ApplicationVersion(49, 999), new ApplicationVersion(499990000));
#endif

			// Some random examples
#if !BuildlessVersioning
			Assert.AreEqual(new ApplicationVersion(1, 0, 1), new ApplicationVersion(10000001));
			Assert.AreEqual(new ApplicationVersion(1, 0, 3), new ApplicationVersion(10000003));
			Assert.AreEqual(new ApplicationVersion(1, 2, 3), new ApplicationVersion(10020003));
			Assert.AreEqual(new ApplicationVersion(3, 5, 8), new ApplicationVersion(30050008));
			Assert.AreEqual(new ApplicationVersion(9, 21, 43), new ApplicationVersion(90210043));
			Assert.AreEqual(new ApplicationVersion(15, 121, 5443), new ApplicationVersion(151215443));
			Assert.AreEqual(new ApplicationVersion(1, 999, 9999), new ApplicationVersion(19999999));
#else
			Assert.AreEqual(new ApplicationVersion(1, 0), new ApplicationVersion(10000000));
			Assert.AreEqual(new ApplicationVersion(1, 0), new ApplicationVersion(10000000));
			Assert.AreEqual(new ApplicationVersion(1, 2), new ApplicationVersion(10020000));
			Assert.AreEqual(new ApplicationVersion(3, 5), new ApplicationVersion(30050000));
			Assert.AreEqual(new ApplicationVersion(9, 21), new ApplicationVersion(90210000));
			Assert.AreEqual(new ApplicationVersion(15, 121), new ApplicationVersion(151210000));
			Assert.AreEqual(new ApplicationVersion(1, 999), new ApplicationVersion(19990000));
#endif
		}

		[Test]
		public static void Parse()
		{
#if !BuildlessVersioning
			Assert.AreEqual(new ApplicationVersion(1, 0, 0), new ApplicationVersion("1.0.0"));
			Assert.AreEqual(new ApplicationVersion(1, 1, 0), new ApplicationVersion("1.1.0"));
			Assert.AreEqual(new ApplicationVersion(1, 0, 1), new ApplicationVersion("1.0.1"));
			Assert.AreEqual(new ApplicationVersion(2, 3, 4), new ApplicationVersion("2.3.4"));
			Assert.AreEqual(new ApplicationVersion(12, 34, 56), new ApplicationVersion("12.34.56"));
			Assert.AreEqual(new ApplicationVersion(1, 999, 9999), new ApplicationVersion("1.999.9999"));
			Assert.AreEqual(new ApplicationVersion(49, 999, 9999), new ApplicationVersion("49.999.9999"));

			// Throws out of range
			Assert.Throws<Exception>(() => new ApplicationVersion("0.0.0"));
			Assert.Throws<Exception>(() => new ApplicationVersion("-1.0.0"));
			Assert.Throws<Exception>(() => new ApplicationVersion("50.0.0"));
			Assert.Throws<Exception>(() => new ApplicationVersion("1.1000.0"));
			Assert.Throws<Exception>(() => new ApplicationVersion("1.-1.0"));
			Assert.Throws<Exception>(() => new ApplicationVersion("1.0.10000"));
			Assert.Throws<Exception>(() => new ApplicationVersion("1.0.-1"));
#else
			Assert.AreEqual(new ApplicationVersion(1, 0), new ApplicationVersion("1.0"));
			Assert.AreEqual(new ApplicationVersion(1, 1), new ApplicationVersion("1.1"));
			Assert.AreEqual(new ApplicationVersion(2, 3), new ApplicationVersion("2.3"));
			Assert.AreEqual(new ApplicationVersion(12, 34), new ApplicationVersion("12.34"));
			Assert.AreEqual(new ApplicationVersion(1, 999), new ApplicationVersion("1.999"));
			Assert.AreEqual(new ApplicationVersion(49, 999), new ApplicationVersion("49.999"));

			// Throws out of range
			Assert.Throws<Exception>(() => new ApplicationVersion("0.0"));
			Assert.Throws<Exception>(() => new ApplicationVersion("-1.0"));
			Assert.Throws<Exception>(() => new ApplicationVersion("50.0"));
			Assert.Throws<Exception>(() => new ApplicationVersion("1.1000"));
			Assert.Throws<Exception>(() => new ApplicationVersion("1.-1"));
#endif

			// Throws if format does not match
			Assert.Throws<Exception>(() => new ApplicationVersion(""));
			Assert.Throws<Exception>(() => new ApplicationVersion(".."));
			Assert.Throws<Exception>(() => new ApplicationVersion(" . . "));
			Assert.Throws<Exception>(() => new ApplicationVersion("a.0.1"));
			Assert.Throws<Exception>(() => new ApplicationVersion("1.0.1a"));
			Assert.Throws<Exception>(() => new ApplicationVersion("1.0a.1"));
			Assert.Throws<Exception>(() => new ApplicationVersion("1.0.1f1"));
			Assert.Throws<Exception>(() => new ApplicationVersion("a.1"));
			Assert.Throws<Exception>(() => new ApplicationVersion("1.1a"));
			Assert.Throws<Exception>(() => new ApplicationVersion("1.1f1"));
		}

		[Test]
		public static void LeadingZeroesWillDisappear()
		{
#if !BuildlessVersioning
			Assert.AreEqual("1.0.0", new ApplicationVersion("1.00.00").ToString());
			Assert.AreEqual("1.1.0", new ApplicationVersion("1.01.00").ToString());
			Assert.AreEqual("1.0.1", new ApplicationVersion("1.00.01").ToString());
			Assert.AreEqual("1.10.10", new ApplicationVersion("1.10.10").ToString());
			Assert.AreEqual("1.10.10", new ApplicationVersion("1.010.010").ToString());
			Assert.AreEqual("1.10.10", new ApplicationVersion("1.000010.000010").ToString());
#else
			Assert.AreEqual("1.0", new ApplicationVersion("1.00").ToString());
			Assert.AreEqual("1.1", new ApplicationVersion("1.01").ToString());
			Assert.AreEqual("1.10", new ApplicationVersion("1.10").ToString());
			Assert.AreEqual("1.10", new ApplicationVersion("1.010").ToString());
			Assert.AreEqual("1.10", new ApplicationVersion("1.000010").ToString());
#endif
		}
	}

}
