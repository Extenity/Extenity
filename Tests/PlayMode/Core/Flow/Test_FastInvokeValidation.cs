using System.Collections;
using System.Globalization;
using System.Linq;
using Extenity.DataToolbox;
using Extenity.FlowToolbox;
using Extenity.MathToolbox;
using Extenity.ParallelToolbox;
using Extenity.Testing;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ExtenityTests.FlowToolbox
{

	public class Test_FastInvokeValidation : TestBase_FastInvoke
	{
		#region Test System Validation

		[UnityTest, Category(TestCategories.Cheesy), TestCase(true, ExpectedResult = null), TestCase(false, ExpectedResult = null)]
		public IEnumerator TestSystemValidation(bool startAtRandomTime)
		{
			yield return InitializeTest(startAtRandomTime);

			// No invoke or fixed update processed yet. They are all zeros.
			Assert.AreEqual(0, Subject.CallbackCallCount);
			Assert.AreEqual(0, Subject.FixedUpdateCallCount);
			Assert.IsFalse(Invoker.IsFastInvokingAny());
			Assert.AreEqual(0, Invoker.TotalActiveFastInvokeCount());

			// Make sure fixed updates are called in expected delta times
			{
				// We need to skip to the first fixed update, since Time.time could be anything random.
				if (startAtRandomTime)
				{
					yield return Yields.WaitForFixedUpdate;
				}

				var previous = (double)Time.time;

				var diffHistory = New.List<double>();

				for (int i = 0; i < 20; i++)
				{
					yield return Yields.WaitForFixedUpdate;
					var now = (double)Time.time;
					var fixedDeltaTime = (double)Time.fixedDeltaTime;
					var tolerance = fixedDeltaTime * 0.001;
					var diff = now - previous;
					diffHistory.Add(diff);
					if (!diff.IsAlmostEqual(fixedDeltaTime, tolerance))
					{
						Assert.Fail($"Failed at {i}. iteration. Details:\n" +
						            $"FixedDeltaTime: {fixedDeltaTime}\n" +
						            $"Tolerance: {tolerance}\n" +
						            $"Diff: {diff}\n" +
						            $"Max: {fixedDeltaTime + tolerance}\n" +
						            $"Min: {fixedDeltaTime - tolerance}\n" +
						            "Diff history:\n" + string.Join("\n", diffHistory.Select(x => x.ToString(CultureInfo.InvariantCulture))));
					}
					previous = now;
				}
			}
		}

		#endregion
	}

}
