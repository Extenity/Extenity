#if BeyondNetworkingUsesPhoton

using Extenity.DataToolbox;
using Extenity.IMGUIToolbox.Editor;
using Extenity.UnityEditorToolbox.Editor;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEditor;

namespace BeyondNetworking
{

	[CustomEditor(typeof(NetworkConnectivityManager))]
	public class NetworkConnectivityManagerInspector : ExtenityEditorBase<NetworkConnectivityManager>
	{
		protected override void OnEnableDerived()
		{
			AutoRepaintInspectorPeriod = 0.3f;
			IsAutoRepaintInspectorEnabled = true;

			NetworkConnectivityManager.OnNetworkStatsRefresh.AddListener(OnNetworkStatsRefresh);
		}

		protected override void OnDisableDerived()
		{
			NetworkConnectivityManager.OnNetworkStatsRefresh.RemoveListener(OnNetworkStatsRefresh);
		}

		private void OnNetworkStatsRefresh()
		{
			Repaint();
		}

		protected override void OnAfterDefaultInspectorGUI()
		{
			GUILayout.Space(40f);

			EditorGUILayoutTools.DrawHeader("Status");
			EditorGUILayout.LabelField("DesiredMode", NetworkConnectivityManager.DesiredMode.ToString());
			EditorGUILayout.LabelField("NetworkState", NetworkConnectivityManager.NetworkState.ToString());

			GUILayout.Space(6f);

			// Network stats
			EditorGUI.BeginDisabledGroup(!EditorApplication.isPlaying);
			{
				EditorGUILayoutTools.DrawHeader("Network Stats");
				NetworkConnectivityManager.NetworkStatisticsEnabled = GUILayout.Toggle(NetworkConnectivityManager.NetworkStatisticsEnabled, "Enabled");
				EditorGUI.BeginDisabledGroup(!NetworkConnectivityManager.NetworkStatisticsEnabled);
				NetworkConnectivityManager.NetworkStatisticsPlottingEnabled = GUILayout.Toggle(NetworkConnectivityManager.NetworkStatisticsPlottingEnabled, "Enable Graph Plotting");
				EditorGUI.EndDisabledGroup();

				EditorGUILayout.LabelField("Total Sent", $"{NetworkConnectivityManager.TotalSentPacketBytes:N0}\tCount: {NetworkConnectivityManager.TotalSentPacketCount}");
				EditorGUILayout.LabelField("Total Received", $"{NetworkConnectivityManager.TotalReceivedPacketBytes:N0}\tCount: {NetworkConnectivityManager.TotalReceivedPacketCount}");
				EditorGUILayout.LabelField("Delta Sent", $"{NetworkConnectivityManager.SentPacketBytesPerSecond:N0}\tCount: {NetworkConnectivityManager.SentPacketCountPerSecond}");
				EditorGUILayout.LabelField("Delta Received", $"{NetworkConnectivityManager.ReceivedPacketBytesPerSecond:N0}\tCount: {NetworkConnectivityManager.ReceivedPacketCountPerSecond}");
			}
			EditorGUI.EndDisabledGroup();

			GUILayout.Space(6f);

			// Lobby stats
			var clientAvailable = PhotonNetwork.NetworkClientState != ClientState.Disconnected;
			if (clientAvailable)
			{
				EditorGUILayoutTools.DrawHeader("Lobby Stats");
				EditorGUILayout.LabelField("Online Players", NetworkConnectivityManager.OnlinePlayerCount.ToString());
				EditorGUILayout.LabelField("Online Players In Menus", NetworkConnectivityManager.OnlinePlayerCountInMenus.ToString());
				EditorGUILayout.LabelField("Online Players In Rooms", NetworkConnectivityManager.OnlinePlayerCountInRooms.ToString());
				EditorGUILayout.LabelField("Online Rooms", NetworkConnectivityManager.OnlineRoomCount.ToString());

				GUILayout.Space(6f);
				GUILayout.Label($"Lobbies ({NetworkConnectivityManager.LobbyStatistics.SafeCount()}):");
				if (NetworkConnectivityManager.LobbyStatistics != null)
				{
					foreach (var lobby in NetworkConnectivityManager.LobbyStatistics)
					{
						GUILayout.Label(lobby.ToHumanReadableString());
					}
				}

				GUILayout.Space(6f);
				GUILayout.Label($"Rooms ({NetworkConnectivityManager.RoomList.SafeCount()}):");
				if (NetworkConnectivityManager.RoomList != null)
				{
					foreach (var room in NetworkConnectivityManager.RoomList)
					{
						GUILayout.Label(room.ToHumanReadableString());
					}
				}
			}

			GUILayout.Space(6f);

			EditorGUILayoutTools.DrawHeader("Photon Internals");
			EditorGUILayout.LabelField("NetworkClientState", PhotonNetwork.NetworkClientState.ToString());
			EditorGUILayout.LabelField("Server", PhotonNetwork.Server.ToString());
			EditorGUILayout.LabelField("IsConnected", PhotonNetwork.IsConnected.ToString());
			EditorGUILayout.LabelField("IsConnectedAndReady", PhotonNetwork.IsConnectedAndReady.ToString());
			EditorGUILayout.LabelField("AutomaticallySyncScene", PhotonNetwork.AutomaticallySyncScene.ToString());
			EditorGUILayout.LabelField("InRoom", PhotonNetwork.InRoom.ToString());
			EditorGUILayout.LabelField("InLobby", clientAvailable ? PhotonNetwork.InLobby.ToString() : "N/A");
			EditorGUILayout.LabelField("OfflineMode", PhotonNetwork.OfflineMode.ToString());
			EditorGUILayout.LabelField("ConnectMethod", PhotonNetwork.ConnectMethod.ToString());
			EditorGUILayout.LabelField("CurrentLobby", clientAvailable ? PhotonNetwork.CurrentLobby.ToHumanReadableString() : "N/A");
		}
	}

}

#endif
