using System;
using Extenity.DataToolbox;

namespace Extenity.DebugFlowTool.Generic
{

	public class TimedChartEntry
	{
		public DateTime X { get; set; }
		public float Y { get; set; }
	}

	public class TimedChart
	{
		#region Initialization

		public TimedChart(int id, string chartName, Color color)
		{
			ID = id;
			ChartName = chartName;
			Color = color;

			InitializeData();
		}

		#endregion

		#region Metadata

		public readonly int ID;
		public string ChartName;
		public Color Color;

		#endregion

		#region Data

		public static readonly int Capacity = 100000;
		public CircularArray<TimedChartEntry> Entries;

		private void InitializeData()
		{
			Entries = new CircularArray<TimedChartEntry>(Capacity);
		}

		/// <summary>
		/// Adds the entry to the chart while keeping the data ordered.
		/// </summary>
		public void AddEntry(DateTime time, float value)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

}
