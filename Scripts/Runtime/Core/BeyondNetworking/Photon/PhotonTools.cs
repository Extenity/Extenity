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
	}

}
