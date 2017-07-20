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
	}

}
