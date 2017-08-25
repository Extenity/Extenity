
namespace Extenity.DebugFlowTool.Generic
{

	public static class Constants
	{
		#region Version

		public static readonly byte MajorVersion = 1;
		public static readonly ushort MinorVersion = 12;
		public static readonly string VersionText = BuildVersionText(MajorVersion, MinorVersion);

		public static string BuildVersionText(byte majorVersion, ushort minorVersion)
		{
			return majorVersion + "." + minorVersion;
		}

		public static bool IsMatchingVersion(byte majorVersion, ushort minorVersion)
		{
			return
				MajorVersion == majorVersion &&
				MinorVersion == minorVersion;
		}

		#endregion

		#region Handshake

		public static readonly byte[] HandshakeMagicSequence = { (byte)'P', (byte)'O', (byte)'P', (byte)'I', (byte)'T' };

		#endregion
	}

}
