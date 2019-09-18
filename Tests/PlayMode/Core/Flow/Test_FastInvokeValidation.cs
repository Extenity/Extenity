using System.Collections;
using Extenity.FlowToolbox;
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
			Assert.AreEqual(0, Invoker.TotalFastInvokeCount());

			// Make sure fixed updates are called in expected delta times
			{
				// We need to skip to the first fixed update, since Time.time could be anything random.
				if (startAtRandomTime)
				{
					yield return new WaitForFixedUpdate();
				}

				var previous = Time.time;
				var fixedDeltaTime = Time.fixedDeltaTime;

				for (int i = 0; i < 20; i++)
				{
					yield return new WaitForFixedUpdate();
					var now = Time.time;
					var diff = now - previous;
					Assert.AreEqual(fixedDeltaTime, diff, FastInvokeHandler.Tolerance);
					previous = now;
				}

			}
		}

		#endregion
	}

}
