using System.Collections.Generic;
using NUnit.Framework;

namespace Extenity.Testing
{

	public static class AssertTools
	{
		public static void AreEqual<T>(IList<T> expected, int expectedLength, IList<T> actual, int actualLength, string message = null)
		{
			// Make sure input data's integrity.
			if (expectedLength > 0)
			{
				Assert.NotNull(expected, "Expected list is missing.");
				Assert.GreaterOrEqual(expected.Count, expectedLength, $"Expected list size '{expected.Count}' does not cover the specified expectedLength '{expectedLength}'.");
			}
			if (actualLength > 0)
			{
				Assert.NotNull(actual, "Actual list is missing.");
				Assert.GreaterOrEqual(actual.Count, actualLength, $"Actual list size '{actual.Count}' does not cover the specified actualLength '{actualLength}'.");
			}

			Assert.AreEqual(expectedLength, actualLength, message);
			for (int i = 0; i < expectedLength; i++)
			{
				Assert.AreEqual(expected[i], actual[i], message);
			}
		}
	}

}
