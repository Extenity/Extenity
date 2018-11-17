using System.Collections.Generic;
using System.Linq;
using Extenity.MathToolbox;
using NUnit.Framework;

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
			//Log.Info("_________________________________________________________________________");

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
				//Log.Info(ratio.ToStringAsPercentageBar());
			}

			var averageCompletionRatioAtFailTime = completionRatiosAtFailTime.Average();
			//Log.Info($"Rapid seed generation tests completed among '{generationCount:N0}' seeds with an average of {averageCompletionRatioAtFailTime:P} success ratio.");

			// We count how many seeds generated before a collision occured. Then we divide it by the
			// total seeds that we were going to generate. This gives "Completion ratio at fail time".
			// Then we calculate the average of completion ratios among "repeatCount" times of repeats.
			// This gives a rough percentage of how seed generation algorithm is likely to succeed
			// when it is given a task to generate rapid consecutive seeds "generationCount" times.
			//
			// See UnityRandomTools.GenerateTimestampedSeed for detailed test results.
			Assert.Greater(averageCompletionRatioAtFailTime, 0.95f);
		}

		#endregion
	}

}
