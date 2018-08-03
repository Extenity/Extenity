using System;
using Extenity.DataToolbox;
using UnityEditor;
using UnityEngine;

namespace Extenity.IMGUIToolbox.Editor
{

	public static class EditorGUILayoutTools
	{
		#region GUI Components - Progress Bar

		public static void ProgressBar(float value, string inlineText)
		{
			ProgressBar(null, value, inlineText);
		}

		public static void ProgressBar(string title, float value, string inlineText)
		{
			if (!string.IsNullOrEmpty(title))
			{
				GUILayout.Label(title);
			}

			EditorGUILayout.BeginHorizontal();
			var rect = EditorGUILayout.BeginVertical();
			EditorGUI.ProgressBar(rect, value, inlineText);
			GUILayout.Space(16);
			EditorGUILayout.EndVertical();
			GUILayout.Label(("% " + (int)(value * 100f)).ToString(), GUILayout.Width(40f));
			EditorGUILayout.EndHorizontal();
		}

		public static void ProgressBar(float value)
		{
			ProgressBar(null, value, null);
		}

		public static void ProgressBar(string title, float value)
		{
			ProgressBar(title, value, null);
		}

		#endregion

		#region GUI Components - PopupWithInputField

		public static bool PopupWithInputField(string[] displayedOptions, ref int popupIndex, ref string value, GUILayoutOption[] textFieldOptions = null, GUILayoutOption[] popupOptions = null)
		{
			EditorGUILayout.BeginHorizontal();

			var changed = false;

			EditorGUI.BeginChangeCheck();
			value = EditorGUILayout.TextField(value, textFieldOptions);
			if (EditorGUI.EndChangeCheck())
			{
				// User has changed the text of input field. Figure out what to do with popup.
				changed = true;

				// First check for exact matching value in popup list.
				popupIndex = displayedOptions.IndexOf(value);

				// Check for case invariant matching if the exact value is not found.
				if (popupIndex < 0)
				{
					popupIndex = displayedOptions.IndexOf(value, StringComparison.InvariantCultureIgnoreCase);
					if (popupIndex >= 0)
					{
						// User has entered an entry in popup with incorrect casing. Correct the casing of the value.
						value = displayedOptions[popupIndex];
					}
				}
			}

			EditorGUI.BeginChangeCheck();
			popupIndex = EditorGUILayout.Popup(popupIndex, displayedOptions, popupOptions);
			if (EditorGUI.EndChangeCheck())
			{
				// User has changed the selection in popup. Update the value as well.
				changed = true;
				value = displayedOptions[popupIndex];
			}

			EditorGUILayout.EndHorizontal();
			return changed;
		}

		#endregion

		#region Header

		public static void DrawHeader(string header)
		{
			GUILayout.Space(8f);
			var position = GUILayoutUtility.GetRect(0, float.MaxValue, 16f, 16f);
			position = EditorGUI.IndentedRect(position);
			GUI.Label(position, header, EditorStyles.boldLabel);
		}

		#endregion

		#region Horizontal Line

		public static void DrawHorizontalLine()
		{
			EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
		}

		#endregion

		#region UnwrapParam

		// Copied from UnityEditor.dll/ModelImporterModelEditor.cs
		private static GUIContent UnwrapParamContent_AngleDistortion = new GUIContent("Angle Error", "Measured in percents. Angle error measures deviation of UV angles from geometry angles. Area error measures deviation of UV triangles area from geometry triangles if they were uniformly scaled.");
		private static GUIContent UnwrapParamContent_AreaDistortion = new GUIContent("Area Error", "Measured in percents. Angle error measures deviation of UV angles from geometry angles. Area error measures deviation of UV triangles area from geometry triangles if they were uniformly scaled.");
		private static GUIContent UnwrapParamContent_HardAngle = new GUIContent("Hard Angle", "Angle between neighbor triangles that will generate seam.");
		private static GUIContent UnwrapParamContent_PackMargin = new GUIContent("Pack Margin", "Measured in pixels, assuming mesh will cover an entire 1024x1024 lightmap.");

		public static UnwrapParam UnwrapParam(UnwrapParam unwrapParam, bool showResetButton)
		{
			unwrapParam.hardAngle = EditorGUILayout.IntSlider(UnwrapParamContent_HardAngle, Mathf.RoundToInt(unwrapParam.hardAngle), 0, 180);
			unwrapParam.packMargin = EditorGUILayout.IntSlider(UnwrapParamContent_PackMargin, Mathf.RoundToInt(unwrapParam.packMargin * 1024f), 1, 64) / 1024f;
			unwrapParam.angleError = EditorGUILayout.IntSlider(UnwrapParamContent_AngleDistortion, Mathf.RoundToInt(unwrapParam.angleError * 100f), 1, 75) / 100f;
			unwrapParam.areaError = EditorGUILayout.IntSlider(UnwrapParamContent_AreaDistortion, Mathf.RoundToInt(unwrapParam.areaError * 100f), 1, 75) / 100f;

			if (showResetButton)
			{
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Reset", GUILayoutTools.DontExpandWidth, GUILayout.Height(16f)))
				{
					UnityEditor.UnwrapParam.SetDefaults(out unwrapParam);
				}
				GUILayout.EndHorizontal();
			}
			return unwrapParam;
		}

		#endregion

		#region Search Bar

		private static GUIStyle _Style_ToolbarSeachTextField;
		public static GUIStyle Style_ToolbarSeachTextField
		{
			get
			{
				if (_Style_ToolbarSeachTextField == null)
					_Style_ToolbarSeachTextField = GUI.skin.FindStyle("ToolbarSeachTextField");
				return _Style_ToolbarSeachTextField;
			}
		}

		private static GUIStyle _Style_Toolbar;
		public static GUIStyle Style_Toolbar
		{
			get
			{
				if (_Style_Toolbar == null)
					_Style_Toolbar = GUI.skin.FindStyle("Toolbar");
				return _Style_Toolbar;
			}
		}

		private static GUIStyle _Style_ToolbarSeachCancelButton;
		public static GUIStyle Style_ToolbarSeachCancelButton
		{
			get
			{
				if (_Style_ToolbarSeachCancelButton == null)
					_Style_ToolbarSeachCancelButton = GUI.skin.FindStyle("ToolbarSeachCancelButton");
				return _Style_ToolbarSeachCancelButton;
			}
		}

		/// <summary>
		/// Draws a search bar with cancel button just like Unity's search bars.
		///
		/// Returns true if search input changes so that any search operation may easily be triggered by just checking the return value.
		/// </summary>
		public static bool SearchBar(ref string searchInput)
		{
			GUILayout.BeginHorizontal(Style_Toolbar, GUILayoutTools.ExpandWidth);
			var newSearchInput = GUILayout.TextField(searchInput, Style_ToolbarSeachTextField, GUILayoutTools.ExpandWidth);
			if (GUILayout.Button(GUIContent.none, Style_ToolbarSeachCancelButton))
			{
				newSearchInput = "";
				GUI.FocusControl(null);
			}
			GUILayout.EndHorizontal();

			var changed = newSearchInput != searchInput;
			searchInput = newSearchInput;
			return changed;
		}

		#endregion
	}

}
