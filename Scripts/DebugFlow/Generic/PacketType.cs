using System;

namespace Extenity.DebugFlowTool.Generic
{

	public enum PacketType : byte
	{
		Unspecified = 0,
		CreatePage = 5,
		CreateTimedChartGroup = 6,
		CreateTimedChart = 7,
		AddTimedChartEntry = 15,
	}

	public static class PacketSpecifications
	{
		public static bool IsVariableSize(this PacketType packetType)
		{
			switch (packetType)
			{
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
				case PacketType.CreatePage:
				case PacketType.CreateTimedChartGroup:
				case PacketType.CreateTimedChart:
					throw new Exception("Queried the size of a variable length package of type '" + packetType.ToString() + "'.");

				case PacketType.AddTimedChartEntry: return TimedChartEntry.PacketSize;

				case PacketType.Unspecified:
				default:
					throw new ArgumentOutOfRangeException("packetType", packetType, null);
			}
		}
	}

}
