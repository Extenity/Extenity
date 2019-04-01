using UnityEditor;
using UnityEngine;

namespace Extenity.IMGUIToolbox.Editor
{

	public static class EditorGUIUtilityTools
	{
		#region EditorGUIUtility Exposed Internals

		private static GUIContent s_Text = new GUIContent();
		private static GUIContent s_Image = new GUIContent();
		private static GUIContent s_TextImage = new GUIContent();

		public static GUIContent TempContent(string t)
		{
			s_Text.text = t;
			return s_Text;
		}

		public static GUIContent TempContent(Texture i)
		{
			s_Image.image = i;
			return s_Image;
		}

		public static GUIContent TempContent(string t, Texture i)
		{
			s_TextImage.image = i;
			s_TextImage.text = t;
			return s_TextImage;
		}

		#endregion

		#region Icons

		private static Texture2D[] _UnityEditorIcons;
		public static Texture2D[] UnityEditorIcons
		{
			get
			{
				if (_UnityEditorIcons == null)
				{
					_UnityEditorIcons = new[]
					{
						// ReSharper disable StringLiteralTypo
						EditorGUIUtility.FindTexture("Material Icon"),
						EditorGUIUtility.FindTexture("Folder Icon"),
						EditorGUIUtility.FindTexture("AudioSource Icon"),
						EditorGUIUtility.FindTexture("Camera Icon"),
						EditorGUIUtility.FindTexture("Windzone Icon"),
						EditorGUIUtility.FindTexture("GameObject Icon"),
						EditorGUIUtility.FindTexture("Texture Icon"),
						// ReSharper restore StringLiteralTypo
					};
				}
				return _UnityEditorIcons;
			}
		}

		#endregion
	}

}
