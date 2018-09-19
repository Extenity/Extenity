using ExitGames.Client.Photon;
using Extenity.DataToolbox;
using Photon.Realtime;

namespace BeyondNetworking
{

	public static class PhotonTools
	{
		#region ToString Variations

		public static string ToHumanReadableString(this TypedLobby lobby)
		{
			if (lobby == null)
				return "N/A";
			return $"Lobby '{lobby.Name}' of type '{lobby.Type}'";
		}

		public static string ToHumanReadableString(this TypedLobbyInfo lobby)
		{
			if (lobby == null)
				return "N/A";
			return $"Lobby '{lobby.Name}' of type '{lobby.Type}' with '{lobby.PlayerCount}' players and '{lobby.RoomCount}' rooms";
		}

		public static string ToHumanReadableString(this RoomInfo room)
		{
			if (room == null)
				return "N/A";
			return $"Room '{room.Name}' ({room.PlayerCount}/{room.MaxPlayers}) {(room.IsOpen ? "Open" : "Closed")}, {(room.IsVisible ? "Visible" : "Hidden")} {(room.CustomProperties.IsNotNullAndEmpty() ? "Properties:\n" + room.CustomProperties.ToJoinedString() : "")}";
		}

		public static string NickNameOrActorNumber(this Player player)
		{
			if (player == null)
				return "[Null]";
			return string.IsNullOrEmpty(player.NickName)
				? player.ActorNumber.ToString()
				: player.NickName;
		}

		#endregion

		#region Serialization

		private static readonly byte[] _Buffer = new byte[4];

		public static int Serialize(this StreamBuffer outStream, int value)
		{
			lock (_Buffer)
			{
				var index = 0;
				Protocol.Serialize(value, _Buffer, ref index);
				outStream.Write(_Buffer, 0, index);
				return index;
			}
		}

		public static int Serialize(this StreamBuffer outStream, short value)
		{
			lock (_Buffer)
			{
				var index = 0;
				Protocol.Serialize(value, _Buffer, ref index);
				outStream.Write(_Buffer, 0, index);
				return index;
			}
		}

		public static int Serialize(this StreamBuffer outStream, float value)
		{
			lock (_Buffer)
			{
				var index = 0;
				Protocol.Serialize(value, _Buffer, ref index);
				outStream.Write(_Buffer, 0, index);
				return index;
			}
		}

		public static int SerializeAsShort(this StreamBuffer outStream, int value)
		{
			lock (_Buffer)
			{
				var index = 0;
				var valueShort = (short)value;
				Protocol.Serialize(valueShort, _Buffer, ref index);
				outStream.Write(_Buffer, 0, index);
				return index;
			}
		}

		public static bool Deserialize(this StreamBuffer inStream, out int value)
		{
			lock (_Buffer)
			{
				var index = 0;
				var readSize = inStream.Read(_Buffer, 0, sizeof(int));
				if (readSize != sizeof(int))
				{
					value = int.MinValue;
					return false;
				}
				Protocol.Deserialize(out value, _Buffer, ref index);
				return true;
			}
		}

		public static bool Deserialize(this StreamBuffer inStream, out short value)
		{
			lock (_Buffer)
			{
				var index = 0;
				var readSize = inStream.Read(_Buffer, 0, sizeof(short));
				if (readSize != sizeof(short))
				{
					value = short.MinValue;
					return false;
				}
				Protocol.Deserialize(out value, _Buffer, ref index);
				return true;
			}
		}

		public static bool Deserialize(this StreamBuffer inStream, out float value)
		{
			lock (_Buffer)
			{
				var index = 0;
				var readSize = inStream.Read(_Buffer, 0, sizeof(float));
				if (readSize != sizeof(float))
				{
					value = float.NaN;
					return false;
				}
				Protocol.Deserialize(out value, _Buffer, ref index);
				return true;
			}
		}

		public static bool DeserializeAsShort(this StreamBuffer inStream, out int value)
		{
			lock (_Buffer)
			{
				var index = 0;
				var readSize = inStream.Read(_Buffer, 0, sizeof(short));
				if (readSize != sizeof(short))
				{
					value = int.MinValue;
					return false;
				}
				short valueShort;
				Protocol.Deserialize(out valueShort, _Buffer, ref index);
				value = valueShort;
				return true;
			}
		}

		#endregion
	}

}
