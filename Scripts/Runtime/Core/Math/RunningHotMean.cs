using System;
using Extenity.DataToolbox;

namespace Extenity.MathToolbox
{

	public class RunningHotMeanDouble
	{
		/// <summary>
		/// CAUTION! Use it as readonly, do not modify. Use 'Push' and 'Clear' instead.
		/// </summary>
		public readonly CircularArray<double> Values;
		public int ValueCount => Values.Count;

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
		public readonly CircularArray<float> Values;
		public int ValueCount => Values.Count;

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

	public class RunningHotMeanInt
	{
		/// <summary>
		/// CAUTION! Use it as readonly, do not modify. Use 'Push' and 'Clear' instead.
		/// </summary>
		public readonly CircularArray<Int32> Values;
		public int ValueCount => Values.Count;

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
			Int32 total = 0;
			Values.ForEach(item => total += item);
			return (float)((double)total / (double)valueCount);
		}

		public RunningHotMeanInt(int size)
		{
			Values = new CircularArray<Int32>(size);
			IsInvalidated = false;
			_Mean = 0;
		}

		public void Clear()
		{
			Values.Clear();
			IsInvalidated = false;
			_Mean = 0;
		}

		public void Push(Int32 value)
		{
			IsInvalidated = true;
			Values.Add(value);
		}
	}

}
