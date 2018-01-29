
namespace Extenity.MathToolbox
{

	public class RunningMean
	{
		private double _PreviousMean;
		private double _Mean;
		public double Mean { get { return _Mean; } }
		private int _ValueCount;
		public int ValueCount { get { return _ValueCount; } }

		public void Clear()
		{
			_ValueCount = 0;
			_Mean = 0.0;
			_PreviousMean = 0.0;
		}

		public void Push(double value)
		{
			_ValueCount++;

			if (_ValueCount == 1)
			{
				_PreviousMean = _Mean = value;
			}
			else
			{
				_Mean = _PreviousMean + (value - _PreviousMean) / _ValueCount;
				_PreviousMean = _Mean;
			}
		}
	}

}
