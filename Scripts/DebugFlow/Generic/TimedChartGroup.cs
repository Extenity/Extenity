using System.Collections.Generic;
using System.IO;

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

		#region Network Serialization

		public void SendToNetwork(BinaryWriter destination)
		{
			var packet = PacketBuilder.Create();
			packet.Writer.Write(ID);
			packet.Writer.Write(GroupName);
			PacketBuilder.Finalize(PacketType.CreateTimedChartGroup, ref packet, destination);
		}

		public static TimedChartGroup ReceiveFromNetwork(BinaryReader source)
		{
			return new TimedChartGroup(
				source.ReadInt32(),
				source.ReadString()
			);
		}

		#endregion
	}

}
