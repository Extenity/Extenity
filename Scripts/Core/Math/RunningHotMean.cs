using Extenity.DataToolbox;

namespace Extenity.MathToolbox
{

	public class RunningHotMean
	{
		private CircularArray<double> Values;
		public int ValueCount { get { return Values.Count; } }

		private bool IsInvalidated;

		private double _Mean;
		public double Mean
		{
			get
			{
				if (IsInvalidated)
				{
					_Mean = CalculateMean();
				}
				return _Mean;
			}
		}

		private double CalculateMean()
		{
			var valueCount = Values.Count;
			if (valueCount == 0)
				return 0;
			double total = 0;
			Values.ForEach(item => total += item);
			return total / valueCount;
		}

		public RunningHotMean(int size)
		{
			Values = new CircularArray<double>(size);
			IsInvalidated = false;
			_Mean = 0;
		}

		public void Clear()
		{
			Values.Clear();
			IsInvalidated = false;
			_Mean = 0;
		}

		public void Push(double value)
		{
			IsInvalidated = true;
			Values.Add(value);
		}
	}

}
