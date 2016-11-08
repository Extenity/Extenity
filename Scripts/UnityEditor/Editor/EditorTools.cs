using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor.SceneManagement;

namespace Extenity.EditorUtilities
{

	public static class EditorTools
	{
		#region Initialization

		static EditorTools()
		{
			InitializeWindowDock();
		}

		#endregion

		#region File/Directory Delete

		public static void DeleteMetaFileAndItem(string path)
		{
			if (Directory.Exists(path))
			{
				Directory.Delete(path, true);
				DeleteMetaFileOfItem(path);
			}
			else if (File.Exists(path))
			{
				File.Delete(path);
				DeleteMetaFileOfItem(path);
			}
			else
			{
				Debug.LogError("Tried to delete file or directory at path '" + path + "' but item cannot be found.");
			}
		}

		public static void DeleteMetaFileOfItem(string path)
		{
			var metaFile = path + ".meta";
			if (File.Exists(metaFile))
				File.Delete(metaFile);
		}

		#endregion

		#region Load Scene

		public static void LoadSceneInEditorByPath(string scenePath)
		{
			if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
				EditorSceneManager.OpenScene(scenePath);
		}

		public static void LoadSceneInEditorByName(string sceneName)
		{
			if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
				EditorSceneManager.OpenScene(GetScenePathFromBuildSettings(sceneName, false));
		}

		#endregion

		#region Get Scene Names From Build Settings

		public static string[] GetSceneNamesFromBuildSettings(List<string> excludingNames = null)
		{
			var list = new List<string>();

			for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
			{
				var scene = EditorBuildSettings.scenes[i];

				if (scene.enabled)
				{
					string name = scene.path.Substring(scene.path.LastIndexOf('/') + 1);
					name = name.Substring(0, name.Length - 6);

					if (excludingNames != null)
					{
						if (excludingNames.Contains(name))
							continue;
					}

					list.Add(name);
				}
			}

			return list.ToArray();
		}

		public static string[] GetScenePathsFromBuildSettings(List<string> excludingPaths = null)
		{
			var list = new List<string>();

			for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
			{
				var scene = EditorBuildSettings.scenes[i];

				if (scene.enabled)
				{
					if (excludingPaths != null)
					{
						if (excludingPaths.Contains(scene.path))
							continue;
					}

					list.Add(scene.path);
				}
			}

			return list.ToArray();
		}

		public static string GetScenePathFromBuildSettings(string sceneName, bool onlyIfEnabled)
		{
			if (string.IsNullOrEmpty(sceneName))
				return null;

			for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
			{
				var scene = EditorBuildSettings.scenes[i];

				if (!onlyIfEnabled || scene.enabled)
				{
					string name = scene.path.Substring(scene.path.LastIndexOf('/') + 1);
					name = name.Substring(0, name.Length - 6);

					if (name == sceneName)
					{
						return scene.path;
					}
				}
			}
			return null;
		}

		#endregion

		#region Enable/Disable Auto Refresh

		public static bool IsAutoRefreshEnabled
		{
			get
			{
				return EditorPrefs.GetBool("kAutoRefresh");
			}
		}

		public static void EnableAutoRefresh(bool enabled)
		{
			EditorPrefs.SetBool("kAutoRefresh", enabled);
		}

		public static void EnableAutoRefresh()
		{
			EditorPrefs.SetBool("kAutoRefresh", true);
		}

		public static void DisableAutoRefresh()
		{
			EditorPrefs.SetBool("kAutoRefresh", false);
		}

		public static void ToggleAutoRefresh()
		{
			if (IsAutoRefreshEnabled)
			{
				DisableAutoRefresh();
			}
			else
			{
				EnableAutoRefresh();
			}
		}

		#endregion

		#region Layout Options

		public static class LayoutOptions
		{
			public static readonly GUILayoutOption[] DontExpand = { GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false) };
		}

		#endregion

		#region Tags

		public class TagsPane
		{
			public int EditingIndex = -1;
			public string PreviousTagValueBeforeEditing = "";
			public bool NeedsRepaint = false;
			public bool NeedsEditingFocus = false;

			internal List<int> LineBreaks = new List<int>();
		}

		private static class TagPaneThings
		{
			public static readonly float ButtonSize = 20f;
			public static readonly float Separator = 5f;
			public static readonly float LineSeparator = 5f;

			public static readonly float BackgroundPadding = 3f;
			public static readonly float BackgroundDoublePadding = BackgroundPadding * 2f;
			public static readonly float MinimumLabelWidth = 40f;

