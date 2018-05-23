using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Extenity.DebugFlowTool.Generic
{

	/// <summary>
	/// Packet layout is:
	///		byte (1 byte): PacketType
	///		ushort (2 bytes): PacketSize (Only if the type is a variable size packet)
	///		byte[]: Packet content
	/// </summary>
	public class PacketBuilder
	{
		#region Configuration

		public const int MaxPacketSize = ushort.MaxValue;
		public static readonly Encoding PacketEncoding = Encoding.UTF8;

		/// <summary>
		/// A hard limit to prevent any memory damage caused by heavy networking operations.
		/// </summary>
		private const int PoolingMaxSize = 1000;

		public const int FixedSizePacketHeaderLength = 1; // PacketType: 1 (byte)
		public const int VariableSizePacketHeaderLength = 1 + 2; // PacketType: 1 (byte) + PacketSize: 2 (ushort)

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

		private void StartBuild()
		{
			Clear();
		}

		private void EndBuild(BinaryWriter destination, PacketType packetType)
		{
			var writtenSize = (int)Buffer.Position;
			var isVariableSize = packetType.IsVariableSize();
			int packetSize;

			if (isVariableSize)
			{
				packetSize = writtenSize;
				if (packetSize > MaxPacketSize)
				{
					throw new Exception($"Network packet of type '{packetType.ToString()}' exceeds the maximum packet size with '{packetSize}' which should be lesser than '{MaxPacketSize}'.");
				}
			}
			else
			{
				packetSize = packetType.FixedPacketSize();
				if (packetSize != writtenSize)
				{
					throw new Exception($"Network packet written size '{writtenSize}' differs from the expected size '{packetSize}' for packet of type '{packetType.ToString()}'.");
				}
			}

			// Everything looks good. Write the packet to destination.
			destination.Write((byte)packetType);
			if (isVariableSize)
				destination.Write((ushort)packetSize);
			destination.Write(Buffer.GetBuffer(), 0, packetSize);
		}

		#endregion

		#region Pooling

		private static List<PacketBuilder> PooledPacketBuilders = new List<PacketBuilder>(10);

		public static PacketBuilder Create()
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

			packetBuilder.StartBuild();
			return packetBuilder;
		}

		public static void Finalize(PacketType packetType, ref PacketBuilder packetBuilder, BinaryWriter destination)
		{
			var cachedPacketBuilder = packetBuilder;
			packetBuilder = null;

			try
			{
				cachedPacketBuilder.EndBuild(destination, packetType);
			}
			catch
			{
				throw;
			}
			finally
			{
				cachedPacketBuilder.Clear();
				if (PooledPacketBuilders.Count < PoolingMaxSize)
					PooledPacketBuilders.Add(cachedPacketBuilder);
			}
		}

		#endregion
	}

}
