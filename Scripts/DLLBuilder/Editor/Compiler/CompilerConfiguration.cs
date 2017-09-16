using System;
using System.Collections.Generic;
using System.IO;
using Extenity.ConsistencyToolbox;
using Extenity.DataToolbox;
using UnityEngine;


namespace Extenity.DLLBuilder
{

	public enum CompilerType
	{
		Gmcs = 0,
		Smcs = 1,
		MSBuild = 20,
	}

	[Serializable]
	public class CompilerConfiguration : IConsistencyChecker
	{
		public bool Enabled = true;

		[Header("Paths")]
		public string DLLNameWithoutExtension;
		public string DLLName { get { return DLLNameWithoutExtension + ".dll"; } }
		public string EditorDLLNamePostfix = ".Editor";
		public string EditorDLLNameWithoutExtension { get { return DLLNameWithoutExtension + EditorDLLNamePostfix; } }
		public string EditorDLLName { get { return EditorDLLNameWithoutExtension + ".dll"; } }
		public string DLLOutputDirectoryPath = @"Output\Assets\Plugins\ProjectName";
		public string EditorDLLOutputDirectoryPath = "Editor";
		public bool UseRelativeEditorDLLOutputDirectoryPath = true;
		public string ProcessedDLLOutputDirectoryPath
		{
			get
			{
				return DLLOutputDirectoryPath.FixDirectorySeparatorChars();
			}
		}
		public string ProcessedEditorDLLOutputDirectoryPath
		{
			get
			{
				return UseRelativeEditorDLLOutputDirectoryPath
					? Path.Combine(DLLOutputDirectoryPath.FixDirectorySeparatorChars(), EditorDLLOutputDirectoryPath.FixDirectorySeparatorChars())
					: EditorDLLOutputDirectoryPath.FixDirectorySeparatorChars();
			}
		}
		public string DLLPath { get { return Path.Combine(ProcessedDLLOutputDirectoryPath, DLLName); } }
		public string EditorDLLPath { get { return Path.Combine(ProcessedEditorDLLOutputDirectoryPath, EditorDLLName); } }
		public string DLLDocumentationPath { get { return Path.Combine(ProcessedDLLOutputDirectoryPath, DLLNameWithoutExtension + ".xml"); } }
		public string DLLDebugDatabasePath { get { return Path.Combine(ProcessedDLLOutputDirectoryPath, DLLName + ".mdb"); } }
		public string EditorDLLDocumentationPath { get { return Path.Combine(ProcessedEditorDLLOutputDirectoryPath, EditorDLLNameWithoutExtension + ".xml"); } }
		public string EditorDLLDebugDatabasePath { get { return Path.Combine(ProcessedEditorDLLOutputDirectoryPath, EditorDLLName + ".mdb"); } }

		[Header("Sources")]
		public string SourcePath;
		public string IntermediateSourceDirectoryPath;
		public string[] ExcludedKeywords;
		public string ProcessedSourcePath
		{
			get { return SourcePath.FixDirectorySeparatorChars().RemoveEndingDirectorySeparatorChar(); }
		}
		public string ProcessedIntermediateSourceDirectoryPath
		{
			get { return IntermediateSourceDirectoryPath.FixDirectorySeparatorChars().RemoveEndingDirectorySeparatorChar(); }
		}

		[Header("Generation")]
		public bool GenerateDocumentation;
		public bool GenerateDebugInfo;

		[Header("DLL References")]
		public bool AddUnityEngineDLLReferenceIntoRuntimeAndEditorDLLs = true;
		public bool AddUnityEditorDLLReferenceIntoEditorDLL = true;
		//public bool AddAllDLLsInUnityManagedDirectory = false; Not a wise idea
		public bool AddRuntimeDLLReferenceIntoEditorDLL;
		public string[] RuntimeReferences;
		public string[] EditorReferences;

		[Header("Preprocessor")]
		public string[] RuntimeDefines = { };
		public string[] EditorDefines = { "TRACE", "DEBUG" };

		public string RuntimeDefinesAsString
		{
			get { return RuntimeDefines.Serialize(';'); }
		}
		public string EditorDefinesAsString
		{
			get { return EditorDefines.Serialize(';'); }
		}

		[Header("Compiler")]
		public CompilerType Compiler = CompilerType.Gmcs;

		[Header("Obfuscation")]
		public bool Obfuscate = false;

		#region Consistency

		public void CheckConsistencyOfPaths(ref List<ConsistencyError> errors)
		{
			// Do not check for enabled here. Other parts of the application may depend on this.
			//if (!Enabled)
			//	return;

			if (string.IsNullOrEmpty(DLLNameWithoutExtension))
			{
				errors.Add(new ConsistencyError(this, "DLL Name Without Extension must be specified."));
			}
			if (string.IsNullOrEmpty(DLLOutputDirectoryPath))
			{
				errors.Add(new ConsistencyError(this, "DLL Output Directory Path must be specified."));
			}
		}

		public void CheckConsistencyOfSources(ref List<ConsistencyError> errors)
		{
			// Do not check for enabled here. Other parts of the application may depend on this.
			//if (!Enabled)
			//	return;

			if (string.IsNullOrEmpty(SourcePath))
			{
				errors.Add(new ConsistencyError(this, "Source Path must be specified."));
			}
			if (string.IsNullOrEmpty(IntermediateSourceDirectoryPath))
			{
				errors.Add(new ConsistencyError(this, "Intermediate Source Directory Path must be specified."));
			}
		}

		public void CheckConsistency(ref List<ConsistencyError> errors)
		{
			// We won't be doing this anymore. Instead, we won't be calling consistency checks on disabled configurations.
			//if (!Enabled)
			//	return;

			CheckConsistencyOfPaths(ref errors);
			CheckConsistencyOfSources(ref errors);
		}

		#endregion
	}


}
