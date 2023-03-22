using Extenity.ProjectToolbox;
using Extenity.UnityEditorToolbox;
using UnityEditor;

namespace Extenity.DebugToolbox.Editor
{

	public static class LogEditor
	{
		public const string Menu = ExtenityMenu.Logging;

		#region Toggle Verbose Logging Menu

		[MenuItem(Menu + "Verbose Logging/Enable", priority = ExtenityMenu.LoggingPriority + 1)]
		private static void Menu_EnableVerboseLogging()
		{
			ActivateVerboseLogging(true);
		}

		[MenuItem(Menu + "Verbose Logging/Disable", priority = ExtenityMenu.LoggingPriority + 2)]
		private static void Menu_DisableVerboseLogging()
		{
			ActivateVerboseLogging(false);
		}

		[MenuItem(Menu + "Verbose Logging/Enable", validate = true)]
		private static bool Validate_EnableVerboseLogging()
		{
#if DisableVerboseLogging
			return true;
#else
			return false;
#endif
		}

		[MenuItem(Menu + "Verbose Logging/Disable", validate = true)]
		private static bool Validate_DisableVerboseLogging()
		{
			return !Validate_EnableVerboseLogging();
		}

		#endregion

		#region Toggle Info Logging Menu

		[MenuItem(Menu + "Info Logging/Enable", priority = ExtenityMenu.LoggingPriority + 3)]
		private static void Menu_EnableInfoLogging()
		{
			ActivateInfoLogging(true);
		}

		[MenuItem(Menu + "Info Logging/Disable", priority = ExtenityMenu.LoggingPriority + 4)]
		private static void Menu_DisableInfoLogging()
		{
			ActivateInfoLogging(false);
		}

		[MenuItem(Menu + "Info Logging/Enable", validate = true)]
		private static bool Validate_EnableInfoLogging()
		{
#if DisableInfoLogging
			return true;
#else
			return false;
#endif
		}

		[MenuItem(Menu + "Info Logging/Disable", validate = true)]
		private static bool Validate_DisableInfoLogging()
		{
			return !Validate_EnableInfoLogging();
		}

		#endregion

		#region Toggle Verbose Logging

		private const string DisableVerboseLoggingSymbol = "DisableVerboseLogging";

		public static void ActivateVerboseLogging(bool active)
		{
			if (active)
			{
				PlayerSettingsTools.RemoveDefineSymbols(new[] { DisableVerboseLoggingSymbol }, true);
			}
			else
			{
				PlayerSettingsTools.AddDefineSymbols(new[] { DisableVerboseLoggingSymbol }, false, true);
			}
		}

		#endregion

		#region Toggle Info Logging

		private const string DisableInfoLoggingSymbol = "DisableInfoLogging";

		public static void ActivateInfoLogging(bool active)
		{
			if (active)
			{
				PlayerSettingsTools.RemoveDefineSymbols(new[] { DisableInfoLoggingSymbol }, true);
			}
			else
			{
				PlayerSettingsTools.AddDefineSymbols(new[] { DisableInfoLoggingSymbol }, false, true);
			}
		}

		#endregion
	}

}
