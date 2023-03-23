using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Extenity.AssetToolbox.Editor;
using Extenity.UnityEditorToolbox;
using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Extenity.CodeSnippetsToolbox.Editor
{

	public static class CodeSnippets
	{
#pragma warning disable 414

		#region Snippet - Main Script

		private static readonly SnippetInfo EmptyScript = new SnippetInfo(
			nameof(EmptyScript),
			"__NAME__.cs",
			new[] { "__NAME__", "__NAMESPACE__" },
			@"using UnityEngine;

namespace __NAMESPACE__
{

	public class __NAME__ : MonoBehaviour
	{
	}

}
"
		);

		#endregion

		#region Snippet - Main Script

		private static readonly SnippetInfo MainScript = new SnippetInfo(
			nameof(MainScript),
			"__NAME__.cs",
			new[] { "__NAME__", "__NAMESPACE__" },
			@"using UnityEngine;

namespace __NAMESPACE__
{

	public class __NAME__ : MonoBehaviour
	{
		#region Initialization

		protected void Awake()
		{
		}

		#endregion

		#region Deinitialization

		protected void OnDestroy()
		{
		}

		#endregion

		#region Update

		protected void Update()
		{
		}

		#endregion
	}

}
"
		);

		#endregion

		#region Snippet - Inspector Script

		/*
		private static readonly SnippetInfo InspectorScript = new SnippetInfo(
			nameof(InspectorScript),
			"Editor/__NAME__Inspector.cs",
			new[] { "__NAME__", "__NAMESPACE__" },
			@"using Extenity.UnityEditorToolbox.Editor;
using UnityEngine;
using UnityEditor;

namespace __NAMESPACE__
{

	[CustomEditor(typeof(__NAME__))]
	public class __NAME__Inspector : ExtenityEditorBase<__NAME__>
	{
		protected override void OnEnableDerived()
		{
		}

		protected override void OnDisableDerived()
		{
		}

		protected override void OnAfterDefaultInspectorGUI()
		{
		}
	}

}
"
		);
		*/

		#endregion

		#region Snippet - Test Assemblies

		private static readonly SnippetInfo TestAssemblyReadme = new SnippetInfo(
			nameof(TestAssemblyReadme),
			"__NAME__.Tests/Readme.txt",
			new[] { "__NAME__", "__NAMESPACE__" },
			@"Tests can be run on a device or can be run inside Unity Editor.
It's always a good idea to try setting up tests to run on all
platforms if possible which uncovers platform specific problems.

- EditAndPlayModes
All tests should be put here whenever applicable. They will be run
as EditMode tests when working in Editor and can easily be
converted to PlayMode tests via ""Tools > Extenity > Tests"" menu.
Also Build Machine can automatically convert them for test builds.

- EditModeOnly
Put the tests here which can only be run in Editor or won't be
meaningful to run on all platforms.

- PlayModeOnly
Put the tests here which requires Unity Play Mode to be run.
"
		);

		private static readonly SnippetInfo TestAsmdef_EditAndPlayModes = new SnippetInfo(
			nameof(TestAsmdef_EditAndPlayModes),
			"__NAME__.Tests/EditAndPlayModes/__NAME__.Tests.EditAndPlayModes.asmdef",
			new[] { "__NAME__", "__NAMESPACE__" },
			@"{
	""name"": ""__NAME__.Tests.EditAndPlayModes"",
	""rootNamespace"": ""__NAMESPACE__"",
	""references"": [
		""__NAME__"",
		""Extenity.Core"",
		""Extenity.Testing"",
		""UnityEngine.TestRunner"",
		""UnityEditor.TestRunner""
	],
	""includePlatforms"": [
		""Editor""
	],
	""excludePlatforms"": [],
	""allowUnsafeCode"": false,
	""overrideReferences"": true,
	""precompiledReferences"": [
		""nunit.framework.dll""
	],
	""autoReferenced"": false,
	""defineConstraints"": [
		""UNITY_INCLUDE_TESTS""
	],
	""versionDefines"": [],
	""noEngineReferences"": false
}
"
		);

		private static readonly SnippetInfo TestAssemblyInfo_EditAndPlayModes = new SnippetInfo(
			nameof(TestAssemblyInfo_EditAndPlayModes),
			"__NAME__.Tests/EditAndPlayModes/AssemblyInfo.cs",
			new[] { "__NAME__", "__NAMESPACE__" },
			@"using Extenity.CodingToolbox;

[assembly: EnsuredNamespace(""__NAMESPACE__"")]
"
		);

		private static readonly SnippetInfo TestScript_EditAndPlayModes = new SnippetInfo(
			nameof(TestScript_EditAndPlayModes),
			"__NAME__.Tests/EditAndPlayModes/Test___NAME__.cs",
			new[] { "__NAME__", "__NAMESPACE__" },
			@"/*
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace __NAMESPACE__
{

	public class Test___NAME__
	{
		[Test]
		public void NewTestScriptSimplePasses()
		{
			// Use the Assert class to test conditions.
		}
	}

}
*/
"
		);

		private static readonly SnippetInfo TestAsmdef_EditModeOnly = new SnippetInfo(
			nameof(TestAsmdef_EditModeOnly),
			"__NAME__.Tests/EditModeOnly/__NAME__.Tests.EditModeOnly.asmdef",
			new[] { "__NAME__", "__NAMESPACE__" },
			@"{
	""name"": ""__NAME__.Tests.EditModeOnly"",
	""rootNamespace"": ""__NAMESPACE__"",
	""references"": [
		""__NAME__"",
		""Extenity.Core"",
		""Extenity.Testing"",
		""UnityEngine.TestRunner"",
		""UnityEditor.TestRunner""
	],
	""includePlatforms"": [
		""Editor""
	],
	""excludePlatforms"": [],
	""allowUnsafeCode"": false,
	""overrideReferences"": true,
	""precompiledReferences"": [
		""nunit.framework.dll""
	],
	""autoReferenced"": false,
	""defineConstraints"": [
		""UNITY_INCLUDE_TESTS""
	],
	""versionDefines"": [],
	""noEngineReferences"": false
}
"
		);


		private static readonly SnippetInfo TestAssemblyInfo_EditModeOnly = new SnippetInfo(
			nameof(TestAssemblyInfo_EditModeOnly),
			"__NAME__.Tests/EditModeOnly/AssemblyInfo.cs",
			new[] { "__NAME__", "__NAMESPACE__" },
			@"using Extenity.CodingToolbox;

[assembly: EnsuredNamespace(""__NAMESPACE__"")]
"
		);

		private static readonly SnippetInfo TestScript_EditModeOnly = new SnippetInfo(
			nameof(TestScript_EditModeOnly),
			"__NAME__.Tests/EditModeOnly/Test___NAME__.cs",
			new[] { "__NAME__", "__NAMESPACE__" },
			@"/*
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace __NAMESPACE__
{

	public class Test___NAME__
	{
		[Test]
		public void NewTestScriptSimplePasses()
		{
			// Use the Assert class to test conditions.
		}
	}

}
*/
"
		);

		private static readonly SnippetInfo TestAsmdef_PlayModeOnly = new SnippetInfo(
			nameof(TestAsmdef_PlayModeOnly),
			"__NAME__.Tests/PlayModeOnly/__NAME__.Tests.PlayModeOnly.asmdef",
			new[] { "__NAME__", "__NAMESPACE__" },
			@"{
	""name"": ""__NAME__.Tests.PlayModeOnly"",
	""rootNamespace"": ""__NAMESPACE__"",
	""references"": [
		""__NAME__"",
		""Extenity.Core"",
		""Extenity.Testing"",
		""UnityEngine.TestRunner"",
		""UnityEditor.TestRunner""
	],
	""includePlatforms"": [],
	""excludePlatforms"": [],
	""allowUnsafeCode"": false,
	""overrideReferences"": true,
	""precompiledReferences"": [
		""nunit.framework.dll""
	],
	""autoReferenced"": false,
	""defineConstraints"": [
		""UNITY_INCLUDE_TESTS""
	],
	""versionDefines"": [],
	""noEngineReferences"": false
}
"
		);

		private static readonly SnippetInfo TestAssemblyInfo_PlayModeOnly = new SnippetInfo(
			nameof(TestAssemblyInfo_PlayModeOnly),
			"__NAME__.Tests/PlayModeOnly/AssemblyInfo.cs",
			new[] { "__NAME__", "__NAMESPACE__" },
			@"using Extenity.CodingToolbox;

[assembly: EnsuredNamespace(""__NAMESPACE__"")]
"
		);

		private static readonly SnippetInfo TestScript_PlayModeOnly = new SnippetInfo(
			nameof(TestScript_PlayModeOnly),
			"__NAME__.Tests/PlayModeOnly/Test___NAME__.cs",
			new[] { "__NAME__", "__NAMESPACE__" },
			@"/*
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace __NAMESPACE__
{

	public class Test___NAME__
	{
		// A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
		// `yield return null;` to skip a frame.
		[UnityTest]
		public IEnumerator NewTestScriptWithEnumeratorPasses()
		{
			// Use the Assert class to test conditions.
			// Use yield to skip a frame.
			yield return null;
		}
	}

}
*/
"
		);

		#endregion

		#region Snippet Groups

		// private static readonly SnippetGroup ScriptWithInspectorGroup = new SnippetGroup()
		// {
		// 	MainFileExtension = "cs",
		// 	AskForNamespace = true,
		// 	SnippetNames = new List<string> { nameof(MainScript), nameof(InspectorScript) }
		// };

		private static readonly SnippetGroup EmptyScriptGroup = new SnippetGroup()
		{
			MainFileExtension = "cs",
			AskForNamespace = true,
			SnippetNames = new List<string> { nameof(EmptyScript) }
		};

		private static readonly SnippetGroup MainScriptGroup = new SnippetGroup()
		{
			MainFileExtension = "cs",
			AskForNamespace = true,
			SnippetNames = new List<string> { nameof(MainScript) }
		};

		// private static readonly SnippetGroup InspectorGroup = new SnippetGroup()
		// {
		// 	MainFileExtension = "cs",
		// 	AskForNamespace = true,
		// 	SnippetNames = new List<string> { nameof(InspectorScript) }
		// };

		private static readonly SnippetGroup TestAssembliesGroup = new SnippetGroup()
		{
			MainFileExtension = "Tests",
			AskForNamespace = true,
			SnippetNames = new List<string>
			{
				nameof(TestAssemblyReadme),

				nameof(TestAsmdef_EditAndPlayModes),
				nameof(TestAssemblyInfo_EditAndPlayModes),
				nameof(TestScript_EditAndPlayModes),

				nameof(TestAsmdef_EditModeOnly),
				nameof(TestAssemblyInfo_EditModeOnly),
				nameof(TestScript_EditModeOnly),

				nameof(TestAsmdef_PlayModeOnly),
				nameof(TestAssemblyInfo_PlayModeOnly),
				nameof(TestScript_PlayModeOnly),
			}
		};

		#endregion

