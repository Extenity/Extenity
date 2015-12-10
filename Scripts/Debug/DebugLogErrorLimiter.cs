using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DebugLogErrorLimiter : MonoBehaviour
{
	public int StopApplicationWhenErrorCountReaches = 500;
	private int ReceivedErrorCount;
	//public List<string> ErrorMessages;

	protected void Awake()
	{
		//ErrorMessages = new List<string>(StopApplicationWhenErrorCountReaches);

		//Application.logMessageReceived += OnLogMessageReceived;
		Application.logMessageReceivedThreaded += OnLogMessageReceived;
	}

	protected void OnDestroy()
	{
		//Application.logMessageReceived -= OnLogMessageReceived;
		Application.logMessageReceivedThreaded -= OnLogMessageReceived;
	}

	public void OnLogMessageReceived(string condition, string stackTrace, LogType type)
	{
		if (type == LogType.Error)
		{
			ReceivedErrorCount++;

			//ErrorMessages.Add(condition);

			if (ReceivedErrorCount >= StopApplicationWhenErrorCountReaches)
			{
#if UNITY_EDITOR
				EditorApplication.isPaused = true;
#else
				Application.Quit();
#endif

				Destroy(this);
			}
		}
	}
}
