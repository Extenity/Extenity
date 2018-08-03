using System;

namespace Extenity.MathToolbox
{

	/// <summary>
	/// https://www.johndcook.com/blog/standard_deviation/
	/// </summary>
	public class RunningStandardDeviation
	{
		private double _PreviousMean;
		private double _Mean;
		private double _PreviousS;
		private double _S;

		private int _ValueCount;
		public int ValueCount { get { return _ValueCount; } }

		public RunningStandardDeviation()
		{
			_ValueCount = 0;
		}

		public void Clear()
		{
			_ValueCount = 0;
		}

		public void Push(double value)
		{
			_ValueCount++;

			// See Knuth TAOCP vol 2, 3rd edition, page 232
			if (_ValueCount == 1)
			{
				_PreviousMean = _Mean = value;
				_PreviousS = 0.0;
			}
			else
			{
				_Mean = _PreviousMean + (value - _PreviousMean) / _ValueCount;
				_S = _PreviousS + (value - _PreviousMean) * (value - _Mean);

				// set up for next iteration
				_PreviousMean = _Mean;
				_PreviousS = _S;
			}
		}

		public double Mean
		{
			get
			{
				return _ValueCount > 0
						? _Mean
						: 0.0;
			}
		}

		public double Variance
		{
			get
			{
				return _ValueCount > 1
						? _S / _ValueCount
						: 0.0;
			}
		}

		public double StandardDeviation
		{
			get { return Math.Sqrt(Variance); }
		}
	}

}
