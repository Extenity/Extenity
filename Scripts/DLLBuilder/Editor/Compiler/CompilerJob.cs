using System;
using System.Collections.Generic;

namespace Extenity.DLLBuilder
{

	public enum CompileResult
	{
		Failed,
		Succeeded,
		Skipped,
	}

	public class CompilerJob
	{
		public CompilerConfiguration Configuration;

		public List<string> SourceFilePathsForRuntimeDLL;
		public List<string> SourceFilePathsForEditorDLL;

		public List<string> UnityManagedReferences;

		public CompileResult RuntimeDLLSucceeded;
		public CompileResult EditorDLLSucceeded;
		public bool Finished;
		public Action<CompilerJob> OnFinished;

		public CompilerJob(CompilerConfiguration configuration, Action<CompilerJob> onFinished = null)
		{
			Configuration = configuration;
			OnFinished = onFinished;
		}
	}


}
