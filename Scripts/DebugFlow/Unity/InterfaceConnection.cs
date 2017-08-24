using System;
using System.IO;
using System.Net.Sockets;
using Extenity.DebugFlowTool.Generic;

namespace Extenity.DebugFlowTool.Unity
{

	public class InterfaceConnection
	{
		#region Initialization

		public InterfaceConnection(TcpClient tcpClient)
		{
			//TCPClient = tcpClient;
			Stream = tcpClient.GetStream();
			NetworkReader = new BinaryReader(Stream, PacketBuilder.PacketEncoding);
			NetworkWriter = new BinaryWriter(Stream, PacketBuilder.PacketEncoding);

			SendInitialData();
		}

		#endregion

		#region TCPClient

		//private TcpClient TCPClient;
		public NetworkStream Stream { get; private set; }
		public BinaryReader NetworkReader { get; private set; }
		public BinaryWriter NetworkWriter { get; private set; }

		#endregion

		#region Receive Data

		internal void ReceiveDataAndHandleDisconnection()
		{
			// TODO:
			throw new NotImplementedException();
		}

		#endregion

		#region Send Data - Create Widgets

		internal void InformCreatePage(Page page)
		{
			lock (Stream)
			{
				var packet = PacketBuilder.Create(PacketType.CreatePage);
				packet.Writer.Write(page.ID);
				packet.Writer.Write(page.PageName);
				PacketBuilder.Finalize(ref packet, NetworkWriter);
			}
		}

		internal void InformCreateTimedChartGroup(TimedChartGroup group)
		{
			lock (Stream)
			{
				var packet = PacketBuilder.Create(PacketType.CreateTimedChartGroup);
				packet.Writer.Write(group.ID);
				packet.Writer.Write(group.GroupName);
				PacketBuilder.Finalize(ref packet, NetworkWriter);
			}
		}

		internal void InformCreateTimedChart(TimedChart chart)
		{
			lock (Stream)
			{
				var packet = PacketBuilder.Create(PacketType.CreateTimedChart);
				packet.Writer.Write(chart.ID);
				packet.Writer.Write(chart.ChartName);
				PacketBuilder.Finalize(ref packet, NetworkWriter);
			}
		}

		#endregion

		#region Send Data - Values

		// TODO:

		#endregion

		#region Send Data - Initial Data

		private void SendInitialData()
		{
			// Pages
			foreach (var page in DebugFlow.Pages)
			{
				InformCreatePage(page);

				// TimedChartGroups
				foreach (var timedChartGroup in page.TimedChartGroups)
				{
					InformCreateTimedChartGroup(timedChartGroup);

					// TimedCharts
					foreach (var timedChart in timedChartGroup.TimedCharts)
					{
						InformCreateTimedChart(timedChart);
					}
				}
			}
		}

		#endregion
	}

}
