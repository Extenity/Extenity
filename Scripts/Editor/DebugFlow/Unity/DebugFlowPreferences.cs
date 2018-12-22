using Extenity.IMGUIToolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Extenity.DebugFlowTool.Unity
{

	public static class DebugFlowPreferences
	{
		#region GUI

		[PreferenceItem(nameof(DebugFlow))]
		public static void PreferencesGUI()
		{
			EditorGUILayoutTools.DrawHeader("Target Interface");

			var currentAddress = Connector.Address;
			var newAddress = EditorGUILayout.TextField("Address", currentAddress);
			if (currentAddress != newAddress)
			{
				Connector.Address = newAddress;
			}

			var currentPort = Connector.Port;
			var newPort = EditorGUILayout.IntField("Port", currentPort);
			if (currentPort != newPort)
			{
				Connector.Port = newPort;
			}

			if (DebugFlow.Connector != null)
			{
				GUILayout.Space(20f);
				EditorGUILayoutTools.DrawHeader("Status");
				GUILayout.Label(DebugFlow.Connector.IsConnected ? "Connected" : "Not Connected");

				if (DebugFlow.Connector.IsConnected &&
					(
						DebugFlow.Connector.ConnectedAddress != newAddress ||
						DebugFlow.Connector.ConnectedPort != newPort
					)
				)
				{
					GUILayout.Label("WARNING! Connected address is not the configured address.");
				}

				if (DebugFlow.Connector.IsConnected)
				{
					GUILayout.Space(20f);
					GUILayout.Button("Reconnect");
				}
			}
		}

		#endregion
	}

}
