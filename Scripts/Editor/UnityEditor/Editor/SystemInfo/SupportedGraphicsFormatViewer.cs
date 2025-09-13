using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Extenity.UnityEditorToolbox.Editor
{

	/// <summary>
	/// Source: https://github.com/keijiro/GfxCapsView
	/// </summary>
	public class SupportedGraphicsFormatViewer : EditorWindow
	{
		string Text;
		Vector2 Scroll;

		[MenuItem(ExtenityMenu.Analysis + "System Info/Graphics Formats", priority = ExtenityMenu.AnalysisPriority + 9)]
		static void Init()
		{
			GetWindow<SupportedGraphicsFormatViewer>(true, "Supported Graphics Formats").Show();
		}

		private void OnGUI()
		{
			if (Text == null)
				Text = GenerateText();
			var rect = new Rect(0, 0, position.width, position.height);
			EditorGUI.TextArea(rect, Text, EditorStyles.wordWrappedLabel);
		}

		private string GenerateText()
		{
			var names = new List<string>();
			foreach (GraphicsFormat format in Enum.GetValues(typeof(GraphicsFormat)))
			{
				if (format == GraphicsFormat.None)
					continue;
				if (!SystemInfo.IsFormatSupported(format, GraphicsFormatUsage.Render))
					continue;
				names.Add(format.ToString());
			}

			var s = "The following graphics formats are available for rendering:\n";
			s += string.Join("\n", names);

			return s;
		}
	}

}
