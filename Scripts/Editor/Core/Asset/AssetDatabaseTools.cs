using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Extenity.ApplicationToolbox;
using Extenity.CompilationToolbox.Editor;
using Extenity.DataToolbox;
using Extenity.FileSystemToolbox;
using Extenity.GameObjectToolbox;
using Extenity.ProfilingToolbox;
using Extenity.ReflectionToolbox;
using Extenity.UnityEditorToolbox;
using Extenity.UnityEditorToolbox.Editor;
using JetBrains.Annotations;
using Object = UnityEngine.Object;
using SelectionMode = UnityEditor.SelectionMode;

namespace Extenity.AssetToolbox.Editor
{

	public static class AssetDatabaseTools
	{
		#region Prefabs

		public static List<GameObject> LoadAllPrefabs()
		{
			var assetPaths = GetAllPrefabAssetPaths();
			var assets = assetPaths
			             .Select(assetPath => AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject)
			             .Where(loadedAsset => loadedAsset != null);
			var list = assets.ToList();
			return list;
		}

		public static List<GameObject> GetPrefabsContainingComponent<TComponent>(bool includeChildren, bool alsoIncludeInactiveChildren) where TComponent : Component
		{
			if (includeChildren)
			{
				return LoadAllPrefabs()
				       .Where(gameObject => gameObject.GetComponentInChildren<TComponent>(alsoIncludeInactiveChildren) != null)
				       .ToList();
			}
			else
			{
				return LoadAllPrefabs()
				       .Where(gameObject => gameObject.GetComponent<TComponent>() != null)
				       .ToList();
			}
		}

		public static List<TComponent> GetComponentsInPrefabs<TComponent>(bool includeChildren, bool alsoIncludeInactiveChildren) where TComponent : Component
		{
			if (includeChildren)
			{
				return LoadAllPrefabs()
				       .SelectMany(gameObject => gameObject.GetComponentsInChildren<TComponent>(alsoIncludeInactiveChildren))
				       .ToList();
			}
			else
			{
				return LoadAllPrefabs()
				       .Select(gameObject => gameObject.GetComponent<TComponent>())
				       .Where(found => found != null)
				       .ToList();
			}
		}

		#endregion

		#region GetAllAssetPaths Exluding Folders

		public static List<string> GetAllAssetPathsExcludingFolders()
		{
			var paths = AssetDatabase.GetAllAssetPaths().ToList();
			for (int i = 0; i < paths.Count; i++)
			{
				// See if this is a file, not a folder.
				if (!File.Exists(paths[i]))
				{
					paths.RemoveAt(i);
					i--;
				}
			}
			return paths;
		}

		#endregion

		#region Get Asset Paths

		public static List<string> GetAllSceneAssetPaths()
		{
			return GetAllAssetPathsExcludingFolders().Where(item => item.ToLowerInvariant().EndsWith(".unity")).ToList();
		}

		public static List<string> GetAllPrefabAssetPaths()
		{
			return GetAllAssetPathsExcludingFolders().Where(item => item.ToLowerInvariant().EndsWith(".prefab")).ToList();
		}

		public static List<string> GetAllModelAssetPaths()
		{
			return GetAllAssetPathsExcludingFolders().Where(item =>
			{
				var lower = item.ToLowerInvariant();
				return lower.EndsWith(".fbx") ||
				       lower.EndsWith(".dae") ||
				       lower.EndsWith(".3ds") ||
				       lower.EndsWith(".dxf") ||
				       lower.EndsWith(".obj") ||
				       lower.EndsWith(".skp");
			}).ToList();
		}

		public static List<string> GetAllAnimationAssetPaths()
		{
			return GetAllAssetPathsExcludingFolders().Where(item =>
			{
				var lower = item.ToLowerInvariant();
				return lower.EndsWith(".anim") ||
				       lower.EndsWith(".controller") ||
				       lower.EndsWith(".overrideController") ||
				       lower.EndsWith(".mask");
			}).ToList();
		}

		public static List<string> GetAllMaterialAssetPaths()
		{
			return GetAllAssetPathsExcludingFolders().Where(item => item.ToLowerInvariant().EndsWith(".mat")).ToList();
		}

