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
using Extenity.FileSystemToolbox;
using Extenity.SceneManagementToolbox.Editor;
using Extenity.TextureToolbox;
using Extenity.UnityEditorToolbox;
using UnityEditor.SceneManagement;

namespace Extenity.AssetToolbox.Editor
{

	public static class EditorAssetTools
	{
		#region Assets Menu - Operations - Texture

		private const string GenerateImageMenu = ExtenityMenu.AssetOperationsContext + "Generate Embedded Code For Image/";

		[MenuItem(GenerateImageMenu + "Generate Embedded Code For Image File", validate = true)]
		[MenuItem(GenerateImageMenu + "Generate Embedded Code For Texture As PNG", validate = true)]
		[MenuItem(GenerateImageMenu + "Generate Embedded Code For Texture As JPG", validate = true)]
		private static bool _GenerateEmbeddedCodeForImage_Validate()
		{
			var objects = Selection.objects;
			if (objects == null || objects.Length != 1)
				return false;
			return objects[0] is Texture2D;
		}

		[MenuItem(GenerateImageMenu + "Generate Embedded Code For Image File", priority = 3101)]
		public static void GenerateEmbeddedCodeForImageFile()
		{
			_GenerateEmbeddedCodeForImageFile(TextureFormat.ARGB32);
		}

		[MenuItem(GenerateImageMenu + "Generate Embedded Code For Texture As PNG", priority = 3102)]
		public static void GenerateEmbeddedCodeForTextureAsPNG()
		{
			_GenerateEmbeddedCodeForTexture(texture => texture.EncodeToPNG(), TextureFormat.ARGB32);
		}

		[MenuItem(GenerateImageMenu + "Generate Embedded Code For Texture As JPG", priority = 3103)]
		public static void GenerateEmbeddedCodeForTextureAsJPG()
		{
			_GenerateEmbeddedCodeForTexture(texture => texture.EncodeToJPG(), TextureFormat.RGB24);
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
			Log.InfoWithContext(texture, $"Generated texture data as field '{fieldName}' and copied to clipboard. Path: {path}");
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
			Log.InfoWithContext(texture, $"Generated texture data as field '{fieldName}' and copied to clipboard. Path: {path}");
		}

		#endregion

		#region Assets Menu - Operations - RenderTexture

		private const string SaveRenderTextureMenu = ExtenityMenu.AssetOperationsContext + "Save RenderTexture To File/";

		[MenuItem(SaveRenderTextureMenu + "All", validate = true)]
		[MenuItem(SaveRenderTextureMenu + "RGBA32", validate = true)]
		[MenuItem(SaveRenderTextureMenu + "ARGB32", validate = true)]
		[MenuItem(SaveRenderTextureMenu + "RGB24", validate = true)]
		[MenuItem(SaveRenderTextureMenu + "RGBAFloat", validate = true)]
		[MenuItem(SaveRenderTextureMenu + "RGBAHalf", validate = true)]
		private static bool Validate_SaveRenderTextureToFile()
		{
			var objects = Selection.objects;
			return objects.IsNotNullAndEmpty() && objects.Any(item => item is RenderTexture);
		}

		[MenuItem(SaveRenderTextureMenu + "All", priority = 2901)]
		public static void SaveRenderTextureToFile_All()
		{
			var allTextureFormats = Enum.GetValues(typeof(TextureFormat)) as TextureFormat[];
			foreach (var textureFormat in allTextureFormats)
			{
				_SaveSelectedRenderTexturesToFile(textureFormat, true);
				_SaveSelectedRenderTexturesToFile(textureFormat, false);
			}
		}

		[MenuItem(SaveRenderTextureMenu + "RGBA32", priority = 2801)]
		public static void SaveRenderTextureToFile_RGBA32()
		{
			_SaveSelectedRenderTexturesToFile(TextureFormat.RGBA32, true);
		}

		[MenuItem(SaveRenderTextureMenu + "ARGB32", priority = 2802)]
		public static void SaveRenderTextureToFile_ARGB32()
		{
			_SaveSelectedRenderTexturesToFile(TextureFormat.ARGB32, true);
		}

