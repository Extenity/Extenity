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

		private static readonly Vector2 MinimumWindowSize = new Vector2(200f, 50f);

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
		private static readonly GUILayoutOption InspectButtonHeight = GUILayout.Height(50);
		private static readonly GUILayoutOption InspectButtonWidth = GUILayout.Width(200);

		private bool ToggleInspect_UnexpectedCharacters = true;
		private bool ToggleInspect_YieldAllocations = true;
		private bool ToggleInspect_OnGUIUsage = true;

		protected override void OnGUIDerived()
		{
			GUILayout.Space(8f);

			// Control panel
			{
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Inspect Scripts", InspectButtonHeight, InspectButtonWidth))
				{
					EditorApplication.delayCall += Inspect;
				}
				GUILayout.BeginVertical();
				ToggleInspect_UnexpectedCharacters = GUILayout.Toggle(ToggleInspect_UnexpectedCharacters, "Unexpected Characters");
				ToggleInspect_YieldAllocations = GUILayout.Toggle(ToggleInspect_YieldAllocations, "Yield Allocations");
				ToggleInspect_OnGUIUsage = GUILayout.Toggle(ToggleInspect_OnGUIUsage, "OnGUI Usage");
				GUILayout.EndVertical();
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

						GUILayout.Label(result.ScriptPathGUIContent);
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
						GUILayout.BeginVertical();
						GUILayout.Label("Yield allocations: ");

						foreach (var entry in result.YieldAllocations)
						{
							GUILayout.BeginHorizontal();
							if (GUILayout.Button("Go", ButtonWidth))
							{
								Log.Info($"Opening script '{result.ScriptPath}' at line '{entry.Line}'.");
								AssetTools.OpenScriptInIDE(result.ScriptPath, entry.Line);
							}
							GUILayout.Label(entry.GUIContent);
							GUILayout.EndHorizontal();
						}

						GUILayout.EndVertical();
					}

					// OnGUI usages
					if (result.OnGUIUsages.IsNotNullAndEmpty())
					{
						GUILayout.BeginVertical();
						GUILayout.Label("OnGUI usages: ");

						foreach (var entry in result.OnGUIUsages)
						{
							GUILayout.BeginHorizontal();
							if (GUILayout.Button("Go", ButtonWidth))
							{
								Log.Info($"Opening script '{result.ScriptPath}' at line '{entry.Line}'.");
								AssetTools.OpenScriptInIDE(result.ScriptPath, entry.Line);
							}
							GUILayout.Label(entry.GUIContent);
							GUILayout.EndHorizontal();
						}

						GUILayout.EndVertical();
					}

					GUILayout.EndVertical();

					GUILayout.Space(30f);
				}

				GUILayout.EndScrollView();
			}
		}

		#endregion

		#region Inspect Scripts

		public class InspectionResult
		{
			public struct ScriptLineEntry
			{
				public int Line;
				public string Content;
				public GUIContent GUIContent;
			}

			public string ScriptPath;
			public HashSet<char> UnexpectedCharacters;
			public ScriptLineEntry[] YieldAllocations;
			public ScriptLineEntry[] OnGUIUsages;

			public GUIContent ScriptPathGUIContent;
		}

		public List<InspectionResult> Results;

		private static Regex OnGUIUsageRegex;
		private static Regex YieldAllocationsRegex;

		public void Inspect()
		{
			Results = new List<InspectionResult>();

			var scriptPaths = AssetTools.GetAllScriptAssetPaths();
			foreach (var scriptPath in scriptPaths)
			{
				InternalInspectScript(scriptPath,
					ToggleInspect_UnexpectedCharacters,
					ToggleInspect_YieldAllocations,
					ToggleInspect_OnGUIUsage);
			}

			SortResults();
			Repaint();
		}

		private void InternalInspectScript(string scriptPath, bool inspectUnexpectedCharacters, bool inspectYieldAllocations, bool inspectOnGUIUsage)
		{
			if (string.IsNullOrEmpty(scriptPath))
				throw new ArgumentNullException(nameof(scriptPath));

			var isTestScript = Path.GetFileName(scriptPath).StartsWith(TestScriptFileNamePrefix, StringComparison.InvariantCultureIgnoreCase);
			var isInEditorFolder = scriptPath.FixDirectorySeparatorChars('/').Split('/').Any(item => item.Equals("Editor", StringComparison.InvariantCultureIgnoreCase));
			// Skip editor scripts and test scripts. Currently there are no inspections that is meaningful to be applied on these scripts.
			if (isInEditorFolder || isTestScript)
				return;

			var lines = File.ReadLines(scriptPath).ToList();
			var anyErrors = false;

			// Trim all lines
			for (var i = 0; i < lines.Count; i++)
			{
				lines[i] = lines[i].Trim();
			}

			// Check if the script contains any unexpected characters
			HashSet<char> unexpectedCharacters = null;
			if (inspectUnexpectedCharacters)
			{
				for (int iLine = 0; iLine < lines.Count; iLine++)
				{
					var line = lines[iLine];
					if (IsLineAllowedToBeInspected(line))
					{
						for (int i = 0; i < line.Length; i++)
						{
							if (ExpectedCharacters.IndexOf(line[i]) < 0)
							{
								if (unexpectedCharacters == null)
									unexpectedCharacters = new HashSet<char>();
								unexpectedCharacters.Add(line[i]);
								anyErrors = true;
							}
						}
					}
				}
			}

			// Check if the script contains yield allocations
			List<InspectionResult.ScriptLineEntry> yieldAllocations = null;
			if (inspectYieldAllocations)
			{
				if (YieldAllocationsRegex == null)
					YieldAllocationsRegex = new Regex(@"yield\s+return\s+new", RegexOptions.Compiled);
				for (int iLine = 0; iLine < lines.Count; iLine++)
				{
					var line = lines[iLine];
					if (YieldAllocationsRegex.IsMatch(line) && IsLineAllowedToBeInspected(line))
					{
						if (yieldAllocations == null)
							yieldAllocations = new List<InspectionResult.ScriptLineEntry>();
						var lineNumber = iLine + 1;
						yieldAllocations.Add(new InspectionResult.ScriptLineEntry
						{
							Line = lineNumber,
							Content = line,
							GUIContent = new GUIContent($"{lineNumber}\t: {line}".ClipIfNecessary(100)),
						});
						anyErrors = true;
					}
				}
			}

			// Check if the script contains OnGUI
			List<InspectionResult.ScriptLineEntry> onGUIUsages = null;
			if (inspectOnGUIUsage)
			{
				if (OnGUIUsageRegex == null)
					OnGUIUsageRegex = new Regex(@"OnGUI\s*\(\s*\)", RegexOptions.Compiled);
				for (int iLine = 0; iLine < lines.Count; iLine++)
				{
					var line = lines[iLine];
					if (OnGUIUsageRegex.IsMatch(line) && IsLineAllowedToBeInspected(line))
					{
						if (onGUIUsages == null)
							onGUIUsages = new List<InspectionResult.ScriptLineEntry>();
						var lineNumber = iLine + 1;
						onGUIUsages.Add(new InspectionResult.ScriptLineEntry
						{
							Line = lineNumber,
							Content = line,
							GUIContent = new GUIContent($"{lineNumber}\t: {line}".ClipIfNecessary(100)),
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
					ScriptPathGUIContent = new GUIContent(scriptPath),
				};
				Results.Add(result);
			}
		}

		public void SortResults()
		{
			if (Results.IsNullOrEmpty())
				return;

			Results.Sort((a, b) => string.Compare(a.ScriptPath, b.ScriptPath, StringComparison.Ordinal));
		}

		#endregion

		#region Ignore Inspection

		private bool IsLineAllowedToBeInspected(string line)
		{
			return
				!line.Contains(IgnoreInspectionText, StringComparison.InvariantCultureIgnoreCase) && // Does not contain the ignore label that is entered by the coder
				line[0] != '/'; // Does not start with comment
		}

		#endregion
	}

}