		public static List<string> GetAllShaderAssetPaths(bool shaders, bool shaderVariants, bool shaderGraphs, bool computeShaders)
		{
			return GetAllAssetPathsExcludingFolders().Where(item =>
			{
				var lower = item.ToLowerInvariant();
				return (computeShaders && lower.EndsWith(".compute")) ||
				       (shaderGraphs && lower.EndsWith(".shadergraph")) ||
				       (shaderVariants && lower.EndsWith(".shadervariants")) ||
				       (shaders && lower.EndsWith(".shader"));
			}).ToList();
		}

		public static List<string> GetAllTextureAssetPaths()
		{
			return GetAllAssetPathsExcludingFolders().Where(item =>
			{
				var lower = item.ToLowerInvariant();
				return lower.EndsWith(".cubemap") ||
				       lower.EndsWith(".bmp") ||
				       lower.EndsWith(".exr") ||
				       lower.EndsWith(".gif") ||
				       lower.EndsWith(".hdr") ||
				       lower.EndsWith(".iff") ||
				       lower.EndsWith(".jpg") ||
				       lower.EndsWith(".pict") ||
				       lower.EndsWith(".png") ||
				       lower.EndsWith(".psd") ||
				       lower.EndsWith(".tga") ||
				       lower.EndsWith(".tiff");
			}).ToList();
		}

		public static List<string> GetAllProceduralTextureAssetPaths()
		{
			return GetAllAssetPathsExcludingFolders().Where(item => item.ToLowerInvariant().EndsWith(".sbsar")).ToList();
		}

		public static List<string> GetAllRenderTextureAssetPaths()
		{
			return GetAllAssetPathsExcludingFolders().Where(item => item.ToLowerInvariant().EndsWith(".renderTexture")).ToList();
		}

		public static List<string> GetAllLightmapAssetPaths()
		{
			return GetAllAssetPathsExcludingFolders().Where(item => item.ToLowerInvariant().EndsWith(".giparams")).ToList();
		}

		public static List<string> GetAllFlareAssetPaths()
		{
			return GetAllAssetPathsExcludingFolders().Where(item => item.ToLowerInvariant().EndsWith(".flare")).ToList();
		}

		public static List<string> GetAllVideoAssetPaths()
		{
			return GetAllAssetPathsExcludingFolders().Where(item =>
			{
				var lower = item.ToLowerInvariant();
				return lower.EndsWith(".asf") ||
				       lower.EndsWith(".avi") ||
				       lower.EndsWith(".dv") ||
				       lower.EndsWith(".m4v") ||
				       lower.EndsWith(".mov") ||
				       lower.EndsWith(".mp4") ||
				       lower.EndsWith(".mpg") ||
				       lower.EndsWith(".mpeg") ||
				       lower.EndsWith(".ogv") ||
				       lower.EndsWith(".vp8") ||
				       lower.EndsWith(".webm") ||
				       lower.EndsWith(".wmv");
			}).ToList();
		}

		public static List<string> GetAllUIAssetPaths()
		{
			return GetAllAssetPathsExcludingFolders().Where(item =>
			{
				var lower = item.ToLowerInvariant();
				return lower.EndsWith(".guiskin") ||
				       lower.EndsWith(".fontsettings");
			}).ToList();
		}

		public static List<string> GetAllAudioAssetPaths()
		{
			return GetAllAssetPathsExcludingFolders().Where(item =>
			{
				var lower = item.ToLowerInvariant();
				return lower.EndsWith(".mixer") ||
				       lower.EndsWith(".mp3") ||
				       lower.EndsWith(".ogg") ||
				       lower.EndsWith(".wav") ||
				       lower.EndsWith(".aiff ") ||
				       lower.EndsWith(".aif") ||
				       lower.EndsWith(".mod") ||
				       lower.EndsWith(".it") ||
				       lower.EndsWith(".s3m") ||
				       lower.EndsWith(".xm");
			}).ToList();
		}

		public static List<string> GetAllPhysicsAssetPaths()
		{
			return GetAllAssetPathsExcludingFolders().Where(item =>
			{
				var lower = item.ToLowerInvariant();
				return lower.EndsWith(".physicMaterial") ||
				       lower.EndsWith(".physicsMaterial2D");
			}).ToList();
		}

