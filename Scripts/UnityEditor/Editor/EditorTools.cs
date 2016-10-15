using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using FluffyUnderware.DevTools.Extensions;
using UnityEditor.SceneManagement;

namespace Extenity.EditorUtilities
{

	public static class EditorTools
	{
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
		}

		private static GUIStyle TagBackgroundStyle;
		private static GUIStyle TagEditingBackgroundStyle;
		private static GUIStyle TagLabelStyle;

		private static bool _IsTagRenderingInitialized = false;

		private static void InitializeTagRendering()
		{
			if (_IsTagRenderingInitialized)
				return;
			_IsTagRenderingInitialized = true;

			TagBackgroundStyle = new GUIStyle(GUI.skin.box);
			TagEditingBackgroundStyle = new GUIStyle(GUI.skin.box);
			var tintedBackground = TagBackgroundStyle.normal.background.CopyTextureAsReadable();
			tintedBackground = TextureTools.Tint(tintedBackground, new Color(0.8f, 0.7f, 0.7f, 1f));
			TagEditingBackgroundStyle.normal.background = tintedBackground;
			TagLabelStyle = new GUIStyle(GUI.skin.label);
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

			const float buttonSize = 20f;
			bool stopEditing = false;
			bool revertTag = false;
			int changeEditingTo = -1;
			int delayedRemoveAt = -1;

			// Draw add button
			{
				if (GUILayout.Button("+", GUILayout.Width(buttonSize), GUILayout.Height(buttonSize)))
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

			// Draw tags
			if (tags.Length > 0)
			{
				for (int i = 0; i < tags.Length; i++)
				{
					var tag = tags[i];

					// Draw tag
					{
						const float margin = 3f;
						const float doubleMargin = margin * 2f;
						const float minimumLabelWidth = 40f;

						float labelMinWidth, labelMaxWidth, labelHeight;
						var labelContent = new GUIContent(tag);
						TagLabelStyle.CalcMinMaxWidth(labelContent, out labelMinWidth, out labelMaxWidth);
						labelMaxWidth += 6f; // Add a couple of pixels to get rid of silly clamping at the end
						labelHeight = TagLabelStyle.CalcHeight(labelContent, labelMaxWidth);
						var labelWidth = Mathf.Max(minimumLabelWidth, labelMaxWidth);
						var totalWidth = labelWidth + buttonSize + doubleMargin;
						var totalHeight = Mathf.Max(buttonSize + doubleMargin, labelHeight);
						var backgroundRect = GUILayoutUtility.GetRect(totalWidth, totalHeight, LayoutOptions.DontExpand);
						var labelRect = backgroundRect;
						labelRect.xMin += margin;
						labelRect.yMin = backgroundRect.yMin + (backgroundRect.height - labelHeight) / 2f;
						labelRect.width -= doubleMargin + buttonSize;
						labelRect.height = labelHeight;
						var labelClickArea = backgroundRect;
						labelClickArea.width -= margin + buttonSize;

						if (tagsPane.EditingIndex == i)
						{
							// Draw background
							GUI.Box(backgroundRect, "", TagEditingBackgroundStyle);

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
							GUI.Box(backgroundRect, "", TagBackgroundStyle);

							// Draw tag
							if (Event.current.isMouse && Event.current.type == EventType.MouseUp && Event.current.button == 0 && labelClickArea.Contains(Event.current.mousePosition))
							{
								changeEditingTo = i;
							}
							GUI.Label(labelRect, tag);
						}

						// Draw remove button
						var removeButtonRect = backgroundRect;
						removeButtonRect.xMin = backgroundRect.xMax - margin - buttonSize;
						removeButtonRect.yMin = backgroundRect.yMin + margin;
						removeButtonRect.width = buttonSize;
						removeButtonRect.height = buttonSize;
						if (GUI.Button(removeButtonRect, "X"))
						{
							delayedRemoveAt = i;
						}

						// Add separator
						if (i != tags.Length - 1)
						{
							GUILayout.Space(5f);
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

		#endregion
	}

}
