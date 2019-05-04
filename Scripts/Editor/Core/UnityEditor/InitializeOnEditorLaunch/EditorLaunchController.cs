using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Extenity.DataToolbox;
using Extenity.DataToolbox.Editor;
using UnityEditor;

namespace Extenity.UnityEditorToolbox.Editor
{

	public static class EditorLaunchController
	{
		[InitializeOnLoadMethod]
		private static void LaunchMethodsWithAttributes()
		{
			CheckIfFirstAssemblyReloadOnEditorLifeTime();

			if (IsJustLaunched)
			{
				CallAttributedMethods();
			}
		}

		#region Initialize by checking for first assembly reload

		public static readonly IntEditorPref LastLaunchedEditorPID = new IntEditorPref("LastLaunchedEditorPID", PathHashPostfix.Yes, -1);

		private static bool _IsInitialized;

		private static void CheckIfFirstAssemblyReloadOnEditorLifeTime()
		{
			_IsInitialized = true;

			var pid = Process.GetCurrentProcess().Id;
			_IsJustLaunched = LastLaunchedEditorPID.Value != pid;
			if (_IsJustLaunched)
			{
				LastLaunchedEditorPID.Value = pid;
			}
		}

		#endregion

		#region Is Just Launched

		private static bool _IsJustLaunched;
		/// <summary>
		/// Tells if the Editor has just launched. More specifically, tells if the loaded assembly
		/// is a result of Editor's first assembly reload operation. Consecutive assembly reloads
		/// (happens when going into Play mode or after recompilation) will tell as the Editor
		/// is not just launched.
		/// </summary>
		public static bool IsJustLaunched
		{
			get
			{
				if (!_IsInitialized)
				{
					Log.Error("Tried to get Editor launch state before it has initialized.");
					return false;
				}
				return _IsJustLaunched;
			}
		}

		#endregion

		#region Call methods with attribute InitializeOnEditorLaunchMethodAttribute

		private static void CallAttributedMethods()
		{
			var methods = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(assembly => assembly
					.GetTypes()
					.SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
					.Where(method => method.GetCustomAttributes(typeof(InitializeOnEditorLaunchMethodAttribute), false).Length > 0)
					.ToList()
				).ToList();

			foreach (var method in methods)
			{
				method.Invoke(null, null);
			}
		}

		#endregion
	}

}