			public static GUIStyle TagBackgroundStyle;
			public static GUIStyle TagEditingBackgroundStyle;
			public static GUIStyle TagLabelStyle;
			public static GUILayoutOption[] TagEditingPlusButtonLayoutOptions;

			public static bool IsTagRenderingInitialized = false;
		}

		private static void InitializeTagRendering()
		{
			if (TagPaneThings.IsTagRenderingInitialized)
				return;
			TagPaneThings.IsTagRenderingInitialized = true;

			TagPaneThings.TagBackgroundStyle = new GUIStyle(GUI.skin.box);
			TagPaneThings.TagEditingBackgroundStyle = new GUIStyle(GUI.skin.box);
			var tintedBackground = TagPaneThings.TagBackgroundStyle.normal.background.CopyTextureAsReadable();
			tintedBackground = TextureTools.Tint(tintedBackground, new Color(0.8f, 0.7f, 0.7f, 1f));
			TagPaneThings.TagEditingBackgroundStyle.normal.background = tintedBackground;
			TagPaneThings.TagLabelStyle = new GUIStyle(GUI.skin.label);
			TagPaneThings.TagEditingPlusButtonLayoutOptions = new[] { GUILayout.Width(TagPaneThings.ButtonSize), GUILayout.Height(TagPaneThings.ButtonSize) };
		}

