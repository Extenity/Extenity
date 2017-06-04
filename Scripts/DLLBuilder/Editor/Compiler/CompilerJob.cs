using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Extenity.DLLBuilder
{

	public class CompilerJob
	{
		public DLLBuilderConfiguration.CompilerConfiguration Configuration;

		public List<string> SourceFilePathsForRuntimeDLL;
		public List<string> SourceFilePathsForEditorDLL;

		public List<string> UnityManagedReferences;

		public bool RuntimeDLLSucceeded;
		public bool EditorDLLSucceeded;
		public bool Finished;
		public Action<CompilerJob> OnFinished;

		public CompilerJob(DLLBuilderConfiguration.CompilerConfiguration configuration, Action<CompilerJob> onFinished = null)
		{
			Configuration = configuration;
			OnFinished = onFinished;
		}
	}


}
