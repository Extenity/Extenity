using System.Collections;
using ExtenityTests.Common;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace ExtenityTests.FlowToolbox
{

	public class Test_UnityInvoke : TestBase_FastInvoke
	{
		#region Timing

		// Zero
		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator Timing_1_Zero(bool startAtRandomTime) { yield return TestInvoke_Zero(DoUnityInvoke, startAtRandomTime, CheesyRepeats); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(int.MaxValue), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator Timing_1_Zero_Detailed(bool startAtRandomTime) { yield return TestInvoke_Zero(DoUnityInvoke, startAtRandomTime, DetailedRepeats); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(int.MaxValue), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator Timing_1_Zero_Overnight(bool startAtRandomTime) { yield return TestInvoke_Zero(DoUnityInvoke, startAtRandomTime, OvernightRepeats); }

		// Various
		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator Timing_2_Various(bool startAtRandomTime) { yield return TestInvoke_Various(DoUnityInvoke, startAtRandomTime, CheesyRepeats); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(int.MaxValue), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator Timing_2_Various_Detailed(bool startAtRandomTime) { yield return TestInvoke_Various(DoUnityInvoke, startAtRandomTime, DetailedRepeats); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(int.MaxValue), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator Timing_2_Various_Overnight(bool startAtRandomTime) { yield return TestInvoke_Various(DoUnityInvoke, startAtRandomTime, OvernightRepeats); }

		// LongRun
		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator Timing_3_LongRun(bool startAtRandomTime) { yield return TestInvoke(DoUnityInvoke, CheesyLongRunDuration, startAtRandomTime, LongRunRepeats); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(int.MaxValue), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator Timing_3_LongRun_Detailed(bool startAtRandomTime) { yield return TestInvoke(DoUnityInvoke, DetailedLongRunDuration, startAtRandomTime, LongRunRepeats); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(int.MaxValue), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator Timing_3_LongRun_Overnight(bool startAtRandomTime) { yield return TestInvoke(DoUnityInvoke, OvernightLongRunDuration, startAtRandomTime, LongRunRepeats); }

		#endregion
	}

}
