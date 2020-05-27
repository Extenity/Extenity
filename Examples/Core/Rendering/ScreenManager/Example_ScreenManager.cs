using System.Collections.Generic;
using Extenity.RenderingToolbox;
using UnityEngine;

namespace ExtenityExamples.RenderingToolbox
{

	public class Example_ScreenManager : MonoBehaviour
	{
		private void Start()
		{
			Application.logMessageReceived += ApplicationOnLogMessageReceived;
			Application.logMessageReceivedThreaded += ApplicationOnLogMessageReceived;

			Log.Info("Initializing auto native size adjuster.");

			ScreenManager.Instance.IsFullscreenAutoNativeSizeAdjusterLoggingEnabled = true;
			ScreenManager.Instance.ActivateFullscreenAutoNativeSizeAdjuster(true);
		}

		private List<string> Logs = new List<string>();

		private void ApplicationOnLogMessageReceived(string condition, string stackTrace, LogType type)
		{
			Logs.Add(condition);
		}

		private void OnGUI()
		{
			for (int i = 0; i < Logs.Count; i++)
			{
				GUILayout.Label(Logs[i], GUILayout.Height(20));
			}
		}
	}

}
