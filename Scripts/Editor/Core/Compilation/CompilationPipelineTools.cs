using System.Collections.Generic;
using System.Linq;
using UnityEditor.Compilation;

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
	}

}
