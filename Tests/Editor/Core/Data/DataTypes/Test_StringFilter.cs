using System;
using Extenity.DataToolbox;
using NUnit.Framework;

namespace ExtenityTests.DataToolbox
{

	public class Test_StringFilter : AssertionHelper
	{
		#region String Operations

		[Test]
		public void StringFilter()
		{
			// Does not match if not setup correctly
			Assert.IsFalse(new StringFilter().IsMatching("Good old text"));
			Assert.IsFalse(new StringFilter(new StringFilterEntry()).IsMatching("Good old text"));

			// Some examples
			Assert.IsFalse(new StringFilter(new StringFilterEntry(StringFilterType.Contains, "Not expected")).IsMatching("Good old text"));
			Assert.IsTrue(new StringFilter(new StringFilterEntry(StringFilterType.Contains, "old")).IsMatching("Good old text"));
			Assert.IsFalse(new StringFilter(new StringFilterEntry(StringFilterType.Exactly, "old")).IsMatching("Good old text"));
			Assert.IsTrue(new StringFilter(new StringFilterEntry(StringFilterType.Exactly, "Good old text")).IsMatching("Good old text"));
			Assert.IsFalse(new StringFilter(new StringFilterEntry(StringFilterType.StartsWith, "old")).IsMatching("Good old text"));
			Assert.IsTrue(new StringFilter(new StringFilterEntry(StringFilterType.StartsWith, "Good")).IsMatching("Good old text"));
			Assert.IsFalse(new StringFilter(new StringFilterEntry(StringFilterType.EndsWith, "old")).IsMatching("Good old text"));
			Assert.IsTrue(new StringFilter(new StringFilterEntry(StringFilterType.EndsWith, "text")).IsMatching("Good old text"));

			// Case insensitive
			Assert.IsFalse(new StringFilter(new StringFilterEntry(StringFilterType.Exactly, "Good OLD text")).IsMatching("Good old text"));
			Assert.IsFalse(new StringFilter(new StringFilterEntry(StringFilterType.Exactly, "Good OLD text", StringComparison.InvariantCulture)).IsMatching("Good old text"));
			Assert.IsTrue(new StringFilter(new StringFilterEntry(StringFilterType.Exactly, "Good OLD text", StringComparison.InvariantCultureIgnoreCase)).IsMatching("Good old text"));

			// Wildcards
			Assert.IsTrue(new StringFilter(new StringFilterEntry(StringFilterType.Wildcard, "*old*")).IsMatching("Good old text"));
			Assert.IsFalse(new StringFilter(new StringFilterEntry(StringFilterType.Wildcard, "*old")).IsMatching("Good old text"));
			Assert.IsFalse(new StringFilter(new StringFilterEntry(StringFilterType.Wildcard, "old*")).IsMatching("Good old text"));
			Assert.IsTrue(new StringFilter(new StringFilterEntry(StringFilterType.Wildcard, "Good*")).IsMatching("Good old text"));
			Assert.IsTrue(new StringFilter(new StringFilterEntry(StringFilterType.Wildcard, "*text")).IsMatching("Good old text"));
			Assert.IsTrue(new StringFilter(new StringFilterEntry(StringFilterType.Wildcard, "Good*text")).IsMatching("Good old text"));
			Assert.IsFalse(new StringFilter(new StringFilterEntry(StringFilterType.Wildcard, "old")).IsMatching("Good old text")); // Acts as 'Exactly' with no wildcards
			Assert.IsTrue(new StringFilter(new StringFilterEntry(StringFilterType.Wildcard, "Good old text")).IsMatching("Good old text")); // Acts as 'Exactly' with no wildcards
			Assert.IsFalse(new StringFilter(new StringFilterEntry(StringFilterType.Wildcard, "Good ? text")).IsMatching("Good old text"));
			Assert.IsTrue(new StringFilter(new StringFilterEntry(StringFilterType.Wildcard, "Good ??? text")).IsMatching("Good old text"));
			Assert.IsTrue(new StringFilter(new StringFilterEntry(StringFilterType.Wildcard, "Go?d ol? ?ext")).IsMatching("Good old text"));
			Assert.IsTrue(new StringFilter(new StringFilterEntry(StringFilterType.Wildcard, "Go?d * ?ext")).IsMatching("Good old text"));

			// Multiple conditions
			Assert.IsTrue(new StringFilter( // Matching any one filter succeeds
				new StringFilterEntry(StringFilterType.Contains, "old"),
				new StringFilterEntry(StringFilterType.EndsWith, "NOPE")
			).IsMatching("Good old text"));
			Assert.IsTrue(new StringFilter( // Matching any one filter succeeds
				new StringFilterEntry(StringFilterType.Contains, "NOPE"),
				new StringFilterEntry(StringFilterType.EndsWith, "text")
			).IsMatching("Good old text"));
			Assert.IsFalse(new StringFilter( // Matching any one filter succeeds
				new StringFilterEntry(StringFilterType.Contains, "NO"),
				new StringFilterEntry(StringFilterType.EndsWith, "NOPE")
			).IsMatching("Good old text"));
			Assert.IsTrue(new StringFilter( // All 'MustMatch' checks must match
				new StringFilterEntry(StringFilterType.Contains, "old"),
				new StringFilterEntry(StringFilterType.EndsWith, "text") { MustMatch = true }
			).IsMatching("Good old text"));
			Assert.IsTrue(new StringFilter( // All 'MustMatch' checks must match
				new StringFilterEntry(StringFilterType.Contains, "old"),
				new StringFilterEntry(StringFilterType.StartsWith, "Good") { MustMatch = true },
				new StringFilterEntry(StringFilterType.EndsWith, "text") { MustMatch = true }
			).IsMatching("Good old text"));
			Assert.IsFalse(new StringFilter( // All 'MustMatch' checks must match
				new StringFilterEntry(StringFilterType.Contains, "old"),
				new StringFilterEntry(StringFilterType.StartsWith, "Good") { MustMatch = true },
				new StringFilterEntry(StringFilterType.EndsWith, "NOPE") { MustMatch = true }
			).IsMatching("Good old text"));
			Assert.IsFalse(new StringFilter( // All 'MustMatch' checks must match
				new StringFilterEntry(StringFilterType.Contains, "old"),
				new StringFilterEntry(StringFilterType.StartsWith, "NOPE") { MustMatch = true },
				new StringFilterEntry(StringFilterType.EndsWith, "text") { MustMatch = true }
			).IsMatching("Good old text"));
		}


		#endregion
	}

}