		public static string[] DrawTags(string[] tags, TagsPane tagsPane, float maxWidth)
		{
			InitializeTagRendering();

			if (tags == null)
			{
				tags = new string[0];
			}

			GUILayout.BeginVertical(GUILayout.MaxWidth(maxWidth));
			GUILayout.BeginHorizontal();

			bool stopEditing = false;
			bool revertTag = false;
			int changeEditingTo = -1;
			int delayedRemoveAt = -1;

			// Draw add button
			{
				if (GUILayout.Button("+", TagPaneThings.TagEditingPlusButtonLayoutOptions))
				{
					// Ignore if related button is already empty
					if (tags.Length > 0 && string.IsNullOrEmpty(tags[0]))
					{
						// Ignored
					}
					else
					{
						tags = tags.InsertAt(0);
					}

					changeEditingTo = 0;
				}
			}

			// Calculate layout
			if (Event.current.type == EventType.Layout)
			{
				tagsPane.LineBreaks.Clear();
				float currentLineWidth = 0f;
				currentLineWidth += TagPaneThings.ButtonSize; // "add" button

				for (int i = 0; i < tags.Length; i++)
				{
					var labelWidth = _CalculateLabelWidth(new GUIContent(tags[i]));
					var totalWidth = _CalculateTagBackgroundTotalWidth(labelWidth);

					// New line if required
					var startedNewLine = false;
					if (currentLineWidth + totalWidth >= maxWidth)
					{
						tagsPane.LineBreaks.Add(i);
						startedNewLine = true;
						currentLineWidth = 0f;
					}

					// Draw background
					currentLineWidth += totalWidth;

					// Add separator
					if (i != tags.Length - 1 && !startedNewLine)
					{
						currentLineWidth += TagPaneThings.Separator;
					}
				}
			}

			// Draw tags
			if (tags.Length > 0)
			{
				for (int i = 0; i < tags.Length; i++)
				{
					var tag = tags[i];

					// Draw tag
					{
						var labelContent = new GUIContent(tag);
						var labelWidth = _CalculateLabelWidth(labelContent);
						var labelHeight = TagPaneThings.TagLabelStyle.CalcHeight(labelContent, labelWidth);
						var totalWidth = _CalculateTagBackgroundTotalWidth(labelWidth);
						var totalHeight = Mathf.Max(TagPaneThings.ButtonSize + TagPaneThings.BackgroundDoublePadding, labelHeight);

						// New line if required
						var startedNewLine = false;
						{
							if (tagsPane.LineBreaks.Contains(i)) // Not the best way but whatever
							{
								GUILayout.EndHorizontal();
								GUILayout.Space(TagPaneThings.LineSeparator);
								GUILayout.BeginHorizontal();
							}
						}

						var backgroundRect = GUILayoutUtility.GetRect(totalWidth, totalHeight, LayoutOptions.DontExpand);
						var labelRect = backgroundRect;
						labelRect.xMin += TagPaneThings.BackgroundPadding; // Background + left padding
						labelRect.yMin = backgroundRect.yMin + (backgroundRect.height - labelHeight) / 2f; // Center vertically
						labelRect.width -= TagPaneThings.BackgroundDoublePadding + TagPaneThings.ButtonSize; // Backround - close button - left and right paddings
						labelRect.height = labelHeight; // Only the text's height
						var labelClickArea = backgroundRect;
						labelClickArea.width -= TagPaneThings.BackgroundPadding + TagPaneThings.ButtonSize; // Backround - close button - right padding

						if (tagsPane.EditingIndex == i)
						{
							// Draw background
							GUI.Box(backgroundRect, "", TagPaneThings.TagEditingBackgroundStyle);

							// Draw tag editing text filed
							GUI.SetNextControlName("TagEditingTextField");
							tags[i] = GUI.TextField(labelRect, tag == null ? "" : tag);
							if (tagsPane.NeedsEditingFocus)
							{
								tagsPane.NeedsEditingFocus = false;
								GUI.FocusControl("TagEditingTextField");
							}

							if ((Event.current.type == EventType.MouseUp || Event.current.type == EventType.MouseDown) && !labelClickArea.Contains(Event.current.mousePosition))
							{
								stopEditing = true;
							}
							if (Event.current.isKey)
							{
								if (Event.current.keyCode == KeyCode.Return)
								{
									stopEditing = true;
								}
								else if (Event.current.keyCode == KeyCode.Escape)
								{
									stopEditing = true;
									revertTag = true;
								}
							}
						}
						else
						{
							// Draw background
							GUI.Box(backgroundRect, "", TagPaneThings.TagBackgroundStyle);

							// Detect left click
							if (Event.current.isMouse && Event.current.type == EventType.MouseUp && Event.current.button == 0 && labelClickArea.Contains(Event.current.mousePosition))
							{
								changeEditingTo = i;
							}

							// Draw tag
							GUI.Label(labelRect, tag);
						}

						// Draw remove button
						var removeButtonRect = backgroundRect;
						removeButtonRect.xMin = backgroundRect.xMax - TagPaneThings.BackgroundPadding - TagPaneThings.ButtonSize;
						removeButtonRect.yMin = backgroundRect.yMin + TagPaneThings.BackgroundPadding;
						removeButtonRect.width = TagPaneThings.ButtonSize;
						removeButtonRect.height = TagPaneThings.ButtonSize;
						if (GUI.Button(removeButtonRect, "X"))
						{
							delayedRemoveAt = i;
						}

						// Add separator
						if (i != tags.Length - 1 && !startedNewLine)
						{
							GUILayout.Space(TagPaneThings.Separator);
						}
					}
				}
			}

			if (revertTag)
			{
				tags[tagsPane.EditingIndex] = tagsPane.PreviousTagValueBeforeEditing;
			}
			if (changeEditingTo >= 0)
			{
				tagsPane.EditingIndex = changeEditingTo;
				tagsPane.PreviousTagValueBeforeEditing = tags[changeEditingTo];
				tagsPane.NeedsRepaint = true;
				tagsPane.NeedsEditingFocus = true;
			}
			else if (stopEditing)
			{
				tagsPane.EditingIndex = -1;
				tagsPane.PreviousTagValueBeforeEditing = "";
				tagsPane.NeedsRepaint = true;
				GUI.FocusControl(null);
			}

			if (delayedRemoveAt >= 0)
			{
				tags = tags.RemoveAt(delayedRemoveAt);
			}

			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			return tags;
		}

		private static float _CalculateLabelWidth(GUIContent labelContent)
		{
			float labelMinWidth, labelMaxWidth;
			TagPaneThings.TagLabelStyle.CalcMinMaxWidth(labelContent, out labelMinWidth, out labelMaxWidth);
			labelMaxWidth += 6f; // Add a couple of pixels to get rid of silly clamping at the end
			return Mathf.Max(TagPaneThings.MinimumLabelWidth, labelMaxWidth);
		}

		private static float _CalculateTagBackgroundTotalWidth(float labelWidth)
		{
			return labelWidth + TagPaneThings.ButtonSize + TagPaneThings.BackgroundDoublePadding;
		}

		#endregion

		#region Window Dock

		static PropertyInfo DockedPropertyInfo;

		private static void InitializeWindowDock()
		{
			DockedPropertyInfo = typeof(EditorWindow).GetProperty("docked", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.GetProperty);
		}

		public static bool IsDocked(this EditorWindow window)
		{
			var obj = DockedPropertyInfo.GetValue(window, null);
			return bool.Parse(obj.ToString());
		}

		#endregion
	}

}