		[MenuItem(SaveRenderTextureMenu + "RGB24", priority = 2803)]
		public static void SaveRenderTextureToFile_RGB24()
		{
			_SaveSelectedRenderTexturesToFile(TextureFormat.RGB24, true);
		}

		[MenuItem(SaveRenderTextureMenu + "RGBAFloat", priority = 2804)]
		public static void SaveRenderTextureToFile_RGBAFloat()
		{
			_SaveSelectedRenderTexturesToFile(TextureFormat.RGBAFloat, true);
		}

		[MenuItem(SaveRenderTextureMenu + "RGBAHalf", priority = 2805)]
		public static void SaveRenderTextureToFile_RGBAHalf()
		{
			_SaveSelectedRenderTexturesToFile(TextureFormat.RGBAHalf, true);
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
				Log.Error(new Exception($"Exception was thrown while processing for texture format '{format}'.", exception));
			}
		}

		#endregion

		#region Assets Menu - Operations - Reserialize Assets

		private const string ReserializeMenu = ExtenityMenu.AssetOperationsContext + "Reserialize/";

		[MenuItem(ReserializeMenu + "Reserialize Selected Assets", priority = 1101)]
		public static void ReserializeSelectedAssets()
		{
			EditorApplication.delayCall += () => // Delaying the call to hopefully fix the dreaded random crash problem. See 719274423.
			{
				var fullList = New.List<string>();
				var log = new StringBuilder();

				var list = AssetDatabaseTools.GetSelectedAssetPaths(true);
				InternalAddToAssetList(ref list, fullList, "Selected Asset", log);

				Log.Info(log.ToString());

				ReserializeAssets(ref fullList);
			};
		}

		[MenuItem(ReserializeMenu + "Reserialize Project Settings", priority = 1151)]
		public static void ReserializeProjectSettings()
		{
			EditorApplication.delayCall += () => // Delaying the call to hopefully fix the dreaded random crash problem. See 719274423.
			{
				var fullList = New.List<string>();
				var log = new StringBuilder();

				var list = AssetDatabase.GetAllAssetPaths().Where(path => path.StartsWith(ApplicationTools.UnityProjectPaths.ProjectSettingsDirectory)).ToList();
				InternalAddToAssetList(ref list, fullList, "Project Settings Asset", log);

				Log.Info(log.ToString());

				ReserializeAssets(ref fullList);
			};
		}

		[MenuItem(ReserializeMenu + "Reserialize All Assets", priority = 1203)]
		public static void ReserializeAllAssets()
		{
			EditorApplication.delayCall += () => // Delaying the call to hopefully fix the dreaded random crash problem. See 719274423.
			{
				var list = AssetDatabase.GetAllAssetPaths().ToPooledList();
				ReserializeAssets(ref list);
			};
		}

		[MenuItem(ReserializeMenu + "Reserialize All Scenes", priority = 1204)]
		public static void ReserializeAllScenes()
		{
			EditorApplication.delayCall += () => // Delaying the call to hopefully fix the dreaded random crash problem. See 719274423.
			{
				ReserializeAssets(true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false);
			};
		}

		[MenuItem(ReserializeMenu + "Reserialize All Prefabs", priority = 1205)]
		public static void ReserializeAllPrefabs()
		{
			EditorApplication.delayCall += () => // Delaying the call to hopefully fix the dreaded random crash problem. See 719274423.
			{
				ReserializeAssets(false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false);
			};
		}

		[MenuItem(ReserializeMenu + "Reserialize All Graphics Assets", priority = 1206)]
		public static void ReserializeAllGraphicsAssets()
		{
			EditorApplication.delayCall += () => // Delaying the call to hopefully fix the dreaded random crash problem. See 719274423.
			{
				ReserializeAssets(false, false, true, true, true, true, true, true, true, true, true, true, true, false, false, false);
			};
		}

		[MenuItem(ReserializeMenu + "Reserialize All Audio Assets", priority = 1207)]
		public static void ReserializeAllAudioAssets()
		{
			EditorApplication.delayCall += () => // Delaying the call to hopefully fix the dreaded random crash problem. See 719274423.
			{
				ReserializeAssets(false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false);
			};
		}

