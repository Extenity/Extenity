
namespace Extenity.MathToolbox
{

	public class RunningMean
	{
		private double PreviousMean;
		public double Mean { get; private set; }
		public int ValueCount { get; private set; }

		public void Clear()
		{
			ValueCount = 0;
			Mean = 0.0;
			PreviousMean = 0.0;
		}

		public void Push(double value)
		{
			ValueCount++;

			if (ValueCount == 1)
			{
				PreviousMean = Mean = value;
			}
			else
			{
				Mean = PreviousMean + (value - PreviousMean) / ValueCount;
				PreviousMean = Mean;
			}
		}
	}

}
