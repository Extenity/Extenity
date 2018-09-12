
namespace BeyondNetworking
{

	public enum NetworkState
	{
		Unspecified,

		NotConnected,

		ConnectingToCloud,
		Cloud,
		DisconnectingFromCloud,

		JoiningToLobby,
		Lobby,
		//LeavingLobby, Photon does not support this. If required, it can be implemented the old fashion way by setting a flag when leaving a lobby, and checking for that flag in GrabStateOfPhoton.

		JoiningToRoom,
		Room,
		LeavingRoom,
	}

	public enum ConnectivityMode
	{
		Unspecified,

		/// <summary>
		/// Disconnects from cloud. Useful to be used in main menu of the game.
		/// </summary>
		OfflineMenu,

		/// <summary>
		/// Connects to cloud and waits there. This will allow getting online player count in menu.
		/// </summary>
		OnlineMenu,

		/// <summary>
		/// Connects to cloud, then a lobby and waits there. This will allow getting the room list of connected lobby.
		/// </summary>
		GameFinder,

		/// <summary>
		/// Launches a play session in offline mode. Turns off the networking subsystem.
		/// </summary>
		OfflineSession,

		/// <summary>
		/// Launches a play session in single player mode. Allows playing over cloud servers, even though the game will be a single player one.
		/// </summary>
		SinglePlayerSession,

		/// <summary>
		/// Launches a play session and creates a room. The local player will be the host of the room.
		/// </summary>
		HostSession,

		/// <summary>
		/// Launches a play session and joins the specified room.
		/// </summary>
		JoinSession,

		/// <summary>
		/// Launches a play session and joins a random room.
		/// </summary>
		RandomJoinSession,
	}

	public enum NetworkProcessStep
	{
		Unknown,
		InternalError,

		// General process status
		ProcessStarted,
		ProcessCompleted,
		ProcessFailed,

		// Offline/Online modes
		SettingOffline,
		SettingOnline,

		// Authentication
		AuthenticationResponseInformation,
		AuthenticationFailedInformation,

		// Cloud connection
		ConnectedToMasterInformation,
		ConnectedToPhotonInformation,
		ConnectingToCloud,
		AlreadyConnectedToCloud,
		ConnectedToCloud,
		CloudConnectionFailed,
		Disconnecting,
		Disconnected,
		DisconnectedInformation,

		// Lobby - Join
		JoiningToLobby,
		JoinedLobbyInformation,
		AlreadyInsideLobby,
		ChangingLobby,
		JoinedToLobby,
		FailedToJoinLobby,

		// Lobby - Leave
		LeavingLobby,
		LeftLobbyInformation,
		AlreadyNotInLobby,
		LeftLobby,
		FailedToLeaveLobby,

		// Room creation
		CreatingOfflineRoom,
		CreatingSinglePlayerRoom,
		CreatingHostRoom,
		RoomCreatedInformation,
		CreateRoomFailedInformation,

		// Room joining
		JoiningRoom,
		JoinedRoomInformation,
		JoinRoomFailedInformation,

		// Room random joining
		RandomJoinFailedInformation,

		// Room leaving
		LeavingRoom,
		LeftRoom,
		FailedToLeaveRoom,
		AlreadyNotInRoom,
		LeftRoomInformation,
	}

}
