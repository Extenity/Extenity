#if UNITY_5_3_OR_NEWER

using System;
using UnityEngine;

namespace Extenity.DebugToolbox
{

	public class ErrorLogDetector : IDisposable
	{
		public bool DetectWarnings;
		public bool DetectErrors;
		public bool DetectExceptions;

		public bool AnyDetected;

		public ErrorLogDetector(bool detectWarnings, bool detectErrors, bool detectExceptions)
		{
			DetectWarnings = detectWarnings;
			DetectErrors = detectErrors;
			DetectExceptions = detectExceptions;

			Application.logMessageReceivedThreaded += OnLogMessageReceivedThreaded;
		}

		public void Dispose()
		{
			Application.logMessageReceivedThreaded -= OnLogMessageReceivedThreaded;
		}

		private void OnLogMessageReceivedThreaded(string condition, string stacktrace, LogType type)
		{
			switch (type)
			{
				case LogType.Log:
					break; // Skip regular log messages.
				case LogType.Error:
				case LogType.Assert:
					if (DetectErrors)
					{
						AnyDetected = true;
					}
					break;
				case LogType.Warning:
					if (DetectWarnings)
					{
						AnyDetected = true;
					}
					break;
				case LogType.Exception:
					if (DetectExceptions)
					{
						AnyDetected = true;
					}
					break;
			}
		}
	}

}

#endif
