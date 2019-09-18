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
			Assert.AreEqual(0, ApplicationVersion.MinBuildVersion);

			Assert.AreEqual(49, ApplicationVersion.MaxMajorVersion);
			Assert.AreEqual(999, ApplicationVersion.MaxMinorVersion);
			Assert.AreEqual(9999, ApplicationVersion.MaxBuildVersion);

			Assert.AreEqual(1000, ApplicationVersion.MinorDigits);
			Assert.AreEqual(10000, ApplicationVersion.BuildDigits);
		}

		[Test]
		public static void VersionRangeLimits()
		{
			// Major version
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(ApplicationVersion.MinMajorVersion - 1, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(ApplicationVersion.MaxMajorVersion + 1, 0, 0));
			Assert.DoesNotThrow(() => new ApplicationVersion(ApplicationVersion.MinMajorVersion, 0, 0));
			Assert.DoesNotThrow(() => new ApplicationVersion(ApplicationVersion.MaxMajorVersion, 0, 0));

			// Minor version
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(1, ApplicationVersion.MinMinorVersion - 1, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(1, ApplicationVersion.MaxMinorVersion + 1, 0));
			Assert.DoesNotThrow(() => new ApplicationVersion(1, ApplicationVersion.MinMinorVersion, 0));
			Assert.DoesNotThrow(() => new ApplicationVersion(1, ApplicationVersion.MinMinorVersion, 0));

			// Build version
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(1, 0, ApplicationVersion.MinBuildVersion - 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(1, 0, ApplicationVersion.MaxBuildVersion + 1));
			Assert.DoesNotThrow(() => new ApplicationVersion(1, 0, ApplicationVersion.MinBuildVersion));
			Assert.DoesNotThrow(() => new ApplicationVersion(1, 0, ApplicationVersion.MinBuildVersion));
		}

		[Test]
		public static void Combined()
		{
			Assert.AreEqual(10000000, new ApplicationVersion(1, 0, 0).Combined);
			Assert.AreEqual(10020003, new ApplicationVersion(1, 2, 3).Combined);
			Assert.AreEqual(10210043, new ApplicationVersion(1, 21, 43).Combined);
			Assert.AreEqual(499999999, new ApplicationVersion(49, 999, 9999).Combined);
		}

		[Test]
		public static void IncrementsAndDecrements()
		{
			Assert.AreEqual(new ApplicationVersion(2, 0, 0), new ApplicationVersion(1, 0, 0).IncrementedMajor);
			Assert.AreEqual(new ApplicationVersion(1, 0, 0), new ApplicationVersion(2, 0, 0).DecrementedMajor);
			Assert.AreEqual(new ApplicationVersion(1, 1, 0), new ApplicationVersion(1, 0, 0).IncrementedMinor);
			Assert.AreEqual(new ApplicationVersion(1, 0, 0), new ApplicationVersion(1, 1, 0).DecrementedMinor);
			Assert.AreEqual(new ApplicationVersion(1, 0, 1), new ApplicationVersion(1, 0, 0).IncrementedBuild);
			Assert.AreEqual(new ApplicationVersion(1, 0, 0), new ApplicationVersion(1, 0, 1).DecrementedBuild);

			// Throws if tried to increment/decrement to out of range.
			Assert.Throws<Exception>(() => { var dummy = new ApplicationVersion(ApplicationVersion.MaxMajorVersion, 0, 0).IncrementedMajor; });
			Assert.Throws<Exception>(() => { var dummy = new ApplicationVersion(ApplicationVersion.MinMajorVersion, 0, 0).DecrementedMajor; });
			Assert.Throws<Exception>(() => { var dummy = new ApplicationVersion(1, ApplicationVersion.MaxMinorVersion, 0).IncrementedMinor; });
			Assert.Throws<Exception>(() => { var dummy = new ApplicationVersion(1, ApplicationVersion.MinMinorVersion, 0).DecrementedMinor; });
			Assert.Throws<Exception>(() => { var dummy = new ApplicationVersion(1, 0, ApplicationVersion.MaxBuildVersion).IncrementedBuild; });
			Assert.Throws<Exception>(() => { var dummy = new ApplicationVersion(1, 0, ApplicationVersion.MinBuildVersion).DecrementedBuild; });
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
			Assert.AreEqual(new ApplicationVersion(1, 0, 0), new ApplicationVersion(ApplicationVersion.MinorAndBuildDigits)); // This line is just here to make sure we use "MinorAndBuildDigits-1" above properly.
			Assert.AreEqual(new ApplicationVersion(1, 0, 0), new ApplicationVersion(1 * 1000 * 10000));

			// Throws if Major is above 49
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(500000000));
			Assert.AreEqual(new ApplicationVersion(49, 999, 9999), new ApplicationVersion(499999999));

			// Some random examples
			Assert.AreEqual(new ApplicationVersion(1, 0, 1), new ApplicationVersion(10000001));
			Assert.AreEqual(new ApplicationVersion(1, 0, 3), new ApplicationVersion(10000003));
			Assert.AreEqual(new ApplicationVersion(1, 2, 3), new ApplicationVersion(10020003));
			Assert.AreEqual(new ApplicationVersion(3, 5, 8), new ApplicationVersion(30050008));
			Assert.AreEqual(new ApplicationVersion(9, 21, 43), new ApplicationVersion(90210043));
			Assert.AreEqual(new ApplicationVersion(15, 121, 5443), new ApplicationVersion(151215443));
			Assert.AreEqual(new ApplicationVersion(1, 999, 9999), new ApplicationVersion(19999999));
		}

		[Test]
		public static void Parse()
		{
			Assert.AreEqual(new ApplicationVersion(1, 0, 0), new ApplicationVersion("1.0.0"));
			Assert.AreEqual(new ApplicationVersion(1, 1, 0), new ApplicationVersion("1.1.0"));
			Assert.AreEqual(new ApplicationVersion(1, 0, 1), new ApplicationVersion("1.0.1"));
			Assert.AreEqual(new ApplicationVersion(2, 3, 4), new ApplicationVersion("2.3.4"));
			Assert.AreEqual(new ApplicationVersion(12, 34, 56), new ApplicationVersion("12.34.56"));
			Assert.AreEqual(new ApplicationVersion(1, 999, 9999), new ApplicationVersion("1.999.9999"));
			Assert.AreEqual(new ApplicationVersion(49, 999, 9999), new ApplicationVersion("49.999.9999"));

			// Throws if format does not match
			Assert.Throws<Exception>(() => new ApplicationVersion(""));
			Assert.Throws<Exception>(() => new ApplicationVersion(".."));
			Assert.Throws<Exception>(() => new ApplicationVersion(" . . "));
			Assert.Throws<Exception>(() => new ApplicationVersion("a.0.1"));
			Assert.Throws<Exception>(() => new ApplicationVersion("1.0.1a"));
			Assert.Throws<Exception>(() => new ApplicationVersion("1.0a.1"));
			Assert.Throws<Exception>(() => new ApplicationVersion("1.0.1f1"));

			// Throws out of range
			Assert.Throws<Exception>(() => new ApplicationVersion("0.0.0"));
			Assert.Throws<Exception>(() => new ApplicationVersion("-1.0.0"));
			Assert.Throws<Exception>(() => new ApplicationVersion("50.0.0"));
			Assert.Throws<Exception>(() => new ApplicationVersion("1.1000.0"));
			Assert.Throws<Exception>(() => new ApplicationVersion("1.-1.0"));
			Assert.Throws<Exception>(() => new ApplicationVersion("1.0.10000"));
			Assert.Throws<Exception>(() => new ApplicationVersion("1.0.-1"));
		}

		[Test]
		public static void LeadingZeroesWillDisappear()
		{
			Assert.AreEqual("1.0.0", new ApplicationVersion("1.00.00").ToString());
			Assert.AreEqual("1.1.0", new ApplicationVersion("1.01.00").ToString());
			Assert.AreEqual("1.0.1", new ApplicationVersion("1.00.01").ToString());
			Assert.AreEqual("1.10.10", new ApplicationVersion("1.10.10").ToString());
			Assert.AreEqual("1.10.10", new ApplicationVersion("1.010.010").ToString());
			Assert.AreEqual("1.10.10", new ApplicationVersion("1.000010.000010").ToString());
		}
	}

}
