using System;
using System.Collections.Generic;
using System.Text;
using Extenity.ApplicationToolbox;
using UnityEditor;
using UnityEditor.Compilation;

namespace Extenity.UnityEditorToolbox.CompilationProfiling
{

	/// <summary>
	/// Source: https://forum.unity.com/threads/unity-assembly-definition-files-slower-not-faster-how-to-compile-faster.511199/#post-3667132
	/// </summary>
	[InitializeOnLoad]
	public class CompilationProfiler
	{
		#region Configuration

		private const string AssemblyReloadStartTimeKey = "AssemblyReloadStartTime";
		private const string AssemblyCompilationLogKey = "AssemblyCompilationLog";
		private const string IsEnabledKey = "AssemblyCompilationProfilingEnabled";
		private static readonly int ScriptAssembliesPathLength = (ApplicationTools.UnityProjectPaths.LibraryDirectory + "/ScriptAssemblies/").Length;

		#endregion

		#region Menu

		[MenuItem(ExtenityMenu.Analysis + "Toggle Compilation Profiling", priority = ExtenityMenu.AnalysisPriority + 7)]
		private static void ToggleCompilationProfiling()
		{
			IsEnabled = !IsEnabled;
		}

		#endregion

		#region Initialization

		static CompilationProfiler()
		{
			if (IsEnabled)
				RegisterCompilationEvents();
		}

		private static void RegisterCompilationEvents()
		{
			CompilationPipeline.assemblyCompilationStarted += OnAssemblyCompilationStarted;
			CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;
			AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
			AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
		}

		#endregion

		#region Deinitialization

		private static void DeregisterCompilationEvents()
		{
			ResetData();
			CompilationPipeline.assemblyCompilationStarted -= OnAssemblyCompilationStarted;
			CompilationPipeline.assemblyCompilationFinished -= OnAssemblyCompilationFinished;
			AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
			AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
		}

		private static void ResetData()
		{
			AssemblyCompilationStartTimes = null;
			CompilationLog = null;
			TotalCompilationDuration = double.NaN;
		}

		#endregion

		#region Enable/Disable

		public static bool IsEnabled
		{
			get => EditorPrefs.GetBool(IsEnabledKey, false);
			set
			{
				if (IsEnabled == value)
					return;
				EditorPrefs.SetBool(IsEnabledKey, value);
				DeregisterCompilationEvents(); // Deregister first to prevent double registering.
				if (value)
				{
					RegisterCompilationEvents();
				}
				Log.Info("Compilation profiling is " + (value ? "enabled" : "disabled"));
			}
		}

		#endregion

		#region Process

		private static Dictionary<string, DateTime> AssemblyCompilationStartTimes;
		private static StringBuilder CompilationLog;
		private static double TotalCompilationDuration;

		private static void OnAssemblyCompilationStarted(string assembly)
		{
			if (AssemblyCompilationStartTimes == null)
				AssemblyCompilationStartTimes = new Dictionary<string, DateTime>();

			AssemblyCompilationStartTimes[assembly] = DateTime.UtcNow;
		}

		private static void OnAssemblyCompilationFinished(string assembly, CompilerMessage[] compilerMessages)
		{
			var passedTime = DateTime.UtcNow - AssemblyCompilationStartTimes[assembly];

			if (double.IsNaN(TotalCompilationDuration))
				TotalCompilationDuration = 0f;
			if (CompilationLog == null)
				CompilationLog = new StringBuilder();

			TotalCompilationDuration += passedTime.TotalMilliseconds;
			CompilationLog.Append($"\t{(passedTime.TotalMilliseconds / 1000.0):0.00}s\t{assembly.Substring(ScriptAssembliesPathLength)}\n");
		}

		private static void OnBeforeAssemblyReload()
		{
			if (CompilationLog == null)
				CompilationLog = new StringBuilder();

			CompilationLog.Append($"\t{(TotalCompilationDuration / 1000.0):0.00}s\tTotal compilation duration\n");
			TotalCompilationDuration = double.NaN;
			EditorPrefs.SetString(AssemblyReloadStartTimeKey, DateTime.UtcNow.ToBinary().ToString());
			EditorPrefs.SetString(AssemblyCompilationLogKey, CompilationLog.ToString());
		}

		private static void OnAfterAssemblyReload()
		{
			var assemblyReloadStartTimeString = EditorPrefs.GetString(AssemblyReloadStartTimeKey);
			EditorPrefs.DeleteKey(AssemblyReloadStartTimeKey);

			if (long.TryParse(assemblyReloadStartTimeString, out var assemblyReloadStartTimeBinary))
			{
				var assemblyReloadStartTime = DateTime.FromBinary(assemblyReloadStartTimeBinary);
				var passedTime = DateTime.UtcNow - assemblyReloadStartTime;
				var compilationLog = EditorPrefs.GetString(AssemblyCompilationLogKey);
				EditorPrefs.DeleteKey(AssemblyCompilationLogKey);
				if (!string.IsNullOrEmpty(compilationLog))
				{
					Log.Info($"Compilation Profiling Report\n{compilationLog}\n\t{(passedTime.TotalMilliseconds / 1000.0):0.00}s\tAssembly reload duration\n");
				}
			}

			// Memory cleanup
			ResetData();

			// Registration should be left untouched. We are actually registering for the next compilation, not for the current compilation.
			//DeregisterCompilationEvents();
		}

		#endregion
	}

}
