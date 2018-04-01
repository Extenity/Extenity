using Extenity.DataToolbox;

namespace Extenity.MathToolbox
{

	public class RunningHotMeanDouble
	{
		/// <summary>
		/// CAUTION! Use it as readonly, do not modify. Use 'Push' and 'Clear' instead.
		/// </summary>
		public CircularArray<double> Values;
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

		public RunningHotMeanDouble(int size)
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

	public class RunningHotMeanFloat
	{
		/// <summary>
		/// CAUTION! Use it as readonly, do not modify. Use 'Push' and 'Clear' instead.
		/// </summary>
		public CircularArray<float> Values;
		public int ValueCount { get { return Values.Count; } }

		private bool IsInvalidated;

		private float _Mean;
		public float Mean
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

		private float CalculateMean()
		{
			var valueCount = Values.Count;
			if (valueCount == 0)
				return 0;
			float total = 0;
			Values.ForEach(item => total += item);
			return total / valueCount;
		}

		public RunningHotMeanFloat(int size)
		{
			Values = new CircularArray<float>(size);
			IsInvalidated = false;
			_Mean = 0;
		}

		public void Clear()
		{
			Values.Clear();
			IsInvalidated = false;
			_Mean = 0;
		}

		public void Push(float value)
		{
			IsInvalidated = true;
			Values.Add(value);
		}
	}

}
