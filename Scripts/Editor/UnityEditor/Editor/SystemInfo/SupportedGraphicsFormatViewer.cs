using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Extenity.UnityTestToolbox.Editor
{

	/// <summary>
	/// Source: https://github.com/keijiro/GfxCapsView
	/// </summary>
	public class SupportedGraphicsFormatViewer : EditorWindow
	{
		string _text;
		Vector2 scroll;

		[MenuItem("Tools/System Info/Graphics Formats")]
		static void Init()
		{
			GetWindow<SupportedGraphicsFormatViewer>(true, "Supported Graphics Formats").Show();
		}

		private void OnGUI()
		{
			if (_text == null)
				_text = GenerateText();
			var rect = new Rect(0, 0, position.width, position.height);
			EditorGUI.TextArea(rect, _text, EditorStyles.wordWrappedLabel);
		}

		private string GenerateText()
		{
			var names = new List<string>();
			foreach (GraphicsFormat format in Enum.GetValues(typeof(GraphicsFormat)))
			{
				if (format == GraphicsFormat.None)
					continue;
				if (!SystemInfo.IsFormatSupported(format, FormatUsage.Render))
					continue;
				names.Add(format.ToString());
			}

			var s = "The following graphics formats are available for rendering:\n";
			s += string.Join("\n", names);

			return s;
		}
	}

}
