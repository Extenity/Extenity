using Extenity.ApplicationToolbox;
using UnityEditor;

namespace Extenity.AssetToolbox.Editor
{

	[InitializeOnLoad]
	public static class QuickPlay
	{
		#region Configuration

		public static readonly AsyncKeyCodes[] ShortcutKeyCombination =
		{
			AsyncKeyCodes.RControl,
			AsyncKeyCodes.RShift,
			AsyncKeyCodes.X,
		};

		#endregion

		#region Initialization

		static QuickPlay()
		{
			EditorApplication.update += DetectGlobalShortcutKeyPress;
		}

		#endregion

		#region Global Shortcut Key Press

		private static bool IsPressing = true;

		private static void DetectGlobalShortcutKeyPress()
		{
			var currentlyPressing = true;
			for (var i = 0; i < ShortcutKeyCombination.Length; i++)
			{
				if (!OperatingSystemTools.IsAsyncKeyStateDown(ShortcutKeyCombination[i]))
				{
					currentlyPressing = false;
					break;
				}
			}

			if (currentlyPressing)
			{
				if (!IsPressing) // Prevent calling refresh multiple times before user releases the keys
				{
					IsPressing = true;
					Process();
				}
			}
			else
			{
				IsPressing = false;
			}
		}

		#endregion

		#region Process

		private static void Process()
		{
			Log.Info("QuickPlay triggered.");
			Command_StopPlaying();
			Command_RefreshAssetDatabase();
		}

		#endregion

		#region Commands

		private static void Command_StopPlaying()
		{
			if (EditorApplication.isPlaying)
			{
				EditorApplication.isPlaying = false;
			}
		}

		private static void Command_RefreshAssetDatabase()
		{
			AssetDatabase.Refresh();
		}

		#endregion
	}

}
