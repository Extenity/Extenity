using System.Collections.Generic;
using System.Linq;
using Extenity.MathToolbox;
using NUnit.Framework;
using UnityEngine;

namespace ExtenityTests.MathToolbox
{

	public class Test_UnityRandomTools : AssertionHelper
	{
		#region Int Float Double

		[Test]
		public void Test_GenerateTimestampedSeed()
		{
			const int repeatCount = 1000;
			const int generationCount = 10000;

			var bag = new HashSet<int>();
			var completionRatiosAtFailTime = new List<float>(repeatCount);
			//Debug.Log("_________________________________________________________________________");

			for (int iRepeat = 0; iRepeat < repeatCount; iRepeat++)
			{
				bag.Clear();

				for (int i = 0; i < generationCount; i++)
				{
					var seed = UnityRandomTools.GenerateTimestampedSeed();
					if (!bag.Add(seed))
					{
						// Failed to generate a unique seed.
						break;
						//Assert.Fail($"GenerateTimestampedSeed failed at '{bag.Count}' tries to generate unique seeds in rapid consecutive calls.");
					}
				}

				var ratio = (float)bag.Count / generationCount;
				completionRatiosAtFailTime.Add(ratio);
				//Debug.Log(ratio.ToStringAsPercentageBar());
			}

			var averageCompletionRatioAtFailTime = completionRatiosAtFailTime.Average();
			//Debug.Log($"Rapid seed generation tests completed among '{generationCount:N0}' seeds with an average of {averageCompletionRatioAtFailTime:P} success ratio.");

			// We count how many seeds generated before a collision occured. Then we divide it by the
			// total seeds that we were going to generate. This gives "Completion ratio at fail time".
			// Then we calculate the average of completion ratios among "repeatCount" times of repeats.
			// This gives a rough percentage of how seed generation algorithm is likely to succeed
			// when it is given a task to generate rapid consecutive seeds "generationCount" times.
			//
			// When the algorithm tried to generate 10.000 consecutive seeds, it performed 99.25%
			// average completion rate. In real world, there would be none, or only rare use cases that
			// an application would want to reset RNG generator 10.000 times in fraction of a second.
			// So we may assume this ratio is more than enough.
			//
			// Heck, even 10 times without a collision in a whole second would be more than enough.
			Assert.Greater(averageCompletionRatioAtFailTime, 0.95f);
		}

		#endregion
	}

}
