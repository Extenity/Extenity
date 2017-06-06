using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Text;
using Extenity.ApplicationToolbox;
using Extenity.DataToolbox;
using Extenity.DebugToolbox;
using Extenity.TextureToolbox;

namespace Extenity.AssetToolbox.Editor
{

	public static class EditorAssetTools
	{
		#region Context Menu - Operations - Texture

		[MenuItem("Assets/Operations/Generate Embedded Code For Image File", priority = 3105)]
		public static void GenerateEmbeddedCodeForImageFile()
		{
			_GenerateEmbeddedCodeForImageFile(TextureFormat.ARGB32);
		}

		[MenuItem("Assets/Operations/Generate Embedded Code For Image File", validate = true)]
		private static bool Validate_GenerateEmbeddedCodeForImageFile()
		{
			if (Selection.objects == null || Selection.objects.Length != 1)
				return false;
			return Selection.objects[0] is Texture2D;
		}

		[MenuItem("Assets/Operations/Generate Embedded Code For Texture As PNG", priority = 3108)]
		public static void GenerateEmbeddedCodeForTextureAsPNG()
		{
			_GenerateEmbeddedCodeForTexture(texture => texture.EncodeToPNG(), TextureFormat.ARGB32);
		}

		[MenuItem("Assets/Operations/Generate Embedded Code For Texture As PNG", validate = true)]
		private static bool Validate_GenerateEmbeddedCodeForTextureAsPNG()
		{
			if (Selection.objects == null || Selection.objects.Length != 1)
				return false;
			return Selection.objects[0] is Texture2D;
		}

		[MenuItem("Assets/Operations/Generate Embedded Code For Texture As JPG", priority = 3109)]
		public static void GenerateEmbeddedCodeForTextureAsJPG()
		{
			_GenerateEmbeddedCodeForTexture(texture => texture.EncodeToJPG(), TextureFormat.RGB24);
		}

		[MenuItem("Assets/Operations/Generate Embedded Code For Texture As JPG", validate = true)]
		private static bool Validate_GenerateEmbeddedCodeForTextureAsJPG()
		{
			if (Selection.objects == null || Selection.objects.Length != 1)
				return false;
			return Selection.objects[0] is Texture2D;
		}

		private static void _GenerateEmbeddedCodeForTexture(Func<Texture2D, byte[]> getDataOfTexture, TextureFormat format)
		{
			var texture = Selection.objects[0] as Texture2D;
			var path = AssetDatabase.GetAssetPath(texture);
			var textureImporter = TextureImporter.GetAtPath(path) as TextureImporter;
			var mipmapEnabled = textureImporter.mipmapEnabled;
			var linear = !textureImporter.sRGBTexture;
			texture = texture.CopyTextureAsReadable(); // Get a readable copy of the texture
			var data = getDataOfTexture(texture);
			var fileName = Path.GetFileNameWithoutExtension(path);
			var textureName = fileName.ClearSpecialCharacters();
			var stringBuilder = new StringBuilder();
			var fieldName = TextureTools.GenerateEmbeddedCodeForTexture(data, textureName, format, mipmapEnabled, linear, "		", ref stringBuilder);
			Clipboard.SetClipboardText(stringBuilder.ToString());
			Debug.LogFormat("Generated texture data as field '{0}' and copied to clipboard. Path: {1}", fieldName, path);
		}

		private static void _GenerateEmbeddedCodeForImageFile(TextureFormat format)
		{
			var texture = Selection.objects[0] as Texture2D;
			var path = AssetDatabase.GetAssetPath(texture);
			var textureImporter = TextureImporter.GetAtPath(path) as TextureImporter;
			var mipmapEnabled = textureImporter.mipmapEnabled;
			var linear = !textureImporter.sRGBTexture;
			var data = File.ReadAllBytes(path);
			var fileName = Path.GetFileNameWithoutExtension(path);
			var textureName = fileName.ClearSpecialCharacters();
			var stringBuilder = new StringBuilder();
			var fieldName = TextureTools.GenerateEmbeddedCodeForTexture(data, textureName, format, mipmapEnabled, linear, "		", ref stringBuilder);
			Clipboard.SetClipboardText(stringBuilder.ToString());
			Debug.LogFormat("Generated texture data as field '{0}' and copied to clipboard. Path: {1}", fieldName, path);
		}

		#endregion

		#region Context Menu - Operations - Prefabs and Scenes

		[MenuItem("Assets/Operations/Mark All Prefabs And Scenes As Dirty", priority = 1105)]
		public static void MarkAllPrefabsAndScenesAsDirty()
		{
			{
				var list = GetAllSceneAssetPaths();
				list.Sort();
				list.LogList();
				foreach (var assetPath in list)
				{
					var asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
					EditorUtility.SetDirty(asset);
				}
			}
			{
				var list = GetAllPrefabAssetPaths();
				list.Sort();
				list.LogList();
				foreach (var assetPath in list)
				{
					var asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
					EditorUtility.SetDirty(asset);
				}
			}
		}

		public static List<string> GetAllSceneAssetPaths()
		{
			return AssetDatabase.GetAllAssetPaths().Where(item => item.ToLowerInvariant().Contains(".unity")).ToList();
		}
		public static List<string> GetAllPrefabAssetPaths()
		{
			return AssetDatabase.GetAllAssetPaths().Where(item => item.ToLowerInvariant().Contains(".prefab")).ToList();
		}

		#endregion
	}

}
