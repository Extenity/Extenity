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
				if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false), GUILayout.Height(16f)))
				{
					UnityEditor.UnwrapParam.SetDefaults(out unwrapParam);
				}
				GUILayout.EndHorizontal();
			}
			return unwrapParam;
		}

		#endregion
	}

}
