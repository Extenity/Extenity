using System.Collections.Generic;

namespace Extenity.DebugFlowTool.Generic
{

	public class TimedChartGroup
	{
		#region Initialization

		public TimedChartGroup(int id, string groupName)
		{
			ID = id;
			GroupName = groupName;
			TimedCharts = new List<TimedChart>(4);
		}

		#endregion

		#region Metadata

		public readonly int ID;
		public string GroupName;

		#endregion

		#region Data - TimedCharts

		public List<TimedChart> TimedCharts;

		public TimedChart GetTimedChart(string chartName)
		{
			for (var i = 0; i < TimedCharts.Count; i++)
			{
				if (TimedCharts[i].ChartName == chartName)
					return TimedCharts[i];
			}
			return null;
		}

		#endregion
	}

}
