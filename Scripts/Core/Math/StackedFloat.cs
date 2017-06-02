using System.Collections.Generic;
using UnityEngine;

namespace Extenity.MathToolbox
{

	public class StackedFloat
	{
		public class StackedValue
		{
			public float value;
			public float endTime;
		}

		private List<StackedValue> values = new List<StackedValue>();

		public StackedValue AddValue(float value, float duration = 0f)
		{
			var newValue = new StackedValue
			{
				value = value,
				endTime = Time.time + duration,
			};
			values.Add(newValue);
			return newValue;
		}

		internal void Update()
		{
			if (values == null)
				return;

			var now = Time.time;

			for (int i = values.Count - 1; i >= 0; i--)
			{
				var value = values[i];

				// Remove if duration passed
				if (value.endTime > 0f && value.endTime <= now)
				{
					values.RemoveAt(i);
				}
			}
		}

		public float Value
		{
			get
			{
				if (values == null)
					return 0;

				float totalValue = 0;
				for (int i = 0; i < values.Count; i++)
				{
					totalValue += values[i].value;
				}
				return totalValue;
			}
		}

		public int ValueCount
		{
			get
			{
				return values == null ? 0 : values.Count;
			}
		}
	}

}
