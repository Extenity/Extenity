using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
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
			Reader = new BinaryReader(Stream, Encoding.UTF8);
			Writer = new BinaryWriter(Stream, Encoding.UTF8);

			SendInitialData();
		}

		#endregion

		#region TCPClient

		//private TcpClient TCPClient;
		public NetworkStream Stream { get; private set; }
		public BinaryReader Reader { get; private set; }
		public BinaryWriter Writer { get; private set; }

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
				Writer.Write((byte)Packets.CreatePage);
				Writer.Write(page.ID);
				Writer.Write(page.PageName);
			}
		}

		internal void InformCreateTimedChartGroup(TimedChartGroup group)
		{
			lock (Stream)
			{
				Writer.Write((byte)Packets.CreateTimedChartGroup);
				Writer.Write(group.ID);
				Writer.Write(group.GroupName);
			}
		}

		internal void InformCreateTimedChart(TimedChart chart)
		{
			lock (Stream)
			{
				Writer.Write((byte)Packets.CreateTimedChart);
				Writer.Write(chart.ID);
				Writer.Write(chart.ChartName);
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
