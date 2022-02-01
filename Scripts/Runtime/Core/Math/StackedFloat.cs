using System.Collections.Generic;

namespace Extenity.MathToolbox
{

	public class StackedFloat
	{
		public struct StackedValue
		{
			public float Value;
			public float EndTime;

			public StackedValue(float value, float endTime)
			{
				Value = value;
				EndTime = endTime;
			}
		}

		private List<StackedValue> Values = new List<StackedValue>();

		public void AddValue(float value, float endTime)
		{
			Values.Add(new StackedValue(value, endTime));
		}

		public void Update(float now)
		{
			for (int i = Values.Count - 1; i >= 0; i--)
			{
				// Remove if end time passed
				if (Values[i].EndTime <= now)
				{
					Values.RemoveAt(i);
				}
			}
		}

		public float TotalValue
		{
			get
			{
				float totalValue = 0;
				for (int i = 0; i < Values.Count; i++)
				{
					totalValue += Values[i].Value;
				}
				return totalValue;
			}
		}

		public int ValueCount
		{
			get
			{
				return Values.Count;
			}
		}
	}

}
