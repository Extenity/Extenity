using System.Collections.Generic;
using Extenity.DataToolbox;

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
		public bool RuntimeDLLFinished;
		public bool EditorDLLFinished;

		public CompilerJob(CompilerConfiguration configuration)
		{
			Configuration = configuration;
		}

		public bool AnyRuntimeSourceFiles
		{
			get { return SourceFilePathsForRuntimeDLL.IsNotNullAndEmpty(); }
		}

		public bool AnyEditorSourceFiles
		{
			get { return SourceFilePathsForEditorDLL.IsNotNullAndEmpty(); }
		}

		public bool AnySourceFiles
		{
			get { return AnyRuntimeSourceFiles || AnyEditorSourceFiles; }
		}
	}


}
