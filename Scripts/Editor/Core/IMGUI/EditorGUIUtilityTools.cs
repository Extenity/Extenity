using System.Reflection;
using Extenity.DataToolbox;
using Extenity.TextureToolbox;
using UnityEditor;
using UnityEngine;

namespace Extenity.IMGUIToolbox.Editor
{

	public static class EditorGUIUtilityTools
	{
		#region EditorGUIUtility Exposed Internals - TempContent

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

		#region EditorGUIUtility Exposed Internals - GetBasicTextureStyle

		private static GUIStyle s_BasicTextureStyle;

		public static GUIStyle GetBasicTextureStyle(Texture2D texture)
		{
			if (s_BasicTextureStyle == null)
				s_BasicTextureStyle = new GUIStyle();
			s_BasicTextureStyle.normal.background = texture;
			return s_BasicTextureStyle;
		}

		#endregion

		#region EditorGUIUtility Exposed Internals - GetDefaultBackgroundColor

		private static Color _DefaultBackgroundColor;
		public static Color DefaultBackgroundColor
		{
			get
			{
				if (_DefaultBackgroundColor.a == 0)
				{
					var method = typeof(EditorGUIUtility).GetMethod("GetDefaultBackgroundColor", BindingFlags.NonPublic | BindingFlags.Static);
					_DefaultBackgroundColor = (Color)method.Invoke(null, null);
				}
				return _DefaultBackgroundColor;
			}
		}

		#endregion

		#region Default Background Texture

		private static Texture2D _DefaultBackgroundTexture;
		public static Texture2D DefaultBackgroundTexture
		{
			get
			{
				if (!_DefaultBackgroundTexture)
				{
					_DefaultBackgroundTexture = TextureTools.CreateSimpleTexture(DefaultBackgroundColor);
				}
				return _DefaultBackgroundTexture;
			}
		}

		private static Texture2D _DarkerDefaultBackgroundTexture;
		public static Texture2D DarkerDefaultBackgroundTexture
		{
			get
			{
				if (!_DarkerDefaultBackgroundTexture)
				{
					_DarkerDefaultBackgroundTexture = TextureTools.CreateSimpleTexture(DefaultBackgroundColor.AdjustBrightness(0.8f));
				}
				return _DarkerDefaultBackgroundTexture;
			}
		}

		#endregion

		#region Unity Editor Icons

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
