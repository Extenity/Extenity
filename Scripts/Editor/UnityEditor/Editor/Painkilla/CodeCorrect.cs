using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Extenity.ApplicationToolbox;
using Extenity.AssetToolbox.Editor;
using Extenity.DataToolbox;
using Extenity.IMGUIToolbox.Editor;
using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Extenity.PainkillaTool.Editor
{

	// TODO: Add tool to check script encoding

	public class CodeCorrect : ExtenityEditorWindowBase
	{
		#region Configuration

		public static readonly string ExpectedCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789`~!@#$%^&*()_-+=[]{}\\/,.:;|?<>'\"\t\n\r ";
		private const string IgnoreInspectionText = "Ignored by Code Correct";
		private const string TestScriptFileNamePrefix = "Test_";

		private static readonly Vector2 MinimumWindowSize = new Vector2(500f, 60f);

		#endregion

		#region Initialization

		[MenuItem("Tools/Code Correct")]
		private static void ShowWindow()
		{
			var window = GetWindow<CodeCorrect>();
			window.Show();
		}

		private void OnEnable()
		{
			SetTitleAndIcon("Code Correct", null);
			minSize = MinimumWindowSize;
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

		private void InspectByUserClick()
		{
			Results = Inspect(new InspectionConfiguration
			{
				InspectUnexpectedCharacters = ToggleInspect_UnexpectedCharacters,
				InspectYieldAllocations = ToggleInspect_YieldAllocations,
				InspectOnGUIUsage = ToggleInspect_OnGUIUsage,
				InspectOnMouseUsage = ToggleInspect_OnMouseUsage,
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
							AssetTools.OpenScriptInIDE(result.ScriptPath);
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

					GUILayout.EndVertical();

					GUILayout.Space(30f);
				}

				GUILayout.EndScrollView();
			}
		}

		private void DrawEntryForLinedScripts(string title, InspectionResult.ScriptLineEntry[] entries, string scriptPath)
		{
			GUILayout.BeginVertical();
			GUILayout.Label(title);

			foreach (var entry in entries)
			{
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Go", ButtonWidth))
				{
					AssetTools.OpenScriptInIDE(scriptPath, entry.Line);
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

			public string ScriptPath;
			public HashSet<char> UnexpectedCharacters;
			public ScriptLineEntry[] YieldAllocations;
			public ScriptLineEntry[] OnGUIUsages;
			public ScriptLineEntry[] OnMouseUsages;
		}

		private static Regex YieldAllocationsRegex;
		private static Regex OnGUIUsageRegex;
		private static Regex OnMouseUsageRegex;

		public static List<InspectionResult> Inspect(InspectionConfiguration configuration)
		{
			var scriptPaths = AssetTools.GetAllScriptAssetPaths();
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
				AssetTools.DeleteCommentsInScriptAssetContents(lines);

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

			var anyErrors = false;

			// Check if the script contains any unexpected characters
			HashSet<char> unexpectedCharacters = null;
			if (configuration.InspectUnexpectedCharacters)
			{
				for (int i = 0; i < originalFileContent.Length; i++)
				{
					if (ExpectedCharacters.IndexOf(originalFileContent[i]) < 0)
					{
						if (unexpectedCharacters == null)
							unexpectedCharacters = new HashSet<char>();
						unexpectedCharacters.Add(originalFileContent[i]);
						anyErrors = true;
					}
				}
			}

			// Check if the script contains yield allocations
			List<InspectionResult.ScriptLineEntry> yieldAllocations = null;
			if (configuration.InspectYieldAllocations)
			{
				if (YieldAllocationsRegex == null)
					YieldAllocationsRegex = new Regex(@"yield\s+return\s+new", RegexOptions.Compiled);
				for (int iLine = 0; iLine < lines.Length; iLine++)
				{
					var line = lines[iLine];
					if (YieldAllocationsRegex.IsMatch(line))
					{
						if (yieldAllocations == null)
							yieldAllocations = new List<InspectionResult.ScriptLineEntry>();
						var lineNumber = iLine + 1;
						yieldAllocations.Add(new InspectionResult.ScriptLineEntry
						{
							Line = lineNumber,
							Content = line,
						});
						anyErrors = true;
					}
				}
			}

			// Check if the script contains OnGUI methods
			List<InspectionResult.ScriptLineEntry> onGUIUsages = null;
			if (configuration.InspectOnGUIUsage)
			{
				if (OnGUIUsageRegex == null)
					OnGUIUsageRegex = new Regex(@"OnGUI\s*\(\s*\)", RegexOptions.Compiled);
				for (int iLine = 0; iLine < lines.Length; iLine++)
				{
					var line = lines[iLine];
					if (OnGUIUsageRegex.IsMatch(line))
					{
						if (onGUIUsages == null)
							onGUIUsages = new List<InspectionResult.ScriptLineEntry>();
						var lineNumber = iLine + 1;
						onGUIUsages.Add(new InspectionResult.ScriptLineEntry
						{
							Line = lineNumber,
							Content = line,
						});
						anyErrors = true;
					}
				}
			}

			// Check if the script contains OnMouse_ methods
			List<InspectionResult.ScriptLineEntry> onMouseUsages = null;
			if (configuration.InspectOnMouseUsage)
			{
				if (OnMouseUsageRegex == null)
					OnMouseUsageRegex = new Regex(@"OnMouse.*\(\s*\)", RegexOptions.Compiled);
				for (int iLine = 0; iLine < lines.Length; iLine++)
				{
					var line = lines[iLine];
					if (OnMouseUsageRegex.IsMatch(line))
					{
						if (onMouseUsages == null)
							onMouseUsages = new List<InspectionResult.ScriptLineEntry>();
						var lineNumber = iLine + 1;
						onMouseUsages.Add(new InspectionResult.ScriptLineEntry
						{
							Line = lineNumber,
							Content = line,
						});
						anyErrors = true;
					}
				}
			}

			if (anyErrors)
			{
				var result = new InspectionResult
				{
					ScriptPath = scriptPath,
					UnexpectedCharacters = unexpectedCharacters,
					YieldAllocations = yieldAllocations?.ToArray(),
					OnGUIUsages = onGUIUsages?.ToArray(),
					OnMouseUsages = onMouseUsages?.ToArray(),
				};
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
	}

}
