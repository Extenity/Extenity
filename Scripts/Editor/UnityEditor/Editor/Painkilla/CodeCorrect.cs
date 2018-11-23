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

	// TODO: Implement a tool that checks all runtime scripts for OnGUI methods and throws error if any exists. All OnGUI methods should be killed with fire.

	public class CodeCorrect : ExtenityEditorWindowBase
	{
		#region Configuration

		public static readonly string ExpectedCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789`~!@#$%^&*()_-+=[]{}\\/,.:;|?<>'\"\t\n\r ";

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
		private static readonly GUILayoutOption InspectButtonWidth = GUILayout.Width(200);

		private bool ToggleInspect_UnexpectedCharacters = true;
		private bool ToggleInspect_YieldAllocations = true;

		protected override void OnGUIDerived()
		{
			GUILayout.Space(8f);

			// Control panel
			{
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Inspect Scripts", BigButtonHeight, InspectButtonWidth))
				{
					EditorApplication.delayCall += Inspect;
				}
				GUILayout.BeginVertical();
				ToggleInspect_UnexpectedCharacters = GUILayout.Toggle(ToggleInspect_UnexpectedCharacters, "Unexpected Characters");
				ToggleInspect_YieldAllocations = GUILayout.Toggle(ToggleInspect_YieldAllocations, "Yield Allocations");
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
			public struct YieldAllocationEntry
			{
				public int Line;
				public string Content;
				public GUIContent GUIContent;
			}

			public string ScriptPath;
			public HashSet<char> UnexpectedCharacters;
			public YieldAllocationEntry[] YieldAllocations;

			public GUIContent ScriptPathGUIContent;
		}

		public List<InspectionResult> Results;

		public void Inspect()
		{
			Results = new List<InspectionResult>();

			var scriptPaths = AssetTools.GetAllScriptAssetPaths();
			foreach (var scriptPath in scriptPaths)
			{
				InternalInspectScript(scriptPath,
					ToggleInspect_UnexpectedCharacters,
					ToggleInspect_YieldAllocations);
			}

			SortResults();
			Repaint();
		}

		private void InternalInspectScript(string scriptPath, bool inspectUnexpectedCharacters, bool inspectYieldAllocations)
		{
			//// Check script encoding
			//{
			//}

			var lines = File.ReadLines(scriptPath).ToList();
			var anyErrors = false;

			// Check if the script contains any unexpected characters
			HashSet<char> unexpectedCharacters = null;
			if (inspectUnexpectedCharacters)
			{
				for (int iLine = 0; iLine < lines.Count; iLine++)
				{
					var line = lines[iLine];
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

			// Check if the script contains yield allocations
			List<InspectionResult.YieldAllocationEntry> yieldAllocations = null;
			if (inspectYieldAllocations)
			{
				var regex = new Regex(@"yield\s+return\s+new", RegexOptions.Compiled);
				for (int iLine = 0; iLine < lines.Count; iLine++)
				{
					var line = lines[iLine];
					if (regex.IsMatch(line))
					{
						if (yieldAllocations == null)
							yieldAllocations = new List<InspectionResult.YieldAllocationEntry>();
						var lineNumber = iLine + 1;
						var content = line.Trim();
						yieldAllocations.Add(new InspectionResult.YieldAllocationEntry
						{
							Line = lineNumber,
							Content = content,
							GUIContent = new GUIContent($"{lineNumber}\t: {content}".ClipIfNecessary(100)),
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
	}

}
