using System;
using Extenity.DataToolbox;
using Sirenix.OdinInspector;
using Unity.Mathematics;

namespace Extenity.MathToolbox
{

	public class RunningHotMeanDouble
	{
		/// <summary>
		/// CAUTION! Use it as readonly, do not modify. Use 'Push' and 'Clear' instead.
		/// </summary>
		public readonly CircularArray<double> Values;
		public int ValueCount => Values.Count;
		public int ValueCapacity => Values.Capacity;
		public bool IsCapacityFilled => Values.IsCapacityFilled;

		private bool IsInvalidated;

		private double _Mean;
		[ShowInInspector]
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
				return default;
			double total = default;
			Values.ForEach(item => total += item);
			return total / valueCount;
		}

		public RunningHotMeanDouble(int size)
		{
			Values = new CircularArray<double>(size);
			IsInvalidated = false;
			_Mean = default;
		}

		public void Clear()
		{
			Values.Clear();
			IsInvalidated = false;
			_Mean = default;
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
		public int ValueCapacity => Values.Capacity;
		public bool IsCapacityFilled => Values.IsCapacityFilled;

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
				return default;
			float total = default;
			Values.ForEach(item => total += item);
			return total / valueCount;
		}

		public RunningHotMeanFloat(int size)
		{
			Values = new CircularArray<float>(size);
			IsInvalidated = false;
			_Mean = default;
		}

		public void Clear()
		{
			Values.Clear();
			IsInvalidated = false;
			_Mean = default;
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
		public int ValueCapacity => Values.Capacity;
		public bool IsCapacityFilled => Values.IsCapacityFilled;

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
				return default;
			Int32 total = default;
			Values.ForEach(item => total += item);
			return (float)((double)total / (double)valueCount);
		}

		public RunningHotMeanInt(int size)
		{
			Values = new CircularArray<Int32>(size);
			IsInvalidated = false;
			_Mean = default;
		}

		public void Clear()
		{
			Values.Clear();
			IsInvalidated = false;
			_Mean = default;
		}

		public void Push(Int32 value)
		{
			IsInvalidated = true;
			Values.Add(value);
		}
	}

	public class RunningHotMeanFloat3
	{
		/// <summary>
		/// CAUTION! Use it as readonly, do not modify. Use 'Push' and 'Clear' instead.
		/// </summary>
		public readonly CircularArray<float3> Values;
		public int ValueCount => Values.Count;
		public int ValueCapacity => Values.Capacity;
		public bool IsCapacityFilled => Values.IsCapacityFilled;

		private bool IsInvalidated;

		private float3 _Mean;
		public float3 Mean
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

		private float3 CalculateMean()
		{
			var valueCount = Values.Count;
			if (valueCount == 0)
				return default;
			float3 total = default;
			Values.ForEach(item => total += item);
			return total / valueCount;
		}

		public RunningHotMeanFloat3(int size)
		{
			Values = new CircularArray<float3>(size);
			IsInvalidated = false;
			_Mean = default;
		}

		public void Clear()
		{
			Values.Clear();
			IsInvalidated = false;
			_Mean = default;
		}

		public void Push(float3 value)
		{
			IsInvalidated = true;
			Values.Add(value);
		}
	}

}
