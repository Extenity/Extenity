using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Extenity.Debugging
{

	public class DebugLogErrorLimiter : MonoBehaviour
	{
		public int StopApplicationWhenErrorCountReaches = 500;
		public int ReceivedErrorCount;
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
			if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
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

}
