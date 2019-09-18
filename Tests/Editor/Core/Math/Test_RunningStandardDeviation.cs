using Extenity.MathToolbox;
using Extenity.Testing;
using NUnit.Framework;

namespace ExtenityTests.MathToolbox
{

	public class Test_RunningStandardDeviation : ExtenityTestBase
	{
		[Test]
		public void StandardDeviation()
		{
			var mean = new RunningStandardDeviation();
			Assert.True(mean.Mean == 0.0);
			Assert.True(mean.ValueCount == 0);
			Assert.True(mean.StandardDeviation == 0.0);
			Assert.True(mean.Variance == 0.0);
			mean.Push(-5.0);
			Assert.True(mean.Mean == -5.0);
			Assert.True(mean.ValueCount == 1);
			Assert.True(mean.StandardDeviation == 0.0);
			Assert.True(mean.Variance == 0.0);
			mean.Push(1.0);
			Assert.True(mean.Mean == -2.0);
			Assert.True(mean.ValueCount == 2);
			Assert.True(mean.StandardDeviation.IsAlmostEqual(3.0, 0.0000001), mean.StandardDeviation.ToString());
			Assert.True(mean.Variance == 9.0);
			mean.Push(4.0);
			Assert.True(mean.Mean == 0.0);
			Assert.True(mean.ValueCount == 3);
			Assert.True(mean.StandardDeviation.IsAlmostEqual(3.741657387, 0.0000001));
			Assert.True(mean.Variance == 14.0);
		}

		[Test]
		public void Clear()
		{
			var mean = new RunningStandardDeviation();
			mean.Push(-5.0);
			mean.Push(1.0);
			mean.Push(10.0);
			mean.Push(-4.0);
			mean.Clear();
			Assert.True(mean.Mean == 0.0);
			Assert.True(mean.ValueCount == 0);
			Assert.True(mean.StandardDeviation == 0.0);
			Assert.True(mean.Variance == 0.0);
		}
	}

}
