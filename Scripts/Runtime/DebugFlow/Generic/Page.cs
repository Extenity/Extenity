using System.Collections.Generic;
using System.IO;

namespace Extenity.DebugFlowTool.Generic
{

	public class Page
	{
		#region Initialization

		public Page(int id, string pageName)
		{
			ID = id;
			PageName = pageName;
			TimedChartGroups = new List<TimedChartGroup>(10);
		}

		#endregion

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

		#region Network Serialization

		public void SendToNetwork(BinaryWriter destination)
		{
			var packet = PacketBuilder.Create();
			packet.Writer.Write(ID);
			packet.Writer.Write(PageName);
			PacketBuilder.Finalize(PacketType.CreatePage, ref packet, destination);
		}

		public static Page ReceiveFromNetwork(BinaryReader source)
		{
			return new Page(
				source.ReadInt32(),
				source.ReadString()
			);
		}

		#endregion
	}

}
