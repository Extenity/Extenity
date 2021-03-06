using System;
using System.Collections.Generic;
using System.Linq;
using Extenity.DataToolbox;
using Extenity.FileSystemToolbox;
using Extenity.GameObjectToolbox;
using Extenity.SceneManagementToolbox;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Extenity.UnityEditorToolbox.Editor
{

	/// <summary>
	/// Don't forget to see <see cref="InternalEditorUtility"/> for more goodness.
	/// </summary>
	public static class EditorUtilityTools
	{
		#region CollectDependenciesReferencedIn...

		public static KeyValue<Scene, TSearched[]>[] CollectDependenciesReferencedInScenes<TSearched>(SceneListFilter sceneListFilter) where TSearched : Object
		{
			var scenes = SceneManagerTools.GetScenes(sceneListFilter);
			var objectsInScenes = new KeyValue<Scene, TSearched[]>[scenes.Count];
			var objects = new HashSet<TSearched>();
			for (var i = 0; i < scenes.Count; i++)
			{
				var scene = scenes[i];
				objects.Clear();
				scene.CollectDependenciesReferencedInScene(objects);

				objectsInScenes[i] = new KeyValue<Scene, TSearched[]>
				{
					Key = scene,
					Value = objects.ToArray()
				};
			}
			return objectsInScenes;
		}

		public static void CollectDependenciesReferencedInScene<TSearched>(this Scene scene, HashSet<TSearched> result) where TSearched : Object
		{
			var objects = EditorUtility.CollectDependencies(scene.ListAllGameObjectsInScene().ToArray());
			foreach (var obj in objects)
			{
				var cast = obj as TSearched;
				if (cast)
					result.Add(cast);
			}
		}

		public static void CollectDependenciesReferencedInComponents<T, TSearched>(this IEnumerable<T> components, HashSet<TSearched> result) where T : Component where TSearched : Object
		{
			var objects = EditorUtility.CollectDependencies(components.ToArray());
			foreach (var obj in objects)
			{
				var cast = obj as TSearched;
				if (cast)
					result.Add(cast);
			}
		}

		public static void CollectDependenciesReferencedInComponent<T, TSearched>(this T component, HashSet<TSearched> result) where T : Component where TSearched : Object
		{
			var objects = EditorUtility.CollectDependencies(new Object[] { component });
			foreach (var obj in objects)
			{
				var cast = obj as TSearched;
				if (cast)
					result.Add(cast);
			}
		}

		public static void CollectDependenciesReferencedInGameObject<TSearched>(this GameObject gameObject, HashSet<TSearched> result) where TSearched : Object
		{
			var objects = EditorUtility.CollectDependencies(new Object[] { gameObject });
			foreach (var obj in objects)
			{
				var cast = obj as TSearched;
				if (cast)
					result.Add(cast);
			}
		}

		public static void CollectDependenciesReferencedInUnityObject<TSearched>(this Object unityObject, HashSet<TSearched> result) where TSearched : Object
		{
			var objects = EditorUtility.CollectDependencies(new Object[] { unityObject });
			foreach (var obj in objects)
			{
				var cast = obj as TSearched;
				if (cast)
					result.Add(cast);
			}
		}

		#endregion

		#region Load / Save Asset File

		public static void SaveUnityAssetFile(string path, Object obj)
		{
			try
			{
				DirectoryTools.CreateFromFilePath(path);
				InternalEditorUtility.SaveToSerializedFileAndForget(new[] {obj}, path, true);
			}
			catch (Exception exception)
			{
				throw new Exception($"Failed to save '{path}'. Careful inspection required to prevent losing any data.", exception);
			}
		}

		public static T LoadUnityAssetFile<T>(string path) where T : Object
		{
			try
			{
				var obj = InternalEditorUtility.LoadSerializedFileAndForget(path)[0];
				var cast = (T)obj;
				return cast;
			}
			catch (Exception exception)
			{
				throw new Exception($"Failed to load '{path}'. Careful inspection required to prevent losing any data.", exception);
			}
		}

		#endregion

		#region Save File Dialog

		/// <summary>
		/// Does the same as <see cref="UnityEditor.EditorUtility.SaveFilePanel"/> but with an additional operation that
		/// allows automatically creating the 'directory'. The directory will only be created if the path is a relative
		/// path. Otherwise, this method would create empty directories all over the system.
		/// </summary>
		/// <param name="alsoCreateDirectoryIfNotAbsolute">Whether 'directory' will be created if does not exist.</param>
		public static string SaveFilePanel(string title, string directory, string defaultName, string extension, bool alsoCreateDirectoryIfNotAbsolute)
		{
			// Create the directory if does not exist. But only do that if it's a relative path. Otherwise we would
			// create empty directories all over the system.
			if (alsoCreateDirectoryIfNotAbsolute &&
			    !string.IsNullOrWhiteSpace(directory) &&
			    directory.IsRelativePath())
			{
				DirectoryTools.Create(directory);
			}

			return EditorUtility.SaveFilePanel(title, directory, defaultName, extension);
		}

		/// <summary>
		/// Does the same as <see cref="UnityEditor.EditorUtility.SaveFilePanel"/> but with an additional operation that
		/// tries to convert the returned path into a relative path to 'expectedBasePath'. The original path will be
		/// returned without any modification if the path is outside of 'expectedBasePath'.
		/// </summary>
		/// <param name="alsoCreateDirectoryIfNotAbsolute">Whether 'directory' will be created if does not exist.</param>
		/// <param name="expectedBasePath">The returned path will be converted to a path that is relative to 'expectedBasePath'.</param>
		public static string SaveFilePanelWithRelativePathIfPossible(string title, string directory, string defaultName, string extension, bool alsoCreateDirectoryIfNotAbsolute, string expectedBasePath)
		{
			if (string.IsNullOrWhiteSpace(expectedBasePath))
				throw new ArgumentNullException(nameof(expectedBasePath));

			var path = SaveFilePanel(title, directory, defaultName, extension, alsoCreateDirectoryIfNotAbsolute);

			// The path that is returned from SaveFilePanel is expected to be a full path. Ensure it really is.
			if (path.IsFullPath())
			{
				var relativePath = expectedBasePath.MakeRelativePath(path);
				// Check if the relative path is outside of expectedBasePath. Note that if the root paths are not
				// the same, relative path will still be the full path to actual file, which is what we want.
				if (!relativePath.StartsWith(".."))
				{
					return relativePath;
				}
			}

			return path;
		}

		#endregion

		#region Script Reload

		public static void RequestScriptReload()
		{
#if UNITY_2019_3_OR_NEWER
			EditorUtility.RequestScriptReload();
#else
			InternalEditorUtility.RequestScriptReload();
#endif
		}

		#endregion
	}

}
