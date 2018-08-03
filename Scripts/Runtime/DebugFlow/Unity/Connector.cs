using System.Net.Sockets;
using UnityEngine;

namespace Extenity.DebugFlowTool.Unity
{

	public class Connector : MonoBehaviour
	{
		#region Configuration

		private const string DefaultAddress = "localhost";
		private const int DefaultPort = 39741;
		private static readonly string AddressSettingsKey = "DebugFlowAddress";
		private static readonly string PortSettingsKey = "DebugFlowPort";

		public static string Address
		{
			get { return PlayerPrefs.GetString(AddressSettingsKey, DefaultAddress); }
			set { PlayerPrefs.SetString(AddressSettingsKey, value); }
		}

		public static int Port
		{
			get { return PlayerPrefs.GetInt(PortSettingsKey, DefaultPort); }
			set { PlayerPrefs.SetInt(PortSettingsKey, value); }
		}

		#endregion

		#region Initialization

		private void Start()
		{
			StartListening();
		}

		#endregion

		#region Deinitialization

		//protected void OnDestroy()
		//{
		//}

		#endregion

		#region Update

		private void Update()
		{
			// Receive data from connected clients
			// Also handle disconnection while doing that
			Connection.ReceiveDataAndHandleDisconnection();
		}

		#endregion

		#region Address And Port

		public string ConnectedAddress { get; private set; }
		public int ConnectedPort { get; private set; }

		public void ChangeAddressAndPort(string address, int port)
		{
			if (ConnectedAddress == address && ConnectedPort == port)
				return; // Ignore

			if (IsConnected)
			{
				StopListening();
				StartListening();
			}
			else
			{
				// Ignore
			}
		}

		#endregion

		#region Connect

		private TcpClient TCPClient;

		public bool IsConnected { get; private set; }

		private void StartListening()
		{
			if (IsConnected)
			{
				StopListening();
			}

			ConnectedAddress = null;
			ConnectedPort = -1;
			var address = Address;
			var port = Port;
			TCPClient = new TcpClient();
			TCPClient.Connect(address, port);
			Connection = new InterfaceConnection(TCPClient);

			ConnectedAddress = address;
			ConnectedPort = port;
			IsConnected = true;
		}

		private void StopListening()
		{
			if (!IsConnected)
				return;

			IsConnected = false;
			ConnectedAddress = null;
			ConnectedPort = -1;
			TCPClient.Close();
			TCPClient = null;
		}

		#endregion

		#region Connection

		public InterfaceConnection Connection;

		#endregion
	}

}
