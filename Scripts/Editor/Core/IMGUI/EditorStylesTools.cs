using UnityEditor;
using UnityEngine;

namespace Extenity.IMGUIToolbox.Editor
{

	public static class EditorStylesTools
	{
		private static GUIStyle _CenteredLabel;
		public static GUIStyle CenteredLabel
		{
			get
			{
				if (_CenteredLabel == null)
				{
					_CenteredLabel = new GUIStyle(EditorStyles.label);
					_CenteredLabel.alignment = TextAnchor.UpperCenter;
				}
				return _CenteredLabel;
			}
		}

		private static GUIStyle _RichLabel;
		public static GUIStyle RichLabel
		{
			get
			{
				if (_RichLabel == null)
				{
					_RichLabel = new GUIStyle(GUI.skin.label);
					_RichLabel.richText = true;
					_RichLabel.wordWrap = true;
				}
				return _RichLabel;
			}
		}

		private static GUIStyle _RichWordWrappedLabel;
		public static GUIStyle RichWordWrappedLabel
		{
			get
			{
				if (_RichWordWrappedLabel == null)
				{
					_RichWordWrappedLabel = new GUIStyle(EditorStyles.wordWrappedLabel);
					_RichWordWrappedLabel.richText = true;
					_RichWordWrappedLabel.wordWrap = true;
				}
				return _RichWordWrappedLabel;
			}
		}

		private static GUIStyle _BoldFoldout;
		public static GUIStyle BoldFoldout
		{
			get
			{
				if (_BoldFoldout == null)
				{
					_BoldFoldout = new GUIStyle(EditorStyles.foldout);
					_BoldFoldout.fontStyle = FontStyle.Bold;
				}
				return _BoldFoldout;
			}
		}
	}

}