		public static List<string> GetAllScriptAssetPaths()
		{
			return GetAllAssetPathsExcludingFolders().Where(item =>
			{
				var lower = item.ToLowerInvariant();
				return lower.EndsWith(".cs", StringComparison.OrdinalIgnoreCase);
			}).ToList();
		}

		public static List<string> GetAllScriptAssetPathsOfType(ScriptType scriptType)
		{
			return GetAllScriptAssetPaths()
			       .Where(scriptPath => CompilationPipelineTools.GetScriptType(scriptPath) == scriptType)
			       .ToList();
		}

		#endregion

		#region Get Asset Path Of Selection

		public static string GetSelectedDirectoryPathOrAssetsRootPath()
		{
			var path = GetSelectedDirectoryPath();
			if (string.IsNullOrEmpty(path))
			{
				return ApplicationTools.UnityProjectPaths.AssetsRelativePath;
			}
			return path;
		}

		public static string GetSelectedDirectoryPath()
		{
			var filteredSelection = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);

			foreach (Object obj in filteredSelection)
			{
				var path = AssetDatabase.GetAssetPath(obj);
				if (string.IsNullOrEmpty(path))
					continue;

				if (Directory.Exists(path))
				{
					return path;
				}
				if (File.Exists(path))
				{
					return Path.GetDirectoryName(path);
				}
			}
			return "";
		}

		#endregion

		#region Convert Physical Path To AssetDatabase Path

		/// <summary>
		/// Creates a relative path to Unity project directory. Also modifies Packages paths to use package name
		/// instead of physical path, so that AssetDatabase would understand and load assets in these paths.
		///
		/// CAUTION! Note that this operation involves loading the package manifest every time the method is called if
		/// the path starts with "Packages" directory. A caching mechanism is not implemented to keep the system robust.
		/// </summary>
		public static string MakeProjectRelativeAssetDatabasePath(this string filePath)
		{
			// Convert and ensure the path is relative to project directory.
			var relativePath = ApplicationTools.ApplicationPath.MakeRelativePath(filePath, true);

			// See if the path is a Package path. If so, the path may need to be modified to use package name, instead
			// of the physical path. AssetDatabase will only understand package names in such paths.
			if (relativePath.StartsWith(ApplicationTools.UnityProjectPaths.PackagesDirectory, StringComparison.InvariantCultureIgnoreCase))
			{
				// The path expected to be relative to project directory. MakeRelativePath above does that.
				// Also the path assumed to have normalized directory separators. MakeRelativePath above does that too.
				var separatorFirst = relativePath.IndexOf(PathTools.DirectorySeparatorChar);
				var separatorSecond = relativePath.IndexOf(PathTools.DirectorySeparatorChar, separatorFirst + 1);
				var packageDirectory = relativePath.Substring(0, separatorSecond);
				var remainderPath = relativePath.Substring(separatorSecond + 1);
				var manifestPath = Path.Combine(packageDirectory, PackageManagerTools.PackageJsonFileName);
				var packageName = PackageManagerTools.GetPackageNameInManifestJson(manifestPath);
				relativePath = Path.Combine(ApplicationTools.UnityProjectPaths.PackagesDirectory, packageName, remainderPath);
			}

			return relativePath;
		}

		#endregion

		#region Find Assets

		public static List<T> FindAssetsOfType<T>(string filter, [NotNull] params string[] searchInFolders) where T : Object
		{
			var result = new List<T>();
			var guids = AssetDatabase.FindAssets(filter, searchInFolders);
			if (guids != null)
			{
				foreach (var guid in guids)
				{
					var path = AssetDatabase.GUIDToAssetPath(guid);
					var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
					if (obj != null && obj is T)
					{
						result.Add((T)obj);
					}
				}
			}
			return result;
		}

		#endregion

		#region Scene Path

