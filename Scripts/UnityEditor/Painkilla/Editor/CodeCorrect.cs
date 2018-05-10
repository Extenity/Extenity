using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Extenity.AssetToolbox.Editor;
using Extenity.DataToolbox;
using Extenity.IMGUIToolbox.Editor;
using Extenity.OperatingSystemToolbox;
using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Extenity.PainkillaTool.Editor
{

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

		protected override void OnGUIDerived()
		{
			GUILayout.Space(8f);

			if (GUILayout.Button("Inspect Scripts", BigButtonHeight))
			{
				EditorApplication.delayCall += Inspect;
			}

			if (Results != null)
			{
				EditorGUILayoutTools.DrawHeader($"Results ({Results.Count})");

				ScrollPosition = GUILayout.BeginScrollView(ScrollPosition);

				foreach (var result in Results)
				{
					GUILayout.BeginHorizontal();

					if (GUILayout.Button("Open", ButtonWidth))
					{
						var obj = AssetDatabase.LoadAssetAtPath<MonoScript>(result.ScriptPath);
						AssetDatabase.OpenAsset(obj);
					}
					GUILayout.Label(result.GUIContent);
					GUILayout.FlexibleSpace();
					foreach (var character in result.UnexpectedCharacters)
					{
						if (GUILayout.Button(character.ToString(), CharacterButtonWidth))
						{
							Debug.Log($"Character '{character}' with unicode representation '\\u{((ulong)character).ToHexString()}' copied into clipboard.");
							Clipboard.SetClipboardText(character.ToString());
						}
					}

					GUILayout.EndHorizontal();
				}

				GUILayout.EndScrollView();
			}
		}

		#endregion

		#region Inspect Scripts

		public class InspectionResult
		{
			public string ScriptPath;
			public HashSet<char> UnexpectedCharacters;

			public GUIContent GUIContent;
		}

		public List<InspectionResult> Results;

		public void Inspect()
		{
			Results = new List<InspectionResult>();

			var scriptPaths = AssetTools.GetAllScriptAssetPaths();
			foreach (var scriptPath in scriptPaths)
			{
				InternalInspectScript(scriptPath);
			}

			SortResults();
			Repaint();
		}

		private void InternalInspectScript(string scriptPath)
		{
			//// Check script encoding
			//{
			//}

			// Check if the script contains any unexpected characters
			HashSet<char> unexpectedCharacters = null;
			{
				var content = File.ReadAllText(scriptPath);
				for (int i = 0; i < content.Length; i++)
				{
					if (!ExpectedCharacters.Contains(content[i]))
					{
						if (unexpectedCharacters == null)
							unexpectedCharacters = new HashSet<char>();
						unexpectedCharacters.Add(content[i]);
					}
				}
			}

			var anyErrors = unexpectedCharacters != null;

			if (anyErrors)
			{
				var result = new InspectionResult
				{
					ScriptPath = scriptPath,
					UnexpectedCharacters = unexpectedCharacters,
					GUIContent = new GUIContent(scriptPath, "Unexpected characters: " + string.Join(" ", unexpectedCharacters)),
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
