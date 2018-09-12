using System;
using ExitGames.Client.Photon;
using Photon.Realtime;

namespace BeyondNetworking
{

	[Serializable]
	public class RandomJoinConfiguration
	{
		public Hashtable ExpectedCustomRoomProperties;
		public byte ExpectedMaxPlayers;
		public MatchmakingMode MatchingType;
		public TypedLobby Lobby;
		public string SqlLobbyFilter;
		public string[] ExpectedUserIDs = null;

		// TODO: Implement this
		/// <summary>
		/// This will make the system not to show error logs when a proper room could not be found.
		/// </summary>
		public bool AssumeMatchmakingFailingIsOkay = false;

		//public object Tag; This is kept here commented out as a reminder for future ideas.

		public string ValidateAndFix()
		{
			if (ExpectedCustomRoomProperties != null && ExpectedCustomRoomProperties.Count == 0)
				ExpectedCustomRoomProperties = null; // Make sure it's not an empty collection
			if (string.IsNullOrWhiteSpace(SqlLobbyFilter))
				SqlLobbyFilter = null; // Make sure it's not an empty string

			if (ExpectedUserIDs != null)
			{
				if (ExpectedUserIDs.Length == 0)
				{
					ExpectedUserIDs = null; // Make sure it's not an empty array
				}
				else
				{
					for (var i = 0; i < ExpectedUserIDs.Length; i++)
						if (string.IsNullOrWhiteSpace(ExpectedUserIDs[i]))
							return $"An empty user ID at index '{i}' given as expected user.";
				}
			}

			if (Lobby != null)
			{
				if (string.IsNullOrWhiteSpace(Lobby.Name)) // Make sure it's not an empty string
					Lobby.Name = null;
			}
			else
			{
				if (Lobby == null)
					Lobby = TypedLobby.Default; // Not stating a lobby means default lobby
			}

			return null;
		}
	}

}
