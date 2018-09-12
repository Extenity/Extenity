using System;
using Photon.Realtime;

namespace BeyondNetworking
{

	[Serializable]
	public class GameFinderConfiguration
	{
		public TypedLobby Lobby;
		//public object Tag; This is kept here commented out as a reminder for future ideas.

		public string ValidateAndFix()
		{
			if (Lobby == null)
				Lobby = TypedLobby.Default; // Not stating a lobby means default lobby
			return null;
		}
	}

}
