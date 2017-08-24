using System.Collections.Generic;

namespace Extenity.DebugFlowTool.Generic
{

	public class Page
	{
		#region Metadata

		public readonly int ID;
		public string PageName;

		#endregion

		#region Data - TimedChartGroups

		public List<TimedChartGroup> TimedChartGroups;

		public TimedChartGroup GetTimedChartGroup(string groupName)
		{
			for (var i = 0; i < TimedChartGroups.Count; i++)
			{
				if (TimedChartGroups[i].GroupName == groupName)
					return TimedChartGroups[i];
			}
			return null;
		}

		#endregion

		#region Initialization

		public Page(int id, string pageName)
		{
			ID = id;
			PageName = pageName;
			TimedChartGroups = new List<TimedChartGroup>(10);
		}

		#endregion
	}

}
