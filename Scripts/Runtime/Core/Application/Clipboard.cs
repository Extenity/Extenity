#if UNITY_5_3_OR_NEWER

using UnityEngine;

namespace Extenity.ApplicationToolbox
{

	public static class Clipboard
	{
		public static void SetClipboardText(string text, bool log)
		{
			GUIUtility.systemCopyBuffer = text;

			if (log)
			{
				Log.With("Clipboard").Info("Copied to clipboard: " + text);
			}
		}

		public static string GetClipboardText()
		{
			return GUIUtility.systemCopyBuffer;
		}
	}

}

#endif