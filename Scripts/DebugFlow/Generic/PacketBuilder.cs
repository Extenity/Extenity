using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Extenity.DebugFlowTool.Generic
{

	public class PacketBuilder
	{
		#region Configuration

		public const int MaxPacketSize = ushort.MaxValue;
		public static readonly Encoding PacketEncoding = Encoding.UTF8;

		#endregion

		#region Initialization

		/// <summary>
		/// Constructor is private because it is not allowed to 
		/// be created by hand. Use Create() and Reserve() instead.
		/// </summary>
		private PacketBuilder()
		{
			Buffer = new MemoryStream();
			Writer = new BinaryWriter(Buffer, PacketEncoding);
		}

		public void Clear()
		{
			Buffer.Position = 0;
		}

		#endregion

		#region Memory

		private MemoryStream Buffer;
		public BinaryWriter Writer { get; private set; }

		#endregion

		#region Start / End Build

		private BinaryWriter StartBuild(PacketType packetType)
		{
			Clear();
			Writer.Write((byte)packetType);
			return Writer;
		}

		private void EndBuild(BinaryWriter destination)
		{
			var packetSize = (int)Buffer.Position;
			if (packetSize > MaxPacketSize)
			{
				throw new Exception("Network package sizes larger than '" + MaxPacketSize + "' are not supported.");
			}
			destination.Write((ushort)packetSize);
			destination.Write(Buffer.GetBuffer(), 0, packetSize);
		}

		#endregion

		#region Pooling

		private static List<PacketBuilder> PooledPacketBuilders = new List<PacketBuilder>(10);

		public static PacketBuilder Create(PacketType packetType)
		{
			PacketBuilder packetBuilder;

			if (PooledPacketBuilders.Count > 0)
			{
				var lastIndex = PooledPacketBuilders.Count - 1;
				packetBuilder = PooledPacketBuilders[lastIndex];
				PooledPacketBuilders.RemoveAt(lastIndex);
			}
			else
			{
				packetBuilder = new PacketBuilder();
			}

			packetBuilder.StartBuild(packetType);
			return packetBuilder;
		}

		public static void Finalize(ref PacketBuilder packetBuilder, BinaryWriter destination)
		{
			packetBuilder.EndBuild(destination);
			packetBuilder.Clear();
			PooledPacketBuilders.Add(packetBuilder);
			packetBuilder = null;
		}

		#endregion
	}

}
