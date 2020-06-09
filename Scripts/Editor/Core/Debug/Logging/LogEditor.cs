using Extenity.ProjectToolbox;
using UnityEditor;

namespace Extenity.DebugToolbox.Editor
{

	public static class LogEditor
	{
		public const string Menu = "Tools/Logging/";

		#region Toggle Verbose Logging Menu

		[MenuItem(Menu + "Verbose Logging/Enable")]
		private static void Menu_EnableVerboseLogging()
		{
			ActivateVerboseLogging(true);
		}

		[MenuItem(Menu + "Verbose Logging/Disable")]
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
#if DisableVerboseLogging
			return false;
#else
			return true;
#endif
		}

		#endregion

		#region Toggle Info Logging Menu

		[MenuItem(Menu + "Info Logging/Enable")]
		private static void Menu_EnableInfoLogging()
		{
			ActivateInfoLogging(true);
		}

		[MenuItem(Menu + "Info Logging/Disable")]
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
#if DisableInfoLogging
			return false;
#else
			return true;
#endif
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