		public static void SplitSceneAndOtherAssetPaths(IEnumerable<string> assetPaths, List<string> sceneAssetPaths, List<string> otherAssetPaths)
		{
			foreach (var assetPath in assetPaths)
			{
				if (assetPath.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
				{
					sceneAssetPaths.Add(assetPath);
				}
				else
				{
					otherAssetPaths.Add(assetPath);
				}
			}
		}

		#endregion

		#region CreateOrReplaceAsset

		public static T CreateOrReplaceAsset<T>(T asset, string path) where T : Object
		{
			var existingAsset = AssetDatabase.LoadAssetAtPath<T>(path);

			if (!existingAsset)
			{
				AssetDatabase.CreateAsset(asset, path);
				existingAsset = asset;
			}
			else
			{
				EditorUtility.CopySerialized(asset, existingAsset);
			}

			return existingAsset;
		}

		public static void CreateOrReplaceAsset(string sourcePath, string destinationPath)
		{
			var destinationAsset = AssetDatabase.LoadAssetAtPath<Object>(destinationPath);

			if (!destinationAsset)
			{
				AssetDatabase.CopyAsset(sourcePath, destinationPath);
			}
			else
			{
				var sourceAsset = AssetDatabase.LoadAssetAtPath<Object>(sourcePath);
				EditorUtility.CopySerialized(sourceAsset, destinationAsset);
				AssetDatabase.SaveAssets();
			}
		}

		public static void CreateOrReplaceScene(string sourcePath, string destinationPath)
		{
			FileTools.Copy(sourcePath, destinationPath, true);
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
		}

		#endregion

		#region Manually Move Asset File Or Directory With Meta

		/// <summary>
		/// Moves a file or a directory with its meta file. You should use <see cref="AssetDatabase.MoveAsset"/> where possible.
		/// But it does not work outside of Assets directory. This is where this method comes to play.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		public static void ManuallyMoveFileOrDirectoryWithMeta(string source, string destination)
		{
			// TODO: It throws access violation exceptions for C++ dlls that is loaded by Unity editor. AssetDatabase.DeleteAsset might work. So this method should do "File.Copy and AssetDatabase.DeleteAsset source" rather than "File.Move".

			if (string.IsNullOrEmpty(source))
				throw new ArgumentNullException(nameof(source));
			if (string.IsNullOrEmpty(destination))
				throw new ArgumentNullException(nameof(destination));

			AssetDatabase.ReleaseCachedFileHandles(); // Make Unity release the files to prevent any IO errors.

			var assetName = Path.GetFileName(source.RemoveEndingDirectorySeparatorChar());
			var sourceMeta = source.RemoveEndingDirectorySeparatorChar() + ".meta";
			var destinationMeta = destination.RemoveEndingDirectorySeparatorChar() + ".meta";

			if (Directory.Exists(source))
			{
				Log.Info($"{assetName} (Directory)\n\tFROM: {source}\n\tTO: {destination}");
				DirectoryTools.CreateFromFilePath(destination.RemoveEndingDirectorySeparatorChar());
				Directory.Move(source, destination);
				File.Move(sourceMeta, destinationMeta);
			}
			else if (File.Exists(source))
			{
				Log.Info($"{assetName} (File)\n\tFROM: {source}\n\tTO: {destination}");
				DirectoryTools.CreateFromFilePath(destination.RemoveEndingDirectorySeparatorChar());
				File.Move(source, destination);
				File.Move(sourceMeta, destinationMeta);
			}
			else
			{
				Log.Info($"{assetName} (Not Found)\n\tFROM: {source}\n\tTO: {destination}");
			}
		}

		/// <summary>
		/// See <see cref="ManuallyMoveFileOrDirectoryWithMeta"/>.
		/// </summary>
		public static void ManuallyMoveFilesAndDirectoriesWithMetaAndEnsureCompleted(List<string> sourcePaths, List<string> destinationPaths, bool refreshAssetDatabase)
		{
			// Initial idea was to move scripts in a safe environment with no ongoing compilations. But that's a fairy tale.
			//EditorApplicationTools.EnsureNotCompiling();

			EnsureNoAssetsExistAtPath(destinationPaths);
			for (var i = 0; i < sourcePaths.Count; i++)
			{
				ManuallyMoveFileOrDirectoryWithMeta(sourcePaths[i], destinationPaths[i]);
			}
			// Before refreshing AssetDatabase.
			EnsureNoAssetsExistAtPath(sourcePaths);
			if (refreshAssetDatabase)
			{
				AssetDatabase.Refresh();
				// After refreshing AssetDatabase.
				EnsureNoAssetsExistAtPath(sourcePaths);
			}

			// Initial idea was to move scripts in a safe environment with no ongoing compilations. But that's a fairy tale.
			//EditorApplicationTools.EnsureNotCompiling();
		}

		#endregion

		#region Manually Delete Asset File and Meta

		/// <summary>
		/// Deletes the file without informing Unity. You may need to do <see cref="AssetDatabase.Refresh()"/> at some point.
		/// </summary>
		public static void ManuallyDeleteMetaFileAndAsset(string path)
		{
			AssetDatabase.ReleaseCachedFileHandles(); // Make Unity release the files to prevent any IO errors.

			if (Directory.Exists(path))
			{
				DirectoryTools.DeleteWithContent(path);
				ManuallyDeleteMetaFileOfAsset(path);
			}
			else if (File.Exists(path))
			{
				FileTools.Delete(path, false);
				ManuallyDeleteMetaFileOfAsset(path);
			}
			else
			{
				Log.Error("Tried to delete file or directory at path '" + path + "' but item cannot be found.");
			}
		}

		/// <summary>
		/// Deletes the file without informing Unity. You may need to do <see cref="AssetDatabase.Refresh()"/> at some point.
		/// </summary>
		public static void ManuallyDeleteMetaFileOfAsset(string path)
		{
			var metaFile = path + ".meta";
			FileTools.Delete(metaFile, true);
		}

		#endregion

		#region Check Directory For Assets

		public static void EnsureNoAssetsExistAtPath(IEnumerable<string> paths)
		{
			foreach (var path in paths)
			{
				EnsureNoAssetExistAtPath(path);
			}
		}

		public static void EnsureNoAssetExistAtPath(string path)
		{
			if (Directory.Exists(path) || File.Exists(path))
			{
				throw new Exception($"The asset is not expected to exist at path '{path}'.");
			}
			var metaPath = path.RemoveEndingDirectorySeparatorChar() + ".meta";
			if (Directory.Exists(metaPath) || File.Exists(metaPath))
			{
				throw new Exception($"The asset is not expected to exist at path '{metaPath}'.");
			}
		}

		#endregion

		#region Script Assets

		public static void OpenScriptInIDE(string scriptPath, int line = -1)
		{
			var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(scriptPath); // Maybe use MonoScript instead of TextAsset

			if (line >= 0)
			{
				Log.InfoWithContext(asset, $"Opening script '{scriptPath}' at line '{line}'.");
				AssetDatabase.OpenAsset(asset, line);
			}
			else
			{
				Log.InfoWithContext(asset, $"Opening script '{scriptPath}'.");
				AssetDatabase.OpenAsset(asset);
			}
		}

		/// <summary>
		/// Note that this won't check if the comment characters are inside a string or not.
		/// </summary>
		public static void DeleteCommentsInScriptAssetContents(string[] lines)
		{
			if (lines.IsNullOrEmpty())
				return;

			var tries = lines.Length * 10;

			const int commentLength = 2; // Length of '//', '/*', '*/'
			var inMultiLineComment = false;
			var multiLineCommentStartIndex = -1;
			var cursor = 0;
			var iLine = 0;
			string line = lines[0];

			while (true)
			{
				//Log.Info($"Line: {iLine} (of {lines.Length}) \t Tries: {tries} \t mult: {inMultiLineComment} \t multStart: {multiLineCommentStartIndex} \t cursor: {cursor}   \n" + string.Join(Environment.NewLine, lines));

				if (--tries < 0)
					throw new InternalException(11917861);

				if (inMultiLineComment)
				{
					var commentEndIndex = line.IndexOf("*/", cursor, StringComparison.Ordinal);
					if (commentEndIndex < 0)
					{
						// No ending found. Delete comment parts. Then move on to the next line.
						if (multiLineCommentStartIndex > 0)
						{
							lines[iLine] = line.Substring(0, multiLineCommentStartIndex);
							multiLineCommentStartIndex = -1;
						}
						else
						{
							lines[iLine] = "";
						}
						cursor = 0;
						if (++iLine >= lines.Length)
							break;
						line = lines[iLine];
					}
					else
					{
						if (multiLineCommentStartIndex > 0)
						{
							line = line.Remove(multiLineCommentStartIndex, commentEndIndex + commentLength - multiLineCommentStartIndex);
							lines[iLine] = line;
							cursor = multiLineCommentStartIndex;
							multiLineCommentStartIndex = -1;
						}
						else
						{
							line = line.Remove(0, commentEndIndex + commentLength);
							lines[iLine] = line;
							cursor = 0;
						}
						inMultiLineComment = false;
					}
				}
				else
				{
					var commentStartIndex1 = line.IndexOf("/*", cursor, StringComparison.Ordinal);
					var commentStartIndex2 = line.IndexOf("//", cursor, StringComparison.Ordinal);
					if (commentStartIndex1 >= 0 && (commentStartIndex2 < 0 || commentStartIndex1 < commentStartIndex2))
					{
						inMultiLineComment = true;
						multiLineCommentStartIndex = commentStartIndex1;
						cursor = multiLineCommentStartIndex + commentLength;
					}
					else if (commentStartIndex2 >= 0 && (commentStartIndex1 < 0 || commentStartIndex2 < commentStartIndex1))
					{
						// Delete the single-line comment.
						lines[iLine] = line.Substring(0, commentStartIndex2);

						// Continue from the next line.
						cursor = 0;
						if (++iLine >= lines.Length)
							break;
						line = lines[iLine];
					}
					else
					{
						// No comments on this line. Continue from the next line.
						cursor = 0;
						if (++iLine >= lines.Length)
							break;
						line = lines[iLine];
					}
				}
			}

			if (inMultiLineComment)
			{
				throw new Exception("Unclosed multi-line comment.");
			}
		}

		#endregion

		#region Reload Scripts

		[MenuItem(ExtenityMenu.AssetsBaseContext + "Reload Scripts", priority = 40)] // Priority is just below the Reimport All option. Unfortunately seems like there is no way to put this option near Refresh or Reimport options, because priority 39 puts the item above them.  
		public static void ReloadScripts()
		{
			EditorUtilityTools.RequestScriptReload();
		}

		#endregion
		
		#region Reimport All - Menu Additions

		[MenuItem(ExtenityMenu.AssetsBaseContext + "Reimport All Scripts", priority = 41)] // Priority is just below the Reimport All option.
		public static void ReimportAllScripts()
		{
			var paths = GetAllScriptAssetPaths().OrderByDescending(item => item).ToList();

			using (new QuickProfilerStopwatch(Log, "Reimporting scripts"))
			{
				Log.Info($"Reimporting {paths.Count} scripts.");
				AssetDatabase.StartAssetEditing();
				for (var i = 0; i < paths.Count; i++)
				{
					AssetDatabase.ImportAsset(paths[i], ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.DontDownloadFromCacheServer);
				}
				AssetDatabase.StopAssetEditing();
			}

			using (new QuickProfilerStopwatch(Log, "Refreshing AssetDatabase"))
			{
				AssetDatabase.Refresh();
			}

			Log.Info("Done.");
		}

		[MenuItem(ExtenityMenu.AssetsBaseContext + "Reimport All Shaders", priority = 42)] // Priority is just below the Reimport All option.
		public static void ReimportAllShaders()
		{
			var paths = GetAllShaderAssetPaths(true, true, true, true).OrderByDescending(item => item).ToList();
			AssetDatabase.StartAssetEditing();
			foreach (var path in paths)
			{
				AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
			}
			AssetDatabase.StopAssetEditing();
			AssetDatabase.Refresh();
			Log.Info("Done.");
		}

		[MenuItem(ExtenityMenu.AssetsBaseContext + "Reimport All Prefabs", priority = 43)] // Priority is just below the Reimport All option.
		public static void ReimportAllPrefabs()
		{
			var paths = GetAllPrefabAssetPaths().OrderByDescending(item => item).ToList();
			AssetDatabase.StartAssetEditing();
			foreach (var path in paths)
			{
				AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
			}
			AssetDatabase.StopAssetEditing();
			AssetDatabase.Refresh();
			Log.Info("Done.");
		}

		#endregion

		#region Asset Usage

		public static void LogListReferencedAssetsOfTypeInGameObject(this GameObject gameObject, bool includeChildren, params Type[] types)
		{
			new List<GameObject> { gameObject } // Yes, not cool. But yeah, really cool.
				.LogListReferencedAssetsOfTypeInGameObjects(includeChildren, types);
		}

		public static void LogListReferencedAssetsOfTypeInGameObjects(this IList<GameObject> gameObjects, bool includeChildren, params Type[] types)
		{
			var resultsAsDictionary = new Dictionary<Object, List<Component>>();

			foreach (var gameObject in gameObjects)
			{
				// Include children if user wants.
				var gameObjectAndChildren = new List<GameObject> { gameObject };
				if (includeChildren)
				{
					gameObject.ListAllChildrenGameObjects(gameObjectAndChildren, false);
				}

				foreach (var processedGameObject in gameObjectAndChildren)
				{
					var components = New.List<Component>();
					processedGameObject.GetComponents(components);
					foreach (var component in components)
					{
						var fieldsAndValues = component.GetUnitySerializedFieldsAndValues(true);
						foreach (var fieldAndValue in fieldsAndValues)
						{
							// See if the field type is one of the types that user wants.
							if (!types.Any(type => fieldAndValue.FieldInfo.FieldType.IsSameOrSubclassOf(type)))
								continue;

							// Save the referenced asset into a list which will be logged in a minute.
							// Also the list then saved into a dictionary for detailed logging.
							var castValue = (Object)fieldAndValue.Value;
							if (!resultsAsDictionary.TryGetValue(castValue, out var list))
							{
								list = New.List<Component>();
								resultsAsDictionary.Add(castValue, list);
							}
							list.Add(component);
						}
						Release.List(ref fieldsAndValues);
					}
					Release.List(ref components);
				}
			}

			// Sort the results
			var sortedResults = new List<(string AssetPath, Object ReferencedObject, List<Component> ReferencedInComponents)>();
			foreach (var entry in resultsAsDictionary)
			{
				var path = AssetDatabase.GetAssetPath(entry.Key);
				sortedResults.Add((path, entry.Key, entry.Value));
				Release.ListUnsafe(entry.Value);
			}
			sortedResults.Sort((item1, item2) => string.Compare(item1.AssetPath, item2.AssetPath, StringComparison.Ordinal));

			// List referenced assets
			Log.Info($"Listing all assets of type(s) '{string.Join(", ", types.Select(type => type.Name))}' in '{gameObjects.Count}' object(s){(includeChildren ? " and their children" : "")}...");
			using (Log.IndentedScope)
			{
				foreach (var result in sortedResults)
				{
					Log.InfoWithContext(result.ReferencedObject, $"{result.AssetPath}");
				}
			}

			// List referenced assets and components where they are referenced
			Log.Info("Detailed listing of components where the assets are referenced...");
			using (Log.IndentedScope)
			{
				foreach (var result in sortedResults)
				{
					Log.InfoWithContext(result.ReferencedObject, $"{result.AssetPath}");
					using (Log.IndentedScope)
					{
						foreach (var referencedByComponent in result.ReferencedInComponents)
						{
							Log.InfoWithContext(referencedByComponent, $"Referenced in: '{referencedByComponent.FullName()}'");
						}
					}
				}
			}
		}

		#endregion

		#region Script Path

		/// <summary>
		/// Returns the path of the script which calls this method.
		///
		/// Uses C# compiler magic to get the script path.
		/// </summary>
		public static string GetCurrentScriptPath(bool relativeToProjectFolder = true, [CallerFilePath] string DontTouch_LeaveThisAsDefault = "")
		{
			var scriptPath = DontTouch_LeaveThisAsDefault;
			if (relativeToProjectFolder)
			{
				scriptPath = scriptPath.MakeProjectRelativeAssetDatabasePath();
			}
			return scriptPath;
		}

		/// <summary>
		/// Instantiates the prefab with the same name of the script which calls this method.
		/// Example:
		///		When called inside Test_FluxCapacitor.cs,
		///		instantiates Test_FluxCapacitor.prefab inside the same folder of the script.
		/// 
		/// Heavily used in testing environment to instantiate the related prefab of the script
		/// that is being tested.
		///
		/// Uses C# compiler magic to get the script path.
		/// </summary>
		public static void InstantiatePrefabWithTheSameNameOfThisScript([CallerFilePath] string DontTouch_LeaveThisAsDefault = "")
		{
			var scriptPath = GetCurrentScriptPath(true, DontTouch_LeaveThisAsDefault);
			var prefabPath = scriptPath.ChangeFileExtension(".prefab");
			var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
			if (!prefab)
			{
				throw new Exception("Failed to find prefab at path: " + prefabPath);
			}
			GameObject.Instantiate(prefab);
		}

		#endregion

		#region Show In Explorer

		public static void ShowInExplorer(string itemPath)
		{
#if UNITY_EDITOR_WIN
			Log.Info("Showing in Explorer: " + itemPath);
			itemPath = itemPath.Replace(@"/", @"\"); // Explorer doesn't like slashes
			if (Directory.Exists(itemPath))
			{
				System.Diagnostics.Process.Start("explorer.exe", "\"" + itemPath + "\"");
			}
			else if (File.Exists(itemPath))
			{
				System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + itemPath + "\"");
			}
#else
			throw new NotImplementedException();
#endif
		}

		#endregion

		#region Copy Selected Asset Paths

		[MenuItem(ExtenityMenu.AssetsBaseContext + "Copy Path(s)", priority = 19)]
		public static void CopySelectedAssetPaths()
		{
			var selectionObjects = Selection.objects;
			if (selectionObjects.Length > 0)
			{
				var stringBuilder = new StringBuilder();

				foreach (Object obj in selectionObjects)
				{
					if (AssetDatabase.Contains(obj))
					{
						stringBuilder.AppendLine(AssetDatabase.GetAssetPath(obj));
					}
					else
					{
						Log.Warning($"{obj} is not a source asset.");
					}
				}
				var paths = stringBuilder.ToString().Trim();
				Clipboard.SetClipboardText(paths, true);
			}
			else
			{
				Log.Warning("Select one or more objects to copy their paths into clipboard.");
			}
		}

		#endregion

		#region Log

		private static Logger Log = new(nameof(AssetDatabaseTools));

		#endregion
		
		public static List<string> GetSelectedAssetPaths(bool includeFilesInSubdirectoriesOfSelectedDirectories)
		{
			var selectionObjects = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
			var list = New.List<string>(selectionObjects.Length);

			foreach (Object obj in selectionObjects)
			{
				if (AssetDatabase.Contains(obj))
				{
					var path = AssetDatabase.GetAssetPath(obj);
					if (AssetDatabase.IsValidFolder(path))
					{
						if (includeFilesInSubdirectoriesOfSelectedDirectories)
						{
							var assetGuids = AssetDatabase.FindAssets("*", new[] { path });
							for (int i = 0; i < assetGuids.Length; i++)
							{
								var assetPath = AssetDatabase.GUIDToAssetPath(assetGuids[i]);
								list.Add(assetPath);
							}
						}
					}
					else
					{
						list.Add(path);
					}
				}
				else
				{
					Log.Warning($"{obj} is not a source asset.");
				}
			}

			return list;
		}

		public static bool IsFolderAsset(Object obj)
		{
			if (obj == null)
				return false;

			var path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
			if (string.IsNullOrEmpty(path))
				return false;

			return Directory.Exists(path);
		}

		public static T CreateAsset<T>(string assetPath = "", bool isPathRelativeToSelectedObjectDirectory = false) where T : ScriptableObject
		{
			// Create scriptable object instance
			T asset = ScriptableObject.CreateInstance<T>();

			// Generate asset file name
			if (string.IsNullOrEmpty(assetPath))
			{
				assetPath = "New" + typeof(T).ToString() + ".asset";
			}

			// Generate full asset path
			string fullPath;
			if (isPathRelativeToSelectedObjectDirectory)
			{
				var directory = GetSelectedDirectoryPathOrAssetsRootPath();
				fullPath = Path.Combine(directory, assetPath);
			}
			else
			{
				fullPath = assetPath;
			}
			fullPath = AssetDatabase.GenerateUniqueAssetPath(fullPath);

			// Create asset
			AssetDatabase.CreateAsset(asset, fullPath);
			AssetDatabase.SaveAssets();

			// Focus on created asset
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = asset;

			return asset;
		}

		public static T SaveAssetToFile<T>(T asset, string assetPath) where T : Object
		{
			if (asset == null)
				throw new NullReferenceException("asset");
			if (string.IsNullOrEmpty(assetPath))
				throw new NullReferenceException("assetPath");

			AssetDatabase.CreateAsset(asset, assetPath);
			AssetDatabase.SaveAssets();
			return AssetDatabase.LoadAssetAtPath<T>(assetPath);
		}
	}

}
