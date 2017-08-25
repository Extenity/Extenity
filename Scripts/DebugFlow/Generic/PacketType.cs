using System;

namespace Extenity.DebugFlowTool.Generic
{

	public enum PacketType : byte
	{
		Unspecified = 0,

		SessionRequest = 1,
		SessionAcknowledge = 2,
		SessionDeny = 3,

		CreatePage = 11,

		CreateTimedChartGroup = 21,

		CreateTimedChart = 31,
		AddTimedChartEntry = 32,
	}

	public static class PacketSpecifications
	{
		public static bool IsVariableSize(this PacketType packetType)
		{
			switch (packetType)
			{
				case PacketType.SessionRequest:
					return false;

				case PacketType.SessionAcknowledge:
				case PacketType.SessionDeny:
				case PacketType.CreatePage:
				case PacketType.CreateTimedChartGroup:
				case PacketType.CreateTimedChart:
					return true;

				case PacketType.AddTimedChartEntry:
					return false;

				case PacketType.Unspecified:
				default:
					throw new ArgumentOutOfRangeException("packetType", packetType, null);
			}
		}

		public static int FixedPacketSize(this PacketType packetType)
		{
			switch (packetType)
			{
				case PacketType.SessionAcknowledge:
				case PacketType.SessionDeny:
				case PacketType.CreatePage:
				case PacketType.CreateTimedChartGroup:
				case PacketType.CreateTimedChart:
					throw new Exception("Queried the size of a variable length package of type '" + packetType.ToString() + "'.");

				case PacketType.SessionRequest: return Handshake.SessionRequestPacketSize;

				case PacketType.AddTimedChartEntry: return TimedChartEntry.PacketSize;

				case PacketType.Unspecified:
				default:
					throw new ArgumentOutOfRangeException("packetType", packetType, null);
			}
		}
	}

}
