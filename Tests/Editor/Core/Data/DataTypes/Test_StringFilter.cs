using System;
using Extenity.DataToolbox;
using Extenity.Testing;
using NUnit.Framework;

namespace ExtenityTests.DataToolbox
{

	public class Test_StringFilter : ExtenityTestBase
	{
		#region String Operations

		[Test]
		public void StringFilterUsage()
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
			// Some examples - Any
			Assert.IsTrue(new StringFilter(new StringFilterEntry(StringFilterType.Any, "")).IsMatching("Good old text"));
			Assert.IsTrue(new StringFilter(new StringFilterEntry(StringFilterType.Any, "Filter is ignored for this type")).IsMatching("Good old text"));
			Assert.IsTrue(StringFilter.Any.IsMatching("Good old text")); // This is the shortcut for 'Any'. Good for using as a default value.
			// Some examples - Empty
			Assert.IsFalse(new StringFilter(new StringFilterEntry(StringFilterType.Empty, "")).IsMatching("Good old text"));
			Assert.IsFalse(new StringFilter(new StringFilterEntry(StringFilterType.Empty, "Filter is ignored for this type")).IsMatching("Good old text"));
			Assert.IsFalse(StringFilter.Empty.IsMatching("Good old text")); // This is the shortcut for 'Empty'. Good for using as a default value.
			Assert.IsTrue(new StringFilter(new StringFilterEntry(StringFilterType.Empty, "")).IsMatching(""));
			Assert.IsTrue(new StringFilter(new StringFilterEntry(StringFilterType.Empty, "Filter is ignored for this type")).IsMatching(""));
			Assert.IsTrue(StringFilter.Empty.IsMatching("")); // This is the shortcut for 'Empty'. Good for using as a default value.
			Assert.IsTrue(new StringFilter(new StringFilterEntry(StringFilterType.Empty, "")).IsMatching(null));
			Assert.IsTrue(new StringFilter(new StringFilterEntry(StringFilterType.Empty, "Filter is ignored for this type")).IsMatching(null));
			Assert.IsTrue(StringFilter.Empty.IsMatching(null)); // This is the shortcut for 'Empty'. Good for using as a default value.

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
			Assert.IsTrue(new StringFilter( // One 'Any' is enough to override the others, which likely makes it nonsensical to be used in multiple conditions.
				              new StringFilterEntry(StringFilterType.Any, ""),
				              new StringFilterEntry(StringFilterType.Contains, "NOPE")
			              ).IsMatching("Good old text"));
			Assert.IsTrue(new StringFilter( // One 'Any' is enough to override the others, which likely makes it nonsensical to be used in multiple conditions.
				              new StringFilterEntry(StringFilterType.Contains, "NOPE"),
				              new StringFilterEntry(StringFilterType.Any, "")
			              ).IsMatching("Good old text"));
		}

		#endregion
	}

}
