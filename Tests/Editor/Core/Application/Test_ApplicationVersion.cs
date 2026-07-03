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
			Assert.AreEqual(0, ApplicationVersion.MinMajorVersion);
			Assert.AreEqual(1, ApplicationVersion.MinMinorVersion);
			Assert.AreEqual(1, ApplicationVersion.MinBuildVersion);

			Assert.AreEqual(49, ApplicationVersion.MaxMajorVersion);
			Assert.AreEqual(99, ApplicationVersion.MaxMinorVersion);
			Assert.AreEqual(999, ApplicationVersion.MaxBuildVersion);

			Assert.AreEqual(100, ApplicationVersion.MinorDigits);
			Assert.AreEqual(1000, ApplicationVersion.BuildDigits);
		}

		[Test]
		public static void VersionRangeLimits()
		{
			// Major version
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(ApplicationVersion.MinMajorVersion - 1, 1, 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(ApplicationVersion.MaxMajorVersion + 1, 1, 1));
			Assert.DoesNotThrow(() => new ApplicationVersion(ApplicationVersion.MinMajorVersion, 1, 1));
			Assert.DoesNotThrow(() => new ApplicationVersion(ApplicationVersion.MaxMajorVersion, 1, 1));

			// Minor version
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(1, ApplicationVersion.MinMinorVersion - 1, 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(1, ApplicationVersion.MaxMinorVersion + 1, 1));
			Assert.DoesNotThrow(() => new ApplicationVersion(1, ApplicationVersion.MinMinorVersion, 1));
			Assert.DoesNotThrow(() => new ApplicationVersion(1, ApplicationVersion.MaxMinorVersion, 1));

			// Build version
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(1, 1, ApplicationVersion.MinBuildVersion - 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(1, 1, ApplicationVersion.MaxBuildVersion + 1));
			Assert.DoesNotThrow(() => new ApplicationVersion(1, 1, ApplicationVersion.MinBuildVersion));
			Assert.DoesNotThrow(() => new ApplicationVersion(1, 1, ApplicationVersion.MaxBuildVersion));
		}

		[Test]
		public static void Combined()
		{
			Assert.AreEqual(1001, new ApplicationVersion(0, 1, 1).Combined);
			Assert.AreEqual(102003, new ApplicationVersion(1, 2, 3).Combined);
			Assert.AreEqual(121043, new ApplicationVersion(1, 21, 43).Combined);
			Assert.AreEqual(4999999, new ApplicationVersion(49, 99, 999).Combined);
		}

		[Test]
		public static void IncrementsAndDecrements()
		{
			Assert.AreEqual(new ApplicationVersion(2, 1, 1), new ApplicationVersion(1, 1, 1).IncrementedMajor);
			Assert.AreEqual(new ApplicationVersion(1, 1, 1), new ApplicationVersion(2, 1, 1).DecrementedMajor);
			Assert.AreEqual(new ApplicationVersion(1, 2, 1), new ApplicationVersion(1, 1, 1).IncrementedMinor);
			Assert.AreEqual(new ApplicationVersion(1, 1, 1), new ApplicationVersion(1, 2, 1).DecrementedMinor);
			Assert.AreEqual(new ApplicationVersion(1, 1, 2), new ApplicationVersion(1, 1, 1).IncrementedBuild);
			Assert.AreEqual(new ApplicationVersion(1, 1, 1), new ApplicationVersion(1, 1, 2).DecrementedBuild);

			// Throws if tried to increment/decrement to out of range.
			Assert.Throws<Exception>(() => { var dummy = new ApplicationVersion(ApplicationVersion.MaxMajorVersion, 1, 1).IncrementedMajor; });
			Assert.Throws<Exception>(() => { var dummy = new ApplicationVersion(ApplicationVersion.MinMajorVersion, 1, 1).DecrementedMajor; });
			Assert.Throws<Exception>(() => { var dummy = new ApplicationVersion(1, ApplicationVersion.MaxMinorVersion, 1).IncrementedMinor; });
			Assert.Throws<Exception>(() => { var dummy = new ApplicationVersion(1, ApplicationVersion.MinMinorVersion, 1).DecrementedMinor; });
			Assert.Throws<Exception>(() => { var dummy = new ApplicationVersion(1, 1, ApplicationVersion.MaxBuildVersion).IncrementedBuild; });
			Assert.Throws<Exception>(() => { var dummy = new ApplicationVersion(1, 1, ApplicationVersion.MinBuildVersion).DecrementedBuild; });
		}

		[Test]
		public static void FromCombined()
		{
			// Throws if zero or negative
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(0));
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(-1));
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(-1000));
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(int.MinValue));

			// Throws if Minor or Build is below 1
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(1)); // 0.0.1
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(1000)); // 0.1.0
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(100000)); // 1.0.0
			Assert.AreEqual(new ApplicationVersion(0, 1, 1), new ApplicationVersion(1001)); // 0.1.1 is the smallest valid version.

			// Throws if Major is above 49
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(5001001)); // 50.1.1
			Assert.AreEqual(new ApplicationVersion(49, 99, 999), new ApplicationVersion(4999999));

			// Some random examples
			Assert.AreEqual(new ApplicationVersion(1, 1, 1), new ApplicationVersion(101001));
			Assert.AreEqual(new ApplicationVersion(0, 5, 443), new ApplicationVersion(5443));
			Assert.AreEqual(new ApplicationVersion(1, 2, 3), new ApplicationVersion(102003));
			Assert.AreEqual(new ApplicationVersion(3, 5, 8), new ApplicationVersion(305008));
			Assert.AreEqual(new ApplicationVersion(9, 21, 43), new ApplicationVersion(921043));
			Assert.AreEqual(new ApplicationVersion(15, 81, 543), new ApplicationVersion(1581543));
			Assert.AreEqual(new ApplicationVersion(1, 99, 999), new ApplicationVersion(199999));
		}

		[Test]
		public static void Parse()
		{
			Assert.AreEqual(new ApplicationVersion(0, 1, 1), new ApplicationVersion("0.1.1"));
			Assert.AreEqual(new ApplicationVersion(0, 5, 443), new ApplicationVersion("0.5.443"));
			Assert.AreEqual(new ApplicationVersion(1, 1, 1), new ApplicationVersion("1.1.1"));
			Assert.AreEqual(new ApplicationVersion(2, 3, 4), new ApplicationVersion("2.3.4"));
			Assert.AreEqual(new ApplicationVersion(12, 34, 56), new ApplicationVersion("12.34.56"));
			Assert.AreEqual(new ApplicationVersion(1, 99, 999), new ApplicationVersion("1.99.999"));
			Assert.AreEqual(new ApplicationVersion(49, 99, 999), new ApplicationVersion("49.99.999"));

			// Throws out of range
			Assert.Throws<Exception>(() => new ApplicationVersion("0.0.0"));
			Assert.Throws<Exception>(() => new ApplicationVersion("-1.1.1"));
			Assert.Throws<Exception>(() => new ApplicationVersion("50.1.1"));
			Assert.Throws<Exception>(() => new ApplicationVersion("1.0.1"));
			Assert.Throws<Exception>(() => new ApplicationVersion("1.100.1"));
			Assert.Throws<Exception>(() => new ApplicationVersion("1.-1.1"));
			Assert.Throws<Exception>(() => new ApplicationVersion("1.1.0"));
			Assert.Throws<Exception>(() => new ApplicationVersion("1.1.1000"));
			Assert.Throws<Exception>(() => new ApplicationVersion("1.1.-1"));

			// Throws if format does not match
			Assert.Throws<Exception>(() => new ApplicationVersion(""));
			Assert.Throws<Exception>(() => new ApplicationVersion(".."));
			Assert.Throws<Exception>(() => new ApplicationVersion(" . . "));
			Assert.Throws<Exception>(() => new ApplicationVersion("a.1.1"));
			Assert.Throws<Exception>(() => new ApplicationVersion("1.1.1a"));
			Assert.Throws<Exception>(() => new ApplicationVersion("1.1a.1"));
			Assert.Throws<Exception>(() => new ApplicationVersion("1.1.1f1"));
			Assert.Throws<Exception>(() => new ApplicationVersion("a.1"));
			Assert.Throws<Exception>(() => new ApplicationVersion("1.1a"));
			Assert.Throws<Exception>(() => new ApplicationVersion("1.1f1"));
		}

		[Test]
		public static void LeadingZeroesWillDisappear()
		{
			Assert.AreEqual("1.1.1", new ApplicationVersion("1.01.01").ToString());
			Assert.AreEqual("0.1.1", new ApplicationVersion("00.01.01").ToString());
			Assert.AreEqual("1.10.10", new ApplicationVersion("1.10.10").ToString());
			Assert.AreEqual("1.10.10", new ApplicationVersion("1.010.010").ToString());
			Assert.AreEqual("1.10.10", new ApplicationVersion("1.000010.000010").ToString());
		}
	}

}
