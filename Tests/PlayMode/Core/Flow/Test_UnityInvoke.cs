using System.Collections;
using ExtenityTests.Common;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace ExtenityTests.FlowToolbox
{

	public class Test_UnityInvoke : TestBase_FastInvoke
	{
		#region Timing - Unity Invoke

		// UnityInvoke_Zero
		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator UnityInvoke_Zero(bool startAtRandomTime) { yield return TestInvoke_Zero(DoUnityInvoke, startAtRandomTime, CheesyRepeats); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(int.MaxValue), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator UnityInvoke_Zero_Detailed(bool startAtRandomTime) { yield return TestInvoke_Zero(DoUnityInvoke, startAtRandomTime, DetailedRepeats); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(int.MaxValue), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator UnityInvoke_Zero_Overnight(bool startAtRandomTime) { yield return TestInvoke_Zero(DoUnityInvoke, startAtRandomTime, OvernightRepeats); }

		// UnityInvoke_Various
		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator UnityInvoke_Various(bool startAtRandomTime) { yield return TestInvoke_Various(DoUnityInvoke, startAtRandomTime, CheesyRepeats); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(int.MaxValue), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator UnityInvoke_Various_Detailed(bool startAtRandomTime) { yield return TestInvoke_Various(DoUnityInvoke, startAtRandomTime, DetailedRepeats); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(int.MaxValue), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator UnityInvoke_Various_Overnight(bool startAtRandomTime) { yield return TestInvoke_Various(DoUnityInvoke, startAtRandomTime, OvernightRepeats); }

		// UnityInvoke_LongRun
		[UnityTest, Category(TestCategories.Cheesy), Timeout(int.MaxValue), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator UnityInvoke_LongRun(bool startAtRandomTime) { yield return TestInvoke(DoUnityInvoke, CheesyLongRunDuration, startAtRandomTime, LongRunRepeats); }
		[UnityTest, Category(TestCategories.Detailed), Timeout(int.MaxValue), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator UnityInvoke_LongRun_Detailed(bool startAtRandomTime) { yield return TestInvoke(DoUnityInvoke, DetailedLongRunDuration, startAtRandomTime, LongRunRepeats); }
		[UnityTest, Category(TestCategories.Overnight), Timeout(int.MaxValue), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator UnityInvoke_LongRun_Overnight(bool startAtRandomTime) { yield return TestInvoke(DoUnityInvoke, OvernightLongRunDuration, startAtRandomTime, LongRunRepeats); }

		#endregion
	}

}
