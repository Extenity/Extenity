using UnityEditor;

namespace Extenity.UnityEditorToolbox.Editor
{

	/// <summary>
	/// Tools for programmatically setting configurations in Unity Editor's Preferences window.
	/// </summary>
	public static class EditorPreferencesTools
	{
		#region Enable/Disable Auto Refresh

		public static bool IsAutoRefreshEnabled
		{
			get
			{
				return EditorPrefs.GetBool("kAutoRefresh");
			}
		}

		public static void EnableAutoRefresh(bool enabled)
		{
			EditorPrefs.SetBool("kAutoRefresh", enabled);
		}

		public static void EnableAutoRefresh()
		{
			EditorPrefs.SetBool("kAutoRefresh", true);
		}

		public static void DisableAutoRefresh()
		{
			EditorPrefs.SetBool("kAutoRefresh", false);
		}

		public static void ToggleAutoRefresh()
		{
			if (IsAutoRefreshEnabled)
			{
				DisableAutoRefresh();
			}
			else
			{
				EnableAutoRefresh();
			}
		}

		#endregion
	}

}
