using System;
using Photon.Realtime;

namespace PhotonTools
{

	[Serializable]
	public class JoinConfiguration
	{
		public string RoomName;
		public string RoomPassword;
		public string[] ExpectedUserIDs;
		public TypedLobby Lobby;
		//public object Tag; This is kept here commented out as a reminder for future ideas.

		public string ValidateAndFix()
		{
			if (string.IsNullOrWhiteSpace(RoomName))
			{
				return "Room name was not provided to join into.";
			}
			if (string.IsNullOrWhiteSpace(RoomPassword))
				RoomPassword = null; // Make sure it's not an empty string

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

			// TODO: Figure out how to use password
			if (!string.IsNullOrWhiteSpace(RoomPassword))
			{
				return "Password protected rooms are not implemented yet!";
			}

			return null;
		}
	}

}
