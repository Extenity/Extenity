using System.Collections.Generic;
using System.Text;

namespace Extenity.MathToolbox
{

	public class Histogram
	{
		#region Values

		public Dictionary<int, int> ValuesBySlices = new Dictionary<int, int>();

		public void Add(int slice, int addedValue)
		{
			if (ValuesBySlices.TryGetValue(slice, out int value))
			{
				ValuesBySlices[slice] = value + addedValue;
			}
			else
			{
				ValuesBySlices.Add(slice, addedValue);
			}
		}

		public int GetValue(int slice)
		{
			if (ValuesBySlices.TryGetValue(slice, out int value))
				return value;
			return default;
		}

		public int SumRange(int sliceRangeMin, int sliceRangeMax)
		{
			var sum = 0;
			foreach (var entry in ValuesBySlices)
			{
				if (entry.Key >= sliceRangeMin && entry.Key <= sliceRangeMax)
				{
					sum += entry.Value;
				}
			}
			return sum;
		}

		public void Clear()
		{
			ValuesBySlices.Clear();
		}

		#endregion

		#region Log

		public string GenerateTextInLines(int sliceRangeMin, int sliceRangeMax)
		{
			var sumBelow = SumRange(int.MinValue, sliceRangeMin - 1);
			var sumAbove = SumRange(sliceRangeMax + 1, int.MaxValue);

			var stringBuilder = new StringBuilder();
			lock (stringBuilder)
			{
				stringBuilder.Clear();
				stringBuilder.AppendLine("Below\t" + sumBelow.ToString("N0"));
				for (int i = sliceRangeMin; i <= sliceRangeMax; i++)
				{
					stringBuilder.AppendLine(i + "\t" + GetValue(i).ToString("N0"));
				}
				stringBuilder.AppendLine("Above\t" + sumAbove.ToString("N0"));
				return stringBuilder.ToString();
			}
		}

		#endregion
	}

}
