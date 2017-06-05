using System;
using System.Collections.Generic;
using System.IO;
using Extenity.AssetToolbox.Editor;
using Extenity.ConsistencyToolbox;
using Extenity.DataToolbox;
using UnityEditor;
using UnityEngine;

namespace Extenity.DLLBuilder
{

	public class DLLBuilderConfiguration : ScriptableObject
	{
		#region Configuration - Compiler

		[Serializable]
		public class CompilerConfiguration : IConsistencyChecker
		{
			public bool Enabled = true;

			[Header("Paths")]
			public string DLLNameWithoutExtension;
			public string DLLName { get { return DLLNameWithoutExtension + ".dll"; } }
			public string DLLOutputDirectoryPath = @"Output\Assets\Plugins\ProjectName";
			public string EditorDLLNamePostfix = ".Editor";
			public string EditorDLLNameWithoutExtension { get { return DLLNameWithoutExtension + EditorDLLNamePostfix; } }
			public string EditorDLLName { get { return EditorDLLNameWithoutExtension + ".dll"; } }
			public bool UseRelativeEditorDLLOutputDirectoryPath = true;
			public string EditorDLLOutputDirectoryPath = "Editor";
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
			public string DLLDebugDatabasePath { get { return Path.Combine(ProcessedDLLOutputDirectoryPath, DLLNameWithoutExtension + ".mdb"); } }
			public string EditorDLLDocumentationPath { get { return Path.Combine(ProcessedEditorDLLOutputDirectoryPath, EditorDLLNameWithoutExtension + ".xml"); } }
			public string EditorDLLDebugDatabasePath { get { return Path.Combine(ProcessedEditorDLLOutputDirectoryPath, EditorDLLNameWithoutExtension + ".mdb"); } }

			[Header("Sources")]
			public string SourcePath;
			public string IntermediateSourceDirectoryPath;
			public string[] ExcludedKeywords;

			[Header("Generation")]
			public bool GenerateDocumentation;
			public bool GenerateDebugInfo;

			[Header("DLL References")]
			public bool AddUnityEngineDLLInUnityManagedDirectory = true;
			public bool AddUnityEditorDLLInUnityManagedDirectoryForEditorDLL = true;
			public bool AddAllDLLsInUnityManagedDirectory = false;
			public bool AddRuntimeDLLReferenceInEditorDLL;
			public string[] References;

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

			public void CheckConsistencyOfPaths(ref List<ConsistencyError> errors)
			{
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
				if (!Enabled)
					return;

				CheckConsistencyOfPaths(ref errors);
				CheckConsistencyOfSources(ref errors);
			}
		}

		[SerializeField]
		public CompilerConfiguration[] CompilerConfigurations;

		#endregion

		#region Configuration - Distributer

		[Serializable]
		public class DistributerConfiguration
		{
			[Serializable]
			public class DistributionTarget
			{
				public bool Enabled = true;
				public string Path;
			}

			[SerializeField]
			public DistributionTarget[] Targets;
		}

		[SerializeField]
		public DistributerConfiguration Distributer;

		#endregion

		#region Instance

		private static DLLBuilderConfiguration _Instance;
		public static DLLBuilderConfiguration Instance
		{
			get
			{
				if (_Instance == null)
					LoadOrCreateConfiguration();
				return _Instance;
			}
		}

		#endregion

		#region Load or Create Configuration Asset

		public static void LoadOrCreateConfiguration()
		{
			var filter = "t:" + typeof(DLLBuilderConfiguration).Name;
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
			var assetIDs = AssetDatabase.FindAssets(filter);
			if (assetIDs.Length == 0)
			{
				Debug.Log(Constants.DLLBuilderName + " configuration asset does not exist. Creating new one.");
				_Instance = AssetTools.CreateAsset<DLLBuilderConfiguration>(Constants.DefaultConfigurationPath);
			}
			else if (assetIDs.Length > 1)
			{
				throw new Exception("There are more than one " + typeof(DLLBuilderConfiguration).Name + " asset in project. Please make sure only one of them exists. Until then, " + Constants.DLLBuilderName + " won't be able to function.");
			}
			else
			{
				var assetPath = AssetDatabase.GUIDToAssetPath(assetIDs[0]);
				Debug.Log(Constants.DLLBuilderName + " configuration asset found at path: " + assetPath);
				_Instance = AssetDatabase.LoadAssetAtPath<DLLBuilderConfiguration>(assetPath);
			}
		}

		#endregion

		#region Menu

		[MenuItem(Constants.MenuItemPrefix + "Select Configuration Asset", priority = 1005)]
		public static void SelectOrCreateConfigurationAsset()
		{
			Selection.activeObject = Instance;
		}

		#endregion
	}

}