		[MenuItem(ReserializeMenu + "Reserialize All Script Assets", priority = 1208)]
		public static void ReserializeAllScriptAssets()
		{
			EditorApplication.delayCall += () => // Delaying the call to hopefully fix the dreaded random crash problem. See 719274423.
			{
				ReserializeAssets(false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true);
			};
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
			var fullList = New.List<string>();
			var log = new StringBuilder();
			if (scenes)
			{
				var list = AssetDatabaseTools.GetAllSceneAssetPaths();
				InternalAddToAssetList(ref list, fullList, "Scene", log);
			}
			if (prefabs)
			{
				var list = AssetDatabaseTools.GetAllPrefabAssetPaths();
				InternalAddToAssetList(ref list, fullList, "Prefab", log);
			}
			if (models)
			{
				var list = AssetDatabaseTools.GetAllModelAssetPaths();
				InternalAddToAssetList(ref list, fullList, "Model", log);
			}
			if (animations)
			{
				var list = AssetDatabaseTools.GetAllAnimationAssetPaths();
				InternalAddToAssetList(ref list, fullList, "Animation", log);
			}
			if (materials)
			{
				var list = AssetDatabaseTools.GetAllMaterialAssetPaths();
				InternalAddToAssetList(ref list, fullList, "Material", log);
			}
			if (shaders)
			{
				var list = AssetDatabaseTools.GetAllShaderAssetPaths(true, true, true, true);
				InternalAddToAssetList(ref list, fullList, "Shader", log);
			}
			if (textures)
			{
				var list = AssetDatabaseTools.GetAllTextureAssetPaths();
				InternalAddToAssetList(ref list, fullList, "Texture", log);
			}
			if (proceduralTextures)
			{
				var list = AssetDatabaseTools.GetAllProceduralTextureAssetPaths();
				InternalAddToAssetList(ref list, fullList, "Procedural Texture", log);
			}
			if (renderTextures)
			{
				var list = AssetDatabaseTools.GetAllRenderTextureAssetPaths();
				InternalAddToAssetList(ref list, fullList, "Render Texture", log);
			}
			if (lightmap)
			{
				var list = AssetDatabaseTools.GetAllLightmapAssetPaths();
				InternalAddToAssetList(ref list, fullList, "Lightmap", log);
			}
			if (flares)
			{
				var list = AssetDatabaseTools.GetAllFlareAssetPaths();
				InternalAddToAssetList(ref list, fullList, "Flare Asset", log);
			}
			if (videos)
			{
				var list = AssetDatabaseTools.GetAllVideoAssetPaths();
				InternalAddToAssetList(ref list, fullList, "Video", log);
			}
			if (ui)
			{
				var list = AssetDatabaseTools.GetAllUIAssetPaths();
				InternalAddToAssetList(ref list, fullList, "UI Asset", log);
			}
			if (audio)
			{
				var list = AssetDatabaseTools.GetAllAudioAssetPaths();
				InternalAddToAssetList(ref list, fullList, "Audio Asset", log);
			}
			if (physics)
			{
				var list = AssetDatabaseTools.GetAllPhysicsAssetPaths();
				InternalAddToAssetList(ref list, fullList, "Physics Asset", log);
			}
			if (script)
			{
				var list = AssetDatabaseTools.GetAllScriptAssetPaths();
				InternalAddToAssetList(ref list, fullList, "Script", log);
			}

			Log.Info(log.ToString());

			// The old way.
			//foreach (var assetPath in fullList)
			//{
			//	var asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
			//	EditorUtility.SetDirty(asset);
			//}

			ReserializeAssets(ref fullList);
		}

		private static void InternalAddToAssetList(ref List<string> list, List<string> fullList, string logTitle, StringBuilder log)
		{
			log.AppendLine($"====  Reserialized {list.Count.ToStringWithEnglishPluralPostfix(logTitle)}  ====");

			if (list.Count == 0)
				return;

			list.Sort();
			fullList.AddRange(list);
			foreach (var item in list)
			{
				log.AppendLine(item);
			}

			Release.List(ref list);
		}