#pragma warning restore 414

		#region Snippet Info

		private class SnippetInfo
		{
			public string Name;
			public string Path;
			public string[] Properties;
			public string FileContent;

			public SnippetInfo(string name, string path, string[] properties, string fileContent)
			{
				Name = name;
				Path = path;
				Properties = properties;
				FileContent = fileContent;

				_RegisterSnippetInfo(this);
			}
		}

		#endregion

		#region All Snippet Infos

		private static List<SnippetInfo> AllSnippetInfos;

		private static void _RegisterSnippetInfo(SnippetInfo snippetInfo)
		{
			if (AllSnippetInfos == null)
			{
				AllSnippetInfos = new List<SnippetInfo>();
			}
			AllSnippetInfos.Add(snippetInfo);
		}

		private static SnippetInfo GetSnippetInfo(string snippetName)
		{
			if (AllSnippetInfos == null)
				return null;

			return AllSnippetInfos.FirstOrDefault(snippetInfo => snippetInfo.Name == snippetName);
		}

		#endregion

		#region Snippet Group

		private class SnippetGroup
		{
			public List<string> SnippetNames;
			public string MainFileExtension;
			public bool AskForNamespace;

			public List<string> GetAllSnippetFilePaths(string baseDirectory, Dictionary<string, string> macroDefinitions)
			{
				var list = new List<string>(SnippetNames.Count);
				foreach (var snippetName in SnippetNames)
				{
					var snippetInfo = GetSnippetInfo(snippetName);
					var processedPath = ProcessMacrosInText(snippetInfo.Path, macroDefinitions);
					var combinedPath = Path.Combine(baseDirectory, processedPath);
					list.Add(combinedPath);
				}
				return list;
			}

			public void CreateAllSnippetFiles(string baseDirectory, Dictionary<string, string> macroDefinitions)
			{
				foreach (var snippetName in SnippetNames)
				{
					try
					{
						var snippetInfo = GetSnippetInfo(snippetName);
						var processedPath = ProcessMacrosInText(snippetInfo.Path, macroDefinitions);
						var combinedPath = Path.Combine(baseDirectory, processedPath);
						var combinedPathDirectory = Path.GetDirectoryName(combinedPath);
						var processedFileContent = ProcessMacrosInText(snippetInfo.FileContent, macroDefinitions);

						if (!string.IsNullOrEmpty(combinedPathDirectory))
						{
							if (!Directory.Exists(combinedPathDirectory))
							{
								Directory.CreateDirectory(combinedPathDirectory);
							}
						}
						File.WriteAllText(combinedPath, processedFileContent);
					}
					catch (Exception exception)
					{
						Log.Error(new Exception("Could not create snippet file for '" + snippetName + "' snippet.", exception));
					}
				}
			}
		}

		#endregion

		#region Macros

		private static Dictionary<string, string> CreateMacroDefinitions(string scriptName, string scriptNamespace)
		{
			var definitions = new Dictionary<string, string>
			{
				{ "__NAME__", scriptName },
			};

			if (!string.IsNullOrEmpty(scriptNamespace))
			{
				definitions.Add("__NAMESPACE__", scriptNamespace);
			}

			return definitions;
		}

		private static string ProcessMacrosInText(string text, Dictionary<string, string> macroDefinitions)
		{
			foreach (var macroDefinition in macroDefinitions)
			{
				text = text.Replace(macroDefinition.Key, macroDefinition.Value);
			}
			return text;
		}

		#endregion

		#region File Operations

		private static bool CheckIfAnySnippetFileAlreadyExists(List<string> filePaths)
		{
			var found = false;
			foreach (var filePath in filePaths)
			{
				if (File.Exists(filePath))
				{
					found = true;
					Log.Error("Could not create snippet file because a file already exists at path '" + filePath + "'.");
				}
			}
			return found;
		}

		#endregion

		#region Create Snippet

		private static void CreateSnippet(SnippetGroup snippetGroup, Func<string, string> overrideDefaultNamespaceProcessor = null)
		{
			var path = EditorUtility.SaveFilePanel("Give a name to your script", AssetDatabaseTools.GetSelectedDirectoryPathOrAssetsRootPath(), "", snippetGroup.MainFileExtension);

			if (string.IsNullOrEmpty(path))
				return;

			var scriptName = Path.GetFileNameWithoutExtension(path);

			if (snippetGroup.AskForNamespace)
			{
				var rootNamespace = overrideDefaultNamespaceProcessor != null
					? overrideDefaultNamespaceProcessor(scriptName)
					: EditorSettings.projectGenerationRootNamespace;
				var inputField = new[] { new UserInputField("Namespace", string.IsNullOrEmpty(rootNamespace) ? "" : rootNamespace, false) };
				EditorMessageBox.Show(new Vector2Int(400, 200), "Enter Namespace", $"Enter the namespace of class '{scriptName}'.", inputField, "Create Snippet", "Cancel",
					() =>
					{
						var scriptNamespace = inputField[0].Value;
						InternalCreateSnippet(snippetGroup, path, scriptName, scriptNamespace);
					},
					() =>
					{
						Log.Info($"Cancelled snippet creation for '{scriptName}'.");
					}
				);
			}
			else
			{
				InternalCreateSnippet(snippetGroup, path, scriptName, "");
			}
		}

		private static void InternalCreateSnippet(SnippetGroup snippetGroup, string path, string scriptName, string scriptNamespace)
		{
			var baseDirectory = Path.GetDirectoryName(path);
			var macroDefinitions = CreateMacroDefinitions(scriptName, scriptNamespace);
			var snippetFilePaths = snippetGroup.GetAllSnippetFilePaths(baseDirectory, macroDefinitions);

			if (CheckIfAnySnippetFileAlreadyExists(snippetFilePaths))
				return;

			snippetGroup.CreateAllSnippetFiles(baseDirectory, macroDefinitions);

			AssetDatabase.Refresh();
		}

		#endregion

		#region Menu Commands

		[MenuItem(ExtenityMenu.CreateAssetBaseContext + "C# Script (Namespaced)", priority = ExtenityMenu.UnityCreateCSScriptMenuPriority)]
		private static void _CreateScript_MainScriptGroup()
		{
			CreateSnippet(MainScriptGroup);
		}

		[MenuItem(ExtenityMenu.CreateAssetBaseContext + "C# Script (Namespaced Empty)", priority = ExtenityMenu.UnityCreateCSScriptMenuPriority)]
		private static void _CreateScript_EmptyScriptGroup()
		{
			CreateSnippet(EmptyScriptGroup);
		}

		[MenuItem(ExtenityMenu.CreateAssetTestingContext + "Tests Assembly Triple Folders Setup", priority = ExtenityMenu.UnityCreateTestingScriptMenuPriority)]
		private static void _CreateScript_TestAssembliesGroup()
		{
			CreateSnippet(TestAssembliesGroup, scriptName => scriptName + ".Tests");
		}


		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(CreateSnippet));

		#endregion
	}

}
