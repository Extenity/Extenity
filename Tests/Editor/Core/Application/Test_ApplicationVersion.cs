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
			Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationVersion(0, 1, 1));

			// Maximum allowed Major version is 50.
			// But that is for no good reason. May safely be changed in future. But think wisely about how that would change other systems that uses the version.
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
		public static void FromCombined()
		{
			throw new NotImplementedException();
		}

		[Test]
		public static void Parse()
		{
			throw new NotImplementedException();
		}
	}

}
