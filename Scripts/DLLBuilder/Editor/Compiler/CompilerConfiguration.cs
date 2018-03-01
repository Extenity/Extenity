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
		McsBleedingEdge = 2,
		MSBuild = 20,
	}

	[Serializable]
	public class CompilerConfiguration : IConsistencyChecker
	{
		[Header("Meta")]
		public string DLLNameWithoutExtension;
		public bool Enabled = true;

		[Header("Paths")]
		public string EditorDLLNamePostfix = ".Editor";
		public string EditorDLLNameWithoutExtension { get { return DLLNameWithoutExtension + EditorDLLNamePostfix; } }
		public string EditorDLLName { get { return EditorDLLNameWithoutExtension + ".dll"; } }
		public string DLLName { get { return DLLNameWithoutExtension + ".dll"; } }
		[Tooltip("Allows environment variables.")]
		public string DLLOutputDirectoryPath = @"Output\Assets\Plugins\ProjectName";
		[Tooltip("Allows environment variables.")]
		public string EditorDLLOutputDirectoryPath = "Editor";
		public bool UseRelativeEditorDLLOutputDirectoryPath = true;
		public string ProcessedDLLOutputDirectoryPath
		{
			get
			{
				return DLLBuilderConfiguration.InsertEnvironmentVariables(DLLOutputDirectoryPath).FixDirectorySeparatorChars();
			}
		}
		public string ProcessedEditorDLLOutputDirectoryPath
		{
			get
			{
				return UseRelativeEditorDLLOutputDirectoryPath
					? Path.Combine(DLLBuilderConfiguration.InsertEnvironmentVariables(DLLOutputDirectoryPath).FixDirectorySeparatorChars(), DLLBuilderConfiguration.InsertEnvironmentVariables(EditorDLLOutputDirectoryPath).FixDirectorySeparatorChars())
					: DLLBuilderConfiguration.InsertEnvironmentVariables(EditorDLLOutputDirectoryPath).FixDirectorySeparatorChars();
			}
		}
		public string DLLPath { get { return Path.Combine(ProcessedDLLOutputDirectoryPath, DLLName); } }
		public string EditorDLLPath { get { return Path.Combine(ProcessedEditorDLLOutputDirectoryPath, EditorDLLName); } }
		public string DLLDocumentationPath { get { return Path.Combine(ProcessedDLLOutputDirectoryPath, DLLNameWithoutExtension + ".xml"); } }
		public string DLLDebugDatabasePath { get { return Path.Combine(ProcessedDLLOutputDirectoryPath, DLLName + ".mdb"); } }
		public string EditorDLLDocumentationPath { get { return Path.Combine(ProcessedEditorDLLOutputDirectoryPath, EditorDLLNameWithoutExtension + ".xml"); } }
		public string EditorDLLDebugDatabasePath { get { return Path.Combine(ProcessedEditorDLLOutputDirectoryPath, EditorDLLName + ".mdb"); } }

		[Header("Sources")]
		[Tooltip("Allows environment variables.")]
		public string SourcePath;
		[Tooltip("Allows environment variables.")]
		public string IntermediateSourceDirectoryPath;
		public string[] ExcludedKeywords;
		public string ProcessedSourcePath
		{
			get { return DLLBuilderConfiguration.InsertEnvironmentVariables(SourcePath).FixDirectorySeparatorChars().RemoveEndingDirectorySeparatorChar(); }
		}
		public string ProcessedIntermediateSourceDirectoryPath
		{
			get { return DLLBuilderConfiguration.InsertEnvironmentVariables(IntermediateSourceDirectoryPath).FixDirectorySeparatorChars().RemoveEndingDirectorySeparatorChar(); }
		}

		[Header("Generation")]
		public bool GenerateDocumentation;
		public bool GenerateDebugInfo;

		[Header("DLL References")]
		public bool AddUnityEngineDLLReferenceIntoRuntimeAndEditorDLLs = true;
		public bool AddUnityEditorDLLReferenceIntoEditorDLL = true;
		//public bool AddAllDLLsInUnityManagedDirectory = false; Not a wise idea
		public bool AddRuntimeDLLReferenceIntoEditorDLL;
		[Tooltip("Allows environment variables.")]
		public string[] RuntimeReferences;
		[Tooltip("Allows environment variables.")]
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
		public bool Unsafe = false;

		[Header("Obfuscation")]
		public bool ObfuscateRuntimeDLL = false;
		public bool ObfuscateEditorDLL = false;

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
			DLLBuilderConfiguration.CheckEnvironmentVariableConsistency(DLLOutputDirectoryPath, ref errors);

			//if (string.IsNullOrEmpty(EditorDLLOutputDirectoryPath)) Nope, not required
			//{
			//	errors.Add(new ConsistencyError(this, "Editor DLL Output Directory Path must be specified."));
			//}
			DLLBuilderConfiguration.CheckEnvironmentVariableConsistency(EditorDLLOutputDirectoryPath, ref errors);
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
			DLLBuilderConfiguration.CheckEnvironmentVariableConsistency(SourcePath, ref errors);

			if (string.IsNullOrEmpty(IntermediateSourceDirectoryPath))
			{
				errors.Add(new ConsistencyError(this, "Intermediate Source Directory Path must be specified."));
			}
			DLLBuilderConfiguration.CheckEnvironmentVariableConsistency(IntermediateSourceDirectoryPath, ref errors);
		}

		public void CheckConsistencyOfReferences(ref List<ConsistencyError> errors)
		{
			// Do not check for enabled here. Other parts of the application may depend on this.
			//if (!Enabled)
			//	return;

			for (var i = 0; i < RuntimeReferences.Length; i++)
			{
				DLLBuilderConfiguration.CheckEnvironmentVariableConsistency(RuntimeReferences[i], ref errors);
			}

			for (var i = 0; i < EditorReferences.Length; i++)
			{
				DLLBuilderConfiguration.CheckEnvironmentVariableConsistency(EditorReferences[i], ref errors);
			}
		}

		public void CheckConsistency(ref List<ConsistencyError> errors)
		{
			// We won't be doing this anymore. Instead, we won't be calling consistency checks on disabled configurations.
			//if (!Enabled)
			//	return;

			CheckConsistencyOfPaths(ref errors);
			CheckConsistencyOfSources(ref errors);
			CheckConsistencyOfReferences(ref errors);
		}

		#endregion
	}


}
