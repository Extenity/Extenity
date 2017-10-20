using System;
using System.IO;
using Extenity.DataToolbox;

namespace Extenity.DebugFlowTool.Generic
{

	public struct TimedChartEntry
	{
		public const int PacketSize = sizeof(UInt64) + sizeof(float);

		public readonly DateTime X;
		public readonly float Y;

		public TimedChartEntry(DateTime x, float y)
		{
			X = x;
			Y = y;
		}

		#region Network Serialization

		public void SendToNetwork(BinaryWriter destination)
		{
			var packet = PacketBuilder.Create();
			packet.Writer.Write(X.ToBinary());
			packet.Writer.Write(Y);
			PacketBuilder.Finalize(PacketType.AddTimedChartEntry, ref packet, destination);
		}

		public static TimedChartEntry ReceiveFromNetwork(BinaryReader source)
		{
			return new TimedChartEntry(
				DateTime.FromBinary(source.ReadInt64()),
				source.ReadSingle()
			);
		}

		#endregion
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

		#region Network Serialization

		public void SendToNetwork(BinaryWriter destination)
		{
			var packet = PacketBuilder.Create();
			packet.Writer.Write(ID);
			packet.Writer.Write(ChartName);
			packet.Writer.Write(Color.r);
			packet.Writer.Write(Color.g);
			packet.Writer.Write(Color.b);
			PacketBuilder.Finalize(PacketType.CreateTimedChart, ref packet, destination);
		}

		public static TimedChart ReceiveFromNetwork(BinaryReader source)
		{
			return new TimedChart(
				source.ReadInt32(),
				source.ReadString(),
				new Color(
					source.ReadByte(),
					source.ReadByte(),
					source.ReadByte()
				)
			);
		}

		#endregion
	}

}
