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
using Extenity.GameObjectToolbox;
using Extenity.SceneManagementToolbox.Editor;
using Extenity.TextureToolbox;
using UnityEditor.SceneManagement;

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
			Clipboard.SetClipboardText(stringBuilder.ToString(), false);
			Log.Info($"Generated texture data as field '{fieldName}' and copied to clipboard. Path: {path}");
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
			Clipboard.SetClipboardText(stringBuilder.ToString(), false);
			Log.Info($"Generated texture data as field '{fieldName}' and copied to clipboard. Path: {path}");
		}

		#endregion

		#region Assets Menu - Operations - RenderTexture

		[MenuItem("Assets/Operations/Save RenderTexture To File/All", priority = 2901)]
		public static void SaveRenderTextureToFile_All()
		{
			var allTextureFormats = Enum.GetValues(typeof(TextureFormat)) as TextureFormat[];
			foreach (var textureFormat in allTextureFormats)
			{
				_SaveSelectedRenderTexturesToFile(textureFormat, true);
				_SaveSelectedRenderTexturesToFile(textureFormat, false);
			}
		}
		[MenuItem("Assets/Operations/Save RenderTexture To File/All", validate = true)]
		private static bool Validate_SaveRenderTextureToFile_All()
		{
			return IsSelectionContainsAnyRenderTexture();
		}

		[MenuItem("Assets/Operations/Save RenderTexture To File/RGBA32", priority = 2801)]
		public static void SaveRenderTextureToFile_RGBA32()
		{
			_SaveSelectedRenderTexturesToFile(TextureFormat.RGBA32, true);
		}
		[MenuItem("Assets/Operations/Save RenderTexture To File/RGBA32", validate = true)]
		private static bool Validate_SaveRenderTextureToFile_RGBA32()
		{
			return IsSelectionContainsAnyRenderTexture();
		}

		[MenuItem("Assets/Operations/Save RenderTexture To File/ARGB32", priority = 2802)]
		public static void SaveRenderTextureToFile_ARGB32()
		{
			_SaveSelectedRenderTexturesToFile(TextureFormat.ARGB32, true);
		}
		[MenuItem("Assets/Operations/Save RenderTexture To File/ARGB32", validate = true)]
		private static bool Validate_SaveRenderTextureToFile_ARGB32()
		{
			return IsSelectionContainsAnyRenderTexture();
		}

		[MenuItem("Assets/Operations/Save RenderTexture To File/RGB24", priority = 2803)]
		public static void SaveRenderTextureToFile_RGB24()
		{
			_SaveSelectedRenderTexturesToFile(TextureFormat.RGB24, true);
		}
		[MenuItem("Assets/Operations/Save RenderTexture To File/RGB24", validate = true)]
		private static bool Validate_SaveRenderTextureToFile_RGB24()
		{
			return IsSelectionContainsAnyRenderTexture();
		}

		[MenuItem("Assets/Operations/Save RenderTexture To File/RGBAFloat", priority = 2804)]
		public static void SaveRenderTextureToFile_RGBAFloat()
		{
			_SaveSelectedRenderTexturesToFile(TextureFormat.RGBAFloat, true);
		}
		[MenuItem("Assets/Operations/Save RenderTexture To File/RGBAFloat", validate = true)]
		private static bool Validate_SaveRenderTextureToFile_RGBAFloat()
		{
			return IsSelectionContainsAnyRenderTexture();
		}

		[MenuItem("Assets/Operations/Save RenderTexture To File/RGBAHalf", priority = 2805)]
		public static void SaveRenderTextureToFile_RGBAHalf()
		{
			_SaveSelectedRenderTexturesToFile(TextureFormat.RGBAHalf, true);
		}
		[MenuItem("Assets/Operations/Save RenderTexture To File/RGBAHalf", validate = true)]
		private static bool Validate_SaveRenderTextureToFile_RGBAHalf()
		{
			return IsSelectionContainsAnyRenderTexture();
		}

		private static void _SaveSelectedRenderTexturesToFile(TextureFormat format, bool linear)
		{
			var selectedObjects = Selection.objects;
			if (selectedObjects.IsNotNullAndEmpty())
			{
				foreach (var selected in selectedObjects)
				{
					if (selected is RenderTexture)
					{
						var renderTexturePath = AssetDatabase.GetAssetPath(selected);
						var renderTextureFileName = Path.GetFileNameWithoutExtension(renderTexturePath);
						var fileName = $"{renderTextureFileName}-{(linear ? "Linear" : "NonLinear")}-{format}.png";
						//var directory = Path.GetDirectoryName(renderTexturePath);
						var directory = "RenderTextureDump";
						var path = Path.Combine(directory, fileName);
						DirectoryTools.CreateFromFilePath(path);
						_SaveRenderTextureToFile(path, (RenderTexture)selected, format, linear);
					}
				}
			}
		}

		private static void _SaveRenderTextureToFile(string path, RenderTexture renderTexture, TextureFormat format, bool linear)
		{
			try
			{
				using (var error = new ErrorLogDetector(true, true, true))
				{
					var texture = new Texture2D(renderTexture.width, renderTexture.height, format, false, linear);
					if (!texture)
					{
						Log.Warning($"Could not create texture with format '{format}'.");
						return;
					}

					RenderTexture.active = renderTexture;

					texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
					texture.Apply();

					RenderTexture.active = null;

					var imageBytes = texture.EncodeToPNG();
					if (imageBytes.IsNullOrEmpty())
					{
						Log.Warning($"PNG data is empty for texture format '{format}'.");
						return;
					}

					// Reroute the image into Erroneous directory.
					if (error.AnyDetected)
					{
						var fileName = Path.GetFileName(path);
						var directory = Path.GetDirectoryName(path);
						directory = Path.Combine(directory, "Erroneous");
						path = Path.Combine(directory, fileName);
						DirectoryTools.CreateFromFilePath(path);
					}

					File.WriteAllBytes(path, imageBytes);
					Log.Info($"Texture saved to '{path}'.");
				}
			}
			catch (Exception exception)
			{
				Log.Warning($"Exception was thrown while processing for texture format '{format}'. Exception: {exception.Message}");
			}
		}

		private static bool IsSelectionContainsAnyRenderTexture()
		{
			var objects = Selection.objects;
			return objects.IsNotNullAndEmpty() && objects.Any(item => item is RenderTexture);
		}

		#endregion

		#region Assets Menu - Operations - Reserialize Assets

		[MenuItem("Assets/Operations/Reserialize Selected Assets", priority = 1102)]
		public static void ReserializeSelectedAssets()
		{
			var fullList = new List<string>();
			var log = new StringBuilder();

			Log.Info("NOTE! If you want to include files in a folder, make sure you select the folder <b>in right column</b> if you use Two-Column Layout project window.");
			var list = AssetTools.GetSelectedAssetPaths(true);
			InternalAddToAssetList(list, fullList, "Selected Assets", log);

			Log.Info(log.ToString());

			ReserializeAssets(fullList);
		}

		[MenuItem("Assets/Operations/Reserialize All Assets", priority = 1103)]
		public static void ReserializeAllAssets()
		{
			ReserializeAssets(AssetDatabase.GetAllAssetPaths());
		}

		[MenuItem("Assets/Operations/Reserialize All Scenes", priority = 1104)]
		public static void ReserializeAllScenes()
		{
			ReserializeAssets(true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false);
		}

		[MenuItem("Assets/Operations/Reserialize All Prefabs", priority = 1105)]
		public static void ReserializeAllPrefabs()
		{
			ReserializeAssets(false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false);
		}

		[MenuItem("Assets/Operations/Reserialize All Graphics Assets", priority = 1106)]
		public static void ReserializeAllGraphicsAssets()
		{
			ReserializeAssets(false, false, true, true, true, true, true, true, true, true, true, true, true, false, false, false);
		}

		[MenuItem("Assets/Operations/Reserialize All Audio Assets", priority = 1107)]
		public static void ReserializeAllAudioAssets()
		{
			ReserializeAssets(false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false);
		}

		[MenuItem("Assets/Operations/Reserialize All Script Assets", priority = 1108)]
		public static void ReserializeAllScriptAssets()
		{
			ReserializeAssets(false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true);
		}

		public static void ReserializeAssets(
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
			bool audio,
			bool script
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
				var list = AssetTools.GetAllShaderAssetPaths(true, true, true, true);
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
			if (script)
			{
				var list = AssetTools.GetAllScriptAssetPaths();
				InternalAddToAssetList(list, fullList, "Scripts", log);
			}

			Log.Info(log.ToString());

			// The old way.
			//foreach (var assetPath in fullList)
			//{
			//	var asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
			//	EditorUtility.SetDirty(asset);
			//}

			ReserializeAssets(fullList);
		}

		private static void InternalAddToAssetList(List<string> list, List<string> fullList, string logTitle, StringBuilder log)
		{
			log.AppendLine($"====  ({list.Count}) {logTitle}  ====");

			if (list.Count == 0)
				return;

			list.Sort();
			fullList.AddRange(list);
			foreach (var item in list)
			{
				log.AppendLine(item);
			}
		}

		public static void ReserializeAssets(IEnumerable<string> assetPaths)
		{
			// This is the old way of doing it. Which somewhat worked with some flaws.
			//MarkAssetsAsDirty(true, true, true, true, true, true, true, true, true, true, true, true, true, true, true);

			// This is the brand new Unity's method. This works quite better.
			// But still, we need to process scenes separately because Unity
			// can't handle them well if the scene is not loaded. Somehow 
			// Unity includes all prefab data in scenes.
			{
				var sceneAssetPaths = new List<string>();
				var otherAssetPaths = new List<string>();
				AssetTools.SplitSceneAndOtherAssetPaths(assetPaths, sceneAssetPaths, otherAssetPaths);
				if (sceneAssetPaths.IsNotNullAndEmpty())
				{
					// Check if the current scene has modifications and warn user
					EditorSceneManagerTools.EnforceUserToSaveAllModifiedScenes("Reserialization needs to load scenes one by one. To prevent loosing any unsaved work, first you need to save current changes before reserialization.");
				}

				AssetDatabase.ForceReserializeAssets(otherAssetPaths, ForceReserializeAssetsOptions.ReserializeAssetsAndMetadata);
				InternalReserializeScenes(sceneAssetPaths);
			}
		}

		private static void InternalReserializeScenes(IEnumerable<string> sceneAssetPaths)
		{
			foreach (var sceneAssetPath in sceneAssetPaths)
			{
				InternalReserializeScene(sceneAssetPath, false);
			}

			// Make sure there is no garbage left after reserialization. 
			EditorSceneManagerTools.UnloadAllScenes(true);
		}

		private static void InternalReserializeScene(string sceneAssetPath, bool unloadAfterwards)
		{
			if (string.IsNullOrEmpty(sceneAssetPath))
				throw new ArgumentNullException(nameof(sceneAssetPath));

			// Make sure there were nothing loaded before opening the scene.
			// An already loaded asset may confuse the loading process.
			EditorSceneManagerTools.UnloadAllScenes(true);

			EditorSceneManagerTools.ThrowIfAnyLoadedSceneIsDirty(Log.BuildInternalErrorMessage(105851));

			// Load the scene.
			var openedScene = EditorSceneManager.OpenScene(sceneAssetPath, OpenSceneMode.Single);
			if (!openedScene.IsValid())
				throw new Exception($"Failed to load scene '{sceneAssetPath}' for reserialization.");

			// Mark the scene as dirty.
			EditorSceneManager.MarkAllScenesDirty();

			// Save the scene to make it reserialize itself to file.
			var saveResult = EditorSceneManager.SaveOpenScenes();
			if (!saveResult)
				throw new Exception($"Failed to save scene '{sceneAssetPath}' for reserialization.");

			if (EditorSceneManagerTools.IsAnyLoadedSceneDirty())
			{
				Log.Warning($"Scene '{sceneAssetPath}' still has unsaved changes just after it is saved. Probably a script in scene makes some changes after serialization, which is an unexpected behaviour. You may want to inspect it further, though most of the time you can safely ignore this message.");
			}

			// Make sure there is no garbage left after reserialization. 
			if (unloadAfterwards)
			{
				EditorSceneManagerTools.UnloadAllScenes(true);
			}
		}

		#endregion

		#region Context Menu - Fill Empty References

		[MenuItem("CONTEXT/Component/Fill Empty References", true)]
		private static bool FillEmptyReferences_Validate(MenuCommand menuCommand)
		{
			var component = menuCommand.context as Component;
			if (!component)
				return false;
			return component.GetNotAssignedSerializedComponentFields().Count > 0;
		}

		[MenuItem("CONTEXT/Component/Fill Empty References", priority = 524)]
		private static void FillEmptyReferences(MenuCommand menuCommand)
		{
			var component = menuCommand.context as Component;
			FillEmptyReferences(component);
		}

		[MenuItem("CONTEXT/Component/Fill Empty References In All Components", true)]
		private static bool FillEmptyReferencesInAllComponents_Validate(MenuCommand menuCommand)
		{
			return true;
		}

		[MenuItem("CONTEXT/Component/Fill Empty References In All Components", priority = 526)]
		private static void FillEmptyReferencesInAllComponents(MenuCommand menuCommand)
		{
			var selectedComponent = menuCommand.context as Component;
			var components = selectedComponent.gameObject.GetComponents<Component>();
			foreach (var component in components)
			{
				FillEmptyReferences(component);
			}
		}

		public static void FillEmptyReferences(Component component)
		{
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
						Log.Error($"Found more than one candidate of type '{field.FieldType}'. You need to manually assign the field '{field.Name}'.");
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
							Log.Error($"Found more than one candidate of type '{field.FieldType}' in children. You need to manually assign the field '{field.Name}'.");
						}
						else
						{
							selected = candidates[0];
						}
					}
					else
					{
						Log.Error($"No candidates found of type '{field.FieldType}' for field '{field.Name}'.");
					}
				}

				if (selected != null)
				{
					Log.Info($"Filling field '{field.Name}' of type '{field.FieldType}' with '{selected.gameObject.FullName()}'.");
					Undo.RecordObject(component, "Fill Empty References");
					field.SetValue(component, candidates[0]);
					EditorUtility.SetDirty(component);
				}
			}
		}

		#endregion

		#region Context Menu - Copy Component/GameObject Path

		[MenuItem("CONTEXT/Component/Copy Component Path", true)]
		private static bool CopyComponentPath_Validate(MenuCommand menuCommand)
		{
			var component = menuCommand.context as Component;
			return component;
		}

		[MenuItem("CONTEXT/Component/Copy Component Path", priority = 541)]
		private static void CopyComponentPath(MenuCommand menuCommand)
		{
			var component = menuCommand.context as Component;
			Clipboard.SetClipboardText(component.FullName(), true);
		}

		[MenuItem("CONTEXT/Component/Copy GameObject Path", true)]
		private static bool CopyGameObjectPath_Validate(MenuCommand menuCommand)
		{
			var component = menuCommand.context as Component;
			return component;
		}

		[MenuItem("CONTEXT/Component/Copy GameObject Path", priority = 542)]
		private static void CopyGameObjectPath(MenuCommand menuCommand)
		{
			var component = menuCommand.context as Component;
			Clipboard.SetClipboardText(component.gameObject.FullName(), true);
		}

		#endregion

		#region Context Menu - Serialize to JSON

		[MenuItem("CONTEXT/Component/Serialize to JSON", true)]
		private static bool SerializeToJSON_Validate(MenuCommand menuCommand)
		{
			var component = menuCommand.context as Component;
			return component;
		}

		[MenuItem("CONTEXT/Component/Serialize to JSON", priority = 561)]
		private static void SerializeToJSON(MenuCommand menuCommand)
		{
			var component = menuCommand.context as Component;
			var json = EditorJsonUtility.ToJson(component, true);
			Log.Info(json, component);
		}

		#endregion
	}

}
