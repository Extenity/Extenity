using System;

namespace Extenity.DataToolbox
{

	[Serializable]
	public class MinMaxRange
	{
		public float Min;
		public float Max;

		public static readonly MinMaxRange Invalid = new MinMaxRange(float.PositiveInfinity, float.NegativeInfinity);
		public static readonly MinMaxRange Zero = new MinMaxRange(0f, 0f);
		public static readonly MinMaxRange Identity = new MinMaxRange(0f, 1f);

		public MinMaxRange()
		{
			Min = float.PositiveInfinity;
			Max = float.NegativeInfinity;
		}

		public MinMaxRange(float min, float max)
		{
			Min = min;
			Max = max;
		}

		public bool IsValid()
		{
			return Min <= Max;
		}

		public bool IsInside(float value)
		{
			return value >= Min && value <= Max;
		}

		public bool IsLower(float value)
		{
			return value < Min;
		}

		public bool IsHigher(float value)
		{
			return value > Max;
		}

		public int GetPosition(float value)
		{
			if (value < Min)
				return -1;
			if (value > Max)
				return 1;
			return 0;
		}

		public float GetDistance(float value)
		{
			if (value < Min)
				return value - Min;
			if (value > Max)
				return value - Max;
			return 0f;
		}
	}

}
