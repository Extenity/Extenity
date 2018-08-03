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

		#region Send Data - Initial Data

		private void SendInitialData()
		{
			// Pages
			foreach (var page in DebugFlow.Pages)
			{
				page.SendToNetwork(NetworkWriter);

				// TimedChartGroups
				foreach (var timedChartGroup in page.TimedChartGroups)
				{
					timedChartGroup.SendToNetwork(NetworkWriter);

					// TimedCharts
					foreach (var timedChart in timedChartGroup.TimedCharts)
					{
						timedChart.SendToNetwork(NetworkWriter);
					}
				}
			}
		}

		#endregion
	}

}
