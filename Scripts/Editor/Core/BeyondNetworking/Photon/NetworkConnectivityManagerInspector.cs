using Extenity.DataToolbox;
using Extenity.IMGUIToolbox.Editor;
using Extenity.UnityEditorToolbox.Editor;
using Photon.Pun;
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
		}

		protected override void OnDisableDerived()
		{
		}

		protected override void OnAfterDefaultInspectorGUI()
		{
			GUILayout.Space(40f);

			EditorGUILayoutTools.DrawHeader("Status");
			EditorGUILayout.LabelField("DesiredMode", NetworkConnectivityManager.DesiredMode.ToString());
			EditorGUILayout.LabelField("NetworkState", NetworkConnectivityManager.NetworkState.ToString());

			//if (PhotonNetwork.IsConnected)
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

			EditorGUILayoutTools.DrawHeader("Photon Internals");
			EditorGUILayout.LabelField("NetworkClientState", PhotonNetwork.NetworkClientState.ToString());
			EditorGUILayout.LabelField("Server", PhotonNetwork.Server.ToString());
			EditorGUILayout.LabelField("IsConnected", PhotonNetwork.IsConnected.ToString());
			EditorGUILayout.LabelField("IsConnectedAndReady", PhotonNetwork.IsConnectedAndReady.ToString());
			EditorGUILayout.LabelField("AutomaticallySyncScene", PhotonNetwork.AutomaticallySyncScene.ToString());
			EditorGUILayout.LabelField("InRoom", PhotonNetwork.InRoom.ToString());
			EditorGUILayout.LabelField("InLobby", PhotonNetwork.InLobby.ToString());
			EditorGUILayout.LabelField("OfflineMode", PhotonNetwork.OfflineMode.ToString());
			EditorGUILayout.LabelField("ConnectMethod", PhotonNetwork.ConnectMethod.ToString());
			EditorGUILayout.LabelField("CurrentLobby", PhotonNetwork.CurrentLobby.ToHumanReadableString());
		}
	}

}
