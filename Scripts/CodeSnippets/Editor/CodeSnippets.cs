using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class CodeSnippets
{
#pragma warning disable 414

	#region Snippet - Main Script

	private static readonly SnippetInfo MainScript = new SnippetInfo()
	{
		Name = "Main",
		Path = "__NAME__.cs",
		Properties = new[] { "__NAME__" },
		FileContent =
@"using UnityEngine;
using Extenity.Logging;
using System.Collections;
using System.Collections.Generic;

public class __NAME__ : MonoBehaviour
{
	#region Initialization

	//protected void Awake()
	//{
	//}

	#endregion

	#region Deinitialization

	//protected void OnDestroy()
	//{
	//}

	#endregion

	#region Update

	//protected void Update()
	//{
	//}

	#endregion
}
"
	};

	#endregion

	#region Snippet - Main Script (Namespace)

	private static readonly SnippetInfo NamespacedMainScript = new SnippetInfo()
	{
		Name = "NamespacedMain",
		Path = "__NAME__.cs",
		Properties = new[] { "__NAME__", "__NAMESPACE__" },
		FileContent =
@"using UnityEngine;
using Extenity.Logging;
using System.Collections;
using System.Collections.Generic;

namespace __NAMESPACE__
{

	public class __NAME__ : MonoBehaviour
	{
		#region Initialization

		//protected void Awake()
		//{
		//}

		#endregion

		#region Deinitialization

		//protected void OnDestroy()
		//{
		//}

		#endregion

		#region Update

		//protected void Update()
		//{
		//}

		#endregion
	}

}
"
	};

	#endregion

	#region Snippet - Inspector Script

	private static readonly SnippetInfo InspectorScript = new SnippetInfo()
	{
		Name = "Inspector",
		Path = "Editor/__NAME__Inspector.cs",
		Properties = new[] { "__NAME__" },
		FileContent =
@"using UnityEngine;
using Extenity.Logging;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

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
"
	};

	#endregion

	#region Snippet - Inspector Script (Namespace)

	private static readonly SnippetInfo NamespacedInspectorScript = new SnippetInfo()
	{
		Name = "Inspector",
		Path = "Editor/__NAME__Inspector.cs",
		Properties = new[] { "__NAME__", "__NAMESPACE__" },
		FileContent =
@"using UnityEngine;
using Extenity.Logging;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

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
	};

	#endregion

	#region Snippet Groups

	private static SnippetGroup ScriptWithInspectorGroup = new SnippetGroup()
	{
		MainFileExtension = "cs",
		SnippetNames = new List<string> { "Main", "Inspector" }
	};

	private static SnippetGroup ScriptOnlyGroup = new SnippetGroup()
	{
		MainFileExtension = "cs",
		SnippetNames = new List<string> { "Main" }
	};

	private static SnippetGroup InspectorOnlyGroup = new SnippetGroup()
	{
		MainFileExtension = "cs",
		SnippetNames = new List<string> { "Inspector" }
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

		public SnippetInfo()
		{
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
				catch (Exception e)
				{
					Debug.LogError("Could not create snippet file for '" + snippetName + "' snippet. Exception: " + e);
				}
			}
		}
	}

	#endregion

	#region Macros

	private static Dictionary<string, string> CreateMacroDefinitions(string scriptName)
	{
		return new Dictionary<string, string>
			{
				{"__NAME__", scriptName}
			};
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
				Debug.LogError("Could not create snippet file because a file already exists at path '" + filePath + "'.");
			}
		}
		return found;
	}

	#endregion

	#region Create Snippet

	private static void CreateSnippet(SnippetGroup snippetGroup)
	{
		var path = EditorUtility.SaveFilePanel("Give a name to your script", AssetTools.GetSelectedPathOrAssetRootPath(), "", snippetGroup.MainFileExtension);

		if (string.IsNullOrEmpty(path))
			return;

		var scriptName = Path.GetFileNameWithoutExtension(path);
		var baseDirectory = Path.GetDirectoryName(path);
		var macroDefinitions = CreateMacroDefinitions(scriptName);
		var snippetFilePaths = snippetGroup.GetAllSnippetFilePaths(baseDirectory, macroDefinitions);

		if (CheckIfAnySnippetFileAlreadyExists(snippetFilePaths))
			return;

		snippetGroup.CreateAllSnippetFiles(baseDirectory, macroDefinitions);

		AssetDatabase.Refresh();
	}

	#endregion

	#region Menu Commands

	[MenuItem("Snippets/Create Script with Inspector")]
	public static void CreateScriptWithInspector()
	{
		CreateSnippet(ScriptWithInspectorGroup);
	}

	[MenuItem("Snippets/Create Script")]
	public static void CreateScript()
	{
		CreateSnippet(ScriptOnlyGroup);
	}

	[MenuItem("Snippets/Create Inspector")]
	public static void CreateInspector()
	{
		CreateSnippet(InspectorOnlyGroup);
	}

	#endregion
}
