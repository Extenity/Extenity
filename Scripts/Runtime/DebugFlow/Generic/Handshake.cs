using System;
using System.IO;

namespace Extenity.DebugFlowTool.Generic
{

	public static class Handshake
	{
		/// <summary>
		/// Size of the packet is total of 240 bytes, including reserved spaces for future uses.
		/// This total size should not be changed. Otherwise different versions of the application
		/// mat not be able to communicate well.
		/// </summary>
		public static readonly int SessionRequestPacketSize =
			Constants.HandshakeMagicSequence.Length +
			sizeof(byte) +
			sizeof(ushort) +
			58 * sizeof(UInt32);

		public static void SendSessionRequest(BinaryWriter destination)
		{
			var packet = PacketBuilder.Create();

			// Write handshake magic sequence and then version info.
			// Note that these MUST come first in handshaking packet, no matter what.
			// That will allow version checking between old and new versions even if 
			// the handshaking packet content changes in future releases.
			packet.Writer.Write(Constants.HandshakeMagicSequence);
			if (Constants.MajorVersion.GetType() != typeof(byte)) throw new Exception();
			if (Constants.MinorVersion.GetType() != typeof(ushort)) throw new Exception();
			packet.Writer.Write(Constants.MajorVersion);
			packet.Writer.Write(Constants.MinorVersion);

			// Reserved for future uses
			for (int i = 0; i < 58; i++)
			{
				packet.Writer.Write((UInt32)0xF0F0F0F0);
			}

			PacketBuilder.Finalize(PacketType.SessionRequest, ref packet, destination);
		}

		public static void SendSessionDenyResponse(BinaryWriter destination, string message)
		{
			throw new NotImplementedException();
		}

		public static void SendSessionAcknowledgeResponse(BinaryWriter destination, UInt32 sessionID)
		{
			throw new NotImplementedException();
		}

		public static void ProcessSessionRequest(BinaryReader source, BinaryWriter destination)
		{
			var majorVersion = source.ReadByte();
			var minorVersion = source.ReadUInt16();

			// Check if version matches without further reading the incoming data.
			// Otherwise we can't be sure about the data format of another version.
			if (!Constants.IsMatchingVersion(majorVersion, minorVersion))
			{
				// Version does not match. Refuse connection request.
				var message = string.Format(Texts.RefuseConnection_VersionMismatch,
					Constants.BuildVersionText(majorVersion, minorVersion), // Client version
					Constants.VersionText // Interface version
				);
				SendSessionDenyResponse(destination, message);
			}
			else
			{
				// Everything looks good. Generate a unique SessionID and send response.
				var sessionID = UniqueIDGenerator.GenerateUInt32();

				SendSessionAcknowledgeResponse(destination, sessionID);
			}
		}

		public static void ProcessSessionResponse(BinaryReader source)
		{
			throw new NotImplementedException();
		}
	}

}
