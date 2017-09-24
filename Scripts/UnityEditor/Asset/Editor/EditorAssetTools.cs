using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Text;
using Extenity.ApplicationToolbox;
using Extenity.DataToolbox;
using Extenity.GameObjectToolbox;
using Extenity.TextureToolbox;

namespace Extenity.AssetToolbox.Editor
{

	public static class EditorAssetTools
	{
		#region Assets Menu - Operations - Texture

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

		#region Assets Menu - Operations - Mark As Dirty

		[MenuItem("Assets/Operations/Mark All Assets As Dirty", priority = 1104)]
		public static void MarkAllAssetsAsDirty()
		{
			MarkAssetsAsDirty(true, true, true, true, true, true, true, true, true, true, true, true, true, true, true);
		}

		[MenuItem("Assets/Operations/Mark All Prefabs And Scenes As Dirty", priority = 1105)]
		public static void MarkAllPrefabsAndScenesAsDirty()
		{
			MarkAssetsAsDirty(true, true, false, false, false, false, false, false, false, false, false, false, false, false, false);
		}

		public static void MarkAssetsAsDirty(
			bool scenes,
			bool prefabs,
			bool animations,
			bool materials,
			bool shaders,
			bool models,
			bool textures,
			bool proceduralTextures,
			bool renderTextures,
			bool lightmap,
			bool flares,
			bool videos,
			bool ui,
			bool physics,
			bool audio
		)
		{
			var fullList = new List<string>();
			var log = new StringBuilder();
			if (scenes)
			{
				var list = AssetTools.GetAllSceneAssetPaths();
				InternalAddToAssetList(list, fullList, "Scenes", log);
			}
			if (prefabs)
			{
				var list = AssetTools.GetAllPrefabAssetPaths();
				InternalAddToAssetList(list, fullList, "Prefabs", log);
			}
			if (models)
			{
				var list = AssetTools.GetAllModelAssetPaths();
				InternalAddToAssetList(list, fullList, "Models", log);
			}
			if (animations)
			{
				var list = AssetTools.GetAllAnimationAssetPaths();
				InternalAddToAssetList(list, fullList, "Animations", log);
			}
			if (materials)
			{
				var list = AssetTools.GetAllMaterialAssetPaths();
				InternalAddToAssetList(list, fullList, "Materials", log);
			}
			if (shaders)
			{
				var list = AssetTools.GetAllShaderAssetPaths();
				InternalAddToAssetList(list, fullList, "Shaders", log);
			}
			if (textures)
			{
				var list = AssetTools.GetAllTextureAssetPaths();
				InternalAddToAssetList(list, fullList, "Textures", log);
			}
			if (proceduralTextures)
			{
				var list = AssetTools.GetAllProceduralTextureAssetPaths();
				InternalAddToAssetList(list, fullList, "Procedural Textures", log);
			}
			if (renderTextures)
			{
				var list = AssetTools.GetAllRenderTextureAssetPaths();
				InternalAddToAssetList(list, fullList, "Render Textures", log);
			}
			if (lightmap)
			{
				var list = AssetTools.GetAllLightmapAssetPaths();
				InternalAddToAssetList(list, fullList, "Lightmaps", log);
			}
			if (flares)
			{
				var list = AssetTools.GetAllFlareAssetPaths();
				InternalAddToAssetList(list, fullList, "Flares", log);
			}
			if (videos)
			{
				var list = AssetTools.GetAllVideoAssetPaths();
				InternalAddToAssetList(list, fullList, "Videos", log);
			}
			if (ui)
			{
				var list = AssetTools.GetAllUIAssetPaths();
				InternalAddToAssetList(list, fullList, "UI", log);
			}
			if (audio)
			{
				var list = AssetTools.GetAllAudioAssetPaths();
				InternalAddToAssetList(list, fullList, "Audio", log);
			}
			if (physics)
			{
				var list = AssetTools.GetAllPhysicsAssetPaths();
				InternalAddToAssetList(list, fullList, "Physics", log);
			}

			Debug.Log(log.ToString());

			foreach (var assetPath in fullList)
			{
				var asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
				EditorUtility.SetDirty(asset);
			}
		}

		private static void InternalAddToAssetList(List<string> list, List<string> fullList, string logTitle, StringBuilder log)
		{
			if (list.Count == 0)
				return;

			list.Sort();
			fullList.AddRange(list);
			log.AppendLine(string.Format("====  ({0}) {1}  ====", list.Count, logTitle));
			foreach (var item in list)
			{
				log.AppendLine(item);
			}
		}

		#endregion

		#region Assets Menu - Operations - Set Static

		[MenuItem("Assets/Operations/Set Selection As Static", priority = 804)]
		public static void SetSelectionAsStatic()
		{
			var gameObjects = Selection.gameObjects;
			foreach (var gameObject in gameObjects)
				if (!gameObject.isStatic)
					gameObject.isStatic = true;
		}

		[MenuItem("Assets/Operations/Set Selection As Static", validate = true)]
		public static bool Validate_SetSelectionAsStatic()
		{
			return Selection.gameObjects.Length > 0;
		}

		[MenuItem("Assets/Operations/Set Selection As Non Static", priority = 805)]
		public static void SetSelectionAsNonStatic()
		{
			var gameObjects = Selection.gameObjects;
			foreach (var gameObject in gameObjects)
				if (gameObject.isStatic)
					gameObject.isStatic = false;
		}

		[MenuItem("Assets/Operations/Set Selection As Non Static", validate = true)]
		public static bool Validate_SetSelectionAsNonStatic()
		{
			return Selection.gameObjects.Length > 0;
		}

		#endregion

		#region Context Menu - Fill Empty References

		[MenuItem("CONTEXT/Component/Fill Empty References", true)]
		private static bool FillEmptyReferences_Validate(MenuCommand menuCommand)
		{
			var component = (Component)menuCommand.context;
			return component.GetNotAssignedSerializedComponentFields().Count > 0;
		}

		[MenuItem("CONTEXT/Component/Fill Empty References", priority = 504)]
		private static void FillEmptyReferences(MenuCommand menuCommand)
		{
			var component = (Component)menuCommand.context;
			var fields = component.GetNotAssignedSerializedComponentFields();

			foreach (var field in fields)
			{
				// First search inside current gameobject
				Component selected = null;
				var candidates = component.GetComponents(field.FieldType);
				if (candidates != null && candidates.Length > 0)
				{
					if (candidates.Length > 1)
					{
						Debug.LogErrorFormat("Found more than one candidate of type '{0}'. You need to manually assign the field '{1}'.", field.FieldType, field.Name);
					}
					else
					{
						selected = candidates[0];
					}
				}
				else
				{
					// Then search inside children
					candidates = component.GetComponentsInChildren(field.FieldType);
					if (candidates != null && candidates.Length > 0)
					{
						if (candidates.Length > 1)
						{
							Debug.LogErrorFormat("Found more than one candidate of type '{0}' in children. You need to manually assign the field '{1}'.", field.FieldType, field.Name);
						}
						else
						{
							selected = candidates[0];
						}
					}
					else
					{
						Debug.LogErrorFormat("No candidates found of type '{0}' for field '{1}'.", field.FieldType, field.Name);
					}
				}

				if (selected != null)
				{
					Debug.LogFormat("Filling field '{0}' of type '{1}' with '{2}'.", field.Name, field.FieldType, selected.gameObject.FullName());
					Undo.RecordObject(component, "Fill Empty References");
					field.SetValue(component, candidates[0]);

				}
			}
		}

		#endregion
	}

}
