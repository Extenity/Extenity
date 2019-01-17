using System;
using Extenity.ApplicationToolbox;
using NUnit.Framework;

namespace ExtenityTests.ApplicationToolbox
{

	public class Test_ApplicationVersion : AssertionHelper
	{
		[Test]
		public static void VersionRangeLimits()
		{
			// Minimum allowed Major version is 1. That is, 0 for Major is not allowed.
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(0, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(0, 1, 1));

			// Maximum allowed Major version is 50.
			// But that is for no good reason. May safely be changed in future. But think wisely about how that would change other systems that uses the version.
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(50, 0, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(50, 1, 1));
			for (int i = 1; i <= 49; i++)
			{
				Assert.DoesNotThrow(() => new ApplicationVersion(i, 0, 0));
			}

			// Minor and Build versions should be in range of 0-99 (inclusive)
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(1, 100, 0));
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(1, 0, 100));
			for (int i = 0; i <= 99; i++)
			{
				Assert.DoesNotThrow(() => new ApplicationVersion(1, 0, i));
				Assert.DoesNotThrow(() => new ApplicationVersion(1, i, 0));
			}
		}

		[Test]
		public static void Combined()
		{
			Assert.AreEqual(10000, new ApplicationVersion(1, 0, 0).Combined);
			Assert.AreEqual(10203, new ApplicationVersion(1, 2, 3).Combined);
			Assert.AreEqual(12143, new ApplicationVersion(1, 21, 43).Combined);
			Assert.AreEqual(19999, new ApplicationVersion(1, 99, 99).Combined);
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
			Assert.Throws<ArgumentOutOfRangeException>(() => { var dummy = new ApplicationVersion(49, 0, 0).IncrementedMajor; });
			Assert.Throws<ArgumentOutOfRangeException>(() => { var dummy = new ApplicationVersion(1, 0, 0).DecrementedMajor; });
			Assert.Throws<ArgumentOutOfRangeException>(() => { var dummy = new ApplicationVersion(1, 99, 0).IncrementedMinor; });
			Assert.Throws<ArgumentOutOfRangeException>(() => { var dummy = new ApplicationVersion(1, 0, 0).DecrementedMinor; });
			Assert.Throws<ArgumentOutOfRangeException>(() => { var dummy = new ApplicationVersion(1, 0, 99).IncrementedBuild; });
			Assert.Throws<ArgumentOutOfRangeException>(() => { var dummy = new ApplicationVersion(1, 0, 0).DecrementedBuild; });
		}

		[Test]
		public static void FromCombined()
		{
			// Throws if zero or negative
			Assert.Throws<ArgumentOutOfRangeException>(() => ApplicationVersion.FromCombined(0));
			Assert.Throws<ArgumentOutOfRangeException>(() => ApplicationVersion.FromCombined(-1));

			// Throws if major is below 1
			for (int i = 0; i < ApplicationVersion.MajorDigits; i++)
			{
				Assert.Throws<ArgumentOutOfRangeException>(() => ApplicationVersion.FromCombined(i));
			}

			Assert.AreEqual(ApplicationVersion.FromCombined(10000), new ApplicationVersion(1, 0, 0));
			Assert.AreEqual(ApplicationVersion.FromCombined(10203), new ApplicationVersion(1, 2, 3));
			Assert.AreEqual(ApplicationVersion.FromCombined(12143), new ApplicationVersion(1, 21, 43));
			Assert.AreEqual(ApplicationVersion.FromCombined(19999), new ApplicationVersion(1, 99, 99));
		}

		[Test]
		public static void Parse()
		{
			Assert.AreEqual(new ApplicationVersion(1, 0, 0), ApplicationVersion.Parse("1.0.0"));
			Assert.AreEqual(new ApplicationVersion(1, 1, 0), ApplicationVersion.Parse("1.1.0"));
			Assert.AreEqual(new ApplicationVersion(1, 0, 1), ApplicationVersion.Parse("1.0.1"));
			Assert.AreEqual(new ApplicationVersion(2, 3, 4), ApplicationVersion.Parse("2.3.4"));
			Assert.AreEqual(new ApplicationVersion(12, 34, 56), ApplicationVersion.Parse("12.34.56"));

			// Throws if format does not match
			Assert.Throws<Exception>(() => ApplicationVersion.Parse(""));
			Assert.Throws<Exception>(() => ApplicationVersion.Parse(".."));
			Assert.Throws<Exception>(() => ApplicationVersion.Parse(" . . "));
			Assert.Throws<Exception>(() => ApplicationVersion.Parse("a.0.1"));
			Assert.Throws<Exception>(() => ApplicationVersion.Parse("1.0.1a"));
			Assert.Throws<Exception>(() => ApplicationVersion.Parse("1.0.1f1"));

			// Throws out of range
			Assert.Throws<ArgumentOutOfRangeException>(() => ApplicationVersion.Parse("0.0.0"));
			Assert.Throws<ArgumentOutOfRangeException>(() => ApplicationVersion.Parse("-1.0.0"));
			Assert.Throws<ArgumentOutOfRangeException>(() => ApplicationVersion.Parse("50.0.0"));
			Assert.Throws<ArgumentOutOfRangeException>(() => ApplicationVersion.Parse("1.100.0"));
			Assert.Throws<ArgumentOutOfRangeException>(() => ApplicationVersion.Parse("1.-1.0"));
			Assert.Throws<ArgumentOutOfRangeException>(() => ApplicationVersion.Parse("1.0.100"));
			Assert.Throws<ArgumentOutOfRangeException>(() => ApplicationVersion.Parse("1.0.-1"));
		}
	}

}