		public static void ReserializeAssets(ref List<string> assetPaths)
		{
			// This is the old way of doing it. Which somewhat worked with some flaws.
			//MarkAssetsAsDirty(true, true, true, true, true, true, true, true, true, true, true, true, true, true, true);

			// This is the brand new Unity's method. This works quite better.
			// But still, we need to process scenes separately because Unity
			// can't handle them well if the scene is not loaded. Somehow 
			// Unity includes all prefab data in scenes.
			{
				var sceneAssetPaths = New.List<string>();
				var otherAssetPaths = New.List<string>();
				AssetDatabaseTools.SplitSceneAndOtherAssetPaths(assetPaths, sceneAssetPaths, otherAssetPaths);
				if (sceneAssetPaths.IsNotNullAndEmpty())
				{
					// Check if the current scene has modifications and warn user
					EditorSceneManagerTools.EnforceUserToSaveAllModifiedScenes("Reserialization needs to load scenes one by one. To prevent loosing any unsaved work, first you need to save current changes before reserialization.");
				}

				AssetDatabase.ForceReserializeAssets(otherAssetPaths, ForceReserializeAssetsOptions.ReserializeAssetsAndMetadata);
				InternalReserializeScenes(sceneAssetPaths);

				Release.List(ref assetPaths, ref sceneAssetPaths, ref otherAssetPaths);
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

			if (EditorSceneManagerTools.IsAnyLoadedSceneDirty())
			{
				throw new InternalException(105851);
			}

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

		[MenuItem(ExtenityMenu.ComponentContext + "Fill Empty References", true)]
		private static bool FillEmptyReferences_Validate(MenuCommand menuCommand)
		{
			var component = menuCommand.context as Component;
			if (!component)
				return false;
			return component.GetNotAssignedSerializedComponentFields().Count > 0;
		}

		[MenuItem(ExtenityMenu.ComponentContext + "Fill Empty References", priority = 524)]
		private static void FillEmptyReferences(MenuCommand menuCommand)
		{
			var component = menuCommand.context as Component;
			FillEmptyReferences(component);
		}

		[MenuItem(ExtenityMenu.ComponentContext + "Fill Empty References In All Components", true)]
		private static bool FillEmptyReferencesInAllComponents_Validate(MenuCommand menuCommand)
		{
			return true;
		}

		[MenuItem(ExtenityMenu.ComponentContext + "Fill Empty References In All Components", priority = 526)]
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
					Log.Info($"Filling field '{field.Name}' of type '{field.FieldType}' with '{selected.FullGameObjectName()}'.");
					Undo.RecordObject(component, "Fill Empty References");
					field.SetValue(component, candidates[0]);
					EditorUtility.SetDirty(component);
				}
			}
		}

		#endregion

		#region Context Menu - Copy Component/GameObject Path

		[MenuItem(ExtenityMenu.ComponentContext + "Copy Component Path", true)]
		private static bool CopyComponentPath_Validate(MenuCommand menuCommand)
		{
			var component = menuCommand.context as Component;
			return component;
		}

		[MenuItem(ExtenityMenu.ComponentContext + "Copy Component Path", priority = 541)]
		private static void CopyComponentPath(MenuCommand menuCommand)
		{
			var component = menuCommand.context as Component;
			Clipboard.SetClipboardText(component.FullName(), true);
		}

		[MenuItem(ExtenityMenu.ComponentContext + "Copy GameObject Path", true)]
		private static bool CopyGameObjectPath_Validate(MenuCommand menuCommand)
		{
			var component = menuCommand.context as Component;
			return component;
		}

		[MenuItem(ExtenityMenu.ComponentContext + "Copy GameObject Path", priority = 542)]
		private static void CopyGameObjectPath(MenuCommand menuCommand)
		{
			var component = menuCommand.context as Component;
			Clipboard.SetClipboardText(component.FullGameObjectName(), true);
		}

		#endregion

		#region Context Menu - Serialize to JSON

		[MenuItem(ExtenityMenu.ComponentContext + "Serialize to JSON", true)]
		private static bool SerializeToJSON_Validate(MenuCommand menuCommand)
		{
			var component = menuCommand.context as Component;
			return component;
		}

		[MenuItem(ExtenityMenu.ComponentContext + "Serialize to JSON", priority = 561)]
		private static void SerializeToJSON(MenuCommand menuCommand)
		{
			var component = menuCommand.context as Component;
			var json = EditorJsonUtility.ToJson(component, true);
			Log.InfoWithContext(component, json);
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(EditorAssetTools));

		#endregion
	}

}
