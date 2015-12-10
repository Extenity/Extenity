using UnityEngine;
using System.Net;
using System.IO;
using Extenity.Logging;
using Logger = Extenity.Logging.Logger;

public static class NetworkTools
{
	public static bool IsNetworkStarted
	{
		get { return Network.peerType != NetworkPeerType.Disconnected; }
	}

	public static bool IsNetworkReady
	{
		get { return Network.peerType == NetworkPeerType.Server || Network.peerType == NetworkPeerType.Client; }
	}

	public static bool IsServerStarted
	{
		get { return Network.peerType == NetworkPeerType.Server; }
	}

	public static bool IsClientConnecting
	{
		get { return Network.peerType == NetworkPeerType.Connecting; }
	}

	public static bool IsClientReady
	{
		get { return Network.peerType == NetworkPeerType.Client; }
	}

	public static int NetworkPlayerID(this NetworkPlayer player)
	{
		return int.Parse(player.ToString());
	}

	public static int GetSelfNetworkPlayerID
	{
		get { return int.Parse(Network.player.ToString()); }
	}

	public static bool CheckForAddressInconsistency(string address)
	{
		if (string.IsNullOrEmpty(address))
			return false;
		return true;
	}

	public static bool CheckForPortInconsistency(string port)
	{
		if (string.IsNullOrEmpty(port))
			return false;
		if (!port.IsNumeric())
			return false;
		return CheckForPortInconsistency(int.Parse(port));
	}

	public static bool CheckForPortInconsistency(int port)
	{
		return port > 0 && port < 65536;
	}

	public static string QueryExternalIP()
	{
		HttpWebRequest request = HttpWebRequest.Create("http://www.whatismyip.org/") as HttpWebRequest;
		if (request == null)
			return string.Empty;

		request.UserAgent = "User-Agent: Mozilla/4.0 (compatible; MSIE6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
		HttpWebResponse response = request.GetResponse() as HttpWebResponse;
		if (response == null)
			return string.Empty;

		StreamReader reader = new StreamReader(response.GetResponseStream());
		string externalIP = reader.ReadToEnd();
		reader.Close();

		response.Close();
		return externalIP;
	}

	#region Master Server
	
	public static bool IsEqual(this HostData[] us, HostData[] other)
	{
		if (us.Length != other.Length)
			return false;

		if (ReferenceEquals(us, other))
			return true;

		for (int i = 0; i < us.Length; i++)
		{
			if (!us[i].IsEqual(other[i]))
			{
				return false;
			}
		}

		return true;
	}

	public static bool IsEqual(this HostData us, HostData other)
	{
		return
			us.connectedPlayers == other.connectedPlayers &&
			us.comment == other.comment &&
			us.gameName == other.gameName &&
			us.gameType == other.gameType &&
			us.passwordProtected == other.passwordProtected &&
			us.ip.IsEqual(other.ip) &&
			us.port == other.port &&
			us.guid == other.guid &&
			us.playerLimit == other.playerLimit &&
			us.useNat == other.useNat;
	}

	public static void DebugLog(this HostData data)
	{
		string text = "";
		text += "connectedPlayers: " + data.connectedPlayers + "\n";
		text += "comment: " + data.comment + "\n";
		text += "gameName: " + data.gameName + "\n";
		text += "gameType: " + data.gameType + "\n";
		text += "passwordProtected: " + data.passwordProtected + "\n";
		text += "ip: " + data.ip.Serialize() + "\n";
		text += "port: " + data.port + "\n";
		text += "guid: " + data.guid + "\n";
		text += "playerLimit: " + data.playerLimit + "\n";
		text += "useNat: " + data.useNat;

		Logger.Log(text);
	}

	#endregion
}
