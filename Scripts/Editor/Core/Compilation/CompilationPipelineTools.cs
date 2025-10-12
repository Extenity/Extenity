using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Extenity.CompilationToolbox.Editor
{

	public enum ScriptType
	{
		Unspecified,
		Runtime,
		Editor,
		Test,
	}

	public static class CompilationPipelineTools
	{
		#region Get Script Paths

		private static List<string> _ScriptPathsOfRuntimeAssemblies;
		public static List<string> ScriptPathsOfRuntimeAssemblies
		{
			get
			{
				if (_ScriptPathsOfRuntimeAssemblies == null)
				{
					_ScriptPathsOfRuntimeAssemblies = CompilationPipeline.GetAssemblies(AssembliesType.Player)
						.Select(item => item.sourceFiles.ToList())
						.Aggregate((list1, list2) => list1.Concat(list2).ToList());
				}
				return _ScriptPathsOfRuntimeAssemblies;
			}
		}
		private static List<string> _ScriptPathsOfEditorAssemblies;
		public static List<string> ScriptPathsOfEditorAssemblies
		{
			get
			{
				if (_ScriptPathsOfEditorAssemblies == null)
				{
					_ScriptPathsOfEditorAssemblies = CompilationPipeline.GetAssemblies(AssembliesType.Editor)
						.Select(item => item.sourceFiles
							.Where(source => !ScriptPathsOfRuntimeAssemblies.Contains(source)) // Need to exclude the ones that are already in Runtime list.
							.ToList())
						.Aggregate((list1, list2) => list1.Concat(list2).ToList());
				}
				return _ScriptPathsOfEditorAssemblies;
			}
		}

		#endregion

		#region Script Metadata

		public static ScriptType GetScriptType(string scriptPath)
		{
			if (ScriptPathsOfRuntimeAssemblies.Contains(scriptPath))
			{
				return ScriptType.Runtime;
			}
			else if (ScriptPathsOfEditorAssemblies.Contains(scriptPath))
			{
				return ScriptType.Editor;
			}
			else
			{
				return ScriptType.Test;
			}
		}

		#endregion

		#region Pause Recompilation

		private const string PauseRecompilationMenuPath = "Assets/Pause Recompilation";
		private const string ResumeRecompilationMenuPath = "Assets/Resume Recompilation";
		private const string ToggleRecompilationMenuPath = "Assets/Toggle Recompilation %&r";

		private static bool IsRecompilationPaused;

		[MenuItem(PauseRecompilationMenuPath, priority = 551)]
		public static void PauseRecompilation()
		{
			IsRecompilationPaused = true;
			EditorApplication.LockReloadAssemblies();
			Debug.Log("Recompilation paused. Scripts will not be recompiled until resumed.");
		}

		[MenuItem(ResumeRecompilationMenuPath, priority = 552)]
		public static void ResumeRecompilation()
		{
			IsRecompilationPaused = false;
			EditorApplication.UnlockReloadAssemblies();
			Debug.Log("Recompilation resumed. Scripts will now be recompiled on changes.");
		}

		[MenuItem(ToggleRecompilationMenuPath, priority = 553)]
		public static void ToggleRecompilation()
		{
			if (IsRecompilationPaused)
			{
				ResumeRecompilation();
			}
			else
			{
				PauseRecompilation();
			}
		}

		[MenuItem(PauseRecompilationMenuPath, true)]
		public static bool ValidatePauseRecompilation()
		{
			return !IsRecompilationPaused;
		}

		[MenuItem(ResumeRecompilationMenuPath, true)]
		public static bool ValidateResumeRecompilation()
		{
			return IsRecompilationPaused;
		}

		#endregion
	}

}
