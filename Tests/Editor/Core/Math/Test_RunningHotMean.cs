using Extenity.MathToolbox;
using Extenity.Testing;
using NUnit.Framework;

namespace ExtenityTests.MathToolbox
{

	public class Test_RunningHotMean : ExtenityTestBase
	{
		[Test]
		public void Mean()
		{
			var mean = new RunningHotMeanDouble(10);
			Assert.True(mean.Mean == 0.0);
			Assert.True(mean.ValueCount == 0);
			mean.Push(10.0);
			Assert.True(mean.Mean == 10.0);
			Assert.True(mean.ValueCount == 1);
			mean.Push(20.0);
			Assert.True(mean.Mean == 15.0);
			Assert.True(mean.ValueCount == 2);
			mean.Push(30.0);
			Assert.True(mean.Mean == 20.0);
			Assert.True(mean.ValueCount == 3);
			mean.Push(20.0);
			Assert.True(mean.Mean == 20.0);
			Assert.True(mean.ValueCount == 4);
			mean.Push(120.0);
			Assert.True(mean.Mean == 40.0);
			Assert.True(mean.ValueCount == 5);
			mean.Push(40.0);
			Assert.True(mean.Mean == 40.0);
			Assert.True(mean.ValueCount == 6);
			mean.Push(40.0);
			Assert.True(mean.Mean == 40.0);
			Assert.True(mean.ValueCount == 7);
			mean.Push(0.0);
			Assert.True(mean.Mean == 35.0);
			Assert.True(mean.ValueCount == 8);
		}

		[Test]
		public void HotMean()
		{
			var mean = new RunningHotMeanDouble(2);
			mean.Push(10.0);
			Assert.True(mean.Mean == 10.0);
			Assert.True(mean.ValueCount == 1);
			mean.Push(20.0);
			Assert.True(mean.Mean == 15.0);
			Assert.True(mean.ValueCount == 2);
			mean.Push(30.0);
			Assert.True(mean.Mean == 25.0);
			Assert.True(mean.ValueCount == 2);
			mean.Push(50.0);
			Assert.True(mean.Mean == 40.0);
			Assert.True(mean.ValueCount == 2);
			mean.Push(100.0);
			Assert.True(mean.Mean == 75.0);
			Assert.True(mean.ValueCount == 2);
			mean.Push(0.0);
			Assert.True(mean.Mean == 50.0);
			Assert.True(mean.ValueCount == 2);
			mean.Push(0.0);
			Assert.True(mean.Mean == 0.0);
			Assert.True(mean.ValueCount == 2);
		}

		[Test]
		public void Clear()
		{
			var mean = new RunningHotMeanDouble(2);
			mean.Push(10.0);
			Assert.True(mean.Mean == 10.0);
			Assert.True(mean.ValueCount == 1);
			mean.Push(20.0);
			Assert.True(mean.Mean == 15.0);
			Assert.True(mean.ValueCount == 2);
			mean.Push(30.0);
			Assert.True(mean.Mean == 25.0);
			Assert.True(mean.ValueCount == 2);
			mean.Clear();
			Assert.True(mean.Mean == 0.0);
			Assert.True(mean.ValueCount == 0);
			mean.Push(10.0);
			Assert.True(mean.Mean == 10.0);
			Assert.True(mean.ValueCount == 1);
			mean.Push(20.0);
			Assert.True(mean.Mean == 15.0);
			Assert.True(mean.ValueCount == 2);
			mean.Push(30.0);
			Assert.True(mean.Mean == 25.0);
			Assert.True(mean.ValueCount == 2);
		}
	}

}
