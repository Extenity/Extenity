using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Extenity.ApplicationToolbox;
using Extenity.AssetToolbox.Editor;
using Extenity.CompilationToolbox.Editor;
using Extenity.DataToolbox;
using Extenity.FileSystemToolbox;
using Extenity.IMGUIToolbox.Editor;
using Extenity.UnityEditorToolbox;
using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Extenity.PainkillerToolbox.Editor
{

	// TODO: Add tool to check script encoding
	// TODO: Improve ignore comments by defining which action(s) to ignore. So it will only exclude the intended check(s) and still catch other bad coding practices.
	// TODO: Detect empty methods (Awake, Start, OnDestroy, Update, FixedUpdate, LateUpdate)
	// TODO: Detect where Debug.Log_ methods are called.
	// TODO: Detect where Log._ methods with no context are called.
	// TODO: Detect OnValidate, OnDrawGizmos, OnDrawGizmosSelected methods that are not covered inside UNITY_EDITOR blocks.
	// TODO: Detect "EditorApplication.delayCall =" which should be "EditorApplication.delayCall +=".
	// TODO: Detect Destroy and DestroyImmediate calls that trying to destroy a Transform component. This is a Fatal bug and can't be ignored.
	// TODO: Detect "yield return new" usages that can be cached.
	// TODO: Detect closures. See https://docs.unity3d.com/Manual/BestPracticeUnderstandingPerformanceInUnity4-1.html
	// TODO: Detect boxing. See https://docs.unity3d.com/Manual/BestPracticeUnderstandingPerformanceInUnity4-1.html
	// TODO: Detect dictionaries that use enum as key. See https://docs.unity3d.com/Manual/BestPracticeUnderstandingPerformanceInUnity4-1.html
	// TODO: Detect foreach usages.
	// TODO: Detect Unity API usages that cause memory allocations (Input.touches, Mesh.vertices etc). Maybe try to detect if they are used inside loops.
	// TODO: Detect generating empty arrays where in places other than assigning them into static fields.
	// TODO: Detect static fields. (Maybe exclude fields that keeps empty arrays)
	// TODO: Make sure IPunObservable, IPunOwnershipCallbacks, IPunInstantiateMagicCallback, IConnectionCallbacks, ILobbyCallbacks, IMatchmakingCallbacks, IInRoomCallbacks, IOnEventCallback, IWebRpcCallback interfaces are only used in classes that derive from NetworkedBehaviour base class class.
	// TODO: Make sure methods marked with PunRPC attribute are only exist in classes that derive from NetworkedBehaviour base class.
	// TODO: Make sure InitializeNetworkedObjectInstantiation is the first line of every OnPhotonInstantiate(PhotonMessageInfo) method.
	// TODO: Make sure PreprocessRPC is checked in the first line of every PunRPC method and specified method name is correct.
	// TODO: Make sure PhotonView.RPC and RpcSecure are not used. NetworkedBehaviour.RPC and RpcSecure should be used instead.
	// TODO: Debug.ClearDeveloperConsole does not work. Use EditorDebugTools.ClearDeveloperConsole instead.
	// TODO: Make sure "using Boo." does not exist anywhere.

	public class CodeCorrect : ExtenityEditorWindowBase
	{
		#region Configuration

		public const string ExpectedCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789`~!@#$%^&*()_-+=[]{}\\/,.:;|?<>'\"\t\n\r ";
		private const string IgnoreInspectionText = "Ignored by Code Correct";
		private const string TestScriptFileNamePrefix = "Test_";

		protected override WindowSpecifications Specifications => new WindowSpecifications
		{
			Title = "Code Correct",
			MinimumWindowSize = new Vector2(500f, 60f),
		};

		#endregion

		#region Initialization

		[MenuItem(ExtenityMenu.Painkiller + "Code Correct", priority = ExtenityMenu.PainkillerPriority + 6)]
		private static void ToggleWindow()
		{
			EditorWindowTools.ToggleWindow<CodeCorrect>();
		}

		#endregion

		#region Deinitialization

		//protected void OnDestroy()
		//{
		//}

		#endregion

		#region GUI - Window

		private static readonly GUILayoutOption CharacterButtonWidth = GUILayout.Width(24);
		private static readonly GUILayoutOption ButtonWidth = GUILayout.Width(60);
		private static readonly GUILayoutOption InspectButtonHeight = GUILayout.Height(34);
		private static readonly GUILayoutOption InspectButtonWidth = GUILayout.Width(200);

		public List<InspectionResult> Results;

		public bool ToggleInspect_UnexpectedCharacters = true;
		public bool ToggleInspect_YieldAllocations = true;
		public bool ToggleInspect_OnGUIUsage = true;
		public bool ToggleInspect_OnMouseUsage = true;
		public bool ToggleInspect_DebugLogUsage = true;

		private void InspectByUserClick()
		{
			Results = Inspect(new InspectionConfiguration
			{
				InspectUnexpectedCharacters = ToggleInspect_UnexpectedCharacters,
				InspectYieldAllocations = ToggleInspect_YieldAllocations,
				InspectOnGUIUsage = ToggleInspect_OnGUIUsage,
				InspectOnMouseUsage = ToggleInspect_OnMouseUsage,
				InspectDebugLogUsage = ToggleInspect_DebugLogUsage,
			});

			Repaint();
		}

		protected override void OnGUIDerived()
		{
			GUILayout.Space(8f);

			// Control panel
			{
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Inspect Scripts", InspectButtonHeight, InspectButtonWidth))
				{
					EditorApplication.delayCall += InspectByUserClick;
				}
				GUILayout.BeginVertical();
				ToggleInspect_UnexpectedCharacters = GUILayout.Toggle(ToggleInspect_UnexpectedCharacters, "Unexpected Characters");
				ToggleInspect_YieldAllocations = GUILayout.Toggle(ToggleInspect_YieldAllocations, "Yield Allocations");
				GUILayout.EndVertical();
				GUILayout.BeginVertical();
				ToggleInspect_OnGUIUsage = GUILayout.Toggle(ToggleInspect_OnGUIUsage, "OnGUI Usage");
				ToggleInspect_OnMouseUsage = GUILayout.Toggle(ToggleInspect_OnMouseUsage, "OnMouse_ Usage");
				GUILayout.EndVertical();
				GUILayout.BeginVertical();
				ToggleInspect_DebugLogUsage = GUILayout.Toggle(ToggleInspect_DebugLogUsage, "Debug.Log_ Usage");
				GUILayout.EndVertical();
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}

			// Results
			if (Results != null)
			{
				EditorGUILayoutTools.DrawHeader($"Results ({Results.Count})");
				GUILayout.Space(10f);

				ScrollPosition = GUILayout.BeginScrollView(ScrollPosition);

				foreach (var result in Results)
				{
					GUILayout.BeginVertical();

					// Script path and Open button
					{
						GUILayout.BeginHorizontal();

						if (GUILayout.Button("Open", ButtonWidth))
						{
							AssetDatabaseTools.OpenScriptInIDE(result.ScriptPath);
						}

						GUILayout.Label(result.ScriptPath);
						GUILayout.EndHorizontal();
					}

					// Unexpected characters
					if (result.UnexpectedCharacters.IsNotNullAndEmpty())
					{
						GUILayout.BeginHorizontal();
						GUILayout.Label("Unexpected characters: ");

						foreach (var character in result.UnexpectedCharacters)
						{
							if (GUILayout.Button(character.ToString(), CharacterButtonWidth))
							{
								Log.Info($"Character '{character}' with unicode representation '\\u{((ulong)character).ToHexString()}' copied into clipboard.");
								Clipboard.SetClipboardText(character.ToString(), false);
							}
						}

						GUILayout.FlexibleSpace();
						GUILayout.EndHorizontal();
					}

					// Yield allocations
					if (result.YieldAllocations.IsNotNullAndEmpty())
					{
						DrawEntryForLinedScripts("Yield allocations: ", result.YieldAllocations, result.ScriptPath);
					}

					// OnGUI usages
					if (result.OnGUIUsages.IsNotNullAndEmpty())
					{
						DrawEntryForLinedScripts("OnGUI usages: ", result.OnGUIUsages, result.ScriptPath);
					}

					// OnMouse_ usages
					if (result.OnMouseUsages.IsNotNullAndEmpty())
					{
						DrawEntryForLinedScripts("OnMouse_ usages: ", result.OnMouseUsages, result.ScriptPath);
					}

					// Debug.Log_ usages
					if (result.DebugLogUsages.IsNotNullAndEmpty())
					{
						DrawEntryForLinedScripts("Debug.Log_ usages: ", result.DebugLogUsages, result.ScriptPath);
					}

					GUILayout.EndVertical();

					GUILayout.Space(30f);
				}

				GUILayout.EndScrollView();
			}
		}

		private void DrawEntryForLinedScripts(string title, List<InspectionResult.ScriptLineEntry> entries, string scriptPath)
		{
			GUILayout.BeginVertical();
			GUILayout.Label(title);

			foreach (var entry in entries)
			{
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Go", ButtonWidth))
				{
					AssetDatabaseTools.OpenScriptInIDE(scriptPath, entry.Line);
				}

				GUILayout.Label(entry.GUIContent);
				GUILayout.EndHorizontal();
			}

			GUILayout.EndVertical();
		}

		#endregion

		#region Inspect

		public struct InspectionConfiguration
		{
			public bool InspectUnexpectedCharacters;
			public bool InspectYieldAllocations;
			public bool InspectOnGUIUsage;
			public bool InspectOnMouseUsage;
			public bool InspectDebugLogUsage;
		}

		public class InspectionResult
		{
			public struct ScriptLineEntry
			{
				public int Line;
				public string Content;

				#region Cached GUIContent

				private GUIContent _GUIContent;
				public GUIContent GUIContent
				{
					get
					{
						if (_GUIContent == null)
							_GUIContent = new GUIContent($"{Line}\t: {Content}".ClipIfNecessary(100));
						return _GUIContent;
					}
				}

				#endregion
			}

			// Metadata
			public readonly string ScriptPath;

			// Inspections
			public HashSet<char> UnexpectedCharacters;
			public List<ScriptLineEntry> YieldAllocations;
			public List<ScriptLineEntry> OnGUIUsages;
			public List<ScriptLineEntry> OnMouseUsages;
			public List<ScriptLineEntry> DebugLogUsages;
			
			public bool AnyErrors
			{
				get
				{
					return
						UnexpectedCharacters != null ||
						YieldAllocations != null ||
						OnGUIUsages != null ||
						OnMouseUsages != null ||
						DebugLogUsages != null;
				}
			}

			#region Initialization

			public InspectionResult(string scriptPath)
			{
				ScriptPath = scriptPath;
			}

			#endregion
		}

		private static readonly Regex YieldAllocationsRegex = new Regex(@"yield\s+return\s+new", RegexOptions.Compiled);
		private static readonly Regex OnGUIUsageRegex = new Regex(@"OnGUI\s*\(\s*\)", RegexOptions.Compiled);
		private static readonly Regex OnMouseUsageRegex = new Regex(@"OnMouse.*\(", RegexOptions.Compiled);
		private static readonly Regex DebugLogUsageRegex = new Regex(@"Debug\s*\.\s*Log.*\(", RegexOptions.Compiled);

		public static List<InspectionResult> Inspect(InspectionConfiguration configuration)
		{
			var scriptPaths = AssetDatabaseTools.GetAllScriptAssetPathsOfType(ScriptType.Runtime);
			return Inspect(configuration, scriptPaths);
		}

		public static List<InspectionResult> Inspect(InspectionConfiguration configuration, List<string> scriptPaths)
		{
			var results = new List<InspectionResult>();

			foreach (var scriptPath in scriptPaths)
			{
				InternalInspectScript(results, scriptPath, configuration);
			}

			SortResults(results);

			return results;
		}

		private static void InternalInspectScript(List<InspectionResult> results, string scriptPath, InspectionConfiguration configuration)
		{
			if (string.IsNullOrEmpty(scriptPath))
				throw new ArgumentNullException(nameof(scriptPath));

			var isTestScript = Path.GetFileName(scriptPath).StartsWith(TestScriptFileNamePrefix, StringComparison.InvariantCultureIgnoreCase);
			var isInEditorFolder = scriptPath.FixDirectorySeparatorChars('/').Split('/').Any(item => item.Equals("Editor", StringComparison.InvariantCultureIgnoreCase));
			// Skip editor scripts and test scripts. Currently there are no inspections that is meaningful to be applied on these scripts.
			if (isInEditorFolder || isTestScript)
				return;

			// Read file. Remove comments. Remove ignored lines. Trim lines.
			List<int> ignoredLineIndices = null;
			string originalFileContent;
			string[] lines;
			try
			{
				originalFileContent = File.ReadAllText(scriptPath).NormalizeLineEndingsCRLF();
				lines = originalFileContent.Split(new[] { "\r\n" }, StringSplitOptions.None);

				// Before removing commented out parts, detect lines that contain the ignore label that is entered by the coder.
				for (var i = 0; i < lines.Length; i++)
				{
					if (lines[i].Contains(IgnoreInspectionText, StringComparison.InvariantCultureIgnoreCase))
					{
						if (ignoredLineIndices == null)
							ignoredLineIndices = new List<int>();
						ignoredLineIndices.Add(i);
					}
					// Also trim lines while doing that.
					lines[i] = lines[i].Trim();
				}

				// Delete commented out parts.
				AssetDatabaseTools.DeleteCommentsInScriptAssetContents(lines);

				// Delete lines marked as ignored by coder.
				if (ignoredLineIndices != null)
				{
					foreach (var ignoredLineIndex in ignoredLineIndices)
					{
						lines[ignoredLineIndex] = "";
					}
				}
			}
			catch (Exception exception)
			{
				throw new Exception("Failed to process script at path: " + scriptPath, exception);
			}

			var result = new InspectionResult(scriptPath);

			// Check if the script contains any unexpected characters
			if (configuration.InspectUnexpectedCharacters)
			{
				result.UnexpectedCharacters = SearchUnexpectedCharacters(originalFileContent);
			}

			// Check if the script contains yield allocations
			if (configuration.InspectYieldAllocations)
			{
				result.YieldAllocations = SearchScriptLineEntriesForRegexMatches(lines, YieldAllocationsRegex);
			}

			// Check if the script contains OnGUI methods
			if (configuration.InspectOnGUIUsage)
			{
				result.OnGUIUsages = SearchScriptLineEntriesForRegexMatches(lines, OnGUIUsageRegex);
			}

			// Check if the script contains OnMouse_ methods
			if (configuration.InspectOnMouseUsage)
			{
				result.OnMouseUsages = SearchScriptLineEntriesForRegexMatches(lines, OnMouseUsageRegex);
			}

			// Check if the script contains Debug.Log_ methods
			if (configuration.InspectDebugLogUsage)
			{
				result.DebugLogUsages = SearchScriptLineEntriesForRegexMatches(lines, DebugLogUsageRegex);
			}

			if (result.AnyErrors)
			{
				results.Add(result);
			}
		}

		private static void SortResults(List<InspectionResult> results)
		{
			if (results.IsNullOrEmpty())
				return;

			results.Sort((a, b) => string.Compare(a.ScriptPath, b.ScriptPath, StringComparison.Ordinal));
		}

		#endregion

		#region Inspection Methods

		private static HashSet<char> SearchUnexpectedCharacters(string originalFileContent)
		{
			HashSet<char> unexpectedCharacters = null;
			for (int i = 0; i < originalFileContent.Length; i++)
			{
				if (ExpectedCharacters.IndexOf(originalFileContent[i]) < 0)
				{
					if (unexpectedCharacters == null)
						unexpectedCharacters = new HashSet<char>();
					unexpectedCharacters.Add(originalFileContent[i]);
				}
			}
			return unexpectedCharacters;
		}

		private static List<InspectionResult.ScriptLineEntry> SearchScriptLineEntriesForRegexMatches(string[] lines, Regex regex)
		{
			List<InspectionResult.ScriptLineEntry> foundLines = null;
			for (int iLine = 0; iLine < lines.Length; iLine++)
			{
				var line = lines[iLine];
				if (regex.IsMatch(line))
				{
					if (foundLines == null)
						foundLines = new List<InspectionResult.ScriptLineEntry>();
					var lineNumber = iLine + 1;
					foundLines.Add(new InspectionResult.ScriptLineEntry
					{
						Line = lineNumber,
						Content = line,
					});
				}
			}
			return foundLines;
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(CodeCorrect));

		#endregion
	}

}
