using System.Collections;
using ExtenityTests.Common;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace ExtenityTests.FlowToolbox
{

	public class Test_UnityInvoke : TestBase_FastInvoke
	{
		#region Simple

		[UnityTest, Category(TestCategories.Cheesy), Timeout(TimeRequired_Simple * 3), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator Simple(bool startAtRandomTime) { yield return TestInvoke_Simple(DoUnityInvoke, 3, startAtRandomTime, 1); }

		#endregion

		#region Timing

		// Zero
		[UnityTest, Category(TestCategories.Cheesy), Timeout(TimeRequired_Zero * CheesyRepeats), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator Timing_1_Zero(bool startAtRandomTime) { yield return TestInvoke_Zero(DoUnityInvoke, startAtRandomTime, CheesyRepeats); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(TimeRequired_Zero * DetailedRepeats), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator Timing_1_Zero_Detailed(bool startAtRandomTime) { yield return TestInvoke_Zero(DoUnityInvoke, startAtRandomTime, DetailedRepeats); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(TimeRequired_Zero * OvernightRepeats), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator Timing_1_Zero_Overnight(bool startAtRandomTime) { yield return TestInvoke_Zero(DoUnityInvoke, startAtRandomTime, OvernightRepeats); }

		// Various
		[UnityTest, Category(TestCategories.Cheesy), Timeout(TimeRequired_Various * CheesyRepeats), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator Timing_2_Various(bool startAtRandomTime) { yield return TestInvoke_Various(DoUnityInvoke, startAtRandomTime, CheesyRepeats); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(TimeRequired_Various * DetailedRepeats), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator Timing_2_Various_Detailed(bool startAtRandomTime) { yield return TestInvoke_Various(DoUnityInvoke, startAtRandomTime, DetailedRepeats); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(TimeRequired_Various * OvernightRepeats), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator Timing_2_Various_Overnight(bool startAtRandomTime) { yield return TestInvoke_Various(DoUnityInvoke, startAtRandomTime, OvernightRepeats); }

		// LongRun
		[UnityTest, Category(TestCategories.Cheesy), Timeout(TimeRequired_Simple * CheesyLongRunDuration * LongRunRepeats), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator Timing_3_LongRun(bool startAtRandomTime) { yield return TestInvoke_Simple(DoUnityInvoke, CheesyLongRunDuration, startAtRandomTime, LongRunRepeats); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(TimeRequired_Simple * DetailedLongRunDuration * LongRunRepeats), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator Timing_3_LongRun_Detailed(bool startAtRandomTime) { yield return TestInvoke_Simple(DoUnityInvoke, DetailedLongRunDuration, startAtRandomTime, LongRunRepeats); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(TimeRequired_Simple * OvernightLongRunDuration * LongRunRepeats), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator Timing_3_LongRun_Overnight(bool startAtRandomTime) { yield return TestInvoke_Simple(DoUnityInvoke, OvernightLongRunDuration, startAtRandomTime, LongRunRepeats); }

		#endregion
	}

}
